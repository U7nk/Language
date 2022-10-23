namespace Language.Analysis.CodeAnalysis.Syntax;

public class ParameterSyntax : SyntaxNode
{
    public ParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeClauseSyntax type) : base(syntaxTree)
    {
        Identifier = identifier;
        Type = type;
    }

    public TypeClauseSyntax Type { get; }

    public SyntaxToken Identifier { get; }

    public override SyntaxKind Kind => SyntaxKind.Parameter;
}