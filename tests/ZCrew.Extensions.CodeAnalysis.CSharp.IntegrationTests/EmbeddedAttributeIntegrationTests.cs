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
        """;

    private static readonly CSharpCompilation Compilation = CreateCompilation();

    [Fact]
    public void EmbeddedSourceText_CompilesInTheConsumer()
    {
        var errors = Compilation
            .GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.Empty(errors);
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
        Assert.False(AttributeOn("Consumer.GenericService").TryGetServiceAttributeData(out _));
        Assert.False(AttributeOn("Consumer.MinimalService").TryGetServiceAttributeData_2(out _));
    }

    [Fact]
    public void TryGetOverloadAttributeData_PicksTheOverloadMatchingTheArgumentType()
    {
        Assert.True(AttributeOn("Consumer.OverloadByString").TryGetOverloadAttributeData(out var byString));
        Assert.Equal("named", byString.Name);
        Assert.Null(byString.Type);
        Assert.Equal(0, byString.Value);

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
