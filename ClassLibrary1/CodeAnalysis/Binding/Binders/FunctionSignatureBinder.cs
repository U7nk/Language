using System.Collections.Generic;
using System.Collections.Immutable;
using Wired.CodeAnalysis.Binding.Lookup;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding.Binders;

sealed class FunctionSignatureBinder 
{
    readonly DiagnosticBag _diagnostics = new();
    readonly BaseBinderLookup _lookup;
    readonly BoundScope _scope;

    public FunctionSignatureBinder(BaseBinderLookup lookup, BoundScope scope)
    {
        _lookup = lookup;
        _scope = scope;
    }

    public ImmutableArray<Diagnostic> BindFunctionSignature(FunctionDeclarationSyntax function)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameters = new HashSet<string>();
        foreach (var parameter in function.Parameters)
        {
            var parameterName = parameter.Identifier.Text;
            if (!seenParameters.Add(parameterName))
            {
                _diagnostics.ReportParameterAlreadyDeclared(parameter.Location, parameterName);
                continue;
            }

            var parameterType = BinderHelp.BindTypeClause(parameter.Type, _diagnostics, _lookup);
            if (parameterType is null)
            {
                _diagnostics.ReportParameterShouldHaveTypeExplicitlyDefined(parameter.Location, parameterName);
                parameterType = TypeSymbol.Error;
            }

            parameters.Add(new ParameterSymbol(parameterName, parameterType));
        }

        var returnType = BinderHelp.BindTypeClause(function.ReturnType, _diagnostics, _lookup) ?? TypeSymbol.Void;

        var functionSymbol =
            new FunctionSymbol(function.Identifier.Text, parameters.ToImmutable(), returnType, function);
        if (!_scope.TryDeclareFunction(functionSymbol))
        {
            _diagnostics.ReportFunctionAlreadyDeclared(function.Identifier.Location, function.Identifier.Text);
        }

        return _diagnostics.ToImmutableArray();
    }
}