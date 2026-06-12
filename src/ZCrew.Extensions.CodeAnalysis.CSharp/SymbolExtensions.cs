using System.Text;
using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Extensions for <see cref="ISymbol"/>.
/// </summary>
public static class SymbolExtensions
{
    private static readonly SymbolDisplayFormat genericTypeFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
    );

    private static readonly SymbolDisplayFormat nonGenericTypeFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.None,
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
    );

    private static readonly SymbolDisplayFormat methodDeclarationPostPartialFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
        memberOptions: SymbolDisplayMemberOptions.IncludeType
            | SymbolDisplayMemberOptions.IncludeExplicitInterface
            | SymbolDisplayMemberOptions.IncludeParameters
            | SymbolDisplayMemberOptions.IncludeRef,
        parameterOptions: SymbolDisplayParameterOptions.IncludeExtensionThis
            | SymbolDisplayParameterOptions.IncludeModifiers
            | SymbolDisplayParameterOptions.IncludeType
            | SymbolDisplayParameterOptions.IncludeName
            | SymbolDisplayParameterOptions.IncludeDefaultValue
    );

    private static readonly SymbolDisplayFormat classDeclarationFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword
    );

    extension(ISymbol symbol)
    {
        /// <summary>
        ///     Gets a string representation of a <see cref="INamedTypeSymbol"/> with generic information but without
        ///     the global namespace qualifier. This will present the way most C# developers will write a type.
        /// </summary>
        /// <returns>The type name.</returns>
        /// <example>
        ///     <list type="numbered">
        ///         <item>
        ///             <description><c>System.Collections.Generic.List&lt;string&gt;</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>System.Tuple&lt;string, object&gt;</c></description>
        ///         </item>
        ///     </list>
        /// </example>
        public string ToGenericTypeName(bool globalUsings = false)
        {
            var options = genericTypeFormat;
            if (globalUsings)
            {
                options = options.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);
            }

            return symbol.ToDisplayString(options);
        }

        /// <summary>
        ///     Gets a string representation of a <see cref="INamedTypeSymbol"/> without generic information and without
        ///     the global namespace qualifier.
        /// </summary>
        /// <remarks>
        ///     This can be useful for types with a varying number of generic type parameters, such as
        ///     <see cref="Tuple"/>.
        /// </remarks>
        /// <returns>The type name.</returns>
        /// <example>
        ///     <list type="numbered">
        ///         <item>
        ///             <description><c>System.Collections.Generic.List</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>System.Tuple</c></description>
        ///         </item>
        ///     </list>
        /// </example>
        public string ToNonGenericTypeName(bool globalUsings = false)
        {
            var options = nonGenericTypeFormat;
            if (globalUsings)
            {
                options = options.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);
            }

            return symbol.ToDisplayString(options);
        }

        /// <summary>
        ///     Gets a string representation of a <see cref="INamedTypeSymbol"/> as an open generic type and with the
        ///     global namespace qualifier.
        /// </summary>
        /// <returns>The type name.</returns>
        /// <example>
        ///     <list type="numbered">
        ///         <item>
        ///             <description><c>global::System.Collections.Generic.List&lt;&gt;</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>global::System.Tuple&lt;,&gt;</c></description>
        ///         </item>
        ///     </list>
        /// </example>
        public string ToOpenGenericTypeName(bool globalUsings = false)
        {
            var options = nonGenericTypeFormat;
            if (globalUsings)
            {
                options = options.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);
            }

            var nonGenericTypeName = symbol.ToDisplayString(options);

            // If this is a type symbol, the consider adding open generic parameters
            if (symbol.Kind == SymbolKind.NamedType)
            {
                // Only add in the open generics if they are present
                var namedTypeSymbol = (INamedTypeSymbol)symbol;
                if (namedTypeSymbol.Arity > 0)
                {
                    // For N type parameters we need N-1 commas
                    var commas = Enumerable.Repeat(',', namedTypeSymbol.Arity - 1).ToArray();

                    // Creates type names like "Dictionary<,>"
                    return $"{nonGenericTypeName}<{new string(commas)}>";
                }
            }

            return nonGenericTypeName;
        }

        /// <summary>
        ///     Generates a single-line partial method declaration used to provide an implementation of a partial
        ///     method.
        /// </summary>
        /// <returns>The partial method declaration.</returns>
        /// <example>
        ///     <code>
        ///         public static partial void Print(global::System.Collections.Generic.IEnumerable values)
        ///     </code>
        /// </example>
        public string ToPartialMethodDeclaration()
        {
            var stringBuilder = new StringBuilder();

            // Purposefully do these manually so that the 'partial' modifier can be added.
            // It isn't included in 'ToDisplayString' for some reason.
            AddMemberAccessibility(symbol, stringBuilder);
            AddMemberModifiers(symbol, stringBuilder);

            // Always add partial. If the user has not marked their method as partial, then this will cause an error so
            // they are forced to add it.
            stringBuilder.Append("partial ");

            // Add the method name, parameters, constraints, etc.
            stringBuilder.Append(symbol.ToDisplayString(methodDeclarationPostPartialFormat));
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Generates a single-line partial class declaration used to provide a partial class part. Accessibility,
        ///     modifiers, generic types, and generic type constraints are not included as they are not necessary.
        /// </summary>
        /// <returns>The partial class declaration.</returns>
        /// <example>
        ///     <code>
        ///         partial class EnumerableExtensions
        ///     </code>
        /// </example>
        public string ToPartialClassDeclaration()
        {
            var stringBuilder = new StringBuilder();

            // Always add partial. If the user has not marked their method as partial, then this will cause an error so
            // they  are forced to add it.
            stringBuilder.Append("partial ");

            // Add type name, type parameters, etc.
            stringBuilder.Append(symbol.ToDisplayString(classDeclarationFormat));
            return stringBuilder.ToString();
        }
    }

    private static void AddMemberAccessibility(ISymbol symbol, StringBuilder builder)
    {
        switch (symbol.DeclaredAccessibility)
        {
            case Accessibility.Private:
                builder.Append("private ");
                break;
            case Accessibility.Internal:
                builder.Append("internal ");
                break;
            case Accessibility.ProtectedAndInternal:
                builder.Append("private protected ");
                break;
            case Accessibility.Protected:
                builder.Append("protected ");
                break;
            case Accessibility.ProtectedOrInternal:
                builder.Append("protected internal ");
                break;
            case Accessibility.Public:
                builder.Append("public ");
                break;
        }
    }

    private static void AddMemberModifiers(ISymbol symbol, StringBuilder builder)
    {
        if (symbol.IsStatic)
        {
            builder.Append("static ");
        }

        if (symbol.IsOverride)
        {
            builder.Append("override ");
        }

        if (symbol.IsAbstract)
        {
            builder.Append("abstract ");
        }

        if (symbol.IsSealed)
        {
            builder.Append("sealed ");
        }

        if (symbol.IsExtern)
        {
            builder.Append("extern ");
        }

        if (symbol.IsVirtual)
        {
            builder.Append("virtual ");
        }
    }
}
