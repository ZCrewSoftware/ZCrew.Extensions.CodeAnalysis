# Test Conventions

## Running Tests

Tests use **xUnit v3** with the Microsoft Testing Platform runner. This does **not**
support VSTest-style `--filter "FullyQualifiedName~Foo"` syntax. Use the xUnit.net v3
filter flags instead: `--filter-class`, `--filter-method`, `--filter-namespace`.
Wildcards (`*`) are supported at the start and/or end of each value.

The six test projects are `ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests`,
`ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests`,
`ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.SourceGeneratorTests`,
`ZCrew.Extensions.CodeAnalysis.CSharp.Embedded.IntegrationTests`,
`ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGeneratorTests` and
`ZCrew.Extensions.CodeAnalysis.CSharp.IsType.IntegrationTests`. Each generator owns its own
source-generator and integration pair; nothing spans both generators.

Run all tests in a single project:
```bash
dotnet test --project tests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.csproj
```

Run all tests in a single class (wildcard matches the namespace prefix):
```bash
dotnet test --project tests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.csproj \
  --filter-class "*RoslynTestBuilderTests"
```

Run a single test method (wildcard-match the method name):
```bash
dotnet test --project tests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.csproj \
  --filter-method "*ShouldCaptureOnce*"
```

Run several tests matching a pattern (e.g. all variable-expansion tests across classes):
```bash
dotnet test --project tests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests/ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests.csproj \
  --filter-method "*Variable*"
```

`--filter-query "/<assembly>/<namespace>/<class>/<method>"` also works and is what the
root `CLAUDE.md` documents; either is fine.

## Test Naming

Follow `Member_T_When_Should` style. The name reads as: what member is being tested,
what condition triggers it, and what the expected outcome is.

```csharp
Add_T_WhenEntryIsValid_ShouldAddEntry()
InvokeAsync_WhenCalled_ShouldYield()
GetOrder_WhenIdNotFound_ShouldReturnNull()
```

## AAA Structure

Every test must have `// Arrange`, `// Act`, and `// Assert` comments separating the three phases.
The only exception is when the 'Arrange' section is empty. This is very rare though.

```csharp
[Fact]
public void GetOrder_WhenIdIsValid_ShouldReturnOrder()
{
    // Arrange
    var service = new OrderService();

    // Act
    var result = service.GetOrder(42);

    // Assert
    Assert.NotNull(result);
}
```

**Never combine Act and Assert or Arrange & Act.**
If the action itself is the assertion (like testing that something throws), capture the call in a `Func` or `Action` first:

```csharp
// Bad — don't do this:
// Arrange & Act
var act = () => service.GetOrder(-1);

// Good — the arrange isn't necessary:
// Act
var act = () => service.GetOrder(-1);
```

```csharp
// Bad — don't do this:
// Act & Assert
Assert.Throws<ArgumentException>(() => service.GetOrder(-1));

// Good — separate the phases:
// Act
var act = () => service.GetOrder(-1);

// Assert
Assert.Throws<ArgumentException>(act);
```

## No Regions or Decorative Comments

Never use `#region` / `#endregion`. Never use decorative comments to separate groups of
tests (e.g., `// -- Non-keyed source descriptors --`). If a test class is large enough
to need visual separation, split it into partial classes or separate classes instead.

## Test Isolation

Each test must stand alone. Never share mutable state between tests via fields or
static members. If two tests share setup, each should create its own instance.
This prevents cascading failures where one broken test poisons the rest.

A shared `RoslynTestBuilder` is fine despite being a static field: it is immutable, so
every `With*` call forks a new builder and leaves the shared one untouched.

## Naming Variables

Never call anything `sut` (system under test). Use a name that describes what the
thing actually is — `service`, `factory`, `generator`, `provider`, etc.

## Test Doubles

**Don't create test doubles when a real type works.** If you need something that
implements `IList<T>`, just use `List<T>`. Only fake when you need to control or
observe behavior that a real type can't give you.

**Use NSubstitute** when you need to verify interactions: was a method called, how
many times, with what arguments. That's what mocking libraries are for. No test project
here references it yet, so add it to `Directory.Packages.props` when the first real need
turns up.

```csharp
var service = Substitute.For<IService>();
service.Process("x");
service.Received(1).Process(Arg.Any<string>());
```

**Generators and analyzers are the exception.** `RoslynTestBuilder` constrains them to
`new()` and instantiates them itself, so they cannot be substituted. Hand-write minimal
fakes in a `TestDoubles/` folder instead — see
`ZCrew.Extensions.CodeAnalysis.CSharp.Testing.UnitTests/TestDoubles`. Give them the
narrowest behavior the test needs and no `[Generator]` / `[DiagnosticAnalyzer]`
attribute; the attributes trip the Roslyn analyzer-authoring rules (`RS1042` and
friends) and the harness never needs them.
