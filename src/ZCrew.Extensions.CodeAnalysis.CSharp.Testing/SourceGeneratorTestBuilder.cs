using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
///     <see cref="ITestCase" />.
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
///         var testCase = await JsonTestCase.FromJsonFileAsync("ITestCases/MyCase.json");
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
    private ImmutableDictionary<string, object?> properties = ImmutableDictionary<string, object?>.Empty;
    private bool includePostInitializationSources;
    private bool updateExpectedSources;
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
    ///     Enables in-place updates of the expected generated files. When enabled, <see cref="BuildAsync" /> runs the
    ///     generator and, for each expected generated file that is missing or whose content differs (comparing
    ///     line-ending insensitively), overwrites it on disk with the produced output (normalized to CRLF).
    /// </summary>
    /// <remarks>
    ///     Because it writes to the source tree, this is off by default. Gate it off wherever the workspace must not be
    ///     mutated (e.g. running tests in CI) by passing <paramref name="enabled" /> a condition such as
    ///     <c>Environment.GetEnvironmentVariable("CI") is null</c>. The assertion still runs when writes are
    ///     suppressed, so regressions are still caught.
    /// </remarks>
    /// <param name="enabled">Whether to write updated expected files; defaults to <see langword="true" />.</param>
    /// <returns>A new builder that writes updated expected files when <paramref name="enabled" /> is set.</returns>
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithExpectedSourceUpdates(bool enabled = true)
    {
        var builder = Clone();
        builder.updateExpectedSources = enabled;
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
    public SourceGeneratorTestBuilder<TSourceGenerator, TVerifier> WithVariable(string name, object? value)
    {
        var builder = Clone();
        builder.properties = builder.properties.Add(name, value);
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
    ///     resolved relative to <see cref="ITestCase.Directory" />.
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
        ITestCase testCase,
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
            updateExpectedSources = this.updateExpectedSources,
            configurations = this.configurations,
            generatedFilePathResolver = this.generatedFilePathResolver,
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
        return (ResolveGeneratedPath(generatedFileName), SourceText.From(contents, Encoding.UTF8));
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
            var generatedFileName = ExpandVariables(testCase, generatedFile.GeneratedFileName);

            // The content the test will assert against: the expected file exactly as it exists now (null if it does
            // not exist yet). Never the freshly written content — that is what keeps a mismatch a failure.
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

            generatedFiles[i] = (
                ResolveGeneratedPath(generatedFileName),
                SourceText.From(original ?? string.Empty, Encoding.UTF8)
            );
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

        var generator = GeneratorActivator.CreateSourceGenerator<TSourceGenerator>();
        var runResult = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation, token).GetRunResult();

        var produced = new Dictionary<string, SourceText>(StringComparer.Ordinal);
        foreach (var result in runResult.Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                produced[source.HintName] = source.SourceText;
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

        // UTF-8 without a BOM, CRLF line endings — matching the repository's checked-in generated fixtures.
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

    private void AddExpectedDiagnostics(
        CSharpSourceGeneratorTest<TSourceGenerator, TVerifier> test,
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

        foreach (var property in this.properties)
        {
            text = text.Replace($"$({property.Key})", property.Value?.ToString() ?? string.Empty);
        }
        foreach (var property in testCase.Properties)
        {
            text = text.Replace($"$({property.Key})", property.Value?.ToString() ?? string.Empty);
        }

        return text;
    }

    private static string GetTestFilePath(string? directory, string fileName)
    {
        var fullPath =
            directory == null ? TestPath.CurrentDirectory / fileName : TestPath.CurrentDirectory / directory / fileName;
        return fullPath;
    }
}
