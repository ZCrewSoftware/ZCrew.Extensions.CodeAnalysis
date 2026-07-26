using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

/// <summary>
///     Covers registering more than one generator and analyzer on a single test.
/// </summary>
public class RoslynTestBuilderMultiTargetTests
{
    private static string ExpectedGeneratedPath<TGenerator>(string hintName)
    {
        return Path.Combine(typeof(TGenerator).Assembly.GetName().Name!, typeof(TGenerator).FullName!, hintName);
    }

    [Fact]
    public async Task BuildAsync_WithMultipleGenerators_ShouldRunAllOfThemInRegistrationOrder()
    {
        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithIncrementalGenerator<PostInitializationGenerator>()
            .WithIncrementalGenerator<SecondPostInitializationGenerator>()
            .WithSourceGenerator<LegacySourceGenerator>()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(
            [
                typeof(PostInitializationGenerator),
                typeof(SecondPostInitializationGenerator),
                typeof(LegacySourceGenerator),
            ],
            test.SourceGenerators.Select(generator => generator.SourceGeneratorType)
        );
    }

    [Fact]
    public async Task BuildAsync_WithMultipleAnalyzers_ShouldRunAllOfThemInRegistrationOrder()
    {
        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithDiagnosticAnalyzer<MarkerAnalyzer>()
            .WithDiagnosticAnalyzer(new SilentAnalyzer())
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(
            [typeof(MarkerAnalyzer), typeof(SilentAnalyzer)],
            test.DiagnosticAnalyzers.Select(analyzer => analyzer.GetType())
        );
    }

    [Fact]
    public async Task WithGeneratorPostInitializationSources_WithMultipleGenerators_ShouldAddEachUnderItsOwnPath()
    {
        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithIncrementalGenerator<PostInitializationGenerator>()
            .WithIncrementalGenerator<SecondPostInitializationGenerator>()
            .WithGeneratorPostInitializationSources()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert — each generator's source lands under its own assembly/type directory.
        Assert.Equal(
            [
                ExpectedGeneratedPath<PostInitializationGenerator>(PostInitializationGenerator.HintName),
                ExpectedGeneratedPath<SecondPostInitializationGenerator>(SecondPostInitializationGenerator.HintName),
            ],
            test.TestState.GeneratedSources.Select(source => source.filename)
        );
    }

    [Fact]
    public async Task BuildAsync_WithMultipleGenerators_ShouldExpandEachGeneratorVariable()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("First.g.cs", "// first");
        temp.WriteFile("Second.g.cs", "// second");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            GeneratedFiles =
            [
                new TestGeneratedFile
                {
                    SourceFileName = "First.g.cs",
                    GeneratedFileName = "$(PostInitializationGenerator)/First.g.cs",
                },
                new TestGeneratedFile
                {
                    SourceFileName = "Second.g.cs",
                    GeneratedFileName = "$(SecondPostInitializationGenerator)/Second.g.cs",
                },
            ],
        };

        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithIncrementalGenerator<PostInitializationGenerator>()
            .WithIncrementalGenerator<SecondPostInitializationGenerator>()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(
            [
                ExpectedGeneratedPath<PostInitializationGenerator>("First.g.cs"),
                ExpectedGeneratedPath<SecondPostInitializationGenerator>("Second.g.cs"),
            ],
            test.TestState.GeneratedSources.Select(source => source.filename)
        );
    }

    [Fact]
    public async Task WithVariable_AfterTheGenerator_ShouldOverrideTheGeneratorVariable()
    {
        // Arrange — the generator's variable and WithVariable share one dictionary, so the later call wins.
        using var temp = new TempDirectory();
        temp.WriteFile("Expected.g.cs", "// expected");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            GeneratedFiles =
            [
                new TestGeneratedFile
                {
                    SourceFileName = "Expected.g.cs",
                    GeneratedFileName = "$(EmptyGenerator)/Source.g.cs",
                },
            ],
        };

        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithGenerator<EmptyGenerator>()
            .WithVariable("EmptyGenerator", "custom")
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (filename, _) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal(Path.Combine("custom", "Source.g.cs"), filename);
    }

    [Fact]
    public async Task TestCaseProperty_ShouldOverrideTheGeneratorVariable()
    {
        // Arrange — the builder is the shared baseline, so an individual test case gets to override it.
        using var temp = new TempDirectory();
        temp.WriteFile("Expected.g.cs", "// expected");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            Properties = new Dictionary<string, object> { ["EmptyGenerator"] = "fromTestCase" },
            GeneratedFiles =
            [
                new TestGeneratedFile
                {
                    SourceFileName = "Expected.g.cs",
                    GeneratedFileName = "$(EmptyGenerator)/Source.g.cs",
                },
            ],
        };

        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithGenerator<EmptyGenerator>()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (filename, _) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal(Path.Combine("fromTestCase", "Source.g.cs"), filename);
    }

    [Fact]
    public async Task BuildAsync_WithOnlyAnalyzers_ShouldBuildTheTest()
    {
        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithDiagnosticAnalyzer<MarkerAnalyzer>()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(test.SourceGenerators);
        Assert.Single(test.DiagnosticAnalyzers);
    }

    [Fact]
    public async Task BuildAsync_WithoutGeneratorsOrAnalyzers_ShouldThrowInvalidOperationException()
    {
        // Act
        var act = async () =>
            await RoslynTestBuilder.Create().BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public void WithIncrementalGenerator_AddedTwice_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = RoslynTestBuilder.Create().WithGenerator<EmptyGenerator>();

        // Act
        var act = () => builder.WithIncrementalGenerator<EmptyGenerator>();

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void WithSourceGenerator_AddedTwice_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = RoslynTestBuilder.Create().WithSourceGenerator<LegacySourceGenerator>();

        // Act
        var act = () => builder.WithSourceGenerator<LegacySourceGenerator>();

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void WithDiagnosticAnalyzer_AddedTwice_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = RoslynTestBuilder.Create().WithDiagnosticAnalyzer<MarkerAnalyzer>();

        // Act
        var act = () => builder.WithDiagnosticAnalyzer(new MarkerAnalyzer());

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void WithDiagnosticAnalyzer_WithNullAnalyzer_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => RoslynTestBuilder.Create().WithDiagnosticAnalyzer(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    /// <summary>
    ///     Builds a test over one marked class, expecting <paramref name="expectedDiagnosticId" /> from the analyzer
    ///     and, when <paramref name="expectPostInitializationSources" /> is set, the generator's emitted source.
    /// </summary>
    private static async Task<RoslynTest<DefaultVerifier>> BuildMarkedSampleTestAsync(
        TempDirectory temp,
        string expectedDiagnosticId,
        bool expectPostInitializationSources = true
    )
    {
        temp.WriteFile("Marked.cs", "class MarkedSample { }\n");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles =
            [
                new TestSourceFile
                {
                    SourceFileName = "Marked.cs",
                    ExpectedDiagnostics =
                    [
                        new TestExpectedDiagnostic
                        {
                            Id = expectedDiagnosticId,
                            Severity = DiagnosticSeverity.Warning,
                            Snippet = "MarkedSample",
                            Message = "Type 'MarkedSample' is marked",
                        },
                    ],
                },
            ],
        };

        var builder = RoslynTestBuilder
            .Create()
            .WithReferenceAssemblies(ReferenceAssemblies.Net.Net100)
            .WithGenerator<PostInitializationGenerator>()
            .WithDiagnosticAnalyzer<MarkerAnalyzer>();

        if (expectPostInitializationSources)
        {
            builder = builder.WithGeneratorPostInitializationSources();
        }

        return await builder.BuildAsync(testCase, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RunAsync_WhenTheGeneratorAndAnalyzerBothMatch_ShouldNotThrow()
    {
        // Arrange
        using var temp = new TempDirectory();
        var test = await BuildMarkedSampleTestAsync(temp, MarkerAnalyzer.Id);

        // Act
        var exception = await Record.ExceptionAsync(() => test.RunAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task RunAsync_WhenTheAnalyzerDiagnosticIsNotReported_ShouldThrow()
    {
        // Arrange — the analyzer never reports this id, so the analyzer half must fail the run.
        using var temp = new TempDirectory();
        var test = await BuildMarkedSampleTestAsync(temp, "ZCTEST999");

        // Act
        var act = () => test.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);
    }

    [Fact]
    public async Task RunAsync_WhenTheGeneratedSourceIsNotExpected_ShouldThrow()
    {
        // Arrange — the generator still emits its post-initialization source, but nothing expects it.
        using var temp = new TempDirectory();
        var test = await BuildMarkedSampleTestAsync(temp, MarkerAnalyzer.Id, expectPostInitializationSources: false);

        // Act
        var act = () => test.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);
    }
}
