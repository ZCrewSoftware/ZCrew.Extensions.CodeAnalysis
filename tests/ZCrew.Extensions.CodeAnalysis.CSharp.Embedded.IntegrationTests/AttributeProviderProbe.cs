using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.IntegrationTests;

/// <summary>
///     Drives one of the generated <c>SyntaxValueProviderExtensions</c> providers and captures what its transform
///     produced, so the pipeline can be asserted as objects rather than as generated text.
/// </summary>
/// <remarks>
///     Deliberately not marked with <see cref="GeneratorAttribute"/>: the driver takes it via
///     <c>AsSourceGenerator()</c>, and the attribute would let it be discovered as an analyzer.
/// </remarks>
/// <param name="register">Attaches the provider under test to a <see cref="SyntaxValueProvider"/>.</param>
internal sealed class AttributeProviderProbe<T>(Func<SyntaxValueProvider, IncrementalValuesProvider<T>> register)
    : IIncrementalGenerator
{
    private readonly List<T> results = [];

    /// <summary>
    ///     Every value the provider produced, in the order the driver reported them.
    /// </summary>
    public IReadOnlyList<T> Results => this.results;

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(register(context.SyntaxProvider), (_, value) => this.results.Add(value));
    }
}
