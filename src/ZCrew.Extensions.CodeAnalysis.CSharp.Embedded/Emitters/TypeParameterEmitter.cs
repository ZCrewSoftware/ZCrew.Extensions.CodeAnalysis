using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Emitters;

internal static class TypeParameterEmitter
{
    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeInfo info, EmbeddedAttributeNames names)
    {
        if (info.TypeParameters.IsDefaultOrEmpty)
        {
            return;
        }

        var attributeData = names.AttributeData;
        var typeParameter = names.TypeParameter;

        builder
            .Append("file sealed class ")
            .Append(typeParameter)
            .Append(" : global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeTypeParameter<")
            .Append(attributeData)
            .Append('>')
            .AppendLine()
            .AppendBlock(block =>
            {
                // Generate static readonly fields for each unique type parameter
                foreach (var typeParam in info.TypeParameters)
                {
                    block
                        .Append("public static readonly ")
                        .Append(typeParameter)
                        .Append(' ')
                        .Append(typeParam.Name)
                        .Append(" = new((model, symbol) => model.")
                        .Append(typeParam.Name)
                        .Append(" = symbol);")
                        .AppendLine();
                }

                // Generate constructor
                block
                    .Append("public ")
                    .Append(typeParameter)
                    .Append("(global::System.Action<")
                    .Append(attributeData)
                    .Append(", global::Microsoft.CodeAnalysis.ITypeSymbol> valueAction) : base(valueAction) { }");
            })
            .AppendLine();
    }
}
