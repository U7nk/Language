using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis;

public class DiagnosticBag : List<Diagnostic>
{
    public void Report(TextLocation textLocation, string message, string code)
    {
        var diagnostic = new Diagnostic(textLocation, message, code);
        ReportDiagnostic(diagnostic);
    }

    void ReportDiagnostic(Diagnostic diagnostic)
    {
        var diagnosticHash = diagnostic.GetHashCode();
        var sameHashDiagnostics = this.Where(d => d.GetHashCode() == diagnosticHash);
        if (sameHashDiagnostics.Any(sameHashDiagnostic => sameHashDiagnostic == diagnostic))
            return;

        Add(diagnostic);
    }

    public void MergeWith(IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            ReportDiagnostic(diagnostic);
        }
    }

    public const string INVALID_NUMBER_CODE = "[0001:Error]";

    public void ReportInvalidNumber(TextLocation textLocation, string text, Type type)
    {
        var message = $"Number '{text}' is not a valid '{type}'.";
        Report(textLocation, message, INVALID_NUMBER_CODE);
    }

    public const string BAD_CHARACTER_CODE = "[0002:Error]";

    public void ReportBadCharacter(TextLocation textLocation, char character)
    {
        var message = $"Bad character '{character}'.";
        Report(textLocation, message, BAD_CHARACTER_CODE);
    }

    public const string UNEXPECTED_TOKEN_CODE = "[0003:Error]";

    public void ReportUnexpectedToken(TextLocation textLocation, SyntaxKind token, SyntaxKind expected)
    {
        var message = $"Unexpected token <{token}> expected <{expected}>.";
        Report(textLocation, message, UNEXPECTED_TOKEN_CODE);
    }

    public const string UNDEFINED_UNARY_OPERATOR_CODE = "[0004:Error]";

    public void ReportUndefinedUnaryOperator(TextLocation location, string text, TypeSymbol operandType)
    {
        var message = $"Invalid unary operator '{text}' for type '{operandType}'.";
        Report(location, message, UNDEFINED_UNARY_OPERATOR_CODE);
    }

    public const string UNDEFINED_BINARY_OPERATOR_CODE = "[0005:Error]";

    public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType,
        TypeSymbol rightType)
    {
        var message = $"Invalid binary operator '{operatorText}' for types '{leftType}' and '{rightType}'.";
        Report(location, message, UNDEFINED_BINARY_OPERATOR_CODE);
    }

    public const string UNDEFINED_NAME_CODE = "[0006:Error]";

    public void ReportUndefinedName(TextLocation location, string name)
    {
        var message = $"'{name}' is undefined.";
        Report(location, message, UNDEFINED_NAME_CODE);
    }

    public const string VARIABLE_ALREADY_DECLARED_CODE = "[0007:Error]";

    public void ReportVariableAlreadyDeclared(SyntaxToken variableIdentifier)
    {
        var message = $"Variable with same name '{variableIdentifier.Text}' is already declared.";
        Report(variableIdentifier.Location, message, VARIABLE_ALREADY_DECLARED_CODE);
    }

    public const string CANNOT_CONVERT_CODE = "[0008:Error]";

    public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert '{fromType}' to '{toType}'.";
        Report(location, message, CANNOT_CONVERT_CODE);
    }

    public const string CANNOT_ASSIGN_TO_READONLY_CODE = "[0009:Error]";

    public void ReportCannotAssignToReadonly(SyntaxToken identifierToken)
    {
        var message = $"'{identifierToken.Text}' is readonly and cannot be assigned to.";
        Report(identifierToken.Location, message, CANNOT_ASSIGN_TO_READONLY_CODE);
    }

    public const string VARIABLE_DOES_NOT_EXISTS_IN_CURRENT_SCOPE_CODE = "[0010:Error]";

    public void VariableDoesntExistsInCurrentScope(TextLocation location, string name)
    {
        var message = $"'{name}' doesn't exists in current scope.";
        Report(location, message, VARIABLE_DOES_NOT_EXISTS_IN_CURRENT_SCOPE_CODE);
    }

    public const string UNTERMINATED_STRING_CODE = "[0011:Error]";

    public void ReportUnterminatedString(TextLocation textLocation)
    {
        var message = "Unterminated string literal.";
        Report(textLocation, message, UNTERMINATED_STRING_CODE);
    }

    public const string PARAMETER_COUNT_MISMATCH_CODE = "[0013:Error]";

    public void ReportParameterCountMismatch(TextLocation location, string identifierText, int parametersLength,
        int argumentsCount)
    {
        var message =
            $"Method '{identifierText}' requires {parametersLength} arguments but was given {argumentsCount}.";
        Report(location, message, PARAMETER_COUNT_MISMATCH_CODE);
    }

    public const string EXPRESSION_MUST_HAVE_VALUE_CODE = "[0014:Error]";

    public void ReportExpressionMustHaveValue(TextLocation location)
    {
        var message = $"Expression must have a value.";
        Report(location, message, EXPRESSION_MUST_HAVE_VALUE_CODE);
    }

    public const string UNDEFINED_TYPE_CODE = "[0015:Error]";

    public void ReportUndefinedType(TextLocation location, string typeName)
    {
        var message = $"Type '{typeName}' is undefined.";
        Report(location, message, UNDEFINED_TYPE_CODE);
    }

    public const string NO_IMPLICIT_CONVERSION_CODE = "[0016:Error]";

    public void ReportNoImplicitConversion(TextLocation location, TypeSymbol expressionType, TypeSymbol type)
    {
        var message = $"No implicit conversion from '{expressionType}' to '{type}'.";
        Report(location, message, NO_IMPLICIT_CONVERSION_CODE);
    }

    public const string TYPE_CLAUSE_EXPECTED_CODE = "[0017:Error]";

    public void ReportTypeClauseExpected(TextLocation location)
    {
        var message = $"Type clause expected.";
        Report(location, message, TYPE_CLAUSE_EXPECTED_CODE);
    }

    public const string PARAMETER_ALREADY_DECLARED_CODE = "[0018:Error]";

    public void ReportParameterAlreadyDeclared(SyntaxToken parameterIdentifier)
    {
        var message = $"Parameter with same name '{parameterIdentifier.Text}' is already declared.";
        Report(parameterIdentifier.Location, message, PARAMETER_ALREADY_DECLARED_CODE);
    }

    public const string METHOD_ALREADY_DECLARED_CODE = "[0019:Error]";

    public void ReportMethodAlreadyDeclared(SyntaxToken methodIdentifier)
    {
        var message = $"Method '{methodIdentifier.Text}' is already declared.";
        Report(methodIdentifier.Location, message, METHOD_ALREADY_DECLARED_CODE);
    }

    public const string INVALID_BREAK_OR_CONTINUE_CODE = "[0020:Error]";

    public void ReportInvalidBreakOrContinue(SyntaxToken breakKeyword)
    {
        var message = $"Invalid use of '{breakKeyword.Text}'. Must be inside a loop.";
        Report(breakKeyword.Location, message, INVALID_BREAK_OR_CONTINUE_CODE);
    }

    public const string RETURN_STATEMENT_IS_INVALID_FOR_VOID_METHOD_CODE = "[0021:Error]";

    public void ReportReturnStatementIsInvalidForVoidMethod(TextLocation location)
    {
        var message = $"Return statement is invalid for {BuiltInTypeSymbols.Void} method.";
        Report(location, message, RETURN_STATEMENT_IS_INVALID_FOR_VOID_METHOD_CODE);
    }

    public const string RETURN_STATEMENT_IS_INVALID_FOR_NON_VOID_METHOD_CODE = "[0022:Error]";

    public void ReportReturnStatementIsInvalidForNonVoidMethod(TextLocation location, TypeSymbol methodReturnType)
    {
        var message = $"return should have value for method with return type of {methodReturnType}.";
        Report(location, message, RETURN_STATEMENT_IS_INVALID_FOR_NON_VOID_METHOD_CODE);
    }

    public const string INVALID_RETURN_CODE = "[0023:Error]";

    public void ReportInvalidReturn(TextLocation location)
    {
        var message = $"Return statement should be inside a method.";
        Report(location, message, INVALID_RETURN_CODE);
    }

    public const string ALL_PATHS_MUST_RETURN_CODE = "[0024:Error]";

    public void ReportAllPathsMustReturn(TextLocation location)
    {
        var message = $"All paths must return a value.";
        Report(location, message, ALL_PATHS_MUST_RETURN_CODE);
    }

    public const string INVALID_EXPRESSION_STATEMENT_CODE = "[0025:Error]";

    public void ReportInvalidExpressionStatement(TextLocation syntaxLocation)
    {
        var message = "Only assignment, and call expressions can be used as a statement.";
        Report(syntaxLocation, message, INVALID_EXPRESSION_STATEMENT_CODE);
    }

    public const string MAIN_CANNOT_BE_USED_WITH_GLOBAL_STATEMENTS_CODE = "[0026:Error]";

    public void ReportMainCannotBeUsedWithGlobalStatements(TextLocation identifierLocation)
    {
        var message = "Main cannot be used with global statements.";
        Report(identifierLocation, message, MAIN_CANNOT_BE_USED_WITH_GLOBAL_STATEMENTS_CODE);
    }

    public const string MULTIPLE_MAIN_METHODS_CODE = "[0027:Error]";

    public void ReportMultipleMainmethods(TextLocation identifierLocation)
    {
        var message = "Multiple main methods found.";
        Report(identifierLocation, message, MULTIPLE_MAIN_METHODS_CODE);
    }

    public const string PARAMETER_SHOULD_HAVE_TYPE_EXPLICITLY_DEFINED_CODE = "[0028:Error]";

    public void ReportParameterShouldHaveTypeExplicitlyDefined(TextLocation parameterLocation, string parameterName)
    {
        var message = $"Parameter '{parameterName}' should have type explicitly defined.";
        Report(parameterLocation, message, PARAMETER_SHOULD_HAVE_TYPE_EXPLICITLY_DEFINED_CODE);
    }

    public const string MAIN_MUST_HAVE_CORRECT_SIGNATURE_CODE = "[0029:Error]";

    public void ReportMainMustHaveCorrectSignature(TextLocation identifierLocation)
    {
        var message = $"main method must have correct signature(main must be static, have return type {BuiltInTypeSymbols.Void} and 0 parameters).";
        Report(identifierLocation, message, MAIN_MUST_HAVE_CORRECT_SIGNATURE_CODE);
    }

    public const string GLOBAL_STATEMENTS_SHOULD_ONLY_BE_IN_A_SINGLE_FILE_CODE = "[0030:Error]";

    public void ReportGlobalStatementsShouldOnlyBeInASingleFile(TextLocation location)
    {
        var message = $"Global statements should only be in a single file.";
        Report(location, message, GLOBAL_STATEMENTS_SHOULD_ONLY_BE_IN_A_SINGLE_FILE_CODE);
    }

    public const string CANNOT_EMIT_GLOBAL_STATEMENTS_BECAUSE_TYPE_ALREADY_EXISTS_CODE = "[0031:Error]";

    public void ReportCannotEmitGlobalStatementsBecauseTypeAlreadyExists(string typeName, TextLocation location)
    {
        var message = $"Cannot emit global statements because type '{typeName}' already exists.";
        Report(location, message, CANNOT_EMIT_GLOBAL_STATEMENTS_BECAUSE_TYPE_ALREADY_EXISTS_CODE);
    }

    public const string UNDEFINED_METHOD_CALL_CODE = "[0032:Error]";

    public void ReportUndefinedMethodCall(TextLocation rightLocation, string methodSymbolName, TypeSymbol leftType)
    {
        var message = $"method '{methodSymbolName}' is undefined for type '{leftType}'.";
        Report(rightLocation, message, UNDEFINED_METHOD_CALL_CODE);
    }

    public const string AMBIGUOUS_TYPE_CODE = "[0033:Error]";

    public void ReportAmbiguousType(TextLocation typeIdentifierLocation, string typeIdentifier,
        List<TypeSymbol> matchingTypes)
    {
        var message =
            $"Type '{typeIdentifier}' is ambiguous. Found {matchingTypes.Count} matching types: {string.Join(", ", matchingTypes.Select(t => t.Name))}.";
        Report(typeIdentifierLocation, message, AMBIGUOUS_TYPE_CODE);
    }

    public const string NO_MAIN_METHOD_ALLOWED_IN_SCRIPT_MODE_CODE = "[0034:Error]";

    public void ReportNoMainMethodAllowedInScriptMode(TextLocation mainIdentifierLocation)
    {
        var message = $"No main method allowed in script mode. Use global statements instead.";
        Report(mainIdentifierLocation, message, NO_MAIN_METHOD_ALLOWED_IN_SCRIPT_MODE_CODE);
    }

    public const string CLASS_WITH_THAT_NAME_IS_ALREADY_DECLARED_CODE = "[0035:Error]";

    public void ReportClassWithThatNameIsAlreadyDeclared(TextLocation classIdentifierLocation, string className)
    {
        var message = $"Class with name '{className}' is already declared.";
        Report(classIdentifierLocation, message, CLASS_WITH_THAT_NAME_IS_ALREADY_DECLARED_CODE);
    }

    public const string FIELD_ALREADY_DECLARED_CODE = "[0036:Error]";

    public void ReportFieldAlreadyDeclared(SyntaxToken fieldIdentifier)
    {
        var message = $"Field '{fieldIdentifier.Text}' is already declared.";
        Report(fieldIdentifier.Location, message, FIELD_ALREADY_DECLARED_CODE);
    }

    public const string UNDEFINED_FIELD_ACCESS_CODE = "[0037:Error]";

    public void ReportUndefinedFieldAccess(SyntaxToken fieldIdentifier, TypeSymbol type)
    {
        var message = $"Field '{fieldIdentifier.Text}' is undefined for type '{type}'.";
        Report(fieldIdentifier.Location, message, UNDEFINED_FIELD_ACCESS_CODE);
    }

    public const string UNDEFINED_MEMBER_CODE = "[0038:Error]";

    public void ReportUndefinedMember(SyntaxToken memberIdentifierLocation, TypeSymbol type)
    {
        var message = $"Member '{memberIdentifierLocation.Text}' is undefined on type {type}.";
        Report(memberIdentifierLocation.Location, message, UNDEFINED_MEMBER_CODE);
    }

    public const string UNDEFINED_METHOD_CODE = "[0039:Error]";

    public void ReportUndefinedMethod(SyntaxToken methodCallIdentifier, TypeSymbol leftType)
    {
        var message = $"Method '{methodCallIdentifier.Text}' is undefined on type {leftType}.";
        Report(methodCallIdentifier.Location, message, UNDEFINED_METHOD_CODE);
    }

    public const string CLASS_MEMBER_CANNOT_HAVE_NAME_OF_CLASS_CODE = "[0040:Error]";

    public void ReportClassMemberCannotHaveNameOfClass(SyntaxToken memberIdentifier)
    {
        var message = "Class member cannot have the same name as the class.";
        Report(memberIdentifier.Location, message, CLASS_MEMBER_CANNOT_HAVE_NAME_OF_CLASS_CODE);
    }
    
    public const string CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE = "[0041:Error]";

    public void ReportClassMemberWithThatNameAlreadyDeclared(SyntaxToken memberIdentifier)
    {
        var message = $"Class member with name '{memberIdentifier.Text}' is already declared.";
        Report(memberIdentifier.Location, message, CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE);
    }
    
    public const string REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE = "[0042:Error]";
    public void ReportCannotUseUninitializedVariable(SyntaxToken identifier)
    {
        var message = $"Cannot use uninitialized variable '{identifier.Text}'.";
        Report(identifier.Location, message, REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE);
    }

    public const string AMBIGUOUS_MEMBER_ACCESS_CODE = "[0043:Error]";
    public void ReportAmbiguousMemberMemberAccess(SyntaxToken memberIdentifier, ImmutableArray<Symbol> symbols)
    {
        var memberName = memberIdentifier.Text;
        var message = $"Ambiguous '{memberName}' member access. Found:\n ";
        foreach (var symbol in symbols)
        {
            message += symbol.Kind switch
            {
                SymbolKind.Variable => $"variable '{symbol.Name}' of type '{symbol.As<VariableSymbol>().Type}'.",
                SymbolKind.Type => $"type '{symbol.Name}'.",
                SymbolKind.Parameter => $"parameter '{symbol.Name}' of type '{symbol.As<ParameterSymbol>().Type}'.",
                SymbolKind.Method => $"method '{symbol.Name}' of type '{symbol.As<MethodSymbol>().Type}'.",
                SymbolKind.Field => $"field '{symbol.Name}' of type '{symbol.As<FieldSymbol>().Type}'.",
                _ => throw new ArgumentOutOfRangeException()
            };
            message += "\n";
        }
        message += $"All of them have member with '{memberName}' name.";
        
        Report(memberIdentifier.Location, message, AMBIGUOUS_MEMBER_ACCESS_CODE);
    }
    
    public const string THIS_EXPRESSION_NOT_ALLOWED_IN_STATIC_CONTEXT_CODE = "[0044:Error]";

    public void ReportThisExpressionNotAllowedInStaticContext(SyntaxToken thisKeyword)
    {
        var message = "This expression is not allowed in static context.";
        Report(thisKeyword.Location, message, THIS_EXPRESSION_NOT_ALLOWED_IN_STATIC_CONTEXT_CODE);
    }

    
    public const string MAIN_METHOD_SHOULD_BE_DECLARED_CODE = "[0046:Error]";
    public void ReportMainMethodShouldBeDeclared(SourceText sourceText)
    {
        var message = "Main method should be declared.";
        Report(new TextLocation(sourceText, new TextSpan(0, 0)), message, MAIN_METHOD_SHOULD_BE_DECLARED_CODE);
    }

    public const string CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE = "[0047:Error]";
    public void ReportCannotAccessStaticFieldOnNonStaticMember(SyntaxToken identifierToken)
    {
        var message = $"Cannot access static field '{identifierToken.Text}' on non-static member.";
        Report(identifierToken.Location, message, CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE);
    }
    
    public const string CLASS_CANNOT_INHERIT_FROM_SELF_CODE = "[0048:Error]";
    public void ReportClassCannotInheritFromSelf(SyntaxToken classIdentifier)
    {
        var message = "Class cannot inherit from itself.";
        Report(classIdentifier.Location, message, CLASS_CANNOT_INHERIT_FROM_SELF_CODE);
    }

    public const string METHOD_CANNOT_USE_VIRTUAL_WITH_OVERRIDE_CODE = "[0049:Error]";
    public void ReportCannotUseVirtualWithOverride(SyntaxToken virtualKeyword, SyntaxToken overrideKeyword)
    {
        var message = "Method cannot use virtual with override";
        var firstLocation = virtualKeyword.Location.Span.Start < overrideKeyword.Location.Span.Start
            ? virtualKeyword.Location
            : overrideKeyword.Location;
        Report(firstLocation, message, METHOD_CANNOT_USE_VIRTUAL_WITH_OVERRIDE_CODE);
    }

    public const string UNEXPECTED_EXPRESSION_INSIDE_CAST_EXPRESSION_CODE = "[0049:Error]";
    public void ReportUnexpectedExpressionToCast(ExpressionSyntax toCastExpression)
    {
        var message = "Unexpected expression inside cast expression";
        Report(toCastExpression.Location, message, UNEXPECTED_EXPRESSION_INSIDE_CAST_EXPRESSION_CODE);
    }
    
    public const string INHERITANCE_DIAMOND_PROBLEM_CODE = "[0050:Error]";
    public void ReportInheritanceDiamondProblem(SyntaxToken classIdentifier, List<TypeSymbol> baseTypes, Symbol symbol)
    {
        (classIdentifier.Kind is SyntaxKind.IdentifierToken).EnsureTrue();
        var baseTypeNamesString = string.Join(", ", baseTypes.Select(x => x.Name));
        var symbolString = symbol.Kind switch
        {
            SymbolKind.Method => $"method '{symbol.Name}'",
            SymbolKind.Field => $"field '{symbol.Name}'",
            _ => throw new ArgumentOutOfRangeException()
        };
        var message = $"Inheritance diamond problem. Problem with {symbolString}. Classes involved {baseTypeNamesString}.";
        Report(classIdentifier.Location, message, INHERITANCE_DIAMOND_PROBLEM_CODE);
    }
    
    public const string METHOD_ALREADY_DECLARED_IN_BASE_CLASS_CODE = "[0051:Error]";
    public void ReportMethodAlreadyDeclaredInBaseClass(MethodSymbol methodSymbol, TypeSymbol baseType)
    {
        var identifier = methodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier;
        var message = $"Method '{identifier.Text}' is already declared in base class '{baseType}'.";
        Report(identifier.Location, message, METHOD_ALREADY_DECLARED_IN_BASE_CLASS_CODE);
    }
}