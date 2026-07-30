using System;

namespace AttributeTests;

[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute(string matches, string equals, string toString) { }

    public new object ToString { get; set; }

    public new string GetHashCode { get; set; }
}
