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
                diagnostics.ReportAllPathsMustReturn(methodSymbol.DeclarationSyntax
                    .Cast<MethodDeclarationSyntax>()
                    .First().Identifier.Location);

            ControlFlowGraph.AllVariablesInitializedBeforeUse(loweredBody, diagnostics);
            
            methodBinder.Diagnostics.AddRangeTo(diagnostics);

            _lookup.CurrentType.MethodTable.Declare(methodSymbol, loweredBody);
        }
        
        return diagnostics.ToImmutableArray();
    }
}