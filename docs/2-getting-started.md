# Getting Started

## Installation

Add the NuGet package to your source generator project:

```xml
<ItemGroup>
    <PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
        <PrivateAssets>all</PrivateAssets>
    </PackageReference>
</ItemGroup>
```

Your source generator project must target `netstandard2.0` (a Roslyn requirement).

This package's runtime assembly travels with your generator automatically: it is added to your package's `analyzers/dotnet/cs` when you pack, and forwarded to projects that reference your generator project as an analyzer. No extra configuration is needed.

## Namespaces

The library's types are organized under these namespaces:

```csharp
using ZCrew.Extensions.CodeAnalysis.CSharp;              // Core types: AttributeConstructor, SymbolExtensions, etc.
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;         // FormattedStringBuilder and extensions
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;  // EquatableArray<T> and ImmutableArray extensions
```

## Available Utilities

### FormattedStringBuilder

A `StringBuilder` wrapper with automatic indentation (4 spaces per level). Call `Indent()` and `Unindent()` to manage nesting, and `AppendLine()` to emit indented lines.

```csharp
var builder = new FormattedStringBuilder();
builder.AppendLine("public class Foo");
builder.Append('{');
builder.Indent();
builder.AppendLine();
builder.Append("public int Bar { get; set; }");
builder.Unindent();
builder.AppendLine();
builder.Append('}');
```

See [FormattedStringBuilder](./5-formatted-string-builder.md) for the full API.

### EquatableArray&lt;T&gt;

An `ImmutableArray<T>` wrapper with value equality semantics. Use it in `readonly record struct` models that flow through Roslyn's incremental pipeline to ensure caching works correctly.

```csharp
readonly record struct MyModel(
    string Name,
    EquatableArray<string> Items
);
```

See [EquatableArray](./6-equatable-array.md) for the full API.

### Symbol and constant helpers

`SymbolExtensions` formats and matches Roslyn symbols:

```csharp
// Fully qualified name, optionally without nullable annotations
string name = typeSymbol.ToFullyQualifiedName();

// Match a metadata name without building a display string
bool isList = typeSymbol.HasFullMetadataName("System.Collections.Generic.List`1");
```

`TypedConstantExtensions` reads attribute argument values. The value is written to an `out` parameter rather than returned so that `T` is inferred from the assignment target -- which lets an array constant bind to the `ImmutableArray<T>` overload without you choosing between them:

```csharp
constant.GetValue(out string name);
constant.GetValue(out ImmutableArray<string> tags);
```

### The `[Embedded]` Attribute

Mark any type with `[Microsoft.CodeAnalysis.Embedded]` in your source generator project. The library's built-in generator will produce a `SourceText` class for that type, and -- if the type is an attribute -- a full parsing infrastructure.

See [Emitting Attributes](./3-emitting-attributes.md) and [Emitting Other Abstractions](./4-emitting-other-abstractions.md) for details.

### The `[IsType]` Attribute

Mark a `partial bool` method with `[IsType<T>]` or `[IsType(typeof(T))]` and the library fills in a fast symbol check that avoids `ToDisplayString()` comparisons.

See [Fast Type Checks](./7-is-type-checks.md) for details.

## Next Steps

- [Emitting Attributes](./3-emitting-attributes.md) -- The full attribute parsing pipeline
- [Emitting Other Abstractions](./4-emitting-other-abstractions.md) -- Embedding enums, classes, and other types
- [FormattedStringBuilder](./5-formatted-string-builder.md) -- Indentation-aware code generation
- [EquatableArray](./6-equatable-array.md) -- Value-equality arrays for incremental generators
- [Fast Type Checks](./7-is-type-checks.md) -- Generating fast Roslyn type checks with `[IsType]`
