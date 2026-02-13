using EnumToStringFast;

namespace MyConsoleApp;

[ToStringFast] // Generates MyConsoleApp.ColorEnumExtensions
public enum ColorEnum
{
    Red,
    Green,
    Blue,
}
