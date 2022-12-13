using System;
using System.Diagnostics;

namespace Language.Analysis.CodeAnalysis.Syntax;



public class MethodDeclarationSyntax : SyntaxNode, ITopMemberDeclarationSyntax, IClassMemberDeclarationSyntax
{
    public MethodDeclarationSyntax(SyntaxTree syntaxTree,
                                   SyntaxToken? staticKeyword, 
                                   SyntaxToken functionKeyword,
                                   SyntaxToken virtualKeyword,
                                   SyntaxToken overrideKeyword,
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
        VirtualKeyword = virtualKeyword;
        OverrideKeyword = overrideKeyword;
        Identifier = identifier;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
        ReturnType = returnType;
        Body = body;
    }

    public SyntaxToken? StaticKeyword { get; }
    public SyntaxToken FunctionKeyword { get; }
    public SyntaxToken? VirtualKeyword { get; }
    public SyntaxToken? OverrideKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public TypeClauseSyntax? ReturnType { get; }
    public BlockStatementSyntax Body { get; }
    
    public override SyntaxKind Kind => SyntaxKind.MethodDeclaration;
}