using System.Collections.Generic;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Binding.Binders.Field;
using Language.Analysis.CodeAnalysis.Binding.Binders.Method;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Class;

sealed class ClassBinder(BoundScope scope, TypeSymbol typeSymbol)
{
    readonly TypeSymbol _typeSymbol = typeSymbol;
    readonly BoundScope _scope = scope;
    readonly DiagnosticBag _diagnostics = new();
    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public void BindClassBody(IEnumerable<FullMethodBinder> methodBinders, IEnumerable<FullFieldBinder> fullFieldBinders)
    {
        foreach (var methodBinder in methodBinders)
        {
            var body = methodBinder.BindMethodBody(_diagnostics);
            var loweredBody = Lowerer.Lower(body);

            if (!Equals(methodBinder.MethodSymbol.ReturnType, TypeSymbol.BuiltIn.Void()) &&
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