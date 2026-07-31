namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a generated file the test expects: the on-disk file holding the expected content and the path the
///     generator produces it under.
/// </summary>
public class TestGeneratedFile
{
    /// <summary>
    ///     The file name holding the expected generated content, resolved relative to
    ///     <see cref="ITestCase.Directory" />.
    /// </summary>
    public string SourceFileName { get; set; } = string.Empty;

    /// <summary>
    ///     The full path the generator emits this source under: the generator's assembly name, its full type name,
    ///     then the <c>hintName</c> passed to
    ///     <see cref="Microsoft.CodeAnalysis.SourceProductionContext.AddSource(string, Microsoft.CodeAnalysis.Text.SourceText)" />.
    ///     Qualify it with the generator's variable rather than writing the prefix out, for example
    ///     <c>$(MyGenerator)/MyNamespace.MyType.g.cs</c>, where <c>MyGenerator</c> is the generator's type name.
    /// </summary>
    public string GeneratedFileName { get; set; } = string.Empty;

    /// <summary>
    ///     The diagnostics the test expects to be reported in this generated file, located within its expected content
    ///     by <see cref="TestExpectedDiagnostic.Snippet" /> or by an explicit line and column.
    /// </summary>
    public List<TestExpectedDiagnostic> ExpectedDiagnostics { get; set; } = [];
}
