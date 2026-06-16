using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.IsTypeTests;

public class IsTypeTests
{
    private static readonly TestPath testCases = TestPath.CurrentDirectory / "IsTypeTests" / "TestCases";

    [Theory]
    [InlineData("ServiceKeyGeneric.json")]
    [InlineData("ServiceKeyTypeof.json")]
    [InlineData("SpecialType.json")]
    [InlineData("PreNarrowedParameter.json")]
    [InlineData("PreNarrowedSpecialType.json")]
    [InlineData("NestedType.json")]
    [InlineData("NonNullableExtension.json")]
    [InlineData("GlobalNamespace.json")]
    [InlineData("GlobalNamespaceType.json")]
    [InlineData("ClosedGeneric.json")]
    [InlineData("GenericTypeDefinition.json")]
    [InlineData("NestedGeneric.json")]
    public async Task IsType_WithMarkedPartialMethod_ShouldGenerateTypeCheck(string testDescriptor)
    {
        // Arrange
        var testCaseFile = testCases / testDescriptor;
        var testCase = await JsonTestCase.FromJsonFileAsync(testCaseFile, TestContext.Current.CancellationToken);

        // Act
        var test = await GeneratorTest.IsTypeBaseline.BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
