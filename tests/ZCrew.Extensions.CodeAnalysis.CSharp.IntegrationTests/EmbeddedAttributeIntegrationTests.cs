using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IntegrationTests;

public class EmbeddedAttributeIntegrationTests
{
    // Microsoft.CodeAnalysis.EmbeddedAttribute is emitted into consuming compilations by the generator's
    // post-initialization output; the stand-in compilation gets a hand-rolled copy so the captured text resolves.
    private const string EmbeddedAttributeShim = """
        namespace Microsoft.CodeAnalysis;

        internal sealed class EmbeddedAttribute : System.Attribute { }
        """;

    private const string ConsumerSource = """
        using System;
        using ZCrew.Extensions.CodeAnalysis.CSharp.IntegrationTests;

        namespace Consumer;

        [Service(typeof(string))]
        internal class MinimalService { }

        [Service(typeof(string), typeof(int), new[] { "alpha", "beta" }, Name = "widget")]
        internal class FullService { }

        [Service<string, int>("generic")]
        internal class GenericService { }

        // Same arity and same constructor argument shape as ServiceAttribute's first overload; only the metadata
        // name tells them apart.
        internal class DecoyAttribute : Attribute
        {
            public DecoyAttribute(Type serviceType) { }
        }

        [Decoy(typeof(string))]
        internal class DecoyService { }

        [Overload("named")]
        internal class OverloadByString { }

        [Overload(typeof(string))]
        internal class OverloadByType { }

        [Overload(42)]
        internal class OverloadByInt { }

        [Note(null, "boxed")]
        internal class NullAndObject { }

        // Applied through ServiceAttribute's internal constructor, which the generated matcher does not carry.
        [Service(7L)]
        internal class InternalCtorService { }

        [Note("first", "a")]
        [Note("second", "b")]
        internal class TwoNotes { }
        """;

    private static readonly CSharpCompilation Compilation = CreateCompilation();

    [Fact]
    public void EmbeddedSourceText_CompilesInTheConsumer()
    {
        // Warnings count too: the compilation enables nullable, so anything short of this lets a badly annotated
        // emission through.
        var diagnostics = Compilation
            .GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .ToArray();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void TryGetServiceAttributeData_BindsTheShortestConstructor()
    {
        Assert.True(AttributeOn("Consumer.MinimalService").TryGetServiceAttributeData(out var data));

        Assert.Equal(SpecialType.System_String, data.ServiceType.SpecialType);
        Assert.Null(data.ImplementationType);
        Assert.Empty(data.Tags);
        Assert.Null(data.Name);
    }

    [Fact]
    public void TryGetServiceAttributeData_BindsArraysTypesAndNamedArguments()
    {
        Assert.True(AttributeOn("Consumer.FullService").TryGetServiceAttributeData(out var data));

        Assert.Equal(SpecialType.System_String, data.ServiceType.SpecialType);
        Assert.Equal(SpecialType.System_Int32, data.ImplementationType?.SpecialType);
        Assert.Equal(["alpha", "beta"], data.Tags);
        Assert.Equal("widget", data.Name);
    }

    [Fact]
    public void TryGetServiceAttributeData_2_BindsTypeArguments()
    {
        Assert.True(AttributeOn("Consumer.GenericService").TryGetServiceAttributeData_2(out var data));

        Assert.Equal(SpecialType.System_String, data.Service.SpecialType);
        Assert.Equal(SpecialType.System_Int32, data.Implementation.SpecialType);
        Assert.Equal("generic", data.Name);
    }

    [Fact]
    public void TryGetServiceAttributeData_RejectsAnotherAttributeOfTheSameShape()
    {
        Assert.False(AttributeOn("Consumer.DecoyService").TryGetServiceAttributeData(out var data));

        Assert.Null(data);
    }

    [Fact]
    public void TryGetServiceAttributeData_RejectsTheGenericOverload()
    {
        Assert.False(AttributeOn("Consumer.GenericService").TryGetServiceAttributeData(out var nonGeneric));
        Assert.Null(nonGeneric);

        Assert.False(AttributeOn("Consumer.MinimalService").TryGetServiceAttributeData_2(out var generic));
        Assert.Null(generic);
    }

    [Fact]
    public void TryGetServiceAttributeData_RejectsAUsageOfANonPublicConstructor()
    {
        Assert.False(AttributeOn("Consumer.InternalCtorService").TryGetServiceAttributeData(out var data));

        Assert.Null(data);
    }

    [Fact]
    public void TryGetOverloadAttributeData_PicksTheOverloadMatchingTheArgumentType()
    {
        Assert.True(AttributeOn("Consumer.OverloadByString").TryGetOverloadAttributeData(out var byString));
        Assert.Equal("named", byString.Name);
        Assert.Null(byString.Type);

        Assert.True(AttributeOn("Consumer.OverloadByType").TryGetOverloadAttributeData(out var byType));
        Assert.Equal(SpecialType.System_String, byType.Type?.SpecialType);
        Assert.Null(byType.Name);

        Assert.True(AttributeOn("Consumer.OverloadByInt").TryGetOverloadAttributeData(out var byInt));
        Assert.Equal(42, byInt.Value);
        Assert.Null(byInt.Name);
        Assert.Null(byInt.Type);
    }

    [Fact]
    public void TryGetNoteAttributeData_AcceptsNullAndObjectTypedArguments()
    {
        Assert.True(AttributeOn("Consumer.NullAndObject").TryGetNoteAttributeData(out var data));

        Assert.Null(data.Text);
        Assert.Equal("boxed", data.Payload);
    }

    [Fact]
    public void ForServiceAttributeData_YieldsOnlyTheNonGenericTargets()
    {
        var targets = RunProvider(provider => provider.ForServiceAttributeData(MatchAll, TargetName));

        Assert.Equal(["FullService", "MinimalService"], targets.Order());
    }

    [Fact]
    public void ForServiceAttributeData_2_YieldsOnlyTheGenericTargets()
    {
        var targets = RunProvider(provider => provider.ForServiceAttributeData_2(MatchAll, TargetName));

        Assert.Equal(["GenericService"], targets);
    }

    [Fact]
    public void ForServiceAttributeData_HonoursThePredicate()
    {
        var targets = RunProvider(provider => provider.ForServiceAttributeData(static (_, _) => false, TargetName));

        Assert.Empty(targets);
    }

    [Fact]
    public void ForServiceAttributeData_SkipsTargetsNoConstructorMatches()
    {
        var targets = RunProvider(provider => provider.ForServiceAttributeData(MatchAll, TargetName));

        Assert.DoesNotContain("InternalCtorService", targets);
    }

    [Fact]
    public void ForNoteAttributeData_YieldsEveryAttributeApplication()
    {
        var results = RunProvider(provider =>
            provider.ForNoteAttributeData(
                MatchAll,
                static (context, data, _) => (Target: context.TargetSymbol.Name, Notes: data)
            )
        );

        var notes = Assert.Single(results, result => result.Target == "TwoNotes").Notes;

        Assert.Equal(2, notes.Length);
        Assert.Equal("first", notes[0].Text);
        Assert.Equal("a", notes[0].Payload);
        Assert.Equal("second", notes[1].Text);
        Assert.Equal("b", notes[1].Payload);
    }

    // Accept every syntax node the provider offers; narrowing is what ForServiceAttributeData_HonoursThePredicate covers.
    private static bool MatchAll(SyntaxNode node, CancellationToken cancellationToken)
    {
        return true;
    }

    // The transform every target-name assertion shares: project the annotated symbol's name.
    private static string TargetName<TData>(
        GeneratorAttributeSyntaxContext context,
        ImmutableArray<TData> data,
        CancellationToken cancellationToken
    )
    {
        return context.TargetSymbol.Name;
    }

    private static IReadOnlyList<T> RunProvider<T>(Func<SyntaxValueProvider, IncrementalValuesProvider<T>> register)
    {
        var probe = new AttributeProviderProbe<T>(register);

        CSharpGeneratorDriver
            .Create(probe.AsSourceGenerator())
            .RunGenerators(Compilation, TestContext.Current.CancellationToken);

        return probe.Results;
    }

    private static AttributeData AttributeOn(string fullyQualifiedMetadataName)
    {
        var symbol =
            Compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
            ?? throw new InvalidOperationException($"Could not resolve the type '{fullyQualifiedMetadataName}'.");

        return symbol.GetAttributes().Single();
    }

    private static CSharpCompilation CreateCompilation()
    {
        var trustedAssemblies = (string)(
            AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("The trusted platform assemblies list is unavailable.")
        );

        var references = trustedAssemblies
            .Split(Path.PathSeparator)
            .Where(path => path.Length > 0)
            .Select(MetadataReference (path) => MetadataReference.CreateFromFile(path));

        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        SyntaxTree[] syntaxTrees =
        [
            CSharpSyntaxTree.ParseText(EmbeddedAttributeShim, parseOptions),
            // The attribute definition exactly as the generator would emit it into a consuming project
            CSharpSyntaxTree.ParseText(ServiceAttributeSourceText.SourceText, parseOptions),
            CSharpSyntaxTree.ParseText(ConsumerSource, parseOptions),
        ];

        return CSharpCompilation.Create(
            "EmbeddedAttributeIntegrationTests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable
            )
        );
    }
}
