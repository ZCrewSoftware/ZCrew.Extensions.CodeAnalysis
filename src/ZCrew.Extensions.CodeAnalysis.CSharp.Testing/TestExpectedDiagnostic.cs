using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

/// <summary>
///     Describes a diagnostic the test expects the compilation or generator to report. Where the diagnostic is
///     declared determines its location: declared on <see cref="ITestCase.ExpectedDiagnostics" /> it is expected to
///     have no location (<see cref="Location.None" />); declared on a <see cref="TestSourceFile" /> or
///     <see cref="TestGeneratedFile" /> it is expected in that file, located by <see cref="Snippet" /> or by an
///     explicit <see cref="Line" /> and <see cref="Column" />.
/// </summary>
public class TestExpectedDiagnostic
{
    /// <summary>
    ///     The diagnostic id, for example a compiler code such as <c>"CS0246"</c> or a generator-authored id such as
    ///     <c>"ZC1001"</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     The severity of the diagnostic. Defaults to <see cref="DiagnosticSeverity.Error" />. In JSON this accepts the
    ///     name of a <see cref="DiagnosticSeverity" /> value (for example <c>"Error"</c> or <c>"Warning"</c>).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<DiagnosticSeverity>))]
    public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Error;

    /// <summary>
    ///     A code snippet to locate within the containing file. The start of its single occurrence becomes the
    ///     diagnostic location. Mutually exclusive with <see cref="Line" /> and <see cref="Column" />. Only applies when
    ///     the diagnostic is declared on a <see cref="TestSourceFile" /> or <see cref="TestGeneratedFile" />.
    /// </summary>
    public string? Snippet { get; set; }

    /// <summary>
    ///     The explicit one-based start line of the diagnostic within the containing file, used instead of
    ///     <see cref="Snippet" />. Must be paired with <see cref="Column" />.
    /// </summary>
    public int? Line { get; set; }

    /// <summary>
    ///     The explicit one-based start column of the diagnostic within the containing file, used instead of
    ///     <see cref="Snippet" />. Must be paired with <see cref="Line" />.
    /// </summary>
    public int? Column { get; set; }

    /// <summary>
    ///     An optional exact message to assert against the reported diagnostic. When set, the reported message must
    ///     equal this value (after <c>$(name)</c> variable expansion); when <see langword="null" /> the message is not
    ///     verified.
    /// </summary>
    public string? Message { get; set; }
}
