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
///     An incremental generator that counts how many times it is constructed. Used to verify the post-initialization
///     capture caches per generator type. Reference it from exactly one test so <see cref="ConstructionCount" /> stays
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
///     A type that satisfies the <c>new()</c> constraint but is neither an <see cref="IIncrementalGenerator" /> nor an
///     <see cref="ISourceGenerator" />.
/// </summary>
internal sealed class NotAGenerator { }
