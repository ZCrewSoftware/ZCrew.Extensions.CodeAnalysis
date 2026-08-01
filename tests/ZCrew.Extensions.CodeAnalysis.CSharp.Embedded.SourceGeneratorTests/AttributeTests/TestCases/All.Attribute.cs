using System;

namespace AttributeTests;

[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute<TService, TImplementation> : Attribute
{
    public TestAttribute(string name, int value) { }

    public string Foo { get; init; }
    public string Bar { get; init; }
}
