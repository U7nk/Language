namespace Wired.CodeAnalysis.Syntax;

public class ParameterSyntax : SyntaxNode
{
    public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax type)
    {
        Identifier = identifier;
        Type = type;
    }

    public TypeClauseSyntax Type { get; }

    public SyntaxToken Identifier { get; }

    public override SyntaxKind Kind => SyntaxKind.Parameter;
}