# Shipping a source generator in a .NET core package

This is the process you should follow to ship a generator built using this library into a .NET Core package.
This pattern is quite common:

- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Logging.Abstractions`
- `System.Text.Json`

All of these projects are written in .NET Core but ship a .NET Standard source generator / analyzer. The way
.NET chose to do that is a bit odd, so this page should be easier to follow.

## Why a contract is needed

Roslyn's analyzer assembly loader resolves an analyzer's dependencies from the `analyzers/dotnet/cs` folder
in the NuGet package instead of `lib/`. Since this project has a build dependency, it needs to be packaged.
Everything the generator calls during analysis is in `ZCrew.Extensions.CodeAnalysis.CSharp.Abstractions.dll`.

## Consumer pattern

In your Roslyn source generator or analyzer, specify the dependency on `ZCrew.Extensions.CodeAnalysis.CSharp`
with its `analyzers`. Notice how `IsPackable` is `false`. This is because your source generator won't be a
separate NuGet package. `CopyLocalLockFileAssemblies` is a good idea since you may get file content when
building for multiple frameworks (like: .NET 8, 9, and 10 at once).

```xml
<PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <IncludeBuildOutput>false</IncludeBuildOutput>
  <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  <IsPackable>false</IsPackable>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="ZCrew.Extensions.CodeAnalysis.CSharp">
    <PrivateAssets>analyzers</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

In your library, which is the NuGet package shipping the generator, library code, and is .NET Core:

```xml
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>

<ItemGroup>
  <!-- The pack items need the generator's output -->
  <ProjectReference Include="../MyGenerator/MyGenerator.csproj">
    <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
    <PrivateAssets>all</PrivateAssets>
  </ProjectReference>
</ItemGroup>

<PropertyGroup>
  <_GeneratorOutput>../MyGenerator/bin/$(Configuration)/netstandard2.0</_GeneratorOutput>
</PropertyGroup>

<ItemGroup>
  <!-- Pack the generator and the abstractions used during analysis -->
  <None Include="$(_GeneratorOutput)/MyGenerator.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  <None Include="$(_GeneratorOutput)/ZCrew.Extensions.CodeAnalysis.CSharp.Abstractions.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
</ItemGroup>
```

Note that there is no `OutputItemType="Analyzer"`: you probably don't want to run the generator on _your_
library code.
