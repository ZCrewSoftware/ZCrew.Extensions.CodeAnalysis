# Getting Started

## Installation

Add the package to your source generator project:

```xml
<ItemGroup>
    <PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
        <PrivateAssets>all</PrivateAssets>
    </PackageReference>
</ItemGroup>
```

Your generator project has to target `netstandard2.0`. Roslyn requires it.

The runtime assembly travels with your generator on its own. It gets added to your package's
`analyzers/dotnet/cs` when you pack, and forwarded to projects that reference your generator project as an
analyzer. Nothing to configure.

If your library is .NET Core and the generator inside it is .NET Standard, see
[Shipping in a .NET Core package](./shipping-in-netcore.md).

## Namespaces

```csharp
using ZCrew.Extensions.CodeAnalysis.CSharp;              // AttributeConstructor, SymbolExtensions, etc.
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;         // FormattedStringBuilder
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;  // EquatableArray<T>
```

## What's in the box

### FormattedStringBuilder

A `StringBuilder` that tracks indentation (4 spaces per level). `Indent()` and `Unindent()` manage nesting,
`AppendLine()` emits an indented line.

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

Full API: [FormattedStringBuilder](./formatted-string-builder.md).

### EquatableArray&lt;T&gt;

An `ImmutableArray<T>` with value equality. Use it in the `readonly record struct` models that flow through
your incremental pipeline so caching works.

```csharp
readonly record struct MyModel(
    string Name,
    EquatableArray<string> Items
);
```

Full API: [EquatableArray](./equatable-array.md).

### Symbol and constant helpers

`SymbolExtensions` formats and matches Roslyn symbols:

```csharp
// fully qualified name, optionally without nullable annotations
string name = typeSymbol.ToFullyQualifiedName();
```

`TypedConstantExtensions` reads attribute argument values. The value goes to an `out` parameter so `T` comes
from the assignment target, which lets an array constant find the `ImmutableArray<T>` overload without you
picking one:

```csharp
constant.GetValue(out string name);
constant.GetValue(out ImmutableArray<string> tags);
```

### The `[Embedded]` attribute

Mark a type with `[Microsoft.CodeAnalysis.Embedded]` in your generator project and you get a `SourceText`
class for it. Attributes get the parsing code too.

See [Emitting Attributes](./emitting-attributes.md) and
[Emitting Other Abstractions](./emitting-other-abstractions.md).

### The `[IsType]` attribute

Mark a `partial bool` method with `[IsType<T>]` or `[IsType(typeof(T))]` and the library fills in a fast
symbol check.

See [Fast Type Checks](./is-type-checks.md).
