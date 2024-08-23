using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Binders.Class;
using Language.Analysis.CodeAnalysis.Binding.Binders.Method;
using Language.Analysis.CodeAnalysis.Binding.Binders.Namespace;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

public class TypeBindingUnit
{
    public TypeBindingUnit(TypeSymbol type, BoundScope scope)
    {
        Type = type;
        Scope = scope;
    }

    public TypeSymbol Type { get; }
    public BoundScope Scope { get; }
}

internal sealed class ProgramBinder
{
    readonly BoundScope _scope;
    readonly ImmutableArray<SyntaxTree> _syntaxTrees;
    readonly DeclarationsBag _allDeclarations;
    readonly DiagnosticBag _diagnostics = new();
    List<NamespaceBinder> _namespaceBinders = new();

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

    internal ProgramBinder(BoundGlobalScope? previous, ImmutableArray<SyntaxTree> syntaxTrees, DeclarationsBag allDeclarations)
    {
        _scope = CreateParentScope();
        _syntaxTrees = syntaxTrees;
        _allDeclarations = allDeclarations;
    }

    void DeclareBuiltInTypes()
    {
        _scope.TryDeclareType(TypeSymbol.BuiltIn.Object(), Option.None);
        _scope.TryDeclareType(TypeSymbol.BuiltIn.Bool(), Option.None);
        _scope.TryDeclareType(TypeSymbol.BuiltIn.Int(), Option.None);
        _scope.TryDeclareType(TypeSymbol.BuiltIn.String(), Option.None);
        _scope.TryDeclareType(TypeSymbol.BuiltIn.Void(), Option.None);
    }

    public BoundGlobalScope BindGlobalScope()
    {
        DeclareBuiltInTypes();

        var namespaceSyntaxes = _syntaxTrees
            .Only<NamespaceSyntax>()
            .ToImmutableArray();


        _namespaceBinders = new List<NamespaceBinder>();
        var namespaceSymbols = new List<NamespaceSymbol>(namespaceSyntaxes.Length);
        foreach (var namespaceSyntax in namespaceSyntaxes)
        {
            var namespaceBinder = new NamespaceBinder(_scope, _allDeclarations, _scope, namespaceSyntax);
            namespaceBinder.BindDeclaration(_diagnostics).AddTo(namespaceSymbols);
            _namespaceBinders.Add(namespaceBinder);
        }

        foreach (var namespaceBinder in _namespaceBinders)
        {
            namespaceBinder.BindClassDeclarations(_diagnostics);
        }
        foreach (var namespaceBinder in _namespaceBinders)
        {
            namespaceBinder.BindClassDeclarationPropertiesAndClassMembersSignatures(_diagnostics);
        }
        
        MethodSymbol? mainMethod = TryFindMainMethod();
        DiagnoseMainMethod();

        if (mainMethod is null)
        {
            _diagnostics.ReportMainMethodShouldBeDeclared(_syntaxTrees.First().SourceText);
        }

        return new BoundGlobalScope(
            _diagnostics.ToImmutableArray(),
            mainMethod,
            _scope.GetDeclaredTypes(), _scope.GetDeclaredVariables(),
            _allDeclarations,
            _namespaceBinders.ToImmutableArray());
    }
    
    MethodSymbol? TryFindMainMethod()
    {
        foreach (var function in _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Select(declaration=>declaration.MethodSymbol)))
        {
            if (function.Name == SyntaxFacts.MAIN_METHOD_NAME)
                return function;
        }

        return null;
    }

    void DiagnoseMainMethod()
    {
        var methods = _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Select(declaration=> declaration.MethodSymbol)).ToList();
        
        foreach (var function in methods.Where(x => x.Name == "main"))
        {
            if (function.Parameters.Any()
                || !Equals(function.ReturnType, TypeSymbol.BuiltIn.Void()) 
                || !function.IsStatic )
            {
                var identifierLocation = function.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>()
                    .Identifier.Location;
                
                _diagnostics.ReportMainMustHaveCorrectSignature(identifierLocation);
            }
        }
    }
    
    static BoundScope CreateParentScope()
    {
        var parent = CreateRootScope();
        return parent;
    }

    static BoundScope CreateRootScope()
    {
        var result = BoundScope.CreateRootScope(Option.None);

        return result;
    }


    public BoundProgram BindProgram(
        BoundProgram? previous,
        BoundGlobalScope globalScope)
    {
        var diagnostics = new DiagnosticBag();

        var typesToBind = globalScope.Types.Exclude(
            x => Equals(x, TypeSymbol.BuiltIn.Object())
                 || Equals(x, TypeSymbol.BuiltIn.Void()) 
                 || Equals(x, TypeSymbol.BuiltIn.Bool()) 
                 || Equals(x, TypeSymbol.BuiltIn.Int()) 
                 || Equals(x, TypeSymbol.BuiltIn.String()) 
                 || Equals(x, TypeSymbol.BuiltIn.Error()))
            .ToImmutableArray();

        foreach (var namespaceBinder in _namespaceBinders)
        {
            namespaceBinder.BindBodies(diagnostics);
        }

        var boundProgram = new BoundProgram(
            previous,
            diagnostics.ToImmutableArray(),
            globalScope.MainMethod,
            typesToBind);
        return boundProgram;
    }
}