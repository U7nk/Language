using Wired.CodeAnalysis.Binding;

namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationSyntax : SyntaxNode
{
    public VariableDeclarationSyntax(SyntaxToken keywordToken, SyntaxToken identifierToken,
        TypeClauseSyntax? typeClause)
    {
        KeywordToken = keywordToken;
        IdentifierToken = identifierToken;
        TypeClause = typeClause;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationSyntax;
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    public TypeClauseSyntax? TypeClause { get; }
}