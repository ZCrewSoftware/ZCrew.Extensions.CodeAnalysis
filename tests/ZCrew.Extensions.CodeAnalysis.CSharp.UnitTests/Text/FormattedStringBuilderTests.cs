using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.Text;

public class FormattedStringBuilderTests
{
    [Fact]
    public void Indent_WhenCalled_ShouldNotAppendCharacters()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Indent();

        // Assert
        Assert.Equal(0, formattedStringBuilder.Length);
    }

    [Fact]
    public void Indent_WhenCalled_ShouldNotIndentUntilNewLineIsAppended()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Indent().Append('a').AppendLine().Append('b');

        // Assert
        var expectedString = """
            a
                b
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void Indent_WhenCalledOnce_ShouldIndentNextLine()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Append('a').Indent().AppendLine().Append('b');

        // Assert
        var expectedString = """
            a
                b
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void Indent_WhenCalledMultipleTimes_ShouldIndentNextLineMultipleTimes()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Append('a').Indent().Indent().AppendLine().Append('b');

        // Assert
        var expectedString = """
            a
                    b
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void Unindent_WhenCalled_ShouldNotAppendCharacters()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Indent().Unindent();

        // Assert
        Assert.Equal(0, formattedStringBuilder.Length);
    }

    [Fact]
    public void Unindent_WhenCalledBeforeIndenting_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        var unindent = () => formattedStringBuilder.Unindent();

        // Assert
        Assert.Throws<InvalidOperationException>(unindent);
    }

    [Fact]
    public void Unindent_WhenCalledMoreTimesThanIndent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Indent().Unindent();
        var unindent = () => formattedStringBuilder.Unindent();

        // Assert
        Assert.Throws<InvalidOperationException>(unindent);
    }

    [Fact]
    public void Unindent_WhenCalledAfterIndentingAndBeforeAppendLine_ShouldUndoIndent()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Append('a').Indent().Unindent().AppendLine().Append('b');

        // Assert
        var expectedString = """
            a
            b
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void Unindent_WhenCalled_ShouldNotUnindentUntilNewLineIsAppended()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Indent().Append('a').AppendLine().Unindent().Append('b').AppendLine().Append('c');

        // Assert
        var expectedString = """
            a
                b
            c
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendLineString_WhenIndented_ShouldIndentAppendLine()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Append('a').Indent().AppendLine("b").Append('c');

        // Assert
        var expectedString = """
            ab
                c
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendLineChar_WhenIndented_ShouldIndentAppendLine()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.Append('a').Indent().AppendLine('b').Append('c');

        // Assert
        var expectedString = """
            ab
                c
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }
}
