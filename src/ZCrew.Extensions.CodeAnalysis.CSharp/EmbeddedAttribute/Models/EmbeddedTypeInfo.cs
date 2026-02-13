using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.Models;

internal readonly record struct EmbeddedTypeInfo(
    string Name,
    string Namespace,
    int Arity,
    string SourceText,
    bool IsAttribute,
    EquatableArray<EmbeddedConstructorInfo> Constructors,
    EquatableArray<EmbeddedTypeParameterInfo> TypeParameters,
    EquatableArray<EmbeddedNamedParameterInfo> Properties
);
