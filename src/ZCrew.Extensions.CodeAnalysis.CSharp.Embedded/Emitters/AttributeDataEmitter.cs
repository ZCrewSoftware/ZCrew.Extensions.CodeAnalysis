using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Emitters;

internal static class AttributeDataEmitter
{
    private const string GeneratorAssemblyName = "ZCrew.Extensions.CodeAnalysis.CSharp";

    public static void Emit(FormattedStringBuilder builder, EmbeddedAttributeInfo info, EmbeddedAttributeNames names)
    {
        var seenNames = new HashSet<string>();
        builder.Append("/// <summary>").AppendLine().Append("///     Attribute data for ");
        AppendAttributeCref(builder, info);
        builder
            .Append('.')
            .AppendLine()
            .Append("/// </summary>")
            .AppendLine()
            .AppendEmbeddedAttribute()
            .AppendGeneratedAttribute(GeneratorAssemblyName)
            .Append("internal partial record ")
            .Append(names.AttributeData)
            .AppendLine()
            .AppendBlock(block =>
            {
                foreach (var typeParameter in info.TypeParameters)
                {
                    if (seenNames.Add(typeParameter.Name))
                    {
                        // Every constructor binds every type argument, so this is always assigned
                        AppendField(
                            block,
                            "global::Microsoft.CodeAnalysis.ITypeSymbol",
                            typeParameter.Name,
                            isAlwaysSet: true,
                            isNonNullableReference: true,
                            isArray: false
                        );
                    }
                }
                foreach (var parameter in info.Parameters)
                {
                    if (seenNames.Add(parameter.Name))
                    {
                        AppendField(
                            block,
                            parameter.PropertyType,
                            parameter.Name,
                            parameter.IsAlwaysSet,
                            parameter.IsNonNullableReference,
                            parameter.IsArray
                        );
                    }
                }
                foreach (var namedProperty in info.NamedProperties)
                {
                    if (seenNames.Add(namedProperty.Name))
                    {
                        AppendField(
                            block,
                            namedProperty.PropertyType,
                            namedProperty.Name,
                            namedProperty.IsAlwaysSet,
                            namedProperty.IsNonNullableReference,
                            namedProperty.IsArray
                        );
                    }
                }
            })
            .AppendLine();
    }

    /// <summary>
    ///     Appends a <c>see</c> reference to the attribute this data was parsed from. The type parameters are named
    ///     here rather than in the generated type name because renaming one is not a breaking change to the attribute,
    ///     so it must not rename the generated type. A <c>cref</c> binds its type arguments by count, so the names only
    ///     have to match the arity.
    /// </summary>
    private static void AppendAttributeCref(FormattedStringBuilder builder, EmbeddedAttributeInfo info)
    {
        builder.Append("<see cref=\"").Append(info.Name);

        if (info.TypeParameters.IsDefaultOrEmpty)
        {
            builder.Append("\"/>");
            return;
        }

        builder
            .Append('{')
            .AppendJoined(info.TypeParameters, ", ", (b, typeParameter) => b.Append(typeParameter.SourceName))
            .Append("}\"/>");
    }

    /// <summary>
    ///     Appends a field, null-forgiving it when the pipeline always assigns it and widening it to a nullable
    ///     reference when it does not. Value types are left alone - they cannot warn, and a nullable one would
    ///     repoint the <c>GetValue</c> overload the emitted setter binds to. Immutable arrays are initialized to an
    ///     empty array.
    /// </summary>
    private static void AppendField(
        FormattedStringBuilder builder,
        string propertyType,
        string name,
        bool isAlwaysSet,
        bool isNonNullableReference,
        bool isArray
    )
    {
        builder.Append("public ").Append(propertyType);

        if (isNonNullableReference && !isAlwaysSet)
        {
            builder.Append('?');
        }

        builder.Append(' ').Append(name);

        if (isArray && !isAlwaysSet)
        {
            // ImmutableArray<T>.Empty
            builder.Append(" = ").Append(propertyType).AppendLine(".Empty;");
            return;
        }

        if (isNonNullableReference && isAlwaysSet)
        {
            builder.AppendLine(" = null!;");
            return;
        }

        builder.AppendLine(';');
    }
}
