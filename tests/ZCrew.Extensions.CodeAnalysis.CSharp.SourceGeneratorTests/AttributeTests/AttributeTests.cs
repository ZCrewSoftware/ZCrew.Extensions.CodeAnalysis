using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.AttributeTests;

public class AttributeTests
{
    private static readonly TestPath TestCases = TestPath.ForCaller() / "TestCases";

    [Theory]
    [InlineData("MultipleNamedParameters.json")]
    [InlineData("MultipleTypeParameters.json")]
    [InlineData("MultipleParameters.json")]
    [InlineData("MultipleConstructors.json")]
    [InlineData("All.json")]
    [InlineData("PartialConstructors.json")]
    [InlineData("RequiredNamedProperty.json")]
    [InlineData("SetsRequiredMembers.json")]
    [InlineData("NullableParameter.json")]
    [InlineData("ArrayAndTypeParameters.json")]
    [InlineData("ReservedNames.json")]
    [InlineData("IgnoredProperties.json")]
    [InlineData("NonPublicConstructors.json")]
    public async Task EmbeddedAttribute_WithMarkedAttribute_ShouldGenerateAttributeData(string testDescriptor)
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
