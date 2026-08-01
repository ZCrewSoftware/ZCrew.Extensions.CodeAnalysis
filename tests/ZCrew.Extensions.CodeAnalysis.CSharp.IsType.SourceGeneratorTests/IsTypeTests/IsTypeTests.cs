using ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.SourceGeneratorTests.IsTypeTests;

public class IsTypeTests
{
    private static readonly TestPath TestCases = TestPath.ForCaller() / "TestCases";

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
        var testCaseFile = TestCases / testDescriptor;
        var testCase = await JsonTestCase.FromJsonFileAsync(testCaseFile, TestContext.Current.CancellationToken);

        // Act
        var test = await GeneratorTest.Baseline.BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
