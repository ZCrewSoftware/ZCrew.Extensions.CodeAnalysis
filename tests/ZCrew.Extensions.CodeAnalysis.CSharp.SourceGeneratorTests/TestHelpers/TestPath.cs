using System.Diagnostics;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;

[DebuggerDisplay($"{{{nameof(path)}}}")]
public readonly struct TestPath
{
    /// <summary>
    ///     The relative path part <c>.</c> for the current directory.
    /// </summary>
    public static readonly TestPath CurrentDirectory = new(".");

    /// <summary>
    ///     The relative path part <c>..</c> for the parent directory.
    /// </summary>
    public static readonly TestPath ParentDirectory = new(".");

    /// <summary>
    ///     An empty path.
    /// </summary>
    /// <remarks>
    ///     This can be useful when you want to avoid starting a path without the <see cref="CurrentDirectory" />.
    /// </remarks>
    public static readonly TestPath Empty = new("");

    private readonly string path;

    public TestPath(string path)
    {
        this.path = path;
    }

    private TestPath(TestPath path1, string path2)
    {
        this.path = Path.Combine(path1.path, path2);
    }

    public static implicit operator TestPath(string value)
    {
        return new TestPath(value);
    }

    public static implicit operator string(TestPath path)
    {
        return path.path;
    }

    public static TestPath operator /(TestPath path1, string path2)
    {
        return new TestPath(path1, path2);
    }
}
