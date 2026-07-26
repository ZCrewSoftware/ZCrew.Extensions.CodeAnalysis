using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests;

public class TestPathTests
{
    [Fact]
    public void CurrentDirectory_ShouldBeDot()
    {
        // Act
        string path = TestPath.CurrentDirectory;

        // Assert
        Assert.Equal(".", path);
    }

    [Fact]
    public void ParentDirectory_ShouldBeDotDot()
    {
        // Act
        string path = TestPath.ParentDirectory;

        // Assert
        Assert.Equal("..", path);
    }

    [Fact]
    public void Empty_ShouldBeEmptyString()
    {
        // Act
        string path = TestPath.Empty;

        // Assert
        Assert.Equal("", path);
    }

    [Fact]
    public void ImplicitConversion_ShouldRoundTripValue()
    {
        // Arrange
        TestPath path = "value";

        // Act
        string value = path;

        // Assert
        Assert.Equal("value", value);
    }

    [Theory]
    [InlineData("some/value")]
    [InlineData("some\\value")]
    public void Constructor_ShouldNormalizeSeparators(string value)
    {
        // Act
        string path = new TestPath(value);

        // Assert
        Assert.Equal(Path.Combine("some", "value"), path);
    }

    [Fact]
    public void DivideOperator_ShouldNormalizeSeparatorsInTheSegment()
    {
        // Act
        string path = TestPath.Empty / "a/b";

        // Assert
        Assert.Equal(Path.Combine("a", "b"), path);
    }

    [Fact]
    public void ToString_ShouldReturnTheUnderlyingValue()
    {
        // Act
        var path = new TestPath("a") / "b";

        // Assert
        Assert.Equal(Path.Combine("a", "b"), path.ToString());
    }

    [Fact]
    public void DivideOperator_ShouldCombineUsingPathCombine()
    {
        // Act
        string path = TestPath.CurrentDirectory / "a" / "b";

        // Assert
        Assert.Equal(Path.Combine(".", "a", "b"), path);
    }

    [Fact]
    public void DivideOperator_FromEmpty_ShouldNotAddLeadingSeparator()
    {
        // Act
        string path = TestPath.Empty / "x";

        // Assert
        Assert.Equal("x", path);
    }
}
