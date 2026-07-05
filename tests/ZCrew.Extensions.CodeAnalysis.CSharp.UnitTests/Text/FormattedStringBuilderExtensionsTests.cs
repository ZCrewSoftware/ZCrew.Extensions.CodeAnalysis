using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.Text;

public class FormattedStringBuilderExtensionsTests
{
    [Fact]
    public void AppendNullableEnable_WhenCalled_ShouldAppendCorrectDirective()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendNullableEnable();

        // Assert
        var expectedString = """
            #nullable enable

            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendCommonPragmaDisable_WhenCalled_ShouldAppendPragmaDisable()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendCommonPragmaDisable();

        // Assert
        var expectedPattern = """
            #pragma warning disable [\w\d]+(, [\w\d]+)*

            """;
        Assert.Matches(expectedPattern, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendGeneratedCodeAttribute_WhenCalledWithToolInfo_ShouldAppendCorrectAttribute()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendGeneratedAttribute("tool", "version");

        // Assert
        var expectedString = """
            [global::System.CodeDom.Compiler.GeneratedCode("tool", "version")]

            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendTypeof_WhenCalledWithType_ShouldAppendCorrectCode()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendTypeof("global::System.Collections.Generic.List<string>");

        // Assert
        var expectedString = "typeof(global::System.Collections.Generic.List<string>)";
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AppendFileScopedNamespaceDeclaration_WhenCalledWithNullOrWhitespaceNamespace_ShouldNotAppendNamespace(
        string? @namespace
    )
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendFileScopedNamespaceDeclaration(@namespace);

        // Assert
        Assert.Empty(formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendFileScopedNamespaceDeclaration_WhenCalledWithNamespace_ShouldAppendCorrectDeclaration()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendFileScopedNamespaceDeclaration("ZCrew.Common");

        // Assert
        var expectedString = """
            namespace ZCrew.Common;

            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Theory]
    [InlineData("first\nsecond\nthird")]
    [InlineData("first\r\nsecond\r\nthird")]
    [InlineData("first\nsecond\r\nthird")]
    public void AppendMultiline_WhenCalledWithDifferentLineEndings_ShouldNormalizeToBuilderNewlines(string lines)
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendMultiline(lines);

        // Assert
        var expectedString = """
            first
            second
            third

            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendMultiline_WhenCalledWithSingleLine_ShouldAppendLineWithTerminator()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendMultiline("only");

        // Assert
        var expectedString = """
            only

            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendMultiline_WhenCalled_ShouldReturnSameBuilder()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        var result = formattedStringBuilder.AppendMultiline("line");

        // Assert
        Assert.Same(formattedStringBuilder, result);
    }

    [Fact]
    public void AppendMultiline_WhenBuilderIsIndented_ShouldIndentEachAppendedLine()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        var comment = """
            /// <summary>
            /// Generated method for registering a component.
            /// </summary>
            """;

        // Act
        formattedStringBuilder
            .Append('{')
            .Indent()
            .AppendLine()
            .AppendMultiline(comment)
            .Append("public void Register() { }")
            .Unindent()
            .AppendLine()
            .Append('}');

        // Assert
        var expectedString = """
            {
                /// <summary>
                /// Generated method for registering a component.
                /// </summary>
                public void Register() { }
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }
}
