using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded;

internal static partial class EmbeddedAttributeInfoFactory
{
    public static EmbeddedAttributeInfo? Create(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return null;
        }

        if (!IsAttribute(namedTypeSymbol))
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var name = namedTypeSymbol.Name;
        var @namespace = namedTypeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : namedTypeSymbol.ContainingNamespace.ToDisplayString();
        var arity = namedTypeSymbol.Arity;

        cancellationToken.ThrowIfCancellationRequested();

        var names = new NameTable();

        var parameters = ExtractParameters(names, namedTypeSymbol, cancellationToken);
        var typeParameters = ExtractTypeParameters(names, namedTypeSymbol, cancellationToken);
        var properties = ExtractProperties(names, namedTypeSymbol, cancellationToken);
        var constructors = ExtractConstructors(names, namedTypeSymbol, cancellationToken);

        return new EmbeddedAttributeInfo(name, @namespace, arity, constructors, parameters, typeParameters, properties);
    }

    private static EquatableArray<ConstructorInfo> ExtractConstructors(
        NameTable names,
        INamedTypeSymbol typeSymbol,
        CancellationToken cancellationToken
    )
    {
        var constructors = typeSymbol.Constructors;
        if (constructors.IsDefaultOrEmpty)
        {
            return EquatableArray<ConstructorInfo>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<ConstructorInfo>(constructors.Length);

        foreach (var constructor in constructors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsSkippedConstructor(constructor, constructors.Length))
            {
                continue;
            }

            var typeParameterNames = ImmutableArray.CreateBuilder<string>(typeSymbol.TypeParameters.Length);
            foreach (var typeParam in typeSymbol.TypeParameters)
            {
                var name = names.GetTypeParameterName(typeParam);
                typeParameterNames.Add(name);
            }

            var parameterNames = ImmutableArray.CreateBuilder<string>(constructor.Parameters.Length);
            foreach (var parameter in constructor.Parameters)
            {
                var name = names.GetParameterName(parameter);
                parameterNames.Add(name);
            }

            builder.Add(new ConstructorInfo(typeParameterNames.ToEquatableArray(), parameterNames.ToEquatableArray()));
        }

        return builder.ToEquatableArray();
    }

    /// <summary>
    ///     Whether the <paramref name="constructor"/> should be ignored. Implicit parameterless constructors and
    ///     non-public constructors should be ignored.
    /// </summary>
    private static bool IsSkippedConstructor(IMethodSymbol constructor, int constructorCount)
    {
        return constructor.IsImplicitlyDeclared && constructorCount > 1
            || constructor.DeclaredAccessibility != Accessibility.Public;
    }

    /// <summary>
    ///     Whether the field emitted for <paramref name="typeSymbol"/> is a reference type that would warn when left
    ///     unassigned. Arrays are emitted as <see cref="ImmutableArray{T}"/>, a value type, and an already-annotated
    ///     type is emitted with its <c>?</c> intact.
    /// </summary>
    private static bool IsNonNullableReference(ITypeSymbol typeSymbol)
    {
        return typeSymbol is not IArrayTypeSymbol
            && typeSymbol.IsReferenceType
            && typeSymbol.NullableAnnotation is not NullableAnnotation.Annotated;
    }

    /// <summary>
    ///     Whether the field for <paramref name="typeSymbol"/> will be an <see cref="ImmutableArray{T}"/>.
    /// </summary>
    private static bool IsArrayType(ITypeSymbol typeSymbol)
    {
        return typeSymbol is IArrayTypeSymbol;
    }

    /// <summary>
    ///     Whether any constructor suppresses the compiler's enforcement of <c>required</c> members. When one does, no
    ///     property can be assumed set, because the matching constructor is not known until compilation.
    /// </summary>
    private static bool HasSetsRequiredMembersConstructor(INamedTypeSymbol typeSymbol)
    {
        var constructors = typeSymbol.Constructors;

        foreach (var constructor in constructors)
        {
            if (IsSkippedConstructor(constructor, constructors.Length))
            {
                continue;
            }

            foreach (var attribute in constructor.GetAttributes())
            {
                if (IsSetsRequiredMembersAttribute(attribute.AttributeClass))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string ToPropertyType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            // A jagged type can be declared even though CS0181 blocks applying the attribute.
            var elementType = ToPropertyType(arrayTypeSymbol.ElementType);
            return $"global::System.Collections.Immutable.ImmutableArray<{elementType}>";
        }

        // A System.Type argument has no runtime type in the compilation being analyzed.
        return IsSystemType(typeSymbol)
            ? "global::Microsoft.CodeAnalysis.ITypeSymbol"
            : typeSymbol.ToFullyQualifiedName();
    }

    private static bool IsAttribute(INamedTypeSymbol typeSymbol)
    {
        for (var baseType = typeSymbol.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (IsSystemAttribute(baseType))
            {
                return true;
            }
        }
        return false;
    }

    [IsType<Attribute>]
    private static partial bool IsSystemAttribute(INamedTypeSymbol? typeSymbol);

    [IsType<Type>]
    private static partial bool IsSystemType(ITypeSymbol typeSymbol);

    [IsType<SetsRequiredMembersAttribute>]
    private static partial bool IsSetsRequiredMembersAttribute(INamedTypeSymbol? typeSymbol);

    private static EquatableArray<ParameterInfo> ExtractParameters(
        NameTable names,
        INamedTypeSymbol typeSymbol,
        CancellationToken token
    )
    {
        var constructors = typeSymbol.Constructors;
        if (constructors.IsDefaultOrEmpty)
        {
            return EquatableArray<ParameterInfo>.Empty;
        }

        var namesSeen = new HashSet<string>();
        var occurrences = new Dictionary<string, int>();
        var constructorCount = 0;
        var builder = ImmutableArray.CreateBuilder<ParameterInfo>(2 * constructors.Length);
        foreach (var constructor in constructors)
        {
            token.ThrowIfCancellationRequested();

            if (IsSkippedConstructor(constructor, constructors.Length))
            {
                continue;
            }

            constructorCount++;
            foreach (var parameter in constructor.Parameters)
            {
                var name = names.GetParameterName(parameter);
                occurrences[name] = occurrences.TryGetValue(name, out var count) ? count + 1 : 1;
                if (namesSeen.Add(name))
                {
                    builder.Add(
                        new ParameterInfo(
                            name,
                            parameter.Type.ToFullyQualifiedName(nullableAnnotations: false),
                            ToPropertyType(parameter.Type),
                            IsAlwaysSet: false,
                            IsNonNullableReference(parameter.Type),
                            IsArrayType(parameter.Type)
                        )
                    );
                }
            }
        }

        // The record unions every constructor's parameters, so only those declared by all of them are always assigned
        for (var i = 0; i < builder.Count; i++)
        {
            var parameter = builder[i];
            builder[i] = parameter with { IsAlwaysSet = occurrences[parameter.Name] == constructorCount };
        }

        return builder.ToEquatableArray();
    }

    private static EquatableArray<TypeParameterInfo> ExtractTypeParameters(
        NameTable names,
        INamedTypeSymbol typeSymbol,
        CancellationToken token
    )
    {
        if (typeSymbol.TypeParameters.IsDefaultOrEmpty)
        {
            return EquatableArray<TypeParameterInfo>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<TypeParameterInfo>(typeSymbol.TypeParameters.Length);
        foreach (var typeParam in typeSymbol.TypeParameters)
        {
            token.ThrowIfCancellationRequested();

            var name = names.GetTypeParameterName(typeParam);
            builder.Add(new TypeParameterInfo(name, typeParam.Name));
        }

        return builder.ToEquatableArray();
    }

    private static EquatableArray<NamedPropertyInfo> ExtractProperties(
        NameTable names,
        INamedTypeSymbol typeSymbol,
        CancellationToken token
    )
    {
        var members = typeSymbol.GetMembers();
        var builder = ImmutableArray.CreateBuilder<NamedPropertyInfo>();

        // Named arguments are optional, so a property is only ever guaranteed when the compiler requires it
        var isRequiredEnforced = !HasSetsRequiredMembersConstructor(typeSymbol);

        foreach (var member in members)
        {
            token.ThrowIfCancellationRequested();

            if (member is not IPropertySymbol property)
            {
                continue;
            }

            if (
                property.IsStatic
                || property.IsIndexer
                || property.SetMethod?.DeclaredAccessibility != Accessibility.Public
            )
            {
                continue;
            }

            builder.Add(
                new NamedPropertyInfo(
                    names.GetPropertyName(property),
                    property.Name,
                    property.Type.ToFullyQualifiedName(nullableAnnotations: false),
                    ToPropertyType(property.Type),
                    property.IsRequired && isRequiredEnforced,
                    IsNonNullableReference(property.Type),
                    IsArrayType(property.Type)
                )
            );
        }

        if (builder.Count == 0)
        {
            return EquatableArray<NamedPropertyInfo>.Empty;
        }

        return builder.ToEquatableArray();
    }
}
