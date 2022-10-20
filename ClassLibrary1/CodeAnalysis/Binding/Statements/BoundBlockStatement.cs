using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

public sealed class BoundBlockStatement : BoundStatement
{
    public ImmutableArray<BoundStatement> Statements { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.BlockStatement;

    public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
    {
        Statements = statements;
    }
}