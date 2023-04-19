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
    readonly TypeSymbol _currentType;
    readonly BoundScope _scope;
    readonly bool _isScript;

    readonly List<FullMethodBinder> _methodBinders = new();
    readonly List<FullFieldBinder> _fieldBinders = new();
    readonly DeclarationsBag _allDeclarations;

    public TypeMembersSignaturesBinder(TypeSymbol currentType, DeclarationsBag allDeclarations, BoundScope scope, bool isScript)
    {
        _currentType = currentType;
        _allDeclarations = allDeclarations;
        _scope = scope;
        _isScript = isScript;
    }

    public (List<FullMethodBinder>, List<FullFieldBinder>) BindMembersSignatures(DiagnosticBag diagnostics)
    {
        var classDeclaration = _currentType.DeclarationSyntax.UnwrapAs<ClassDeclarationSyntax>();
        var typeScope = new BoundScope(_scope);
        foreach (var member in classDeclaration.Members)
        {
            if (member.Kind is SyntaxKind.MethodDeclaration)
            {
                var methodScope = new BoundScope(typeScope);
                var methodBinder = new FullMethodBinder(methodScope, _currentType, _allDeclarations, _isScript, isTopMethod: false);
                _methodBinders.Add(methodBinder);
                
                var methodDeclarationSyntax = (MethodDeclarationSyntax) member;
                methodBinder.BindMethodDeclaration(methodDeclarationSyntax, diagnostics);
            }
            else if (member.Kind is SyntaxKind.FieldDeclaration)
            {
                var field = (FieldDeclarationSyntax)member;
                var fieldBinder = new FullFieldBinder(typeScope, _isScript, _currentType, _allDeclarations);
                _fieldBinders.Add(fieldBinder);
                
                fieldBinder.BindDeclaration(field, diagnostics);
            }
            else
            {
                throw new Exception($"Unexpected member kind {member.Kind}");
            }
        }

        return (_methodBinders, _fieldBinders);
    }

    public void DiagnoseDiamondProblem( DiagnosticBag diagnostics)
    {
        var flattenBaseTypes = FlattenBaseTypes(_currentType);
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
                diagnostics.ReportInheritanceDiamondProblem(_currentType.DeclarationSyntax.Unwrap().Identifier, problemBaseTypes, symbol);
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