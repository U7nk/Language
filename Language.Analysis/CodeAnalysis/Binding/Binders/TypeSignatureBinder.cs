using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeSignatureBinder
{
    readonly BoundScope _outerScope;
    readonly BinderLookup _lookup;

    public TypeSignatureBinder(BoundScope outerScope, BinderLookup lookup)
    {
        _outerScope = outerScope;
        _lookup = lookup;
    }

    public Result<TypeBindingUnit, Unit> BindClassDeclaration(ClassDeclarationSyntax classDeclaration, DiagnosticBag diagnostics)
    {
        var typeScope = new BoundScope(_outerScope);
        
        var typeConstraintsByGenericName = new Dictionary<string, IEnumerable<TypeSymbol>>();
        var genericParameters = new List<TypeSymbol>();
        if (classDeclaration.GenericParametersSyntax.IsSome)
        {
            foreach (var x in classDeclaration.GenericConstraintsClause.SomeOr(ImmutableArray<GenericConstraintsClauseSyntax>.Empty))
            {
                if (classDeclaration.GenericConstraintsClause.IsSome)
                {
                    var typeConstraintsSyntax = x.TypeConstraints;
                    var typeConstraints = new List<TypeSymbol>();
                    foreach (var genericTypeConstraintIdentifier in typeConstraintsSyntax)
                    {
                        var genericTypeConstraintName = genericTypeConstraintIdentifier.Identifier.Text;
                        if (!_outerScope.TryLookupType(genericTypeConstraintName, out var genericTypeConstraintType))
                        {
                            diagnostics.ReportUndefinedType(genericTypeConstraintIdentifier.Location,
                                                            genericTypeConstraintName);
                            continue;
                        }

                        typeConstraints.Add(genericTypeConstraintType);
                    }

                    typeConstraintsByGenericName.Add(x.Identifier.Text, typeConstraints);
                }
            }

            var genericArgumentsSyntax = classDeclaration.GenericParametersSyntax.Unwrap();
            foreach (var genericArgumentSyntax in genericArgumentsSyntax.Arguments)
            {
                var genericArgumentName = genericArgumentSyntax.Identifier.Text;
                var typeConstraints = typeConstraintsByGenericName.TryGetValue(genericArgumentName, out var value) 
                    ? Option.Some(value.ToImmutableArray()) 
                    : Option.None;

                var baseTypes = new SingleOccurenceList<TypeSymbol> { BuiltInTypeSymbols.Object };
                if (typeConstraints.IsSome) 
                    typeConstraints.Unwrap().AddRangeTo(baseTypes);
                
                var genericType = TypeSymbol.New(genericArgumentName, 
                                                 Option.None,
                                                 null,
                                                 new MethodTable(),
                                                 new FieldTable(),
                                                 baseTypes,
                                                 isGenericMethodParameter: false,
                                                 isGenericClassParameter: true, 
                                                 Option.None, typeConstraints);

                
                if (!typeScope.TryDeclareType(genericType))
                {
                    diagnostics.ReportAmbiguousType(genericArgumentSyntax.Location,
                                                    genericArgumentName,
                                                    _outerScope.GetDeclaredTypes().Where(x => x.Name == genericArgumentName));
                }
                else
                {
                    genericParameters.Add(genericType);
                }
            }
        }
        
        var name = classDeclaration.Identifier.Text;
        var typeSymbol = TypeSymbol.New(name,
                                        classDeclaration, 
                                        classDeclaration.InheritanceClause,
                                        new MethodTable(),
                                        new FieldTable(),
                                        new SingleOccurenceList<TypeSymbol>(),
                                        isGenericMethodParameter: false, 
                                        isGenericClassParameter: false,
                                        genericParameters: genericParameters.ToImmutableArray(),
                                        genericParameterTypeConstraints: Option.None);
        _lookup.AddDeclaration(typeSymbol, classDeclaration);
        
        if (!_outerScope.TryDeclareType(typeSymbol))
        {
            var existingClassDeclarationsIdentifiers = _lookup.LookupDeclarations<ClassDeclarationSyntax>(typeSymbol)
                .Select(x => x.Identifier);
            foreach (var identifier in existingClassDeclarationsIdentifiers)
            {
                diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
                    identifier.Location,
                    identifier.Text);
            }

            return Unit.Default;
        }

        return new TypeBindingUnit(typeSymbol, typeScope);
    }
    
    public void BindInheritanceClause(TypeBindingUnit typeBindingUnit, DiagnosticBag diagnostics)
    {
        var baseTypes = BindInheritanceClause(typeBindingUnit.Type.InheritanceClauseSyntax, diagnostics);
        AddBaseTypesToCurrentType(typeBindingUnit.Type, baseTypes);
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

        if (Equals(currentType, BuiltInTypeSymbols.Object))
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
                if (!_outerScope.TryLookupType(baseTypeName, out var baseTypeSymbol))
                {
                    diagnostics.ReportUndefinedType(baseTypeIdentifier.Location, baseTypeName);
                    continue;
                }

                if (Equals(baseTypeSymbol, BuiltInTypeSymbols.Object))
                    continue;
                
                builder.Add(baseTypeSymbol);
            }
        }
        
        return builder.ToImmutable();
    }
}