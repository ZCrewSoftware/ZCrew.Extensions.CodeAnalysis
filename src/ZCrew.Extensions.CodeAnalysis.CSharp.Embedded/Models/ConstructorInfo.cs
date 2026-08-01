using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

internal readonly record struct ConstructorInfo(
    EquatableArray<string> TypeParameterNames,
    EquatableArray<string> ParameterNames
);
