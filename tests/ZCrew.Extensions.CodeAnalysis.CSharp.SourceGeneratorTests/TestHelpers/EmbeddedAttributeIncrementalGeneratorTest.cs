using System.Text;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;

internal class EmbeddedAttributeIncrementalGeneratorTest
    : CSharpSourceGeneratorTest<EmbeddedAttributeIncrementalGenerator, DefaultVerifier>
{
    [Obsolete("#78 - change this to point to the pre-fab ReferenceAssemblies when available for .NET 10")]
    private static readonly ReferenceAssemblies Net10 = new(
        "net10.0",
        new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
        Path.Combine("ref", "net10.0")
    );

    public static async ValueTask<EmbeddedAttributeIncrementalGeneratorTest> ForTestCaseAsync(
        TestCase testCase,
        CancellationToken token = default
    )
    {
        var test = new EmbeddedAttributeIncrementalGeneratorTest
        {
            ReferenceAssemblies = Net10,
            TestState =
            {
                // All tests will inevitably use this project
                AdditionalReferences =
                {
                    "Microsoft.CodeAnalysis.dll",
                    "Microsoft.CodeAnalysis.CSharp.dll",
                    "Microsoft.CodeAnalysis.CSharp.Workspaces.dll",
                    "ZCrew.Extensions.CodeAnalysis.CSharp.dll",
                },

                // All tests will emit these attributes
                GeneratedSources =
                {
                    (
                        GetGeneratedFilePath("Microsoft.CodeAnalysis.EmbeddedAttribute.cs"),
                        EmbeddedAttributeSourceText.SourceText
                    ),
                },
            },
            CompilerDiagnostics = CompilerDiagnostics.All,
            DisabledDiagnostics =
            {
                "CS1591", // Disable the warning on the source files about missing XML comments
                "CS1701", // Disable warning about .NET 9 vs .NET 10 dependencies, TODO: #78 - Remove
            },
        };

        var sourceFileTasks = testCase.SourceFiles.Select(file => LoadSourceFile(file, testCase, token));
        var generatedFileTasks = testCase.GeneratedFiles.Select(file => LoadGeneratedFile(file, testCase, token));

        var sourceFiles = await Task.WhenAll(sourceFileTasks);
        var generatedFiles = await Task.WhenAll(generatedFileTasks);

        test.TestState.Sources.AddRange(sourceFiles);
        test.TestState.GeneratedSources.AddRange(generatedFiles);

        return test;
    }

    private static async Task<(string, SourceText)> LoadSourceFile(
        TestSourceFile testSourceFile,
        TestCase testCase,
        CancellationToken token
    )
    {
        var file = await LoadFile(testCase.Directory, testSourceFile.SourceFileName, token);
        var sourceText = SourceText.From(file, Encoding.UTF8);
        return (testSourceFile.SourceFileName, sourceText);
    }

    private static async Task<(string, SourceText)> LoadGeneratedFile(
        TestGeneratedFile testGeneratedFile,
        TestCase testCase,
        CancellationToken token
    )
    {
        var generatedFileName = GetGeneratedFilePath(testGeneratedFile.GeneratedFileName);
        var file = await LoadFile(testCase.Directory, testGeneratedFile.SourceFileName, token);
        var sourceText = SourceText.From(file, Encoding.UTF8);
        return (generatedFileName, sourceText);
    }

    private static async Task<string> LoadFile(string? directory, string fileName, CancellationToken token)
    {
        var fullFileName = GetTestFilePath(directory, fileName);
        var fileContents = await File.ReadAllTextAsync(fullFileName, token);
        return fileContents;
    }

    private static string GetGeneratedFilePath(string fileName)
    {
        return TestPath.Empty
            / "ZCrew.Extensions.CodeAnalysis.CSharp"
            / "ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttributeIncrementalGenerator"
            / fileName;
    }

    private static string GetTestFilePath(string? directory, string fileName)
    {
        var fullPath =
            directory == null ? TestPath.CurrentDirectory / fileName : TestPath.CurrentDirectory / directory / fileName;
        return fullPath;
    }
}
