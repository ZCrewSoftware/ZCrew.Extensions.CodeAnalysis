using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Emitters;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A type argument matched against a named type, by walking its <c>Name</c>/<c>Arity</c>/<c>TypeArguments</c>/
///     <c>ContainingType</c> chain and containing namespaces. Recurses through <see cref="TypeSegment.TypeArguments"/>,
///     so nested generics (e.g. the <c>List&lt;string&gt;</c> in <c>Task&lt;List&lt;string&gt;&gt;</c>) are
///     constrained too.
/// </summary>
/// <param name="TypeChain">The argument type and its containing types, innermost-first.</param>
/// <param name="Namespaces">The argument type's containing namespace names, innermost-first.</param>
internal sealed record NamedTypeArgument(EquatableArray<TypeSegment> TypeChain, EquatableArray<string> Namespaces)
    : ITypeArgument
{
    /// <inheritdoc/>
    public bool IsConstraining => true;

    /// <inheritdoc/>
    public bool IsMultiline => true;

    /// <inheritdoc/>
    public FormattedStringBuilder Emit(FormattedStringBuilder builder)
    {
        NamedTypeImplementationEmitter.EmitNamedTypeArgument(builder, this.TypeChain, this.Namespaces);
        return builder;
    }

    /// <inheritdoc/>
    public bool Equals(ITypeArgument? other)
    {
        return Equals(other as NamedTypeArgument);
    }
}
