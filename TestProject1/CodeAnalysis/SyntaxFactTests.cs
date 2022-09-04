using FluentAssertions;
using Wired.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

public class SyntaxFactTests
{
    [Theory]
    [MemberData(nameof(GetSyntaxKindData))]
    public void SyntaxFact_GetText_RoundTrips(SyntaxKind kind)
    {
        var text = SyntaxFacts.GetText(kind);
        if (text == null)
            return;

        var tokens = SyntaxTree.ParseTokens(text);
        var token = tokens.Single();
        kind.Should().Be(token.Kind);
        text.Should().Be(token.Text);
    }

    public static IEnumerable<object[]> GetSyntaxKindData()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            yield return new object[] { kind };
        }        
    }
}