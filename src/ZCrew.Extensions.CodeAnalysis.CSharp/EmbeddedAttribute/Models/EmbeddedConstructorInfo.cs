using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.Models;

internal readonly record struct EmbeddedConstructorInfo(
    EquatableArray<string> TypeParameterNames,
    EquatableArray<EmbeddedParameterInfo> Parameters
);
