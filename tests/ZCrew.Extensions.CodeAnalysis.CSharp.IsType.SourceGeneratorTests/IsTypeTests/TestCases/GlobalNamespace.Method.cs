#nullable enable

using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp;

internal static partial class SymbolChecks
{
    [IsType(typeof(System.IDisposable))]
    public static partial bool IsDisposable(ISymbol? symbol);
}
