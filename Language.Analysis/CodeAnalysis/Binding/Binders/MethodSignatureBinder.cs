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

        var isVirtual = method.VirtualKeyword.IsSome;
        var isOverriding = method.OverrideKeyword.IsSome;
        
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
        _lookup.ContainingType.TryDeclareMethod(currentMethodSymbol, diagnostics, _lookup);

        ReportModifiersMisuseIfAny(currentMethodSymbol, diagnostics);

        return diagnostics.ToImmutableArray();
    }

    void ReportModifiersMisuseIfAny(MethodSymbol methodSymbol, DiagnosticBag diagnosticBag)
    {
        if (methodSymbol is { IsVirtual: true, IsOverriding: true })
        {
            var syntax = methodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>(); 
            diagnosticBag.ReportCannotUseVirtualWithOverride(
                syntax.VirtualKeyword.Unwrap(), syntax.OverrideKeyword.Unwrap());
        }
    }
}