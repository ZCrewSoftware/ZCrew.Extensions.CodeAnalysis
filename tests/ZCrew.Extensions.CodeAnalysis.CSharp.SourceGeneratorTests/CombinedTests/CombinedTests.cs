using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.CombinedTests;

public class CombinedTests
{
    private static readonly TestPath TestCases = TestPath.ForCaller() / "TestCases";

    [Theory]
    [InlineData("BothGenerators.json")]
    public async Task BothGenerators_InOneCompilation_ShouldGenerateEachUnderItsOwnDirectory(string testDescriptor)
    {
        // Arrange
        var testCaseFile = TestCases / testDescriptor;
        var testCase = await JsonTestCase.FromJsonFileAsync(testCaseFile, TestContext.Current.CancellationToken);

        // Act
        var test = await GeneratorTest.CombinedBaseline.BuildAsync(testCase, TestContext.Current.CancellationToken);

        // Assert
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
