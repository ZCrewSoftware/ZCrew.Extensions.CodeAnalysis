using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

internal class IncrementalGenerator<TIncrementalGenerator> : Generator
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    internal override Type SourceGeneratorType => typeof(TIncrementalGenerator);

    internal override IIncrementalGenerator CreateIncrementalGenerator()
    {
        return new TIncrementalGenerator();
    }
}
