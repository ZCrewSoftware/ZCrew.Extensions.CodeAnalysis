using System;

namespace AttributeTests;

// The compiler forces every usage to set 'Name', but never 'Extra'.
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public required string Name { get; set; }

    public string Extra { get; set; }
}
