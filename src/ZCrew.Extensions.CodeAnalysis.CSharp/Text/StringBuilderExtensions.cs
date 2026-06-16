using System.Text;
using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Text;

/// <summary>
///     Extensions for <see cref="StringBuilder"/>.
/// </summary>
public static class StringBuilderExtensions
{
    extension(StringBuilder builder)
    {
        /// <summary>
        ///     Appends the member accessibility to this <see cref="StringBuilder"/>. This ends with a trailing space
        ///     unless the accessibility is blank or unknown.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>This same <see cref="String"/> for chaining calls.</returns>
        public StringBuilder AppendMemberAccessibility(ISymbol symbol)
        {
            return symbol.DeclaredAccessibility switch
            {
                Accessibility.Private => builder.Append("private "),
                Accessibility.Internal => builder.Append("internal "),
                Accessibility.ProtectedAndInternal => builder.Append("private protected "),
                Accessibility.Protected => builder.Append("protected "),
                Accessibility.ProtectedOrInternal => builder.Append("protected internal "),
                Accessibility.Public => builder.Append("public "),
                _ => builder,
            };
        }

        /// <summary>
        ///     Appends the member modifiers to this <see cref="StringBuilder"/>. This ends with a trailing space unless
        ///     there were no modifiers.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public StringBuilder AppendMemberModifiers(ISymbol symbol)
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

            return builder;
        }
    }
}
