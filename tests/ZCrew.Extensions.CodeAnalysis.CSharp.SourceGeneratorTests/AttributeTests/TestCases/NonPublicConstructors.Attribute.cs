using System;

namespace AttributeTests;

// Only public constructors are matched; the others contribute neither a constructor nor a parameter field
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public TestAttribute(string name) { }

    internal TestAttribute(long code) { }

    protected TestAttribute(int rank) { }

    private TestAttribute(bool flag) { }
}
