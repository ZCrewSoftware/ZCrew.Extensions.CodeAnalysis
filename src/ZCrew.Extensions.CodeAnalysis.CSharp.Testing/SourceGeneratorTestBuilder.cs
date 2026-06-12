using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Convenience entry point for <see cref="SourceGeneratorTestBuilder{TSourceGenerator, TVerifier}" /> that uses the
///     <see cref="DefaultVerifier" />.
/// </summary>
/// <typeparam name="TSourceGenerator">
///     The source generator under test. Must be an <see cref="IIncrementalGenerator" /> or
///     <see cref="ISourceGenerator" /> with a public parameterless constructor.
/// </typeparam>
public static class SourceGeneratorTestBuilder<TSourceGenerator>
    where TSourceGenerator : new()
{
    /// <summary>
    ///     Creates an empty, unconfigured builder that uses the <see cref="DefaultVerifier" />.
    /// </summary>
    /// <returns>A new builder with no configuration applied.</returns>
    public static SourceGeneratorTestBuilder<TSourceGenerator, DefaultVerifier> Create()
    {
        return SourceGeneratorTestBuilder<TSourceGenerator, DefaultVerifier>.Create();
    }

    /// <summary>
    ///     Creates a builder that uses the <see cref="DefaultVerifier" />, pre-configured with the common defaults
    ///     described on <see cref="SourceGeneratorTestBuilder{TSourceGenerator, TVerifier}.CreateDefaultBuilder" />.
    /// </summary>
    /// <returns>A new builder pre-configured with common defaults.</returns>
    public static SourceGeneratorTestBuilder<TSourceGenerator, DefaultVerifier> CreateDefaultBuilder()
    {
        return SourceGeneratorTestBuilder<TSourceGenerator, DefaultVerifier>.CreateDefaultBuilder();
    }
}

/// <summary>
///     Builds <see cref="CSharpSourceGeneratorTest{TSourceGenerator, TVerifier}" /> instances from a
///     <see cref="TestCase" />.
/// </summary>
/// <remarks>
///     The builder is immutable: every <c>With*</c> method returns a new builder and leaves the original unchanged.
///     This lets a fully configured instance be shared as a test fixture and specialized per test without affecting
///     other tests that share it.
/// </remarks>
/// <typeparam name="TSourceGenerator">
///     The source generator under test. Must be an <see cref="IIncrementalGenerator" /> or
///     <see cref="ISourceGenerator" /> with a public parameterless constructor.
/// </typeparam>
/// <typeparam name="TVerifier">
///     The <see cref="IVerifier" /> used to assert results, for example <see cref="DefaultVerifier" />.
/// </typeparam>
/// <example>
///     Configure a shared baseline once and fork it per test:
///     <code>
///     private static readonly SourceGeneratorTestBuilder&lt;MyGenerator, DefaultVerifier&gt; Baseline =
///         SourceGeneratorTestBuilder&lt;MyGenerator&gt;
///             .CreateDefaultBuilder()
///             .WithReferenceAssemblies(ReferenceAssemblies.Net.Net100);
///
///     [Fact]
///     public async Task Generates_Expected_Sources()
///     {
///         var testCase = await JsonTestCase.FromJsonFileAsync("TestCases/MyCase.json");
///         // The shared Baseline is never mutated; BuildAsync produces a fresh test.
///         var test = await Baseline.BuildAsync(testCase);
///         await test.RunAsync();
///     }
///     </code>
/// </example>
public sealed class SourceGeneratorTestBuilder<TSourceGenerator, TVerifier>
    where TSourceGenerator : new()
    where TVerifier : IVerifier, new()
{
    private ReferenceAssemblies? referenceAssemblies;
    private ImmutableList<string> additionalReferences = ImmutableList<string>.Empty;
    private CompilerDiagnostics? compilerDiagnostics;
    private ImmutableList<string> disabledDiagnostics = ImmutableList<string>.Empty;
    private ImmutableList<(string HintName, SourceText Content)> generatedSources = ImmutableList<(
        string HintName,
        SourceText Content
    )>.Empty;
    private bool includePostInitializationSources;
    private ImmutableList<Func<string, string>> contentTransforms = ImmutableList<Func<string, string>>.Empty;
    private ImmutableList<Action<CSharpSourceGeneratorTest<TSourceGenerator, TVerifier>>> configurations =
        ImmutableList<Action<CSharpSourceGeneratorTest<TSourceGenerator, TVerifier>>>.Empty;
    private Func<string, string>? generatedFilePathResolver;

    private SourceGeneratorTestBuilder() { }

    /// <summary>
    ///     Creates an empty, unconfigured builder.
    /// </summary>
    /// <returns>A new builder with no configuration applied.</returns>
    public static SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> Create()
    {
        return new SourceGeneratorTestBuilder<TSourceGenerator, TVerifier>();
    }

    /// <summary>
    ///     Creates a builder pre-configured with the most common defaults:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 the generator's post-initialization sources are captured and verified
    ///                 (<see cref="WithGeneratorPostInitializationSources" />);
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>all compiler diagnostics are verified (<see cref="CompilerDiagnostics.All" />);</description>
    ///         </item>
    ///         <item>
    ///             <description>the missing-XML-comment warning (<c>CS1591</c>) is suppressed.</description>
    ///         </item>
    ///     </list>
    ///     Reference assemblies are not set; call <see cref="WithReferenceAssemblies" /> to target a specific framework.
    /// </summary>
    /// <returns>A new builder pre-configured with common defaults.</returns>
    public static SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> CreateDefaultBuilder()
    {
        return Create()
            .WithGeneratorPostInitializationSources()
            .WithCompilerDiagnostics(CompilerDiagnostics.All)
            // Disable the warning on the source files about missing XML comments
            .WithDisabledDiagnostics("CS1591");
    }

    /// <summary>
    ///     Sets the <see cref="ReferenceAssemblies" /> (i.e. the target framework) the code is compiled against.
    /// </summary>
    /// <param name="value">The reference assemblies to compile against.</param>
    /// <returns>A new builder that compiles against <paramref name="value" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithReferenceAssemblies(ReferenceAssemblies value)
    {
        var builder = Clone();
        builder.referenceAssemblies = value;
        return builder;
    }

    /// <summary>
    ///     Adds the <paramref name="assemblyName" /> to the test state's additional references.
    /// </summary>
    /// <param name="assemblyName">
    ///     The assembly to reference, e.g. a file name such as <c>"MyLibrary.dll"</c> resolvable by the test host.
    /// </param>
    /// <returns>A new builder that additionally references <paramref name="assemblyName" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithAdditionalReference(string assemblyName)
    {
        var builder = Clone();
        builder.additionalReferences = this.additionalReferences.Add(assemblyName);
        return builder;
    }

    /// <summary>
    ///     Adds the <paramref name="assemblyNames" /> to the test state's additional references.
    /// </summary>
    /// <param name="assemblyNames">The assemblies to reference.</param>
    /// <returns>A new builder that additionally references <paramref name="assemblyNames" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithAdditionalReferences(
        params string[] assemblyNames
    )
    {
        var builder = Clone();
        builder.additionalReferences = this.additionalReferences.AddRange(assemblyNames);
        return builder;
    }

    /// <summary>
    ///     Sets which compiler diagnostics the test verifies.
    /// </summary>
    /// <param name="value">The category of compiler diagnostics to verify.</param>
    /// <returns>A new builder that verifies <paramref name="value" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithCompilerDiagnostics(CompilerDiagnostics value)
    {
        var builder = Clone();
        builder.compilerDiagnostics = value;
        return builder;
    }

    /// <summary>
    ///     Disables the diagnostic with <paramref name="diagnosticId" /> during verification.
    /// </summary>
    /// <param name="diagnosticId">The diagnostic id to suppress, e.g. <c>"CS1591"</c>.</param>
    /// <returns>A new builder that suppresses <paramref name="diagnosticId" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithDisabledDiagnostic(string diagnosticId)
    {
        var builder = Clone();
        builder.disabledDiagnostics = this.disabledDiagnostics.Add(diagnosticId);
        return builder;
    }

    /// <summary>
    ///     Disables the diagnostics with <paramref name="diagnosticIds" /> during verification.
    /// </summary>
    /// <param name="diagnosticIds">The diagnostic ids to suppress.</param>
    /// <returns>A new builder that suppresses <paramref name="diagnosticIds" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithDisabledDiagnostics(
        params string[] diagnosticIds
    )
    {
        var builder = Clone();
        builder.disabledDiagnostics = this.disabledDiagnostics.AddRange(diagnosticIds);
        return builder;
    }

    /// <summary>
    ///     Adds a baseline expected generated source, independent of any test case. The hint name is mapped to its full
    ///     generated file path at build time (see <see cref="WithGeneratedFilePathResolver" />).
    /// </summary>
    /// <param name="hintName">
    ///     The hint name the generator supplies when calling
    ///     <see cref="SourceProductionContext.AddSource(string, SourceText)" /> (or the post-initialization equivalent).
    /// </param>
    /// <param name="content">The expected content of the generated source.</param>
    /// <returns>
    ///     A new builder that expects <paramref name="content" /> to be generated for <paramref name="hintName" />.
    /// </returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithGeneratedSource(
        string hintName,
        SourceText content
    )
    {
        var builder = Clone();
        builder.generatedSources = this.generatedSources.Add((hintName, content));
        return builder;
    }

    /// <summary>
    ///     Adds the sources <typeparamref name="TSourceGenerator" /> emits during post-initialization as baseline
    ///     expected generated sources. The sources are captured once per generator type by running it against an empty
    ///     compilation, then cached.
    /// </summary>
    /// <remarks>
    ///     Call this whenever the generator registers post-initialization output — for example a generator that calls
    ///     <see cref="IncrementalGeneratorPostInitializationContext.AddEmbeddedAttributeDefinition" />. Otherwise the
    ///     test fails because those sources are generated but not expected.
    /// </remarks>
    /// <returns>A new builder that additionally expects the generator's post-initialization sources.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithGeneratorPostInitializationSources()
    {
        var builder = Clone();
        builder.includePostInitializationSources = true;
        return builder;
    }

    /// <summary>
    ///     Registers a content transform that replaces every <c>$(name)</c> token with <paramref name="value" /> in all
    ///     test case file contents (both sources and expected generated files) as they are loaded.
    /// </summary>
    /// <param name="name">The variable name; the token <c>$(name)</c> is replaced.</param>
    /// <param name="value">The replacement value.</param>
    /// <returns>A new builder that substitutes <paramref name="name" /> with <paramref name="value" />.</returns>
    /// <example>
    ///     Replace <c>$(Namespace)</c> in the test files with <c>MyApp.Generated</c>:
    ///     <code>
    ///     builder.WithVariable("Namespace", "MyApp.Generated");
    ///     </code>
    /// </example>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithVariable(string name, string value)
    {
        return WithContentTransform(content => content.Replace($"$({name})", value));
    }

    /// <summary>
    ///     Adds a transform applied to all test case file contents (sources and expected generated files) as they are
    ///     loaded. Transforms run in registration order.
    /// </summary>
    /// <param name="transform">A function that maps the raw file contents to the contents used by the test.</param>
    /// <returns>A new builder that applies <paramref name="transform" /> when loading file contents.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithContentTransform(Func<string, string> transform)
    {
        var builder = Clone();
        builder.contentTransforms = this.contentTransforms.Add(transform);
        return builder;
    }

    /// <summary>
    ///     Overrides how generated source hint names are mapped to full generated file paths. The default maps a hint
    ///     name to <c>{generator assembly name}/{generator full type name}/{hint name}</c>.
    /// </summary>
    /// <param name="resolver">A function that maps a hint name to its full generated file path.</param>
    /// <returns>A new builder that resolves generated file paths using <paramref name="resolver" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithGeneratedFilePathResolver(
        Func<string, string> resolver
    )
    {
        var builder = Clone();
        builder.generatedFilePathResolver = resolver;
        return builder;
    }

    /// <summary>
    ///     Adds an arbitrary configuration action applied to the built test after all other configuration. Use this as
    ///     an escape hatch for settings the builder does not model directly.
    /// </summary>
    /// <param name="configure">An action that mutates the fully configured test before it is returned.</param>
    /// <returns>A new builder that applies <paramref name="configure" /> during <see cref="BuildAsync" />.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithConfiguration(
        Action<CSharpSourceGeneratorTest<TSourceGenerator, TVerifier>> configure
    )
    {
        var builder = Clone();
        builder.configurations = this.configurations.Add(configure);
        return builder;
    }

    /// <summary>
    ///     Builds a <see cref="CSharpSourceGeneratorTest{TSourceGenerator, TVerifier}" /> from
    ///     <paramref name="testCase" />, applying this builder's configuration. The returned test is ready to run via
    ///     its <c>RunAsync</c> method.
    /// </summary>
    /// <param name="testCase">
    ///     The test case describing the source files to compile and the generated files to verify. File names are
    ///     resolved relative to <see cref="TestCase.Directory" />.
    /// </param>
    /// <param name="token">A token to cancel the asynchronous file loading.</param>
    /// <returns>A configured test that has not yet been run.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="token" /> is canceled.</exception>
    /// <exception cref="FileNotFoundException">
    ///     Thrown when a source or generated file referenced by <paramref name="testCase" /> cannot be found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when post-initialization sources were requested via
    ///     <see cref="WithGeneratorPostInitializationSources" /> but <typeparamref name="TSourceGenerator" /> is neither
    ///     an <see cref="IIncrementalGenerator" /> nor an <see cref="ISourceGenerator" />.
    /// </exception>
    public async Task<CSharpSourceGeneratorTest<TSourceGenerator, TVerifier>> BuildAsync(
        TestCase testCase,
        CancellationToken token = default
    )
    {
        var test = new CSharpSourceGeneratorTest<TSourceGenerator, TVerifier>();

        if (this.referenceAssemblies != null)
        {
            test.ReferenceAssemblies = this.referenceAssemblies;
        }

        foreach (var reference in this.additionalReferences)
        {
            test.TestState.AdditionalReferences.Add(reference);
        }

        if (this.compilerDiagnostics.HasValue)
        {
            test.CompilerDiagnostics = this.compilerDiagnostics.Value;
        }

        foreach (var diagnosticId in this.disabledDiagnostics)
        {
            test.DisabledDiagnostics.Add(diagnosticId);
        }

        if (this.includePostInitializationSources)
        {
            foreach (var (hintName, content) in GeneratorPostInitializationSources<TSourceGenerator>.Sources)
            {
                test.TestState.GeneratedSources.Add((ResolveGeneratedPath(hintName), content));
            }
        }

        foreach (var (hintName, content) in this.generatedSources)
        {
            test.TestState.GeneratedSources.Add((ResolveGeneratedPath(hintName), content));
        }

        var sourceFileTasks = testCase.SourceFiles.Select(file => LoadSourceFileAsync(file, testCase, token));
        var generatedFileTasks = testCase.GeneratedFiles.Select(file => LoadGeneratedFileAsync(file, testCase, token));

        var sourceFiles = await Task.WhenAll(sourceFileTasks).ConfigureAwait(false);
        var generatedFiles = await Task.WhenAll(generatedFileTasks).ConfigureAwait(false);

        test.TestState.Sources.AddRange(sourceFiles);
        test.TestState.GeneratedSources.AddRange(generatedFiles);

        foreach (var configure in this.configurations)
        {
            configure(test);
        }

        return test;
    }

    private SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> Clone()
    {
        return new SourceGeneratorTestBuilder<TSourceGenerator, TVerifier>
        {
            referenceAssemblies = this.referenceAssemblies,
            additionalReferences = this.additionalReferences,
            compilerDiagnostics = this.compilerDiagnostics,
            disabledDiagnostics = this.disabledDiagnostics,
            generatedSources = this.generatedSources,
            includePostInitializationSources = this.includePostInitializationSources,
            contentTransforms = this.contentTransforms,
            configurations = this.configurations,
            generatedFilePathResolver = this.generatedFilePathResolver,
        };
    }

    private async Task<(string filename, SourceText content)> LoadSourceFileAsync(
        TestSourceFile testSourceFile,
        TestCase testCase,
        CancellationToken token
    )
    {
        var contents = await LoadFileAsync(testCase.Directory, testSourceFile.SourceFileName, token)
            .ConfigureAwait(false);
        return (testSourceFile.SourceFileName, SourceText.From(contents, Encoding.UTF8));
    }

    private async Task<(string filename, SourceText content)> LoadGeneratedFileAsync(
        TestGeneratedFile testGeneratedFile,
        TestCase testCase,
        CancellationToken token
    )
    {
        var contents = await LoadFileAsync(testCase.Directory, testGeneratedFile.SourceFileName, token)
            .ConfigureAwait(false);
        return (ResolveGeneratedPath(testGeneratedFile.GeneratedFileName), SourceText.From(contents, Encoding.UTF8));
    }

    private async Task<string> LoadFileAsync(string? directory, string fileName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var fullFileName = GetTestFilePath(directory, fileName);
        using var reader = new StreamReader(fullFileName, Encoding.UTF8);
        var contents = await reader.ReadToEndAsync().ConfigureAwait(false);

        foreach (var transform in this.contentTransforms)
        {
            contents = transform(contents);
        }

        return contents;
    }

    private string ResolveGeneratedPath(string hintName)
    {
        if (this.generatedFilePathResolver != null)
        {
            return this.generatedFilePathResolver(hintName);
        }

        return TestPath.Empty
            / typeof(TSourceGenerator).Assembly.GetName().Name
            / typeof(TSourceGenerator).FullName
            / hintName;
    }

    private static string GetTestFilePath(string? directory, string fileName)
    {
        var fullPath =
            directory == null ? TestPath.CurrentDirectory / fileName : TestPath.CurrentDirectory / directory / fileName;
        return fullPath;
    }
}
