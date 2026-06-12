using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class SourceGeneratorTestBuilderFacadeTests
{
    [Fact]
    public async Task Create_ShouldBuildTestUsingDefaultVerifier()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<EmptyGenerator>
            .Create()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<CSharpSourceGeneratorTest<EmptyGenerator, DefaultVerifier>>(test);
    }

    [Fact]
    public async Task CreateDefaultBuilder_ShouldApplyCommonDefaults()
    {
        // Act
        var test = await SourceGeneratorTestBuilder<PostInitializationGenerator>
            .CreateDefaultBuilder()
            .BuildAsync(new TestCase(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(CompilerDiagnostics.All, test.CompilerDiagnostics);
        Assert.Contains("CS1591", test.DisabledDiagnostics);
        Assert.Single(test.TestState.GeneratedSources);
    }
}
