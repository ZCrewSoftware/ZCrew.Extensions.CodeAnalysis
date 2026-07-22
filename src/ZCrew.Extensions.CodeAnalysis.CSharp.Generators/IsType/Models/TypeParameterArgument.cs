using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Emitters;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A type argument that is an open type parameter (e.g. the placeholder an unbound <c>Task&lt;&gt;</c> exposes for
///     its arguments). It places no constraint on the match and renders as a discard (<c>_</c>).
/// </summary>
internal sealed record TypeParameterArgument : ITypeArgument
{
    /// <inheritdoc/>
    public bool IsConstraining => false;

    /// <inheritdoc/>
    public bool IsMultiline => false;

    /// <inheritdoc/>
    public FormattedStringBuilder Emit(FormattedStringBuilder builder)
    {
        NamedTypeImplementationEmitter.EmitTypeParameterArgument(builder);
        return builder;
    }

    /// <inheritdoc/>
    public bool Equals(ITypeArgument? other)
    {
        return other is TypeParameterArgument;
    }
}
