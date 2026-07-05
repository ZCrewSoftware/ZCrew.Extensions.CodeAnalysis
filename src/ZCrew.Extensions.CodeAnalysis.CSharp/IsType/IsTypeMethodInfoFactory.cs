using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Collections;
using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.Models;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType;

/// <summary>
///     Extracts a cache-safe <see cref="IsTypeMethodInfo"/> from a partial method marked with <c>IsTypeAttribute</c>.
/// </summary>
internal static class IsTypeMethodInfoFactory
{
    private static readonly SymbolDisplayFormat parameterTypeFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    /// <summary>
    ///     Creates an <see cref="IsTypeMethodInfo"/> for the generic <c>IsTypeAttribute&lt;T&gt;</c> form.
    /// </summary>
    /// <param name="context">The attribute syntax context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The extracted metadata, or <see langword="null"/> if the usage is unsupported.</returns>
    public static IsTypeMethodInfo? CreateFromGeneric(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        return Create(context, GetGenericTarget, cancellationToken);
    }

    /// <summary>
    ///     Creates an <see cref="IsTypeMethodInfo"/> for the non-generic <c>IsTypeAttribute(typeof(T))</c> form.
    /// </summary>
    /// <param name="context">The attribute syntax context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The extracted metadata, or <see langword="null"/> if the usage is unsupported.</returns>
    public static IsTypeMethodInfo? CreateFromTypeof(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        return Create(context, GetTypeofTarget, cancellationToken);
    }

    private static ITypeSymbol? GetGenericTarget(AttributeData attribute)
    {
        var typeArguments = attribute.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        return typeArguments.Length == 1 ? typeArguments[0] : null;
    }

    private static ITypeSymbol? GetTypeofTarget(AttributeData attribute)
    {
        if (
            attribute.ConstructorArguments.Length == 1
            && attribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: ITypeSymbol typeSymbol }
        )
        {
            return typeSymbol;
        }

        return null;
    }

    private static IsTypeMethodInfo? Create(
        GeneratorAttributeSyntaxContext context,
        Func<AttributeData, ITypeSymbol?> targetSelector,
        CancellationToken cancellationToken
    )
    {
        if (context.TargetSymbol is not IMethodSymbol method)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        // The method must be a partial definition (that we provide the implementation for) returning a bool and
        // accepting a single parameter. Anything else cannot be expressed as a single 'is' pattern.
        if (
            !method.IsPartialDefinition
            || method.PartialImplementationPart is not null
            || method.ReturnType.SpecialType != SpecialType.System_Boolean
            || method.Parameters.Length != 1
        )
        {
            return null;
        }

        if (context.Attributes.Length == 0)
        {
            return null;
        }

        var target = targetSelector(context.Attributes[0]);
        if (target is null or IErrorTypeSymbol)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var parameter = method.Parameters[0];
        var addNotNullWhenTrue = parameter.Type.NullableAnnotation == NullableAnnotation.Annotated;

        var signature = BuildSignature(method, parameter, addNotNullWhenTrue);
        var @namespace = method.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
            ? containingNamespace.ToDisplayString()
            : null;
        var containingTypeDeclarations = BuildContainingTypeDeclarations(method, out var containingTypeNames);
        var hintName = BuildHintName(@namespace, containingTypeNames, method.Name);

        var implementation = BuildImplementation(parameter.Name, target);
        if (implementation is null)
        {
            return null;
        }

        return new IsTypeMethodInfo(hintName, @namespace, containingTypeDeclarations, signature, implementation);
    }

    private static IIsTypeImplementationInfo? BuildImplementation(string parameterName, ITypeSymbol target)
    {
        // Special types (e.g. System.IDisposable) are matched directly via their SpecialType.
        if (target.SpecialType != SpecialType.None)
        {
            return new SpecialTypeImplementationInfo(parameterName, target.SpecialType.ToString());
        }

        // Otherwise we walk the type (and any containing types) plus the containing namespaces.
        if (target is not INamedTypeSymbol namedTarget)
        {
            return null;
        }

        var (typeChain, namespaces) = BuildNamedTypeShape(namedTarget);
        return new NamedTypeImplementationInfo(parameterName, typeChain, namespaces);
    }

    private static (EquatableArray<TypeSegment> TypeChain, EquatableArray<string> Namespaces) BuildNamedTypeShape(
        INamedTypeSymbol named
    )
    {
        var typeChain = ImmutableArray.CreateBuilder<TypeSegment>();
        for (var current = named; current is not null; current = current.ContainingType)
        {
            typeChain.Add(new TypeSegment(current.Name, current.Arity, BuildTypeArguments(current)));
        }

        var namespaces = ImmutableArray.CreateBuilder<string>();
        for (
            var current = named.ContainingNamespace;
            current is { IsGlobalNamespace: false };
            current = current.ContainingNamespace
        )
        {
            namespaces.Add(current.Name);
        }

        return (typeChain.ToEquatableArray(), namespaces.ToEquatableArray());
    }

    private static EquatableArray<ITypeArgument> BuildTypeArguments(INamedTypeSymbol named)
    {
        // An unbound generic (e.g. 'typeof(Task<>)') names no concrete arguments -- its "arguments" are placeholders
        // for the type parameters -- so it places no constraint and matches any instantiation of that arity.
        if (named.IsUnboundGenericType || named.TypeArguments.Length == 0)
        {
            return EquatableArray<ITypeArgument>.Empty;
        }

        var arguments = ImmutableArray.CreateBuilder<ITypeArgument>(named.TypeArguments.Length);
        foreach (var argument in named.TypeArguments)
        {
            arguments.Add(BuildTypeArgument(argument));
        }

        return arguments.ToEquatableArray();
    }

    private static ITypeArgument BuildTypeArgument(ITypeSymbol argument)
    {
        // A type parameter -- or the error-typed placeholder an unbound generic exposes for its arguments -- places no
        // constraint on the argument.
        if (argument.TypeKind == TypeKind.TypeParameter || argument is IErrorTypeSymbol)
        {
            return new TypeParameterArgument();
        }

        // Special types (e.g. string) are matched directly via their SpecialType.
        if (argument.SpecialType != SpecialType.None)
        {
            return new SpecialTypeArgument(argument.SpecialType.ToString());
        }

        // Named types recurse so nested generics (e.g. List<string> in Task<List<string>>) are constrained too.
        if (argument is INamedTypeSymbol namedArgument)
        {
            var (typeChain, namespaces) = BuildNamedTypeShape(namedArgument);
            return new NamedTypeArgument(typeChain, namespaces);
        }

        // Anything else (arrays, pointers, ...) is not expressible as a closed attribute argument; leave it open.
        return new TypeParameterArgument();
    }

    private static string BuildSignature(IMethodSymbol method, IParameterSymbol parameter, bool addNotNullWhenTrue)
    {
        var builder = new StringBuilder();
        builder.AppendMemberAccessibility(method).AppendMemberModifiers(method);

        builder.Append("partial bool ").Append(method.Name).Append('(');

        if (addNotNullWhenTrue)
        {
            builder.Append("[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] ");
        }

        if (method.IsExtensionMethod)
        {
            builder.Append("this ");
        }

        builder
            .Append(parameter.Type.ToDisplayString(parameterTypeFormat))
            .Append(' ')
            .Append(parameter.Name)
            .Append(')');

        return builder.ToString();
    }

    private static EquatableArray<string> BuildContainingTypeDeclarations(
        IMethodSymbol method,
        out ImmutableArray<string> containingTypeNames
    )
    {
        var declarations = ImmutableArray.CreateBuilder<string>();
        var names = ImmutableArray.CreateBuilder<string>();

        for (var current = method.ContainingType; current is not null; current = current.ContainingType)
        {
            declarations.Add(current.ToPartialTypeDeclaration());
            names.Add(current.Name);
        }

        declarations.Reverse();
        names.Reverse();

        containingTypeNames = names.ToImmutable();
        return declarations.ToEquatableArray();
    }

    private static string BuildHintName(
        string? @namespace,
        ImmutableArray<string> containingTypeNames,
        string methodName
    )
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(@namespace))
        {
            builder.Append(@namespace).Append('.');
        }

        foreach (var name in containingTypeNames)
        {
            builder.Append(name).Append('.');
        }

        builder.Append(methodName).Append(".g.cs");
        return builder.ToString();
    }
}
