using Microsoft.CodeAnalysis.Testing;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;

internal static class GeneratorTest
{
    /// <summary>
    ///     The shared baseline for all generator tests. The builder is immutable, so per-test specialization via
    ///     <c>With*</c> calls forks a new builder and never affects this instance.
    /// </summary>
    public static readonly SourceGeneratorTestBuilder<EmbeddedAttributeIncrementalGenerator, DefaultVerifier> Baseline =
        SourceGeneratorTestBuilder<EmbeddedAttributeIncrementalGenerator>
            .Create()
            .WithReferenceAssemblies(ReferenceAssemblies.Net.Net100)
            // All tests will inevitably use this project
            .WithAdditionalReferences("Microsoft.CodeAnalysis.dll")
            .WithAdditionalReferences("Microsoft.CodeAnalysis.CSharp.dll")
            .WithAdditionalReferences("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")
            .WithAdditionalReferences("ZCrew.Extensions.CodeAnalysis.CSharp.dll")
            .WithCompilerDiagnostics(CompilerDiagnostics.All)
            // Disable the warning on the source files about missing XML comments
            .WithDisabledDiagnostics("CS1591")
            // All tests will emit the post-initialization sources (e.g. 'Microsoft.CodeAnalysis.EmbeddedAttribute')
            .WithGeneratorPostInitializationSources();
}
