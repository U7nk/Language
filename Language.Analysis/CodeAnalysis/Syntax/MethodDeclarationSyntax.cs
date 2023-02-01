using System;
using System.Diagnostics;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class GenericClauseSyntax : SyntaxNode
{
    public GenericClauseSyntax(SyntaxTree syntaxTree, SyntaxToken lessThanToken, SeparatedSyntaxList<SyntaxToken> arguments, SyntaxToken greaterThanToken) 
        : base(syntaxTree)
    {
        LessThanToken = lessThanToken;
        Arguments = arguments;
        GreaterThanToken = greaterThanToken;
    }

    public SyntaxToken LessThanToken { get; }
    public SeparatedSyntaxList<SyntaxToken> Arguments { get; }
    public SyntaxToken GreaterThanToken { get; } 
    public override SyntaxKind Kind => SyntaxKind.GenericArgumentsList;
}

public class MethodDeclarationSyntax : SyntaxNode, ITopMemberDeclarationSyntax, IClassMemberDeclarationSyntax
{
    public MethodDeclarationSyntax(SyntaxTree syntaxTree,
                                   Option<SyntaxToken> staticKeyword, 
                                   SyntaxToken functionKeyword,
                                   Option<SyntaxToken> virtualKeyword,
                                   Option<SyntaxToken> overrideKeyword,
                                   SyntaxToken identifier,
                                   Option<GenericClauseSyntax> genericParametersSyntax,
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
        GenericParametersSyntax = genericParametersSyntax;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
        ReturnType = returnType;
        Body = body;
    }

    public Option<SyntaxToken> StaticKeyword { get; }
    public SyntaxToken FunctionKeyword { get; }
    public Option<SyntaxToken> VirtualKeyword { get; }
    public Option<SyntaxToken> OverrideKeyword { get; }
    public SyntaxToken Identifier { get; }
    public Option<GenericClauseSyntax> GenericParametersSyntax { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public TypeClauseSyntax? ReturnType { get; }
    public BlockStatementSyntax Body { get; }
    
    public override SyntaxKind Kind => SyntaxKind.MethodDeclaration;
}