using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.TestDoubles;

/// <summary>
///     An analyzer that reports <see cref="Id" /> on every type declaration whose name starts with "Marked". No
///     <c>[DiagnosticAnalyzer]</c> attribute: the test harness is handed the instance directly, and the attribute
///     would trip the Roslyn analyzer rules for real analyzer assemblies.
/// </summary>
internal sealed class MarkerAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "ZCTEST001";

    private static readonly DiagnosticDescriptor Descriptor = new(
        Id,
        "Marked type",
        "Type '{0}' is marked",
        "Testing",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(
            symbolContext =>
            {
                var symbol = symbolContext.Symbol;
                if (!symbol.Name.StartsWith("Marked", StringComparison.Ordinal))
                {
                    return;
                }

                symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));
            },
            SymbolKind.NamedType
        );
    }
}

/// <summary>
///     A second analyzer that reports nothing. Used to verify multiple analyzers can be registered.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SilentAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        "ZCTEST002",
        "Never reported",
        "Never reported",
        "Testing",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
