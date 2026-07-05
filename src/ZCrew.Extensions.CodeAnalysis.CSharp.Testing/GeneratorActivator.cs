using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Instantiates a source generator type and adapts it to <see cref="ISourceGenerator" />.
/// </summary>
internal static class GeneratorActivator
{
    /// <summary>
    ///     Creates a new instance of <typeparamref name="TGenerator" /> and returns it as an
    ///     <see cref="ISourceGenerator" />, wrapping an <see cref="IIncrementalGenerator" /> via
    ///     <see cref="GeneratorExtensions.AsSourceGenerator" />.
    /// </summary>
    /// <typeparam name="TGenerator">
    ///     The generator type. Must be an <see cref="IIncrementalGenerator" /> or <see cref="ISourceGenerator" />
    ///     with a public parameterless constructor.
    /// </typeparam>
    /// <returns>The generator instance as an <see cref="ISourceGenerator" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <typeparamref name="TGenerator" /> is neither an <see cref="IIncrementalGenerator" /> nor an
    ///     <see cref="ISourceGenerator" />.
    /// </exception>
    public static ISourceGenerator CreateSourceGenerator<TGenerator>()
        where TGenerator : new()
    {
        return new TGenerator() switch
        {
            IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
            ISourceGenerator source => source,
            var other => throw new InvalidOperationException(
                $"'{other!.GetType()}' is neither an IIncrementalGenerator nor an ISourceGenerator."
            ),
        };
    }
}
