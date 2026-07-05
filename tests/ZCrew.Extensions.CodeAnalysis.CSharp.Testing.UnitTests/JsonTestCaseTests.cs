using System.Text.Json;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class JsonTestCaseTests
{
    private const string ValidJson = """
        {
            "Description": "an example",
            "SourceFiles": [{ "SourceFileName": "Source.cs" }],
            "GeneratedFiles": [{ "SourceFileName": "Expected.g.cs", "GeneratedFileName": "Source.g.cs" }]
        }
        """;

    private const string DiagnosticsJson = """
        {
            "SourceFiles": [
                {
                    "SourceFileName": "Source.cs",
                    "ExpectedDiagnostics": [
                        { "Id": "CS0246", "Snippet": "CreateService<T>(" }
                    ]
                }
            ],
            "GeneratedFiles": [
                {
                    "SourceFileName": "Expected.g.cs",
                    "GeneratedFileName": "Source.g.cs",
                    "ExpectedDiagnostics": [
                        { "Id": "ZC1001", "Severity": "Warning", "Line": 10, "Column": 5, "Message": "not supported" }
                    ]
                }
            ],
            "ExpectedDiagnostics": [
                { "Id": "CS5001" }
            ]
        }
        """;

    [Fact]
    public void FromJsonFile_ShouldParseExpectedDiagnostics()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", DiagnosticsJson);

        // Act
        var testCase = JsonTestCase.FromJsonFile(file);

        // Assert — snippet diagnostic nested under the source file.
        var snippetDiagnostic = Assert.Single(Assert.Single(testCase.SourceFiles).ExpectedDiagnostics);
        Assert.Equal("CS0246", snippetDiagnostic.Id);
        Assert.Equal("CreateService<T>(", snippetDiagnostic.Snippet);
        // Severity defaults to Error when omitted; location fields stay null in snippet form.
        Assert.Equal(DiagnosticSeverity.Error, snippetDiagnostic.Severity);
        Assert.Null(snippetDiagnostic.Line);
        Assert.Null(snippetDiagnostic.Column);
        Assert.Null(snippetDiagnostic.Message);

        // Explicit line/column diagnostic nested under the generated file.
        var explicitDiagnostic = Assert.Single(Assert.Single(testCase.GeneratedFiles).ExpectedDiagnostics);
        Assert.Equal("ZC1001", explicitDiagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, explicitDiagnostic.Severity);
        Assert.Equal(10, explicitDiagnostic.Line);
        Assert.Equal(5, explicitDiagnostic.Column);
        Assert.Equal("not supported", explicitDiagnostic.Message);

        // Locationless diagnostic at the top level.
        var locationlessDiagnostic = Assert.Single(testCase.ExpectedDiagnostics);
        Assert.Equal("CS5001", locationlessDiagnostic.Id);
        Assert.Null(locationlessDiagnostic.Snippet);
        Assert.Null(locationlessDiagnostic.Line);
    }

    [Fact]
    public void FromJsonFile_WithOmittedExpectedDiagnostics_ShouldDefaultToEmpty()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", ValidJson);

        // Act
        var testCase = JsonTestCase.FromJsonFile(file);

        // Assert — empty at the top level and on each file entry.
        Assert.Empty(testCase.ExpectedDiagnostics);
        Assert.Empty(Assert.Single(testCase.SourceFiles).ExpectedDiagnostics);
        Assert.Empty(Assert.Single(testCase.GeneratedFiles).ExpectedDiagnostics);
    }

    [Fact]
    public void FromJsonFile_ShouldNotLeakExpectedDiagnosticsIntoProperties()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", DiagnosticsJson);

        // Act
        var testCase = JsonTestCase.FromJsonFile(file);

        // Assert — a typed property binds the key, so it must not fall through to the extension-data bag.
        Assert.DoesNotContain("ExpectedDiagnostics", ((ITestCase)testCase).Properties.Keys);
    }

    [Fact]
    public void FromJsonFile_ShouldParseSourceAndGeneratedFiles()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", ValidJson);

        // Act
        var testCase = JsonTestCase.FromJsonFile(file);

        // Assert
        var sourceFile = Assert.Single(testCase.SourceFiles);
        Assert.Equal("Source.cs", sourceFile.SourceFileName);
        var generatedFile = Assert.Single(testCase.GeneratedFiles);
        Assert.Equal("Expected.g.cs", generatedFile.SourceFileName);
        Assert.Equal("Source.g.cs", generatedFile.GeneratedFileName);
    }

    [Fact]
    public void FromJsonFile_ShouldSetDirectoryToDescriptorDirectory()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", ValidJson);

        // Act
        var testCase = JsonTestCase.FromJsonFile(file);

        // Assert
        Assert.Equal(temp.DirectoryPath, testCase.Directory);
    }

    [Fact]
    public async Task FromJsonFileAsync_ShouldParseSourceAndGeneratedFiles()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", ValidJson);

        // Act
        var testCase = await JsonTestCase.FromJsonFileAsync(file, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(testCase.SourceFiles);
        Assert.Single(testCase.GeneratedFiles);
        Assert.Equal(temp.DirectoryPath, testCase.Directory);
    }

    [Fact]
    public void FromJsonFile_WithOmittedArrays_ShouldDefaultToEmpty()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", "{}");

        // Act
        var testCase = JsonTestCase.FromJsonFile(file);

        // Assert
        Assert.Empty(testCase.SourceFiles);
        Assert.Empty(testCase.GeneratedFiles);
    }

    [Fact]
    public void FromJsonFile_WithNullJsonLiteral_ShouldThrowIOException()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", "null");

        // Act
        var act = () => JsonTestCase.FromJsonFile(file);

        // Assert
        Assert.Throws<IOException>(act);
    }

    [Fact]
    public void FromJsonFile_WithMalformedJson_ShouldThrowJsonException()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", "{ not json");

        // Act
        var act = () => JsonTestCase.FromJsonFile(file);

        // Assert
        Assert.Throws<JsonException>(act);
    }

    [Fact]
    public void FromJsonFile_WithMissingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        using var temp = new TempDirectory();
        var missing = Path.Combine(temp.DirectoryPath, "missing.json");

        // Act
        var act = () => JsonTestCase.FromJsonFile(missing);

        // Assert
        Assert.Throws<FileNotFoundException>(act);
    }

    [Fact]
    public async Task FromJsonFileAsync_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var temp = new TempDirectory();
        var file = temp.WriteFile("case.json", ValidJson);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => JsonTestCase.FromJsonFileAsync(file, cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(act);
    }
}
