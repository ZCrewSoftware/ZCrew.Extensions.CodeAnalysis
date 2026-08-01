using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded;

internal static class EmbeddedAbstractionSourceTextFactory
{
    public static EmbeddedAttributeSourceText? Create(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var name = namedTypeSymbol.Name;
        var @namespace = namedTypeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : namedTypeSymbol.ContainingNamespace.ToDisplayString();
        var arity = namedTypeSymbol.Arity;
        var sourceText = context.TargetNode.SyntaxTree.GetText(cancellationToken).ToString();

        return new EmbeddedAttributeSourceText(name, @namespace, arity, sourceText);
    }
}
