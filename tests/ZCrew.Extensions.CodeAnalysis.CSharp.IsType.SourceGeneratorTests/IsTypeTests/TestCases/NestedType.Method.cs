#nullable enable

using Microsoft.CodeAnalysis;
using ZCrew.Dependable;
using ZCrew.Extensions.CodeAnalysis.CSharp;

namespace IsTypeTests
{
    internal static partial class SymbolChecks
    {
        [IsType<Outer.Inner>]
        public static partial bool IsInner(ISymbol? symbol);
    }
}

namespace ZCrew.Dependable
{
    internal static class Outer
    {
        internal sealed class Inner { }
    }
}
