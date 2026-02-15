# ZCrew.Extensions.CodeAnalysis.CSharp

Utility library for building Roslyn source generators. Mark types with `[Embedded]` to generate `SourceText` constants and attribute-parsing infrastructure automatically.

## Installation

Available on NuGet for .NET Standard 2.0:

```xml
<PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
    <PrivateAssets>analyzers</PrivateAssets>
</PackageReference>
```

## Usage

### Embedding any type

Any `internal` type marked with `[Embedded]` gets a generated `SourceText` constant you can emit from your generator:

```csharp
[Microsoft.CodeAnalysis.Embedded]
internal enum Lifetime
{
    Transient,
    Scoped,
    Singleton,
}
```

This generates a `LifetimeSourceText` class containing a ready-to-use `SourceText` instance, so your generator can emit the type into consuming projects without maintaining raw strings.

### Embedding attributes

Attributes get the `SourceText` constant **plus** a full parsing pipeline:

```csharp
[Microsoft.CodeAnalysis.Embedded]
internal class ServiceAttribute<TService, TImplementation> : Attribute
{
    public ServiceAttribute(string name) { }

    public Lifetime Lifetime { get; init; }
}
```

This generates:
- **`ServiceAttributeSourceText`** — embeddable source text
- **`IServiceAttributeDataBuilder`** — builder interface with `WithName()`, `WithService()`, `WithImplementation()`, `WithLifetime()`
- **`ServiceAttributeConstructor`** — matches `AttributeData` to the right constructor overload
- **`ServiceAttributeParameter`** — wraps constructor parameters
- **`ServiceAttributeTypeParameter`** — wraps generic type parameters (`TService`, `TImplementation`)
- **`ServiceAttributeNamedParameter`** — wraps properties with public setters (`Lifetime`)

All generated code handles `TypedConstant` and `ITypeSymbol` unwrapping, so your generator can focus on business logic instead of Roslyn plumbing.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
