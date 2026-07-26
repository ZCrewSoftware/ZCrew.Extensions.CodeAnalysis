namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a single generator or analyzer test: the source files to compile and the generated files and
///     diagnostics to verify against.
/// </summary>
public interface ITestCase
{
    /// <summary>
    ///     The name of the test. Typically, this is the name of the test metadata file without an extension.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     An optional human-readable description of the test case. Not used during execution.
    /// </summary>
    string? Description { get; }

    /// <summary>
    ///     The source files compiled as input to the generators and analyzers under test.
    /// </summary>
    IReadOnlyList<TestSourceFile> SourceFiles { get; }

    /// <summary>
    ///     The generated files the test expects the generators to produce.
    /// </summary>
    IReadOnlyList<TestGeneratedFile> GeneratedFiles { get; }

    /// <summary>
    ///     The diagnostics the test expects the compilation, a generator or an analyzer to report with no location
    ///     (<see cref="Microsoft.CodeAnalysis.Location.None" />). Diagnostics tied to a specific file are declared on
    ///     that <see cref="TestSourceFile" /> or <see cref="TestGeneratedFile" /> instead.
    /// </summary>
    IReadOnlyList<TestExpectedDiagnostic> ExpectedDiagnostics { get; }

    /// <summary>
    ///     The directory that input and output source files are resolved relative to. Typically set to the directory of
    ///     the descriptor that produced this test case (for example by
    ///     <see cref="JsonTestCase.FromJsonFile" />).
    /// </summary>
    string? Directory { get; }

    /// <summary>
    ///     All other properties from the test file that didn't match an existing property. Each is available as a
    ///     <c>$(name)</c> variable and overrides a variable of the same name set on the builder.
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }
}
