using ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions.Emitters;

internal static class ParameterEmitter
{
    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeInfo info, EmbeddedAttributeNames names)
    {
        if (info.Constructors.IsDefaultOrEmpty)
        {
            return;
        }
        var hasParameters = false;
        foreach (var constructor in info.Constructors)
        {
            if (!constructor.ParameterNames.IsDefaultOrEmpty)
            {
                hasParameters = true;
                break;
            }
        }
        if (!hasParameters)
        {
            return;
        }

        var attributeData = names.AttributeData;
        var parameter = names.Parameter;

        builder
            .Append("file sealed class ")
            .Append(parameter)
            .Append(" : global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeParameter<")
            .Append(attributeData)
            .Append('>')
            .AppendLine()
            .AppendBlock(block =>
            {
                // Generate static readonly fields for each unique parameter
                foreach (var param in info.Parameters)
                {
                    block
                        .Append("public static readonly ")
                        .Append(parameter)
                        .Append(' ')
                        .Append(param.Name)
                        .Append(" = new(\"")
                        .Append(param.Type)
                        .Append(
                            "\", (model, constant) => global::ZCrew.Extensions.CodeAnalysis.CSharp.TypedConstantExtensions.GetValue(constant, out model."
                        )
                        .Append(param.Name)
                        .Append("));")
                        .AppendLine();
                }

                // Generate constructor
                block
                    .Append("public ")
                    .Append(parameter)
                    .Append("(string type, global::System.Action<")
                    .Append(attributeData)
                    .Append(
                        ", global::Microsoft.CodeAnalysis.TypedConstant> valueAction) : base(type, valueAction) { }"
                    );
            })
            .AppendLine();
    }
}
