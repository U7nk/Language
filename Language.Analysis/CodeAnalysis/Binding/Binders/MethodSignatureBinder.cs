using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

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
        
        var typeConstraintsByGenericName = new Dictionary<string, IEnumerable<TypeSymbol>>();
        if (method.GenericParametersSyntax.IsSome)
        {
            foreach (var x in method.GenericConstraintsClauseSyntax.SomeOr(ImmutableArray<GenericConstraintsClauseSyntax>.Empty))
            {
                if (method.GenericConstraintsClauseSyntax.IsSome)
                {
                    var typeConstraintsSyntax = x.TypeConstraints;
                    var typeConstraints = new List<TypeSymbol>();
                    foreach (var genericTypeConstraintIdentifier in typeConstraintsSyntax)
                    {
                        var genericTypeConstraintName = genericTypeConstraintIdentifier.Text;
                        if (!_scope.TryLookupType(genericTypeConstraintName, out var genericTypeConstraintType))
                        {
                            diagnostics.ReportUndefinedType(genericTypeConstraintIdentifier.Location,
                                                            genericTypeConstraintName);
                            continue;
                        }

                        typeConstraints.Add(genericTypeConstraintType);
                    }

                    typeConstraintsByGenericName.Add(x.Identifier.Text, typeConstraints);
                }
            }

            var genericArgumentsSyntax = method.GenericParametersSyntax.Unwrap();
            foreach (var genericArgumentSyntax in genericArgumentsSyntax.Arguments)
            {
                var genericArgumentName = genericArgumentSyntax.Text;
                var typeConstraints = typeConstraintsByGenericName.TryGetValue(genericArgumentName, out var value) 
                    ? Option.Some(value.ToImmutableArray()) 
                    : Option.None;

                var baseTypes = new SingleOccurenceList<TypeSymbol> { BuiltInTypeSymbols.Object };
                if (typeConstraints.IsSome) 
                    typeConstraints.Unwrap().AddRangeTo(baseTypes);
                
                var genericType = TypeSymbol.New(genericArgumentName, 
                                                 Option.None,
                                                 null,
                                                 new MethodTable(),
                                                 new FieldTable(),
                                                 baseTypes,
                                                 isGenericMethodParameter: true, 
                                                 typeConstraints);

                if (!_scope.TryDeclareType(genericType))
                {
                    diagnostics.ReportAmbiguousType(genericArgumentSyntax.Location,
                                                    genericArgumentName,
                                                    _scope.GetDeclaredTypes().Where(x => x.Name == genericArgumentName));
                }
            }
        }
        
        
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (var parameter in method.Parameters)
        {
            var parameterName = parameter.Identifier.Text;

            var parameterType = BinderHelp.BindTypeClause(parameter.Type, diagnostics, _scope);
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

        var returnType = BinderHelp.BindTypeClause(method.ReturnType, diagnostics, _scope) ?? BuiltInTypeSymbols.Void;

        var isStatic = method.StaticKeyword.IsSome || _lookup.IsTopMethod;

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