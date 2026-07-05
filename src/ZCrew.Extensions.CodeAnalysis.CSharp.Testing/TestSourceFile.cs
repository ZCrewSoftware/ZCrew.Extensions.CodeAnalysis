namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a source file compiled as input to the generator under test.
/// </summary>
public class TestSourceFile
{
    /// <summary>
    ///     The file name of the source, resolved relative to <see cref="TestCase.Directory" />.
    /// </summary>
    public string SourceFileName { get; set; } = string.Empty;

    /// <summary>
    ///     The diagnostics the test expects to be reported in this source file, located within it by
    ///     <see cref="TestExpectedDiagnostic.Snippet" /> or by an explicit line and column.
    /// </summary>
    public List<TestExpectedDiagnostic> ExpectedDiagnostics { get; set; } = [];
}
