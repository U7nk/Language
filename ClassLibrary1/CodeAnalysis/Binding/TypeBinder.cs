using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding.Lookup;
using Wired.CodeAnalysis.Lowering;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

sealed class TypeBinder
{
    readonly TypeBinderLookup? _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;

    public TypeBinder(BoundScope scope, bool isScript, TypeBinderLookup? lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
    }

    public void BindClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        var name = classDeclaration.Identifier.Text;

        var type = TypeSymbol.New(name, classDeclaration, new MethodTable(), new FieldTable());
        
        _scope.TryDeclareType(type);
    }


    public void BindMembersDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        _lookup.Unwrap();
        if (classDeclaration is not { })
        {
            return;
        }
        
        var typeScope = new BoundScope(_scope);
        foreach (var function in classDeclaration.Functions)
        {
            var functionBinder = new FunctionBinder(typeScope, _isScript, new FunctionBinderLookup(_lookup.CurrentType, _lookup.AvailableTypes, null!));
            functionBinder.BindFunctionDeclaration(function);
        }
        
        foreach (var declaredFunction in typeScope.GetDeclaredFunctions())
        {
            _lookup.CurrentType.MethodTable.Add(declaredFunction, null);
        }
        
        foreach (var field in classDeclaration.Fields)
        {
            var fieldBinder = new FieldBinder(typeScope, _isScript, new FieldBinderLookup(_lookup.AvailableTypes));
            fieldBinder.BindDeclaration(field);
        }
        
        foreach (var declaredField in typeScope.GetDeclaredFields())
        {
            _lookup.CurrentType.FieldTable.Add(declaredField);
        }
    }
    
    public ImmutableArray<Diagnostic> BindBody()
    {
        var diagnostics = new DiagnosticBag();
        var typeScope = new BoundScope(_scope);
        _lookup.Unwrap();
        
        foreach (var (functionSymbol, _) in _lookup.CurrentType.MethodTable)
        {
            typeScope.TryDeclareFunction(functionSymbol);
        }
        foreach (var fieldSymbol in _lookup.CurrentType.FieldTable)
        {
            typeScope.TryDeclareField(fieldSymbol);
        }
        
        foreach (var functionSymbol in _lookup.Unwrap().CurrentType.MethodTable.Symbols)
        {
            if (functionSymbol.Declaration is null
                && (functionSymbol.Name == SyntaxFacts.MainFunctionName || functionSymbol.Name == SyntaxFacts.ScriptMainFunctionName)
                && _lookup.CurrentType.Name == SyntaxFacts.StartTypeName)
            {
                // function generated from global statements
                // binding of this function should be done later to place proper returns;
                continue;
            }
            
            var functionBinder = new FunctionBinder(typeScope, _isScript, new FunctionBinderLookup(
                _lookup.CurrentType,
                _lookup.AvailableTypes,
                functionSymbol));
            var body = functionBinder.BindFunctionBody(functionSymbol);
            var loweredBody = Lowerer.Lower(body);
            if (!Equals(functionSymbol.ReturnType, TypeSymbol.Void)
                && !ControlFlowGraph.AllPathsReturn(loweredBody))
            {
                diagnostics.ReportAllPathsMustReturn(functionSymbol.Declaration.Unwrap().Identifier.Location);
            }

            _lookup.CurrentType.MethodTable.Declare(functionSymbol, loweredBody);
        }
        
        return diagnostics.ToImmutableArray();
    }
}