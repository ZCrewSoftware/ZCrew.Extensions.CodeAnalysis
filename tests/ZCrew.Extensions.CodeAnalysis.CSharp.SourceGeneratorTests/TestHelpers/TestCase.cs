using System.Text.Json;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;

public class TestCase
{
    private static readonly JsonSerializerOptions testCaseDeserializationOptions = new();

    public string? Description { get; init; }

    public required TestSourceFile[] SourceFiles { get; init; }

    public required TestGeneratedFile[] GeneratedFiles { get; init; }

    public string? Directory { get; private set; }

    public static TestCase FromJsonFile(string testFilePath)
    {
        using var fileStream = File.OpenRead(testFilePath);
        var testCase = JsonSerializer.Deserialize<TestCase>(fileStream);
        if (testCase == null)
        {
            throw new IOException($"Failed to parse test case from file: {testFilePath}");
        }

        testCase.Directory = Path.GetDirectoryName(testFilePath);
        return testCase;
    }

    public static async Task<TestCase> FromJsonFileAsync(string testFilePath, CancellationToken token = default)
    {
        await using var fileStream = File.OpenRead(testFilePath);
        var testCase = await JsonSerializer.DeserializeAsync<TestCase>(fileStream, cancellationToken: token);
        if (testCase == null)
        {
            throw new IOException($"Failed to parse test case from file: {testFilePath}");
        }

        testCase.Directory = Path.GetDirectoryName(testFilePath);
        return testCase;
    }
}
