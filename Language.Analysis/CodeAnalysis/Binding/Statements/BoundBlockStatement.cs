using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public sealed class BoundBlockStatement : BoundStatement
{
    public ImmutableArray<BoundStatement> Statements { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.BlockStatement;

    public BoundBlockStatement(SyntaxNode? syntax, ImmutableArray<BoundStatement> statements) : base(syntax)
    {
        Statements = statements;
    }
}