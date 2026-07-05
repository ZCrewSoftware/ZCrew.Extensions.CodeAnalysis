using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.TestHelpers;

/// <summary>
///     Compiles C# source snippets into Roslyn symbols so that <see cref="SymbolExtensions"/> can be exercised
///     against real <see cref="ISymbol"/> instances.
/// </summary>
internal static class RoslynTestHelper
{
    private static readonly MetadataReference[] References = (
        (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!
    )
        .Split(Path.PathSeparator)
        .Where(path => path.Length > 0)
        .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
        .ToArray();

    /// <summary>
    ///     Compiles <paramref name="source"/> with nullable reference types enabled and asserts that it produced no
    ///     compiler errors, so any symbols pulled from it are trustworthy.
    /// </summary>
    /// <param name="source">The C# source to compile.</param>
    /// <returns>The resulting compilation.</returns>
    public static CSharpCompilation Compile(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            "SymbolExtensionsTests",
            [syntaxTree],
            References,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable
            )
        );

        var errors = compilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.Empty(errors);

        return compilation;
    }

    /// <summary>
    ///     Compiles <paramref name="source"/> and returns the named type symbol.
    /// </summary>
    /// <param name="source">The C# source to compile.</param>
    /// <param name="fullyQualifiedMetadataName">The metadata name of the type, e.g. <c>"N.Outer+Inner"</c>.</param>
    /// <returns>The named type symbol.</returns>
    public static INamedTypeSymbol GetType(string source, string fullyQualifiedMetadataName)
    {
        var compilation = Compile(source);
        var symbol = compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
        Assert.NotNull(symbol);
        return symbol;
    }

    /// <summary>
    ///     Compiles <paramref name="source"/> and returns the single method with the given name on the given type.
    /// </summary>
    /// <param name="source">The C# source to compile.</param>
    /// <param name="fullyQualifiedTypeName">The metadata name of the declaring type.</param>
    /// <param name="methodName">The method name.</param>
    /// <returns>The method symbol.</returns>
    public static IMethodSymbol GetMethod(string source, string fullyQualifiedTypeName, string methodName)
    {
        var type = GetType(source, fullyQualifiedTypeName);
        return type.GetMembers(methodName).OfType<IMethodSymbol>().Single();
    }
}
