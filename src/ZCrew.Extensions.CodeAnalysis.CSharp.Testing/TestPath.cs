using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     A small immutable path helper that composes relative paths via the <c>/</c> operator and converts implicitly
///     to and from <see cref="string" />. Every segment is normalized to the current platform's directory separator,
///     so paths written with <c>/</c> work on Windows too.
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
    ///     Use this to start a relative path that should not be prefixed with the <see cref="CurrentDirectory" />.
    /// </remarks>
    public static readonly TestPath Empty = new("");

    /// <summary>
    ///     Returns the directory of the source file that calls this method, captured at compile time via
    ///     <see cref="CallerFilePathAttribute" />.
    /// </summary>
    /// <remarks>
    ///     This resolves test fixtures relative to the source tree rather than the build output directory, so
    ///     files loaded through the returned path (and anything written back to them) stay in the tracked source
    ///     location. Because the path is baked in when the caller is compiled, it is valid for the local
    ///     build-and-run loop and for CI, which builds and runs from the same checkout.
    /// </remarks>
    /// <param name="callerFilePath">
    ///     Supplied automatically by the compiler; do not pass a value. The absolute path of the calling source file.
    /// </param>
    /// <returns>A <see cref="TestPath" /> for the directory containing the calling source file.</returns>
    public static TestPath ForCaller([CallerFilePath] string callerFilePath = "")
    {
        return new TestPath(Path.GetDirectoryName(callerFilePath)!);
    }

    private readonly string path = ".";

    /// <summary>
    ///     Creates a <see cref="TestPath" /> wrapping the given <paramref name="path" />, normalized to the current
    ///     platform's directory separator.
    /// </summary>
    /// <param name="path">The path value to wrap.</param>
    public TestPath(string path)
    {
        this.path = NormalizeSeparators(path);
    }

    private TestPath(TestPath path1, string path2)
    {
        this.path = Path.Combine(path1.path, NormalizeSeparators(path2));
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
    ///     Combines <paramref name="path1" /> with the normalized <paramref name="path2" /> segment using
    ///     <see cref="System.IO.Path.Combine(string, string)" />.
    /// </summary>
    /// <param name="path1">The base path.</param>
    /// <param name="path2">The segment to append.</param>
    /// <returns>A new <see cref="TestPath" /> representing the combined path.</returns>
    public static TestPath operator /(TestPath path1, string path2)
    {
        return new TestPath(path1, path2);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.path;
    }

    private static string NormalizeSeparators(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    }
}
