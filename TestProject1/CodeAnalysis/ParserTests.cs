using Wired;
using Wired.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetBinaryOperatorPairsData))]
    public void Parser_BinaryExpression_HonorsPrecedence(SyntaxKind op1, SyntaxKind op2)
    {
        var op1Precedence = op1.GetBinaryOperatorPrecedence();
        var op2Precedence = op2.GetBinaryOperatorPrecedence();
        var op1Text = SyntaxFacts.GetText(op1);
        var op2Text = SyntaxFacts.GetText(op2);
        var text = $"a {op1Text} b {op2Text} c";
        var expression = SyntaxTree.Parse(text).Root;

        if (op1Precedence >= op2Precedence)
        {
            
            //          op2  
            //     /     |   \
            //    op1    +    c
            //   /  | \       |
            //  a   +  b    tok c
            //  |      |
            // tok a   tok b
            using var e = new AssertingEnumerator(expression);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertToken(op2, op2Text);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(op1, op1Text);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "c");
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
        else
        {
            //     op1
            //  /   |    \
            // a   op1T  op2
            // |       /  |  \
            // aT     b  op2T c
            //        |       |
            //        bT      cT
            using var e = new AssertingEnumerator(expression);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(op1, op1Text);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(op2, op2Text);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
    }

    public static IEnumerable<object[]> GetBinaryOperatorPairsData()
    {
        foreach (var op1 in SyntaxFacts.GetBinaryOperatorKinds())
        {
            foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
            {
                yield return new object[] { op1, op2 };
            }   
        }
    }
}