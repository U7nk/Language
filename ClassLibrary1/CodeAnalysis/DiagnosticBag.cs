using System;
using System.Collections;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis;

public class DiagnosticBag : IEnumerable<Diagnostic>
{
    readonly List<Diagnostic> _diagnostics = new();


    public void Report(TextSpan textSpan, string message) 
        => _diagnostics.Add(new Diagnostic(textSpan, message));
    
    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void AddRange(IEnumerable<Diagnostic> diagnosticsEnumerable) 
        => _diagnostics.AddRange(diagnosticsEnumerable);
    
    
    public void ReportInvalidNumber(int start, int length, string text, Type type)
    {
        var message = $"Number '{text}' is not a valid '{type}'.";
        Report(new TextSpan(start, length), message);
    }

    public void ReportBadCharacter(int position, char character)
    {
        var message = $"error: bad character '{character}'.";
        Report(new TextSpan(position, 1), message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind token, SyntaxKind expected)
    {
        var message = $"error: Unexpected token <{token}> expected <{expected}>.";
        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan operatorTokenSpan, string text, TypeSymbol operandType)
    {
        var message = $"Invalid unary operator '{text}' for type '{operandType}'.";
        Report(operatorTokenSpan, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
    {
        var message = $"Invalid binary operator '{operatorText}' for types '{leftType}' and '{rightType}'.";
        Report(span, message);
    }

    public void ReportUndefinedName(TextSpan identifierTokenSpan, string name)
    {
        var message = $"'{name}' is undefined.";
        Report(identifierTokenSpan, message);
    }

    public void ReportVariableAlreadyDeclared(TextSpan span, string name)
    { 
        var message = $"Variable '{name}' is already declared.";
        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan expressionSpan, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert '{fromType}' to '{toType}'.";
        Report(expressionSpan, message);
    }

    public void ReportCannotAssignToReadonly(TextSpan span, string name)
    {
        var message = $"'{name}' is readonly and cannot be assigned to.";
        Report(span, message);
    }

    public void VariableDoesntExistsInCurrentScope(TextSpan identifierTokenSpan, string name)
    {
        var message = $"'{name}' doesn't exists in current scope.";
        Report(identifierTokenSpan, message);
    }

    public void ReportUnterminatedString(TextSpan span)
    {
        var message = "Unterminated string literal.";
        Report(span, message);
    }

    public void ReportUndefinedFunction(TextSpan identifierSpan, string identifierText)
    {
        var message = $"Function '{identifierText}' is undefined.";
        Report(identifierSpan, message);
    }

    public void ReportParameterCountMismatch(TextSpan identifierSpan, string identifierText, int parametersLength, int argumentsCount)
    {
        var message = $"Function '{identifierText}' requires {parametersLength} arguments but was given {argumentsCount}.";
        Report(identifierSpan, message);
    }

    public void ReportExpressionMustHaveValue(TextSpan initializerSpan)
    {
        var message = $"Expression must have a value.";
        Report(initializerSpan, message);
    }

    public void ReportUndefinedType(TextSpan identifierSpan, string identifierText)
    {
        var message = $"Type '{identifierText}' is undefined.";
        Report(identifierSpan, message);
    }

    public void ReportNoImplicitConversion(TextSpan diagnosticSpan, TypeSymbol expressionType, TypeSymbol type)
    {
        var message = $"No implicit conversion from '{expressionType}' to '{type}'.";
        Report(diagnosticSpan, message);
    }

    public void ReportTypeClauseExpected(TextSpan identifierSpan)
    {
        var message = $"Type clause expected.";
        Report(identifierSpan, message);
    }

    public void ReportParameterAlreadyDeclared(TextSpan span, string name)
    {
        var message = $"Parameter '{name}' is already declared.";
        Report(span, message);
    }

    public void ReportFunctionAlreadyDeclared(TextSpan identifierSpan, string identifierText)
    {
        var message = $"Function '{identifierText}' is already declared.";
        Report(identifierSpan, message);
    }

    public void ReportInvalidBreakOrContinue(SyntaxToken breakKeyword)
    {
        var message = $"Invalid use of '{breakKeyword.Text}'. Must be inside a loop.";
        Report(breakKeyword.Span, message);
    }

    public void ReportReturnStatementIsInvalidForVoidFunction(TextSpan syntaxSpan)
    {
        var message = $"Return statement is invalid for {TypeSymbol.Void} function.";
        Report(syntaxSpan, message);
    }

    public void ReportReturnStatementIsInvalidForNonVoidFunction(TextSpan syntaxSpan)
    {
        var message = $"return should have value for {TypeSymbol.Void} function.";
        Report(syntaxSpan, message);
    }

    public void ReportInvalidReturn(TextSpan returnKeywordSpan)
    {
        var message = $"Return statement should be inside a function.";
        Report(returnKeywordSpan, message);
        
    }
}