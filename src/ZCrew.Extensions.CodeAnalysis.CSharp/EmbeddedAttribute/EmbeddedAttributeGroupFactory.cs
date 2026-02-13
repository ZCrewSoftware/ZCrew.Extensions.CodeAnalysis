using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.Models;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute;

internal static class EmbeddedAttributeGroupFactory
{
    public static EquatableArray<EmbeddedAttributeGroup> Create(
        ImmutableArray<EmbeddedTypeInfo> types,
        CancellationToken cancellationToken
    )
    {
        if (types.IsDefaultOrEmpty)
        {
            return EquatableArray<EmbeddedAttributeGroup>.Empty;
        }

        // Group by (Name, Namespace) - INamedTypeSymbol.Name does not include arity
        var groups = new Dictionary<(string Name, string Namespace), List<EmbeddedTypeInfo>>();

        foreach (var type in types)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = (type.Name, type.Namespace);
            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }

            list.Add(type);
        }

        var result = ImmutableArray.CreateBuilder<EmbeddedAttributeGroup>(groups.Count);

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(CreateGroup(group.Key.Name, group.Key.Namespace, group.Value));
        }

        return result.ToEquatableArray();
    }

    private static EmbeddedAttributeGroup CreateGroup(string baseName, string @namespace, List<EmbeddedTypeInfo> types)
    {
        // Combine source texts, deduplicating identical texts
        var uniqueSourceTexts = new HashSet<string>();
        foreach (var type in types)
        {
            uniqueSourceTexts.Add(type.SourceText);
        }

        var combinedSourceText = string.Join("\n", uniqueSourceTexts);

        // Check if any type is an attribute
        var hasAttributes = false;
        foreach (var type in types)
        {
            if (type.IsAttribute)
            {
                hasAttributes = true;
                break;
            }
        }

        // Collect all constructors from attribute types
        var allConstructors = ImmutableArray.CreateBuilder<EmbeddedConstructorInfo>();
        foreach (var type in types)
        {
            if (type.IsAttribute)
            {
                foreach (var constructor in type.Constructors)
                {
                    allConstructors.Add(constructor);
                }
            }
        }

        // Deduplicate parameters by name
        var uniqueParameters = DeduplicateByName(
            types,
            static type => type.IsAttribute ? type.Constructors : default,
            static constructor => constructor.Parameters,
            static param => param.Name
        );

        // Deduplicate type parameters by name
        var uniqueTypeParameters = DeduplicateTypeParameters(types);

        // Deduplicate named parameters (properties) by name
        var uniqueNamedParameters = DeduplicateProperties(types);

        return new EmbeddedAttributeGroup(
            baseName,
            @namespace,
            combinedSourceText,
            hasAttributes,
            allConstructors.ToEquatableArray(),
            uniqueParameters,
            uniqueTypeParameters,
            uniqueNamedParameters
        );
    }

    private static EquatableArray<EmbeddedParameterInfo> DeduplicateByName(
        List<EmbeddedTypeInfo> types,
        Func<EmbeddedTypeInfo, EquatableArray<EmbeddedConstructorInfo>> getConstructors,
        Func<EmbeddedConstructorInfo, EquatableArray<EmbeddedParameterInfo>> getParameters,
        Func<EmbeddedParameterInfo, string> getName
    )
    {
        var seen = new HashSet<string>();
        var result = ImmutableArray.CreateBuilder<EmbeddedParameterInfo>();

        foreach (var type in types)
        {
            var constructors = getConstructors(type);
            if (constructors.IsDefaultOrEmpty)
            {
                continue;
            }

            foreach (var constructor in constructors)
            {
                var parameters = getParameters(constructor);
                foreach (var parameter in parameters)
                {
                    var name = getName(parameter);
                    if (seen.Add(name))
                    {
                        result.Add(parameter);
                    }
                }
            }
        }

        if (result.Count == 0)
        {
            return EquatableArray<EmbeddedParameterInfo>.Empty;
        }

        return result.ToEquatableArray();
    }

    private static EquatableArray<EmbeddedTypeParameterInfo> DeduplicateTypeParameters(List<EmbeddedTypeInfo> types)
    {
        var seen = new HashSet<string>();
        var result = ImmutableArray.CreateBuilder<EmbeddedTypeParameterInfo>();

        foreach (var type in types)
        {
            if (!type.IsAttribute)
            {
                continue;
            }

            foreach (var typeParam in type.TypeParameters)
            {
                if (seen.Add(typeParam.Name))
                {
                    result.Add(typeParam);
                }
            }
        }

        if (result.Count == 0)
        {
            return EquatableArray<EmbeddedTypeParameterInfo>.Empty;
        }

        return result.ToEquatableArray();
    }

    private static EquatableArray<EmbeddedNamedParameterInfo> DeduplicateProperties(List<EmbeddedTypeInfo> types)
    {
        var seen = new HashSet<string>();
        var result = ImmutableArray.CreateBuilder<EmbeddedNamedParameterInfo>();

        foreach (var type in types)
        {
            if (!type.IsAttribute)
            {
                continue;
            }

            foreach (var property in type.Properties)
            {
                if (seen.Add(property.Name))
                {
                    result.Add(property);
                }
            }
        }

        if (result.Count == 0)
        {
            return EquatableArray<EmbeddedNamedParameterInfo>.Empty;
        }

        return result.ToEquatableArray();
    }
}
