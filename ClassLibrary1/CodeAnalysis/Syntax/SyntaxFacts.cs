namespace Wired.CodeAnalysis.Syntax;

internal static class SyntaxFacts
{
    public static int GetBinaryOperatorPrecedence(this SyntaxKind syntaxKind)
    {
        switch (syntaxKind) {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
                return 2;
      
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 1;

            default:
                return 0;
        }
    }
    
    public static int GetUnaryOperatorPrecedence(this SyntaxKind syntaxKind)
    {
        switch (syntaxKind) {
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 3;

            default:
                return 0;
        }
    }

    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch {
            "true" => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            _ => SyntaxKind.IdentifierToken,
        };
    }
}