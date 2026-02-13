using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;

namespace EnumToStringFast;

internal class ToStringFastAttributeDataBuilder : IToStringFastAttributeDataBuilder
{
    public string? ExtensionClassNamespace { get; set; }

    public string? ExtensionClassName { get; set; }

    public string? EnumNamespace { get; set; }

    public string? EnumName { get; set; }

    public EquatableArray<string> Members { get; set; }

    public void WithEnum(ITypeSymbol symbol)
    {
        EnumNamespace = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();
        EnumName = symbol.Name;

        Members = ((INamedTypeSymbol)symbol)
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .Select(f => f.Name)
            .ToImmutableArray()
            .ToEquatableArray();
    }

    public void WithExtensionClassNamespace(TypedConstant constant)
    {
        if (constant.IsString)
        {
            ExtensionClassNamespace = (string)constant.Value!;
        }
    }

    public void WithExtensionClassName(TypedConstant constant)
    {
        if (constant.IsString)
        {
            ExtensionClassName = (string)constant.Value!;
        }
    }

    public ToStringFastAttributeData Build()
    {
        return new ToStringFastAttributeData(
            ExtensionClassNamespace,
            ExtensionClassName,
            EnumNamespace,
            EnumName,
            Members
        );
    }
}
