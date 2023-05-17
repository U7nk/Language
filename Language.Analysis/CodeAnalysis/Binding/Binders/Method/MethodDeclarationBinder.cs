using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Method;

class MethodDeclarationBinder
{
    readonly TypeSymbol _containingType;
    readonly BoundScope _methodScope;
    readonly bool _isTopMethod;
    readonly DeclarationsBag _allDeclarations;

    public MethodDeclarationBinder(BoundScope methodScope, TypeSymbol containingType, bool isTopMethod, DeclarationsBag allDeclarations)
    {
        _methodScope = methodScope;
        _isTopMethod = isTopMethod;
        _allDeclarations = allDeclarations;
        _containingType = containingType;
    }
    public MethodDeclarationBinder(BoundScope methodScope, bool successfullyDeclaredInType, bool isTopMethod,TypeSymbol containingType, DeclarationsBag allDeclarations) : this(methodScope, containingType, isTopMethod, allDeclarations)
    {
        SuccessfullyDeclaredInType = successfullyDeclaredInType;
    }

    public MethodSymbol BindMethodDeclaration(MethodDeclarationSyntax methodDeclaration, DiagnosticBag diagnostics)
    {
        var typeConstraintsByGenericName = new Dictionary<string, IEnumerable<TypeSymbol>>();
        if (methodDeclaration.GenericParametersSyntax.IsSome)
        {
            foreach (var x in methodDeclaration.GenericConstraintsClause.SomeOrEmpty<GenericConstraintsClauseSyntax>())
            {
                if (methodDeclaration.GenericConstraintsClause.IsSome)
                {
                    var typeConstraintsSyntax = x.TypeConstraints;
                    var typeConstraints = new List<TypeSymbol>();
                    foreach (var genericTypeConstraintIdentifier in typeConstraintsSyntax)
                    {
                        var genericTypeConstraintName = genericTypeConstraintIdentifier.Identifier.Text;
                        if (!_methodScope.TryLookupType(genericTypeConstraintName, out var genericTypeConstraintType))
                        {
                            diagnostics.ReportUndefinedType(genericTypeConstraintIdentifier.Location,
                                                            genericTypeConstraintName);
                            continue;
                        }

                        if (genericTypeConstraintType.IsGenericType)
                        {
                            genericTypeConstraintType = TypeSymbol.FromNamedTypeExpression(genericTypeConstraintIdentifier, _methodScope, diagnostics);
                        }
                            
                        typeConstraints.Add(genericTypeConstraintType);
                    }
                    typeConstraintsByGenericName.Add(x.Identifier.Text, typeConstraints);
                }
            }
            
            var genericParametersSyntax = methodDeclaration.GenericParametersSyntax.Unwrap();
            foreach (var genericParameterSyntax in genericParametersSyntax.Arguments)
            {
                var genericParameterName = genericParameterSyntax.Identifier.Text;
                var genericParameterTypeConstraints = typeConstraintsByGenericName.TryGetValue(genericParameterName, out var value) 
                    ? Option.Some(value.ToImmutableArray()) 
                    : Option.None;
                var baseTypes = new SingleOccurenceList<TypeSymbol> { BuiltInTypeSymbols.Object };
                if (genericParameterTypeConstraints.IsSome) 
                    genericParameterTypeConstraints.Unwrap().AddRangeTo(baseTypes);
                
                var genericParameter = TypeSymbol.New(genericParameterName, 
                                                 Option.None,
                                                 null,
                                                 new MethodTable(),
                                                 new FieldTable(),
                                                 baseTypes,
                                                 isGenericMethodParameter: true,
                                                 isGenericClassParameter: false, 
                                                 genericParameters: Option.None, genericParameterTypeConstraints: genericParameterTypeConstraints, isGenericTypeDefinition: false);
                if (!_methodScope.TryDeclareType(genericParameter))
                {
                    diagnostics.ReportAmbiguousType(genericParameterSyntax.Location,
                                                    genericParameterName,
                                                    _methodScope.GetDeclaredTypes().Where(x => x.Name == genericParameterName));
                }
            }
        }
        
        
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (var parameter in methodDeclaration.Parameters)
        {
            var parameterName = parameter.Identifier.Text;

            var parameterType = BinderHelp.BindTypeClause(parameter.Type, diagnostics, _methodScope);
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

        var returnType = BinderHelp.BindTypeClause(methodDeclaration.ReturnType, diagnostics, _methodScope) ?? BuiltInTypeSymbols.Void;

        var isStatic = methodDeclaration.StaticKeyword.IsSome || _isTopMethod;

        var isVirtual = methodDeclaration.VirtualKeyword.IsSome;
        var isOverriding = methodDeclaration.OverrideKeyword.IsSome;
        
        var genericParameters = _methodScope.GetDeclaredTypesCurrentScope().Where(x => x.IsGenericMethodParameter).ToList();
        var isGeneric = genericParameters.Any(); 
        var currentMethodSymbol = new MethodSymbol(
            methodDeclaration,
            _containingType,
            isStatic,
            isVirtual,
            isOverriding,
            methodDeclaration.Identifier.Text,
            parameters.ToImmutable(),
            returnType, 
            isGeneric,
            isGeneric ? genericParameters.ToImmutableArray() : Option.None);

        _allDeclarations.AddDeclaration(currentMethodSymbol, methodDeclaration);
        SuccessfullyDeclaredInType = _containingType.TryDeclareMethod(currentMethodSymbol, diagnostics, _allDeclarations);

        ReportModifiersMisuseIfAny(currentMethodSymbol, diagnostics);

        return currentMethodSymbol;
    }

    public Option<bool> SuccessfullyDeclaredInType { get; private set; }

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