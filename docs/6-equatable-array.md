# EquatableArray&lt;T&gt;

`EquatableArray<T>` is an `ImmutableArray<T>` wrapper that provides **value equality** semantics. It exists because Roslyn's incremental generator pipeline uses equality checks to determine whether to re-run generation, and `ImmutableArray<T>` uses reference equality -- which breaks caching.

**Namespace:** `ZCrew.Extensions.CodeAnalysis.CSharp.Collections`

## Why It Exists

Roslyn incremental generators compare pipeline outputs between compilations. If the values are equal, generation is skipped. This is critical for IDE performance.

`ImmutableArray<T>` compares by reference, meaning two arrays with identical contents are considered *not equal*. This causes unnecessary regeneration on every keystroke.

`EquatableArray<T>` solves this by implementing `IEquatable<EquatableArray<T>>` with element-wise comparison using `SequenceEqual`. When used in `readonly record struct` models, the record's auto-generated `Equals` delegates to `EquatableArray<T>.Equals`, and caching works correctly.

## Type Signature

```csharp
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
```

The constraint `T : IEquatable<T>` ensures each element can participate in value equality.

## Core API

### Static Members

```csharp
static readonly EquatableArray<T> Empty  // An empty array instance
```

### Properties

```csharp
int Count             // Number of elements
bool IsEmpty          // True if the array has zero elements
bool IsDefaultOrEmpty // True if the array is default (uninitialized) or empty
T this[int index]     // Element access by index
```

### Conversion Methods

```csharp
ImmutableArray<T> AsImmutableArray()  // Unwrap to the underlying ImmutableArray<T>
ReadOnlySpan<T> AsSpan()              // Get a ReadOnlySpan<T> over the elements
T[] ToArray()                         // Copy elements to a mutable array
```

## Creating Instances

### Constructors

```csharp
new EquatableArray<T>(T value)             // Single-element array
new EquatableArray<T>(T value1, T value2)  // Two-element array
new EquatableArray<T>(ImmutableArray<T>)   // Wrap an existing ImmutableArray<T>
```

### Factory Method

```csharp
EquatableArray<T>.FromImmutableArray(ImmutableArray<T> array)
```

### Implicit Conversions

```csharp
// ImmutableArray<T> -> EquatableArray<T>
EquatableArray<string> items = someImmutableArray;

// EquatableArray<T> -> ImmutableArray<T>
ImmutableArray<string> immutable = items;
```

### Extension Methods

The `ImmutableArrayExtensions` class provides `ToEquatableArray()`:

```csharp
// From ImmutableArray<T>
ImmutableArray<string> immutable = ...;
EquatableArray<string> equatable = immutable.ToEquatableArray();

// From ImmutableArray<T>.Builder
var builder = ImmutableArray.CreateBuilder<string>();
builder.Add("foo");
builder.Add("bar");
EquatableArray<string> equatable = builder.ToEquatableArray();
```

## Usage Pattern: Pipeline Models

Use `EquatableArray<T>` in `readonly record struct` models that flow through the incremental pipeline. This is the pattern used throughout the library itself:

```csharp
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

internal readonly record struct MyTypeInfo(
    string Name,
    string Namespace,
    EquatableArray<MyParameterInfo> Parameters
);

internal readonly record struct MyParameterInfo(
    string Name,
    string FullyQualifiedType
) : IEquatable<MyParameterInfo>;
```

Here is how the library's own `EmbeddedTypeInfo` model is defined:

```csharp
internal readonly record struct EmbeddedTypeInfo(
    string Name,
    string Namespace,
    int Arity,
    string SourceText,
    bool IsAttribute,
    EquatableArray<EmbeddedConstructorInfo> Constructors,
    EquatableArray<EmbeddedTypeParameterInfo> TypeParameters,
    EquatableArray<EmbeddedNamedParameterInfo> Properties
);
```

Because every field uses either a primitive type (which has value equality) or `EquatableArray<T>` (which has value equality), the entire record compares by value -- enabling Roslyn's caching.

## Building Arrays in Factories

A typical pattern for constructing `EquatableArray<T>` from Roslyn data:

```csharp
var builder = ImmutableArray.CreateBuilder<MyParameterInfo>();

foreach (var parameter in symbol.Parameters)
{
    builder.Add(new MyParameterInfo(parameter.Name, parameter.Type.ToGenericTypeName()));
}

return builder.ToEquatableArray();
```

For empty cases, use the static `Empty` field:

```csharp
if (parameters.IsDefaultOrEmpty)
{
    return EquatableArray<MyParameterInfo>.Empty;
}
```

## Best Practices

- **Use `EquatableArray<T>.Empty` instead of `default`.** A `default` `EquatableArray<T>` has a null backing array. While `IsDefaultOrEmpty` handles this, using `Empty` is explicit and avoids null-related edge cases.
- **Ensure `T : IEquatable<T>`.** The generic constraint is enforced by the compiler. For `readonly record struct` types, `IEquatable<T>` is automatically implemented.
- **Use `IsDefaultOrEmpty` for guard checks.** This handles both the uninitialized (`default`) case and the empty case in a single check.
- **Use `ToEquatableArray()` on builders.** When building arrays incrementally with `ImmutableArray<T>.Builder`, call `builder.ToEquatableArray()` as the final step.

## Next Steps

- [FormattedStringBuilder](./5-formatted-string-builder.md) -- Indentation-aware code generation
- [Emitting Attributes](./3-emitting-attributes.md) -- The full attribute parsing pipeline
