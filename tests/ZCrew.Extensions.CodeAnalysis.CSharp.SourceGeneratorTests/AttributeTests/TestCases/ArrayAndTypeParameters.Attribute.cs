using System;

namespace AttributeTests;

// 'System.Type' maps to ITypeSymbol and follows the reference-type rules; an array maps to the ImmutableArray<T>
// value type and is left alone even though only one constructor declares it
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute(Type serviceType) { }

    public TestAttribute(Type serviceType, Type implementationType, string[] tags) { }
}
