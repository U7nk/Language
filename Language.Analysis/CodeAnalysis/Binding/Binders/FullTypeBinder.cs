using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class FullTypeBinder
{
    readonly bool _isScript;
    public BoundScope ParentScope { get; }
    public BoundScope TypeScope { get; }
    DeclarationsBag _allDeclarations;
    public FullTypeBinder(BoundScope parentScope, DeclarationsBag allDeclarations, bool isScript)
    {
        
        TypeScope = new BoundScope(parentScope);
        ParentScope = parentScope;
        _allDeclarations = allDeclarations;
        _isScript = isScript;
        
        TypeSignatureBinder = new(TypeScope, ParentScope, allDeclarations);
        TypeBinder = null;
    }
    
    
    /// <summary>
    /// Used for generated symbols.
    /// </summary>
    /// <param name="parentScope"></param>
    /// <param name="lookup"></param>
    /// <param name="isScript"></param>
    /// <param name="typeSymbol"></param>
    public FullTypeBinder(BoundScope parentScope, DeclarationsBag allDeclarations, bool isScript, TypeSymbol typeSymbol, bool isTopMethod)
    {
        _allDeclarations = allDeclarations;
        TypeScope = new BoundScope(parentScope);
        ParentScope = parentScope;
        _isScript = isScript;
        TypeBinder = new TypeBinder(parentScope, isScript, new TypeBinderLookup(typeSymbol, allDeclarations), typeSymbol);
        TypeSymbol = typeSymbol;
        _allDeclarations = allDeclarations;

        MethodBinders = typeSymbol.MethodTable
            .Select(x => new FullMethodBinder(new BoundScope(TypeScope), typeSymbol, isScript, x.MethodSymbol, isTopMethod, allDeclarations))
            .ToList();
    }
    
    

    public TypeBinder TypeBinder { get; private set; }
    public TypeSymbol TypeSymbol { get; private set; }
    public IEnumerable<FullMethodBinder> MethodBinders { get; private set; }
    public IEnumerable<FullFieldBinder> FieldBinders { get; private set; }

    public TypeMembersSignaturesBinder TypeMembersSignatureBinder { get; private set; }
    public TypeSignatureBinder TypeSignatureBinder { get; }

    public Result<TypeSymbol, Unit> BindClassDeclaration(ClassDeclarationSyntax classDeclaration,
                                                         DiagnosticBag diagnostics)
    {
        var typeSymbol = TypeSignatureBinder.BindClassDeclaration(classDeclaration, diagnostics);
        TypeBinder = new TypeBinder(TypeScope, _isScript, new TypeBinderLookup(typeSymbol.Ok, _allDeclarations), typeSymbol.Ok);
        TypeSymbol = typeSymbol.Ok;

        TypeMembersSignatureBinder = new TypeMembersSignaturesBinder(TypeSymbol, _allDeclarations, TypeScope, _isScript);
        return typeSymbol;
    }

    public void BindInheritanceClause(DiagnosticBag diagnostics)
    {
        TypeSignatureBinder.BindInheritanceClause(TypeSymbol, diagnostics);
    }

    public void DiagnoseTypeDontInheritFromItself(DiagnosticBag diagnostics)
    {
        TypeSignatureBinder.DiagnoseTypeDontInheritFromItself(TypeSymbol, diagnostics);
    }

    public void BindMembersSignatures(DiagnosticBag diagnostics)
    {
        (List<FullMethodBinder> methodBinders, List<FullFieldBinder> fieldBinders) binders = TypeMembersSignatureBinder.BindMembersSignatures(diagnostics);
        FieldBinders = binders.fieldBinders;
        MethodBinders = binders.methodBinders;
    }

    public void DiagnoseDiamondProblem(DiagnosticBag diagnostics)
    {
        TypeMembersSignatureBinder.DiagnoseDiamondProblem(diagnostics);
    }

    public void BindClassBody(DiagnosticBag diagnostics)
    {
        TypeBinder.BindClassBody(MethodBinders, FieldBinders);
        diagnostics.MergeWith(TypeBinder.Diagnostics);
    }
}