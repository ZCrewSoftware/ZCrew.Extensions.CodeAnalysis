using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     A generator registered with a test, either an <see cref="IIncrementalGenerator"/> or an
///     <see cref="ISourceGenerator"/>.
/// </summary>
/// <remarks>
///     Instances are created through <see cref="ForIncrementalGenerator{TIncrementalGenerator}"/> and
///     <see cref="ForSourceGenerator{TSourceGenerator}"/>, which the <c>With*Generator</c> methods on
///     <see cref="RoslynTestBuilder{TVerifier}"/> call for you.
/// </remarks>
public abstract class Generator
{
    private readonly Lazy<ImmutableArray<GeneratedSource>> postInitializationGeneratedSources;

    /// <summary>
    ///     Initializes a new <see cref="Generator"/>.
    /// </summary>
    protected Generator()
    {
        this.postInitializationGeneratedSources = new Lazy<ImmutableArray<GeneratedSource>>(
            FetchPostInitializationGeneratedSources
        );
    }

    internal abstract Type SourceGeneratorType { get; }

    internal abstract IIncrementalGenerator CreateIncrementalGenerator();

    /// <summary>
    ///     Creates a <see cref="Generator"/> for an <see cref="IIncrementalGenerator"/>.
    /// </summary>
    /// <typeparam name="TIncrementalGenerator">The <see cref="IIncrementalGenerator"/> type.</typeparam>
    /// <returns>A <see cref="Generator"/> that runs <typeparamref name="TIncrementalGenerator"/>.</returns>
    public static Generator ForIncrementalGenerator<TIncrementalGenerator>()
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        return new IncrementalGenerator<TIncrementalGenerator>();
    }

    /// <summary>
    ///     Creates a <see cref="Generator"/> for an <see cref="ISourceGenerator"/>.
    /// </summary>
    /// <typeparam name="TSourceGenerator">The <see cref="ISourceGenerator"/> type.</typeparam>
    /// <returns>A <see cref="Generator"/> that runs <typeparamref name="TSourceGenerator"/>.</returns>
    /// <remarks>
    ///     Since <see cref="ISourceGenerator"/> is obsolete, you probably meant to use
    ///     <see cref="ForIncrementalGenerator{TIncrementalGenerator}"/>.
    /// </remarks>
    public static Generator ForSourceGenerator<TSourceGenerator>()
        where TSourceGenerator : ISourceGenerator, new()
    {
        return new SourceGenerator<TSourceGenerator>();
    }

    /// <summary>
    ///     The sources this generator emits during post-initialization, captured once per instance.
    /// </summary>
    internal ImmutableArray<GeneratedSource> GetPostInitializationGeneratedSources()
    {
        return this.postInitializationGeneratedSources.Value;
    }

    private ImmutableArray<GeneratedSource> FetchPostInitializationGeneratedSources()
    {
        var incrementalGenerator = CreateIncrementalGenerator();

        // With no syntax trees, the only outputs a generator can produce are its post-initialization sources
        var compilation = CSharpCompilation.Create(
            "ZCrew.PostInitializationCapture",
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = CSharpGeneratorDriver.Create(incrementalGenerator).RunGenerators(compilation).GetRunResult();
        return
        [
            .. runResult.Results[0].GeneratedSources.Select(result => new GeneratedSource(SourceGeneratorType, result)),
        ];
    }
}
