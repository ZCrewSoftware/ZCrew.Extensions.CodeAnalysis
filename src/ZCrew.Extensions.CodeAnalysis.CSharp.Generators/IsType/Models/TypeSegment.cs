using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     A single segment of a type's containing-type chain, captured as cache-safe primitives.
/// </summary>
/// <param name="Name">The metadata name of the type.</param>
/// <param name="Arity">The number of generic type parameters on the type.</param>
/// <param name="TypeArguments">
///     The type's generic arguments. Empty for non-generic types and for open/unbound generics (whose arguments are
///     type parameters), in which case no argument constraint is emitted.
/// </param>
internal readonly record struct TypeSegment(string Name, int Arity, EquatableArray<ITypeArgument> TypeArguments);
