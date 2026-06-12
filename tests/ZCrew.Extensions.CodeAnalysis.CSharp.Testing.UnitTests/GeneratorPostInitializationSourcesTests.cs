using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class GeneratorPostInitializationSourcesTests
{
    [Fact]
    public void Sources_ForPostInitializationGenerator_ShouldReturnRegisteredSource()
    {
        // Act
        var sources = GeneratorPostInitializationSources<PostInitializationGenerator>.Sources;

        // Assert
        var source = Assert.Single(sources);
        Assert.Equal(PostInitializationGenerator.HintName, source.HintName);
        Assert.Contains(PostInitializationGenerator.Content, source.Content.ToString());
    }

    [Fact]
    public void Sources_ForEmptyGenerator_ShouldReturnEmpty()
    {
        // Act
        var sources = GeneratorPostInitializationSources<EmptyGenerator>.Sources;

        // Assert
        Assert.Empty(sources);
    }

    [Fact]
    public void Sources_ForNonGeneratorType_ShouldThrowInvalidOperationException()
    {
        // Act
        var act = () =>
        {
            _ = GeneratorPostInitializationSources<NotAGenerator>.Sources;
        };

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Sources_AccessedMultipleTimes_ShouldCaptureOnce()
    {
        // Act
        _ = GeneratorPostInitializationSources<CountingGenerator>.Sources;
        _ = GeneratorPostInitializationSources<CountingGenerator>.Sources;
        _ = GeneratorPostInitializationSources<CountingGenerator>.Sources;

        // Assert
        Assert.Equal(1, CountingGenerator.ConstructionCount);
    }
}
