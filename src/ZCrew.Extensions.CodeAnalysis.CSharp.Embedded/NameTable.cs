using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded;

internal class NameTable
{
    private const string TypeParameterName = "TypeParameter";

    /// <summary>
    ///     A type name no real symbol can produce for reversing names.
    /// </summary>
    private const string ReservedTypeName = "<reserved>";

    /// <summary>
    ///     Names the generated types already occupy and cannot give up. The library's own members are
    ///     <see langword="internal"/> so a generated subclass may reuse them, but these cannot be hidden: the
    ///     <see cref="object"/> members are inherited by every generated type, and the record types on the model.
    /// </summary>
    private static readonly string[] ReservedNames =
    [
        "Equals",
        "Finalize",
        "GetHashCode",
        "GetType",
        "MemberwiseClone",
        "ToString",
        "EqualityContract",
        "PrintMembers",
    ];

    private readonly Dictionary<string, string> names = [];

    public NameTable()
    {
        foreach (var reserved in ReservedNames)
        {
            this.names.Add(reserved, ReservedTypeName);
        }
    }

    public string GetTypeParameterName(ITypeParameterSymbol typeParameter)
    {
        var strippedName = StripTypeParameterPrefix(typeParameter.Name);
        return ReserveName(strippedName, TypeParameterName);
    }

    public string GetParameterName(IParameterSymbol parameter)
    {
        var pascalName = ToPascalCase(parameter.Name);
        return ReserveName(pascalName, GetTypeName(parameter.Type));
    }

    public string GetPropertyName(IPropertySymbol property)
    {
        return ReserveName(property.Name, GetTypeName(property.Type));
    }

    private string GetTypeName(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return GetTypeName(array.ElementType) + "Array" + array.Rank;
        }

        return type.Name;
    }

    private string ReserveName(string name, string typeName)
    {
        if (TryReserve(name, typeName))
        {
            return SymbolHelpers.EscapeIdentifier(name);
        }

        var nameWithType = $"{name}{typeName}";
        if (TryReserve(nameWithType, typeName))
        {
            return SymbolHelpers.EscapeIdentifier(nameWithType);
        }

        var suffix = 1;
        while (true)
        {
            var suffixedName = $"{name}{suffix}";
            if (TryReserve(suffixedName, typeName))
            {
                return SymbolHelpers.EscapeIdentifier(suffixedName);
            }
            suffix++;
        }
    }

    private bool TryReserve(string name, string type)
    {
        if (this.names.TryGetValue(name, out var existingType))
        {
            return existingType == type;
        }

        this.names.Add(name, type);
        return true;
    }

    /// <summary>
    ///     Converts a camelCase name to PascalCase by capitalizing the first letter.
    /// </summary>
    private static string ToPascalCase(string name)
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
    ///     Strips the "T" prefix from a type parameter name when it precedes an uppercase letter.
    ///     For example, "TServiceType" becomes "ServiceType", but "T" stays as "T".
    /// </summary>
    private static string StripTypeParameterPrefix(string name)
    {
        if (name.Length > 1 && name[0] == 'T' && char.IsUpper(name[1]))
        {
            return name.Substring(1);
        }

        return name;
    }
}
