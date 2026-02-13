using System;

namespace AttributeTests;

[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public string Foo { get; init; }
    public string Bar { get; init; }
}
