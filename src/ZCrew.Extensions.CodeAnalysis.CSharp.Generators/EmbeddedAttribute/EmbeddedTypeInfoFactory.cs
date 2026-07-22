using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.Models;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute;

internal static class EmbeddedTypeInfoFactory
{
    public static EmbeddedTypeInfo? Create(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var name = namedTypeSymbol.Name;
        var @namespace = namedTypeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : namedTypeSymbol.ContainingNamespace.ToDisplayString();
        var arity = namedTypeSymbol.Arity;
        var sourceText = context.TargetNode.SyntaxTree.GetText(cancellationToken).ToString();
        var isAttribute = IsAttribute(namedTypeSymbol);

        cancellationToken.ThrowIfCancellationRequested();

        var constructors = ExtractConstructors(namedTypeSymbol, cancellationToken);
        var typeParameters = ExtractTypeParameters(namedTypeSymbol);
        var properties = ExtractProperties(namedTypeSymbol);

        return new EmbeddedTypeInfo(
            name,
            @namespace,
            arity,
            sourceText,
            isAttribute,
            constructors,
            typeParameters,
            properties
        );
    }

    private static bool IsAttribute(INamedTypeSymbol typeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (current.Name == "Attribute" && current.ContainingNamespace.ToDisplayString() == "System")
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static EquatableArray<EmbeddedConstructorInfo> ExtractConstructors(
        INamedTypeSymbol typeSymbol,
        CancellationToken cancellationToken
    )
    {
        var constructors = typeSymbol.Constructors;
        if (constructors.IsDefaultOrEmpty)
        {
            return EquatableArray<EmbeddedConstructorInfo>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<EmbeddedConstructorInfo>(constructors.Length);

        foreach (var constructor in constructors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Skip implicit default constructors only when explicit constructors exist
            if (constructor.IsImplicitlyDeclared && constructors.Length > 1)
            {
                continue;
            }

            var typeParameterNames = ImmutableArray.CreateBuilder<string>(typeSymbol.TypeParameters.Length);
            foreach (var typeParam in typeSymbol.TypeParameters)
            {
                typeParameterNames.Add(typeParam.Name);
            }

            var parameters = ImmutableArray.CreateBuilder<EmbeddedParameterInfo>(constructor.Parameters.Length);
            foreach (var parameter in constructor.Parameters)
            {
                parameters.Add(new EmbeddedParameterInfo(parameter.Name, parameter.Type.ToGenericTypeName()));
            }

            builder.Add(
                new EmbeddedConstructorInfo(typeParameterNames.ToEquatableArray(), parameters.ToEquatableArray())
            );
        }

        return builder.ToEquatableArray();
    }

    private static EquatableArray<EmbeddedTypeParameterInfo> ExtractTypeParameters(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeParameters.IsDefaultOrEmpty)
        {
            return EquatableArray<EmbeddedTypeParameterInfo>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<EmbeddedTypeParameterInfo>(typeSymbol.TypeParameters.Length);
        foreach (var typeParam in typeSymbol.TypeParameters)
        {
            builder.Add(new EmbeddedTypeParameterInfo(typeParam.Name));
        }

        return builder.ToEquatableArray();
    }

    private static EquatableArray<EmbeddedNamedParameterInfo> ExtractProperties(INamedTypeSymbol typeSymbol)
    {
        var members = typeSymbol.GetMembers();
        var builder = ImmutableArray.CreateBuilder<EmbeddedNamedParameterInfo>();

        foreach (var member in members)
        {
            if (member is IPropertySymbol { SetMethod.DeclaredAccessibility: Accessibility.Public } propertySymbol)
            {
                builder.Add(new EmbeddedNamedParameterInfo(propertySymbol.Name));
            }
        }

        if (builder.Count == 0)
        {
            return EquatableArray<EmbeddedNamedParameterInfo>.Empty;
        }

        return builder.ToEquatableArray();
    }
}
