using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class ClassDeclarationSyntax : SyntaxNode
{
    public ClassDeclarationSyntax(
        SyntaxTree syntaxTree, SyntaxToken classKeyword,
        SyntaxToken identifier, Option<GenericClauseSyntax> genericParametersSyntax, 
        InheritanceClauseSyntax? inheritanceClause, Option<ImmutableArray<GenericConstraintsClauseSyntax>> genericConstraintsClause,
        SyntaxToken openBraceToken, ImmutableArray<IClassMemberDeclarationSyntax> members,
        SyntaxToken closeBraceToken) 
        : base(syntaxTree)
    {
        ClassKeyword = classKeyword;
        Identifier = identifier;
        InheritanceClause = inheritanceClause;
        OpenBraceToken = openBraceToken;
        CloseBraceToken = closeBraceToken;
        GenericConstraintsClause = genericConstraintsClause;
        GenericParametersSyntax = genericParametersSyntax;
        Members = members;
    }

    public SyntaxToken ClassKeyword { get; }
    public SyntaxToken Identifier { get; }
    public Option<GenericClauseSyntax> GenericParametersSyntax { get; }
    public InheritanceClauseSyntax? InheritanceClause { get; }
    public Option<ImmutableArray<GenericConstraintsClauseSyntax>> GenericConstraintsClause { get; }
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<IClassMemberDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;
}