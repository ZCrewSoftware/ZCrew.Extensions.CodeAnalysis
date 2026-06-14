#nullable enable

using Microsoft.CodeAnalysis;
using ZCrew.Dependable;
using ZCrew.Extensions.CodeAnalysis.CSharp;

namespace IsTypeTests
{
    internal static partial class SymbolChecks
    {
        [IsType<ServiceKeyAttribute>]
        public static partial bool IsServiceKeyAttribute(ISymbol? symbol);
    }
}

namespace ZCrew.Dependable
{
    internal sealed class ServiceKeyAttribute : System.Attribute { }
}
