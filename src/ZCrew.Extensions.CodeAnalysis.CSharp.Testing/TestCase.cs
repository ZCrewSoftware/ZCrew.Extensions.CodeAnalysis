namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a single source generator test: the source files to compile and the generated files to verify
///     against.
/// </summary>
public class TestCase : ITestCase
{
    /// <summary>
    ///     Initializes a new empty <see cref="TestCase"/>.
    /// </summary>
    public TestCase()
    {
        Name = string.Empty;
    }

    /// <summary>
    ///     Initializes a new empty <see cref="TestCase"/> named <see cref="Name"/>.
    /// </summary>
    /// <param name="name">The test name.</param>
    public TestCase(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     The name of the test. Typically, this is the name of the test metadata file without an extension.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     An optional human-readable description of the test case. Not used during execution.
    /// </summary>
    public string? Description { get; set; }

    /// <inheritdoc cref="ITestCase.SourceFiles"/>
    public List<TestSourceFile> SourceFiles { get; set; } = [];

    /// <inheritdoc cref="ITestCase.SourceFiles"/>
    IReadOnlyList<TestSourceFile> ITestCase.SourceFiles => SourceFiles;

    /// <inheritdoc cref="ITestCase.GeneratedFiles"/>
    public List<TestGeneratedFile> GeneratedFiles { get; set; } = [];

    /// <inheritdoc cref="ITestCase.GeneratedFiles"/>
    IReadOnlyList<TestGeneratedFile> ITestCase.GeneratedFiles => GeneratedFiles;

    /// <inheritdoc cref="ITestCase.ExpectedDiagnostics"/>
    public List<TestExpectedDiagnostic> ExpectedDiagnostics { get; set; } = [];

    /// <inheritdoc cref="ITestCase.ExpectedDiagnostics"/>
    IReadOnlyList<TestExpectedDiagnostic> ITestCase.ExpectedDiagnostics => ExpectedDiagnostics;

    /// <inheritdoc/>
    public string? Directory { get; set; }

    /// <inheritdoc cref="ITestCase.Properties"/>
    public Dictionary<string, object> Properties { get; set; } = [];

    /// <inheritdoc cref="ITestCase.Properties"/>
    IReadOnlyDictionary<string, object> ITestCase.Properties => Properties;
}
