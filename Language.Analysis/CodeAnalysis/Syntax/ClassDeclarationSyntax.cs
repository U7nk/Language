using System.Collections.Immutable;

namespace Language.CodeAnalysis.Syntax;

public class ClassDeclarationSyntax : SyntaxNode, ITopMemberSyntax
{
    public ClassDeclarationSyntax(
        SyntaxTree syntaxTree, SyntaxToken classKeyword,
        SyntaxToken identifier, SyntaxToken openBraceToken,
        ImmutableArray<FunctionDeclarationSyntax> functions, ImmutableArray<FieldDeclarationSyntax> fields,
        SyntaxToken closeBraceToken) : base(syntaxTree)
    {
        ClassKeyword = classKeyword;
        Identifier = identifier;
        OpenBraceToken = openBraceToken;
        Functions = functions;
        CloseBraceToken = closeBraceToken;
        Fields = fields;
    }

    public SyntaxToken ClassKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<FunctionDeclarationSyntax> Functions { get; }
    public ImmutableArray<FieldDeclarationSyntax> Fields { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;
}