namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded;

/// <summary>
///     The generated type, method, and hint names for one embedded attribute. Built once per attribute and threaded
///     through the emitters so every name derives from a single definition.
/// </summary>
internal sealed class EmbeddedAttributeNames
{
    public EmbeddedAttributeNames(string name, string @namespace, int arity)
    {
        var aritySegment = arity > 0 ? $"_{arity}" : string.Empty;
        var separator = arity > 0 ? "_" : string.Empty;

        AttributeData = $"{name}Data{aritySegment}";
        AttributeExtensionMethod = $"TryGet{AttributeData}";
        SyntaxProviderExtensionMethod = $"For{AttributeData}";
        Constructor = $"{AttributeData}{separator}Constructor";
        Constructors = $"{Constructor}s";
        Parameter = $"{AttributeData}{separator}Parameter";
        TypeParameter = $"{AttributeData}{separator}TypeParameter";
        NamedParameter = $"{AttributeData}{separator}NamedParameter";
        NamedParameters = $"{NamedParameter}s";
        MetadataName = ToMetadataName(@namespace, name, arity);
        AttributeDataHintName = ToHintName(@namespace, AttributeData);
    }

    /// <summary>
    ///     The attribute data record.
    /// </summary>
    /// <example><c>TestAttributeData_2</c></example>
    public string AttributeData { get; }

    /// <summary>
    ///     The <see cref="AttributeData"/> extension method.
    /// </summary>
    /// <example><c>TryGetTestAttributeData_2</c></example>
    public string AttributeExtensionMethod { get; }

    /// <summary>
    ///     The <c>SyntaxProvider.ForAttributeWithMetadataName</c>-like extension method.
    /// </summary>
    /// <example><c>ForTestAttributeData_2</c></example>
    public string SyntaxProviderExtensionMethod { get; }

    /// <summary>
    ///     The constructor matcher.
    /// </summary>
    /// <example><c>TestAttributeData_2_Constructor</c></example>
    public string Constructor { get; }

    /// <summary>
    ///     The static field holding every <see cref="Constructor"/>.
    /// </summary>
    /// <example><c>TestAttributeData_2_Constructors</c></example>
    public string Constructors { get; }

    /// <summary>
    ///     The constructor parameter matcher.
    /// </summary>
    /// <example><c>TestAttributeData_2_Parameter</c></example>
    public string Parameter { get; }

    /// <summary>
    ///     The type parameter binder.
    /// </summary>
    /// <example><c>TestAttributeData_2_TypeParameter</c></example>
    public string TypeParameter { get; }

    /// <summary>
    ///     The named property binder.
    /// </summary>
    /// <example><c>TestAttributeData_2_NamedParameter</c></example>
    public string NamedParameter { get; }

    /// <summary>
    ///     The static field holding every <see cref="NamedParameter"/>.
    /// </summary>
    /// <example><c>TestAttributeData_2_NamedParameters</c></example>
    public string NamedParameters { get; }

    /// <summary>
    ///     The attribute's fully qualified metadata name, as <c>ForAttributeWithMetadataName</c> expects it. This names
    ///     the attribute itself rather than any generated type.
    /// </summary>
    /// <example><c>AttributeTests.TestAttribute`2</c></example>
    public string MetadataName { get; }

    /// <summary>
    ///     The hint name for the <see cref="AttributeData"/> source.
    /// </summary>
    /// <example><c>AttributeTests.TestAttributeData_2.g.cs</c></example>
    public string AttributeDataHintName { get; }

    private static string ToMetadataName(string @namespace, string attributeName, int arity)
    {
        var aritySegment = arity > 0 ? $"`{arity}" : string.Empty;
        return string.IsNullOrEmpty(@namespace)
            ? $"{attributeName}{aritySegment}"
            : $"{@namespace}.{attributeName}{aritySegment}";
    }

    private static string ToHintName(string @namespace, string typeName)
    {
        return string.IsNullOrEmpty(@namespace) ? $"{typeName}.g.cs" : $"{@namespace}.{typeName}.g.cs";
    }
}
