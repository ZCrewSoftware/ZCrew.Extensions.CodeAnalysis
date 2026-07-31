using Microsoft.CodeAnalysis.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class RoslynTestBuilderFacadeTests
{
    [Fact]
    public async Task Create_ShouldBuildTestUsingDefaultVerifier()
    {
        // Act
        var test = await RoslynTestBuilder
            .Create()
            .WithGenerator<EmptyGenerator>()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RoslynTest<DefaultVerifier>>(test);
    }

    [Fact]
    public async Task CreateDefaultBuilder_ShouldApplyCommonDefaults()
    {
        // Act
        var test = await RoslynTestBuilder
            .CreateDefaultBuilder()
            .WithGenerator<PostInitializationGenerator>()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(CompilerDiagnostics.All, test.CompilerDiagnostics);
        Assert.Contains("CS1591", test.DisabledDiagnostics);
        Assert.Single(test.TestState.GeneratedSources);
    }

    [Fact]
    public async Task IncrementalGeneratorTestBuilder_Create_ShouldRegisterTheGenerator()
    {
        // Act
        var test = await IncrementalGeneratorTestBuilder
            .Create<PostInitializationGenerator>()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert — Create applies no defaults, so the post-initialization source is not expected yet.
        Assert.IsType<RoslynTest<DefaultVerifier>>(test);
        Assert.Empty(test.TestState.GeneratedSources);
    }

    [Fact]
    public async Task IncrementalGeneratorTestBuilder_CreateDefaultBuilder_ShouldRegisterTheGeneratorAndApplyDefaults()
    {
        // Act
        var test = await IncrementalGeneratorTestBuilder
            .CreateDefaultBuilder<PostInitializationGenerator>()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(CompilerDiagnostics.All, test.CompilerDiagnostics);
        Assert.Contains("CS1591", test.DisabledDiagnostics);
        Assert.Single(test.TestState.GeneratedSources);
    }
}
