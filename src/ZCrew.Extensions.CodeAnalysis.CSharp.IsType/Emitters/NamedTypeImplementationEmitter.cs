using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Emitters;

/// <summary>
///     Appends the <c>is</c> pattern for a <see cref="NamedTypeImplementationInfo"/> -- matched by walking the type's
///     <c>Name</c>/<c>Arity</c>/<c>TypeArguments</c>/<c>ContainingType</c> chain and containing namespaces.
/// </summary>
internal static class NamedTypeImplementationEmitter
{
    /// <summary>
    ///     Appends the variant <c>is</c> pattern into the shared <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    /// <param name="info">The named-type check to emit.</param>
    public static void Emit(FormattedStringBuilder builder, NamedTypeImplementationInfo info)
    {
        // Narrow to INamedTypeSymbol so Name/Arity/TypeArguments/ContainingType are available (harmless when the
        // parameter already is one).
        builder.Append(info.ParameterName).Append(" is ");
        EmitNamedTypeArgument(builder, info.TypeChain, info.Namespaces);
        builder.Append(';');
    }

    /// <summary>
    ///     Emits a named-type pattern (<c>INamedTypeSymbol { ... }</c>) for the given chain. Shared by the top-level
    ///     check and by <see cref="NamedTypeArgument"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    /// <param name="typeChain">The target type and its containing types, innermost-first.</param>
    /// <param name="namespaces">The target type's containing namespace names, innermost-first.</param>
    internal static void EmitNamedTypeArgument(
        FormattedStringBuilder builder,
        EquatableArray<TypeSegment> typeChain,
        EquatableArray<string> namespaces
    )
    {
        builder.Append("global::Microsoft.CodeAnalysis.INamedTypeSymbol");
        EmitTypeBlock(builder, typeChain, namespaces, 0);
    }

    /// <summary>
    ///     Emits the discard pattern (<c>_</c>) for an open <see cref="TypeParameterArgument"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    internal static void EmitTypeParameterArgument(FormattedStringBuilder builder)
    {
        builder.Append('_');
    }

    /// <summary>
    ///     Emits a special-type element pattern (<c>{ SpecialType: ... }</c>) for a <see cref="SpecialTypeArgument"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    /// <param name="specialType">The <c>Microsoft.CodeAnalysis.SpecialType</c> enum member name.</param>
    internal static void EmitSpecialTypeArgument(FormattedStringBuilder builder, string specialType)
    {
        builder.Append("{ SpecialType: global::Microsoft.CodeAnalysis.SpecialType.").Append(specialType).Append(" }");
    }

    private static void EmitTypeBlock(
        FormattedStringBuilder builder,
        EquatableArray<TypeSegment> typeChain,
        EquatableArray<string> namespaces,
        int chainIndex
    )
    {
        var segment = typeChain[chainIndex];
        var isOutermost = chainIndex == typeChain.Count - 1;

        builder.AppendLine().Append('{').Indent();
        builder.AppendLine().Append("Name: \"").Append(segment.Name).Append("\",");
        builder.AppendLine().Append("Arity: ").Append(segment.Arity).Append(',');

        EmitTypeArguments(builder, segment.TypeArguments);

        if (isOutermost)
        {
            builder.AppendLine().Append("ContainingType: null,");

            if (namespaces.Count == 0)
            {
                builder.AppendLine().Append("ContainingNamespace.IsGlobalNamespace: true");
            }
            else
            {
                builder.AppendLine().Append("ContainingNamespace:");
                EmitNamespaceValue(builder, namespaces, 0);
            }
        }
        else
        {
            builder.AppendLine().Append("ContainingType:");
            EmitTypeBlock(builder, typeChain, namespaces, chainIndex + 1);
        }

        builder.Unindent().AppendLine().Append('}');
    }

    private static void EmitTypeArguments(FormattedStringBuilder builder, EquatableArray<ITypeArgument> arguments)
    {
        if (arguments.Count == 0)
        {
            return;
        }

        var isConstraining = false;
        var isMultiline = false;
        foreach (var argument in arguments)
        {
            isConstraining |= argument.IsConstraining;
            isMultiline |= argument.IsMultiline;
        }

        // An all-unconstrained list (e.g. an open/unbound generic, or a single unrepresentable array argument) places
        // no constraint, leaving the pattern matching any instantiation -- so emit nothing.
        if (!isConstraining)
        {
            return;
        }

        builder.AppendLine().Append("TypeArguments:");

        // A list with a (multi-line) named element is laid out one element per line; an all-simple list stays inline.
        if (isMultiline)
        {
            builder.AppendLine().Append('[').Indent();
            for (var i = 0; i < arguments.Count; i++)
            {
                builder.AppendLine();
                arguments[i].Emit(builder);
                if (i < arguments.Count - 1)
                {
                    builder.Append(',');
                }
            }
            builder.Unindent().AppendLine().Append("],");
        }
        else
        {
            builder.Append(" [");
            for (var i = 0; i < arguments.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                arguments[i].Emit(builder);
            }
            builder.Append("],");
        }
    }

    private static void EmitNamespaceValue(FormattedStringBuilder builder, EquatableArray<string> namespaces, int index)
    {
        var name = namespaces[index];

        // The deepest namespace (adjacent to the global namespace) is rendered inline.
        if (index == namespaces.Count - 1)
        {
            builder.Append(" { Name: \"").Append(name).Append("\", ContainingNamespace.IsGlobalNamespace: true }");
            return;
        }

        builder.AppendLine().Append('{').Indent();
        builder.AppendLine().Append("Name: \"").Append(name).Append("\",");
        builder.AppendLine().Append("ContainingNamespace:");
        EmitNamespaceValue(builder, namespaces, index + 1);
        builder.Unindent().AppendLine().Append('}');
    }
}
