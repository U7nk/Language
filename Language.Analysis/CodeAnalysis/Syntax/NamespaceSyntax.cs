namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class NamespaceSyntax : SyntaxNode
{
    public NamespaceSyntax(SyntaxTree syntaxTree, SeparatedSyntaxList<SyntaxToken> name) : base(syntaxTree)
    {
        Name = name;
    }

    public override SyntaxKind Kind => SyntaxKind.NamespaceSyntax;
    public SeparatedSyntaxList<SyntaxToken> Name { get; }
}