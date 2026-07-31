using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions.Emitters;

internal static class SyntaxValueProviderExtensionEmitter
{
    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeNames names)
    {
        // Generate ForAttributeWithMetadataName-like method
        builder
            .Append("internal static partial class SyntaxValueProviderExtensions")
            .AppendLine()
            .AppendBlock(classBlock =>
            {
                classBlock
                    .Append("public static global::Microsoft.CodeAnalysis.IncrementalValuesProvider<T> ")
                    .Append(names.SyntaxProviderExtensionMethod)
                    .Append("<T>(this global::Microsoft.CodeAnalysis.SyntaxValueProvider syntaxProvider, ")
                    .Append(
                        "global::System.Func<global::Microsoft.CodeAnalysis.SyntaxNode, global::System.Threading.CancellationToken, bool> predicate, "
                    )
                    .Append("global::System.Func<global::Microsoft.CodeAnalysis.GeneratorAttributeSyntaxContext, ")
                    .Append("global::System.Collections.Immutable.ImmutableArray<")
                    .Append(names.AttributeData)
                    .Append(">, global::System.Threading.CancellationToken, T> transform)")
                    .AppendLine()
                    .AppendBlock(methodBlock =>
                    {
                        methodBlock
                            .Append("var results = syntaxProvider.ForAttributeWithMetadataName(")
                            .Indent()
                            .AppendLine()
                            .Append('"')
                            .Append(names.MetadataName)
                            .AppendLine("\",")
                            .AppendLine("predicate,")
                            .AppendLine("(context, cancellationToken) =>")
                            .AppendBlock(lambdaBlock =>
                            {
                                lambdaBlock
                                    .Append(
                                        "var builder = global::System.Collections.Immutable.ImmutableArray.CreateBuilder<"
                                    )
                                    .Append(names.AttributeData)
                                    .AppendLine(">(context.Attributes.Length);")
                                    .AppendLine("foreach (var attribute in context.Attributes)")
                                    .AppendBlock(forBlock =>
                                    {
                                        forBlock
                                            .AppendLine("cancellationToken.ThrowIfCancellationRequested();")
                                            .Append("if (attribute.")
                                            .Append(names.AttributeExtensionMethod)
                                            .AppendLine("(out var data))")
                                            .AppendBlock(ifBlock =>
                                            {
                                                ifBlock.Append("builder.Add(data);");
                                            });
                                    })
                                    .AppendLine()
                                    // The name matched but no constructor did, so there is nothing to hand the
                                    // transform. Flag it here and drop it below rather than yielding a default.
                                    .AppendLine("if (builder.Count == 0)")
                                    .AppendBlock(emptyBlock =>
                                    {
                                        emptyBlock.Append("return (Value: default(T)!, IsMatch: false);");
                                    })
                                    .AppendLine()
                                    .Append(
                                        "return (Value: transform(context, builder.ToImmutable(), cancellationToken), IsMatch: true);"
                                    );
                            })
                            .Append(");")
                            .Unindent()
                            .AppendLine()
                            .Append(
                                "var matches = global::Microsoft.CodeAnalysis.IncrementalValueProviderExtensions.Where(results, result => result.IsMatch);"
                            )
                            .AppendLine()
                            .Append(
                                "return global::Microsoft.CodeAnalysis.IncrementalValueProviderExtensions.Select(matches, (result, _) => result.Value);"
                            );
                    });
            })
            .AppendLine();
    }
}
