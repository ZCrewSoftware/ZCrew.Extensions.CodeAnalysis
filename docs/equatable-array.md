# EquatableArray&lt;T&gt;

`EquatableArray<T>` wraps `ImmutableArray<T>` and gives it value equality.

**Namespace:** `ZCrew.Extensions.CodeAnalysis.CSharp.Collections`

Roslyn's incremental pipeline compares each step's output against the previous run and skips regeneration when
they're equal. `ImmutableArray<T>` compares by reference, so two arrays with identical contents look different
and your generator re-runs on every keystroke. `EquatableArray<T>` compares element by element with
`SequenceEqual`, so a `readonly record struct` holding one gets a working `Equals` and the cache holds.

```csharp
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
```

The `T : IEquatable<T>` constraint is what keeps the element comparison honest.

## API

```csharp
static readonly EquatableArray<T> Empty

int Count
bool IsEmpty
bool IsDefaultOrEmpty  // true when default (uninitialized) or empty
T this[int index]

ImmutableArray<T> AsImmutableArray()
ReadOnlySpan<T> AsSpan()
T[] ToArray()
```

## Creating one

```csharp
new EquatableArray<T>(T value)             // one element
new EquatableArray<T>(T value1, T value2)  // two elements
new EquatableArray<T>(ImmutableArray<T>)   // wrap an existing array

EquatableArray<T>.FromImmutableArray(ImmutableArray<T> array)
```

It converts both ways implicitly:

```csharp
EquatableArray<string> items = someImmutableArray;
ImmutableArray<string> immutable = items;
```

Or use `ToEquatableArray()` from `ImmutableArrayExtensions`, which works on arrays and builders:

```csharp
EquatableArray<string> fromArray = immutable.ToEquatableArray();

var builder = ImmutableArray.CreateBuilder<string>();
builder.Add("foo");
builder.Add("bar");
EquatableArray<string> fromBuilder = builder.ToEquatableArray();
```

## Pipeline models

Use it in the `readonly record struct` models that flow through your pipeline. This is how the library's own
models are built:

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

Every field is either a primitive or an `EquatableArray<T>`, so the whole record compares by value and Roslyn
can cache it.

## Building them in factories

```csharp
var builder = ImmutableArray.CreateBuilder<MyParameterInfo>();

foreach (var parameter in symbol.Parameters)
{
    builder.Add(new MyParameterInfo(parameter.Name, parameter.Type.ToFullyQualifiedName()));
}

return builder.ToEquatableArray();
```

For the empty case, reach for the static field:

```csharp
if (parameters.IsDefaultOrEmpty)
{
    return EquatableArray<MyParameterInfo>.Empty;
}
```

## Tips

- Prefer `Empty` over `default`. A `default` `EquatableArray<T>` has a null backing array. `IsDefaultOrEmpty`
  copes with it, but `Empty` saves you the edge cases.
- `IsDefaultOrEmpty` covers both the uninitialized and empty cases in one check.
- `readonly record struct` types implement `IEquatable<T>` for free, so they satisfy the constraint already.

## See also

- [FormattedStringBuilder](./formatted-string-builder.md)
- [Emitting Attributes](./emitting-attributes.md)
