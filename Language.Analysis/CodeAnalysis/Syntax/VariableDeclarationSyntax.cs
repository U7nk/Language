namespace Language.Analysis.CodeAnalysis.Syntax;

public class VariableDeclarationSyntax : SyntaxNode
{
    public VariableDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken keywordToken, SyntaxToken identifier,
        TypeClauseSyntax? typeClause) :
        base(syntaxTree)
    {
        KeywordToken = keywordToken;
        Identifier = identifier;
        TypeClause = typeClause;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationSyntax;
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken Identifier { get; }
    public TypeClauseSyntax? TypeClause { get; }
}