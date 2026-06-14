namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A single segment of a type's containing-type chain, captured as cache-safe primitives.
/// </summary>
/// <param name="Name">The metadata name of the type.</param>
/// <param name="Arity">The number of generic type parameters on the type.</param>
internal readonly record struct TypeSegment(string Name, int Arity);
