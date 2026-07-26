using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Extension on <see cref="AnalyzerTest{TVerifier}"/> that allows multiple source generators and multiple
///     diagnostic analyzers.
/// </summary>
/// <typeparam name="TVerifier">
///     The <see cref="IVerifier"/> used to assert results, for example <see cref="DefaultVerifier"/>.
/// </typeparam>
public class RoslynTest<TVerifier> : AnalyzerTest<TVerifier>
    where TVerifier : IVerifier, new()
{
    private static LanguageVersion DefaultLanguageVersion =>
        Enum.TryParse("Default", out LanguageVersion version) ? version : LanguageVersion.CSharp6;

    /// <summary>
    ///     Initialize a new <see cref="RoslynTest{TVerifier}"/> with multiple <paramref name="sourceGenerators"/> and
    ///     <paramref name="diagnosticAnalyzers"/>.
    /// </summary>
    /// <param name="sourceGenerators">The source generators to run.</param>
    /// <param name="diagnosticAnalyzers">The analyzers to run.</param>
    public RoslynTest(IEnumerable<Generator> sourceGenerators, IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers)
    {
        SourceGenerators = [.. sourceGenerators];
        DiagnosticAnalyzers = [.. diagnosticAnalyzers];
    }

    /// <summary>
    ///     The generators this test runs, in registration order.
    /// </summary>
    public IReadOnlyList<Generator> SourceGenerators { get; }

    /// <summary>
    ///     The analyzers this test runs, in registration order.
    /// </summary>
    public IReadOnlyList<DiagnosticAnalyzer> DiagnosticAnalyzers { get; }

    /// <inheritdoc />
    protected override string DefaultFileExt => "cs";

    /// <inheritdoc />
    public override string Language => LanguageNames.CSharp;

    /// <inheritdoc />
    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        return DiagnosticAnalyzers;
    }

    /// <inheritdoc />
    protected override IEnumerable<Type> GetSourceGenerators()
    {
        return SourceGenerators.Select(generator => generator.SourceGeneratorType);
    }

    /// <inheritdoc />
    protected override CompilationOptions CreateCompilationOptions()
    {
        return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
    }

    /// <inheritdoc />
    protected override ParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(DefaultLanguageVersion, DocumentationMode.Diagnose);
    }
}
