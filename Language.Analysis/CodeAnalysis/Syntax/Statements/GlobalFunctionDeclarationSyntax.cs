namespace Language.Analysis.CodeAnalysis.Syntax;

public class GlobalFunctionDeclarationSyntax : SyntaxNode, IGlobalMemberSyntax
{
    public MethodDeclarationSyntax FunctionDeclaration { get; }
    public override SyntaxKind Kind => SyntaxKind.GlobalFunctionStatement;

    public GlobalFunctionDeclarationSyntax(SyntaxTree syntaxTree, MethodDeclarationSyntax functionDeclaration) : base(syntaxTree)
    {
        FunctionDeclaration = functionDeclaration;
    }
}