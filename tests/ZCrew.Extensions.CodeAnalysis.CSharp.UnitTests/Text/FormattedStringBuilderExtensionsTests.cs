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
}
