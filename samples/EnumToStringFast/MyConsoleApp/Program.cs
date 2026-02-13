using MyConsoleApp;

// Non-generic: [ToStringFast] on enum → generates ColorEnumExtensions
Console.WriteLine(ColorEnum.Red.ToStringFast());
Console.WriteLine(ColorEnum.Green.ToStringFast());
Console.WriteLine(ColorEnum.Blue.ToStringFast());

// Generic: [ToStringFast<StatusEnum>] on static partial class → generates into StatusEnumExtensions
Console.WriteLine(StatusEnum.Active.ToStringFast());
Console.WriteLine(StatusEnum.Inactive.ToStringFast());
Console.WriteLine(StatusEnum.Pending.ToStringFast());
