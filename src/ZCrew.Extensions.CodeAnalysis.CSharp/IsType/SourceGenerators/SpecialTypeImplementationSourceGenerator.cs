using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGenerators;

/// <summary>
///     Appends the <c>is</c> pattern for a <see cref="SpecialTypeImplementationInfo"/> -- a direct match on a
///     well-known <see cref="Microsoft.CodeAnalysis.SpecialType"/>.
/// </summary>
internal static class SpecialTypeImplementationSourceGenerator
{
    /// <summary>
    ///     Appends the variant <c>is</c> pattern into the shared <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    /// <param name="info">The special-type check to emit.</param>
    public static void Append(FormattedStringBuilder builder, SpecialTypeImplementationInfo info)
    {
        // Narrow to ITypeSymbol so SpecialType is available (harmless when the parameter already is one).
        builder.Append(info.ParameterName).Append(" is global::Microsoft.CodeAnalysis.ITypeSymbol");
        builder.AppendLine().Append('{').Indent();
        builder
            .AppendLine()
            .Append("SpecialType: ")
            .Append("global::Microsoft.CodeAnalysis.SpecialType.")
            .Append(info.SpecialType);
        builder.Unindent().AppendLine().Append('}').Append(';');
    }
}
