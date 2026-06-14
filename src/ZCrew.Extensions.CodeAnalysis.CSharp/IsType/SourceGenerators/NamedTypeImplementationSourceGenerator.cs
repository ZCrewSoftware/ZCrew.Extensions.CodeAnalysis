using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGenerators;

/// <summary>
///     Appends the <c>is</c> pattern for a <see cref="NamedTypeImplementationInfo"/> -- matched by walking the type's
///     <c>Name</c>/<c>Arity</c>/<c>ContainingType</c> chain and containing namespaces.
/// </summary>
internal static class NamedTypeImplementationSourceGenerator
{
    /// <summary>
    ///     Appends the variant <c>is</c> pattern into the shared <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    /// <param name="info">The named-type check to emit.</param>
    public static void Append(FormattedStringBuilder builder, NamedTypeImplementationInfo info)
    {
        // Narrow to INamedTypeSymbol so Name/Arity/ContainingType are available (harmless when the parameter already
        // is one).
        builder.Append(info.ParameterName).Append(" is global::Microsoft.CodeAnalysis.INamedTypeSymbol");
        AppendTypeBlock(builder, info, 0);
        builder.Append(';');
    }

    private static void AppendTypeBlock(
        FormattedStringBuilder builder,
        NamedTypeImplementationInfo info,
        int chainIndex
    )
    {
        var segment = info.TypeChain[chainIndex];
        var isOutermost = chainIndex == info.TypeChain.Count - 1;

        builder.AppendLine().Append('{').Indent();
        builder.AppendLine().Append("Name: \"").Append(segment.Name).Append("\",");
        builder.AppendLine().Append("Arity: ").Append(segment.Arity).Append(',');

        if (isOutermost)
        {
            builder.AppendLine().Append("ContainingType: null,");

            if (info.Namespaces.Count == 0)
            {
                builder.AppendLine().Append("ContainingNamespace.IsGlobalNamespace: true");
            }
            else
            {
                builder.AppendLine().Append("ContainingNamespace:");
                AppendNamespaceValue(builder, info, 0);
            }
        }
        else
        {
            builder.AppendLine().Append("ContainingType:");
            AppendTypeBlock(builder, info, chainIndex + 1);
        }

        builder.Unindent().AppendLine().Append('}');
    }

    private static void AppendNamespaceValue(
        FormattedStringBuilder builder,
        NamedTypeImplementationInfo info,
        int index
    )
    {
        var name = info.Namespaces[index];

        // The deepest namespace (adjacent to the global namespace) is rendered inline.
        if (index == info.Namespaces.Count - 1)
        {
            builder.Append(" { Name: \"").Append(name).Append("\", ContainingNamespace.IsGlobalNamespace: true }");
            return;
        }

        builder.AppendLine().Append('{').Indent();
        builder.AppendLine().Append("Name: \"").Append(name).Append("\",");
        builder.AppendLine().Append("ContainingNamespace:");
        AppendNamespaceValue(builder, info, index + 1);
        builder.Unindent().AppendLine().Append('}');
    }
}
