# Getting Started

## Installation

Add the NuGet package to your source generator project. Since this is an analyzer dependency, configure it with `OutputItemType` and `ReferenceOutputAssembly`:

```xml
<ItemGroup>
    <PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
        <OutputItemType>Analyzer</OutputItemType>
        <ReferenceOutputAssembly>true</ReferenceOutputAssembly>
    </PackageReference>
</ItemGroup>
```

Your source generator project must target `netstandard2.0` (a Roslyn requirement).

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

### The `[Embedded]` Attribute

Mark any type with `[Microsoft.CodeAnalysis.Embedded]` in your source generator project. The library's built-in generator will produce a `SourceText` class for that type, and -- if the type is an attribute -- a full parsing infrastructure.

See [Emitting Attributes](./3-emitting-attributes.md) and [Emitting Other Abstractions](./4-emitting-other-abstractions.md) for details.

## Next Steps

- [Emitting Attributes](./3-emitting-attributes.md) -- The full attribute parsing pipeline
- [Emitting Other Abstractions](./4-emitting-other-abstractions.md) -- Embedding enums, classes, and other types
- [FormattedStringBuilder](./5-formatted-string-builder.md) -- Indentation-aware code generation
- [EquatableArray](./6-equatable-array.md) -- Value-equality arrays for incremental generators
