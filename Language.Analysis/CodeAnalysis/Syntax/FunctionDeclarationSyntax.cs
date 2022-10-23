namespace Language.Analysis.CodeAnalysis.Syntax;



public class FunctionDeclarationSyntax : SyntaxNode, ITopMemberSyntax, IClassMemberSyntax
{
    public FunctionDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken functionKeyword, SyntaxToken identifier,
        SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters,
        SyntaxToken closeParenthesisToken, TypeClauseSyntax? returnType,
        BlockStatementSyntax body) 
        : base(syntaxTree)
    {
        FunctionKeyword = functionKeyword;
        Identifier = identifier;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
        ReturnType = returnType;
        Body = body;
    }
    
    public SyntaxToken FunctionKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public TypeClauseSyntax? ReturnType { get; }
    public BlockStatementSyntax Body { get; }
    
    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
}