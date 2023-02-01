using FluentAssertions;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.Tests.CodeAnalysis;

public sealed class LexerTests
{
    [Fact]
    public void Lexer_Diagnostic_Unterminated_String()
    {
       var text = "\"Hello";
       
       var tokens = SyntaxTree.ParseTokens(text, out var diagnostics).ToList();
       tokens.Count.Should().Be(1);
       tokens.Single().Kind.Should().Be(SyntaxKind.StringToken); 
       tokens.Single().Text.Should().Be("\"Hello");
       tokens.Single().Value.Should().Be("Hello");
       diagnostics.Single().Message.Should().Be("Unterminated string literal.");
       diagnostics.Single().TextLocation.Span.Should().Be(new TextSpan(0, 1));
    }
    
    [Fact]
    public void Lexer_Tests_All_Tokens()
    {
        var untestedTokens = Enum.GetValues<SyntaxKind>()
            .Where(k => k.ToString().EndsWith("Keyword") || k.ToString().EndsWith("Token"))
            .Where(k=> k != SyntaxKind.BadToken)
            .Where(k=> k != SyntaxKind.EndOfFileToken)
            .ToList();
        
        var testedTokens = GetTokens()
            .Concat(GetSeparators())
            .Select(t => t.kind)
            .Distinct()
            .ToList();
        
        untestedTokens.Except(testedTokens).Should().BeEmpty();
    }
    
    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void Lexer_Lexes_Token(SyntaxKind kind, string source)
    {
        var tokens = SyntaxTree.ParseTokens(source);
        tokens.Single().Kind.Should().Be(kind);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsData))]
    public void Lexer_Lexes_TokenPairs(
        SyntaxKind t1Kind, string t1Text,
        SyntaxKind t2Kind, string t2Text)
    {
        var source = t1Text + t2Text;
        var tokens = SyntaxTree.ParseTokens(source).ToList();
        tokens.Count.Should().Be(2);
        tokens[0].Kind.Should().Be(t1Kind);
        tokens[0].Text.Should().Be(t1Text);
        tokens[1].Kind.Should().Be(t2Kind);
        tokens[1].Text.Should().Be(t2Text);
    }
    
    [Theory]
    [MemberData(nameof(GetTokenPairsWithSeparatorData))]
    public void Lexer_Lexes_TokenPairsWithSeparators(
        SyntaxKind t1Kind, string t1Text,
        SyntaxKind separatorKind, string separatorText,
        SyntaxKind t2Kind, string t2Text)
    {
        var source = t1Text + separatorText + t2Text;
        var tokens = SyntaxTree.ParseTokens(source).ToList();
        tokens.Count.Should().Be(3);
        tokens[0].Kind.Should().Be(t1Kind);
        tokens[0].Text.Should().Be(t1Text);
        
        tokens[1].Kind.Should().Be(separatorKind);
        tokens[1].Text.Should().Be(separatorText);
        
        tokens[2].Kind.Should().Be(t2Kind);
        tokens[2].Text.Should().Be(t2Text);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        foreach (var token in GetTokens().Concat(GetSeparators()))
        {
            yield return new object[] { token.kind, token.text };
        }
    }

    static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        var fixedTokens = Enum.GetValues<SyntaxKind>()
            .Select(k => (kind: k, text: SyntaxFacts.GetText(k)))
            .Where(t => t.text is not null)
            .Cast<(SyntaxKind, string)>()
            .ToList();
        
        var dynamicTokens = new[] {
            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc"),
            (SyntaxKind.NumberToken, "1"),
            (SyntaxKind.NumberToken, "123145"),
            (SyntaxKind.StringToken, "\"\""),
            (SyntaxKind.StringToken, "\"actual string\""),
            (SyntaxKind.StringToken, "\"useful\""),
            (SyntaxKind.StringToken, "\" \"\" \""),
        };

        return fixedTokens.Concat(dynamicTokens);
    }

    static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
    {
        foreach (var token in GetTokens())
        {
            foreach (var secondToken in GetTokens())
            {
                if (RequiresSeparator(token.kind, secondToken.kind))
                    continue;

                yield return (token.kind, token.text, secondToken.kind, secondToken.text);
            }
        }
    }

    static IEnumerable<(
        SyntaxKind t1Kind, string t1Text,
        SyntaxKind separatorKind, string separatorText,
        SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
    {
        foreach (var token in GetTokens())
        {
            foreach (var secondToken in GetTokens())
            {
                if (RequiresSeparator(token.kind, secondToken.kind))
                {
                    foreach (var separator in GetSeparators())
                    {
                        yield return (
                            token.kind, token.text,
                            separator.kind, separator.text,
                            secondToken.kind, secondToken.text);
                    }
                }
            }
        }
    }

    static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
    {
        var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
        var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");
        
        if (t1Kind is SyntaxKind.StringToken && t2Kind is SyntaxKind.StringToken)
            return true;
        
        if (t1IsKeyword)
            if (t2IsKeyword || t2Kind is SyntaxKind.IdentifierToken)
                return true;
        
        if (t1Kind is SyntaxKind.IdentifierToken)
            if (t2IsKeyword || t2Kind is SyntaxKind.IdentifierToken)
                return true;

        if (t1Kind is SyntaxKind.PipeToken)
            switch (t2Kind)
            {
                case SyntaxKind.PipeToken:
                case SyntaxKind.PipePipeToken:
                    return true;
            }

        if (t1Kind is SyntaxKind.AmpersandToken)
            switch (t2Kind)
            {
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.AmpersandAmpersandToken:
                    return true;
            }



        switch (t1Kind, t2Kind)
        {
            case (SyntaxKind.GreaterThanToken, 
                SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken):
                
            case (SyntaxKind.LessThanToken,
                SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken):
            
            case (SyntaxKind.NumberToken, SyntaxKind.NumberToken):
                
            case (SyntaxKind.BangToken, 
                SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken):
            
            case (SyntaxKind.EqualsToken, 
                SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken):
            
            case (SyntaxKind.WhitespaceToken, SyntaxKind.WhitespaceToken):
                return true;
            default:
                return false;
        }
    }

    public static IEnumerable<object[]> GetTokenPairsData()
    {
        foreach (var tokenPair in GetTokenPairs())
        {
            yield return new object[] { tokenPair.t1Kind, tokenPair.t1Text, tokenPair.t2Kind, tokenPair.t2Text };
        }
    }

    public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
    {
        foreach (var tokenPair in GetTokenPairsWithSeparator())
        {
            yield return new object[]
            {
                tokenPair.t1Kind, tokenPair.t1Text,
                tokenPair.separatorKind, tokenPair.separatorText,
                tokenPair.t2Kind, tokenPair.t2Text
            };
        }
    }

    static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
    {
        return new[]
        {
            (SyntaxKind.WhitespaceToken, " "),
            (SyntaxKind.WhitespaceToken, "   "),
            (SyntaxKind.WhitespaceToken, "\n"),
            (SyntaxKind.WhitespaceToken, "\r"),
            (SyntaxKind.WhitespaceToken, "\r\n"),
            (SyntaxKind.WhitespaceToken, "\n\r"),
        };
    }
}