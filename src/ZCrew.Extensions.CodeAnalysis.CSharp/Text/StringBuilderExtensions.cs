using System.Text;
using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Text;

public static class StringBuilderExtensions
{
    extension(StringBuilder builder)
    {
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
