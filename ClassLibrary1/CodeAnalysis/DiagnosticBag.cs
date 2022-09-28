using System;
using System.Collections;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis;

public class DiagnosticBag : IEnumerable<Diagnostic>
{
    readonly List<Diagnostic> _diagnostics = new();


    public void Report(TextLocation textLocation, string message) 
        => _diagnostics.Add(new Diagnostic(textLocation, message));
    
    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void AddRange(IEnumerable<Diagnostic> diagnosticsEnumerable) 
        => _diagnostics.AddRange(diagnosticsEnumerable);
    
    
    public void ReportInvalidNumber(TextLocation textLocation, string text, Type type)
    {
        var message = $"Number '{text}' is not a valid '{type}'.";
        Report(textLocation, message);
    }

    public void ReportBadCharacter(TextLocation textLocation, char character)
    {
        var message = $"error: bad character '{character}'.";
        Report(textLocation, message);
    }

    public void ReportUnexpectedToken(TextLocation textLocation, SyntaxKind token, SyntaxKind expected)
    {
        var message = $"error: Unexpected token <{token}> expected <{expected}>.";
        Report(textLocation, message);
    }

    public void ReportUndefinedUnaryOperator(TextLocation location, string text, TypeSymbol operandType)
    {
        var message = $"Invalid unary operator '{text}' for type '{operandType}'.";
        Report(location, message);
    }

    public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
    {
        var message = $"Invalid binary operator '{operatorText}' for types '{leftType}' and '{rightType}'.";
        Report(location, message);
    }

    public void ReportUndefinedName(TextLocation location, string name)
    {
        var message = $"'{name}' is undefined.";
        Report(location, message);
    }

    public void ReportVariableAlreadyDeclared(TextLocation location, string name)
    { 
        var message = $"Variable '{name}' is already declared.";
        Report(location, message);
    }

    public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert '{fromType}' to '{toType}'.";
        Report(location, message);
    }

    public void ReportCannotAssignToReadonly(TextLocation location, string name)
    {
        var message = $"'{name}' is readonly and cannot be assigned to.";
        Report(location, message);
    }

    public void VariableDoesntExistsInCurrentScope(TextLocation location, string name)
    {
        var message = $"'{name}' doesn't exists in current scope.";
        Report(location, message);
    }

    public void ReportUnterminatedString(TextLocation textLocation)
    {
        var message = "Unterminated string literal.";
        Report(textLocation, message);
    }

    public void ReportUndefinedFunction(TextLocation location, string identifierText)
    {
        var message = $"Function '{identifierText}' is undefined.";
        Report(location, message);
    }

    public void ReportParameterCountMismatch(TextLocation location, string identifierText, int parametersLength, int argumentsCount)
    {
        var message = $"Function '{identifierText}' requires {parametersLength} arguments but was given {argumentsCount}.";
        Report(location, message);
    }

    public void ReportExpressionMustHaveValue(TextLocation location)
    {
        var message = $"Expression must have a value.";
        Report(location, message);
    }

    public void ReportUndefinedType(TextLocation location, string identifierText)
    {
        var message = $"Type '{identifierText}' is undefined.";
        Report(location, message);
    }

    public void ReportNoImplicitConversion(TextLocation location, TypeSymbol expressionType, TypeSymbol type)
    {
        var message = $"No implicit conversion from '{expressionType}' to '{type}'.";
        Report(location, message);
    }

    public void ReportTypeClauseExpected(TextLocation location)
    {
        var message = $"Type clause expected.";
        Report(location, message);
    }

    public void ReportParameterAlreadyDeclared(TextLocation location, string name)
    {
        var message = $"Parameter '{name}' is already declared.";
        Report(location, message);
    }

    public void ReportFunctionAlreadyDeclared(TextLocation location, string identifierText)
    {
        var message = $"Function '{identifierText}' is already declared.";
        Report(location, message);
    }

    public void ReportInvalidBreakOrContinue(SyntaxToken breakKeyword)
    {
        var message = $"Invalid use of '{breakKeyword.Text}'. Must be inside a loop.";
        Report(breakKeyword.Location, message);
    }

    public void ReportReturnStatementIsInvalidForVoidFunction(TextLocation location)
    {
        var message = $"Return statement is invalid for {TypeSymbol.Void} function.";
        Report(location, message);
    }

    public void ReportReturnStatementIsInvalidForNonVoidFunction(TextLocation location)
    {
        var message = $"return should have value for {TypeSymbol.Void} function.";
        Report(location, message);
    }

    public void ReportInvalidReturn(TextLocation location)
    {
        var message = $"Return statement should be inside a function.";
        Report(location, message);
        
    }

    public void ReportAllPathsMustReturn(TextLocation location)
    {
        var message = $"All paths must return a value.";
        Report(location, message);
    }

    public void ReportInvalidExpressionStatement(TextLocation syntaxLocation)
    {
        var message = "Only assignment, and call expressions can be used as a statement.";
        Report(syntaxLocation, message);
    }
}