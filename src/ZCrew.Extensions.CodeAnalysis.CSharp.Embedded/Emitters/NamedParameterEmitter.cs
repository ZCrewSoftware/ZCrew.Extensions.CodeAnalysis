using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Emitters;

internal static class NamedParameterEmitter
{
    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeInfo info, EmbeddedAttributeNames names)
    {
        if (info.NamedProperties.IsDefaultOrEmpty)
        {
            return;
        }

        var attributeData = names.AttributeData;
        var namedParameter = names.NamedParameter;

        builder
            .Append("file sealed class ")
            .Append(namedParameter)
            .Append(" : global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeNamedParameter<")
            .Append(attributeData)
            .AppendLine('>')
            .AppendBlock(classBlock =>
            {
                // Generate private static readonly fields for each named parameter
                foreach (var namedParam in info.NamedProperties)
                {
                    classBlock
                        .Append("private static readonly ")
                        .Append(namedParameter)
                        .Append(' ')
                        .Append(namedParam.Name)
                        .Append(" = new(\"")
                        // The named argument is keyed by the attribute's property name, not the generated field name
                        .Append(namedParam.SourceName)
                        .Append(
                            "\", (model, _, constant) => global::ZCrew.Extensions.CodeAnalysis.CSharp.TypedConstantExtensions.GetValue(constant, out model."
                        )
                        .Append(namedParam.Name)
                        .Append("));")
                        .AppendLine();
                }

                // Generate NamedParameters array
                classBlock
                    .Append(
                        "internal static readonly global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeNamedParameter<"
                    )
                    .Append(attributeData)
                    .Append(">[] ")
                    .Append(names.NamedParameters)
                    .Append(" = [")
                    .AppendJoined(
                        info.NamedProperties,
                        ", ",
                        (b, property) =>
                        {
                            b.Append(property.Name);
                        }
                    )
                    .Append("];");

                // Generate constructor
                classBlock
                    .AppendLine()
                    .Append("public ")
                    .Append(namedParameter)
                    .Append("(string name, global::System.Action<")
                    .Append(attributeData)
                    .AppendLine(
                        ", string, global::Microsoft.CodeAnalysis.TypedConstant> valueAction) : base(name, valueAction) { }"
                    );
            })
            .AppendLine();
    }
}
