using FluentAssertions;
using Wired.CodeAnalysis.Syntax;
using Xunit.Abstractions;

namespace TestProject1.CodeAnalysis;

public class SyntaxFactTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public SyntaxFactTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Theory]
    [MemberData(nameof(GetSyntaxKindData), "get text not null")]
    public void SyntaxFact_GetText_RoundTrips(SyntaxKind kind)
    {
        var text = SyntaxFacts.GetText(kind)!;
        
        var tokens = SyntaxTree.ParseTokens(text);
        var token = tokens.Single();
        kind.Should().Be(token.Kind);
        text.Should().Be(token.Text);
    }
    
    [Theory]
    [MemberData(nameof(GetSyntaxKindData), "keywords")]
    public void SyntaxFact_GetText_GetKeywordKind_Chain_Returns_Same_Kind(SyntaxKind kind)
    {
        var text = SyntaxFacts.GetText(kind);
        text.Should().NotBeNull();
        var actualKind = SyntaxFacts.GetKeywordKind(text!);
        this.testOutputHelper.WriteLine($"{nameof(SyntaxFacts.GetText)}-\'{text}\'");
        actualKind.Should().Be(kind);

    }

    public static IEnumerable<object[]> GetSyntaxKindData(string type = "")
    {
        var kinds = Enum.GetValues<SyntaxKind>();

        if (type is "get text not null")
        {
            kinds = kinds.Where(x => SyntaxFacts.GetText(x) != null).ToArray();
        }
        
        if (type is "keywords")
            kinds = kinds.Where(x => x.ToString().EndsWith("Keyword")).ToArray();
        
        foreach (var kind in kinds)
            yield return new object[] { kind };
    }
}