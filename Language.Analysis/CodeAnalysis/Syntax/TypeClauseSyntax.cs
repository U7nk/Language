namespace Language.Analysis.CodeAnalysis.Syntax;

public class TypeClauseSyntax : SyntaxNode
{
    public TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colon, NamedTypeExpressionSyntax namedTypeExpression) : base(syntaxTree)
    {
        Colon = colon;
        NamedTypeExpression = namedTypeExpression;
    }

    public SyntaxToken Colon { get; }
    public NamedTypeExpressionSyntax NamedTypeExpression { get; }
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
}