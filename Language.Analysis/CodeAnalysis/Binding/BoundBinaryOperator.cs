using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundBinaryOperator
{
    public BoundBinaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public TypeSymbol LeftType { get; }
    public TypeSymbol RightType { get; }
    public TypeSymbol ResultType { get; }

    BoundBinaryOperator(BoundBinaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol left) :
        this(kind, syntaxKind, left, left, left)
    {
    }

    BoundBinaryOperator(BoundBinaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
    {
        Kind = kind;
        SyntaxKind = syntaxKind;
        LeftType = leftType;
        RightType = rightType;
        ResultType = resultType;
    }

    static readonly List<BoundBinaryOperator> _operators = new()
    {
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, BuiltInTypeSymbols.Int),
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, BuiltInTypeSymbols.String),
        new(BoundBinaryOperatorKind.Subtraction, SyntaxKind.MinusToken, BuiltInTypeSymbols.Int),
        new(BoundBinaryOperatorKind.Multiplication, SyntaxKind.StarToken, BuiltInTypeSymbols.Int),
        new(BoundBinaryOperatorKind.Division, SyntaxKind.SlashToken, BuiltInTypeSymbols.Int),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, BuiltInTypeSymbols.Bool),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, BuiltInTypeSymbols.Int),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, BuiltInTypeSymbols.Int),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, BuiltInTypeSymbols.Int),
        
        new(BoundBinaryOperatorKind.LogicalAnd, SyntaxKind.AmpersandAmpersandToken, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.LogicalOr, SyntaxKind.PipePipeToken, BuiltInTypeSymbols.Bool),
        
        new(BoundBinaryOperatorKind.LessThan, SyntaxKind.LessToken, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.LessThanOrEquals, SyntaxKind.LessOrEqualsToken, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Bool),
        
        new(BoundBinaryOperatorKind.GreaterThan, SyntaxKind.GreaterToken, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.GreaterThanOrEquals, SyntaxKind.GreaterOrEqualsToken, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Bool),
        
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, BuiltInTypeSymbols.String),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Bool),
        
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, BuiltInTypeSymbols.Bool),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, BuiltInTypeSymbols.String),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Int, BuiltInTypeSymbols.Bool),
    };

    internal static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
    {
        if (syntaxKind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken 
            && leftType.IsOutside(BuiltInTypeSymbols.All).OrEquals(BuiltInTypeSymbols.Object) 
            && leftType.IsOutside(BuiltInTypeSymbols.All).OrEquals(BuiltInTypeSymbols.Object))
        {
            return new BoundBinaryOperator(syntaxKind is SyntaxKind.EqualsEqualsToken 
                                               ? BoundBinaryOperatorKind.Equality 
                                               : BoundBinaryOperatorKind.Inequality,
                                           syntaxKind, leftType, rightType, BuiltInTypeSymbols.Bool);
        }
        
        foreach (var binaryOperator in _operators)
        {
            if (syntaxKind == binaryOperator.SyntaxKind && binaryOperator.LeftType == leftType && binaryOperator.RightType == rightType)
            {
                return binaryOperator;
            }
        }

        return null;
    }
}