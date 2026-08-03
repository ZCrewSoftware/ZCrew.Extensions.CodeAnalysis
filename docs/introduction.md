# Introduction

ZCrew.Extensions.CodeAnalysis.CSharp is a helper library for writing C# source generators. It handles the
boring parts: shipping types into the projects that consume your generator, reading attributes back out of a
compilation, and building source text without fighting indentation.

## Embedding types

Mark any type with `[Microsoft.CodeAnalysis.Embedded]` (enum, class, struct, record, interface) and you get a
static `SourceText` field holding that type's source, plus an `Add{Name}Definition()` method to emit it. This
is how you hand shared types (enums, marker interfaces, helpers) to the projects that use your generator.

## Attribute parsing

If the embedded type is an attribute, you also get the code to parse it. There's a `{Name}Data` record with
the parsed values on it, and two ways to get one:

```csharp
// drive an incremental pipeline
context.SyntaxProvider.ForRegisterAttributeData(predicate, transform);

// or parse an AttributeData you already have
attributeData.TryGetRegisterAttributeData(out var data);
```

Constructor overloads, type parameters, and named arguments are all handled, so you don't have to poke at
`AttributeData` yourself.

## Fast type checks

Mark a `partial bool` method with `[IsType<T>]` (or `[IsType(typeof(T))]`) and you get a pattern match over
the symbol's `Name`/`ContainingNamespace` chain, or its `SpecialType` for well-known types. Faster than
comparing `ToDisplayString()` output, and it null-checks for free.

## Utilities

- `FormattedStringBuilder`, a `StringBuilder` that tracks indentation for you
- `EquatableArray<T>`, an `ImmutableArray<T>` with value equality so generator caching works

## Where to go next

- [Getting Started](./getting-started.md) for install and setup
- [Emitting Attributes](./emitting-attributes.md) for the attribute parsing pipeline
- [Emitting Other Abstractions](./emitting-other-abstractions.md) to embed enums, classes, and other types
- [FormattedStringBuilder](./formatted-string-builder.md)
- [EquatableArray](./equatable-array.md)
- [Fast Type Checks](./is-type-checks.md)
- [Shipping in a .NET Core package](./shipping-in-netcore.md)
