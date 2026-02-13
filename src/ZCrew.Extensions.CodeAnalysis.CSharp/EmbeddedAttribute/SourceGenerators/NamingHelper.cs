namespace ZCrew.Extensions.CodeAnalysis.CSharp.EmbeddedAttribute.SourceGenerators;

internal static class NamingHelper
{
    /// <summary>
    ///     Converts a camelCase name to PascalCase by capitalizing the first letter.
    /// </summary>
    public static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (char.IsUpper(name[0]))
        {
            return name;
        }

        return char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>
    ///     Converts a PascalCase name to camelCase by lowering the first letter.
    /// </summary>
    public static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (char.IsLower(name[0]))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>
    ///     Strips the "T" prefix from a type parameter name when it precedes an uppercase letter.
    ///     For example, "TServiceType" becomes "ServiceType", but "T" stays as "T".
    /// </summary>
    public static string StripTypeParameterPrefix(string name)
    {
        if (name.Length > 1 && name[0] == 'T' && char.IsUpper(name[1]))
        {
            return name.Substring(1);
        }

        return name;
    }
}
