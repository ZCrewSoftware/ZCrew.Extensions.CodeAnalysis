# ZCrew.Extensions.CodeAnalysis.CSharp.Testing

Testing library for verifying Roslyn source generators and diagnostic analyzers. It drives any number of them
through Roslyn's `AnalyzerTest` harness from a small JSON descriptor, so each test case is just a set of input and
expected-output files plus a `.json` that ties them together.

> **Preview.** The API is still settling and breaks between versions.

The assertion runs in both directions: every file the generators emit must be listed as an expected generated file,
and every file listed must be emitted. Listing none is therefore a real assertion — that the generators produce
nothing for those inputs.

## Installation

Available on NuGet for .NET Standard 2.0:

```xml
<PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp.Testing" />
```

## Writing a test case

A test case is three kinds of file in a `TestCases/` folder next to your test class:

- **Input sources** (e.g. `MyCase.Attribute.cs`) compiled as input to the generator.
- **Expected generated files** (e.g. `MyCase.SourceText.g.cs`) the output to verify against.
- **A JSON descriptor** (`MyCase.json`) maps the inputs, expected outputs, and any expected diagnostics:

```json
{
    "SourceFiles": [
        { "SourceFileName": "MyCase.Attribute.cs" }
    ],
    "GeneratedFiles": [
        {
            "SourceFileName": "MyCase.SourceText.g.cs",
            "GeneratedFileName": "$(MyGenerator)/MyNamespace.MyTypeSourceText.g.cs"
        }
    ]
}
```

`SourceFileName` is the file on disk. `GeneratedFileName` is the full path Roslyn emits the source under:
`{generator assembly name}/{generator full type name}/{hint name}`, where the hint name is what your generator
passes to `context.AddSource(hintName, ...)`.

Every registered generator contributes a variable named after its type, so `$(MyGenerator)` expands to its directory.

## Expecting diagnostics

Declare expected diagnostics where they occur:

- On a **source file** or **generated file** entry, an `ExpectedDiagnostics` array asserts diagnostics located in
  that file. Give each a location by `Snippet` (the start of its single occurrence in that file) or by an explicit
  1-based `Line`/`Column`.
- At the **top level**, an `ExpectedDiagnostics` array asserts diagnostics with no location (`Location.None`) — for
  example `CS8785` when a generator throws, or a compilation-level analyzer diagnostic.

Each entry has an `Id`, an optional `Severity` (defaults to `Error`), and an optional `Message` to match exactly.

```json
{
    "SourceFiles": [
        {
            "SourceFileName": "MyCase.Attribute.cs",
            "ExpectedDiagnostics": [
                { "Id": "CS0246", "Snippet": "CreateService<T>(" },
                { "Id": "CS0246", "Line": 10, "Column": 17 }
            ]
        }
    ],
    "GeneratedFiles": [
        {
            "SourceFileName": "MyCase.SourceText.g.cs",
            "GeneratedFileName": "$(MyGenerator)/MyNamespace.MyTypeSourceText.g.cs",
            "ExpectedDiagnostics": [
                { "Id": "CS0219", "Severity": "Warning", "Snippet": "unused" }
            ]
        }
    ],
    "ExpectedDiagnostics": [
        { "Id": "CS8785", "Severity": "Warning" }
    ]
}
```

Only the diagnostic's **start** is asserted, so the reported span may extend past the located snippet. A snippet
that is missing or appears more than once in its file fails the test with a helpful message — use a more specific
snippet or an explicit `Line`/`Column`.

## Wiring the test

Configure a shared baseline once, then load and run each case. Resolve the `TestCases` folder from the source
tree with `TestPath.ForCaller()` so fixtures are checked into git:

```csharp
private static readonly RoslynTestBuilder<DefaultVerifier> Baseline =
    IncrementalGeneratorTestBuilder
        .CreateDefaultBuilder<MyGenerator>()
        .WithReferenceAssemblies(ReferenceAssemblies.Net.Net100)
        .WithAdditionalReferences("MyLibrary.dll");

private static readonly TestPath testCases = TestPath.ForCaller() / "TestCases";

[Theory]
[InlineData("MyCase.json")]
public async Task Generates_expected_sources(string descriptor)
{
    var testCase = await JsonTestCase.FromJsonFileAsync(testCases / descriptor);
    var test = await Baseline.BuildAsync(testCase);
    await test.RunAsync();
}
```

`CreateDefaultBuilder` verifies all compiler diagnostics, suppresses `CS1591`, and expects every generator's
post-initialization sources (`WithGeneratorPostInitializationSources`). Start from `Create` instead to opt out.

`WithAdditionalReferences` adds the assemblies the input sources need — typically the library declaring the
attributes your generator looks for, without which they fail to compile with `CS0246`. A bare file name resolves
against the test's output directory, so the assembly has to land there (a `ProjectReference` to it does that).

The builder is immutable (every `With*` call forks a new builder) so a fully configured baseline can be shared
as a fixture and specialized per test without affecting other tests.

## Multiple Generators and Analyzers

The `RoslynTestBuilder` supports multiple generators and diagnostic analyzers:

```csharp
RoslynTestBuilder
    .CreateDefaultBuilder()
    .WithIncrementalGenerator<MyIncrementalGenerator>()
    .WithSourceGenerator<MySourceGenerator>()
    .WithDiagnosticAnalyzer<MyAnalyzer>();
```

This would run every generator and then the analyzer on all input and output files. This can even be done with only
source generators or with only diagnostic analyzers.

## Project setup

Keep fixture files out of the compilation and out of the build output. Because the test resolves them from the
source tree via `TestPath.ForCaller()`, they do not need to be copied to `bin`:

```xml
<ItemGroup>
  <!-- These end in .cs for editor hints but must not be compiled. -->
  <Compile Remove="**/TestCases/**/*.cs" />
  <!-- Included for editor visibility/nesting only; not copied to the output directory. -->
  <None Include="**/TestCases/**/*.json" Exclude="bin/**/*.json" />
  <None Include="**/TestCases/**/*.cs" Exclude="bin/**/*.cs" />
</ItemGroup>
```

Expected generated files are compared exactly, with no line-ending normalization, so a `.g.cs` fixture has to match
the generator's output byte for byte. Keep git from rewriting them:

```gitattributes
*.g.cs -text
```

and check that your formatter skips them (CSharpier ignores `*.g.cs` by default; others may not). If the generator
builds its output with `StringBuilder.AppendLine` or `Environment.NewLine`, its line endings follow the OS it runs
on and no single fixture can match both — emit a fixed newline instead if the tests run on more than one platform.

## Updating expected files in place

Hand-writing and maintaining `.g.cs` files is tedious and I hate it, you should too. Add `WithExpectedSourceUpdates()`
to the baseline and, whenever the generator's output differs from (or is missing) an expected file, the test
**overwrites it on disk with the produced output and still fails**:

```csharp
.WithExpectedSourceUpdates(enabled: Environment.GetEnvironmentVariable("CI") is null)
```

Workflow:

1. Change your generator, or add a new test case whose `.g.cs` files do not exist yet.
2. Run the tests. Mismatched and missing expected files are rewritten in place and the run is **red**.
3. Review the changes in source control
4. Keep them (re-run → green) or revert them.

The tests compare the output to previous file contents, so this won't cause false-positives. This does mean that when
you get a failure and you've fixed it: the next test will fail (the previous file contents were broken and the new ones
are fixed), and so running the tests again will then pass.
