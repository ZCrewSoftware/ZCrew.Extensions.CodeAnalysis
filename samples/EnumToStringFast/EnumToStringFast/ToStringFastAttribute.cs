namespace EnumToStringFast;

#nullable enable

[Microsoft.CodeAnalysis.Embedded]
internal class ToStringFastAttribute : Attribute
{
    public string? ExtensionClassNamespace { get; set; }

    public string? ExtensionClassName { get; set; }
}

[Microsoft.CodeAnalysis.Embedded]
internal class ToStringFastAttribute<TEnum> : Attribute { }
