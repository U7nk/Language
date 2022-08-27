namespace Wired.CodeAnalysis.Syntax;

internal static class SyntaxFacts
{
    public static int GetUnaryOperatorPrecedence(this SyntaxKind syntaxKind)
    {
        switch (syntaxKind) {
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.BangToken:
                return 5;

            default:
                return 0;
        }
    }
    
    public static int GetBinaryOperatorPrecedence(this SyntaxKind syntaxKind)
    {
        switch (syntaxKind) {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
                return 4;
      
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 3;
            
            case SyntaxKind.AmpersandAmpersandToken:
                return 2;
            case SyntaxKind.PipePipeToken:
                return 1;

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