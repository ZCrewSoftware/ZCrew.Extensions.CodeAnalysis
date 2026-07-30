# Emitting Attributes

When you mark an attribute type with `[Microsoft.CodeAnalysis.Embedded]`, the library generates everything needed to embed the attribute's source into consuming projects and to parse its usages at compile time.

## The Attribute Pipeline

For an attribute type, the library generates **two files**:

| Generated File                      | Contents                                                                                                                                                            |
|-------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `{Namespace}.{Name}Data.g.cs`       | The `{Name}Data` record, the `AttributeDataExtensions.TryGet{Name}Data` method, the `SyntaxValueProviderExtensions.For{Name}Data` method, and the internal matchers |
| `{Namespace}.{Name}SourceText.g.cs` | The `{Name}SourceText` class holding the attribute's source, plus the `Add{Name}Definition()` post-initialization method                                            |

`{Name}` is the attribute's type name **as written** -- the `Attribute` suffix is kept, so `RegisterAttribute` produces `RegisterAttributeData`, not `RegisterData`. Generic attributes get an arity suffix; see [Naming](#naming).

## Example: Defining an Embedded Attribute

Consider a `RegisterAttribute` with two constructor overloads and a settable property:

```csharp
using System;

namespace MyGenerator;

[Microsoft.CodeAnalysis.Embedded]
internal class RegisterAttribute : Attribute
{
    public RegisterAttribute(Type serviceType) { }

    public RegisterAttribute(Type serviceType, Type implementationType, string[] tags) { }

    public bool Lazy { get; set; }
}
```

## The Data Record

Every constructor overload feeds a **single** record -- you get one type to work with, not one per overload:

```csharp
internal partial record RegisterAttributeData
{
    public global::Microsoft.CodeAnalysis.ITypeSymbol ServiceType = null!;
    public global::Microsoft.CodeAnalysis.ITypeSymbol? ImplementationType;
    public global::System.Collections.Immutable.ImmutableArray<string> Tags =
        global::System.Collections.Immutable.ImmutableArray<string>.Empty;
    public bool Lazy;
}
```

The members are public **fields**, not properties, and they appear in a fixed order: type parameters, then constructor parameters, then named properties.

### Type mapping

| Declared as                | Field type                                    |
|----------------------------|-----------------------------------------------|
| A generic type parameter   | `ITypeSymbol`                                 |
| `System.Type`              | `ITypeSymbol`                                 |
| `T[]`                      | `ImmutableArray<T>`                           |
| Anything else              | The type itself, fully qualified              |

`System.Type` becomes `ITypeSymbol` because a `typeof(...)` argument has no runtime type in the compilation being analyzed. Arrays become `ImmutableArray<T>` because an array `TypedConstant` carries no `Value` -- its elements are read from `Values` individually. Attribute arrays cannot nest, so an element is never itself an array.

### Nullability

Whether a field is nullable depends on whether it is *always* assigned:

- A constructor parameter is always assigned only when **every** surviving constructor declares it. `ServiceType` appears in both overloads, so it is emitted non-nullable with `= null!`. `ImplementationType` appears in only one, so it is widened to `ITypeSymbol?`.
- A named property is always assigned only when it is `required` **and** no surviving constructor carries `[SetsRequiredMembers]`. Named arguments are otherwise optional, so `Lazy` is never treated as guaranteed.
- Arrays are exempt: `ImmutableArray<T>` is a value type, so `Tags` takes `.Empty` rather than becoming nullable, even though only one overload declares it.
- Value types are left alone -- adding `?` would change which `GetValue` overload binds.

### Field names

Names are assigned per attribute by a name table, so the generated fields never collide:

- Constructor parameters are PascalCased (`serviceType` becomes `ServiceType`).
- Type parameters drop a leading `T` when it is followed by an uppercase letter (`TService` becomes `Service`); a bare `T` stays `T`.
- Properties keep their name verbatim.
- A constructor parameter and a property of the same name **and** type deliberately share one field.
- Record members (`Equals`, `GetHashCode`, `ToString`, `EqualityContract`, `PrintMembers`, and friends) are reserved up front. A collision is resolved by appending the type name, then a counter -- so a `string equals` parameter becomes `EqualsString`.
- Keyword identifiers are escaped (`event` becomes `@event`).

## Using the Generated Code in Your Generator

### Emitting the attribute definition

Emit the attribute into consuming projects from post-initialization. The generated `Add{Name}Definition()` extension handles the hint name for you:

```csharp
context.RegisterPostInitializationOutput(static context =>
{
    context.AddEmbeddedAttributeDefinition();
    context.AddRegisterAttributeDefinition();
});
```

`AddEmbeddedAttributeDefinition()` emits `Microsoft.CodeAnalysis.EmbeddedAttribute` itself, which the embedded attribute's own source refers to.

If you need to emit the attribute conditionally instead, the raw `SourceText` is still available as `RegisterAttributeSourceText.SourceText`.

> The captured text is the **entire source file**, not just the marked type declaration. Give each embedded type its own file so no unrelated code travels with it.

### Driving a pipeline

`For{Name}Data<T>` is the intended entry point. It wraps `ForAttributeWithMetadataName` and hands you parsed data instead of raw `AttributeData`:

```csharp
var registrations = context.SyntaxProvider.ForRegisterAttributeData(
    static (node, _) => node is ClassDeclarationSyntax,
    static (context, attributes, cancellationToken) => Model.From(context, attributes));
```

Two behaviours are worth knowing:

- `attributes` is an `ImmutableArray<RegisterAttributeData>`, because the attribute may be applied to the same target more than once.
- If the attribute name matches but no constructor does, the target is **dropped from the pipeline entirely** rather than flowing through with a default value.

### Parsing a single `AttributeData`

When you already hold an `AttributeData`, use the `TryGet{Name}Data` extension:

```csharp
foreach (var attributeData in symbol.GetAttributes())
{
    if (attributeData.TryGetRegisterAttributeData(out var data))
    {
        // data.ServiceType is non-null here; data.ImplementationType may be null
    }
}
```

It walks the attribute's constructors in declaration order and returns the first match. A constructor matches when the attribute's metadata name, type-argument count, constructor-argument count, and every argument's type all line up; named arguments are then applied afterwards by property name. Overloads that share an argument count are therefore told apart by their argument types.

An `object` parameter is the one exception: every attribute constant is assignable to `object`, so it accepts any argument.

## Naming

Every generated name derives from the attribute's name, namespace, and arity. An arity of one or more inserts `_{arity}` plus an `_` separator; arity zero uses neither.

|                                 | `RegisterAttribute`                      | `RegisterAttribute<TService, TImplementation>` |
|---------------------------------|------------------------------------------|------------------------------------------------|
| Data record                     | `RegisterAttributeData`                  | `RegisterAttributeData_2`                      |
| `AttributeData` extension       | `TryGetRegisterAttributeData`            | `TryGetRegisterAttributeData_2`                |
| `SyntaxValueProvider` extension | `ForRegisterAttributeData`               | `ForRegisterAttributeData_2`                   |
| Source text class               | `RegisterAttributeSourceText`            | `RegisterAttributeSourceText_2`                |
| Definition method               | `AddRegisterAttributeDefinition()`       | `AddRegisterAttribute_2_Definition()`          |
| Metadata name                   | `MyGenerator.RegisterAttribute`          | ``MyGenerator.RegisterAttribute`2``            |
| Data hint name                  | `MyGenerator.RegisterAttributeData.g.cs` | `MyGenerator.RegisterAttributeData_2.g.cs`     |

The two declarations are independent: a generic attribute and its non-generic sibling each get their own record, extensions, and source text.

## What Is Skipped

The generator quietly ignores anything it cannot express. **No diagnostics are reported** -- if something you expected is missing from the record, it matched one of these rules:

- A type that does not derive from `System.Attribute` gets the source text file only, with no data record.
- Non-public constructors.
- The implicit parameterless constructor, but only when the type declares other constructors. When it is the only constructor, it is kept.
- Static properties, indexers, and properties whose setter is missing or not public. An `init` accessor counts as a public setter.

## Emitted-Code Invariants

Worth knowing if you read the generated output:

- `AttributeDataExtensions` and `SyntaxValueProviderExtensions` are `partial`, so every attribute in a namespace merges its methods into the same two classes.
- The constructor, parameter, type-parameter, and named-parameter matchers are `file`-scoped. They are an implementation detail, and several attributes can share a namespace without colliding.
- Every type reference is `global::`-qualified, because the output lands in your namespace with no `using` directives.

## Best Practices

- **Use `internal` types.** Embedded attributes are injected into consuming projects, so keeping them `internal` avoids polluting the public API.
- **Give each embedded type its own file.** The source text captures the whole file.
- **Keep attributes simple.** Stick to primitives, enums, `Type`, and single-dimension arrays of those. Complex types cannot be expressed in attribute syntax.
- **Declare `object` parameters last.** An `object` parameter matches any argument, so if it precedes a more specific overload of the same length it will win.

## Next Steps

- [Emitting Other Abstractions](./4-emitting-other-abstractions.md) -- Embedding non-attribute types
- [FormattedStringBuilder](./5-formatted-string-builder.md) -- Generating formatted source code
