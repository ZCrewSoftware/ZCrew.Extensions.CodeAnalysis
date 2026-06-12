using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.CacheTests;

public class CacheTests
{
    private static readonly TestPath testCases = TestPath.CurrentDirectory / "CacheTests" / "TestCases";

    [Theory]
    [InlineData("WithAttribute.json")]
    [InlineData("SingleEnum.json")]
    public async Task EmbeddedAttribute_WithMultipleBuilds_ShouldCacheIncrementalValues(string testDescriptor)
    {
        // Arrange
        var testCaseFile = testCases / testDescriptor;
        var testCase = await JsonTestCase.FromJsonFileAsync(testCaseFile, TestContext.Current.CancellationToken);
        var test = await GeneratorTest.Baseline.BuildAsync(testCase, TestContext.Current.CancellationToken);
        var syntaxTrees = test.TestState.Sources.Select(source => CSharpSyntaxTree.ParseText(source.content)).ToArray();

        var sourceGenerator = new EmbeddedAttributeIncrementalGenerator();
        var driverOptions = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true);
        var driver = CSharpGeneratorDriver.Create([sourceGenerator.AsSourceGenerator()], driverOptions: driverOptions);

        // TODO MWZ #78: change this to point to the pre-fab ReferenceAssemblies when available for .NET 10
        var reference = new ReferenceAssemblies(
            "net10.0",
            new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
            Path.Combine("ref", "net10.0")
        );
        var references = await reference.ResolveAsync(null, CancellationToken.None);

        var cSharpOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation1 = CSharpCompilation.Create(
            Assembly.GetExecutingAssembly().FullName,
            syntaxTrees,
            references,
            cSharpOptions
        );

        var compilation2 = compilation1.Clone();

        // Act
        var cachedDriver = driver.RunGenerators(compilation1, TestContext.Current.CancellationToken);
        var run1 = cachedDriver.GetRunResult();
        var run2 = cachedDriver.RunGenerators(compilation2, TestContext.Current.CancellationToken).GetRunResult();

        // Assert
        Assert.NotNull(run1);
        Assert.NotNull(run2);

        var run2Result = run2.Results.Single();

        var intermediates = run2Result
            .TrackedSteps.Where(step => step.Key.StartsWith("Dependable_"))
            .SelectMany(step => step.Value)
            .SelectMany(step => step.Outputs);

        var outputs = run2Result.TrackedOutputSteps.SelectMany(step => step.Value).SelectMany(step => step.Outputs);

        Assert.All(
            intermediates,
            intermediate =>
            {
                // Some steps depend on the compilation unit which results in 'Unchanged', other steps will be fully cached
                Assert.True(
                    intermediate.Reason is IncrementalStepRunReason.Unchanged or IncrementalStepRunReason.Cached
                );
            }
        );
        Assert.All(
            outputs,
            output =>
            {
                Assert.Equal(IncrementalStepRunReason.Cached, output.Reason);
            }
        );
    }
}
