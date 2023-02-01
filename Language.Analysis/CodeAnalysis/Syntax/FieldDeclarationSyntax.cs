namespace Language.Analysis.CodeAnalysis.Syntax;

public class FieldDeclarationSyntax : SyntaxNode, IClassMemberDeclarationSyntax
{
    public FieldDeclarationSyntax(
        SyntaxTree syntaxTree,
        Option<SyntaxToken> staticKeyword, SyntaxToken identifier, 
        TypeClauseSyntax typeClause, SyntaxToken semicolonToken) : base(syntaxTree)
    {
        StaticKeyword = staticKeyword;
        Identifier = identifier;
        SemicolonToken = semicolonToken;
        TypeClause = typeClause;
    }
    
    public Option<SyntaxToken> StaticKeyword { get; }
    public SyntaxToken Identifier { get; }
    public TypeClauseSyntax TypeClause { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;
}