using System.Collections.Immutable;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.Collections;

public class ImmutableArrayExtensionsTests
{
    [Fact]
    public void ToEquatableArray_FromEmptyImmutableArray_ReturnsEmptyEquatableArray()
    {
        // Arrange
        var immutableArray = ImmutableArray<string>.Empty;

        // Act
        var equatableArray = immutableArray.ToEquatableArray();

        // Assert
        Assert.Empty(equatableArray);
    }

    [Fact]
    public void ToEquatableArray_FromImmutableArray_ReturnsMatchingEquatableArray()
    {
        // Arrange
        var immutableArray = ImmutableArray.Create("a");

        // Act
        var equatableArray = immutableArray.ToEquatableArray();

        // Assert
        var element = Assert.Single(equatableArray);
        Assert.Equal("a", element);
    }

    [Fact]
    public void ToEquatableArray_FromEmptyImmutableArrayBuilder_ReturnsEmptyEquatableArray()
    {
        // Arrange
        var immutableArrayBuilder = ImmutableArray.CreateBuilder<string>();

        // Act
        var equatableArray = immutableArrayBuilder.ToEquatableArray();

        // Assert
        Assert.Empty(equatableArray);
    }

    [Fact]
    public void ToEquatableArray_FromImmutableArrayBuilder_ReturnsMatchingEquatableArray()
    {
        // Arrange
        var immutableArrayBuilder = ImmutableArray.CreateBuilder<string>();
        immutableArrayBuilder.Add("a");

        // Act
        var equatableArray = immutableArrayBuilder.ToEquatableArray();

        // Assert
        var element = Assert.Single(equatableArray);
        Assert.Equal("a", element);
    }
}
