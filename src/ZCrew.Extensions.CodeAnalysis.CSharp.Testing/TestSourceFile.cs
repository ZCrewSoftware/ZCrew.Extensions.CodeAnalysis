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
}
