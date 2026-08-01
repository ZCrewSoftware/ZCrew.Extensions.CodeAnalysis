using System.Collections.Immutable;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

/// <param name="Name">The name of the generated field.</param>
/// <param name="Type">The type as declared on the attribute, matched against the constant's type at runtime.</param>
/// <param name="PropertyType">The type the generated field is declared as.</param>
/// <param name="IsAlwaysSet">Whether every constructor declares this parameter, so it is always assigned.</param>
/// <param name="IsNonNullableReference">Whether <paramref name="PropertyType"/> is an unannotated reference type.</param>
/// <param name="IsArray">Whether <paramref name="PropertyType"/> is an <see cref="ImmutableArray{T}"/>.</param>
internal readonly record struct ParameterInfo(
    string Name,
    string Type,
    string PropertyType,
    bool IsAlwaysSet,
    bool IsNonNullableReference,
    bool IsArray
);
