using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class ClassDeclarationSyntax : SyntaxNode, ITopMemberDeclarationSyntax
{
    public ClassDeclarationSyntax(
        SyntaxTree syntaxTree, SyntaxToken classKeyword,
        SyntaxToken identifier, InheritanceClauseSyntax? inheritanceClause,
        SyntaxToken openBraceToken,
        ImmutableArray<IClassMemberDeclarationSyntax> members,
        SyntaxToken closeBraceToken) : base(syntaxTree)
    {
        ClassKeyword = classKeyword;
        Identifier = identifier;
        InheritanceClause = inheritanceClause;
        OpenBraceToken = openBraceToken;
        CloseBraceToken = closeBraceToken;
        Members = members;
    }

    public SyntaxToken ClassKeyword { get; }
    public SyntaxToken Identifier { get; }
    public InheritanceClauseSyntax? InheritanceClause { get; }
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<IClassMemberDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;
}