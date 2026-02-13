using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace EnumToStringFast;

internal readonly record struct ToStringFastAttributeData(
    string? ExtensionClassNamespace,
    string? ExtensionClassName,
    string? EnumNamespace,
    string? EnumName,
    EquatableArray<string> Members
)
{
    public string? ExtensionClassNamespace { get; } = ExtensionClassNamespace;
    public string? ExtensionClassName { get; } = ExtensionClassName;
    public string? EnumNamespace { get; } = EnumNamespace;
    public string? EnumName { get; } = EnumName;
    public EquatableArray<string> Members { get; } = Members;
}
