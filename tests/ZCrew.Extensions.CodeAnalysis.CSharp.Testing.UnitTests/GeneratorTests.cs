using ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class GeneratorTests
{
    [Fact]
    public void ForIncrementalGenerator_ShouldUseTheIncrementalGeneratorType()
    {
        // Act
        var generator = Generator.ForIncrementalGenerator<PostInitializationGenerator>();

        // Assert
        Assert.Equal(typeof(PostInitializationGenerator), generator.SourceGeneratorType);
    }

    [Fact]
    public void ForSourceGenerator_ShouldUseTheSourceGeneratorType()
    {
        // Act
        var generator = Generator.ForSourceGenerator<LegacySourceGenerator>();

        // Assert
        Assert.Equal(typeof(LegacySourceGenerator), generator.SourceGeneratorType);
    }

    [Fact]
    public void GetPostInitializationGeneratedSources_ForPostInitializationGenerator_ShouldReturnRegisteredSource()
    {
        // Act
        var sources = Generator
            .ForIncrementalGenerator<PostInitializationGenerator>()
            .GetPostInitializationGeneratedSources();

        // Assert
        var source = Assert.Single(sources);
        Assert.Equal(
            Path.Combine(
                typeof(PostInitializationGenerator).Assembly.GetName().Name!,
                typeof(PostInitializationGenerator).FullName!,
                PostInitializationGenerator.HintName
            ),
            source.FileName
        );
        Assert.Contains(PostInitializationGenerator.Content, source.Content.ToString());
    }

    [Fact]
    public void GetPostInitializationGeneratedSources_ForSourceGenerator_ShouldReturnRegisteredSource()
    {
        // Act
        var sources = Generator.ForSourceGenerator<LegacySourceGenerator>().GetPostInitializationGeneratedSources();

        // Assert
        var source = Assert.Single(sources);
        Assert.Equal(
            Path.Combine(
                typeof(LegacySourceGenerator).Assembly.GetName().Name!,
                typeof(LegacySourceGenerator).FullName!,
                LegacySourceGenerator.HintName
            ),
            source.FileName
        );
        Assert.Contains(LegacySourceGenerator.Content, source.Content.ToString());
    }

    [Fact]
    public void GetPostInitializationGeneratedSources_ForEmptyGenerator_ShouldReturnEmpty()
    {
        // Act
        var sources = Generator.ForIncrementalGenerator<EmptyGenerator>().GetPostInitializationGeneratedSources();

        // Assert
        Assert.Empty(sources);
    }

    [Fact]
    public void GetPostInitializationGeneratedSources_AccessedMultipleTimes_ShouldCaptureOnce()
    {
        // Arrange
        var generator = Generator.ForIncrementalGenerator<CountingGenerator>();

        // Act
        _ = generator.GetPostInitializationGeneratedSources();
        _ = generator.GetPostInitializationGeneratedSources();
        _ = generator.GetPostInitializationGeneratedSources();

        // Assert
        Assert.Equal(1, CountingGenerator.ConstructionCount);
    }
}
