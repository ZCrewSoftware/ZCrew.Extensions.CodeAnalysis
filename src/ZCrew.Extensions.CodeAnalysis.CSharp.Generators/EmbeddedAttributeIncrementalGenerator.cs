using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute;
using ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.SourceGenerators;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

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
                EmbeddedTypeInfoFactory.Create
            )
            .Where(static x => x.HasValue)
            .Select(static (x, _) => x!.Value)
            .Collect()
            .Select(EmbeddedAttributeGroupFactory.Create)
            .WithTrackingName("EmbeddedAttribute_Groups");

        context.RegisterSourceOutput(embeddedTypes.SelectMany(static (groups, _) => groups), GenerateSourceFiles);
    }

    private static bool IsTypeDeclaration(SyntaxNode node, CancellationToken _) => node is BaseTypeDeclarationSyntax;

    private static void EmitAbstractions(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddEmbeddedAttributeDefinition();
    }

    private static void GenerateSourceFiles(
        SourceProductionContext context,
        EmbeddedAttribute.Models.EmbeddedAttributeGroup group
    )
    {
        // Always generate SourceText for all [Embedded] types
        SourceTextSourceGenerator.Generate(context, group);

        // Only generate attribute parsing infrastructure for types that inherit System.Attribute
        if (!group.HasAttributes)
        {
            return;
        }

        DataBuilderInterfaceSourceGenerator.Generate(context, group);
        ConstructorSourceGenerator.Generate(context, group);

        if (!group.UniqueParameters.IsDefaultOrEmpty)
        {
            ParameterSourceGenerator.Generate(context, group);
        }

        if (!group.UniqueTypeParameters.IsDefaultOrEmpty)
        {
            TypeParameterSourceGenerator.Generate(context, group);
        }

        if (!group.UniqueNamedParameters.IsDefaultOrEmpty)
        {
            NamedParameterSourceGenerator.Generate(context, group);
        }
    }
}
