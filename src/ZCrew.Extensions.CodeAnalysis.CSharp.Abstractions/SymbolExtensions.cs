using System.Text;
using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Extensions for <see cref="ISymbol"/>.
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    ///     Fully-qualified format including global namespaces, nullable annotations, used for emitted signature text.
    /// </summary>
    private static readonly SymbolDisplayFormat GlobalNameEmit =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
                | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    /// <summary>
    ///     Fully-qualified format including nullable annotations, used for emitted signature text.
    /// </summary>
    private static readonly SymbolDisplayFormat FullyQualifiedNameEmit = GlobalNameEmit.WithGlobalNamespaceStyle(
        SymbolDisplayGlobalNamespaceStyle.Omitted
    );

    /// <summary>
    ///     Fully-qualified format including global namespaces, without nullable annotations at any nesting level,
    ///     used to match a declared type against a <see cref="TypedConstant.Type"/>.
    /// </summary>
    private static readonly SymbolDisplayFormat GlobalNameMatch = SymbolDisplayFormat.FullyQualifiedFormat;

    /// <summary>
    ///     Fully-qualified format without nullable annotations, used for type matching.
    /// </summary>
    private static readonly SymbolDisplayFormat FullyQualifiedNameMatch = GlobalNameMatch.WithGlobalNamespaceStyle(
        SymbolDisplayGlobalNamespaceStyle.Omitted
    );

    private static readonly SymbolDisplayFormat MethodDeclarationPostPartialFormat = new(
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
            | SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    private static readonly SymbolDisplayFormat ClassDeclarationFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            | SymbolDisplayGenericsOptions.IncludeVariance,
        kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
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
        ///             <description><c>string</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>System.Collections.Generic.List&lt;string&gt;</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>System.Tuple&lt;string, object&gt;</c></description>
        ///         </item>
        ///     </list>
        /// </example>
        /// <param name="globalUsings">Whether to qualify the name with <c>global::</c>.</param>
        /// <param name="nullableAnnotations">
        ///     Whether to include nullable reference annotations. Pass <see langword="false"/> when comparing against
        ///     a <see cref="TypedConstant.Type"/>, which is never annotated.
        /// </param>
        public string ToFullyQualifiedName(bool globalUsings = false, bool nullableAnnotations = true)
        {
            var format = (globalUsings, nullableAnnotations) switch
            {
                (true, true) => GlobalNameEmit,
                (true, false) => GlobalNameMatch,
                (false, true) => FullyQualifiedNameEmit,
                (false, false) => FullyQualifiedNameMatch,
            };

            return symbol.ToDisplayString(format);
        }

        /// <summary>
        ///     Generates a single-line partial method declaration used to provide an implementation of a partial
        ///     method. This includes nullability modifiers and should be used in <c>#nullable enable</c> code.
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
            stringBuilder.AppendMemberAccessibility(symbol).AppendMemberModifiers(symbol);

            // Always add partial. If the user has not marked their method as partial, then this will cause an error so
            // they are forced to add it.
            stringBuilder.Append("partial ");

            // Add the method name, parameters, constraints, etc.
            stringBuilder.Append(symbol.ToDisplayString(MethodDeclarationPostPartialFormat));
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Matches a type's fully qualified metadata name against <paramref name="fullMetadataName"/>.
        /// </summary>
        /// <param name="fullMetadataName">The fully qualified metadata name.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="fullMetadataName"/> matches this <paramref name="symbol"/>'s
        ///     metadata name.
        /// </returns>
        /// <example>
        ///     <c>System.Collections.Generic.List`2</c>.
        /// </example>
        public bool HasFullMetadataName(string fullMetadataName)
        {
            var name = symbol.MetadataName;
            var namespaceLength = fullMetadataName.Length - name.Length;
            if (
                namespaceLength < 0
                || string.CompareOrdinal(fullMetadataName, namespaceLength, name, 0, name.Length) != 0
            )
            {
                return false;
            }

            if (namespaceLength == 0)
            {
                return symbol.ContainingNamespace.IsGlobalNamespace;
            }

            return fullMetadataName[namespaceLength - 1] == '.'
                && symbol.ContainingNamespace.ToDisplayString() == fullMetadataName[..(namespaceLength - 1)];
        }

        /// <summary>
        ///     Generates a single-line partial type declaration used to provide a partial type part. This is meant for
        ///     <see langword="class"/>, <see langword="record"/>, <see langword="struct"/> and
        ///     <see langword="interface"/> types. Accessibility, modifiers and generic type constraints are not
        ///     included as they are not necessary.
        /// </summary>
        /// <returns>The partial type declaration.</returns>
        /// <example>
        ///     <code>
        ///         partial class EnumerableExtensions
        ///     </code>
        /// </example>
        public string ToPartialTypeDeclaration()
        {
            var stringBuilder = new StringBuilder();

            // Always add partial. If the user has not marked their method as partial, then this will cause an error so
            // they  are forced to add it.
            stringBuilder.Append("partial ");

            // Add type name, type parameters, etc.
            stringBuilder.Append(symbol.ToDisplayString(ClassDeclarationFormat));
            return stringBuilder.ToString();
        }
    }
}
