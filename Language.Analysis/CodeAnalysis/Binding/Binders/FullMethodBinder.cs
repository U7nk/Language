using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class FullMethodBinder
{
    readonly TypeSymbol _currentType;
    readonly DeclarationsBag _allDeclarations;
    
    bool _isScript;
    public MethodSymbol MethodSymbol { get; private set; }
    public Option<bool> SuccessfullyDeclaredInType => MethodDeclarationBinder.SuccessfullyDeclaredInType; 
    public BoundScope MethodScope { get; }

    public FullMethodBinder(BoundScope methodScope, TypeSymbol currentType, DeclarationsBag allDeclarations, bool isScript, bool isTopMethod)
    {
        _currentType = currentType;
        _allDeclarations = allDeclarations;
        _isScript = isScript;
        MethodDeclarationBinder = new MethodDeclarationBinder(methodScope, currentType, isTopMethod, allDeclarations);
        MethodScope = methodScope;
    }

    /// <summary>
    /// used for generated symbols
    /// </summary>
    /// <param name="methodScope"></param>
    /// <param name="currentType"></param>
    /// <param name="lookup"></param>
    /// <param name="isScript"></param>
    /// <param name="methodSymbol"></param>
    /// <param name="isTopMethod"></param>
    /// <param name="allDeclarations"></param>
    public FullMethodBinder(BoundScope methodScope, TypeSymbol currentType, bool isScript,
                            MethodSymbol methodSymbol, bool isTopMethod, DeclarationsBag allDeclarations)
    {
        _allDeclarations = allDeclarations;
        _currentType = currentType;
        _isScript = isScript;
        MethodDeclarationBinder = new MethodDeclarationBinder(methodScope, successfullyDeclaredInType:true, isTopMethod, currentType, allDeclarations);
        MethodScope = methodScope;
        MethodSymbol = methodSymbol;
        MethodBinder = new MethodBinder(methodScope, isScript, new MethodBinderLookup(allDeclarations, _currentType, methodSymbol));
    }
    
    MethodBinder MethodBinder { get; set; }
    MethodDeclarationBinder MethodDeclarationBinder { get; }

    public void BindMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax, DiagnosticBag diagnostics)
    {
        MethodSymbol = MethodDeclarationBinder.BindMethodDeclaration(methodDeclarationSyntax, diagnostics);
        MethodBinder = new MethodBinder(MethodScope, _isScript, new MethodBinderLookup(_allDeclarations, _currentType, MethodSymbol));
    }

    public BoundStatement BindMethodBody(DiagnosticBag diagnostics)
    {
        var boundStatement = MethodBinder.BindMethodBody(MethodSymbol);
        diagnostics.MergeWith(MethodBinder.Diagnostics);
        return boundStatement;
    }
}