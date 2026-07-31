using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Emitters;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType;

/// <summary>
///     Emits the <c>IsTypeAttribute</c> definitions and generates a fast Roslyn type-check body for every partial
///     method marked with them.
/// </summary>
[Generator]
internal sealed class IsTypeIncrementalGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Emit abstractions such as 'IsTypeAttribute'
        context.RegisterPostInitializationOutput(EmitAbstractions);

        var genericMethods = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "ZCrew.Extensions.CodeAnalysis.CSharp.IsTypeAttribute`1",
                static (node, _) => node is MethodDeclarationSyntax,
                IsTypeMethodInfoFactory.CreateFromGeneric
            )
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        var typeofMethods = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "ZCrew.Extensions.CodeAnalysis.CSharp.IsTypeAttribute",
                static (node, _) => node is MethodDeclarationSyntax,
                IsTypeMethodInfoFactory.CreateFromTypeof
            )
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        context.RegisterSourceOutput(genericMethods, static (context, info) => IsTypeMethodEmitter.Emit(context, info));
        context.RegisterSourceOutput(typeofMethods, static (context, info) => IsTypeMethodEmitter.Emit(context, info));
    }

    private static void EmitAbstractions(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(IsTypeAttributeSource.HintName, SourceText.From(IsTypeAttributeSource.Source, Encoding.UTF8));
    }
}
