using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Emitters;

internal static class ConstructorEmitter
{
    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeInfo info, EmbeddedAttributeNames names)
    {
        var attributeData = names.AttributeData;
        var constructor = names.Constructor;
        var parameter = names.Parameter;
        var typeParameter = names.TypeParameter;
        var namedParameter = names.NamedParameter;
        var hasNamedParams = !info.NamedProperties.IsDefaultOrEmpty;
        var hasTypeParams = !info.TypeParameters.IsDefaultOrEmpty;
        var hasParams = !info.Parameters.IsDefaultOrEmpty;

        builder
            .Append("file sealed class ")
            .Append(constructor)
            .Append(" : global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeConstructor<")
            .Append(attributeData)
            .AppendLine('>')
            .AppendBlock(classBlock =>
            {
                // Generate static instances for each constructor
                for (var i = 0; i < info.Constructors.Count; i++)
                {
                    var attributeConstructor = info.Constructors[i];

                    classBlock
                        .Append("private static readonly ")
                        .Append(constructor)
                        .Append(" constructor")
                        .Append(i)
                        .Append(" = new(");

                    if (hasTypeParams)
                    {
                        classBlock.Append('[');
                        AppendTypeParameterReferences(
                            classBlock,
                            typeParameter,
                            attributeConstructor.TypeParameterNames
                        );
                        classBlock.Append("]");
                    }

                    if (hasParams)
                    {
                        if (hasTypeParams)
                        {
                            classBlock.Append(", ");
                        }
                        classBlock.Append('[');
                        AppendParameterReferences(classBlock, parameter, attributeConstructor.ParameterNames);
                        classBlock.Append(']');
                    }

                    classBlock.Append(");").AppendLine();
                }

                // Generate Constructors array
                classBlock
                    .Append("public static readonly ")
                    .Append(constructor)
                    .Append("[] ")
                    .Append(names.Constructors)
                    .Append(" =");

                if (info.Constructors.IsDefaultOrEmpty)
                {
                    classBlock.Append(" [];");
                }
                else
                {
                    classBlock.Append(" [");
                    for (var i = 0; i < info.Constructors.Count; i++)
                    {
                        classBlock.Append($"constructor{i}");
                        if (i < info.Constructors.Count - 1)
                        {
                            classBlock.Append(", ");
                        }
                    }
                    classBlock.Append("];");
                }
                classBlock
                    .AppendLine()
                    .Append("protected override string AttributeMetadataName => \"")
                    .Append(names.MetadataName)
                    .AppendLine("\";");

                classBlock.Append("public ").Append(constructor).Append('(');
                if (hasTypeParams)
                {
                    classBlock
                        .Append("global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeTypeParameter<")
                        .Append(attributeData)
                        .Append(">[] typeParameters");
                }
                if (hasParams)
                {
                    if (hasTypeParams)
                    {
                        classBlock.Append(", ");
                    }
                    classBlock
                        .Append("global::ZCrew.Extensions.CodeAnalysis.CSharp.AttributeParameter<")
                        .Append(attributeData)
                        .Append(">[] parameters");
                }

                classBlock.Append(") : base(");

                if (hasTypeParams)
                {
                    classBlock.Append("typeParameters, ");
                }
                else
                {
                    classBlock.Append("[], ");
                }

                if (hasParams)
                {
                    classBlock.Append("parameters, ");
                }
                else
                {
                    classBlock.Append("[], ");
                }

                if (hasNamedParams)
                {
                    classBlock.Append(namedParameter).Append('.').Append(names.NamedParameters);
                }
                else
                {
                    classBlock.Append("[]");
                }

                classBlock.Append(") { }");
            })
            .AppendLine();
    }

    private static void AppendParameterReferences(
        FormattedStringBuilder builder,
        string parameter,
        EquatableArray<string> parameters
    )
    {
        builder.AppendJoined(parameters, ", ", (b, p) => b.Append(parameter).Append('.').Append(p));
    }

    private static void AppendTypeParameterReferences(
        FormattedStringBuilder builder,
        string typeParameter,
        EquatableArray<string> typeParameterNames
    )
    {
        builder.AppendJoined(typeParameterNames, ", ", (b, t) => b.Append(typeParameter).Append('.').Append(t));
    }
}
