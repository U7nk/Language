using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Binders.Field;
using Language.Analysis.CodeAnalysis.Binding.Binders.Method;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Class;

internal sealed class FullClassBinder
{
    readonly bool _isScript;
    public BoundScope ParentScope { get; }
    public BoundScope TypeScope { get; }
    readonly DeclarationsBag _allDeclarations;
    
    Option<ClassSignatureBinder> ClassSignatureBinder { get; set; }
    public Option<ClassBinder> ClassBinder { get; private set; }
    public Option<TypeSymbol> TypeSymbol { get; private set; }
    public Option<IEnumerable<FullMethodBinder>> MethodBinders { get; private set; }
    public Option<IEnumerable<FullFieldBinder>> FieldBinders { get; private set; }
    public Option<ClassMembersSignaturesBinder> ClassMembersSignatureBinder { get; private set; }
    
    
    public FullClassBinder(BoundScope parentScope, DeclarationsBag allDeclarations, bool isScript)
    {
        TypeScope = new BoundScope(parentScope);
        ParentScope = parentScope;
        _allDeclarations = allDeclarations;
        _isScript = isScript;
    }

    /// <summary>
    /// Used for generated symbols.
    /// </summary>
    /// <param name="parentScope"></param>
    /// <param name="allDeclarations"></param>
    /// <param name="isScript"></param>
    /// <param name="typeSymbol"></param>
    /// <param name="isTopMethod"></param>
    public FullClassBinder(BoundScope parentScope, DeclarationsBag allDeclarations, bool isScript, TypeSymbol typeSymbol, bool isTopMethod)
    {
        _allDeclarations = allDeclarations;
        TypeScope = new BoundScope(parentScope);
        ParentScope = parentScope;
        _isScript = isScript;
        ClassBinder = new ClassBinder(parentScope, isScript, typeSymbol);
        TypeSymbol = typeSymbol;
        _allDeclarations = allDeclarations;

        MethodBinders = typeSymbol.MethodTable
            .Select(x => new FullMethodBinder(new BoundScope(TypeScope), typeSymbol, isScript, x.MethodSymbol, isTopMethod, allDeclarations))
            .ToList();
        FieldBinders = typeSymbol.FieldTable
            .Select(x => new FullFieldBinder(new BoundScope(TypeScope), _isScript, typeSymbol, _allDeclarations))
            .ToList();
    }

    
    public bool BindClassDeclaration(ClassDeclarationSyntax classDeclaration, DiagnosticBag diagnostics)
    {
        ClassSignatureBinder = new ClassSignatureBinder(TypeScope, ParentScope, _allDeclarations);
        var classDeclarationBind = ClassSignatureBinder.Unwrap().BindClassDeclaration(classDeclaration, diagnostics);
        ClassBinder = new ClassBinder(TypeScope, _isScript, classDeclarationBind.Ok);
        TypeSymbol = classDeclarationBind.Ok;

        ClassMembersSignatureBinder = new ClassMembersSignaturesBinder(TypeSymbol.Unwrap(), _allDeclarations, TypeScope, _isScript);
        return classDeclarationBind.IsOk;
    }

    public void BindInheritanceClause(DiagnosticBag diagnostics)
    {
        ClassSignatureBinder.Unwrap().BindInheritanceClause(TypeSymbol.Unwrap(), diagnostics);
    }

    public void DiagnoseTypeDontInheritFromItself(DiagnosticBag diagnostics)
    {
        ClassSignatureBinder.Unwrap().DiagnoseTypeDontInheritFromItself(TypeSymbol.Unwrap(), diagnostics);
    }

    public void BindMembersSignatures(DiagnosticBag diagnostics)
    {
        (List<FullMethodBinder> methodBinders, List<FullFieldBinder> fieldBinders) binders = ClassMembersSignatureBinder.Unwrap().BindMembersSignatures(diagnostics);
        FieldBinders = binders.fieldBinders;
        MethodBinders = binders.methodBinders;
    }

    public void DiagnoseDiamondProblem(DiagnosticBag diagnostics)
    {
        ClassMembersSignatureBinder.Unwrap().DiagnoseDiamondProblem(diagnostics);
    }

    public void BindClassBody(DiagnosticBag diagnostics)
    {
        ClassBinder.Unwrap().BindClassBody(MethodBinders.Unwrap(), FieldBinders.Unwrap());
        diagnostics.MergeWith(ClassBinder.Unwrap().Diagnostics);
    }
}