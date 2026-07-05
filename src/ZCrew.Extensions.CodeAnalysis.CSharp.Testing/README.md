# ZCrew.Extensions.CodeAnalysis.CSharp.Testing

Testing library for verifying Roslyn source generators. It drives a generator through Roslyn's
`CSharpSourceGeneratorTest` harness from a small JSON descriptor, so each test case is just a set of input and
expected-output files plus a `.json` that ties them together.

## Installation

Available on NuGet for .NET Standard 2.0:

```xml
<PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp.Testing" />
```

## Writing a test case

A test case is three kinds of file in a `TestCases/` folder next to your test class:

- **Input sources** (e.g. `MyCase.Attribute.cs`) compiled as input to the generator.
- **Expected generated files** (e.g. `MyCase.SourceText.g.cs`) the output to verify against.
- **A JSON descriptor** (`MyCase.json`) maps the inputs and expected outputs (diagnostics coming soon(tm)):

```json
{
    "SourceFiles": [
        { "SourceFileName": "MyCase.Attribute.cs" }
    ],
    "GeneratedFiles": [
        {
            "SourceFileName": "MyCase.SourceText.g.cs",
            "GeneratedFileName": "MyNamespace.MyTypeSourceText.g.cs"
        }
    ]
}
```

`SourceFileName` is the file on disk; `GeneratedFileName` is the **hint name** your generator passes to
`context.AddSource(hintName, ...)`.

## Wiring the test

Configure a shared baseline once, then load and run each case. Resolve the `TestCases` folder from the source
tree with `TestPath.ForCaller()` so fixtures are checked into git:

```csharp
private static readonly SourceGeneratorTestBuilder<MyGenerator, DefaultVerifier> Baseline =
    SourceGeneratorTestBuilder<MyGenerator>
        .CreateDefaultBuilder()
        .WithReferenceAssemblies(ReferenceAssemblies.Net.Net100)
        .WithGeneratorPostInitializationSources();

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

The builder is immutable (every `With*` call forks a new builder) so a fully configured baseline can be shared
as a fixture and specialized per test without affecting other tests.

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
