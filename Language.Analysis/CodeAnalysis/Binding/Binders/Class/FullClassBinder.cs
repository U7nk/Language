using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Binders.Field;
using Language.Analysis.CodeAnalysis.Binding.Binders.Method;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Class;

internal sealed class FullClassBinder
{
    public BoundScope ParentScope { get; }
    public BoundScope TypeScope { get; }
    private readonly BoundScope _globalScope;
    readonly DeclarationsBag _allDeclarations;
    private readonly NamespaceSymbol _containingNamespace;

    Option<ClassSignatureBinder> ClassSignatureBinder { get; set; }
    public Option<ClassBinder> ClassBinder { get; private set; }
    public Option<TypeSymbol> TypeSymbol { get; private set; }
    public Option<IEnumerable<FullMethodBinder>> MethodBinders { get; private set; }
    public Option<IEnumerable<FullFieldBinder>> FieldBinders { get; private set; }
    public Option<ClassMembersSignaturesBinder> ClassMembersSignatureBinder { get; private set; }
    
    
    public FullClassBinder(BoundScope parentScope, BoundScope globalScope, DeclarationsBag allDeclarations, NamespaceSymbol containingNamespace)
    {
        TypeScope = parentScope.CreateChild();
        ParentScope = parentScope;
        _globalScope = globalScope;
        _allDeclarations = allDeclarations;
        _containingNamespace = containingNamespace;
    }
    
    public FullClassBinder(BoundScope parentScope, BoundScope globalScope, DeclarationsBag allDeclarations, TypeSymbol typeSymbol)
    {
        _globalScope = globalScope;
        _allDeclarations = allDeclarations;
        TypeScope = parentScope.CreateChild();
        ParentScope = parentScope;
        ClassBinder = new ClassBinder(parentScope, typeSymbol);
        TypeSymbol = typeSymbol;
        _allDeclarations = allDeclarations;

        MethodBinders = typeSymbol.MethodTable
            .Select(x => new FullMethodBinder(TypeScope.CreateChild(), globalScope, typeSymbol, x.MethodSymbol, allDeclarations))
            .ToList();
        FieldBinders = typeSymbol.FieldTable
            .Select(x => new FullFieldBinder(TypeScope.CreateChild(), typeSymbol, _allDeclarations))
            .ToList();
    }

    
    public TypeSymbol BindClassDeclaration(ClassDeclarationSyntax classDeclaration, DiagnosticBag diagnostics)
    {
        ClassSignatureBinder = new ClassSignatureBinder(TypeScope, ParentScope, _allDeclarations, _containingNamespace);
        var classDeclarationBind = ClassSignatureBinder.Unwrap().BindClassDeclaration(classDeclaration, diagnostics);
        ClassBinder = new ClassBinder(TypeScope, classDeclarationBind.Ok);
        TypeSymbol = classDeclarationBind.Ok;

        ClassMembersSignatureBinder = new ClassMembersSignaturesBinder(TypeSymbol.Unwrap(), _allDeclarations, TypeScope, _globalScope);
        return classDeclarationBind.Ok;
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