using FluentAssertions;
using Language;
using Language.Analysis;
using Language.Analysis.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetBinaryOperatorPairsData))]
    public void Parser_BinaryExpression_HonorsPrecedence(SyntaxKind op1, SyntaxKind op2)
    {
        var op1Precedence = op1.GetBinaryOperatorPrecedence();
        var op2Precedence = op2.GetBinaryOperatorPrecedence();
        var op1Text = SyntaxFacts.GetText(op1).Unwrap();
        var op2Text = SyntaxFacts.GetText(op2).Unwrap();
        var text = $"a {op1Text} b {op2Text} c";
        var expression = ParseExpression(text);

        if (op1Precedence >= op2Precedence)
        {
            //          op2  
            //     /     |   \
            //    op1    +    c
            //   /  | \       |
            //  a   +  b    tok c
            //  |      |
            // tok a   tok b
            //
            // asserting nodes from up to down, left to right
            using var e = new AssertingEnumerator(expression);
            e.AssertNode(SyntaxKind.BinaryOperatorExpression);
            e.AssertNode(SyntaxKind.BinaryOperatorExpression);
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
            e.AssertNode(SyntaxKind.BinaryOperatorExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(op1, op1Text);
            e.AssertNode(SyntaxKind.BinaryOperatorExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(op2, op2Text);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
    }

    [Theory]
    [MemberData(nameof(GetUnaryBinaryOperatorPairsData))]
    public void Parser_UnaryExpression_HonorsPrecedence(SyntaxKind unaryKind, SyntaxKind binaryKind)
    {
        var unaryPrecedence = unaryKind.GetUnaryOperatorPrecedence();
        var binaryPrecedence = binaryKind.GetBinaryOperatorPrecedence();
        var unaryText = SyntaxFacts.GetText(unaryKind).Unwrap();
        var binaryText = SyntaxFacts.GetText(binaryKind).Unwrap();
        var text = $"{unaryText} a {binaryText} b";
        var expression = ParseExpression(text);

        if (unaryPrecedence >= binaryPrecedence)
        {
            // 
            //              binary
            //          /     |      \
            //      unary   binaryT   b
            //     /    \             |
            //  unaryT   a            bT
            //           |              
            //          aT           
            using var e = new AssertingEnumerator(expression);
            e.AssertNode(SyntaxKind.BinaryOperatorExpression);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(binaryKind, binaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(unaryKind, unaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
        }
        else
        {
            //       unary
            //    /        \         
            //   unaryT     binary
            //          /     |      \
            //         a    binaryT   b
            //         |              |
            //         aT             bT
            using var e = new AssertingEnumerator(expression);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText);
            e.AssertNode(SyntaxKind.BinaryOperatorExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(binaryKind, binaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
    }

    static ExpressionSyntax ParseExpression(string text)
    {
        var expression = SyntaxTree.Parse(text);
        var root = expression.Root;
        var statement = root.Members.Single();
        root.Members.Single().Should().BeOfType<GlobalStatementSyntax>();
        return ((ExpressionStatementSyntax)((GlobalStatementSyntax)statement).Statement).Expression;
    }

    public static IEnumerable<object[]> GetUnaryBinaryOperatorPairsData()
    {
        foreach (var op1 in SyntaxFacts.GetUnaryOperatorKinds())
        {
            foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
            {
                yield return new object[] { op1, op2 };
            }
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