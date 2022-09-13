using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Syntax;

public abstract class SeparatedSyntaxList
{
    public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
}
public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
    where T : SyntaxNode
{
    public ImmutableArray<SyntaxNode> SeparatorsAndNodes { get; }

    public SeparatedSyntaxList(ImmutableArray<SyntaxNode> separatorsAndNodes)
    {
        SeparatorsAndNodes = separatorsAndNodes;
    }

    public int Count => (SeparatorsAndNodes.Length + 1) / 2;
    public T this[int index] => (T)SeparatorsAndNodes[index * 2];

    public SyntaxToken? GetSeparator(int index)
    {
        if (index == Count - 1)
            return null;

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