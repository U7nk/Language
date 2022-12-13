using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

 class MethodSignatureBinder
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
                parameterType = BuiltInTypeSymbols.Error;
            }

            parameters.Add(new ParameterSymbol(
                               parameter,
                               parameterName,
                               containingType: null,
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
                        sameNameParameter.DeclarationSyntax.UnwrapAs<ParameterSyntax>().Identifier);
                }
            }
        }

        var returnType = BinderHelp.BindTypeClause(method.ReturnType, diagnostics, _lookup) ?? BuiltInTypeSymbols.Void;

        var isStatic = method.StaticKeyword is { } || _lookup.IsTopMethod;

        var isVirtual = method.VirtualKeyword is { };
        var isOverriding = method.OverrideKeyword is { };
        
        var currentMethodSymbol = new MethodSymbol(
            method,
            _lookup.ContainingType,
            isStatic,
            isVirtual,
            isOverriding,
            method.Identifier.Text,
            parameters.ToImmutable(),
            returnType);

        _lookup.AddDeclaration(currentMethodSymbol, method);
        if (!_lookup.ContainingType.TryDeclareMethod(currentMethodSymbol))
        {
            ReportRedeclarationDiagnostics(currentMethodSymbol, diagnostics);
        }

        ReportModifiersMisuseIfAny(currentMethodSymbol, diagnostics);

        return diagnostics.ToImmutableArray();
    }

    private void ReportModifiersMisuseIfAny(MethodSymbol methodSymbol, DiagnosticBag diagnosticBag)
    {
        if (methodSymbol is { IsVirtual: true, IsOverriding: true })
        {
            var syntax = methodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>(); 
            diagnosticBag.ReportCannotUseVirtualWithOverride(syntax.VirtualKeyword, syntax.OverrideKeyword);
        }
    }

    void ReportRedeclarationDiagnostics(MethodSymbol currentMethodSymbol, DiagnosticBag diagnostics)
    {
        if (currentMethodSymbol.Name == _lookup.ContainingType.Name)
        {
            diagnostics.ReportClassMemberCannotHaveNameOfClass(
                currentMethodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier);
        }

        var existingMethodDeclarations = _lookup.LookupDeclarations<MethodDeclarationSyntax>(currentMethodSymbol);
        if (existingMethodDeclarations.Length > 1)
        {
            foreach (var existingMethodDeclaration in existingMethodDeclarations)
            {
                diagnostics.ReportMethodAlreadyDeclared(existingMethodDeclaration.Identifier);
            }
        }


        var sameNameFields = _lookup.ContainingType.FieldTable.Symbols
            .Where(f => f.Name == currentMethodSymbol.Name)
            .ToList();
        if (sameNameFields.Count > 0)
        {
            diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(
                currentMethodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier);
            foreach (var sameNameField in sameNameFields)
            {
                var fieldDeclarations = _lookup.LookupDeclarations<FieldDeclarationSyntax>(sameNameField)
                    .Add(sameNameField.DeclarationSyntax.UnwrapAs<FieldDeclarationSyntax>());
                foreach (var fieldDeclaration in fieldDeclarations)
                    diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(fieldDeclaration.Identifier);
            }
        }
    }
}