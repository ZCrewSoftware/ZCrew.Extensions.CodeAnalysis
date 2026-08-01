# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZCrew.Extensions.CodeAnalysis.CSharp is a **Roslyn source generator utility library** that helps developers build C# source generators. It ships two generators:

- **EmbeddedAttribute** — embeds attribute definitions into consuming projects and generates the attribute parsing code (attribute data record, constructor/parameter/type-parameter/named-parameter matchers, and `SyntaxValueProvider`/`AttributeData` extensions).
- **IsType** — generates a fast Roslyn type-check body for partial methods marked with `[IsType<T>]` / `[IsType(typeof(T))]`.

Two NuGet packages come out of the repo: `ZCrew.Extensions.CodeAnalysis.CSharp` (the generators plus their runtime library) and `ZCrew.Extensions.CodeAnalysis.CSharp.Testing` (a harness for testing *any* source generator).

Shipping assemblies target **netstandard2.0** (required for source generators); test projects target **net10.0**.

## Build Commands

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

Run a single test project by path:
```bash
dotnet test tests/ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests --no-build
dotnet test tests/ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.SourceGeneratorTests --no-build
```

## Running Individual Tests

Uses xUnit v3 with Microsoft Testing Platform (configured in `global.json`). Filter tests with the query filter language via `--filter-query` using the format `/<assembly>/<namespace>/<class>/<method>`:

```bash
# Run a single test method
dotnet test --no-build --filter-query "/*/*/*/MyTestMethod"

# Run all tests in a class
dotnet test --no-build --filter-query "/*/*/MyTestClass/*"

# Run by class (fully qualified name)
dotnet test --no-build --filter-class "Full.Namespace.ClassName"
```

## Code Formatting

The project uses **CSharpier** (v1.2.1) enforced via pre-commit hooks. Format code with:
```bash
dotnet tool run CSharpier format
```

## SDK and Language

- .NET SDK **10.0.100** (see `global.json`)
- C# language version **14.0**
- Nullable reference types enabled everywhere
- Centralized package versions in `Directory.Packages.props`

## Architecture

### Project Split (five projects, two packages)

- **`src/ZCrew.Extensions.CodeAnalysis.CSharp.Abstractions/`** — the runtime library (`AttributeConstructor<>`, `AttributeParameter<>`, `AttributeTypeParameter<>`, `AttributeNamedParameter<>`, `FormattedStringBuilder`, `EquatableArray<T>`, `SymbolExtensions`, `TypedConstantExtensions`). Namespaces stay `ZCrew.Extensions.CodeAnalysis.CSharp`; only the assembly carries the suffix. Deliberately contains **no `[Generator]` types**: downstream packages redistribute this DLL in their `analyzers/dotnet/cs`, where any generator would activate in end-consumer compilations and fail to compile. Enforced by `LibraryAssemblyTests`.
- **`src/ZCrew.Extensions.CodeAnalysis.CSharp.Embedded/`** — the `EmbeddedIncrementalGenerator` plus its factories/models/emitters. ProjectReferences Abstractions; ships only under `analyzers/dotnet/cs`. Also consumes the IsType generator as an analyzer (`OutputItemType="Analyzer"`, `ReferenceOutputAssembly="false"`) so its own symbol checks are generated — keep that edge reference-free or the `ZCrew.Extensions.CodeAnalysis.CSharp.IsType` **namespace** becomes visible and shadows the `IsType` attribute name.
- **`src/ZCrew.Extensions.CodeAnalysis.CSharp.IsType/`** — the `IsTypeIncrementalGenerator` plus its factories/models/emitters. Same shape; ProjectReferences Abstractions only.
- Both generator projects need Abstractions' `InternalsVisibleTo`: their `record struct` models use `init`, and on netstandard2.0 `IsExternalInit` only exists as Polyfill's **internal** shim inside Abstractions.
- **`src/ZCrew.Extensions.CodeAnalysis.CSharp/`** — packaging-only project (no code) that authors the main nupkg: Abstractions in `lib/`, both assemblies in `analyzers/dotnet/cs`, plus `build/*.targets` that auto-pack Abstractions into downstream authors' packages and forward it to their analyzer-ProjectReference consumers via a `GetTargetPath` hook.
- **`src/ZCrew.Extensions.CodeAnalysis.CSharp.Testing/`** — the generator-testing package, published on its own. Self-packing (unlike the generator projects and Abstractions) and depends on nothing else in the repo. See its `README.md`, which is the published package documentation.
- **deps.json traps** (each broke the test hosts with `FileNotFoundException` during development): do not give any project a `PackageId` matching another project's assembly name; do not put `PrivateAssets="all"` on a ProjectReference edge that test projects also reference directly; do not add a `GetTargetPathDependsOn` hook to a project consumed via plain ProjectReference. The layout exists precisely so none of these are needed — the suppressed edges live only in the packaging project, which nothing else references.

### EmbeddedAttribute Pipeline (`Embedded/`)

`EmbeddedIncrementalGenerator` processes types marked with `[Microsoft.CodeAnalysis.EmbeddedAttribute]`. It runs in two phases:

1. **Post-initialization**: emits the `EmbeddedAttribute` definition itself
2. **Source output**: two independent providers over the same attribute, one per generated file

- **Factories**: `EmbeddedAttributeInfoFactory` extracts attribute metadata from Roslyn symbols (and skips non-attributes, non-public constructors, static/indexer properties); `EmbeddedAbstractionSourceTextFactory` captures the attribute's own source text for re-emission downstream.
- **Naming**: `NameTable` assigns collision-free generated field names per attribute — reservations are keyed by `(name, type)` so a constructor parameter and its matching property deliberately alias to one field. `EmbeddedAttributeNames`/`EmbeddedAbstractionNames` derive every generated type/method/hint name from one definition. `SymbolHelpers.EscapeIdentifier` keeps keyword identifiers valid.
- **Models** (`Models/`): immutable `readonly record struct` types (`EmbeddedAttributeInfo`, `ConstructorInfo`, `ParameterInfo`, `TypeParameterInfo`, `NamedPropertyInfo`, `EmbeddedAttributeSourceText`) that carry metadata through the pipeline.
- **Emitters** (`Emitters/`): `EmbeddedAttributeInfoEmitter` orchestrates one file from `AttributeDataEmitter`, `AttributeDataExtensionEmitter`, `SyntaxValueProviderExtensionEmitter`, `ConstructorEmitter`, `ParameterEmitter`, `TypeParameterEmitter`, and `NamedParameterEmitter`. `EmbeddedAbstractionSourceTextEmitter` emits the source-text file separately.

Emitted-code invariants worth preserving: the matcher classes are `file`-scoped so multiple attributes in one namespace don't collide, while `AttributeDataExtensions`/`SyntaxValueProviderExtensions` are `partial` so they merge; every emitted type reference is `global::`-qualified because output lands in the consumer's namespace with no usings.

### IsType Pipeline (`IsType/`)

`IsTypeIncrementalGenerator` handles both the generic and `typeof` attribute forms, emitting the `IsTypeAttribute` definitions at post-initialization (`IsTypeAttributeSource`). `IsTypeMethodInfoFactory` builds the model; `Models/` holds a recursive type-segment representation (`TypeSegment`, `NamedTypeArgument`, `SpecialTypeArgument`, `TypeParameterArgument`); `Emitters/` turns it into a nested symbol check.

### Shared Utilities (in Abstractions)

- **`Text/FormattedStringBuilder`**: StringBuilder wrapper with automatic indentation tracking (4 spaces per level via `Indent()`/`Unindent()`), used for all code generation. `Text/FormattedStringBuilderExtensions` adds the generated-file boilerplate (`AppendAutoGeneratedComment`, `AppendBlock`, `AppendRawString`, …)
- **`Collections/EquatableArray<T>`**: Immutable array with value equality semantics, critical for Roslyn incremental generator caching
- **`SymbolExtensions`**: Helpers for formatting Roslyn symbols as partial class/method declarations, fully qualified names, and generic type names

### Test Structure

Six test projects, all xUnit v3. Each generator owns a `SourceGeneratorTests`/`IntegrationTests` pair, and nothing spans both generators:

- **`tests/.../UnitTests/`** — the Abstractions runtime library (`FormattedStringBuilder`, `EquatableArray<T>`, `SymbolExtensions`, `AttributeParameter`) plus `LibraryAssemblyTests`, which asserts Abstractions ships no `[Generator]`.
- **`tests/.../Embedded.SourceGeneratorTests/`** — the Embedded generator end-to-end against JSON-driven fixtures, split by concern (`AttributeTests`, `EnumTests`, `DiagnosticTests`, `CacheTests`).
- **`tests/.../IsType.SourceGeneratorTests/`** — the same for the IsType generator (`IsTypeTests`).
- Both hold their own `TestHelpers/GeneratorTest`, each exposing one immutable `Baseline` wired to that project's generator. The file is deliberately duplicated so neither project references the other generator.
- **`tests/.../Testing.UnitTests/`** — the Testing package itself (`JsonTestCase`, `SourceGeneratorTestBuilder`, `TestPath`, `GeneratorPostInitializationSources`).
- **`tests/.../Embedded.IntegrationTests/`** and **`tests/.../IsType.IntegrationTests/`** — reference their generator as a real analyzer (`OutputItemType="Analyzer"`) so the implementations are generated into the test assembly and executed against live symbols.

The JSON test-case pattern (provided by the Testing package):
- Each case has a `.json` descriptor listing source files (`.Attribute.cs`), expected generated files (`.g.cs`) keyed by generator hint name, and any expected diagnostics
- `.cs` files under `TestCases/` are excluded from compilation (`<Compile Remove>`) and included as `None` for editor visibility only — they are **not** copied to `bin`; tests resolve them from the source tree via `TestPath.ForCaller()`
- `JsonTestCase.FromJsonFileAsync()` loads the descriptor; `SourceGeneratorTestBuilder.BuildAsync()` sets up the Roslyn harness
- `WithExpectedSourceUpdates()` (enabled off-CI) rewrites mismatched or missing `.g.cs` files in place **and still fails the run** — so regenerating expectations is: run red, review the diff in git, re-run green
- Every `src/` project exposes internals to `$(AssemblyName).SourceGeneratorTests` / `.UnitTests` / `.IntegrationTests` via the convention in `src/Directory.Build.props`; test projects are named to match, so no per-project `InternalsVisibleTo` is needed
- `GeneratedFileName` is the full path Roslyn emits, qualified with a per-generator variable, e.g. `$(IsTypeIncrementalGenerator)/Ns.Type.g.cs`

## Key Conventions

- Data models use `readonly record struct` for immutability and value equality
- All Roslyn pipeline operations thread `CancellationToken`
- Generated code includes `// <auto-generated/>` headers and pragma directives
- The `[ExcludeFromCodeCoverage]` attribute is used on delegating/pass-through code
- Expected `.g.cs` fixtures are CRLF, and CSharpier ignores `.g.cs`
