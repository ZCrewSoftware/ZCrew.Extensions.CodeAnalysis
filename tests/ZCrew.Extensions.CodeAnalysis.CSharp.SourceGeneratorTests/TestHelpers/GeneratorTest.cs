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
            .WithGeneratorPostInitializationSources()
            // Overwrite mismatched/missing expected files in place for review, but never on CI (writes only, the
            // assertion still runs and fails regardless).
            .WithExpectedSourceUpdates(enabled: Environment.GetEnvironmentVariable("CI") is null);

    /// <summary>
    ///     The shared baseline for <see cref="IsTypeIncrementalGenerator" /> tests. Mirrors <see cref="Baseline" /> but
    ///     drives the IsType generator, whose only post-initialization source is the <c>IsTypeAttribute</c> definition.
    /// </summary>
    public static readonly SourceGeneratorTestBuilder<IsTypeIncrementalGenerator, DefaultVerifier> IsTypeBaseline =
        SourceGeneratorTestBuilder<IsTypeIncrementalGenerator>
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
            // All tests will emit the post-initialization sources (e.g. the 'IsTypeAttribute' definition)
            .WithGeneratorPostInitializationSources()
            // Overwrite mismatched/missing expected files in place for review, but never on CI (writes only, the
            // assertion still runs and fails regardless).
            .WithExpectedSourceUpdates(enabled: Environment.GetEnvironmentVariable("CI") is null);
}
