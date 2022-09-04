using FluentAssertions;
using Wired;
using Wired.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

public class ParserTests
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        private readonly IEnumerator<SyntaxNode> enumerator;
        private bool hasErrors;

        public AssertingEnumerator(SyntaxNode node)
        {
            this.enumerator = Flatten(node).GetEnumerator();
        }

        private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
        {
            var results = FlattenChildren(node).ToList();
            return results;
        }

        private static IEnumerable<SyntaxNode> FlattenChildren(SyntaxNode node)
        {   //                             op2 op1 v c a b d  
            //         op2                  op2x op1 v c
            //     /    |  \               
            //    op1   v   c
            //   /   \      |
            //  a     b     d
            var stack = new Queue<SyntaxNode>();
            stack.Enqueue(node);
            while (stack.Count > 0)
            {
                var n = stack.Dequeue();
                yield return n;
                foreach (var child in n.GetChildren())
                {
                    stack.Enqueue(child);
                }
            }
        }
        
        public void AssertToken(SyntaxKind kind, string text)
        {
            try
            {
                this.enumerator.MoveNext().Should().BeTrue();
                this.enumerator.Current.Kind.Should().Be(kind);
                this.enumerator.Current.Should().BeOfType<SyntaxToken>();
                var token = (SyntaxToken)this.enumerator.Current;
                token.Text.Should().Be(text);
            }
            catch (Exception)
            {
                this.hasErrors = true;
                throw;
            }
        }
        
        public void AssertNode(SyntaxKind kind)
        {
            try
            {
                this.enumerator.MoveNext().Should().BeTrue();
                this.enumerator.Current.Kind.Should().Be(kind);
                this.enumerator.Current.Should().NotBeOfType<SyntaxToken>();
            }
            catch (Exception)
            {
                this.hasErrors = true;
                throw;
            }
        }

        public void Dispose()
        {
            if (!this.hasErrors)
            {
                this.enumerator.MoveNext().Should().BeFalse();   
            }
            this.enumerator.Dispose();
        }
    }
    
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