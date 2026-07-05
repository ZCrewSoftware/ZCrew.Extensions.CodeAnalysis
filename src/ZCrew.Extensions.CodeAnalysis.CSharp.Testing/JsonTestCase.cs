using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Loads a <see cref="TestCase" /> from a JSON descriptor file.
/// </summary>
public class JsonTestCase : ITestCase
{
    /// <inheritdoc />
    public string Name { get; private set; } = null!;

    /// <inheritdoc />
    public string? Directory { get; private set; }

    /// <inheritdoc />
    [JsonInclude]
    public string? Description { get; private set; }

    /// <inheritdoc />
    [JsonInclude]
    public IReadOnlyList<TestSourceFile> SourceFiles { get; private set; } = [];

    /// <inheritdoc />
    [JsonInclude]
    public IReadOnlyList<TestGeneratedFile> GeneratedFiles { get; private set; } = [];

    /// <inheritdoc />
    [JsonInclude]
    public IReadOnlyList<TestExpectedDiagnostic> ExpectedDiagnostics { get; private set; } = [];

    /// <inheritdoc cref="ITestCase.Properties"/>
    [JsonInclude, JsonExtensionData]
    public Dictionary<string, object> Properties { get; private set; } = [];

    /// <inheritdoc cref="ITestCase.Properties"/>
    IReadOnlyDictionary<string, object> ITestCase.Properties => Properties;

    /// <summary>
    ///     Deserializes a <see cref="TestCase" /> from the JSON descriptor at <paramref name="testFilePath" />.
    /// </summary>
    /// <param name="testFilePath">The path to the JSON descriptor file.</param>
    /// <returns>
    ///     The deserialized test case, with <see cref="TestCase.Directory" /> set to the descriptor's directory.
    /// </returns>
    /// <exception cref="FileNotFoundException">Thrown when <paramref name="testFilePath" /> does not exist.</exception>
    /// <exception cref="IOException">Thrown when the descriptor cannot be parsed into a <see cref="TestCase" />.</exception>
    /// <exception cref="JsonException">Thrown when the descriptor is not valid JSON.</exception>
    public static JsonTestCase FromJsonFile(string testFilePath)
    {
        using var fileStream = File.OpenRead(testFilePath);
        var testCase = JsonSerializer.Deserialize<JsonTestCase>(fileStream);
        if (testCase == null)
        {
            throw new IOException($"Failed to parse test case from file: {testFilePath}");
        }

        testCase.Name = Path.GetFileNameWithoutExtension(testFilePath);
        testCase.Directory = Path.GetDirectoryName(testFilePath);
        return testCase;
    }

    /// <summary>
    ///     Asynchronously deserializes a <see cref="TestCase" /> from the JSON descriptor at
    ///     <paramref name="testFilePath" />.
    /// </summary>
    /// <param name="testFilePath">The path to the JSON descriptor file.</param>
    /// <param name="token">A token to cancel the asynchronous read.</param>
    /// <returns>
    ///     The deserialized test case, with <see cref="TestCase.Directory" /> set to the descriptor's directory.
    /// </returns>
    /// <exception cref="FileNotFoundException">Thrown when <paramref name="testFilePath" /> does not exist.</exception>
    /// <exception cref="IOException">Thrown when the descriptor cannot be parsed into a <see cref="TestCase" />.</exception>
    /// <exception cref="JsonException">Thrown when the descriptor is not valid JSON.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="token" /> is canceled.</exception>
    public static async Task<JsonTestCase> FromJsonFileAsync(string testFilePath, CancellationToken token = default)
    {
        using var fileStream = File.OpenRead(testFilePath);
        var testCase = await JsonSerializer.DeserializeAsync<JsonTestCase>(fileStream, cancellationToken: token);
        if (testCase == null)
        {
            throw new IOException($"Failed to parse test case from file: {testFilePath}");
        }

        testCase.Name = Path.GetFileNameWithoutExtension(testFilePath);
        testCase.Directory = Path.GetDirectoryName(testFilePath);
        return testCase;
    }
}
