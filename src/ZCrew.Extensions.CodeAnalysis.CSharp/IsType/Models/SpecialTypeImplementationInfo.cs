using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGenerators;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A type check against a well-known special type (e.g. <see cref="System.IDisposable"/>), matched directly via its
///     <see cref="Microsoft.CodeAnalysis.SpecialType"/>.
/// </summary>
/// <param name="ParameterName">The name of the method's single parameter.</param>
/// <param name="SpecialType">
///     The <see cref="Microsoft.CodeAnalysis.SpecialType"/> enum member name (e.g. <see cref="System.IDisposable"/>).
/// </param>
internal readonly record struct SpecialTypeImplementationInfo(string ParameterName, string SpecialType)
    : IIsTypeImplementationInfo
{
    /// <inheritdoc/>
    public FormattedStringBuilder AppendImplementation(FormattedStringBuilder builder)
    {
        SpecialTypeImplementationSourceGenerator.Append(builder, this);
        return builder;
    }
}
