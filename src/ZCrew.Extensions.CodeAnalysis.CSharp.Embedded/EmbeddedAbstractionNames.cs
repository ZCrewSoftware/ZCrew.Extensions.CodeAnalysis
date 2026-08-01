using ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded;

/// <summary>
///     The generated type, method, and hint names for one embedded abstraction. Built once per abstraction and threaded
///     through the emitters so every name derives from a single definition.
/// </summary>
internal sealed class EmbeddedAbstractionNames
{
    public EmbeddedAbstractionNames(string name, string @namespace, int arity)
    {
        var aritySegment = arity > 0 ? $"_{arity}" : string.Empty;
        var separator = arity > 0 ? "_" : string.Empty;

        DefinitionMethod = $"Add{name}{aritySegment}{separator}Definition";
        SourceTextClass = $"{name}SourceText{aritySegment}";
        SourceTextHintName = ToHintName(@namespace, SourceTextClass);
    }

    /// <summary>
    ///     The method that adds the attribute definition.
    /// </summary>
    /// <example><c>AddTestAttribute_2_Definition</c></example>
    public string DefinitionMethod { get; }

    /// <summary>
    ///     The class carrying the attribute definition. Named for the class rather than the text so it does not read
    ///     as the <see cref="EmbeddedAttributeSourceText.SourceText"/> the class carries.
    /// </summary>
    /// <example><c>TestAttributeSourceText_2</c></example>
    public string SourceTextClass { get; }

    /// <summary>
    ///     The hint name for the <see cref="SourceTextClass"/> source.
    /// </summary>
    /// <example><c>AttributeTests.TestAttributeSourceText_2.g.cs</c></example>
    public string SourceTextHintName { get; }

    private static string ToHintName(string @namespace, string typeName)
    {
        return string.IsNullOrEmpty(@namespace) ? $"{typeName}.g.cs" : $"{@namespace}.{typeName}.g.cs";
    }
}
