using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
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

    public ImmutableArray<Diagnostic> BindMembersSignatures(
        ClassDeclarationSyntax classDeclaration,
        TypeSymbol currentType)
    {
        _lookup.NG();

        var diagnostics  = new DiagnosticBag();
        var typeScope = new BoundScope(_scope);
        foreach (var member in classDeclaration.Members)
        {
            if (member.Kind is SyntaxKind.MethodDeclaration)
            {
                var method = (MethodDeclarationSyntax) member;
                var methodSignatureBinder = new MethodSignatureBinder(
                    new MethodSignatureBinderLookup(_lookup.AvailableTypes, currentType, isTopMethod: false),
                    typeScope);
                methodSignatureBinder.BindMethodSignature(method)
                    .AddRangeTo(diagnostics);
            }
            else if (member.Kind is SyntaxKind.FieldDeclaration)
            {
                var field = (FieldDeclarationSyntax)member;
                var fieldBinder = new FieldSignatureBinder(
                    typeScope, 
                    _isScript,
                    new FieldBinderLookup(_lookup.AvailableTypes, currentType));
                fieldBinder.BindDeclaration(field).AddRangeTo(diagnostics);
            }
            else
            {
                throw new Exception($"Unexpected member kind {member.Kind}");
            }
        }

        return diagnostics.ToImmutableArray();
    }
}