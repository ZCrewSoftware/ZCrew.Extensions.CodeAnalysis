# Introduction

ZCrew.Extensions.CodeAnalysis.CSharp is a **Roslyn source generator utility library** that helps developers build C# source generators. Rather than writing boilerplate infrastructure from scratch, this library provides reusable tools for code generation and attribute parsing.

## Core Concepts

The library supports two main workflows:

### SourceText Embedding

Any C# type (enum, class, struct, record, interface) can be marked with `[Microsoft.CodeAnalysis.Embedded]`. The library generates a static `SourceText` property containing the type's source code, ready to be emitted into consuming projects via `PostInitializationOutput` or `RegisterSourceOutput`.

This is useful when your source generator needs to inject shared types (enums, marker interfaces, helper classes) into the projects that reference it.

### Attribute Parsing Pipeline

For types that inherit from `System.Attribute`, the library goes further: it generates a complete infrastructure for parsing attribute usages at compile time. This includes constructor matching, parameter extraction, type parameter handling, and named argument processing -- all driven by a data builder pattern.

Instead of manually inspecting `AttributeData` from Roslyn, you implement a generated `IDataBuilder` interface and let the generated `Constructor` class do the heavy lifting.

## Key Utilities

The library also provides two standalone utilities useful in any source generator:

- **`FormattedStringBuilder`** -- A `StringBuilder` wrapper with automatic indentation tracking, designed for generating well-formatted C# source code.
- **`EquatableArray<T>`** -- An `ImmutableArray<T>` wrapper with value equality semantics, critical for Roslyn incremental generator caching.

## Next Steps

- [Getting Started](./2-getting-started.md) -- Installation and setup
- [Emitting Attributes](./3-emitting-attributes.md) -- The full attribute parsing pipeline
- [Emitting Other Abstractions](./4-emitting-other-abstractions.md) -- Embedding enums, classes, and other types
