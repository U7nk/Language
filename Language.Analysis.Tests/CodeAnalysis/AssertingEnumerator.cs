using FluentAssertions;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.Tests.CodeAnalysis;

internal sealed class AssertingEnumerator : IDisposable
{
    readonly IEnumerator<SyntaxNode> _enumerator;
    bool _hasErrors;

    public AssertingEnumerator(SyntaxNode node)
    {
        _enumerator = Flatten(node).GetEnumerator();
    }

    static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
    {
        var results = FlattenChildren(node).ToList();
        return results;
    }

    static IEnumerable<SyntaxNode> FlattenChildren(SyntaxNode node)
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
            _enumerator.MoveNext().Should().BeTrue();
            _enumerator.Current.Kind.Should().Be(kind);
            _enumerator.Current.Should().BeOfType<SyntaxToken>();
            var token = (SyntaxToken)_enumerator.Current;
            token.Text.Should().Be(text);
        }
        catch (Exception)
        {
            _hasErrors = true;
            throw;
        }
    }
        
    public void AssertNode(SyntaxKind kind)
    {
        try
        {
            _enumerator.MoveNext().Should().BeTrue();
            _enumerator.Current.Kind.Should().Be(kind);
            _enumerator.Current.Should().NotBeOfType<SyntaxToken>();
        }
        catch (Exception)
        {
            _hasErrors = true;
            throw;
        }
    }

    public void Dispose()
    {
        if (!_hasErrors)
        {
            _enumerator.MoveNext().Should().BeFalse();   
        }
        _enumerator.Dispose();
    }
}