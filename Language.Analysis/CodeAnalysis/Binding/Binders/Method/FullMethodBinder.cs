using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Method;

sealed class FullMethodBinder
{
    readonly TypeSymbol _currentType;
    private readonly BoundScope _globalScope;
    readonly DeclarationsBag _allDeclarations;
    public MethodSymbol MethodSymbol { get; private set; }
    public Option<bool> SuccessfullyDeclaredInType => MethodDeclarationBinder.SuccessfullyDeclaredInType; 
    public BoundScope MethodScope { get; }

    public FullMethodBinder(BoundScope methodScope, TypeSymbol currentType, DeclarationsBag allDeclarations, BoundScope globalScope)
    {
        _currentType = currentType;
        _allDeclarations = allDeclarations;
        _globalScope = globalScope;
        MethodDeclarationBinder = new MethodDeclarationBinder(methodScope, currentType, allDeclarations);
        MethodScope = methodScope;
    }

    /// <summary>
    /// used for generated symbols
    /// </summary>
    public FullMethodBinder(BoundScope methodScope, BoundScope globalScope, TypeSymbol currentType, MethodSymbol methodSymbol, DeclarationsBag allDeclarations)
    {
        _globalScope = globalScope;
        _allDeclarations = allDeclarations;
        _currentType = currentType;
        MethodDeclarationBinder = new MethodDeclarationBinder(methodScope, successfullyDeclaredInType:true, currentType, allDeclarations);
        MethodScope = methodScope;
        MethodSymbol = methodSymbol;
        MethodBinder = new MethodBinder(methodScope, globalScope, _currentType, methodSymbol);
    }
    
    MethodBinder MethodBinder { get; set; }
    MethodDeclarationBinder MethodDeclarationBinder { get; }

    public void BindMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax, DiagnosticBag diagnostics)
    {
        MethodSymbol = MethodDeclarationBinder.BindMethodDeclaration(methodDeclarationSyntax, diagnostics);
        MethodBinder = new MethodBinder(MethodScope, _globalScope, _currentType, MethodSymbol);
    }

    public BoundStatement BindMethodBody(DiagnosticBag diagnostics)
    {
        var boundStatement = MethodBinder.BindMethodBody(MethodSymbol);
        diagnostics.MergeWith(MethodBinder.Diagnostics);
        return boundStatement;
    }
}