# Emitting Attributes

Mark an attribute type with `[Microsoft.CodeAnalysis.Embedded]` and you get two things: the attribute's source,
ready to emit into consuming projects, and the code to parse its usages back out of a compilation.

Two files come out:

| Generated file                      | What's in it                                                                                                                                  |
|-------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------|
| `{Namespace}.{Name}Data.g.cs`       | The `{Name}Data` record, `AttributeDataExtensions.TryGet{Name}Data`, `SyntaxValueProviderExtensions.For{Name}Data`, and the internal matchers |
| `{Namespace}.{Name}SourceText.g.cs` | The `{Name}SourceText` class with the attribute's source, plus `Add{Name}Definition()`                                                        |

`{Name}` is the type name as written, so the `Attribute` suffix stays: `RegisterAttribute` gives you
`RegisterAttributeData`. Generic attributes get an arity suffix, see [Naming](#naming).

## An example attribute

Two constructor overloads and a settable property:

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

## The data record

Every overload feeds one record, so there's a single type to work with:

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

Members are public fields, in a fixed order: type parameters, then constructor parameters, then named
properties.

### Type mapping

| Declared as              | Field type                       |
|--------------------------|----------------------------------|
| A generic type parameter | `ITypeSymbol`                    |
| `System.Type`            | `ITypeSymbol`                    |
| `T[]`                    | `ImmutableArray<T>`              |
| Anything else            | The type itself, fully qualified |

`System.Type` maps to `ITypeSymbol` because a `typeof(...)` argument has no runtime type in the compilation
you're analyzing. Arrays map to `ImmutableArray<T>` because an array `TypedConstant` has no `Value`, you read
its elements out of `Values`.

Keep your arrays flat. An `object[]` parameter will take a nested array, so
`new object[] { new int[] { 1, 2 }, "x" }` compiles, but reading that element throws
`InvalidOperationException` from Roslyn ("TypedConstant is an array. Use Values property."). Declaring the
parameter as `object[][]` gets you nowhere either, the compiler rejects every use of the attribute with CS0181.

### Nullability

A field is non-nullable when it's *always* assigned:

- A constructor parameter counts as always assigned when every surviving constructor declares it.
  `ServiceType` is in both overloads, so it gets `= null!`. `ImplementationType` is only in one, so it widens
  to `ITypeSymbol?`.
- A named property counts when it's `required` and no surviving constructor carries
  `[SetsRequiredMembers]`. Named arguments are optional otherwise, so `Lazy` never qualifies.
- Arrays are exempt. `ImmutableArray<T>` is a value type, so `Tags` gets `.Empty` instead of a `?`.
- Value types are left alone. Adding `?` would change which `GetValue` overload binds.

### Field names

A name table hands out the field names per attribute, so they never collide:

- Constructor parameters are PascalCased (`serviceType` to `ServiceType`).
- Type parameters drop a leading `T` when an uppercase letter follows (`TService` to `Service`). A bare `T`
  stays `T`.
- Properties keep their name.
- A constructor parameter and a property with the same name and type deliberately share one field.
- Record members (`Equals`, `GetHashCode`, `ToString`, `EqualityContract`, `PrintMembers`, and friends) are
  reserved up front. Collisions get the type name appended, then a counter, so a `string equals` parameter
  becomes `EqualsString`.
- Keyword identifiers get escaped (`event` to `@event`).

## Using it in your generator

### Emitting the attribute definition

Emit the attribute from post-initialization. `Add{Name}Definition()` picks the hint name:

```csharp
context.RegisterPostInitializationOutput(static context =>
{
    context.AddEmbeddedAttributeDefinition();
    context.AddRegisterAttributeDefinition();
});
```

`AddEmbeddedAttributeDefinition()` emits `Microsoft.CodeAnalysis.EmbeddedAttribute`, which the embedded
attribute's own source refers to.

To emit conditionally, the raw text is on `RegisterAttributeSourceText.SourceText`.

> The captured text is the whole source file, not just the marked type. Give each embedded type its own file
> so nothing unrelated tags along.

### Driving a pipeline

`For{Name}Data<T>` is the main entry point. It wraps `ForAttributeWithMetadataName` and hands you parsed data:

```csharp
var registrations = context.SyntaxProvider.ForRegisterAttributeData(
    static (node, _) => node is ClassDeclarationSyntax,
    static (context, attributes, cancellationToken) => Model.From(context, attributes));
```

Two things worth knowing:

- `attributes` is an `ImmutableArray<RegisterAttributeData>`, since the attribute can be applied to the same
  target more than once.
- If the attribute name matches but no constructor does, the target gets dropped from the pipeline.

### Parsing a single `AttributeData`

```csharp
foreach (var attributeData in symbol.GetAttributes())
{
    if (attributeData.TryGetRegisterAttributeData(out var data))
    {
        // data.ServiceType is non-null here; data.ImplementationType may be null
    }
}
```

It walks the constructors in declaration order and takes the first match. A constructor matches when the
metadata name, type-argument count, constructor-argument count, and every argument type line up. Named
arguments are applied afterwards by property name. Overloads with the same argument count are told apart by
their argument types.

`object` parameters are the exception. Every attribute constant is assignable to `object`, so they accept
anything.

## Naming

Names derive from the attribute's name, namespace, and arity. Arity of one or more inserts `_{arity}` plus an
`_` separator; arity zero uses neither.

|                                 | `RegisterAttribute`                      | `RegisterAttribute<TService, TImplementation>` |
|---------------------------------|------------------------------------------|------------------------------------------------|
| Data record                     | `RegisterAttributeData`                  | `RegisterAttributeData_2`                      |
| `AttributeData` extension       | `TryGetRegisterAttributeData`            | `TryGetRegisterAttributeData_2`                |
| `SyntaxValueProvider` extension | `ForRegisterAttributeData`               | `ForRegisterAttributeData_2`                   |
| Source text class               | `RegisterAttributeSourceText`            | `RegisterAttributeSourceText_2`                |
| Definition method               | `AddRegisterAttributeDefinition()`       | `AddRegisterAttribute_2_Definition()`          |
| Metadata name                   | `MyGenerator.RegisterAttribute`          | ``MyGenerator.RegisterAttribute`2``            |
| Data hint name                  | `MyGenerator.RegisterAttributeData.g.cs` | `MyGenerator.RegisterAttributeData_2.g.cs`     |

The two declarations are independent. A generic attribute and its non-generic sibling each get their own
record, extensions, and source text.

## What gets skipped

Anything the generator can't express is quietly ignored, with no diagnostics. If something's missing from the
record, it hit one of these:

- A type that doesn't derive from `System.Attribute` gets the source text file only, no data record.
- Non-public constructors.
- The implicit parameterless constructor, but only when the type declares other constructors. When it's the
  only one, it's kept.
- Static properties, indexers, and properties whose setter is missing or not public. An `init` accessor counts
  as a public setter.

## Reading the generated code

- `AttributeDataExtensions` and `SyntaxValueProviderExtensions` are `partial`, so every attribute in a
  namespace merges its methods into the same two classes.
- The matchers are `file`-scoped, so several attributes can share a namespace without colliding.
- Every type reference is `global::`-qualified, since the output lands in your namespace with no `using`
  directives.

## Tips

- Keep embedded attributes `internal`. They're injected into consuming projects.
- Give each one its own file. The source text captures the whole file.
- Stick to primitives, enums, `Type`, and single-dimension arrays of those. Attribute syntax can't carry
  anything more complex.
- Declare `object` parameters last. An `object` parameter matches anything, so it wins over a more specific
  overload of the same length that comes after it.

## See also

- [Emitting Other Abstractions](./emitting-other-abstractions.md)
- [FormattedStringBuilder](./formatted-string-builder.md)
