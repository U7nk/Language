using System.Collections.Immutable;
using System.Linq;

namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class CompilerGeneratedBlockOfGlobalStatementsSyntax : SyntaxNode
{
    public CompilerGeneratedBlockOfGlobalStatementsSyntax(SyntaxTree syntaxTree, ImmutableArray<GlobalStatementSyntax> statements) 
        : base(syntaxTree)
    {
        Statements = statements;
    }
    
    public override SyntaxKind Kind => SyntaxKind.GlobalStatementsDeclarationsBlockStatement;
    public ImmutableArray<GlobalStatementSyntax> Statements { get; }
}