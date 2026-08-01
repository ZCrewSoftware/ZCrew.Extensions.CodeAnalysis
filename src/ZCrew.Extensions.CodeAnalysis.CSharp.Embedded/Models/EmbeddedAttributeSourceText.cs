namespace ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.Models;

internal readonly record struct EmbeddedAttributeSourceText(
    string Name,
    string Namespace,
    int Arity,
    string SourceText
);
