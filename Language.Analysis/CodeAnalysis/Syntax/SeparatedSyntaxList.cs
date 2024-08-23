using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis.Syntax;

public abstract class SeparatedSyntaxList
{
    public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
}
public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
    where T : SyntaxNode
{
    public SeparatedSyntaxList(ImmutableArray<T> separatorsAndNodes)
    {
        SeparatorsAndNodes = separatorsAndNodes.CastArray<SyntaxNode>();
    }
    
    public SeparatedSyntaxList(ImmutableArray<SyntaxNode> separatorsAndNodes)
    {
        SeparatorsAndNodes = separatorsAndNodes;
    }

    public ImmutableArray<SyntaxNode> SeparatorsAndNodes { get; }
    public TextSpan Span => TextSpan.FromBounds(
        SeparatorsAndNodes.MinBy(x=> x.Span.Start)?.Span.Start ?? throw new InvalidOperationException(),
        SeparatorsAndNodes.MaxBy(x=> x.Span.End)?.Span.End ?? throw new InvalidOperationException());
    
    public int Count => (SeparatorsAndNodes.Length + 1) / 2;
    public T this[int index] => (T)SeparatorsAndNodes[index * 2];

    public Option<SyntaxToken> GetSeparator(int index)
    {
        if (index == Count - 1)
            return Option.None;

        return SeparatorsAndNodes[index * 2 + 1] as SyntaxToken
               ?? throw new InvalidOperationException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override ImmutableArray<SyntaxNode> GetWithSeparators() 
        => SeparatorsAndNodes;
}