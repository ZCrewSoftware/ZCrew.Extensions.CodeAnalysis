using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Convenience entry point for <see cref="RoslynTestBuilder{TVerifier}"/> for a single
///     <see cref="IIncrementalGenerator"/> that uses the <see cref="DefaultVerifier"/>.
/// </summary>
public static class IncrementalGeneratorTestBuilder
{
    /// <summary>
    ///     Creates an empty, unconfigured builder that uses the <see cref="DefaultVerifier"/> and runs the
    ///     <typeparamref name="TIncrementalGenerator"/>.
    /// </summary>
    /// <typeparam name="TIncrementalGenerator">The <see cref="IIncrementalGenerator"/> under test.</typeparam>
    /// <returns>A new builder with no other configuration applied.</returns>
    public static RoslynTestBuilder<DefaultVerifier> Create<TIncrementalGenerator>()
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        return RoslynTestBuilder<DefaultVerifier>.Create().WithIncrementalGenerator<TIncrementalGenerator>();
    }

    /// <summary>
    ///     Creates a builder that uses the <see cref="DefaultVerifier"/> and runs the
    ///     <typeparamref name="TIncrementalGenerator"/>, pre-configured with the common defaults described on
    ///     <see cref="RoslynTestBuilder{TVerifier}.CreateDefaultBuilder"/>.
    /// </summary>
    /// <typeparam name="TIncrementalGenerator">The <see cref="IIncrementalGenerator"/> under test.</typeparam>
    /// <returns>A new builder pre-configured with common defaults.</returns>
    public static RoslynTestBuilder<DefaultVerifier> CreateDefaultBuilder<TIncrementalGenerator>()
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        return RoslynTestBuilder<DefaultVerifier>
            .CreateDefaultBuilder()
            .WithIncrementalGenerator<TIncrementalGenerator>();
    }
}
