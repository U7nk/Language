using System;
using System.Collections.Generic;

namespace Language.Analysis.CodeAnalysis.Syntax;

internal static class SyntaxFacts
{

    public static string StartTypeName => "Program";
    public static string MainMethodName => "main";
    public static string ScriptMainMethodName => "$main";
    
    public static int GetUnaryOperatorPrecedence(this SyntaxKind syntaxKind)
    {
        switch (syntaxKind)
        {
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.BangToken:
            case SyntaxKind.TildeToken:
                return 6;

            default:
                return 0;
        }
    }

    public static int GetBinaryOperatorPrecedence(this SyntaxKind syntaxKind)
    {
        switch (syntaxKind)
        {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
                return 5;

            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 4;

            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.LessToken:
            case SyntaxKind.LessOrEqualsToken:
            case SyntaxKind.GreaterToken:
            case SyntaxKind.GreaterOrEqualsToken:
                return 3;

            case SyntaxKind.AmpersandToken:
            case SyntaxKind.AmpersandAmpersandToken:
                return 2;
            
            case SyntaxKind.HatToken:
            case SyntaxKind.PipeToken:
            case SyntaxKind.PipePipeToken:
                return 1;

            default:
                return 0;
        }
    }
    
    public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (GetUnaryOperatorPrecedence(kind) > 0)
            {
                yield return kind;
            }
        }
    }
    
    public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (GetBinaryOperatorPrecedence(kind) > 0)
            {
                yield return kind;
            }
        }
    }

    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch
        {
            "true" => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "let" => SyntaxKind.LetKeyword,
            "var" => SyntaxKind.VarKeyword,
            "if" => SyntaxKind.IfKeyword,
            "else" => SyntaxKind.ElseKeyword,
            "while" => SyntaxKind.WhileKeyword,
            "for" => SyntaxKind.ForKeyword,
            "function" => SyntaxKind.FunctionKeyword,
            "break" => SyntaxKind.BreakKeyword,
            "continue" => SyntaxKind.ContinueKeyword,
            "return" => SyntaxKind.ReturnKeyword,
            "class" => SyntaxKind.ClassKeyword,
            "this" => SyntaxKind.ThisKeyword,
            "new" => SyntaxKind.NewKeyword,
            "static" => SyntaxKind.StaticKeyword,
            _ => SyntaxKind.IdentifierToken,
        };
    }

    public static string? GetText(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken => "+",
            SyntaxKind.MinusToken => "-",
            SyntaxKind.StarToken => "*",
            SyntaxKind.SlashToken => "/",
            SyntaxKind.OpenParenthesisToken => "(",
            SyntaxKind.CloseParenthesisToken => ")",
            SyntaxKind.OpenBraceToken => "{",
            SyntaxKind.CloseBraceToken => "}",
            SyntaxKind.SemicolonToken => ";",
            SyntaxKind.ColonToken => ":",
            SyntaxKind.CommaToken => ",",
            SyntaxKind.BangToken => "!",
            SyntaxKind.AmpersandAmpersandToken => "&&",
            SyntaxKind.PipePipeToken => "||",
            SyntaxKind.EqualsEqualsToken => "==",
            SyntaxKind.BangEqualsToken => "!=",
            SyntaxKind.EqualsToken => "=",
            SyntaxKind.LessToken => "<",
            SyntaxKind.LessOrEqualsToken => "<=",
            SyntaxKind.GreaterToken => ">",
            SyntaxKind.GreaterOrEqualsToken => ">=",
            SyntaxKind.PipeToken => "|",
            SyntaxKind.AmpersandToken => "&",
            SyntaxKind.HatToken => "^",
            SyntaxKind.TildeToken => "~",
            SyntaxKind.DotToken => ".",
            
            SyntaxKind.TrueKeyword => "true",
            SyntaxKind.FalseKeyword => "false",
            SyntaxKind.LetKeyword => "let",
            SyntaxKind.VarKeyword => "var",
            SyntaxKind.IfKeyword => "if",
            SyntaxKind.ElseKeyword => "else",
            SyntaxKind.WhileKeyword => "while",
            SyntaxKind.ForKeyword => "for",
            SyntaxKind.FunctionKeyword => "function",
            SyntaxKind.ContinueKeyword => "continue",
            SyntaxKind.BreakKeyword => "break",
            SyntaxKind.ReturnKeyword => "return",
            SyntaxKind.ClassKeyword => "class",
            SyntaxKind.ThisKeyword => "this",
            SyntaxKind.NewKeyword => "new",
            SyntaxKind.StaticKeyword => "static",
            _ => null
        };
    }
}