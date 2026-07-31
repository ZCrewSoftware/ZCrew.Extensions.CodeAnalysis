using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions.Emitters;

internal static class AttributeDataExtensionEmitter
{
    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeNames names)
    {
        // Generate WithAttributeData static method
        builder
            .Append("internal static partial class AttributeDataExtensions")
            .AppendLine()
            .AppendBlock(classBlock =>
            {
                classBlock
                    .Append("public static bool ")
                    .Append(names.AttributeExtensionMethod)
                    .Append("(this global::Microsoft.CodeAnalysis.AttributeData attributeData, ")
                    .Append("[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ")
                    .Append(names.AttributeData)
                    .Append("? data)")
                    .AppendLine()
                    .AppendBlock(methodBlock =>
                    {
                        methodBlock
                            .Append("foreach (var constructor in ")
                            .Append(names.Constructor)
                            .Append('.')
                            .Append(names.Constructors)
                            .AppendLine(")")
                            .AppendBlock(forBlock =>
                            {
                                forBlock
                                    .AppendLine("if (constructor.TryCreateAttributeFor(attributeData, out data))")
                                    .AppendBlock(ifBlock =>
                                    {
                                        ifBlock.AppendLine("return true;");
                                    });
                            })
                            .AppendLine()
                            .AppendLine("data = default;")
                            .Append("return false;");
                    });
            })
            .AppendLine();
    }
}
