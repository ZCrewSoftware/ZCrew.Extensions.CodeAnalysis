using System.Diagnostics;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     A small immutable path helper that composes relative paths via the <c>/</c> operator and converts implicitly
///     to and from <see cref="string" />.
/// </summary>
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
    public static readonly TestPath ParentDirectory = new("..");

    /// <summary>
    ///     An empty path.
    /// </summary>
    /// <remarks>
    ///     This can be useful when you want to avoid starting a path without the <see cref="CurrentDirectory" />.
    /// </remarks>
    public static readonly TestPath Empty = new("");

    private readonly string path;

    /// <summary>
    ///     Creates a <see cref="TestPath" /> wrapping the given <paramref name="path" />.
    /// </summary>
    /// <param name="path">The path value to wrap.</param>
    public TestPath(string path)
    {
        this.path = path;
    }

    private TestPath(TestPath path1, string path2)
    {
        this.path = Path.Combine(path1.path, path2);
    }

    /// <summary>
    ///     Implicitly wraps a <see cref="string" /> as a <see cref="TestPath" />.
    /// </summary>
    /// <param name="value">The path value to wrap.</param>
    public static implicit operator TestPath(string value)
    {
        return new TestPath(value);
    }

    /// <summary>
    ///     Implicitly converts a <see cref="TestPath" /> back to its underlying <see cref="string" /> value.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    public static implicit operator string(TestPath path)
    {
        return path.path;
    }

    /// <summary>
    ///     Combines <paramref name="path1" /> with the <paramref name="path2" /> segment using
    ///     <see cref="System.IO.Path.Combine(string, string)" />.
    /// </summary>
    /// <param name="path1">The base path.</param>
    /// <param name="path2">The segment to append.</param>
    /// <returns>A new <see cref="TestPath" /> representing the combined path.</returns>
    public static TestPath operator /(TestPath path1, string path2)
    {
        return new TestPath(path1, path2);
    }
}
