N# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZCrew.Extensions.CodeAnalysis.CSharp is a **Roslyn source generator utility library** that helps developers build C# source generators. It provides infrastructure for embedding attribute definitions into consuming projects and automatically generating attribute parsing code (constructors, parameters, type parameters, named parameters, and data builders).

The main library targets **netstandard2.0** (required for source generators), while test projects target **net10.0**.

## Build Commands

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

Run a single test project:
```bash
dotnet test --project tests/ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests --no-build
dotnet test --project tests/ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests --no-build
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

### Project Split (three projects, one package)

- **`src/ZCrew.Extensions.CodeAnalysis.CSharp.Abstractions/`** — the runtime library (`AttributeConstructor<>`, `FormattedStringBuilder`, `EquatableArray<T>`, extensions). Namespaces stay `ZCrew.Extensions.CodeAnalysis.CSharp`; only the assembly carries the suffix. Deliberately contains **no `[Generator]` types**: downstream packages redistribute this DLL in their `analyzers/dotnet/cs`, where any generator would activate in end-consumer compilations and fail to compile (see `docs/embedded-attribute-consumer-leak.md`). Enforced by `LibraryAssemblyTests`.
- **`src/ZCrew.Extensions.CodeAnalysis.CSharp.Generators/`** — the incremental generators (`EmbeddedAttributeIncrementalGenerator`, `IsTypeIncrementalGenerator`) plus their factories/models/emitters. ProjectReferences Abstractions; ships only under `analyzers/dotnet/cs`.
- **`src/ZCrew.Extensions.CodeAnalysis.CSharp/`** — packaging-only project (no code) that authors the single nupkg: Abstractions in `lib/`, both assemblies in `analyzers/dotnet/cs`, plus `build/*.targets` that auto-pack Abstractions into downstream authors' packages and forward it to their analyzer-ProjectReference consumers via a `GetTargetPath` hook.
- **deps.json traps** (each broke the test hosts with `FileNotFoundException` during development): do not give any project a `PackageId` matching another project's assembly name; do not put `PrivateAssets="all"` on a ProjectReference edge that test projects also reference directly; do not add a `GetTargetPathDependsOn` hook to a project consumed via plain ProjectReference. The three-project layout exists precisely so none of these are needed — the suppressed edges live only in the packaging project, which nothing else references.

### Source Generator Pipeline (`src/ZCrew.Extensions.CodeAnalysis.CSharp.Generators/`)

The entry point is `EmbeddedAttributeIncrementalGenerator` — an `IIncrementalGenerator` that processes types marked with `[EmbeddedAttribute]`. It runs in two phases:

1. **Post-initialization**: Emits the `EmbeddedAttribute` definition itself
2. **Source output**: Generates attribute parsing infrastructure via multiple specialized source generators

The generation pipeline flows through:
- **Factories** (`EmbeddedAttribute/`): `EmbeddedTypeInfoFactory` extracts metadata from Roslyn symbols; `EmbeddedAttributeGroupFactory` groups and deduplicates types
- **Models** (`EmbeddedAttribute/Models/`): Immutable `readonly record struct` types (`EmbeddedTypeInfo`, `EmbeddedAttributeGroup`, etc.) that carry metadata through the pipeline
- **Source Generators** (`EmbeddedAttribute/SourceGenerators/`): Each generates one aspect of the output — `SourceTextSourceGenerator`, `DataBuilderInterfaceSourceGenerator`, `ConstructorSourceGenerator`, `ParameterSourceGenerator`, `TypeParameterSourceGenerator`, `NamedParameterSourceGenerator`

### Shared Utilities

- **`Text/FormattedStringBuilder`**: StringBuilder wrapper with automatic indentation tracking (4 spaces per level via `Indent()`/`Unindent()`), used for all code generation
- **`Collections/EquatableArray<T>`**: Immutable array with value equality semantics, critical for Roslyn incremental generator caching
- **`Extensions/SymbolExtensions`**: Helpers for formatting Roslyn symbols as partial class/method declarations and generic type names

### Test Structure

**Unit Tests** (`tests/.../UnitTests/`): Standard xUnit v3 tests for utility classes.

**Source Generator Tests** (`tests/.../SourceGeneratorTests/`): Use Roslyn's `CSharpSourceGeneratorTest` infrastructure with a JSON-driven test case pattern:
- Each test case has a `.json` descriptor listing source files (`.Attribute.cs`) and expected generated files (`.g.cs`)
- The `.cs` files in `TestCases/` folders are excluded from compilation (`<Compile Remove>`) and treated as content files
- `TestCase.FromJsonFileAsync()` loads the descriptor; `EmbeddedAttributeIncrementalGeneratorTest.ForTestCaseAsync()` sets up the Roslyn test harness
- The main library exposes internals to the source generator test project via `InternalsVisibleTo`

## Key Conventions

- Data models use `readonly record struct` for immutability and value equality
- All Roslyn pipeline operations thread `CancellationToken`
- Generated code includes `// <auto-generated/>` headers and pragma directives
- The `[ExcludeFromCodeCoverage]` attribute is used on delegating/pass-through code