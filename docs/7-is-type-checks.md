# Fast Type Checks (`IsType`)

Source generators frequently need to answer "is this symbol type *X*?". The fast, allocation-free way
is a pattern match over the symbol's `Name`/`ContainingNamespace` chain (or `SpecialType` for
well-known types) rather than comparing `ToDisplayString()` output. The `[IsType]` attribute generates
that pattern for you, and -- because a `null` symbol simply fails the `is` pattern -- the generated
method doubles as a null check.

## Usage

Declare a `partial` method that returns `bool` and accepts a single
`Microsoft.CodeAnalysis.ISymbol` (or a more derived symbol type), then mark it with either form of the
attribute. The generic form requires C# 11; the `typeof` form works on any language version.

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

The generator provides the implementing part. The declared accessibility, `static`/`this` modifiers,
parameter name, and parameter type are all mirrored onto the generated method.

> The `IsTypeAttribute` definitions are emitted into your project automatically (as `internal` types),
> so you only need to reference the package as an analyzer -- no runtime dependency is required.

## Generated output

For a named type, the generator walks the containing types and namespaces, anchored at the global
namespace, and narrows to `INamedTypeSymbol` when needed:

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

For a **special type** (anything with a `Microsoft.CodeAnalysis.SpecialType`, such as
`System.IDisposable`, `System.String`, or `System.Int32`) the generator emits the much cheaper
`SpecialType` check instead:

```csharp
public static partial bool IsDisposable(
    [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] global::Microsoft.CodeAnalysis.ISymbol? symbol) =>
    symbol is global::Microsoft.CodeAnalysis.ITypeSymbol
    {
        SpecialType: global::Microsoft.CodeAnalysis.SpecialType.System_IDisposable
    };
```

**Nested types** are matched through the `ContainingType` chain, and **generic types** are
disambiguated via `Arity`.

## Null-flow analysis

When the parameter is nullable (e.g. `ISymbol?`), the generated implementation annotates it with
`[NotNullWhen(true)]`, so a `true` result narrows the symbol to non-null for the caller:

```csharp
if (SymbolChecks.IsServiceKeyAttribute(symbol))
{
    // 'symbol' is known to be non-null here.
}
```

## Tips

- **Pre-narrow the parameter** when you already have one. Declaring the parameter as
  `INamedTypeSymbol?` (or `ITypeSymbol?`) skips the redundant `is INamedTypeSymbol`/`is ITypeSymbol`
  narrowing in the generated body.
- **Use extension methods** for fluent call sites: `[IsType<T>] public static partial bool IsX(this ISymbol? symbol)`
  lets you write `symbol.IsX()`.
- The method must be `partial`, return `bool`, and take exactly one parameter; other shapes are
  ignored.

## Next Steps

- [FormattedStringBuilder](./5-formatted-string-builder.md) -- Generating formatted source code
- [Emitting Attributes](./3-emitting-attributes.md) -- The full attribute parsing pipeline
