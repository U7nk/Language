using System.Collections.Generic;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Binding.Binders.Field;
using Language.Analysis.CodeAnalysis.Binding.Binders.Method;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Class;

sealed class ClassBinder
{
    readonly TypeSymbol _typeSymbol;
    readonly BoundScope _scope;
    readonly bool _isScript;
    readonly DiagnosticBag _diagnostics;
    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public ClassBinder(BoundScope scope, bool isScript, TypeSymbol typeSymbol)
    {
        _scope = scope;
        _isScript = isScript;
        _typeSymbol = typeSymbol;
        _diagnostics = new DiagnosticBag();
    }

    public void BindClassBody(IEnumerable<FullMethodBinder> methodBinders, IEnumerable<FullFieldBinder> fullFieldBinders)
    {
        foreach (var methodBinder in methodBinders)
        {
            var body = methodBinder.BindMethodBody(_diagnostics);
            var loweredBody = Lowerer.Lower(body);

            if (!Equals(methodBinder.MethodSymbol.ReturnType, BuiltInTypeSymbols.Void) &&
                !ControlFlowGraph.AllPathsReturn(loweredBody))
            {
                _diagnostics.ReportAllPathsMustReturn(methodBinder.MethodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>()
                                                          .Identifier.Location);
            }

            ControlFlowGraph.AllVariablesInitializedBeforeUse(loweredBody, _diagnostics);
            if (methodBinder.SuccessfullyDeclaredInType.Unwrap())
            {
                _typeSymbol.MethodTable.SetMethodBody(methodBinder.MethodSymbol, loweredBody);
            }
        }
    }
}