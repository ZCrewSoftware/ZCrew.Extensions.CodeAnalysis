using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IntegrationTests;

internal static partial class IsTypeFixture
{
    [IsType<Task<string>>]
    public static partial bool IsTaskOfString(ISymbol? symbol);

    [IsType<Task<List<string>>>]
    public static partial bool IsTaskOfListOfString(ISymbol? symbol);

    [IsType(typeof(Task<>))]
    public static partial bool IsAnyTask(ISymbol? symbol);

    [IsType(typeof(string))]
    public static partial bool IsString(ISymbol? symbol);
}
