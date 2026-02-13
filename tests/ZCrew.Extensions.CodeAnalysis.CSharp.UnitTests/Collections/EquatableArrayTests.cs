using System.Collections;
using System.Collections.Immutable;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.Collections;

public class EquatableArrayTests
{
    [Fact]
    public void GetHashCode_WithDefaultValue_ShouldBeZero()
    {
        // Arrange
        var equatableArray = default(EquatableArray<string>);

        // Act
        var hashCode = equatableArray.GetHashCode();

        // Assert
        Assert.Equal(0, hashCode);
    }

    [Fact]
    public void Empty_WhenCalled_ShouldReturnEmptyArray()
    {
        // Act
        var equatableArray = EquatableArray<string>.Empty;

        // Assert
        Assert.NotEqual(default, equatableArray);
        Assert.Empty(equatableArray);
        Assert.True(equatableArray.IsEmpty);
    }

    [Fact]
    public void Constructor_WithSingleValue_ShouldReturnSingleValueArray()
    {
        // Act
        var equatableArray = new EquatableArray<string>("a");

        // Assert
        var element = Assert.Single(equatableArray);
        Assert.Equal("a", element);
    }

    [Fact]
    public void Constructor_WithTwoValues_ShouldReturnTwoValueArray()
    {
        // Act
        var equatableArray = new EquatableArray<string>("a", "b");

        // Assert
        Assert.Collection(equatableArray, x => Assert.Equal("a", x), x => Assert.Equal("b", x));
        Assert.Equal(2, equatableArray.Count);
    }

    [Fact]
    public void Constructor_WithEmptyImmutableArray_ShouldReturnEmptyArray()
    {
        // Assert
        var immutableArray = ImmutableArray<string>.Empty;

        // Act
        var equatableArray = new EquatableArray<string>(immutableArray);

        // Assert
        Assert.Empty(equatableArray);
        Assert.True(equatableArray.IsEmpty);
    }

    [Fact]
    public void Constructor_WithImmutableArray_ShouldReturnMatchingArray()
    {
        // Assert
        var immutableArray = ImmutableArray.Create<string>("a", "b", "c");

        // Act
        var equatableArray = new EquatableArray<string>(immutableArray);

        // Assert
        Assert.Collection(
            equatableArray,
            x => Assert.Equal("a", x),
            x => Assert.Equal("b", x),
            x => Assert.Equal("c", x)
        );
        Assert.Equal(3, equatableArray.Count);
    }

    [Fact]
    public void GetIndexer_WithImmutableArray_ShouldReturnMatchingArray()
    {
        // Arrange
        var immutableArray = ImmutableArray.Create<string>("a", "b", "c");
        var equatableArray = new EquatableArray<string>(immutableArray);

        // Act
        var a = equatableArray[0];
        var b = equatableArray[1];
        var c = equatableArray[2];

        // Assert
        Assert.Equal("a", a);
        Assert.Equal("b", b);
        Assert.Equal("c", c);
    }

    [Fact]
    public void Equals_WithObject_ShouldReturnFalse()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>(new string("a"));

        // Act
        var result = equatableArray.Equals(new object());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDefault_ShouldReturnFalse()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>(new string("a"));

        // Act
        var result = equatableArray.Equals(default);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>(new string("a"));

        // Act
        var result = equatableArray.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsOperator_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>(new string("a"));

        // Act
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS8073
        var result = equatableArray == null;
#pragma warning restore CS8073

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotEqualsOperator_WithNull_ShouldReturnTrue()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>(new string("a"));

        // Act
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS8073
        var result = equatableArray != null;
#pragma warning restore CS8073

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithSameEmptyArray_ShouldReturnTrue()
    {
        // Arrange
        var left = EquatableArray<string>.Empty;
        var right = EquatableArray<string>.Empty;

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsOperator_WithSameEmptyArray_ShouldReturnTrue()
    {
        // Arrange
        var left = EquatableArray<string>.Empty;
        var right = EquatableArray<string>.Empty;

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NotEqualsOperator_WithSameEmptyArray_ShouldReturnFalse()
    {
        // Arrange
        var left = EquatableArray<string>.Empty;
        var right = EquatableArray<string>.Empty;

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashCode_WithSameEmptyArray_ShouldReturnTrue()
    {
        // Arrange
        var left = EquatableArray<string>.Empty;
        var right = EquatableArray<string>.Empty;

        // Act
        var result = left.GetHashCode() == right.GetHashCode();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithSameArray_ShouldReturnTrue()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"));
        var right = new EquatableArray<string>(new string("a"));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsOperator_WithSameArray_ShouldReturnTrue()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"));
        var right = new EquatableArray<string>(new string("a"));

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NotEqualsOperator_WithSameArray_ShouldReturnFalse()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"));
        var right = new EquatableArray<string>(new string("a"));

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashCode_WithSameArray_ShouldReturnTrue()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"));
        var right = new EquatableArray<string>(new string("a"));

        // Act
        var result = left.GetHashCode() == right.GetHashCode();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithReorderedArray_ShouldReturnFalse()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"), new string("b"));
        var right = new EquatableArray<string>(new string("b"), new string("a"));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsOperator_WithReorderedArray_ShouldReturnFalse()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"), new string("b"));
        var right = new EquatableArray<string>(new string("b"), new string("a"));

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotEqualsOperator_WithReorderedArray_ShouldReturnTrue()
    {
        // Arrange
        var left = new EquatableArray<string>(new string("a"), new string("b"));
        var right = new EquatableArray<string>(new string("b"), new string("a"));

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AsImmutableArray_WithEmptyArray_ShouldReturnEmptyImmutableArray()
    {
        // Arrange
        var equatableArray = EquatableArray<string>.Empty;

        // Act
        var immutableArray = equatableArray.AsImmutableArray();

        // Assert
        Assert.Empty(immutableArray);
    }

    [Fact]
    public void ImplicitCastToImmutableArray_WithEmptyArray_ShouldReturnEmptyImmutableArray()
    {
        // Arrange
        var equatableArray = EquatableArray<string>.Empty;

        // Act
        ImmutableArray<string> immutableArray = equatableArray;

        // Assert
        Assert.Empty(immutableArray);
    }

    [Fact]
    public void ImplicitCastToEquatable_WithEmptyImmutableArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var immutableArray = ImmutableArray<string>.Empty;

        // Act
        EquatableArray<string> equatableArray = immutableArray;

        // Assert
        Assert.Empty(equatableArray);
    }

    [Fact]
    public void AsImmutableArray_WithArray_ShouldReturnMatchingImmutableArray()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>("a");

        // Act
        var immutableArray = equatableArray.AsImmutableArray();

        // Assert
        var element = Assert.Single(immutableArray);
        Assert.Equal("a", element);
    }

    [Fact]
    public void ImplicitCastToImmutableArray_WithArray_ShouldReturnMatchingImmutableArray()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>("a");

        // Act
        ImmutableArray<string> immutableArray = equatableArray;

        // Assert
        var element = Assert.Single(immutableArray);
        Assert.Equal("a", element);
    }

    [Fact]
    public void ImplicitCastToEquatableArray_WithArray_ShouldReturnMatchingEquatableArray()
    {
        // Arrange
        var immutableArray = ImmutableArray.Create<string>("a");

        // Act
        EquatableArray<string> equatableArray = immutableArray;

        // Assert
        var element = Assert.Single(equatableArray);
        Assert.Equal("a", element);
    }

    [Fact]
    public void AsSpan_WithEmptyArray_ShouldReturnEmptySpan()
    {
        // Arrange
        var equatableArray = EquatableArray<string>.Empty;

        // Act
        var span = equatableArray.AsSpan();

        // Assert
        Assert.Equal(0, span.Length);
        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void AsSpan_WithArray_ShouldReturnMatchingSpan()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>("a");

        // Act
        var span = equatableArray.AsSpan();

        // Assert
        Assert.Equal(1, span.Length);
        Assert.False(span.IsEmpty);
        Assert.Equal("a", span[0]);
    }

    [Fact]
    public void AsArray_WithEmptyArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var equatableArray = EquatableArray<string>.Empty;

        // Act
        var array = equatableArray.ToArray();

        // Assert
        Assert.Empty(array);
    }

    [Fact]
    public void AsArray_WithArray_ShouldReturnMatchingArray()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>("a");

        // Act
        var array = equatableArray.ToArray();

        // Assert
        var element = Assert.Single(array);
        Assert.Equal("a", element);
    }

    [Fact]
    public void GetEnumeratorGeneric_WithEmptyArray_ShouldReturnEmptyEnumerator()
    {
        // Arrange
        var equatableArray = EquatableArray<string>.Empty;

        // Act
        var enumerator = equatableArray.GetEnumerator();

        // Assert
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void GetEnumeratorGeneric_WithArray_ShouldReturnMatchingSpan()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>("a");

        // Act
        var enumerator = equatableArray.GetEnumerator();

        // Assert
        Assert.True(enumerator.MoveNext());
        Assert.Equal("a", enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void GetEnumerator_WithEmptyArray_ShouldReturnEmptyEnumerator()
    {
        // Arrange
        var equatableArray = EquatableArray<string>.Empty;

        // Act
        var enumerable = equatableArray as IEnumerable;
        var enumerator = enumerable.GetEnumerator();
        using var _ = enumerator as IDisposable;

        // Assert
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void GetEnumerator_WithArray_ShouldReturnMatchingSpan()
    {
        // Arrange
        var equatableArray = new EquatableArray<string>("a");

        // Act
        var enumerable = equatableArray as IEnumerable;
        var enumerator = enumerable.GetEnumerator();
        using var _ = enumerator as IDisposable;

        // Assert
        Assert.True(enumerator.MoveNext());
        Assert.Equal("a", enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }
}
