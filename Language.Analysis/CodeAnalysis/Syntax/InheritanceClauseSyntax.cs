namespace Language.Analysis.CodeAnalysis.Syntax;

public class InheritanceClauseSyntax : SyntaxNode
{
    public SyntaxToken ColonToken { get; }
    public SyntaxToken BaseTypeIdentifier { get; }

    public InheritanceClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken baseTypeIdentifier) 
        : base(syntaxTree)
    {
        ColonToken = colonToken;
        BaseTypeIdentifier = baseTypeIdentifier;
    }

    public override SyntaxKind Kind => SyntaxKind.InheritanceClause;
}