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
        TestPath path = "some/value";

        // Act
        string value = path;

        // Assert
        Assert.Equal("some/value", value);
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
