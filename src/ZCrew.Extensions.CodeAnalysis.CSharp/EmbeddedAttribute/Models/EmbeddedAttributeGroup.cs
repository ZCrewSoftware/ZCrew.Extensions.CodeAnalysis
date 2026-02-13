using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.Models;

internal readonly record struct EmbeddedAttributeGroup(
    string BaseName,
    string Namespace,
    string CombinedSourceText,
    bool HasAttributes,
    EquatableArray<EmbeddedConstructorInfo> AllConstructors,
    EquatableArray<EmbeddedParameterInfo> UniqueParameters,
    EquatableArray<EmbeddedTypeParameterInfo> UniqueTypeParameters,
    EquatableArray<EmbeddedNamedParameterInfo> UniqueNamedParameters
);
