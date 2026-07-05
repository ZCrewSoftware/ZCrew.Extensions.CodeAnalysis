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

    [Fact]
    public void AppendJoined_WithListAndStringSeparator_ShouldJoinAllElements()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        formattedStringBuilder.AppendJoined(items, ", ", (b, m) => b.Append(m));

        // Assert
        Assert.Equal("static, extern, new", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithListAndCharSeparator_ShouldJoinAllElements()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        formattedStringBuilder.AppendJoined(items, ' ', (b, m) => b.Append(m));

        // Assert
        Assert.Equal("static extern new", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithSpanAndStringSeparator_ShouldJoinAllElements()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        ReadOnlySpan<string> items = ["static", "extern", "new"];

        // Act
        formattedStringBuilder.AppendJoined(items, ", ", (b, m) => b.Append(m));

        // Assert
        Assert.Equal("static, extern, new", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithSpanAndCharSeparator_ShouldJoinAllElements()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        ReadOnlySpan<string> items = ["static", "extern", "new"];

        // Act
        formattedStringBuilder.AppendJoined(items, ' ', (b, m) => b.Append(m));

        // Assert
        Assert.Equal("static extern new", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithListRangeAndStringSeparator_ShouldJoinOnlySubrange()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new", "virtual", "abstract", "sealed", "override"];

        // Act
        formattedStringBuilder.AppendJoined(items, 1, 5, " ", (b, m) => b.Append(m));

        // Assert
        Assert.Equal("extern new virtual abstract sealed", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithListRangeAndCharSeparator_ShouldJoinOnlySubrange()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new", "virtual", "abstract", "sealed", "override"];

        // Act
        formattedStringBuilder.AppendJoined(items, 1, 5, ' ', (b, m) => b.Append(m));

        // Assert
        Assert.Equal("extern new virtual abstract sealed", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithSpanRangeAndStringSeparator_ShouldJoinOnlySubrange()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        ReadOnlySpan<string> items = ["static", "extern", "new", "virtual", "abstract", "sealed", "override"];

        // Act
        formattedStringBuilder.AppendJoined(items, 1, 5, " ", (b, m) => b.Append(m));

        // Assert
        Assert.Equal("extern new virtual abstract sealed", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithSpanRangeAndCharSeparator_ShouldJoinOnlySubrange()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        ReadOnlySpan<string> items = ["static", "extern", "new", "virtual", "abstract", "sealed", "override"];

        // Act
        formattedStringBuilder.AppendJoined(items, 1, 5, ' ', (b, m) => b.Append(m));

        // Assert
        Assert.Equal("extern new virtual abstract sealed", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithEmptyList_ShouldAppendNothing()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = [];

        // Act
        formattedStringBuilder.AppendJoined(items, ", ", (b, m) => b.Append(m));

        // Assert
        Assert.Empty(formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithSingleElement_ShouldNotAppendSeparator()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static"];

        // Act
        formattedStringBuilder.AppendJoined(items, ", ", (b, m) => b.Append(m));

        // Assert
        Assert.Equal("static", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithZeroCount_ShouldAppendNothing()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        formattedStringBuilder.AppendJoined(items, 1, 0, ", ", (b, m) => b.Append(m));

        // Assert
        Assert.Empty(formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithRangeEndingAtLastElement_ShouldJoinToEnd()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new", "virtual", "abstract", "sealed", "override"];

        // Act
        formattedStringBuilder.AppendJoined(items, 5, 2, " ", (b, m) => b.Append(m));

        // Assert
        Assert.Equal("sealed override", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WithCustomAppendElement_ShouldRenderEachElementViaCallback()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        formattedStringBuilder.AppendJoined(items, " ", (b, m) => b.Append(m.ToUpperInvariant()));

        // Assert
        Assert.Equal("STATIC EXTERN NEW", formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendJoined_WhenCalled_ShouldReturnSameBuilder()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern"];

        // Act
        var result = formattedStringBuilder.AppendJoined(items, " ", (b, m) => b.Append(m));

        // Assert
        Assert.Same(formattedStringBuilder, result);
    }

    [Fact]
    public void AppendJoined_WithNegativeStartIndex_ShouldThrowForStartIndex()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            formattedStringBuilder.AppendJoined(items, -1, 1, " ", (b, m) => b.Append(m))
        );

        // Assert
        Assert.Equal("startIndex", exception.ParamName);
    }

    [Fact]
    public void AppendJoined_WithNegativeCount_ShouldThrowForCount()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            formattedStringBuilder.AppendJoined(items, 0, -1, " ", (b, m) => b.Append(m))
        );

        // Assert
        Assert.Equal("count", exception.ParamName);
    }

    [Fact]
    public void AppendJoined_WithRangeExceedingCount_ShouldThrowForCount()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();
        List<string> items = ["static", "extern", "new"];

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            formattedStringBuilder.AppendJoined(items, 2, 5, " ", (b, m) => b.Append(m))
        );

        // Assert
        Assert.Equal("count", exception.ParamName);
    }

    [Fact]
    public void AppendJoined_WithSpanAndNegativeStartIndex_ShouldThrowForStartIndex()
    {
        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ReadOnlySpan<string> items = ["static", "extern", "new"];
            new FormattedStringBuilder().AppendJoined(items, -1, 1, " ", (b, m) => b.Append(m));
        });

        // Assert
        Assert.Equal("startIndex", exception.ParamName);
    }

    [Fact]
    public void AppendJoined_WithSpanAndRangeExceedingLength_ShouldThrowForCount()
    {
        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ReadOnlySpan<string> items = ["static", "extern", "new"];
            new FormattedStringBuilder().AppendJoined(items, 2, 5, " ", (b, m) => b.Append(m));
        });

        // Assert
        Assert.Equal("count", exception.ParamName);
    }

    [Fact]
    public void AppendBlock_WithSingleStatement_ShouldWrapBodyInBraces()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder
            .AppendLine("if (value > 5)")
            .AppendBlock(block => block.AppendLine("value = value * 2 + 1;"));

        // Assert
        var expectedString = """
            if (value > 5)
            {
                value = value * 2 + 1;
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WhenBodyEndsWithAppendLine_ShouldNotEmitBlankLineBeforeClosingBrace()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendLine("void M()").AppendBlock(block => block.AppendLine("Body();"));

        // Assert
        var expectedString = """
            void M()
            {
                Body();
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WhenBodyEndsWithoutNewline_ShouldStillCloseOnItsOwnLine()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendLine("void M()").AppendBlock(block => block.Append("Body();"));

        // Assert
        var expectedString = """
            void M()
            {
                Body();
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WithMultipleStatements_ShouldIndentEachStatement()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder
            .AppendLine("int Add()")
            .AppendBlock(block => block.AppendLine("var x = 1;").AppendLine("var y = 2;").AppendLine("return x + y;"));

        // Assert
        var expectedString = """
            int Add()
            {
                var x = 1;
                var y = 2;
                return x + y;
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WhenNested_ShouldIndentEachLevel()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder
            .AppendLine("void M()")
            .AppendBlock(block => block.AppendLine("if (x)").AppendBlock(inner => inner.AppendLine("y();")));

        // Assert
        var expectedString = """
            void M()
            {
                if (x)
                {
                    y();
                }
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WithEmptyBody_ShouldEmitBracesOnSeparateLines()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendLine("void M()").AppendBlock(block => { });

        // Assert
        var expectedString = """
            void M()
            {
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WhenBodyLeavesTrailingBlankLine_ShouldTrimBeforeClosingBrace()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder.AppendLine("void M()").AppendBlock(block => block.AppendLine("Body();").AppendLine());

        // Assert
        var expectedString = """
            void M()
            {
                Body();
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WhenChainedAsIfElse_ShouldMatchRegularBlockLayout()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        formattedStringBuilder
            .AppendLine("if (value > 5)")
            .AppendBlock(block => block.AppendLine("value = value * 2 + 1;"))
            .AppendLine()
            .AppendLine("else")
            .AppendBlock(block => block.AppendLine("value += 5;"));

        // Assert
        var expectedString = """
            if (value > 5)
            {
                value = value * 2 + 1;
            }
            else
            {
                value += 5;
            }
            """;
        Assert.Equal(expectedString, formattedStringBuilder.ToString());
    }

    [Fact]
    public void AppendBlock_WhenCalled_ShouldReturnSameBuilder()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        var result = formattedStringBuilder.AppendBlock(block => block.Append("Body();"));

        // Assert
        Assert.Same(formattedStringBuilder, result);
    }

    [Fact]
    public void AppendBlock_WithNullBody_ShouldThrowArgumentNullException()
    {
        // Arrange
        var formattedStringBuilder = new FormattedStringBuilder();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => formattedStringBuilder.AppendBlock(null!));

        // Assert
        Assert.Equal("body", exception.ParamName);
    }
}
