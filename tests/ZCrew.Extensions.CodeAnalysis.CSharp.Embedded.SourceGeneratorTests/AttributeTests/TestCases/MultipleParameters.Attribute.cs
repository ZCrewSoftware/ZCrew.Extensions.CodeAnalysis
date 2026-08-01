using System;

namespace AttributeTests;

[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute(string name, int value) { }
}
