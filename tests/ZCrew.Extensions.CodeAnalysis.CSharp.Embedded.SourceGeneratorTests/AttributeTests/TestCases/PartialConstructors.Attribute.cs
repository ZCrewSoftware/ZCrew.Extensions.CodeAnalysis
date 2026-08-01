using System;

namespace AttributeTests;

// 'name' is declared by every constructor, 'extra' by only one
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute(string name) { }

    public TestAttribute(string name, string extra) { }
}
