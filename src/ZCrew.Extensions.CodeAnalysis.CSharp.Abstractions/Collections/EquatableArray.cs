using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

/// <summary>
///     An immutable, equatable array. This is equivalent to <see cref="ImmutableArray{T}" /> but with value equality
///     support.
/// </summary>
/// <typeparam name="T">The type of values in the array.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    /// <summary>
    ///     Empty array.
    /// </summary>
    public static readonly EquatableArray<T> Empty = new(ImmutableArray<T>.Empty);

    /// <summary>
    ///     The underlying <typeparamref name="T" /> array.
    /// </summary>
    private readonly T[]? array;

    /// <summary>
    ///     Creates a new <see cref="EquatableArray{T}" /> instance.
    /// </summary>
    /// <param name="value">The input <typeparamref name="T" /> to wrap.</param>
    public EquatableArray(T value)
        : this([value]) { }

    /// <summary>
    ///     Creates a new <see cref="EquatableArray{T}" /> instance.
    /// </summary>
    /// <param name="value1">The first <typeparamref name="T" /> to wrap.</param>
    /// <param name="value2">The second <typeparamref name="T" /> to wrap.</param>
    public EquatableArray(T value1, T value2)
        : this([value1, value2]) { }

    /// <summary>
    ///     Creates a new <see cref="EquatableArray{T}" /> instance.
    /// </summary>
    /// <param name="array">The input <see cref="ImmutableArray{T}" /> to wrap.</param>
    public EquatableArray(ImmutableArray<T> array)
    {
        this.array = Unsafe.As<ImmutableArray<T>, T[]?>(ref array);
    }

    /// <inheritdoc />
    public int Count => AsImmutableArray().Length;

    /// <summary>
    ///     Gets a reference to an item at a specified position within the array.
    /// </summary>
    /// <param name="index">The index of the item to retrieve a reference to.</param>
    /// <returns>A reference to an item at a specified position within the array.</returns>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsImmutableArray().ItemRef(index);
    }

    /// <summary>
    ///     Gets a value indicating whether the current array is empty.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsImmutableArray().IsEmpty;
    }

    /// <summary>
    ///     Gets a value indicating whether the current array is default or empty.
    /// </summary>
    public bool IsDefaultOrEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this == default || AsImmutableArray().IsEmpty;
    }

    /// <inheritdoc />
    public bool Equals(EquatableArray<T> other)
    {
        if (this.array == null)
        {
            return other.array == null;
        }

        if (other.array == null)
        {
            return false;
        }

        return AsImmutableArray().SequenceEqual(other.AsImmutableArray());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(this, other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (this.array == null)
        {
            return 0;
        }

        var hash = 17;
        foreach (var item in this.array)
        {
            hash = hash * 31 + item.GetHashCode();
        }

        return hash;
    }

    /// <summary>
    ///     Gets an <see cref="ImmutableArray{T}" /> instance from the current <see cref="EquatableArray{T}" />.
    /// </summary>
    /// <returns>The <see cref="ImmutableArray{T}" /> from the current <see cref="EquatableArray{T}" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<T> AsImmutableArray()
    {
        return Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in this.array));
    }

    /// <summary>
    ///     Creates an <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}" />.
    /// </summary>
    /// <param name="array">The input <see cref="ImmutableArray{T}" /> instance.</param>
    /// <returns>An <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}" />.</returns>
    public static EquatableArray<T> FromImmutableArray(ImmutableArray<T> array)
    {
        return new EquatableArray<T>(array);
    }

    /// <summary>
    ///     Returns a <see cref="ReadOnlySpan{T}" /> wrapping the current items.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}" /> wrapping the current items.</returns>
    public ReadOnlySpan<T> AsSpan()
    {
        return AsImmutableArray().AsSpan();
    }

    /// <summary>
    ///     Copies the contents of this <see cref="EquatableArray{T}" /> instance. to a mutable array.
    /// </summary>
    /// <returns>The newly instantiated array.</returns>
    public T[] ToArray()
    {
        return AsImmutableArray().ToArray();
    }

    /// <summary>
    ///     Gets an <see cref="ImmutableArray{T}.Enumerator" /> value to traverse items in the current array.
    /// </summary>
    /// <returns>An <see cref="ImmutableArray{T}.Enumerator" /> value to traverse items in the current array.</returns>
    public ImmutableArray<T>.Enumerator GetEnumerator()
    {
        return AsImmutableArray().GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)AsImmutableArray()).GetEnumerator();
    }

    /// <summary>
    ///     Implicitly converts an <see cref="ImmutableArray{T}" /> to <see cref="EquatableArray{T}" />.
    /// </summary>
    /// <returns>An <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}" />.</returns>
    public static implicit operator EquatableArray<T>(ImmutableArray<T> array)
    {
        return FromImmutableArray(array);
    }

    /// <summary>
    ///     Implicitly converts an <see cref="EquatableArray{T}" /> to <see cref="ImmutableArray{T}" />.
    /// </summary>
    /// <returns>An <see cref="ImmutableArray{T}" /> instance from a given <see cref="EquatableArray{T}" />.</returns>
    public static implicit operator ImmutableArray<T>(EquatableArray<T> array)
    {
        return array.AsImmutableArray();
    }

    /// <summary>
    ///     Checks whether two <see cref="EquatableArray{T}" /> values are the same.
    /// </summary>
    /// <param name="left">The first <see cref="EquatableArray{T}" /> value.</param>
    /// <param name="right">The second <see cref="EquatableArray{T}" /> value.</param>
    /// <returns>Whether <paramref name="left" /> and <paramref name="right" /> are equal.</returns>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks whether two <see cref="EquatableArray{T}" /> values are not the same.
    /// </summary>
    /// <param name="left">The first <see cref="EquatableArray{T}" /> value.</param>
    /// <param name="right">The second <see cref="EquatableArray{T}" /> value.</param>
    /// <returns>Whether <paramref name="left" /> and <paramref name="right" /> are not equal.</returns>
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
    {
        return !left.Equals(right);
    }
}
