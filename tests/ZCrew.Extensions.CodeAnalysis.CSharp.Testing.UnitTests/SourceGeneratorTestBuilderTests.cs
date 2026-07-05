using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class SourceGeneratorTestBuilderTests
{
    // A source whose newlines are explicit '\n' so line/column assertions do not depend on the file's line endings
    private const string SnippetSource = "class Sample\n{\n    Undefined Value;\n}\n";

    private static async Task<CSharpSourceGeneratorTest<EmptyGenerator, DefaultVerifier>> BuildWithSourceAsync(
        TempDirectory temp,
        string sourceContent,
        TestExpectedDiagnostic expected,
        string testName = "Test"
    )
    {
        temp.WriteFile("Source.cs", sourceContent);
        var testCase = new TestCase(testName)
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs", ExpectedDiagnostics = [expected] }],
        };

        return await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);
    }

    private static string ExpectedGeneratedPath<TGenerator>(string hintName)
    {
        return Path.Combine(typeof(TGenerator).Assembly.GetName().Name!, typeof(TGenerator).FullName!, hintName);
    }

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
    public async Task BuildAsync_ShouldExpandTestNameTokenInSourceContent()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Source.cs", "class $(TestName) { }");
        var testCase = new TestCase("MyTest")
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs" }],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (_, content) = Assert.Single(test.TestState.Sources);
        Assert.Equal("class MyTest { }", content.ToString());
    }

    [Fact]
    public async Task BuildAsync_ShouldExpandTestNameTokenInSourceFileName()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("MyTest.Source.cs", "// content");
        var testCase = new TestCase("MyTest")
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "$(TestName).Source.cs" }],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (filename, content) = Assert.Single(test.TestState.Sources);
        Assert.Equal("MyTest.Source.cs", filename);
        Assert.Equal("// content", content.ToString());
    }

    [Fact]
    public async Task BuildAsync_ShouldExpandTestNameTokenInGeneratedFileNames()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("MyTest.Expected.g.cs", "// expected");
        var testCase = new TestCase("MyTest")
        {
            Directory = temp.DirectoryPath,
            GeneratedFiles =
            [
                new TestGeneratedFile
                {
                    SourceFileName = "$(TestName).Expected.g.cs",
                    GeneratedFileName = "$(TestName).g.cs",
                },
            ],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (filename, content) = Assert.Single(test.TestState.GeneratedSources);
        Assert.Equal(ExpectedGeneratedPath<EmptyGenerator>("MyTest.g.cs"), filename);
        Assert.Equal("// expected", content.ToString());
    }

    [Fact]
    public async Task BuildAsync_ShouldExpandTestCasePropertyTokenInSourceContent()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Source.cs", "class $(Name) { }");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs" }],
            Properties = new Dictionary<string, object> { ["Name"] = "Foo" },
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (_, content) = Assert.Single(test.TestState.Sources);
        Assert.Equal("class Foo { }", content.ToString());
    }

    [Fact]
    public async Task BuildAsync_WhenBuilderAndTestCaseDefineSameVariable_ShouldPreferBuilderVariable()
    {
        // Arrange
        using var temp = new TempDirectory();
        temp.WriteFile("Source.cs", "class $(Name) { }");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles = [new TestSourceFile { SourceFileName = "Source.cs" }],
            Properties = new Dictionary<string, object> { ["Name"] = "FromTestCase" },
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .WithVariable("Name", "FromBuilder")
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var (_, content) = Assert.Single(test.TestState.Sources);
        Assert.Equal("class FromBuilder { }", content.ToString());
    }

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

    [Fact]
    public async Task BuildAsync_WithSnippetDiagnostic_ShouldResolveStartLocation()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic { Id = "CS0246", Snippet = "Undefined" };

        // Act
        var test = await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        Assert.Equal("CS0246", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        var location = Assert.Single(diagnostic.Spans);
        Assert.Equal("Source.cs", location.Span.Path);
        Assert.Equal(new LinePosition(2, 4), location.Span.StartLinePosition);
    }

    [Fact]
    public async Task BuildAsync_WithExplicitLineColumn_ShouldUseGivenPosition()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic
        {
            Id = "CS0246",
            Line = 3,
            Column = 5,
        };

        // Act
        var test = await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        var location = Assert.Single(diagnostic.Spans);
        Assert.Equal("Source.cs", location.Span.Path);
        // Line/Column are 1-based; the framework stores a 0-based LinePosition.
        Assert.Equal(new LinePosition(2, 4), location.Span.StartLinePosition);
    }

    [Fact]
    public async Task BuildAsync_WithSeverity_ShouldSetSeverity()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic
        {
            Id = "ZC1001",
            Severity = DiagnosticSeverity.Warning,
            Snippet = "Undefined",
        };

        // Act
        var test = await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Fact]
    public async Task BuildAsync_WithDiagnosticMessage_ShouldExpandAndSetMessage()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic
        {
            Id = "CS0246",
            Snippet = "Undefined",
            Message = "issue in $(TestName)",
        };

        // Act
        var test = await BuildWithSourceAsync(temp, SnippetSource, expected, testName: "MyTest");

        // Assert
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        Assert.Equal("issue in MyTest", diagnostic.Message);
    }

    [Fact]
    public async Task BuildAsync_WithoutMessage_ShouldNotAssertMessage()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic { Id = "CS0246", Snippet = "Undefined" };

        // Act
        var test = await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        Assert.Null(diagnostic.Message);
    }

    [Fact]
    public async Task BuildAsync_WithDiagnosticsOnMultipleFiles_ShouldScopeSnippetToContainingFile()
    {
        // Arrange — the same snippet appears in both files, so each resolves only within the file it is declared on.
        using var temp = new TempDirectory();
        temp.WriteFile("A.cs", "class A { Undefined X; }");
        temp.WriteFile("B.cs", "class B { Undefined Y; }");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            SourceFiles =
            [
                new TestSourceFile
                {
                    SourceFileName = "A.cs",
                    ExpectedDiagnostics = [new TestExpectedDiagnostic { Id = "CS0246", Snippet = "Undefined" }],
                },
                new TestSourceFile
                {
                    SourceFileName = "B.cs",
                    ExpectedDiagnostics = [new TestExpectedDiagnostic { Id = "CS0103", Snippet = "Undefined" }],
                },
            ],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert — each diagnostic points at its own file.
        Assert.Equal(2, test.TestState.ExpectedDiagnostics.Count);
        var fromA = test.TestState.ExpectedDiagnostics.Single(d => d.Id == "CS0246");
        var fromB = test.TestState.ExpectedDiagnostics.Single(d => d.Id == "CS0103");
        Assert.Equal("A.cs", Assert.Single(fromA.Spans).Span.Path);
        Assert.Equal("B.cs", Assert.Single(fromB.Spans).Span.Path);
    }

    [Fact]
    public async Task BuildAsync_WithGeneratedFileDiagnostic_ShouldResolveInGeneratedContent()
    {
        // Arrange — "Broken" is on line 2 (0-based 1), starting at char 12.
        using var temp = new TempDirectory();
        temp.WriteFile("Expected.g.cs", "// generated\nclass Gen { Broken X; }\n");
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            GeneratedFiles =
            [
                new TestGeneratedFile
                {
                    SourceFileName = "Expected.g.cs",
                    GeneratedFileName = "Gen.g.cs",
                    ExpectedDiagnostics = [new TestExpectedDiagnostic { Id = "CS0246", Snippet = "Broken" }],
                },
            ],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert — the location resolves against the generated file's resolved hint-name path.
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        var location = Assert.Single(diagnostic.Spans);
        Assert.Equal(ExpectedGeneratedPath<EmptyGenerator>("Gen.g.cs"), location.Span.Path);
        Assert.Equal(new LinePosition(1, 12), location.Span.StartLinePosition);
    }

    [Fact]
    public async Task BuildAsync_WithTopLevelDiagnostic_ShouldSetNoLocation()
    {
        // Arrange — a top-level diagnostic is locationless.
        using var temp = new TempDirectory();
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            ExpectedDiagnostics = [new TestExpectedDiagnostic { Id = "CS5001" }],
        };

        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        var diagnostic = Assert.Single(test.TestState.ExpectedDiagnostics);
        Assert.Equal("CS5001", diagnostic.Id);
        Assert.Empty(diagnostic.Spans);
    }

    [Fact]
    public async Task BuildAsync_WithTopLevelDiagnosticHavingLocation_ShouldThrow()
    {
        // Arrange — a top-level diagnostic must not carry a location.
        using var temp = new TempDirectory();
        var testCase = new TestCase
        {
            Directory = temp.DirectoryPath,
            ExpectedDiagnostics = [new TestExpectedDiagnostic { Id = "CS0246", Snippet = "Undefined" }],
        };

        // Act
        var act = async () =>
            await SourceGeneratorTestBuilder<EmptyGenerator>
                .Create()
                .BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task BuildAsync_WithSnippetNotFound_ShouldThrow()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic { Id = "CS0246", Snippet = "DoesNotExist" };

        // Act
        var act = async () => await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("DoesNotExist", exception.Message);
    }

    [Fact]
    public async Task BuildAsync_WithAmbiguousSnippet_ShouldThrow()
    {
        // Arrange — "Value" occurs in both "Value1" and "Value2".
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic { Id = "CS0246", Snippet = "Value" };

        // Act
        var act = async () => await BuildWithSourceAsync(temp, "class A { int Value1; int Value2; }", expected);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("more than once", exception.Message);
    }

    [Fact]
    public async Task BuildAsync_WithSnippetAndExplicitLocation_ShouldThrow()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic
        {
            Id = "CS0246",
            Snippet = "Undefined",
            Line = 3,
            Column = 5,
        };

        // Act
        var act = async () => await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task BuildAsync_WithoutSnippetOrLocation_ShouldThrow()
    {
        // Arrange
        using var temp = new TempDirectory();
        var expected = new TestExpectedDiagnostic { Id = "CS0246" };

        // Act
        var act = async () => await BuildWithSourceAsync(temp, SnippetSource, expected);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
