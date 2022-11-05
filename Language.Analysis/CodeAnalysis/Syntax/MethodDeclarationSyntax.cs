namespace Language.Analysis.CodeAnalysis.Syntax;



public class MethodDeclarationSyntax : SyntaxNode, ITopMemberDeclarationSyntax, IClassMemberDeclarationSyntax
{
    public MethodDeclarationSyntax(SyntaxTree syntaxTree,
                                   SyntaxToken? staticKeyword, 
                                   SyntaxToken functionKeyword, 
                                   SyntaxToken identifier,
                                   SyntaxToken openParenthesisToken,
                                   SeparatedSyntaxList<ParameterSyntax> parameters, 
                                   SyntaxToken closeParenthesisToken, 
                                   TypeClauseSyntax? returnType,
                                   BlockStatementSyntax body) 
        : base(syntaxTree)
    {
        StaticKeyword = staticKeyword;
        FunctionKeyword = functionKeyword;
        Identifier = identifier;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
        ReturnType = returnType;
        Body = body;
    }

    public SyntaxToken? StaticKeyword { get; }
    public SyntaxToken FunctionKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public TypeClauseSyntax? ReturnType { get; }
    public BlockStatementSyntax Body { get; }
    
    public override SyntaxKind Kind => SyntaxKind.MethodDeclaration;
}