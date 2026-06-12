namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a single source generator test: the source files to compile and the generated files to verify
///     against.
/// </summary>
public class TestCase
{
    /// <summary>
    ///     An optional human-readable description of the test case. Not used during execution.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The source files compiled as input to the generator.
    /// </summary>
    public TestSourceFile[] SourceFiles { get; set; } = [];

    /// <summary>
    ///     The generated files the test expects the generator to produce.
    /// </summary>
    public TestGeneratedFile[] GeneratedFiles { get; set; } = [];

    /// <summary>
    ///     The directory that <see cref="SourceFiles" /> and <see cref="TestGeneratedFile.SourceFileName" /> are
    ///     resolved relative to. Typically set to the directory of the descriptor that produced this test case (for
    ///     example by <see cref="JsonTestCase.FromJsonFile" />).
    /// </summary>
    public string? Directory { get; set; }
}
