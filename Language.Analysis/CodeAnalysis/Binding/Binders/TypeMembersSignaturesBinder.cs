using System.Collections.Immutable;
using System.Diagnostics;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeMembersSignaturesBinder
{
    readonly BaseBinderLookup _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;

    public TypeMembersSignaturesBinder(BaseBinderLookup lookup, BoundScope scope, bool isScript)
    {
        _lookup = lookup;
        _scope = scope;
        _isScript = isScript;
    }

    public ImmutableArray<Diagnostic> BindMembersSignatures(ClassDeclarationSyntax classDeclaration)
    {
        _lookup.NullGuard();
        _ = _scope.TryLookupType(classDeclaration.Identifier.Text, out var currentType);
        Debug.Assert(currentType != null, "Type should be declared before members");
        
        var diagnostics  = new DiagnosticBag();
        var typeScope = new BoundScope(_scope);
        foreach (var function in classDeclaration.Methods)
        {
            var functionBinder = new MethodSignatureBinder(new BaseBinderLookup(_lookup.AvailableTypes), typeScope);
            functionBinder.BindMethodSignature(function)
                .AddRangeTo(diagnostics);
        }
        
        foreach (var declaredFunction in typeScope.GetDeclaredMethods())
        {
            currentType.MethodTable.Add(declaredFunction, null);
        }
        
        foreach (var field in classDeclaration.Fields)
        {
            var fieldBinder = new FieldSignatureBinder(typeScope, _isScript, new FieldBinderLookup(_lookup.AvailableTypes));
            fieldBinder.BindDeclaration(field)
                .AddRangeTo(diagnostics);
        }
        
        foreach (var declaredField in typeScope.GetDeclaredFields())
        {
            currentType.FieldTable.Add(declaredField);
        }

        return diagnostics.ToImmutableArray();
    }
}