using System.Collections.Immutable;
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

    public TypeBinder(BoundScope scope, bool isScript, TypeBinderLookup lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
    }

    public ImmutableArray<Diagnostic> BindBody()
    {
        var diagnostics = new DiagnosticBag();
        var typeScope = new BoundScope(_scope);

        foreach (var (functionSymbol, _) in _lookup.CurrentType.MethodTable)
        {
            typeScope.TryDeclareMethod(functionSymbol)
                .ThrowIfFalse();
        }
        
        foreach (var fieldSymbol in _lookup.CurrentType.FieldTable)
        {
            typeScope.TryDeclareField(fieldSymbol)
                .ThrowIfFalse();
        }
        
        foreach (var functionSymbol in _lookup.CurrentType.MethodTable.Symbols)
        {
            if (functionSymbol.Declaration is null
                && (functionSymbol.Name == SyntaxFacts.MainMethodName || functionSymbol.Name == SyntaxFacts.ScriptMainMethodName)
                && _lookup.CurrentType.Name == SyntaxFacts.StartTypeName)
            {
                // function generated from global statements
                // binding of this function should be done later to place proper returns;
                continue;
            }
            
            var functionBinder = new MethodBinder(typeScope, _isScript, new MethodBinderLookup(
                _lookup.CurrentType,
                _lookup.AvailableTypes,
                functionSymbol));
            var body = functionBinder.BindMethodBody(functionSymbol);
            var loweredBody = Lowerer.Lower(body);
            if (!Equals(functionSymbol.ReturnType, TypeSymbol.Void)
                && !ControlFlowGraph.AllPathsReturn(loweredBody))
            {
                diagnostics.ReportAllPathsMustReturn(functionSymbol.Declaration.NullGuard().Identifier.Location);
            }
            
            ControlFlowGraph.AllVariablesInitializedBeforeUse(loweredBody, diagnostics);
            
            functionBinder.Diagnostics.AddRangeTo(diagnostics);

            _lookup.CurrentType.MethodTable.Declare(functionSymbol, loweredBody);
        }
        
        return diagnostics.ToImmutableArray();
    }
}