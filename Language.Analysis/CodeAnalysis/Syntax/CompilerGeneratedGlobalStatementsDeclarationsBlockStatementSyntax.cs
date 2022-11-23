using System.Collections.Immutable;
using System.Linq;

namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class CompilerGeneratedGlobalStatementsDeclarationsBlockStatementSyntax : SyntaxNode
{
    public CompilerGeneratedGlobalStatementsDeclarationsBlockStatementSyntax(ImmutableArray<GlobalStatementDeclarationSyntax> statements) 
        : base(statements.First().SyntaxTree)
    {
        Statements = statements;
    }
    
    public override SyntaxKind Kind => SyntaxKind.GlobalStatementsDeclarationsBlockStatement;
    public ImmutableArray<GlobalStatementDeclarationSyntax> Statements { get; }
}