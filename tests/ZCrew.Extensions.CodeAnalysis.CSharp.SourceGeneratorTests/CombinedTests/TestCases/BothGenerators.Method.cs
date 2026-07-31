#nullable enable

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp;

namespace CombinedTests
{
    internal static partial class SymbolChecks
    {
        [IsType<Task>]
        public static partial bool IsTask(ISymbol? symbol);
    }
}
