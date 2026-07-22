using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Emitters;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A type argument matched against a well-known special type (e.g. <see cref="string"/>) via its
///     <see cref="Microsoft.CodeAnalysis.SpecialType"/>.
/// </summary>
/// <param name="SpecialType">The <see cref="Microsoft.CodeAnalysis.SpecialType"/> enum member name.</param>
internal sealed record SpecialTypeArgument(string SpecialType) : ITypeArgument
{
    /// <inheritdoc/>
    public bool IsConstraining => true;

    /// <inheritdoc/>
    public bool IsMultiline => false;

    /// <inheritdoc/>
    public FormattedStringBuilder Emit(FormattedStringBuilder builder)
    {
        NamedTypeImplementationEmitter.EmitSpecialTypeArgument(builder, this.SpecialType);
        return builder;
    }

    /// <inheritdoc/>
    public bool Equals(ITypeArgument? other)
    {
        return Equals(other as SpecialTypeArgument);
    }
}
