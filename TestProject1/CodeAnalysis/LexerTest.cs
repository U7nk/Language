using FluentAssertions;
using FluentAssertions.Primitives;
using Wired.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

public sealed class LexerTest
{
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

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        return new[]
        {
            (SyntaxKind.PlusToken, "+"),
            (SyntaxKind.MinusToken, "-"),
            (SyntaxKind.StarToken, "*"),
            (SyntaxKind.SlashToken, "/"),
            (SyntaxKind.OpenParenthesisToken, "("),
            (SyntaxKind.CloseParenthesisToken, ")"),
            (SyntaxKind.BangToken, "!"),
            (SyntaxKind.AmpersandAmpersandToken, "&&"),
            (SyntaxKind.PipePipeToken, "||"),
            (SyntaxKind.EqualsEqualsToken, "=="),
            (SyntaxKind.BangEqualsToken, "!="),
            (SyntaxKind.EqualsToken, "="),
            (SyntaxKind.TrueKeyword, "true"),
            (SyntaxKind.FalseKeyword, "false"),

            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc"),
            (SyntaxKind.NumberToken, "1"),
            (SyntaxKind.NumberToken, "123145")
        };
    }

    private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
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

    private static IEnumerable<(
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

    private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
    {
        var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
        var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");
        if (t1Kind is SyntaxKind.IdentifierToken)
            if (t2IsKeyword || t2Kind is SyntaxKind.IdentifierToken)
                return true;

        if (t1IsKeyword)
            if (t2IsKeyword || t2Kind is SyntaxKind.IdentifierToken)
                return true;

        if (t1Kind is SyntaxKind.NumberToken)
            if (t2Kind is SyntaxKind.NumberToken)
                return true;

        if (t1Kind is SyntaxKind.BangToken)
            if (t2Kind is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;

        if (t1Kind is SyntaxKind.EqualsToken)
            if (t2Kind is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;

        if (t1Kind is SyntaxKind.WhitespaceToken && t2Kind is SyntaxKind.WhitespaceToken)
            return true;

        return false;
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

    private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
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