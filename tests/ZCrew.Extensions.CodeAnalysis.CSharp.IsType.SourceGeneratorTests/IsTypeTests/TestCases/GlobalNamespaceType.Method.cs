#nullable enable

using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp;

namespace IsTypeTests
{
    internal static partial class SymbolChecks
    {
        [IsType<GlobalType>]
        public static partial bool IsGlobalType(ISymbol? symbol);
    }
}

internal sealed class GlobalType { }
