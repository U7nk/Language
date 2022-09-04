using FluentAssertions;
using Wired.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

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