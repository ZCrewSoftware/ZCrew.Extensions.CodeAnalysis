using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;

/// <summary>
///     Cache-safe metadata about a partial method marked with <c>IsTypeAttribute</c>. Carries the shared
///     method/wrapper data and composes an <see cref="IIsTypeImplementationInfo"/> that supplies the variant
///     <c>is</c> pattern. All members are primitives, <see cref="EquatableArray{T}"/>, or an equatable implementation,
///     so the model participates correctly in incremental generator caching (no <c>ISymbol</c> is retained).
/// </summary>
/// <param name="HintName">The hint name used when emitting the generated source.</param>
/// <param name="Namespace">The namespace of the method's containing type, or <see langword="null"/> for global.</param>
/// <param name="ContainingTypeDeclarations">
///     The partial declarations of the containing type(s), outermost-first (e.g. <c>partial class SymbolChecks</c>).
/// </param>
/// <param name="MethodSignature">The full implementing method signature, excluding the expression body.</param>
/// <param name="Implementation">The variant type check composed into this method.</param>
internal readonly record struct IsTypeMethodInfo(
    string HintName,
    string? Namespace,
    EquatableArray<string> ContainingTypeDeclarations,
    string MethodSignature,
    IIsTypeImplementationInfo Implementation
);
