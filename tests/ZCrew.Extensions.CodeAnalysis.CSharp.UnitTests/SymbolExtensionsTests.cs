using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests.TestHelpers;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests;

public class SymbolExtensionsTests
{
    [Fact]
    public void ToPartialTypeDeclaration_ForClass_IncludesClassKeyword()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public class Foo { }", "Foo");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial class Foo", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForInterface_IncludesInterfaceKeyword()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public interface IFoo { }", "IFoo");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial interface IFoo", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForRecord_IncludesRecordKeyword()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public record Foo { }", "Foo");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial record Foo", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForRecordStruct_IncludesRecordStructKeyword()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public record struct Foo { }", "Foo");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial record struct Foo", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForStruct_IncludesStructKeyword()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public struct Foo { }", "Foo");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial struct Foo", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForGenericType_IncludesTypeParameters()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public class Foo<T> { }", "Foo`1");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial class Foo<T>", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForTypeWithMultipleTypeParameters_IncludesAllTypeParameters()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public class Foo<TKey, TValue> { }", "Foo`2");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial class Foo<TKey, TValue>", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForCovariantInterface_IncludesOutVariance()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public interface IFoo<out T> { }", "IFoo`1");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial interface IFoo<out T>", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForContravariantInterface_IncludesInVariance()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public interface IFoo<in T> { }", "IFoo`1");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial interface IFoo<in T>", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForNestedType_OmitsContainingType()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public class Outer { public class Inner { } }", "Outer+Inner");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial class Inner", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForNamespacedType_OmitsNamespace()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("namespace N { public class Foo { } }", "N.Foo");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial class Foo", declaration);
    }

    [Fact]
    public void ToPartialTypeDeclaration_ForKeywordNamedType_EscapesIdentifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetType("public class @class { }", "class");

        // Act
        var declaration = symbol.ToPartialTypeDeclaration();

        // Assert
        Assert.Equal("partial class @class", declaration);
    }

    [Theory]
    [InlineData("public", "public partial void Foo()")]
    [InlineData("private", "private partial void Foo()")]
    [InlineData("internal", "internal partial void Foo()")]
    [InlineData("protected", "protected partial void Foo()")]
    [InlineData("protected internal", "protected internal partial void Foo()")]
    [InlineData("private protected", "private protected partial void Foo()")]
    public void ToPartialMethodDeclaration_ForEachAccessibility_PrependsAccessibility(
        string accessibility,
        string expected
    )
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            $$"""public class C { {{accessibility}} void Foo() { } }""",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal(expected, declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForStaticMethod_IncludesStaticModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public static void Foo() { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public static partial void Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForVirtualMethod_IncludesVirtualModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public virtual void Foo() { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public virtual partial void Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForOverrideMethod_IncludesOverrideModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            """
            public class Base { public virtual void Foo() { } }
            public class Derived : Base { public override void Foo() { } }
            """,
            "Derived",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public override partial void Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForAbstractMethod_IncludesAbstractModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public abstract class C { public abstract void Foo(); }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public abstract partial void Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForSealedOverrideMethod_IncludesOverrideAndSealedModifiers()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            """
            public class Base { public virtual void Foo() { } }
            public class Derived : Base { public sealed override void Foo() { } }
            """,
            "Derived",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public sealed override partial void Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForExternMethod_IncludesExternModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public static extern void Foo(); }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public static extern partial void Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForNonVoidMethod_IncludesReturnType()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("""public class C { public string Foo() => ""; }""", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial string Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForRefReturningMethod_IncludesRefModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public class C { private int field; public ref int Foo() => ref field; }",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial ref int Foo()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForMethodWithParameter_IncludesParameterTypeAndName()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo(int value) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(int value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForRefParameter_IncludesRefModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo(ref int value) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(ref int value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForOutParameter_IncludesOutModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public class C { public void Foo(out int value) { value = 0; } }",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(out int value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForInParameter_IncludesInModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo(in int value) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(in int value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForParamsParameter_IncludesParamsModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public class C { public void Foo(params int[] values) { } }",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(params int[] values)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForExtensionMethod_IncludesThisModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public static class Extensions { public static void Foo(this int value) { } }",
            "Extensions",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public static partial void Foo(this int value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForGenericMethod_IncludesTypeParameters()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo<T>(T value) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo<T>(T value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForConstrainedGenericMethod_IncludesTypeConstraints()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public class C { public void Foo<T>(T value) where T : class { } }",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo<T>(T value) where T : class", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForExplicitInterfaceImplementation_IncludesInterfaceQualifier()
    {
        // Arrange
        var type = RoslynTestHelper.GetType(
            """
            namespace N
            {
                public interface IFoo { void Bar(); }
                public class C : IFoo { void IFoo.Bar() { } }
            }
            """,
            "N.C"
        );
        var symbol = type.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation);

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("private partial void global::N.IFoo.Bar()", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForNamespacedParameterType_IncludesGlobalQualifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public class C { public void Foo(System.Collections.Generic.IEnumerable<int> values) { } }",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal(
            "public partial void Foo(global::System.Collections.Generic.IEnumerable<int> values)",
            declaration
        );
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForFrameworkTypes_UsesSpecialTypeKeywords()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod(
            "public class C { public System.Int32 Foo(System.String value) => 0; }",
            "C",
            "Foo"
        );

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial int Foo(string value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForKeywordNamedParameter_EscapesIdentifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo(int @int) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(int @int)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForNullableReferenceParameter_IncludesNullableModifier()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo(string? value) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(string? value)", declaration);
    }

    [Fact]
    public void ToPartialMethodDeclaration_ForParameterWithDefaultValue_OmitsDefaultValue()
    {
        // Arrange
        var symbol = RoslynTestHelper.GetMethod("public class C { public void Foo(int value = 5) { } }", "C", "Foo");

        // Act
        var declaration = symbol.ToPartialMethodDeclaration();

        // Assert
        Assert.Equal("public partial void Foo(int value)", declaration);
    }

    [Fact]
    public void ToFullyQualifiedName_ForNullableReference_IncludesAnnotation()
    {
        // Arrange
        var symbol = GetParameterType("public class C { public void Foo(string? value) { } }");

        // Act
        var name = symbol.ToFullyQualifiedName();

        // Assert
        Assert.Equal("string?", name);
    }

    [Fact]
    public void ToFullyQualifiedName_WithoutNullableAnnotations_OmitsAnnotation()
    {
        // Arrange
        var symbol = GetParameterType("public class C { public void Foo(string? value) { } }");

        // Act
        var name = symbol.ToFullyQualifiedName(nullableAnnotations: false);

        // Assert
        Assert.Equal("string", name);
    }

    [Fact]
    public void ToFullyQualifiedName_WithoutNullableAnnotations_OmitsNestedAnnotations()
    {
        // Arrange
        var symbol = GetParameterType("public class C { public void Foo(string?[] value) { } }");

        // Act
        var name = symbol.ToFullyQualifiedName(nullableAnnotations: false);

        // Assert
        Assert.Equal("string[]", name);
    }

    [Fact]
    public void ToFullyQualifiedName_WithGlobalUsingsAndWithoutNullableAnnotations_OmitsAnnotation()
    {
        // Arrange
        var symbol = GetParameterType(
            "public class C { public void Foo(System.Collections.Generic.List<string>? value) { } }"
        );

        // Act
        var name = symbol.ToFullyQualifiedName(globalUsings: true, nullableAnnotations: false);

        // Assert
        Assert.Equal("global::System.Collections.Generic.List<string>", name);
    }

    private static ITypeSymbol GetParameterType(string source)
    {
        return RoslynTestHelper.GetMethod(source, "C", "Foo").Parameters[0].Type;
    }
}
