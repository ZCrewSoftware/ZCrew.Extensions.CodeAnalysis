using System.Text.Json;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Loads a <see cref="TestCase" /> from a JSON descriptor file.
/// </summary>
public class JsonTestCase : TestCase
{
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
    public static async Task<TestCase> FromJsonFileAsync(string testFilePath, CancellationToken token = default)
    {
        using var fileStream = File.OpenRead(testFilePath);
        var testCase = await JsonSerializer.DeserializeAsync<TestCase>(fileStream, cancellationToken: token);
        if (testCase == null)
        {
            throw new IOException($"Failed to parse test case from file: {testFilePath}");
        }

        testCase.Directory = Path.GetDirectoryName(testFilePath);
        return testCase;
    }
}
