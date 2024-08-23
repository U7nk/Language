using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Binders.Class;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Namespace;

public class NamespaceBinder(BoundScope scope, DeclarationsBag declarations, BoundScope globalScope, NamespaceSyntax namespaceSyntax)
{
    private List<FullClassBinder> _classBinders;
    private BoundScope _globalScope = globalScope;
    private Option<NamespaceSymbol> _namespace;
    private NamespaceSyntax _namespaceSyntax = namespaceSyntax;

    public NamespaceSymbol BindDeclaration(DiagnosticBag diagnostics)
    {
        string lookupName = "";
        List<NamespaceSymbol> namespaces = new List<NamespaceSymbol>();
        for (var index = 0; index < namespaceSyntax.NameTokens.Count; index++)
        {
            var syntaxToken = namespaceSyntax.NameTokens[index];
            lookupName += syntaxToken.Text + namespaceSyntax.NameTokens.GetSeparator(index).OnSome(tok => tok.Text).SomeOr("");
            var lookupNamespace = _globalScope.TryLookupNamespace(lookupName[..^1]);
            if (lookupNamespace.IsNone)
            {
                var parent = namespaces.LastOrNone();
                var ns = new NamespaceSymbol(namespaceSyntax,
                                             syntaxToken.Text,
                                             lookupName, new List<TypeSymbol>(), parent, new List<NamespaceSymbol>());
                if (parent.IsSome)
                {
                    parent.Unwrap().Children.Add(ns);
                }

                _globalScope.TryDeclareNamespace(ns);
                namespaces.Add(ns);
            }
            else
            {
                namespaces.Add(lookupNamespace.Unwrap());   
            }
        }

        _namespace = namespaces.LastOrNone().Unwrap();
        return _namespace.Unwrap();
    }

    public void BindClassDeclarations(DiagnosticBag diagnostics)
    {
        _namespace.Unwrap();
        
        List<TypeSymbol> types = new List<TypeSymbol>(namespaceSyntax.Members.Length);
        _classBinders = new List<FullClassBinder>();
        foreach (var classDeclaration in namespaceSyntax.Members)
        {
            var typeBinder = new FullClassBinder(scope.CreateChild(), _globalScope, declarations, _namespace.Unwrap());
            typeBinder.BindClassDeclaration(classDeclaration, diagnostics)
                .AddTo(types)
                .AddTo(_namespace.Unwrap().Types);
            _classBinders.Add(typeBinder);
        }
    }

    /// <summary>
    /// Bind inheritance clauses, diagnose problems with inheritance and generic clause and bind class member signatures
    /// </summary>
    public void BindClassDeclarationPropertiesAndClassMembersSignatures(DiagnosticBag diagnostics)
    {
        foreach (var typeBinder in _classBinders)
        {
            typeBinder.BindInheritanceClause(diagnostics);
        }
        foreach (var typeBinder in _classBinders)
        {
            typeBinder.DiagnoseTypeDontInheritFromItself(diagnostics);
        }
        foreach (var typeBinder in _classBinders)
        {
            typeBinder.BindMembersSignatures(diagnostics);
        }
        foreach (var typeBinder in _classBinders)
        {
            typeBinder.DiagnoseDiamondProblem(diagnostics);
        }   
    }
    public void BindBodies(DiagnosticBag diagnostics)
    {
        foreach (var classBinder in _classBinders)
        {
            classBinder.BindClassBody(diagnostics);
        }
    }
}