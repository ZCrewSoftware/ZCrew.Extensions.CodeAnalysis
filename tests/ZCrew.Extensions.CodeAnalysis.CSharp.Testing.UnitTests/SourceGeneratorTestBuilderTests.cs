using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class SourceGeneratorTestBuilderTests
{
    private static string ExpectedGeneratedPath<TGenerator>(string hintName)
    {
        return Path.Combine(typeof(TGenerator).Assembly.GetName().Name!, typeof(TGenerator).FullName!, hintName);
    }

    // --- Configuration is applied to the built test ---

    [Fact]
    public async Task WithReferenceAssemblies_ShouldSetReferenceAssemblies()
    {
        // Arrange
        var referenceAssemblies = ReferenceAssemblies.Net.Net100;

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithReferenceAssemblies(referenceAssemblies)
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(referenceAssemblies, test.ReferenceAssemblies);
    }

    [Fact]
    public async Task WithAdditionalReferences_ShouldAccumulateAllReferences()
    {
        // Act
        // The framework resolves these eagerly, so they must be real assemblies present in the test output.
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithAdditionalReferences("Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll")
            .WithAdditionalReference("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, test.TestState.AdditionalReferences.Count);
    }

    [Fact]
    public async Task WithCompilerDiagnostics_ShouldSetCompilerDiagnostics()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithCompilerDiagnostics(CompilerDiagnostics.All)
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(CompilerDiagnostics.All, test.CompilerDiagnostics);
    }

    [Fact]
    public async Task WithDisabledDiagnostics_ShouldAccumulateAllDiagnosticIds()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithDisabledDiagnostics("CS0001", "CS0002")
            .WithDisabledDiagnostic("CS0003")
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["CS0001", "CS0002", "CS0003"], test.DisabledDiagnostics);
    }

    [Fact]
    public async Task WithConfiguration_ShouldRunActionAgainstBuiltTest()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithConfiguration(t => t.DisabledDiagnostics.Add("CS9999"))
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("CS9999", test.DisabledDiagnostics);
    }

    // --- Generated source path resolution ---

    [Fact]
    public async Task WithGeneratedSource_ShouldResolveToDefaultGeneratedPath()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithGeneratedSource("Foo.g.cs", SourceText.From("// foo"))
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        var (filename, content) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal(ExpectedGeneratedPath<EmptyGenerator>("Foo.g.cs"), filename);
        Assert.Equal("// foo", content.ToString());
    }

    [Fact]
    public async Task WithGeneratedFilePathResolver_ShouldOverridePathMapping()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithGeneratedFilePathResolver(hintName => "custom/" + hintName)
            .WithGeneratedSource("Foo.g.cs", SourceText.From("// foo"))
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        var (filename, _) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal("custom/Foo.g.cs", filename);
    }

    [Fact]
    public async Task WithGeneratorPostInitializationSources_ShouldAddCapturedSources()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<PostInitializationGenerator>
            .Create()
            .WithGeneratorPostInitializationSources()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        var (filename, content) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal(
            ExpectedGeneratedPath<PostInitializationGenerator>(PostInitializationGenerator.HintName),
            filename
        );
        Assert.Contains(PostInitializationGenerator.Content, content.ToString());
    }

    // --- Immutability (fork-on-write) ---

    [Fact]
    public async Task WithDisabledDiagnostic_ShouldNotMutateOriginalBuilder()
    {
        // Arrange
        var baseBuilder = SourceGeneratorTestBuilder<EmptyGenerator>.Create().WithDisabledDiagnostic("CS0001");

        // Act
        var forkedBuilder = baseBuilder.WithDisabledDiagnostic("CS0002");
        var baseTest = await baseBuilder.BuildAsync(new TestCase(), TestContext.Current.CancellationToken);
        var forkedTest = await forkedBuilder.BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotSame(baseBuilder, forkedBuilder);
        Assert.Equal(["CS0001"], baseTest.DisabledDiagnostics);
        Assert.Equal(["CS0001", "CS0002"], forkedTest.DisabledDiagnostics);
    }

    // --- Content transforms applied when loading files ---

    [Fact]
    public async Task WithVariable_ShouldReplaceTokenInSourceContent()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Source.cs", "class $(Name) { }");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs" }],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithVariable("Name", "Foo")
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (filename, content) = Assert.Single(test.TestState.Sources);
        Assert.Equal("Source.cs", filename);
        Assert.Equal("class Foo { }", content.ToString());
    }

    [Fact]
    public async Task WithContentTransform_ShouldApplyTransformsInRegistrationOrder()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Source.cs", "a");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs" }],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithContentTransform(content => content.Replace("a", "b"))
            .WithContentTransform(content => content.Replace("b", "c"))
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (_, transformed) = Assert.Single(test.TestState.Sources);
        Assert.Equal("c", transformed.ToString());
    }

    // --- BuildAsync file loading and errors ---

    [Fact]
    public async Task BuildAsync_ShouldLoadGeneratedFileAtResolvedPath()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Expected.g.cs", "// expected");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            GeneratedFiles =
            [
                new TestGeneratedFile { SourceFileName = "Expected.g.cs", GeneratedFileName = "Source.g.cs" },
            ],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (filename, content) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal(ExpectedGeneratedPath<EmptyGenerator>("Source.g.cs"), filename);
        Assert.Equal("// expected", content.ToString());
    }

    [Fact]
    public async Task BuildAsync_WithMissingSourceFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        using var temp = new TempDirectory();
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Missing.cs" }],
        };

        // Act
        var act = async () =>
            await SourceGeneratorTestBuilder<EmptyGenerator>
                .Create()
                .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<FileNotFoundException>(act);
    }

    [Fact]
    public async Task BuildAsync_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Source.cs", "// x");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs" }],
        };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await SourceGeneratorTestBuilder<EmptyGenerator>.Create().BuildAsync(testCase, cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(act);
    }
}
