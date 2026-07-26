using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Convenience entry point for <see cref="RoslynTestBuilder{TVerifier}"/> that uses the
///     <see cref="DefaultVerifier"/>.
/// </summary>
public static class RoslynTestBuilder
{
    /// <summary>
    ///     Creates an empty, unconfigured builder that uses the <see cref="DefaultVerifier"/>.
    /// </summary>
    /// <returns>A new builder with no configuration applied.</returns>
    public static RoslynTestBuilder<DefaultVerifier> Create()
    {
        return RoslynTestBuilder<DefaultVerifier>.Create();
    }

    /// <summary>
    ///     Creates a builder that uses the <see cref="DefaultVerifier"/>, pre-configured with the common defaults
    ///     described on <see cref="RoslynTestBuilder{TVerifier}.CreateDefaultBuilder"/>.
    /// </summary>
    /// <returns>A new builder pre-configured with common defaults.</returns>
    public static RoslynTestBuilder<DefaultVerifier> CreateDefaultBuilder()
    {
        return RoslynTestBuilder<DefaultVerifier>.CreateDefaultBuilder();
    }
}

/// <summary>
///     Builds <see cref="RoslynTest{TVerifier}"/> instances from an <see cref="ITestCase"/>.
/// </summary>
/// <remarks>
///     The builder is immutable: every <c>With*</c> method returns a new builder and leaves the original unchanged.
///     This lets a fully configured instance be shared as a test fixture and specialized per test without affecting
///     other tests that share it.
/// </remarks>
/// <typeparam name="TVerifier">
///     The <see cref="IVerifier"/> used to assert results, for example <see cref="DefaultVerifier"/>.
/// </typeparam>
/// <example>
///     Configure a shared baseline once and fork it per test:
///     <code>
///     private static readonly RoslynTestBuilder&lt;DefaultVerifier&gt; Baseline =
///         RoslynTestBuilder
///             .CreateDefaultBuilder()
///             .WithGenerator&lt;MyGenerator&gt;()
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
public class RoslynTestBuilder<TVerifier>
    where TVerifier : IVerifier, new()
{
    private ReferenceAssemblies? referenceAssemblies;
    private ImmutableList<string> additionalReferences = ImmutableList<string>.Empty;
    private CompilerDiagnostics? compilerDiagnostics;
    private ImmutableList<string> disabledDiagnostics = ImmutableList<string>.Empty;

    private ImmutableList<GeneratedSource> generatedSources = ImmutableList<GeneratedSource>.Empty;
    private ImmutableDictionary<string, object?> properties = ImmutableDictionary<string, object?>.Empty;
    private bool includePostInitializationSources;
    private bool updateExpectedSources;
    private ImmutableList<Action<RoslynTest<TVerifier>>> configurations = ImmutableList<
        Action<RoslynTest<TVerifier>>
    >.Empty;
    private ImmutableList<Generator> generators = ImmutableList<Generator>.Empty;
    private ImmutableList<DiagnosticAnalyzer> analyzers = ImmutableList<DiagnosticAnalyzer>.Empty;

    /// <summary>
    ///     Creates an empty, unconfigured builder.
    /// </summary>
    protected RoslynTestBuilder() { }

    /// <summary>
    ///     Creates an empty, unconfigured builder.
    /// </summary>
    /// <returns>A new builder with no configuration applied.</returns>
    public static RoslynTestBuilder<TVerifier> Create()
    {
        return new RoslynTestBuilder<TVerifier>();
    }

    /// <summary>
    ///     Creates a builder pre-configured with the most common defaults:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 every registered generator's post-initialization sources are captured and verified
    ///                 (<see cref="WithGeneratorPostInitializationSources"/>);
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>all compiler diagnostics are verified (<see cref="CompilerDiagnostics.All"/>);</description>
    ///         </item>
    ///         <item>
    ///             <description>the missing-XML-comment warning (<c>CS1591</c>) is suppressed.</description>
    ///         </item>
    ///     </list>
    ///     Reference assemblies are not set; call <see cref="WithReferenceAssemblies"/> to target a specific framework.
    /// </summary>
    /// <returns>A new builder pre-configured with common defaults.</returns>
    public static RoslynTestBuilder<TVerifier> CreateDefaultBuilder()
    {
        return Create()
            .WithGeneratorPostInitializationSources()
            .WithCompilerDiagnostics(CompilerDiagnostics.All)
            // Disable the warning on the source files about missing XML comments
            .WithDisabledDiagnostics("CS1591");
    }

    /// <inheritdoc cref="WithIncrementalGenerator{TIncrementalGenerator}"/>
    /// <remarks>
    ///     Adds the recommended generator type. This is the method most test suites should reach for since
    ///     <see cref="ISourceGenerator"/> is obsolete.
    /// </remarks>
    public RoslynTestBuilder<TVerifier> WithGenerator<TIncrementalGenerator>()
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        return WithIncrementalGenerator<TIncrementalGenerator>();
    }

    /// <summary>
    ///     Adds an <see cref="IIncrementalGenerator"/> to the test.
    /// </summary>
    /// <typeparam name="TIncrementalGenerator">The <see cref="IIncrementalGenerator"/> type.</typeparam>
    /// <returns>A new builder that runs the <typeparamref name="TIncrementalGenerator"/> generator.</returns>
    /// <exception cref="ArgumentException">If this generator was already added.</exception>
    public RoslynTestBuilder<TVerifier> WithIncrementalGenerator<TIncrementalGenerator>()
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        return AddGenerator(Generator.ForIncrementalGenerator<TIncrementalGenerator>());
    }

    /// <summary>
    ///     Adds an <see cref="ISourceGenerator"/> to the test.
    /// </summary>
    /// <typeparam name="TSourceGenerator">The <see cref="ISourceGenerator"/> type.</typeparam>
    /// <returns>A new builder that runs the <typeparamref name="TSourceGenerator"/> generator.</returns>
    /// <remarks>
    ///     Since <see cref="ISourceGenerator"/> is obsolete, you probably meant to use
    ///     <see cref="WithIncrementalGenerator{TIncrementalGenerator}"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">If this generator was already added.</exception>
    public RoslynTestBuilder<TVerifier> WithSourceGenerator<TSourceGenerator>()
        where TSourceGenerator : ISourceGenerator, new()
    {
        return AddGenerator(Generator.ForSourceGenerator<TSourceGenerator>());
    }

    /// <summary>
    ///     Adds a <see cref="DiagnosticAnalyzer"/> to the test.
    /// </summary>
    /// <typeparam name="TDiagnosticAnalyzer">The <see cref="DiagnosticAnalyzer"/> type.</typeparam>
    /// <returns>A new builder that runs the <typeparamref name="TDiagnosticAnalyzer"/> analyzer.</returns>
    /// <exception cref="ArgumentException">If this analyzer was already added.</exception>
    public RoslynTestBuilder<TVerifier> WithDiagnosticAnalyzer<TDiagnosticAnalyzer>()
        where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
    {
        VerifyAnalyzerNotPresent(typeof(TDiagnosticAnalyzer));
        var builder = Clone();
        builder.analyzers = builder.analyzers.Add(new TDiagnosticAnalyzer());
        return builder;
    }

    /// <summary>
    ///     Adds an already constructed <see cref="DiagnosticAnalyzer"/> to the test.
    /// </summary>
    /// <param name="analyzer">The analyzer to run.</param>
    /// <returns>A new builder that runs <paramref name="analyzer"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="analyzer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">If an analyzer of the same type was already added.</exception>
    public RoslynTestBuilder<TVerifier> WithDiagnosticAnalyzer(DiagnosticAnalyzer analyzer)
    {
        if (analyzer == null)
        {
            throw new ArgumentNullException(nameof(analyzer));
        }
        VerifyAnalyzerNotPresent(analyzer.GetType());
        var builder = Clone();
        builder.analyzers = builder.analyzers.Add(analyzer);
        return builder;
    }

    /// <summary>
    ///     Sets the <see cref="ReferenceAssemblies"/> (i.e. the target framework) the code is compiled against.
    /// </summary>
    /// <param name="referenceAssemblies">The reference assemblies to compile against.</param>
    /// <returns>A new builder that compiles against <paramref name="referenceAssemblies"/>.</returns>
    /// <exception cref="ArgumentNullException">
    ///     If <paramref name="referenceAssemblies"/> is <see langword="null"/>.
    /// </exception>
    public RoslynTestBuilder<TVerifier> WithReferenceAssemblies(ReferenceAssemblies referenceAssemblies)
    {
        if (referenceAssemblies == null)
        {
            throw new ArgumentNullException(nameof(referenceAssemblies));
        }
        var builder = Clone();
        builder.referenceAssemblies = referenceAssemblies;
        return builder;
    }

    /// <summary>
    ///     Adds the <paramref name="assemblyName"/> to the test state's additional references.
    /// </summary>
    /// <param name="assemblyName">
    ///     The assembly to reference, e.g. a file name such as <c>"MyLibrary.dll"</c> resolvable by the test host.
    /// </param>
    /// <returns>A new builder that additionally references <paramref name="assemblyName"/>.</returns>
    public RoslynTestBuilder<TVerifier> WithAdditionalReference(string assemblyName)
    {
        var builder = Clone();
        builder.additionalReferences = this.additionalReferences.Add(assemblyName);
        return builder;
    }

    /// <summary>
    ///     Adds the <paramref name="assemblyNames"/> to the test state's additional references.
    /// </summary>
    /// <param name="assemblyNames">The assemblies to reference.</param>
    /// <returns>A new builder that additionally references <paramref name="assemblyNames"/>.</returns>
    public RoslynTestBuilder<TVerifier> WithAdditionalReferences(params string[] assemblyNames)
    {
        var builder = Clone();
        builder.additionalReferences = this.additionalReferences.AddRange(assemblyNames);
        return builder;
    }

    /// <summary>
    ///     Sets which compiler diagnostics the test verifies.
    /// </summary>
    /// <param name="value">The category of compiler diagnostics to verify.</param>
    /// <returns>A new builder that verifies <paramref name="value"/>.</returns>
    public RoslynTestBuilder<TVerifier> WithCompilerDiagnostics(CompilerDiagnostics value)
    {
        var builder = Clone();
        builder.compilerDiagnostics = value;
        return builder;
    }

    /// <summary>
    ///     Disables the diagnostic with <paramref name="diagnosticId"/> during verification.
    /// </summary>
    /// <param name="diagnosticId">The diagnostic id to suppress, e.g. <c>"CS1591"</c>.</param>
    /// <returns>A new builder that suppresses <paramref name="diagnosticId"/>.</returns>
    public RoslynTestBuilder<TVerifier> WithDisabledDiagnostic(string diagnosticId)
    {
        var builder = Clone();
        builder.disabledDiagnostics = this.disabledDiagnostics.Add(diagnosticId);
        return builder;
    }

    /// <summary>
    ///     Disables the diagnostics with <paramref name="diagnosticIds"/> during verification.
    /// </summary>
    /// <param name="diagnosticIds">The diagnostic ids to suppress.</param>
    /// <returns>A new builder that suppresses <paramref name="diagnosticIds"/>.</returns>
    public RoslynTestBuilder<TVerifier> WithDisabledDiagnostics(params string[] diagnosticIds)
    {
        var builder = Clone();
        builder.disabledDiagnostics = this.disabledDiagnostics.AddRange(diagnosticIds);
        return builder;
    }

    /// <summary>
    ///     Adds a baseline expected generated source, independent of any test case. The file name is used verbatim, so
    ///     it must be the full path Roslyn emits — <c>{generator assembly name}/{generator full type name}/{hint
    ///     name}</c> — and <b>not</b> just the hint name. Prefer
    ///     <see cref="WithGeneratedSource{TGenerator}(string, SourceText)"/>, which builds that path for you.
    /// </summary>
    /// <param name="fileName">The full generated file path, including the generator's assembly and type name.</param>
    /// <param name="content">The expected content of the generated source.</param>
    /// <returns>
    ///     A new builder that expects <paramref name="content"/> to be generated for <paramref name="fileName"/>.
    /// </returns>
    public RoslynTestBuilder<TVerifier> WithGeneratedSource(string fileName, SourceText content)
    {
        var source = new GeneratedSource(fileName, content);
        var builder = Clone();
        builder.generatedSources = this.generatedSources.Add(source);
        return builder;
    }

    /// <summary>
    ///     Adds a baseline expected generated source, independent of any test case. The hint name must match the exact
    ///     hint name the generator emits.
    /// </summary>
    /// <param name="hintName">
    ///     The hint name the generator supplies when calling
    ///     <see cref="SourceProductionContext.AddSource(string, SourceText)"/> (or the post-initialization equivalent).
    /// </param>
    /// <param name="content">The expected content of the generated source.</param>
    /// <typeparam name="TGenerator">The generator type, to fully qualify the <paramref name="hintName"/>.</typeparam>
    /// <returns>
    ///     A new builder that expects <paramref name="content"/> to be generated for <paramref name="hintName"/>.
    /// </returns>
    public RoslynTestBuilder<TVerifier> WithGeneratedSource<TGenerator>(string hintName, SourceText content)
    {
        var source = new GeneratedSource(typeof(TGenerator), hintName, content);
        var builder = Clone();
        builder.generatedSources = this.generatedSources.Add(source);
        return builder;
    }

    /// <summary>
    ///     Adds the sources every registered generator emits during post-initialization as baseline expected generated
    ///     sources. The sources are captured by running each generator against an empty compilation, then cached on
    ///     that <see cref="Generator"/> instance.
    /// </summary>
    /// <remarks>
    ///     Call this whenever a generator registers post-initialization output — for example a generator that calls
    ///     <see cref="IncrementalGeneratorPostInitializationContext.AddEmbeddedAttributeDefinition"/>. Otherwise, the
    ///     test fails because those sources are generated but not expected.
    /// </remarks>
    /// <returns>A new builder that additionally expects every generator's post-initialization sources.</returns>
    public RoslynTestBuilder<TVerifier> WithGeneratorPostInitializationSources()
    {
        var builder = Clone();
        builder.includePostInitializationSources = true;
        return builder;
    }

    /// <summary>
    ///     Enables in-place updates of the expected generated files. When enabled, <see cref="BuildAsync"/> runs the
    ///     generator and, for each expected generated file that is missing or whose content differs (comparing
    ///     line-ending insensitively), overwrites it on disk with the produced output (normalized to CRLF).
    /// </summary>
    /// <remarks>
    ///     Because it writes to the source tree, this is off by default. Gate it off wherever the workspace must not be
    ///     mutated (e.g. running tests in CI) by passing <paramref name="enabled"/> a condition such as
    ///     <c>Environment.GetEnvironmentVariable("CI") is null</c>. The assertion still runs when writes are
    ///     suppressed, so regressions are still caught.
    /// </remarks>
    /// <param name="enabled">Whether to write updated expected files; defaults to <see langword="true"/>.</param>
    /// <returns>A new builder that writes updated expected files when <paramref name="enabled"/> is set.</returns>
    public RoslynTestBuilder<TVerifier> WithExpectedSourceUpdates(bool enabled = true)
    {
        var builder = Clone();
        builder.updateExpectedSources = enabled;
        return builder;
    }

    /// <summary>
    ///     Registers a content transform that replaces every <c>$(name)</c> token with <paramref name="value"/> in all
    ///     test case file contents (both sources and expected generated files) as they are loaded.
    /// </summary>
    /// <param name="name">The variable name; the token <c>$(name)</c> is replaced.</param>
    /// <param name="value">The replacement value.</param>
    /// <returns>A new builder that substitutes <paramref name="name"/> with <paramref name="value"/>.</returns>
    /// <remarks>
    ///     Variables set here can be overridden by the test case.
    /// </remarks>
    /// <example>
    ///     Replace <c>$(Namespace)</c> in the test files with <c>MyApp.Generated</c>:
    ///     <code>
    ///     builder.WithVariable("Namespace", "MyApp.Generated");
    ///     </code>
    /// </example>
    public RoslynTestBuilder<TVerifier> WithVariable(string name, object? value)
    {
        var builder = Clone();
        builder.properties = builder.properties.SetItem(name, value);
        return builder;
    }

    /// <summary>
    ///     Adds an arbitrary configuration action applied to the built test after all other configuration. Use this as
    ///     an escape hatch for settings the builder does not model directly.
    /// </summary>
    /// <param name="configure">An action that mutates the fully configured test before it is returned.</param>
    /// <returns>A new builder that applies <paramref name="configure"/> during <see cref="BuildAsync"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="configure"/> is <see langword="null"/>.</exception>
    public RoslynTestBuilder<TVerifier> WithConfiguration(Action<RoslynTest<TVerifier>> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        var builder = Clone();
        builder.configurations = this.configurations.Add(configure);
        return builder;
    }

    /// <summary>
    ///     Builds a <see cref="RoslynTest{TVerifier}"/> from <paramref name="testCase"/>, applying this builder's
    ///     configuration. The returned test is ready to run via its <c>RunAsync</c> method.
    /// </summary>
    /// <param name="testCase">
    ///     The test case describing the source files to compile and the generated files to verify. File names are
    ///     resolved relative to <see cref="ITestCase.Directory"/>.
    /// </param>
    /// <param name="token">A token to cancel the asynchronous file loading.</param>
    /// <returns>A configured test that has not yet been run.</returns>
    /// <remarks>
    ///     Each registered generator contributes a variable named after its type, expanding to the directory Roslyn
    ///     emits that generator's sources into. Use it to qualify a
    ///     <see cref="TestGeneratedFile.GeneratedFileName"/>, for example
    ///     <c>$(MyGenerator)/MyNamespace.MyType.g.cs</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">If <paramref name="testCase"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="token"/> is canceled.</exception>
    /// <exception cref="FileNotFoundException">
    ///     Thrown when a source or generated file referenced by <paramref name="testCase"/> cannot be found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     When no generator was added using <see cref="WithGenerator{TIncrementalGenerator}"/>,
    ///     <see cref="WithIncrementalGenerator{TIncrementalGenerator}"/> or
    ///     <see cref="WithSourceGenerator{TSourceGenerator}"/>, and no analyzer was added using
    ///     <see cref="WithDiagnosticAnalyzer{TDiagnosticAnalyzer}"/>.
    /// </exception>
    public async Task<RoslynTest<TVerifier>> BuildAsync(ITestCase testCase, CancellationToken token = default)
    {
        if (testCase == null)
        {
            throw new ArgumentNullException(nameof(testCase));
        }
        if (this.generators.Count == 0 && this.analyzers.Count == 0)
        {
            throw new InvalidOperationException("There must be at least one generator or one analyzer to test");
        }

        var test = new RoslynTest<TVerifier>(this.generators, this.analyzers);

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
            foreach (var generator in this.generators)
            {
                var generatedSources = generator.GetPostInitializationGeneratedSources();
                foreach (var generatedSource in generatedSources)
                {
                    test.TestState.GeneratedSources.Add((generatedSource.FileName, generatedSource.Content));
                }
            }
        }

        foreach (var generatedSource in this.generatedSources)
        {
            test.TestState.GeneratedSources.Add((generatedSource.FileName, generatedSource.Content));
        }

        var sourceFileTasks = testCase.SourceFiles.Select(file => LoadSourceFileAsync(file, testCase, token));
        var sourceFiles = await Task.WhenAll(sourceFileTasks).ConfigureAwait(false);
        test.TestState.Sources.AddRange(sourceFiles);

        // When updating, write the generator's output over any missing/differing expected files, but assert against
        // the pre-existing content so a write always coincides with a failure (see WithExpectedSourceUpdates).
        var generatedFiles = this.updateExpectedSources
            ? await UpdateAndLoadGeneratedFilesAsync(testCase, sourceFiles, token).ConfigureAwait(false)
            : await Task.WhenAll(testCase.GeneratedFiles.Select(file => LoadGeneratedFileAsync(file, testCase, token)))
                .ConfigureAwait(false);

        test.TestState.GeneratedSources.AddRange(generatedFiles);

        AddExpectedDiagnostics(test, testCase, sourceFiles, generatedFiles);

        foreach (var configure in this.configurations)
        {
            configure(test);
        }

        return test;
    }

    private RoslynTestBuilder<TVerifier> Clone()
    {
        return new RoslynTestBuilder<TVerifier>
        {
            referenceAssemblies = this.referenceAssemblies,
            additionalReferences = this.additionalReferences,
            compilerDiagnostics = this.compilerDiagnostics,
            disabledDiagnostics = this.disabledDiagnostics,
            generatedSources = this.generatedSources,
            includePostInitializationSources = this.includePostInitializationSources,
            updateExpectedSources = this.updateExpectedSources,
            configurations = this.configurations,
            properties = this.properties,
            generators = this.generators,
            analyzers = this.analyzers,
        };
    }

    private async Task<(string filename, SourceText content)> LoadSourceFileAsync(
        TestSourceFile testSourceFile,
        ITestCase testCase,
        CancellationToken token
    )
    {
        var fileName = ExpandVariables(testCase, testSourceFile.SourceFileName);
        var contents = await LoadFileAsync(testCase, fileName, token).ConfigureAwait(false);
        return (fileName, SourceText.From(contents, Encoding.UTF8));
    }

    private async Task<(string filename, SourceText content)> LoadGeneratedFileAsync(
        TestGeneratedFile testGeneratedFile,
        ITestCase testCase,
        CancellationToken token
    )
    {
        var sourceFileName = ExpandVariables(testCase, testGeneratedFile.SourceFileName);
        var contents = await LoadFileAsync(testCase, sourceFileName, token).ConfigureAwait(false);
        var generatedFileName = ExpandVariables(testCase, testGeneratedFile.GeneratedFileName);
        return (NormalizeSeparators(generatedFileName), SourceText.From(contents, Encoding.UTF8));
    }

    private async Task<string> LoadFileAsync(ITestCase testCase, string fileName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var fullFileName = GetTestFilePath(testCase.Directory, fileName);
        using var reader = new StreamReader(fullFileName, Encoding.UTF8);
        var contents = await reader.ReadToEndAsync().ConfigureAwait(false);

        return ExpandVariables(testCase, contents);
    }

    private async Task<string?> TryLoadFileAsync(ITestCase testCase, string fileName, CancellationToken token)
    {
        var fullFileName = GetTestFilePath(testCase.Directory, fileName);
        if (!File.Exists(fullFileName))
        {
            return null;
        }

        return await LoadFileAsync(testCase, fileName, token).ConfigureAwait(false);
    }

    private async Task<(string filename, SourceText content)[]> UpdateAndLoadGeneratedFilesAsync(
        ITestCase testCase,
        (string filename, SourceText content)[] sourceFiles,
        CancellationToken token
    )
    {
        var produced = await GenerateSourcesAsync(sourceFiles, token).ConfigureAwait(false);

        var generatedFiles = new (string filename, SourceText content)[testCase.GeneratedFiles.Count];
        for (var i = 0; i < testCase.GeneratedFiles.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var generatedFile = testCase.GeneratedFiles[i];
            var sourceFileName = ExpandVariables(testCase, generatedFile.SourceFileName);
            var generatedFileName = NormalizeSeparators(ExpandVariables(testCase, generatedFile.GeneratedFileName));

            // The content the test will assert against: the expected file exactly as it exists now (null if it does
            // not exist yet) so that the test still fails
            var original = await TryLoadFileAsync(testCase, sourceFileName, token).ConfigureAwait(false);

            if (produced.TryGetValue(generatedFileName, out var producedText))
            {
                var producedContent = producedText.ToString();
                if (original == null || !LineEndingAgnosticEquals(original, producedContent))
                {
                    var filePath = GetTestFilePath(testCase.Directory, sourceFileName);
                    await WriteExpectedFileAsync(filePath, producedContent, token).ConfigureAwait(false);
                }
            }

            generatedFiles[i] = (generatedFileName, SourceText.From(original ?? string.Empty, Encoding.UTF8));
        }

        return generatedFiles;
    }

    private async Task<Dictionary<string, SourceText>> GenerateSourcesAsync(
        (string filename, SourceText content)[] sourceFiles,
        CancellationToken token
    )
    {
        var references = new List<MetadataReference>();
        if (this.referenceAssemblies != null)
        {
            references.AddRange(
                await this.referenceAssemblies.ResolveAsync(LanguageNames.CSharp, token).ConfigureAwait(false)
            );
        }

        foreach (var name in this.additionalReferences)
        {
            var path = Path.IsPathRooted(name) ? name : Path.Combine(AppContext.BaseDirectory, name);
            references.Add(MetadataReference.CreateFromFile(path));
        }

        var syntaxTrees = sourceFiles.Select(file =>
            CSharpSyntaxTree.ParseText(file.content, path: file.filename, cancellationToken: token)
        );

        var compilation = CSharpCompilation.Create(
            "ZCrew.ExpectedSourceUpdate",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var incrementalGenerators = this
            .generators.Select(generator => generator.CreateIncrementalGenerator())
            .ToArray();
        var runResult = CSharpGeneratorDriver
            .Create(incrementalGenerators)
            .RunGenerators(compilation, token)
            .GetRunResult();

        var produced = new Dictionary<string, SourceText>(StringComparer.Ordinal);
        foreach (var result in runResult.Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                var generatedSource = new GeneratedSource(result.Generator.GetGeneratorType(), source);
                produced[generatedSource.FileName] = generatedSource.Content;
            }
        }

        return produced;
    }

    private static async Task WriteExpectedFileAsync(string filePath, string content, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(
            filePath,
            append: false,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        );
        await writer.WriteAsync(NormalizeToCrlf(content)).ConfigureAwait(false);
    }

    private static bool LineEndingAgnosticEquals(string left, string right)
    {
        return NormalizeToLf(left) == NormalizeToLf(right);
    }

    private static string NormalizeToLf(string value)
    {
        return value.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static string NormalizeToCrlf(string value)
    {
        return NormalizeToLf(value).Replace("\n", "\r\n");
    }

    private void AddExpectedDiagnostics(
        RoslynTest<TVerifier> test,
        ITestCase testCase,
        (string filename, SourceText content)[] sourceFiles,
        (string filename, SourceText content)[] generatedFiles
    )
    {
        // Locationless diagnostics declared at the top level (Location.None).
        foreach (var expected in testCase.ExpectedDiagnostics)
        {
            test.TestState.ExpectedDiagnostics.Add(BuildLocationlessDiagnostic(expected, testCase));
        }

        // Diagnostics located within a specific input source file.
        for (var i = 0; i < testCase.SourceFiles.Count; i++)
        {
            var (path, content) = sourceFiles[i];
            foreach (var expected in testCase.SourceFiles[i].ExpectedDiagnostics)
            {
                test.TestState.ExpectedDiagnostics.Add(BuildLocatedDiagnostic(expected, testCase, path, content));
            }
        }

        // Diagnostics located within a specific generated file's expected content.
        for (var i = 0; i < testCase.GeneratedFiles.Count; i++)
        {
            var (path, content) = generatedFiles[i];
            foreach (var expected in testCase.GeneratedFiles[i].ExpectedDiagnostics)
            {
                test.TestState.ExpectedDiagnostics.Add(BuildLocatedDiagnostic(expected, testCase, path, content));
            }
        }
    }

    private DiagnosticResult BuildLocationlessDiagnostic(TestExpectedDiagnostic expected, ITestCase testCase)
    {
        if (!string.IsNullOrEmpty(expected.Snippet) || expected.Line.HasValue || expected.Column.HasValue)
        {
            throw new InvalidOperationException(
                $"Expected diagnostic '{expected.Id}' declared at the top level is locationless; declare it on a source or generated file to give it a Snippet or Line/Column."
            );
        }

        var diagnostic = new DiagnosticResult(expected.Id, expected.Severity).WithNoLocation();
        return ApplyMessage(diagnostic, expected, testCase);
    }

    private DiagnosticResult BuildLocatedDiagnostic(
        TestExpectedDiagnostic expected,
        ITestCase testCase,
        string path,
        SourceText content
    )
    {
        var (line, column) = ResolveLocation(expected, path, content, testCase);

        // WithLocation asserts the start position only (the framework ignores the span length), matching the
        // "point at where the diagnostic starts" model the test case describes.
        var diagnostic = new DiagnosticResult(expected.Id, expected.Severity).WithLocation(path, line, column);
        return ApplyMessage(diagnostic, expected, testCase);
    }

    private DiagnosticResult ApplyMessage(
        DiagnosticResult diagnostic,
        TestExpectedDiagnostic expected,
        ITestCase testCase
    )
    {
        return expected.Message == null
            ? diagnostic
            : diagnostic.WithMessage(ExpandVariables(testCase, expected.Message));
    }

    private (int line, int column) ResolveLocation(
        TestExpectedDiagnostic expected,
        string path,
        SourceText content,
        ITestCase testCase
    )
    {
        var hasSnippet = !string.IsNullOrEmpty(expected.Snippet);

        if (hasSnippet && (expected.Line.HasValue || expected.Column.HasValue))
        {
            throw new InvalidOperationException(
                $"Expected diagnostic '{expected.Id}' in '{path}' specifies both a Snippet and an explicit Line/Column; use one or the other."
            );
        }

        if (!hasSnippet)
        {
            if (!expected.Line.HasValue || !expected.Column.HasValue)
            {
                throw new InvalidOperationException(
                    $"Expected diagnostic '{expected.Id}' in '{path}' must specify either a Snippet or both Line and Column."
                );
            }

            return (expected.Line.Value, expected.Column.Value);
        }

        var snippet = ExpandVariables(testCase, expected.Snippet!);
        var text = content.ToString();

        var index = text.IndexOf(snippet, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException(
                $"Expected diagnostic '{expected.Id}' could not locate the snippet '{snippet}' in '{path}'."
            );
        }

        if (text.IndexOf(snippet, index + 1, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(
                $"Expected diagnostic '{expected.Id}' located the snippet '{snippet}' more than once in '{path}'; use a more specific snippet or an explicit Line/Column."
            );
        }

        var position = content.Lines.GetLinePosition(index);
        return (position.Line + 1, position.Character + 1);
    }

    private string ExpandVariables(ITestCase testCase, string text)
    {
        // Avoid allocating a new copy just to add this special case
        text = text.Replace("$(TestName)", testCase.Name);

        foreach (var property in testCase.Properties)
        {
            text = text.Replace($"$({property.Key})", property.Value?.ToString() ?? string.Empty);
        }
        foreach (var property in this.properties)
        {
            text = text.Replace($"$({property.Key})", property.Value?.ToString() ?? string.Empty);
        }

        return text;
    }

    private RoslynTestBuilder<TVerifier> AddGenerator(Generator generator)
    {
        var generatorType = generator.SourceGeneratorType;
        if (this.generators.Any(existing => existing.SourceGeneratorType == generatorType))
        {
            throw new ArgumentException($"The generator {generatorType} was already added.");
        }

        var builder = Clone();
        builder.generators = builder.generators.Add(generator);
        builder.properties = builder.properties.SetItem(
            generatorType.Name,
            (string)GeneratedSource.GetDirectory(generatorType)
        );
        return builder;
    }

    private void VerifyAnalyzerNotPresent(Type analyzerType)
    {
        if (this.analyzers.Any(analyzer => analyzer.GetType() == analyzerType))
        {
            throw new ArgumentException($"The analyzer {analyzerType} was already added.");
        }
    }

    private static string NormalizeSeparators(string fileName)
    {
        return new TestPath(fileName);
    }

    private static string GetTestFilePath(string? directory, string fileName)
    {
        var fullPath =
            directory == null ? TestPath.CurrentDirectory / fileName : TestPath.CurrentDirectory / directory / fileName;
        return fullPath;
    }
}
