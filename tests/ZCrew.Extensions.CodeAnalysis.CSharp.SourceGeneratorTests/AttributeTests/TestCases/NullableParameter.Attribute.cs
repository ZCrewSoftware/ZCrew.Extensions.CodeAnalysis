#nullable enable
using System;

namespace AttributeTests;

// An annotated parameter is already nullable, so it is never null-forgiven, and its match type drops the '?'
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute(string? name) { }
}
