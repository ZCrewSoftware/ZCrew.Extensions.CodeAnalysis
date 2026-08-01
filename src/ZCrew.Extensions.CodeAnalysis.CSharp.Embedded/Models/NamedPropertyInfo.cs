using System.Collections.Immutable;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

/// <param name="Name">The name of the generated field, which may be disambiguated from <paramref name="SourceName"/>.</param>
/// <param name="SourceName">The property name as declared on the attribute, matched against the named argument at runtime.</param>
/// <param name="Type">The type as declared on the attribute, matched against the constant's type at runtime.</param>
/// <param name="PropertyType">The type the generated field is declared as.</param>
/// <param name="IsAlwaysSet">Whether the compiler forces the attribute usage to set this property.</param>
/// <param name="IsNonNullableReference">Whether <paramref name="PropertyType"/> is an unannotated reference type.</param>
/// <param name="IsArray">Whether <paramref name="PropertyType"/> is an <see cref="ImmutableArray{T}"/>.</param>
internal readonly record struct NamedPropertyInfo(
    string Name,
    string SourceName,
    string Type,
    string PropertyType,
    bool IsAlwaysSet,
    bool IsNonNullableReference,
    bool IsArray
);
