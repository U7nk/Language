using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Class;

sealed class ClassSignatureBinder
{
    readonly BoundScope _typeScope;
    readonly BoundScope _parentScope;
    readonly DeclarationsBag _allDeclarations;
    private readonly NamespaceSymbol _containingNamespace;

    public ClassSignatureBinder(BoundScope typeScope, BoundScope parentScope, DeclarationsBag allDeclarations, NamespaceSymbol containingNamespace)
    {
        _typeScope = typeScope;
        _parentScope = parentScope;
        _allDeclarations = allDeclarations;
        _containingNamespace = containingNamespace;
    }

    public Result<TypeSymbol, Unit> BindClassDeclaration(ClassDeclarationSyntax classDeclaration, DiagnosticBag diagnostics)
    {
        var searchScope = _typeScope.CreateChild();
        _= BindClassDeclarationWithoutConstraints(classDeclaration, diagnostics, searchScope);
        var typeWithConstraints = BindClassDeclarationInternal(classDeclaration, diagnostics, _typeScope, searchScope);
        
        // constraints satisfy themselves
        if (classDeclaration.GenericConstraintsClause.IsSome)
        {
            foreach (var constraintsClauseSyntax in classDeclaration.GenericConstraintsClause.Unwrap())
            {
                foreach (var typeConstraint in constraintsClauseSyntax.TypeConstraints)
                {
                    _ = TypeSymbol.FromNamedTypeExpression(typeConstraint, _typeScope, diagnostics, _containingNamespace);
                }
            }
        }
        _allDeclarations.AddDeclaration(typeWithConstraints.Ok, classDeclaration);
        return typeWithConstraints.Ok;
    }

    Result<TypeSymbol, Unit> BindClassDeclarationInternal(ClassDeclarationSyntax classDeclaration,
                                                  DiagnosticBag diagnostics, 
                                                  BoundScope declaringScope, 
                                                  BoundScope searchingScope)
    {
        var genericParameters = new List<TypeSymbol>();
        if (classDeclaration.GenericParametersSyntax.IsSome)
        {
            var searchWhileBindingScope = searchingScope.CreateChild();

            var typeConstraintsByGenericName = new Dictionary<string, IEnumerable<TypeSymbol>>();
            foreach (var x in classDeclaration.GenericConstraintsClause.SomeOr(ImmutableArray<GenericConstraintsClauseSyntax>.Empty))
            {
                if (classDeclaration.GenericConstraintsClause.IsSome)
                {
                    var typeConstraintsSyntax = x.TypeConstraints;
                    var typeConstraints = new List<TypeSymbol>();
                    foreach (var genericTypeConstraintIdentifier in typeConstraintsSyntax)
                    {
                        var genericTypeConstraintType = TypeSymbol.FromNamedTypeExpression(genericTypeConstraintIdentifier, searchWhileBindingScope, diagnostics, _containingNamespace);
                        if (Equals(genericTypeConstraintType, TypeSymbol.BuiltIn.Error()))
                            continue;

                        typeConstraints.Add(genericTypeConstraintType);
                    }

                    typeConstraintsByGenericName.Add(x.Identifier.Text, typeConstraints);
                }
            }
            
            var genericArgumentsSyntax = classDeclaration.GenericParametersSyntax.Unwrap();
            foreach (var genericArgumentSyntax in genericArgumentsSyntax.Arguments)
            {
                var genericArgumentName = genericArgumentSyntax.GetName();
                var typeConstraints = typeConstraintsByGenericName.TryGetValue(genericArgumentName, out var value) 
                    ? Option.Some(value.ToImmutableArray()) 
                    : Option.None;

                var baseTypes = new SingleOccurenceList<TypeSymbol> { TypeSymbol.BuiltIn.Object() };
                if (typeConstraints.IsSome) 
                    typeConstraints.Unwrap().AddRangeTo(baseTypes); 
                
                var genericArgumentType = TypeSymbol.New(genericArgumentName, 
                                                 Option.None,
                                                 null,
                                                 new MethodTable(),
                                                 new FieldTable(),
                                                 baseTypes,
                                                 isGenericMethodParameter: false,
                                                 isGenericClassParameter: true, 
                                                 genericParameters: Option.None, genericParameterTypeConstraints: typeConstraints, isGenericTypeDefinition: true,
                                                 _containingNamespace);

                
                if (!declaringScope.TryDeclareType(genericArgumentType, _containingNamespace, isScopeTied: true))
                {
                    diagnostics.ReportAmbiguousType(genericArgumentSyntax.Location,
                                                    genericArgumentName,
                                                    _typeScope.GetDeclaredTypes().Where(x => x.Name == genericArgumentName));
                }
                else
                {
                    genericParameters.Add(genericArgumentType);
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
                                        genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: true,
                                        _containingNamespace);

        if (!_parentScope.TryDeclareType(typeSymbol, _containingNamespace))
        {
            var existingClassDeclarationsIdentifiers = _allDeclarations.LookupDeclarations<ClassDeclarationSyntax>(typeSymbol)
                .Select(x => x.Identifier);
            foreach (var identifier in existingClassDeclarationsIdentifiers)
            {
                diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
                    identifier.Location,
                    identifier.Text);
            }

            return Unit.Default;
        }

        return typeSymbol;
    }

    Result<TypeSymbol, Unit> BindClassDeclarationWithoutConstraints(ClassDeclarationSyntax classDeclaration, DiagnosticBag diagnostics, BoundScope bindingScope)
    {
        var genericParameters = new List<TypeSymbol>();
        if (classDeclaration.GenericParametersSyntax.IsSome)
        {
            var genericArgumentsSyntax = classDeclaration.GenericParametersSyntax.Unwrap();
            foreach (var genericArgumentSyntax in genericArgumentsSyntax.Arguments)
            {
                var genericArgumentName = genericArgumentSyntax.GetName();
                var typeConstraints = Option.None;
    
                var baseTypes = new SingleOccurenceList<TypeSymbol> { TypeSymbol.BuiltIn.Object() };
                
                var genericArgumentType = TypeSymbol.New(genericArgumentName, 
                                                 Option.None,
                                                 null,
                                                 new MethodTable(),
                                                 new FieldTable(),
                                                 baseTypes,
                                                 isGenericMethodParameter: false,
                                                 isGenericClassParameter: true, 
                                                 genericParameters: Option.None, genericParameterTypeConstraints: typeConstraints, isGenericTypeDefinition: true,
                                                 _containingNamespace);
    
                
                if (!bindingScope.TryDeclareType(genericArgumentType, _containingNamespace, isScopeTied: true))
                {
                    diagnostics.ReportAmbiguousType(genericArgumentSyntax.Location,
                                                    genericArgumentName,
                                                    _parentScope.GetDeclaredTypes().Where(x => x.Name == genericArgumentName));
                }
                else
                {
                    genericParameters.Add(genericArgumentType);
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
                                        genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: true,
                                        _containingNamespace);
        
        if (!bindingScope.TryDeclareType(typeSymbol, _containingNamespace, isScopeTied: true))
        {
            var existingClassDeclarationsIdentifiers = _allDeclarations.LookupDeclarations<ClassDeclarationSyntax>(typeSymbol)
                .Select(x => x.Identifier);
            foreach (var identifier in existingClassDeclarationsIdentifiers)
            {
                diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
                    identifier.Location,
                    identifier.Text);
            }
    
            return Unit.Default;
        }
    
        return typeSymbol;
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

        if (Equals(currentType, TypeSymbol.BuiltIn.Object()))
            return;

        if (currentType.DeclarationEquals(checkingType))
        {
            var existingDeclarationSyntax = _allDeclarations.LookupDeclarations<ClassDeclarationSyntax>(currentType);
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
        builder.Add(TypeSymbol.BuiltIn.Object());
        
        if (syntax.IsSome)
        {
            foreach (var baseTypeIdentifier in syntax.Unwrap().BaseTypes)
            {
                var baseTypeName = baseTypeIdentifier.Text;
                var baseTypeSymbol = _typeScope.TryLookupType(baseTypeName, _containingNamespace);
                if (baseTypeSymbol.IsNone)
                {
                    diagnostics.ReportUndefinedType(baseTypeIdentifier.Location, baseTypeName);
                    continue;
                }

                if (Equals(baseTypeSymbol.Unwrap(), TypeSymbol.BuiltIn.Object()))
                    continue;
                
                builder.Add(baseTypeSymbol.Unwrap());
            }
        }
        
        return builder.ToImmutable();
    }
}