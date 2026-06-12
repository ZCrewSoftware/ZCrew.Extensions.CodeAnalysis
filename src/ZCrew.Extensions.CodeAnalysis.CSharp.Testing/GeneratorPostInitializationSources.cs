using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Captures the sources a generator emits for an empty compilation (i.e. its post-initialization sources),
///     once per generator type.
/// </summary>
internal static class GeneratorPostInitializationSources<TGenerator>
    where TGenerator : new()
{
    // ReSharper disable once StaticMemberInGenericType - this is meant to be scoped per generator type
    private static readonly Lazy<ImmutableArray<(string HintName, SourceText Content)>> lazySources = new(
        Capture,
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    /// <summary>
    ///     The sources a generator emits using <see cref="IncrementalGeneratorPostInitializationContext"/>. These are
    ///     typically static (unchanged from run-to-run) and can be cached here for most generators.
    /// </summary>
    public static ImmutableArray<(string HintName, SourceText Content)> Sources => lazySources.Value;

    private static ImmutableArray<(string HintName, SourceText Content)> Capture()
    {
        var generator = new TGenerator() switch
        {
            IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
            ISourceGenerator source => source,
            var other => throw new InvalidOperationException(
                $"'{other!.GetType()}' is neither an IIncrementalGenerator nor an ISourceGenerator."
            ),
        };

        // With no syntax trees, the only outputs a generator can produce are its post-initialization sources
        var compilation = CSharpCompilation.Create(
            "ZCrew.PostInitializationCapture",
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult();
        return runResult.Results[0].GeneratedSources.Select(s => (s.HintName, s.SourceText)).ToImmutableArray();
    }
}
