using System;

namespace AttributeTests;

// Only public, settable, non-static, non-indexer properties become named parameters
[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute : Attribute
{
    public static string Shared { get; set; }

    public string this[int index]
    {
        get => string.Empty;
        set { }
    }

    public string ReadOnly => string.Empty;

    public string PrivateSetter { get; private set; }

    public string Included { get; set; }
}
