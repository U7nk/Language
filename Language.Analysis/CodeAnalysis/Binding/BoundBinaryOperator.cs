using System.Collections.Generic;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

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
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, TypeSymbol.BuiltIn.Int()),
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, TypeSymbol.BuiltIn.String()),
        new(BoundBinaryOperatorKind.Subtraction, SyntaxKind.MinusToken, TypeSymbol.BuiltIn.Int()),
        new(BoundBinaryOperatorKind.Multiplication, SyntaxKind.StarToken, TypeSymbol.BuiltIn.Int()),
        new(BoundBinaryOperatorKind.Division, SyntaxKind.SlashToken, TypeSymbol.BuiltIn.Int()),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, TypeSymbol.BuiltIn.Bool()),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, TypeSymbol.BuiltIn.Int()),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, TypeSymbol.BuiltIn.Int()),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, TypeSymbol.BuiltIn.Int()),
        
        new(BoundBinaryOperatorKind.LogicalAnd, SyntaxKind.AmpersandAmpersandToken, TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.LogicalOr, SyntaxKind.PipePipeToken, TypeSymbol.BuiltIn.Bool()),
        
        new(BoundBinaryOperatorKind.LessThan, SyntaxKind.LessThanToken, TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.LessThanOrEquals, SyntaxKind.LessThanOrEqualsToken, TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Bool()),
        
        new(BoundBinaryOperatorKind.GreaterThan, SyntaxKind.GreaterThanToken, TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.GreaterThanOrEquals, SyntaxKind.GreaterThanOrEqualsToken, TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Bool()),
        
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, TypeSymbol.BuiltIn.String(), TypeSymbol.BuiltIn.String(), TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Bool()),
        
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, TypeSymbol.BuiltIn.String(), TypeSymbol.BuiltIn.String(), TypeSymbol.BuiltIn.Bool()),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Int(), TypeSymbol.BuiltIn.Bool()),
    };

    internal static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
    {
        if (syntaxKind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken 
            && leftType.IsOutside(TypeSymbol.BuiltIn.All).OrEquals(TypeSymbol.BuiltIn.Object()) 
            && leftType.IsOutside(TypeSymbol.BuiltIn.All).OrEquals(TypeSymbol.BuiltIn.Object()))
        {
            return new BoundBinaryOperator(syntaxKind is SyntaxKind.EqualsEqualsToken 
                                               ? BoundBinaryOperatorKind.Equality 
                                               : BoundBinaryOperatorKind.Inequality,
                                           syntaxKind, leftType, rightType, TypeSymbol.BuiltIn.Bool());
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