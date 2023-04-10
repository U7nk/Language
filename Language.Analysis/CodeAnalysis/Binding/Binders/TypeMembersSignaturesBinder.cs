using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeMembersSignaturesBinder
{
    readonly BinderLookup _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;

    public TypeMembersSignaturesBinder(BinderLookup lookup, BoundScope scope, bool isScript)
    {
        _lookup = lookup;
        _scope = scope;
        _isScript = isScript;
    }

    public void BindMembersSignatures(TypeSymbol currentType, DiagnosticBag diagnostics)
    {
        var classDeclaration = currentType.DeclarationSyntax.UnwrapAs<ClassDeclarationSyntax>();
        var typeScope = new BoundScope(_scope);
        foreach (var member in classDeclaration.Members)
        {
            if (member.Kind is SyntaxKind.MethodDeclaration)
            {
                var methodScope = new BoundScope(typeScope);
                
                var method = (MethodDeclarationSyntax) member;
                var methodSignatureBinder = new MethodSignatureBinder(
                    new MethodSignatureBinderLookup(currentType,
                                                    isTopMethod: false,
                                                    _lookup.Declarations),
                    methodScope); 
                methodSignatureBinder.BindMethodSignature(method).AddRangeTo(diagnostics);
            }
            else if (member.Kind is SyntaxKind.FieldDeclaration)
            {
                var field = (FieldDeclarationSyntax)member;
                var fieldBinder = new FieldSignatureBinder(
                    typeScope, 
                    _isScript,
                    new FieldBinderLookup(currentType, _lookup.Declarations));
                fieldBinder.BindDeclaration(field, diagnostics);
            }
            else
            {
                throw new Exception($"Unexpected member kind {member.Kind}");
            }
        }
    }

    public void DiagnoseDiamondProblem(TypeSymbol type, DiagnosticBag diagnostics)
    {
        var flattenBaseTypes = FlattenBaseTypes(type);
        var alreadyCheckedTypes = new List<TypeSymbol>();
        foreach (var firstBaseType in flattenBaseTypes)
        {
            if (alreadyCheckedTypes.Contains(firstBaseType))
                continue;
            
            var otherBaseTypes = flattenBaseTypes.Where(x => !x.Equals(firstBaseType)).ToList();
            var symbols = firstBaseType.MethodTable.Select(x => x.MethodSymbol).Cast<Symbol>().Concat(firstBaseType.FieldTable.Symbols)
                .ToList();
            
            foreach (var symbol in symbols) 
            {
                var problemBaseTypes = otherBaseTypes
                    .Where(x => x.MethodTable.Select(declaration => declaration.MethodSymbol).Any(s => s.Name == symbol.Name) 
                                || x.FieldTable.Symbols.Any(s => s.Name == symbol.Name))
                    .AddRangeTo(alreadyCheckedTypes)
                    .ToList();
                if (problemBaseTypes.Count == 0)
                    continue;
                
                firstBaseType.AddTo(problemBaseTypes);
                diagnostics.ReportInheritanceDiamondProblem(type.DeclarationSyntax.Unwrap().Identifier, problemBaseTypes, symbol);
            }   
            
        }
    }

    List<TypeSymbol> FlattenBaseTypes(TypeSymbol typeSymbol, Option<List<TypeSymbol>> checkedTypes = default)
    {
        if (checkedTypes.IsNone)
            checkedTypes = new List<TypeSymbol>();
        
        if (checkedTypes.Unwrap().Contains(typeSymbol))
            return new List<TypeSymbol>();
        
        checkedTypes.Unwrap().Add(typeSymbol);


        var result = new List<TypeSymbol>();
        var baseTypesExcludeTypeSymbol = typeSymbol.BaseTypes.Where(x => x != typeSymbol);
        foreach (var baseType in baseTypesExcludeTypeSymbol)
        {
            result.Add(baseType);
            result.AddRange(FlattenBaseTypes(baseType, checkedTypes));
        }

        return result.Distinct().ToList();
    }
}