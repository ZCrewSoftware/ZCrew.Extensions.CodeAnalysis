using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.TestHelpers;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests;

public class AttributeParameterTests
{
    private const string AnnotatedParameterSource = """
        using System;

        public class MyAttribute : Attribute
        {
            public MyAttribute(string? value) { }
        }

        [My("x")]
        public class Target { }
        """;

    [Fact]
    public void Matches_ForAnnotatedParameterType_MatchesTheUnannotatedConstantType()
    {
        // Arrange
        var constant = GetFirstConstructorArgument(AnnotatedParameterSource);
        var parameter = new TestParameter("string");

        // Act
        var matches = parameter.Matches(constant);

        // Assert
        Assert.True(matches);
    }

    [Fact]
    public void Matches_ForDifferentType_DoesNotMatch()
    {
        // Arrange
        var constant = GetFirstConstructorArgument(AnnotatedParameterSource);
        var parameter = new TestParameter("int");

        // Act
        var matches = parameter.Matches(constant);

        // Assert
        Assert.False(matches);
    }

    [Fact]
    public void Matches_ForObjectParameterType_MatchesAnyConstantType()
    {
        // Arrange
        var constant = GetFirstConstructorArgument(AnnotatedParameterSource);
        var parameter = new TestParameter("object");

        // Act
        var matches = parameter.Matches(constant);

        // Assert
        Assert.True(matches);
    }

    private static TypedConstant GetFirstConstructorArgument(string source)
    {
        var target = RoslynTestHelper.GetType(source, "Target");
        var attributeData = Assert.Single(target.GetAttributes());
        return attributeData.ConstructorArguments[0];
    }

    private sealed class TestParameter(string type) : AttributeParameter<object>(type, static (_, _) => { });
}
