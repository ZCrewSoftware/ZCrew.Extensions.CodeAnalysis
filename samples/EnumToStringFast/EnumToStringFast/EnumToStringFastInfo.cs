using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace EnumToStringFast;

internal readonly record struct EnumToStringFastInfo(
    string EnumName,
    string? EnumNamespace,
    string? ExtensionClassNamespace,
    string ExtensionClassName,
    EquatableArray<string> Members
)
{
    public string EnumName { get; } = EnumName;
    public string? EnumNamespace { get; } = EnumNamespace;
    public string? ExtensionClassNamespace { get; } = ExtensionClassNamespace;
    public string ExtensionClassName { get; } = ExtensionClassName;
    public EquatableArray<string> Members { get; } = Members;
}
