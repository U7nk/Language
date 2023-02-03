﻿// <auto-generated/>
#nullable enable
using System;
namespace Language.Analysis.CodeAnalysis.Syntax;
public partial class SyntaxTree {
    public Language.Analysis.CodeAnalysis.Syntax.CompilationUnitSyntax NewCompilationUnit(System.Collections.Immutable.ImmutableArray<Language.Analysis.CodeAnalysis.Syntax.ITopMemberDeclarationSyntax> members,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken endOfFileToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.CompilationUnitSyntax(this,members,endOfFileToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.MethodDeclarationSyntax NewMethodDeclaration(Language.Analysis.Option<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> staticKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken functionKeyword,Language.Analysis.Option<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> virtualKeyword,Language.Analysis.Option<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> overrideKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.Option<Language.Analysis.CodeAnalysis.Syntax.GenericClauseSyntax> genericParametersSyntax,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openParenthesisToken,Language.Analysis.CodeAnalysis.Syntax.SeparatedSyntaxList<Language.Analysis.CodeAnalysis.Syntax.ParameterSyntax> parameters,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeParenthesisToken,Language.Analysis.CodeAnalysis.Syntax.TypeClauseSyntax? returnType,Language.Analysis.Option<System.Collections.Immutable.ImmutableArray<Language.Analysis.CodeAnalysis.Syntax.GenericConstraintsClauseSyntax>> genericConstraintsClauseSyntax,Language.Analysis.CodeAnalysis.Syntax.BlockStatementSyntax body)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.MethodDeclarationSyntax(this,staticKeyword,functionKeyword,virtualKeyword,overrideKeyword,identifier,genericParametersSyntax,openParenthesisToken,parameters,closeParenthesisToken,returnType,genericConstraintsClauseSyntax,body);
    }

    public Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationSyntax NewVariableDeclaration(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken keywordToken,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.CodeAnalysis.Syntax.TypeClauseSyntax? typeClause)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationSyntax(this,keywordToken,identifier,typeClause);
    }

    public Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationAssignmentSyntax NewVariableDeclarationAssignment(Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationSyntax variableDeclaration,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken equalsToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax expression)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationAssignmentSyntax(this,variableDeclaration,equalsToken,expression);
    }

    public Language.Analysis.CodeAnalysis.Syntax.TypeClauseSyntax NewTypeClause(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken colon,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.TypeClauseSyntax(this,colon,identifier);
    }

    public Language.Analysis.CodeAnalysis.Syntax.SyntaxToken New(Language.Analysis.CodeAnalysis.Syntax.SyntaxKind kind,int position,string text,object? value)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.SyntaxToken(this,kind,position,text,value);
    }

    public Language.Analysis.CodeAnalysis.Syntax.WhileStatementSyntax NewWhileStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken whileKeyword,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax condition,Language.Analysis.CodeAnalysis.Syntax.StatementSyntax body)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.WhileStatementSyntax(this,whileKeyword,condition,body);
    }

    public Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationStatementSyntax NewVariableDeclarationStatement(Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationSyntax variableDeclaration,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolonToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationStatementSyntax(this,variableDeclaration,semicolonToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationAssignmentStatementSyntax NewVariableDeclarationAssignmentStatement(Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationAssignmentSyntax variableDeclaration,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolonToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationAssignmentStatementSyntax(this,variableDeclaration,semicolonToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.BlockStatementSyntax NewBlockStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openBraceToken,System.Collections.Immutable.ImmutableArray<Language.Analysis.CodeAnalysis.Syntax.StatementSyntax> statements,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeBraceToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.BlockStatementSyntax(this,openBraceToken,statements,closeBraceToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.BreakStatementSyntax NewBreakStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken breakKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolonToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.BreakStatementSyntax(this,breakKeyword,semicolonToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ContinueStatementSyntax NewContinueStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken continueKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolonToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ContinueStatementSyntax(this,continueKeyword,semicolonToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ExpressionStatementSyntax NewExpressionStatement(Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax expression,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolonToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ExpressionStatementSyntax(this,expression,semicolonToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ForStatementSyntax NewForStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken forKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openParenthesis,Language.Analysis.CodeAnalysis.Syntax.VariableDeclarationAssignmentSyntax? variableDeclaration,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax? expression,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolon,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax condition,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken middleSemiColonToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax mutation,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeParenthesis,Language.Analysis.CodeAnalysis.Syntax.StatementSyntax body)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ForStatementSyntax(this,forKeyword,openParenthesis,variableDeclaration,expression,semicolon,condition,middleSemiColonToken,mutation,closeParenthesis,body);
    }

    public Language.Analysis.CodeAnalysis.Syntax.IfStatementSyntax NewIfStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken ifKeyword,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax condition,Language.Analysis.CodeAnalysis.Syntax.StatementSyntax thenStatement,Language.Analysis.CodeAnalysis.Syntax.ElseClauseSyntax? elseClause)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.IfStatementSyntax(this,ifKeyword,condition,thenStatement,elseClause);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ReturnStatementSyntax NewReturnStatement(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken returnKeyword,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax? expression,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolon)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ReturnStatementSyntax(this,returnKeyword,expression,semicolon);
    }

    public Language.Analysis.CodeAnalysis.Syntax.GlobalStatementDeclarationSyntax NewGlobalStatementDeclaration(Language.Analysis.CodeAnalysis.Syntax.StatementSyntax statement)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.GlobalStatementDeclarationSyntax(this,statement);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ParameterSyntax NewParameter(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.CodeAnalysis.Syntax.TypeClauseSyntax type)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ParameterSyntax(this,identifier,type);
    }

    public Language.Analysis.CodeAnalysis.Syntax.GenericClauseSyntax NewGenericClause(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken lessThanToken,Language.Analysis.CodeAnalysis.Syntax.SeparatedSyntaxList<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> arguments,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken greaterThanToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.GenericClauseSyntax(this,lessThanToken,arguments,greaterThanToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.InheritanceClauseSyntax NewInheritanceClause(Language.Analysis.CodeAnalysis.Syntax.SeparatedSyntaxList<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> baseTypes)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.InheritanceClauseSyntax(this,baseTypes);
    }

    public Language.Analysis.CodeAnalysis.Syntax.GenericConstraintsClauseSyntax NewGenericConstraintsClause(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken whereKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken colonToken,Language.Analysis.CodeAnalysis.Syntax.SeparatedSyntaxList<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> typeConstraints)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.GenericConstraintsClauseSyntax(this,whereKeyword,identifier,colonToken,typeConstraints);
    }

    public Language.Analysis.CodeAnalysis.Syntax.FieldDeclarationSyntax NewFieldDeclaration(Language.Analysis.Option<Language.Analysis.CodeAnalysis.Syntax.SyntaxToken> staticKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.CodeAnalysis.Syntax.TypeClauseSyntax typeClause,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken semicolonToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.FieldDeclarationSyntax(this,staticKeyword,identifier,typeClause,semicolonToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.UnaryExpressionSyntax NewUnaryExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken operatorToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax operand)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.UnaryExpressionSyntax(this,operatorToken,operand);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ThisExpressionSyntax NewThisExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken thisKeyword)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ThisExpressionSyntax(this,thisKeyword);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ParenthesizedExpressionSyntax NewParenthesizedExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openParenthesisToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax expression,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeParenthesisToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ParenthesizedExpressionSyntax(this,openParenthesisToken,expression,closeParenthesisToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ObjectCreationExpressionSyntax NewObjectCreationExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken newKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken typeIdentifier,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openParenthesis,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeParenthesis)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ObjectCreationExpressionSyntax(this,newKeyword,typeIdentifier,openParenthesis,closeParenthesis);
    }

    public Language.Analysis.CodeAnalysis.Syntax.NameExpressionSyntax NewNameExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.NameExpressionSyntax(this,identifier);
    }

    public Language.Analysis.CodeAnalysis.Syntax.MethodCallExpressionSyntax NewMethodCallExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.Option<Language.Analysis.CodeAnalysis.Syntax.GenericClauseSyntax> genericArgumentsSyntax,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openParenthesis,Language.Analysis.CodeAnalysis.Syntax.SeparatedSyntaxList<Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax> arguments,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeParenthesis)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.MethodCallExpressionSyntax(this,identifier,genericArgumentsSyntax,openParenthesis,arguments,closeParenthesis);
    }

    public Language.Analysis.CodeAnalysis.Syntax.MemberAssignmentExpressionSyntax NewMemberAssignmentExpression(Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax memberAccess,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken equalsToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax initializer)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.MemberAssignmentExpressionSyntax(this,memberAccess,equalsToken,initializer);
    }

    public Language.Analysis.CodeAnalysis.Syntax.MemberAccessExpressionSyntax NewMemberAccessExpression(Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax left,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken dot,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax right)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.MemberAccessExpressionSyntax(this,left,dot,right);
    }

    public Language.Analysis.CodeAnalysis.Syntax.LiteralExpressionSyntax NewLiteralExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken literalToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.LiteralExpressionSyntax(this,literalToken);
    }

    public Language.Analysis.CodeAnalysis.Syntax.AssignmentExpressionSyntax NewAssignmentExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifierToken,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken equalsToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax expression)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.AssignmentExpressionSyntax(this,identifierToken,equalsToken,expression);
    }

    public Language.Analysis.CodeAnalysis.Syntax.BinaryExpressionSyntax NewBinaryExpression(Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax left,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken operatorToken,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax right)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.BinaryExpressionSyntax(this,left,operatorToken,right);
    }

    public Language.Analysis.CodeAnalysis.Syntax.CastExpressionSyntax NewCastExpression(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openParenthesis,Language.Analysis.CodeAnalysis.Syntax.NameExpressionSyntax nameExpression,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeParenthesis,Language.Analysis.CodeAnalysis.Syntax.ExpressionSyntax castedExpression)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.CastExpressionSyntax(this,openParenthesis,nameExpression,closeParenthesis,castedExpression);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ElseClauseSyntax NewElseClause(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken elseKeyword,Language.Analysis.CodeAnalysis.Syntax.StatementSyntax elseStatement)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ElseClauseSyntax(this,elseKeyword,elseStatement);
    }

    public Language.Analysis.CodeAnalysis.Syntax.ClassDeclarationSyntax NewClassDeclaration(Language.Analysis.CodeAnalysis.Syntax.SyntaxToken classKeyword,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken identifier,Language.Analysis.CodeAnalysis.Syntax.InheritanceClauseSyntax? inheritanceClause,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken openBraceToken,System.Collections.Immutable.ImmutableArray<Language.Analysis.CodeAnalysis.Syntax.IClassMemberDeclarationSyntax> members,Language.Analysis.CodeAnalysis.Syntax.SyntaxToken closeBraceToken)
    {
       return new Language.Analysis.CodeAnalysis.Syntax.ClassDeclarationSyntax(this,classKeyword,identifier,inheritanceClause,openBraceToken,members,closeBraceToken);
    }
}