using Microsoft.CodeAnalysis.CSharp;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded;

internal static class SymbolHelpers
{
    /// <summary>
    ///     Escapes a reserved-keyword identifier; emitted expressions re-use parameter names verbatim, so a
    ///     <c>@event</c> parameter must stay escaped in <c>new { ... }</c> and key arguments.
    /// </summary>
    public static string EscapeIdentifier(string name)
    {
        if (name == string.Empty)
        {
            return string.Empty;
        }
        if (name[0] == '@')
        {
            name = name[1..];
        }
        return SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name : "@" + name;
    }
}
