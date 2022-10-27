using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class ClassDeclarationSyntax : SyntaxNode, ITopMemberSyntax
{
    public ClassDeclarationSyntax(
        SyntaxTree syntaxTree, SyntaxToken classKeyword,
        SyntaxToken identifier, SyntaxToken openBraceToken,
        ImmutableArray<MethodDeclarationSyntax> methods, ImmutableArray<FieldDeclarationSyntax> fields,
        SyntaxToken closeBraceToken) : base(syntaxTree)
    {
        ClassKeyword = classKeyword;
        Identifier = identifier;
        OpenBraceToken = openBraceToken;
        Methods = methods;
        CloseBraceToken = closeBraceToken;
        Fields = fields;
    }

    public SyntaxToken ClassKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<MethodDeclarationSyntax> Methods { get; }
    public ImmutableArray<FieldDeclarationSyntax> Fields { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;
}