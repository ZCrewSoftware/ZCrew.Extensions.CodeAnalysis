using System;
using System.Diagnostics.CodeAnalysis;

namespace AttributeTests;

// The constructor opts out of the compiler's required-member enforcement, so 'Extra' is no longer guaranteed
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    [SetsRequiredMembers]
    public TestAttribute(string name)
    {
        Extra = name;
    }

    public required string Extra { get; set; }
}
