using ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;
using ZCrew.Extensions.CodeAnalysis.CSharp.Testing;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.DiagnosticTests;

public class DiagnosticTests
{
    private static readonly TestPath TestCases = TestPath.ForCaller() / "TestCases";

    [Theory]
    [InlineData("Undefined.json")]
    public async Task ExpectedDiagnostics_ShouldMatchReportedDiagnostic(string testDescriptor)
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
