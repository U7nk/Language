namespace Language.Analysis.CodeAnalysis.Syntax;

public class NamedTypeExpressionSyntax : SyntaxNode
{
    public NamedTypeExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, Option<GenericClauseSyntax> genericClause) : base(syntaxTree)
    {
        Identifier = identifier;
        GenericClause = genericClause;
    }

    public SyntaxToken Identifier { get; }
    public Option<GenericClauseSyntax> GenericClause { get; }
    public override SyntaxKind Kind => SyntaxKind.NamedTypeExpression;
}