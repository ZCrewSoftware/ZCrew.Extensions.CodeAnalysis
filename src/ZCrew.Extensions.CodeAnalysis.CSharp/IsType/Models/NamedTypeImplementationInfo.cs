using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGenerators;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A type check against a named type, matched by walking its <c>Name</c>/<c>Arity</c>/<c>ContainingType</c> chain
///     and containing namespaces (anchored at the global namespace).
/// </summary>
/// <param name="ParameterName">The name of the method's single parameter.</param>
/// <param name="TypeChain">The target type and its containing types, innermost-first.</param>
/// <param name="Namespaces">The target type's containing namespace names, innermost-first.</param>
internal readonly record struct NamedTypeImplementationInfo(
    string ParameterName,
    EquatableArray<TypeSegment> TypeChain,
    EquatableArray<string> Namespaces
) : IIsTypeImplementationInfo
{
    /// <inheritdoc/>
    public FormattedStringBuilder AppendImplementation(FormattedStringBuilder builder)
    {
        NamedTypeImplementationSourceGenerator.Append(builder, this);
        return builder;
    }
}
