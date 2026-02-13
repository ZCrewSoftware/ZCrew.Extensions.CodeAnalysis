using System.Collections.Immutable;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

/// <summary>
///     Extensions for <see cref="ImmutableArray{T}" />.
/// </summary>
public static class ImmutableArrayExtensions
{
    /// <summary>
    ///     Creates an <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray" />.
    /// </summary>
    /// <typeparam name="T">The type of items in the input array.</typeparam>
    /// <param name="array">The input <see cref="ImmutableArray{T}" /> instance.</param>
    /// <returns>An <see cref="EquatableArray{T}" /> instance.</returns>
    public static EquatableArray<T> ToEquatableArray<T>(this ImmutableArray<T> array)
        where T : IEquatable<T>
    {
        return new EquatableArray<T>(array);
    }

    /// <summary>
    ///     Creates an <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}.Builder"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the input array.</typeparam>
    /// <param name="builder">The input <see cref="ImmutableArray{T}.Builder" /> instance.</param>
    /// <returns>An <see cref="EquatableArray{T}" /> instance.</returns>
    public static EquatableArray<T> ToEquatableArray<T>(this ImmutableArray<T>.Builder builder)
        where T : IEquatable<T>
    {
        return builder.ToImmutableArray().ToEquatableArray();
    }
}
