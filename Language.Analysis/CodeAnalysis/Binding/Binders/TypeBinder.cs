using System.Collections.Generic;
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

        foreach (var methodSymbol in _lookup.CurrentType.MethodTable.Select(x => x.MethodSymbol))
        {
            var methodBinder = new MethodBinder(typeScope, _isScript, new MethodBinderLookup(
                                                    _lookup.Declarations, 
                                                    _lookup.CurrentType,
                                                    _lookup.AvailableTypes,
                                                    methodSymbol));
            
            var body = methodBinder.BindMethodBody(methodSymbol);
            var loweredBody = Lowerer.Lower(body);

            if (!Equals(methodSymbol.ReturnType, BuiltInTypeSymbols.Void) &&
                !ControlFlowGraph.AllPathsReturn(loweredBody))
            {
                _diagnostics.ReportAllPathsMustReturn(methodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>()
                                                          .Identifier.Location);
            }

            ControlFlowGraph.AllVariablesInitializedBeforeUse(loweredBody, _diagnostics);

            _diagnostics.MergeWith(methodBinder.Diagnostics);

            _lookup.CurrentType.MethodTable.SetMethodBody(methodSymbol, loweredBody);
        }
    }
}