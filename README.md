# ZCrew.Extensions.CodeAnalysis.CSharp

Utility library for building Roslyn source generators. Mark types with `[Embedded]` to generate `SourceText` constants and attribute-parsing infrastructure automatically.

## Installation

Available on NuGet for .NET Standard 2.0:

```xml
<PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

## Usage

### Embedding any type

Any `internal` type marked with `[Embedded]` gets a generated `SourceText` constant you can emit from your generator:

```csharp
[Microsoft.CodeAnalysis.Embedded]
internal enum Lifetime
{
    Transient,
    Scoped,
    Singleton,
}
```

This generates a `LifetimeSourceText` class containing a ready-to-use `SourceText` instance, plus a `context.AddLifetimeDefinition()` extension you call from post-initialization — so your generator can emit the type into consuming projects without maintaining raw strings or hint names.

### Embedding attributes

Attributes get the `SourceText` constant **plus** a full parsing pipeline. Take an attribute with two constructor overloads, and a generic sibling:

```csharp
[Microsoft.CodeAnalysis.Embedded]
internal class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type serviceType) { }

    public ServiceAttribute(Type serviceType, Type implementationType, string[] tags) { }

    public Lifetime Lifetime { get; init; }
}

[Microsoft.CodeAnalysis.Embedded]
internal class ServiceAttribute<TService, TImplementation> : Attribute
{
    public ServiceAttribute(string name) { }
}
```

Each declaration produces two files — `ServiceAttributeData.g.cs` and `ServiceAttributeSourceText.g.cs`, with the generic sibling suffixed `_2`.

#### The data record

Every constructor overload feeds a **single** record, so you get one type to work with instead of one per overload:

```csharp
internal partial record ServiceAttributeData
{
    public ITypeSymbol ServiceType = null!;          // declared by both overloads → always assigned
    public ITypeSymbol? ImplementationType;          // declared by one → widened to nullable
    public ImmutableArray<string> Tags =
        ImmutableArray<string>.Empty;                // arrays are value types → .Empty, never nullable
    public Lifetime Lifetime;                        // named argument → optional unless `required`
}
```

`System.Type` becomes `ITypeSymbol` (a `typeof(...)` argument has no runtime type in the compilation being analyzed), and arrays become `ImmutableArray<T>`. The generic sibling's `TService`/`TImplementation` become `ITypeSymbol Service` and `ITypeSymbol Implementation` on `ServiceAttributeData_2` — the `T` prefix is stripped when followed by an uppercase letter.

#### Entry points

| Entry point                                                       | Where     | Purpose                                                                   |
|-------------------------------------------------------------------|-----------|---------------------------------------------------------------------------|
| `context.AddEmbeddedAttributeDefinition()`                        | post-init | Emits `Microsoft.CodeAnalysis.EmbeddedAttribute` itself                   |
| `context.AddServiceAttributeDefinition()`                         | post-init | Emits your attribute into the consuming compilation                       |
| `context.AddServiceAttribute_2_Definition()`                      | post-init | Same, for the generic overload                                            |
| `ServiceAttributeSourceText.SourceText`                           | anywhere  | The raw `SourceText`, for emitting conditionally                          |
| `syntaxProvider.ForServiceAttributeData<T>(predicate, transform)` | pipeline  | Preferred: wraps `ForAttributeWithMetadataName` and hands you parsed data |
| `attributeData.TryGetServiceAttributeData(out var data)`          | anywhere  | Manual parse when you already hold an `AttributeData`                     |

In a generator that looks like:

```csharp
public void Initialize(IncrementalGeneratorInitializationContext context)
{
    context.RegisterPostInitializationOutput(static context =>
    {
        context.AddEmbeddedAttributeDefinition();
        context.AddServiceAttributeDefinition();
        context.AddServiceAttribute_2_Definition();
    });

    var services = context.SyntaxProvider.ForServiceAttributeData(
        static (node, _) => node is ClassDeclarationSyntax,
        static (context, attributes, cancellationToken) => Model.From(context, attributes));

    context.RegisterSourceOutput(services, static (context, model) => /* emit */);
}
```

`attributes` is an `ImmutableArray<ServiceAttributeData>`, because the attribute may be applied more than once. A target whose attribute name matched but whose constructor did not is dropped from the pipeline entirely, rather than flowing through with a default value.

When you already hold an `AttributeData`, parse it directly:

```csharp
foreach (var attributeData in symbol.GetAttributes())
{
    if (attributeData.TryGetServiceAttributeData(out var data))
    {
        // data.ServiceType is non-null here; data.ImplementationType may be null
    }
}
```

#### Naming

An arity of one or more inserts `_{arity}` plus an `_` separator; arity zero uses neither. The `Attribute` suffix is kept.

|                                 | `ServiceAttribute`                | `ServiceAttribute<TService, TImplementation>` |
|---------------------------------|-----------------------------------|-----------------------------------------------|
| Data record                     | `ServiceAttributeData`            | `ServiceAttributeData_2`                      |
| `AttributeData` extension       | `TryGetServiceAttributeData`      | `TryGetServiceAttributeData_2`                |
| `SyntaxValueProvider` extension | `ForServiceAttributeData`         | `ForServiceAttributeData_2`                   |
| Source text class               | `ServiceAttributeSourceText`      | `ServiceAttributeSourceText_2`                |
| Definition method               | `AddServiceAttributeDefinition()` | `AddServiceAttribute_2_Definition()`          |

`AttributeDataExtensions` and `SyntaxValueProviderExtensions` are `partial`, so every attribute in a namespace merges into the same two classes; the matchers behind them are `file`-scoped and never collide.

All generated code handles `TypedConstant` and `ITypeSymbol` unwrapping, so your generator can focus on business logic instead of Roslyn plumbing. See [Emitting Attributes](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/3-emitting-attributes.md) for the full rules.

### Fast type checks

Mark a `partial bool` method with `[IsType<T>]` (or `[IsType(typeof(T))]`) and the library fills in a fast pattern-match type check over an `ISymbol`:

```csharp
internal static partial class SymbolChecks
{
    [IsType<ServiceAttribute>]
    public static partial bool IsServiceAttribute(ISymbol? symbol);
}
```

The generated body walks the symbol's `Name`/`ContainingNamespace` chain (or uses `SpecialType` for well-known types like `System.IDisposable`) instead of slower `ToDisplayString()` comparisons, and doubles as a null check. See [Fast Type Checks](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/7-is-type-checks.md) for details.

## Documentation

- [Introduction](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/1-introduction.md) -- What the library does and why
- [Getting Started](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/2-getting-started.md) -- Installation, namespaces, and the utility types
- [Emitting Attributes](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/3-emitting-attributes.md) -- The full attribute parsing pipeline
- [Emitting Other Abstractions](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/4-emitting-other-abstractions.md) -- Embedding enums, classes, and other types
- [FormattedStringBuilder](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/5-formatted-string-builder.md) -- Indentation-aware code generation
- [EquatableArray](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/6-equatable-array.md) -- Value-equality arrays for incremental generators
- [Fast Type Checks](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/7-is-type-checks.md) -- Generating fast Roslyn type checks with `[IsType]`

## Testing your generator

`ZCrew.Extensions.CodeAnalysis.CSharp.Testing` is a companion package for testing *any* source generator, using JSON descriptors that pair input sources with expected generated output. See its [README](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/src/ZCrew.Extensions.CodeAnalysis.CSharp.Testing/README.md).

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/LICENSE.md) file for details.
