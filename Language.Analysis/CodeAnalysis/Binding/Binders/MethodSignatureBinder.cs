using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class MethodSignatureBinder 
{
    readonly MethodSignatureBinderLookup _lookup;
    readonly BoundScope _scope;

    public MethodSignatureBinder(MethodSignatureBinderLookup lookup, BoundScope scope)
    {
        _lookup = lookup;
        _scope = scope;
    }

    public ImmutableArray<Diagnostic> BindMethodSignature(MethodDeclarationSyntax method)
    {
        var diagnostics = new DiagnosticBag();
        
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (var parameter in method.Parameters)
        {
            var parameterName = parameter.Identifier.Text;

            var parameterType = BinderHelp.BindTypeClause(parameter.Type, diagnostics, _lookup);
            if (parameterType is null)
            {
                diagnostics.ReportParameterShouldHaveTypeExplicitlyDefined(parameter.Location, parameterName);
                parameterType = TypeSymbol.Error;
            }

            parameters.Add(new ParameterSymbol(
                ImmutableArray.Create<SyntaxNode>(parameter),
                parameterName,
                parameterType));
        }

        foreach (var parameter in parameters)
        {
            var sameNameParameters = parameters.Where(p => p.Name == parameter.Name).ToList();
            if (sameNameParameters.Count > 1)
            {
                foreach (var sameNameParameter in sameNameParameters)
                {
                    diagnostics.ReportParameterAlreadyDeclared(
                        sameNameParameter.DeclarationSyntax.First().As<ParameterSyntax>().Identifier);
                }
            }
        }
        
        var returnType = BinderHelp.BindTypeClause(method.ReturnType, diagnostics, _lookup) ?? TypeSymbol.Void;

        var methodSymbol = new MethodSymbol(
            ImmutableArray.Create<SyntaxNode>(method), 
            method.Identifier.Text, 
            parameters.ToImmutable(), 
            returnType);
        
        if (!_scope.TryDeclareMethod(methodSymbol, _lookup.ContainingType))
        {
            _scope.TryLookupMethod(methodSymbol.Name, out var existingMethod);
            existingMethod?.AddDeclaration(method);
            var existingDeclarations = existingMethod?.DeclarationSyntax.Cast<MethodDeclarationSyntax>().ToImmutableArray() 
                                       ?? ImmutableArray<MethodDeclarationSyntax>.Empty;
            
            if (methodSymbol.Name == _lookup.ContainingType.Name)
                diagnostics.ReportClassMemberCannotHaveNameOfClass(method.Identifier);
            
            foreach (var syntaxNode in existingDeclarations)
                diagnostics.ReportMethodAlreadyDeclared(syntaxNode.Identifier);
            
            var existingDeclarationsWithCurrentDeclaration = existingDeclarations.Add(method);
            var sameNameFields = _lookup.ContainingType.FieldTable.Symbols.Where(f => f.Name == methodSymbol.Name).ToList();
            if (sameNameFields.Count > 0)
            {
                foreach (var sameNameField in sameNameFields)
                {
                    foreach (var syntaxNode in sameNameField.DeclarationSyntax.Cast<FieldDeclarationSyntax>())
                        diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(syntaxNode.Identifier);
                }
                foreach (var syntaxNode in existingDeclarationsWithCurrentDeclaration)
                {
                    diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(syntaxNode.Identifier);    
                }
            }
        }

        return diagnostics.ToImmutableArray();
    }
}