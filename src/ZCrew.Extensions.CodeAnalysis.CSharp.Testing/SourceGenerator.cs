using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

internal class SourceGenerator<TSourceGenerator> : Generator
    where TSourceGenerator : ISourceGenerator, new()
{
    internal override Type SourceGeneratorType => typeof(TSourceGenerator);

    internal override IIncrementalGenerator CreateIncrementalGenerator()
    {
        return new TSourceGenerator().AsIncrementalGenerator();
    }
}
