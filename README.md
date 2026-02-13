# .CSharp

Helper utility for building source generators.

## Installation

The C# package is available on NuGet as `ZCrew.Extensions.CodeAnalysis.CSharp` for these frameworks:

- .NET Standard 2.0

```xml
<PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
    <OutputItemType>Analyzer</OutputItemType>
</PackageReference>
```
## Quick Start

Add an `[Embedded]` to any `internal` abstractions you wish to embed with your source generator:

```csharp
[Microsoft.CodeAnalysis.Embedded]
internal class InterceptAttribute : Attribute
{
    // Constructors, properties, etc. with automatic AttributeData parsing
}
```

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
