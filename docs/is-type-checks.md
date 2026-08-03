# Fast Type Checks (`IsType`)

Generators ask "is this symbol type *X*?" constantly. The cheap way to answer is a pattern match over the
symbol's `Name`/`ContainingNamespace` chain, or its `SpecialType` for well-known types, instead of comparing
`ToDisplayString()` output. `[IsType]` writes that pattern for you, and since a `null` symbol fails the `is`
pattern anyway, you get a null check out of it.

## Usage

Declare a `partial` method returning `bool` that takes one `Microsoft.CodeAnalysis.ISymbol` (or something more
derived), then mark it with either form of the attribute. The generic form needs C# 11, the `typeof` form works
anywhere.

```csharp
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp;

internal static partial class SymbolChecks
{
    [IsType<ServiceKeyAttribute>]
    public static partial bool IsServiceKeyAttribute(ISymbol? symbol);

    [IsType(typeof(System.IDisposable))]
    public static partial bool IsDisposable(ISymbol? symbol);
}
```

The generator fills in the implementing part, mirroring your accessibility, `static`/`this` modifiers,
parameter name, and parameter type.

> The `IsTypeAttribute` definitions are emitted into your project as `internal` types, so referencing the
> package as an analyzer is enough. There's no runtime dependency.

## What comes out

For a named type, it walks the containing types and namespaces up to the global namespace, narrowing to
`INamedTypeSymbol` when it has to:

```csharp
public static partial bool IsServiceKeyAttribute(
    [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] global::Microsoft.CodeAnalysis.ISymbol? symbol) =>
    symbol is global::Microsoft.CodeAnalysis.INamedTypeSymbol
    {
        Name: "ServiceKeyAttribute",
        Arity: 0,
        ContainingType: null,
        ContainingNamespace:
        {
            Name: "Dependable",
            ContainingNamespace: { Name: "ZCrew", ContainingNamespace.IsGlobalNamespace: true }
        }
    };
```

Anything with a `Microsoft.CodeAnalysis.SpecialType` (`System.IDisposable`, `System.String`, `System.Int32`, etc.)
gets the much cheaper `SpecialType` check:

```csharp
public static partial bool IsDisposable(
    [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] global::Microsoft.CodeAnalysis.ISymbol? symbol) =>
    symbol is global::Microsoft.CodeAnalysis.ITypeSymbol
    {
        SpecialType: global::Microsoft.CodeAnalysis.SpecialType.System_IDisposable
    };
```

Nested types are matched through the `ContainingType` chain, and generic types are told apart by `Arity`.

## Null flow

When the parameter is nullable, the generated implementation annotates it with `[NotNullWhen(true)]`, so a
`true` result narrows the symbol for the caller:

```csharp
if (SymbolChecks.IsServiceKeyAttribute(symbol))
{
    // symbol is non-null here
}
```

## Tips

- Narrow the parameter if you can. Declaring it as `INamedTypeSymbol?` or `ITypeSymbol?` drops the redundant
  `is INamedTypeSymbol` / `is ITypeSymbol` from the generated body.
- Extension methods read nicely: `[IsType<T>] public static partial bool IsX(this ISymbol? symbol)` lets you
  write `symbol.IsX()`.
- The method has to be `partial`, return `bool`, and take exactly one parameter. Other shapes are ignored.

## See also

- [FormattedStringBuilder](./formatted-string-builder.md)
- [Emitting Attributes](./emitting-attributes.md)
