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
        this.SeparatorsAndNodes = separatorsAndNodes;
    }

    public int Count => (this.SeparatorsAndNodes.Length + 1) / 2;
    public T this[int index] => (T)this.SeparatorsAndNodes[index * 2];

    public SyntaxToken? GetSeparator(int index)
    {
        if (index == this.Count - 1)
            return null;

        return this.SeparatorsAndNodes[index * 2 + 1] as SyntaxToken
               ?? throw new InvalidOperationException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < this.Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public override ImmutableArray<SyntaxNode> GetWithSeparators() 
        => this.SeparatorsAndNodes;
}