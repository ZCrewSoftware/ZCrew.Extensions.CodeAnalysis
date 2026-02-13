using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.SourceGeneratorTests.TestHelpers;

internal static class EmbeddedAttributeSourceText
{
    public static readonly SourceText SourceText = SourceText.From(
        """
        namespace Microsoft.CodeAnalysis
        {
            internal sealed partial class EmbeddedAttribute : global::System.Attribute
            {
            }
        }
        """.Replace(Environment.NewLine, "\r\n"),
        Encoding.UTF8
    );
}
