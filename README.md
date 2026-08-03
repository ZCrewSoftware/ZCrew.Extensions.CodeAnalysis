# ZCrew.Extensions.CodeAnalysis.CSharp

Utility library for building Roslyn source generators. Mark a type with `[Embedded]` and you get its source as
a `SourceText` constant, plus the code to parse it back out of a compilation when it's an attribute.

## Installation

On NuGet, targeting .NET Standard 2.0:

```xml
<PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

## Usage

### Embedding any type

Any `internal` type marked with `[Embedded]` gets a `SourceText` constant your generator can emit:

```csharp
[Microsoft.CodeAnalysis.Embedded]
internal enum Lifetime
{
    Transient,
    Scoped,
    Singleton,
}
```

That gives you a `LifetimeSourceText` class holding a ready-to-use `SourceText`, plus a
`context.AddLifetimeDefinition()` extension to call from post-initialization. No raw strings or hint names to
keep in sync.

### Embedding attributes

Attributes get the parsing pipeline on top of the `SourceText` constant. Here's one with two constructor
overloads and a generic sibling:

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

Each declaration produces two files, `ServiceAttributeData.g.cs` and `ServiceAttributeSourceText.g.cs`, with
the generic sibling suffixed `_2`.

#### The data record

Every overload feeds one record, so there's a single type to work with:

```csharp
internal partial record ServiceAttributeData
{
    public ITypeSymbol ServiceType = null!;          // both overloads declare it, so it's always assigned
    public ITypeSymbol? ImplementationType;          // only one overload declares it, so it widens to nullable
    public ImmutableArray<string> Tags =
        ImmutableArray<string>.Empty;                // arrays are value types, so .Empty covers the gap
    public Lifetime Lifetime;                        // named argument, optional unless `required`
}
```

`System.Type` maps to `ITypeSymbol`, since a `typeof(...)` argument has no runtime type in the compilation
you're analyzing. Arrays map to `ImmutableArray<T>`. On the generic sibling, `TService` and `TImplementation`
become `ITypeSymbol Service` and `ITypeSymbol Implementation` on `ServiceAttributeData_2`, dropping the `T`
prefix when an uppercase letter follows.

#### Entry points

| Entry point                                                       | Where     | What it's for                                                 |
|-------------------------------------------------------------------|-----------|---------------------------------------------------------------|
| `context.AddEmbeddedAttributeDefinition()`                        | post-init | Emits `Microsoft.CodeAnalysis.EmbeddedAttribute` itself        |
| `context.AddServiceAttributeDefinition()`                         | post-init | Emits your attribute into the consuming compilation            |
| `context.AddServiceAttribute_2_Definition()`                      | post-init | Same, for the generic overload                                 |
| `ServiceAttributeSourceText.SourceText`                           | anywhere  | The raw `SourceText`, for emitting conditionally               |
| `syntaxProvider.ForServiceAttributeData<T>(predicate, transform)` | pipeline  | The main one: wraps `ForAttributeWithMetadataName` and hands you parsed data |
| `attributeData.TryGetServiceAttributeData(out var data)`          | anywhere  | Parse an `AttributeData` you already hold                      |

Putting it together:

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

`attributes` is an `ImmutableArray<ServiceAttributeData>`, since the attribute can be applied more than once. If
the attribute name matches but no constructor does, the target gets dropped from the pipeline.

For an `AttributeData` you already have:

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

Arity of one or more inserts `_{arity}` plus an `_` separator; arity zero uses neither. The `Attribute` suffix
stays.

|                                 | `ServiceAttribute`                | `ServiceAttribute<TService, TImplementation>` |
|---------------------------------|-----------------------------------|-----------------------------------------------|
| Data record                     | `ServiceAttributeData`            | `ServiceAttributeData_2`                      |
| `AttributeData` extension       | `TryGetServiceAttributeData`      | `TryGetServiceAttributeData_2`                |
| `SyntaxValueProvider` extension | `ForServiceAttributeData`         | `ForServiceAttributeData_2`                   |
| Source text class               | `ServiceAttributeSourceText`      | `ServiceAttributeSourceText_2`                |
| Definition method               | `AddServiceAttributeDefinition()` | `AddServiceAttribute_2_Definition()`          |

`AttributeDataExtensions` and `SyntaxValueProviderExtensions` are `partial`, so every attribute in a namespace
merges into the same two classes. The matchers behind them are `file`-scoped and never collide.

The generated code does the `TypedConstant` and `ITypeSymbol` unwrapping for you. See
[Emitting Attributes](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/emitting-attributes.md)
for the full rules.

### Fast type checks

Mark a `partial bool` method with `[IsType<T>]` (or `[IsType(typeof(T))]`) and the library fills in a fast
pattern-match check over an `ISymbol`:

```csharp
internal static partial class SymbolChecks
{
    [IsType<ServiceAttribute>]
    public static partial bool IsServiceAttribute(ISymbol? symbol);
}
```

The generated body walks the symbol's `Name`/`ContainingNamespace` chain, or uses `SpecialType` for well-known
types like `System.IDisposable`. Cheaper than comparing `ToDisplayString()` output, and it doubles as a null
check. See
[Fast Type Checks](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/is-type-checks.md)
for details.

## Documentation

- [Introduction](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/introduction.md)
  for what the library does
- [Getting Started](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/getting-started.md)
  for install, namespaces, and the utility types
- [Emitting Attributes](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/emitting-attributes.md)
  for the attribute parsing pipeline
- [Emitting Other Abstractions](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/emitting-other-abstractions.md)
  to embed enums, classes, and other types
- [FormattedStringBuilder](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/formatted-string-builder.md)
- [EquatableArray](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/equatable-array.md)
- [Fast Type Checks](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/is-type-checks.md)
- [Shipping in a .NET Core package](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/docs/shipping-in-netcore.md)

## Testing your generator

`ZCrew.Extensions.CodeAnalysis.CSharp.Testing` is a companion package for testing *any* source generator. It
uses JSON descriptors that pair input sources with the output you expect. See its
[README](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/src/ZCrew.Extensions.CodeAnalysis.CSharp.Testing/README.md).

## License

MIT, see [LICENSE.md](https://github.com/ZCrewSoftware/ZCrew.Extensions.CodeAnalysis/blob/main/LICENSE.md).
