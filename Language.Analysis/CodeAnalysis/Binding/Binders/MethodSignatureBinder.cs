using System.Collections.Generic;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class MethodSignatureBinder 
{
    readonly DiagnosticBag _diagnostics = new();
    readonly BaseBinderLookup _lookup;
    readonly BoundScope _scope;

    public MethodSignatureBinder(BaseBinderLookup lookup, BoundScope scope)
    {
        _lookup = lookup;
        _scope = scope;
    }

    public ImmutableArray<Diagnostic> BindMethodSignature(MethodDeclarationSyntax method)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameters = new HashSet<string>();
        foreach (var parameter in method.Parameters)
        {
            var parameterName = parameter.Identifier.Text;
            if (!seenParameters.Add(parameterName))
            {
                _diagnostics.ReportParameterAlreadyDeclared(parameter.Identifier.Location, parameterName);
            }

            var parameterType = BinderHelp.BindTypeClause(parameter.Type, _diagnostics, _lookup);
            if (parameterType is null)
            {
                _diagnostics.ReportParameterShouldHaveTypeExplicitlyDefined(parameter.Location, parameterName);
                parameterType = TypeSymbol.Error;
            }

            parameters.Add(new ParameterSymbol(parameterName, parameterType));
        }

        var returnType = BinderHelp.BindTypeClause(method.ReturnType, _diagnostics, _lookup) ?? TypeSymbol.Void;

        var functionSymbol =
            new MethodSymbol(method.Identifier.Text, parameters.ToImmutable(), returnType, method);
        if (!_scope.TryDeclareMethod(functionSymbol))
        {
            _diagnostics.ReportMethodAlreadyDeclared(method.Identifier.Location, method.Identifier.Text);
        }

        return _diagnostics.ToImmutableArray();
    }
}