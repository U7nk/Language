using System;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeBinder
{
    readonly TypeBinderLookup _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;
    readonly DiagnosticBag _diagnostics;
    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public TypeBinder(BoundScope scope, bool isScript, TypeBinderLookup lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
        _diagnostics = new DiagnosticBag();
    }

    public void BindClassBody()
    {
        var typeScope = new BoundScope(_scope);
        
        var baseType = BindInheritanceClause(_lookup.CurrentType.InheritanceClauseSyntax);
        CheckTypeDontInheritFromItself(_lookup.CurrentType, baseType);
        _lookup.CurrentType.BaseType = baseType;
        
        foreach (var methodSymbol in _lookup.CurrentType.MethodTable.Symbols)
        {
            if (methodSymbol.DeclarationSyntax.Empty()
                && (methodSymbol.Name == SyntaxFacts.MainMethodName || methodSymbol.Name == SyntaxFacts.ScriptMainMethodName)
                && _lookup.CurrentType.Name == SyntaxFacts.StartTypeName)
            {
                // function generated from global statements
                // binding of this function should be done later to place proper returns;
                continue;
            }
            
            var methodBinder = new MethodBinder(typeScope, _isScript, new MethodBinderLookup(
                _lookup.CurrentType,
                _lookup.AvailableTypes,
                methodSymbol));
            var body = methodBinder.BindMethodBody(methodSymbol);
            var loweredBody = Lowerer.Lower(body);
            
            if (!Equals(methodSymbol.ReturnType, TypeSymbol.Void) && !ControlFlowGraph.AllPathsReturn(loweredBody))
                _diagnostics.ReportAllPathsMustReturn(methodSymbol.DeclarationSyntax
                    .Cast<MethodDeclarationSyntax>()
                    .First().Identifier.Location);

            ControlFlowGraph.AllVariablesInitializedBeforeUse(loweredBody, _diagnostics);
            
            methodBinder.Diagnostics.AddRangeTo(_diagnostics);

            _lookup.CurrentType.MethodTable.Declare(methodSymbol, loweredBody);
        }
    }

    void CheckTypeDontInheritFromItself(TypeSymbol currentType, TypeSymbol? baseType)
    {
        if (baseType is null)
            return;
        
        var currentBase = baseType;
        while (currentBase != null)
        {
            if (currentBase == currentType)
            {
                foreach (var declarationSyntax in currentType.DeclarationSyntax.Cast<ClassDeclarationSyntax>())
                {
                    _diagnostics.ReportClassCannotInheritFromSelf(declarationSyntax.Identifier);
                }
                return;
            }
            currentBase = currentBase.BaseType;
        }

    }

    TypeSymbol? BindInheritanceClause(InheritanceClauseSyntax? syntax)
    {
        if (syntax is null)
            return null;
        
        var baseTypeName = syntax.BaseTypeIdentifier.Text;
        if (!_scope.TryLookupType(baseTypeName, out var baseTypeSymbol))
        {
            _diagnostics.ReportUndefinedType(syntax.BaseTypeIdentifier.Location, baseTypeName);
            return null;
        }

        return baseTypeSymbol;
    }
}