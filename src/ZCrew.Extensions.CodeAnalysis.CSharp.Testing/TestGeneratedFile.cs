namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a generated file the test expects: the on-disk file holding the expected content and the hint name
///     the generator produces it under.
/// </summary>
public class TestGeneratedFile
{
    /// <summary>
    ///     The file name holding the expected generated content, resolved relative to
    ///     <see cref="TestCase.Directory" />.
    /// </summary>
    public string SourceFileName { get; set; } = string.Empty;

    /// <summary>
    ///     The hint name the generator emits this source under (i.e. the <c>hintName</c> passed to
    ///     <see cref="Microsoft.CodeAnalysis.SourceProductionContext.AddSource(string, Microsoft.CodeAnalysis.Text.SourceText)" />).
    ///     It is mapped to the full generated file path the test verifies against.
    /// </summary>
    public string GeneratedFileName { get; set; } = string.Empty;
}
