using EnumToStringFast;

namespace MyConsoleApp;

public enum StatusEnum
{
    Active,
    Inactive,
    Pending,
}

[ToStringFast<StatusEnum>]
internal static partial class StatusEnumExtensions { }
