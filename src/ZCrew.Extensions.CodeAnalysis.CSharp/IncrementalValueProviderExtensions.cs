using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

public static class IncrementalValueProviderExtensions
{
    /// <summary>
    ///     Collect and then concatenate two <see cref="IncrementalValuesProvider{TValues}"/> into a single
    ///     <see cref="ImmutableArray{T}"/>.
    /// </summary>
    /// <param name="first">The first <see cref="IncrementalValuesProvider{TValues}"/>.</param>
    /// <param name="second">The second <see cref="IncrementalValuesProvider{TValues}"/>.</param>
    /// <typeparam name="T">The common incremental value type.</typeparam>
    /// <returns>The concatenated <see cref="IncrementalValueProvider{TValue}"/> containing the values from the
    /// <paramref name="first"/> provider and then the <paramref name="second"/> provider.</returns>
    public static IncrementalValueProvider<ImmutableArray<T>> Concat<T>(
        this IncrementalValuesProvider<T> first,
        IncrementalValuesProvider<T> second
    )
    {
        return first
            .Collect()
            .Combine(second.Collect())
            .Select(
                (values, token) =>
                {
                    if (values.Left.IsDefaultOrEmpty && values.Right.IsDefaultOrEmpty)
                    {
                        return ImmutableArray<T>.Empty;
                    }

                    var arrayBuilder = ImmutableArray.CreateBuilder<T>();
                    if (!values.Left.IsDefaultOrEmpty)
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (var incrementalValues in values.Left)
                        {
                            arrayBuilder.AddRange(incrementalValues);
                        }
                    }

                    if (!values.Right.IsDefaultOrEmpty)
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (var incrementalValues in values.Right)
                        {
                            arrayBuilder.AddRange(incrementalValues);
                        }
                    }

                    return arrayBuilder.ToImmutable();
                }
            );
    }

    /// <summary>
    ///     Takes a collected <see cref="IncrementalValueProvider{TValue}"/> and converts the
    ///     <see cref="ImmutableArray{T}"/> value to an <see cref="EquatableArray{T}"/>.
    /// </summary>
    /// <param name="values">The collected values to convert.</param>
    /// <typeparam name="T">The element type of the provider. Must implement <see cref="IEquatable{T}"/>.</typeparam>
    /// <returns>The collected values as an <see cref="EquatableArray{T}"/>.</returns>
    public static IncrementalValueProvider<EquatableArray<T>> ToEnumerableArray<T>(
        this IncrementalValueProvider<ImmutableArray<T>> values
    )
        where T : IEquatable<T>
    {
        return values.Select((value, _) => value.ToEquatableArray());
    }
}
