using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.EnumTests;

public class EnumTests
{
    private static readonly TestPath testCases = TestPath.CurrentDirectory / "EnumTests" / "TestCases";

    [Theory]
    [InlineData("SingleEnum.json")]
    public async Task EmbeddedAttribute_EmbeddedEnum_ShouldGenerateSourceText(string testDescriptor)
    {
        // Arrange
        var testCaseFile = testCases / testDescriptor;
        var testCase = await JsonTestCase.FromJsonFileAsync(testCaseFile, TestContext.Current.CancellationToken);

        // Act
        var test = await GeneratorTest.Baseline.BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
