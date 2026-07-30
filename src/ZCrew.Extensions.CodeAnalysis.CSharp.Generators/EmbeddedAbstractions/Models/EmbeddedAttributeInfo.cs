using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAbstractions.Models;

internal readonly record struct EmbeddedAttributeInfo(
    string Name,
    string Namespace,
    int Arity,
    EquatableArray<ConstructorInfo> Constructors,
    EquatableArray<ParameterInfo> Parameters,
    EquatableArray<TypeParameterInfo> TypeParameters,
    EquatableArray<NamedPropertyInfo> NamedProperties
);
