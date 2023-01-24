using System.Collections.Generic;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class InheritanceClauseSyntax : SyntaxNode
{
    public SeparatedSyntaxList<SyntaxToken> BaseTypes { get; }

    public InheritanceClauseSyntax(SyntaxTree syntaxTree, SeparatedSyntaxList<SyntaxToken> baseTypes) 
        : base(syntaxTree)
    {
        BaseTypes = baseTypes;
    }

    public override SyntaxKind Kind => SyntaxKind.InheritanceClause;
}