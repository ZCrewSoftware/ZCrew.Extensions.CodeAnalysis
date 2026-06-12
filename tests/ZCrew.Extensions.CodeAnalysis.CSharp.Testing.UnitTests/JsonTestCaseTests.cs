using System.Text.Json;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;
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
