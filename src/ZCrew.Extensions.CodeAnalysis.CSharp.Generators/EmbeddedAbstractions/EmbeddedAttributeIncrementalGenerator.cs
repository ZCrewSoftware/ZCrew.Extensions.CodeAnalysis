using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions.Emitters;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions;

[Generator]
internal class EmbeddedAttributeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Emit abstractions such as 'EmbeddedAttribute'
        context.RegisterPostInitializationOutput(EmitAbstractions);

        var embeddedTypes = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Microsoft.CodeAnalysis.EmbeddedAttribute",
                IsTypeDeclaration,
                EmbeddedAttributeInfoFactory.Create
            )
            .Where(static x => x.HasValue)
            .Select(static (x, _) => x!.Value)
            .WithTrackingName("EmbeddedAttribute_Attributes");

        context.RegisterSourceOutput(embeddedTypes, EmbeddedAttributeInfoEmitter.Emit);

        var embeddedSourceTexts = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Microsoft.CodeAnalysis.EmbeddedAttribute",
                IsTypeDeclaration,
                EmbeddedAbstractionSourceTextFactory.Create
            )
            .Where(static x => x.HasValue)
            .Select(static (x, _) => x!.Value)
            .WithTrackingName("EmbeddedAttribute_SourceTexts");

        context.RegisterSourceOutput(embeddedSourceTexts, EmbeddedAbstractionSourceTextEmitter.Emit);
    }

    private static bool IsTypeDeclaration(SyntaxNode node, CancellationToken _)
    {
        return node is BaseTypeDeclarationSyntax;
    }

    private static void EmitAbstractions(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddEmbeddedAttributeDefinition();
    }
}
