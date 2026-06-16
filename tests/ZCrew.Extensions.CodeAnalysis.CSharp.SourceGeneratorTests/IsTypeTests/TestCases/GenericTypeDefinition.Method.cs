#nullable enable

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp;

namespace IsTypeTests
{
    internal static partial class SymbolChecks
    {
        [IsType(typeof(Task<>))]
        public static partial bool IsTask(ISymbol? symbol);
    }
}
