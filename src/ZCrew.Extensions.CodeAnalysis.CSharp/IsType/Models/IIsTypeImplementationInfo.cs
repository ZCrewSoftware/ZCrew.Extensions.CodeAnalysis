using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     Marker for the variant part of a type check: the <c>is</c> pattern that distinguishes one kind of target type
///     from another (e.g. a named type vs. a special type). Composed into an <see cref="IsTypeMethodInfo"/>; the
///     matching source generator in <c>SourceGenerators</c> supplies the emit logic for each variant.
/// </summary>
internal interface IIsTypeImplementationInfo
{
    /// <summary>
    ///     Emit the expression bodied implementation for this check.
    /// </summary>
    /// <param name="builder">The output to emit to.</param>
    /// <returns>The same <see cref="FormattedStringBuilder"/> for chaining calls.</returns>
    FormattedStringBuilder Emit(FormattedStringBuilder builder);
}
