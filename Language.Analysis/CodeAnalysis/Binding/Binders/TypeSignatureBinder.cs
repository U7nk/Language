using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeSignatureBinder
{
    readonly BoundScope _scope;
    readonly BinderLookup _lookup;

    public TypeSignatureBinder(BoundScope scope, BinderLookup lookup)
    {
        _scope = scope;
        _lookup = lookup;
    }

    public ImmutableArray<Diagnostic> BindClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        var name = classDeclaration.Identifier.Text;
        var typeSymbol = TypeSymbol.New(name,
                                        classDeclaration, 
                                        classDeclaration.InheritanceClause,
                                        new MethodTable(),
                                        new FieldTable());
        _lookup.AddDeclaration(typeSymbol, classDeclaration);
        
        var diagnostics = new DiagnosticBag();
        if (!_scope.TryDeclareType(typeSymbol))
        {
            var existingClassDeclarationsIdentifiers = _lookup.LookupDeclarations<ClassDeclarationSyntax>(typeSymbol)
                .Select(x => x.Identifier);
            foreach (var identifier in existingClassDeclarationsIdentifiers)
            {
                diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
                    identifier.Location,
                    identifier.Text);
            }

            return diagnostics.ToImmutableArray();
        }

        return diagnostics.ToImmutableArray();
    }
    
    public void BindInheritanceClause(TypeSymbol typeSymbol, DiagnosticBag diagnostics)
    {
        var baseTypes = BindInheritanceClause(typeSymbol.InheritanceClauseSyntax, diagnostics);
        AddBaseTypesToCurrentType(typeSymbol, baseTypes);
    } 
    
    void AddBaseTypesToCurrentType(TypeSymbol currentType, ImmutableArray<TypeSymbol> baseTypes)
    {
        foreach (var baseType in baseTypes)
        {
            currentType.BaseTypes.Add(baseType);
        }
    }

    /// <summary>
    /// TODO: make warning more specific. So it will show where exactly type is inherited from itself. <br/>
    /// <br/>
    /// Checks if type inherits from itself. <br/>
    /// If so, adds diagnostic to <see cref="diagnostics"/>.
    /// Diagnostic is added to every <see cref="InheritanceClauseSyntax"/> location.
    /// </summary>
    /// <param name="currentType">Type that is being checked for inheritance from itself.</param>
    /// <param name="diagnostics">Diagnostic bag to add diagnostic to.</param>
    /// <returns>true if type inherits from itself, false otherwise.</returns>
    public void DiagnoseTypeDontInheritFromItself(TypeSymbol currentType, DiagnosticBag diagnostics)
    {
        foreach (var baseType in currentType.BaseTypes)
        {
            DiagnoseTypeDontInheritFromItselfInternal(baseType, currentType, diagnostics);
        }
    }

    void DiagnoseTypeDontInheritFromItselfInternal(TypeSymbol currentType, TypeSymbol checkingType,
                                                  DiagnosticBag diagnostics, 
                                                  Option<HashSet<TypeSymbol>> visitedTypes = default)
    {
        if (visitedTypes.IsNone)
            visitedTypes = new HashSet<TypeSymbol>();

        if (currentType == BuiltInTypeSymbols.Object)
            return;

        if (currentType.DeclarationEquals(checkingType))
        {
            var existingDeclarationSyntax = _lookup.LookupDeclarations<ClassDeclarationSyntax>(currentType);
            foreach (var declarationSyntax in existingDeclarationSyntax)
            {
                diagnostics.ReportClassCannotInheritFromSelf(declarationSyntax.Identifier);
            }
        }
        
        if (!visitedTypes.Unwrap().Add(currentType))
            return;
        
        foreach (var baseType in currentType.BaseTypes)
        {
            DiagnoseTypeDontInheritFromItselfInternal(baseType, checkingType, diagnostics, visitedTypes);
        }
    }

    /// <summary>
    /// <b>NOTE:</b> All types is implicitly inherited from <see cref="BuiltInTypeSymbols.Object"/> type. <br/>
    /// So every return array will contain at least one element - <see cref="BuiltInTypeSymbols.Object"/> type.
    /// </summary>
    /// <param name="syntax">Inheritance clause syntax.</param>
    /// <param name="diagnostics">Diagnostic bag to add diagnostic to.</param>
    /// <returns></returns>
    ImmutableArray<TypeSymbol> BindInheritanceClause(Option<InheritanceClauseSyntax> syntax, DiagnosticBag diagnostics)
    {
        var builder = ImmutableArray.CreateBuilder<TypeSymbol>();
        builder.Add(BuiltInTypeSymbols.Object);
        
        if (syntax.IsSome)
        {
            foreach (var baseTypeIdentifier in syntax.Unwrap().BaseTypes)
            {
                var baseTypeName = baseTypeIdentifier.Text;
                if (!_scope.TryLookupType(baseTypeName, out var baseTypeSymbol))
                {
                    diagnostics.ReportUndefinedType(baseTypeIdentifier.Location, baseTypeName);
                    continue;
                }

                if (baseTypeSymbol == BuiltInTypeSymbols.Object)
                    continue;
                
                builder.Add(baseTypeSymbol);
            }
        }
        
        return builder.ToImmutable();
    }
}