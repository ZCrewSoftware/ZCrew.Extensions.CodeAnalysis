using Microsoft.CodeAnalysis;
using ZCrew.Extensions.CodeAnalysis.CSharp.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.UnitTests;

public class LibraryAssemblyTests
{
    [Fact]
    public void LibraryAssembly_ContainsNoSourceGenerators()
    {
        // Arrange
        var libraryAssembly = typeof(FormattedStringBuilder).Assembly;

        // Act
        var generatorTypes = libraryAssembly
            .GetTypes()
            .Where(type => type.IsDefined(typeof(GeneratorAttribute), inherit: false))
            .ToList();

        // Assert
        Assert.Empty(generatorTypes);
    }
}
