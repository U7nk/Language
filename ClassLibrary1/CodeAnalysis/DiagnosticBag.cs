using System;
using System.Collections;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis;

public class DiagnosticBag : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> diagnostics = new();


    public void Report(TextSpan textSpan, string message) 
        => this.diagnostics.Add(new Diagnostic(textSpan, message));
    
    public IEnumerator<Diagnostic> GetEnumerator() => this.diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public void AddRange(IEnumerable<Diagnostic> diagnosticsEnumerable) 
        => this.diagnostics.AddRange(diagnosticsEnumerable);
    
    
    public void ReportInvalidNumber(int start, int length, string text, Type type)
    {
        var message = $"Number '{text}' is not a valid '{type}'.";
        this.Report(new TextSpan(start, length), message);
    }

    public void ReportBadCharacter(int position, char character)
    {
        var message = $"error: bad character '{character}'.";
        this.Report(new TextSpan(position, 1), message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind token, SyntaxKind expected)
    {
        var message = $"error: Unexpected token <{token}> expected <{expected}>.";
        this.Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan operatorTokenSpan, string text, Type operandType)
    {
        var message = $"Invalid unary operator '{text}' for type '{operandType}'.";
        this.Report(operatorTokenSpan, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, Type leftType, Type rightType)
    {
        var message = $"Invalid binary operator '{operatorText}' for types '{leftType}' and '{rightType}'.";
        this.Report(span, message);
    }

    public void ReportUndefinedName(TextSpan identifierTokenSpan, string name)
    {
        var message = $"'{name}' is undefined.";
        this.Report(identifierTokenSpan, message);
    }

    public void ReportVariableAlreadyDeclared(TextSpan span, string name)
    { 
        var message = $"Variable '{name}' is already declared.";
        this.Report(span, message);
    }

    public void ReportCannotConvert(TextSpan expressionSpan, Type fromType, Type toType)
    {
        var message = $"Cannot convert '{fromType}' to '{toType}'.";
        this.Report(expressionSpan, message);
    }

    public void ReportCannotAssignToReadonly(TextSpan span, string name)
    {
        var message = $"'{name}' is readonly and cannot be assigned to.";
        this.Report(span, message);
    }

    public void VariableDoesntExistsInCurrentScope(TextSpan identifierTokenSpan, string name)
    {
        var message = $"'{name}' doesn't exists in current scope.";
        this.Report(identifierTokenSpan, message);
    }
}