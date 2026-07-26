using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

/// <summary>
///     An incremental generator that produces no output.
/// </summary>
internal sealed class EmptyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) { }
}

/// <summary>
///     An incremental generator that registers a single, known post-initialization source.
/// </summary>
internal sealed class PostInitializationGenerator : IIncrementalGenerator
{
    public const string HintName = "Test.PostInit.g.cs";
    public const string Content = "// post-init";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(HintName, Content));
    }
}

/// <summary>
///     A second incremental generator registering its own post-initialization source, so tests can register two
///     generators at once and tell their output apart.
/// </summary>
internal sealed class SecondPostInitializationGenerator : IIncrementalGenerator
{
    public const string HintName = "Test.SecondPostInit.g.cs";
    public const string Content = "// second post-init";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(HintName, Content));
    }
}

/// <summary>
///     An incremental generator that counts how many times it is constructed. Used to verify the post-initialization
///     capture caches per generator. Reference it from exactly one test so <see cref="ConstructionCount" /> stays
///     isolated.
/// </summary>
internal sealed class CountingGenerator : IIncrementalGenerator
{
    public static int ConstructionCount;

    public CountingGenerator()
    {
        Interlocked.Increment(ref ConstructionCount);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource("Counting.g.cs", "// counting"));
    }
}

/// <summary>
///     A non-incremental <see cref="ISourceGenerator" />, covering the obsolete generator path. No
///     <c>[Generator]</c> attribute: the test harness is handed the type directly, and the attribute would trip the
///     Roslyn analyzer rules for real generator assemblies.
/// </summary>
#pragma warning disable CS0618 // ISourceGenerator is obsolete; that is exactly what this double exercises.
internal sealed class LegacySourceGenerator : ISourceGenerator
{
    public const string HintName = "Test.Legacy.g.cs";
    public const string Content = "// legacy";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(ctx => ctx.AddSource(HintName, Content));
    }

    public void Execute(GeneratorExecutionContext context) { }
}
#pragma warning restore CS0618
