using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.AttributeTests;

public class AttributeTests
{
    private static readonly TestPath testCases = TestPath.CurrentDirectory / "AttributeTests" / "TestCases";

    [Theory]
    [InlineData("MultipleNamedParameters.json")]
    [InlineData("MultipleTypeParameters.json")]
    [InlineData("MultipleParameters.json")]
    [InlineData("MultipleConstructors.json")]
    [InlineData("All.json")]
    public async Task EmbeddedAttribute_WithReplaceAction_ShouldGenerateCodeToReplaceServices(string testDescriptor)
    {
        // Arrange
        var testCaseFile = testCases / testDescriptor;
        var testCase = await TestCase.FromJsonFileAsync(testCaseFile, TestContext.Current.CancellationToken);

        // Act
        var test = await EmbeddedAttributeIncrementalGeneratorTest.ForTestCaseAsync(
            testCase,
            TestContext.Current.CancellationToken
        );

        // Assert
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
