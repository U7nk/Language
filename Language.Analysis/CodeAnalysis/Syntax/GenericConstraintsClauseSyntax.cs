namespace Language.Analysis.CodeAnalysis.Syntax;

public class GenericConstraintsClauseSyntax : SyntaxNode 
{
    public GenericConstraintsClauseSyntax(SyntaxTree syntaxTree, SyntaxToken whereKeyword, SyntaxToken identifier, SyntaxToken colonToken, SeparatedSyntaxList<NamedTypeExpressionSyntax> typeConstraints) 
        : base(syntaxTree)
    {
        WhereKeyword = whereKeyword;
        Identifier = identifier;
        ColonToken = colonToken;
        TypeConstraints = typeConstraints;
    }
    
    public SyntaxToken WhereKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken ColonToken { get; }
    public SeparatedSyntaxList<NamedTypeExpressionSyntax> TypeConstraints { get; }
    public override SyntaxKind Kind => SyntaxKind.GenericConstraintsClause;
}