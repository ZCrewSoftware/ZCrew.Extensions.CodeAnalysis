using System;

namespace AttributeTests;

[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute() { }

    public TestAttribute(string name) { }

    public TestAttribute(string name, int value) { }
}
