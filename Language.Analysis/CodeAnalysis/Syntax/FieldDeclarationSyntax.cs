namespace Language.Analysis.CodeAnalysis.Syntax;

public class FieldDeclarationSyntax : SyntaxNode, IClassMemberDeclarationSyntax
{
    public FieldDeclarationSyntax(
        SyntaxTree syntaxTree, SyntaxToken identifier, 
        TypeClauseSyntax typeClause, SyntaxToken semicolonToken) : base(syntaxTree)
    {
        Identifier = identifier;
        SemicolonToken = semicolonToken;
        TypeClause = typeClause;
    }
    
    public SyntaxToken Identifier { get; }
    public TypeClauseSyntax TypeClause { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;
}