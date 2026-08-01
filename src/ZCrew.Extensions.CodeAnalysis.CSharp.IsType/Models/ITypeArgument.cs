using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     One generic type argument of a named-type check. Mirrors <see cref="IIsTypeImplementationInfo"/>: each variant
///     (an open type parameter, a special type, or a named type) is its own implementation, so there are no nullable
///     "union" fields and dispatch is polymorphic rather than a switch over a discriminator.
/// </summary>
/// <remarks>
///     Implementations are stored in an <see cref="Collections.EquatableArray{T}"/> on <see cref="TypeSegment"/>, which
///     requires value equality (hence <see cref="IEquatable{T}"/>); the interface is also a reference type, which is
///     what breaks the otherwise-cyclic <see cref="TypeSegment"/>/argument value-type graph at load time.
/// </remarks>
internal interface ITypeArgument : IEquatable<ITypeArgument>
{
    /// <summary>
    ///     Whether this argument narrows the match. An open type parameter does not, so an all-unconstrained argument
    ///     list emits no <c>TypeArguments</c> clause at all.
    /// </summary>
    bool IsConstraining { get; }

    /// <summary>
    ///     Whether this argument renders across multiple lines (a named type), which forces the whole argument list
    ///     onto multiple lines.
    /// </summary>
    bool IsMultiline { get; }

    /// <summary>
    /// this argument as a list-pattern element into the shared <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder accumulating the generated method body.</param>
    /// <returns>The same <see cref="FormattedStringBuilder"/> for chaining calls.</returns>
    FormattedStringBuilder Emit(FormattedStringBuilder builder);
}
