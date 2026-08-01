namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

/// <param name="Name">The generated field name.</param>
/// <param name="SourceName">The type parameter name as declared on the attribute, used for documentation.</param>
internal readonly record struct TypeParameterInfo(string Name, string SourceName);
