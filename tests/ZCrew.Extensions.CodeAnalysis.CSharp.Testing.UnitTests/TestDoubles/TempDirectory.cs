namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

/// <summary>
///     Creates a unique temporary directory for a test and deletes it on dispose.
/// </summary>
internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    public string WriteFile(string fileName, string content)
    {
        var fullPath = Path.Combine(DirectoryPath, fileName);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(DirectoryPath, recursive: true);
        }
        catch (DirectoryNotFoundException)
        {
            // Already gone — nothing to clean up.
        }
    }
}
