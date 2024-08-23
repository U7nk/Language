using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class TypeSymbol : Symbol, ITypedSymbol
{
    public static class BuiltIn
{
    public static readonly NamespaceSymbol BuiltInTypeSymbolNamespace =
        new NamespaceSymbol(Option.None,
                            "SystemGlobal",
                            "SystemGlobal",
                            new List<TypeSymbol>(),
                            Option.None,
                            new List<NamespaceSymbol>());

    
    private static Option<TypeSymbol> _error;
    public static TypeSymbol Error()
    {
        if (_error.IsNone)
        {
            _error = TypeSymbol.New("error", Option.None,
                                      inheritanceClauseSyntax: null,
                                      methodTable: new MethodTable(),
                                      fieldTable: new FieldTable(),
                                      baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                      isGenericMethodParameter: false,
                                      isGenericClassParameter: false,
                                                            
                                      genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false,
                                      BuiltInTypeSymbolNamespace);
            BuiltInTypeSymbolNamespace.Types.Add(_error.Unwrap());
        }
        
        return _error.Unwrap();
    }
    
    private static Option<TypeSymbol> _void;
    public static TypeSymbol Void()
    {
        if (_void.IsNone)
        {
            _void = TypeSymbol.New("void",
                                   Option.None,
                                   inheritanceClauseSyntax: null,
                                   methodTable: new MethodTable(),
                                   fieldTable: new FieldTable(),
                                   baseTypes: new SingleOccurenceList<TypeSymbol>(),
                                   isGenericMethodParameter: false,
                                   isGenericClassParameter: false,
                                   genericParameters: Option.None,
                                   genericParameterTypeConstraints: Option.None,
                                   isGenericTypeDefinition: false,
                                   BuiltInTypeSymbolNamespace);
            
            BuiltInTypeSymbolNamespace.Types.Add(_void.Unwrap());
        }

        return _void.Unwrap();
    }

    private static Option<TypeSymbol> _bool;
    public static TypeSymbol Bool()
        {
            if (_bool.IsNone)
            {

                _bool = TypeSymbol.New("bool",
                                       Option.None,
                                       inheritanceClauseSyntax: null,
                                       methodTable: new MethodTable(),
                                       fieldTable: new FieldTable(),
                                       baseTypes: new SingleOccurenceList<TypeSymbol>(),
                                       isGenericMethodParameter: false,
                                       isGenericClassParameter: false,
                                       genericParameters: Option.None,
                                       genericParameterTypeConstraints: Option.None,
                                       isGenericTypeDefinition: false,
                                       BuiltInTypeSymbolNamespace);
                BuiltInTypeSymbolNamespace.Types.Add(_bool.Unwrap());
            }

            return _bool.Unwrap();
        }

    private static Option<TypeSymbol> _int;
        public static TypeSymbol Int()
        {
            if (_int.IsNone)
            {
                _int = TypeSymbol.New("int",
                                   Option.None,
                                   inheritanceClauseSyntax: null,
                                   methodTable: new MethodTable(),
                                   fieldTable: new FieldTable(),
                                   baseTypes: new SingleOccurenceList<TypeSymbol>(),
                                   isGenericMethodParameter: false,
                                   isGenericClassParameter: false,
                                   genericParameters: Option.None,
                                   genericParameterTypeConstraints: Option.None,
                                   isGenericTypeDefinition: false,
                                   BuiltInTypeSymbolNamespace);
                BuiltInTypeSymbolNamespace.Types.Add(_int.Unwrap());
            }

            return _int.Unwrap();
        }

        private static Option<TypeSymbol> _string;
        public static TypeSymbol String()
        {
            if (_string.IsNone)
            {
                _string = TypeSymbol.New("string",
                                         Option.None,
                                         inheritanceClauseSyntax: null,
                                         methodTable: new MethodTable(),
                                         fieldTable: new FieldTable(),
                                         baseTypes: new SingleOccurenceList<TypeSymbol>(),
                                         isGenericMethodParameter: false,
                                         isGenericClassParameter: false,
                                         genericParameters: Option.None,
                                         genericParameterTypeConstraints: Option.None,
                                         isGenericTypeDefinition: false,
                                         BuiltInTypeSymbolNamespace);
                BuiltInTypeSymbolNamespace.Types.Add(_string.Unwrap());
            }

            return _string.Unwrap();
        }

        private static Option<TypeSymbol> _object;
        public static TypeSymbol Object()
        {
            if (_object.IsNone)
            {
                _object = InitializeObject();
                BuiltInTypeSymbolNamespace.Types.Add(_object.Unwrap());
            }

            return _object.Unwrap();
        }

        public static readonly IEnumerable<TypeSymbol> All = new[] { Error(), Void(), Bool(), Int(), String(), Object() };
    

    static TypeSymbol InitializeObject()
    {
        var symbol = TypeSymbol.New("object", Option.None,
                       inheritanceClauseSyntax: null,
                       methodTable: new MethodTable(),
                       fieldTable: new FieldTable(), baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                       isGenericMethodParameter: false,
                       isGenericClassParameter: false, genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false,
                       BuiltInTypeSymbolNamespace);

        return symbol;
    }
}
    public static TypeSymbol FromLiteral(SyntaxToken literalToken)
    {
        if (literalToken.Kind is SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword)
            return BuiltIn.Bool();

        if (literalToken.Kind is SyntaxKind.NumberToken)
            return BuiltIn.Int();

        if (literalToken.Kind is SyntaxKind.StringToken)
            return BuiltIn.String();

        return BuiltIn.Error();
    }

    

    public static TypeSymbol FromNamedTypeExpression(NamedTypeExpressionSyntax namedTypeES, 
                                                     BoundScope lookupScope, DiagnosticBag diagnostics, NamespaceSymbol containingNamespace)
    {
        TypeSymbol ReportUndefinedAndReturnError(NamedTypeExpressionSyntax namedTypeExpressionLocal)
        {
            diagnostics.ReportUndefinedType(namedTypeExpressionLocal.Location, namedTypeExpressionLocal.GetName());
            return BuiltIn.Error();
        }
        
        if (namedTypeES.GenericClause.IsNone)
        {
            var foundType = lookupScope.TryLookupType(namedTypeES.GetName(), containingNamespace);
            if (foundType.IsSome && foundType.Unwrap().IsGenericType)
            {
                diagnostics.ReportGenericMethodGenericArgumentsNotSpecified(namedTypeES.Identifier);
                return foundType.Unwrap();
            }
            
            return foundType.SomeOr(() => ReportUndefinedAndReturnError(namedTypeES));
        }
        
        var genericTypeDefinition = lookupScope.TryLookupType(namedTypeES.GetName(), containingNamespace).Unwrap();
        genericTypeDefinition.IsGenericTypeDefinition.EnsureTrue();
        
        var genericArguments = namedTypeES.GenericClause.Unwrap().Arguments
            .Select(x => x.GenericClause.IsSome
                        ? FromNamedTypeExpression(x, lookupScope, diagnostics, containingNamespace)
                        : lookupScope.TryLookupType(x.GetName(), containingNamespace).SomeOr(() => ReportUndefinedAndReturnError(x)))
            .ToImmutableArray();

        
        if (genericArguments.Length is 0)
        {
            diagnostics.ReportGenericMethodGenericArgumentsNotSpecified(namedTypeES.Identifier);
            return BuiltIn.Error();
        }
        
        if (genericArguments.Length != genericTypeDefinition.GenericParameters.Unwrap().Length)
        {
            diagnostics.ReportGenericMethodCallWithWrongTypeArgumentsCount(namedTypeES.Identifier,
                                                                           namedTypeES.GenericClause.Unwrap(), 
                                                                           genericTypeDefinition.GenericParameters.Unwrap());
            return BuiltIn.Error();
        }
        
        CheckGenericConstraints(genericTypeDefinition.GenericParameters.Unwrap().ToList(), 
                                namedTypeES.GenericClause.Unwrap().Arguments.ToList(),
                                lookupScope,
                                diagnostics,
                                containingNamespace);
        
        var type = new TypeSymbol(
            namedTypeES.GetName(),
            Option.None, 
            Option.None,
            genericTypeDefinition.MethodTable /* TODO */,
            genericTypeDefinition.FieldTable  /* TODO */,
            new SingleOccurenceList<TypeSymbol>(),
            false, 
            false,
            genericArguments, 
            Option.None, 
            false,
            genericTypeDefinition, 
            containingNamespace);

        return type;
    }

    public static TypeSymbol New(string name, Option<SyntaxNode> declaration,
                                 InheritanceClauseSyntax? inheritanceClauseSyntax,
                                 MethodTable methodTable, FieldTable fieldTable,
                                 SingleOccurenceList<TypeSymbol> baseTypes,
                                 bool isGenericMethodParameter, bool isGenericClassParameter,
                                 Option<ImmutableArray<TypeSymbol>> genericParameters,
                                 Option<ImmutableArray<TypeSymbol>> genericParameterTypeConstraints,
                                 bool isGenericTypeDefinition, 
                                 NamespaceSymbol containingNamespace)
        => new(name, declaration, inheritanceClauseSyntax, methodTable: methodTable,
               fieldTable: fieldTable, 
               baseTypes: baseTypes, 
               isGenericMethodParameter: isGenericMethodParameter,
               isGenericClassParameter: isGenericClassParameter, 
               genericParameters: genericParameters,
               genericParameterTypeConstraints: genericParameterTypeConstraints,
               isGenericTypeDefinition: isGenericTypeDefinition, 
               genericTypeDefinition: Option.None, containingNamespace);

    TypeSymbol(string name, Option<SyntaxNode> declaration,
               Option<InheritanceClauseSyntax> inheritanceClauseSyntax,
               MethodTable methodTable,
               FieldTable fieldTable, SingleOccurenceList<TypeSymbol> baseTypes,
               bool isGenericMethodParameter, bool isGenericClassParameter,
               Option<ImmutableArray<TypeSymbol>> genericParameters,
               Option<ImmutableArray<TypeSymbol>> genericParameterTypeConstraints,
               bool isGenericTypeDefinition,
               Option<TypeSymbol> genericTypeDefinition, NamespaceSymbol containingNamespace)
        : base(declaration, name)
    {
        MethodTable = methodTable;
        FieldTable = fieldTable;
        InheritanceClauseSyntax = inheritanceClauseSyntax;
        BaseTypes = baseTypes;
        IsGenericMethodParameter = isGenericMethodParameter;
        GenericParameterTypeConstraints = genericParameterTypeConstraints;
        IsGenericTypeDefinition = isGenericTypeDefinition;
        GenericTypeDefinition = genericTypeDefinition;
        ContainingNamespace = containingNamespace;
        GenericParameters = genericParameters;
        IsGenericClassParameter = isGenericClassParameter;
    }

    public bool IsGenericType => GenericParameters.IsSome && GenericParameters.Unwrap().Any(); 
    public bool IsGenericMethodParameter { get; }
    public bool IsGenericClassParameter { get; }
    public bool IsGenericTypeDefinition { get; }
    public Option<TypeSymbol> GenericTypeDefinition { get; }
    public Option<ImmutableArray<TypeSymbol>> GenericParameterTypeConstraints { get; }
    public Option<ImmutableArray<TypeSymbol>> GenericParameters { get; }
    public MethodTable MethodTable { get; }
    public FieldTable FieldTable { get; }
    public SingleOccurenceList<TypeSymbol> BaseTypes { get; }
    public NamespaceSymbol ContainingNamespace { get; }

    public string GetFullName()
    {
        if (IsGenericClassParameter || IsGenericMethodParameter || Equals(ContainingNamespace, BuiltIn.BuiltInTypeSymbolNamespace))
        {
            return Name;
        }
        return ContainingNamespace.FullName + "." + Name;
    }
    
    public new Option<ClassDeclarationSyntax> DeclarationSyntax => base.DeclarationSyntax.IsSome 
        ? base.DeclarationSyntax.UnwrapAs<ClassDeclarationSyntax>() 
        : Option.None;
    public Option<InheritanceClauseSyntax> InheritanceClauseSyntax { get; }
    TypeSymbol ITypedSymbol.Type => this;

    public override SymbolKind Kind => SymbolKind.Type;

    public bool TryDeclareMethod(
        MethodSymbol method,
        DiagnosticBag diagnostics,
        DeclarationsBag allDeclarations)
    {
        var canBeDeclared = true;
        if (Name == method.Name)
        {
            canBeDeclared = false;
            diagnostics.ReportClassMemberCannotHaveNameOfClass(
                method.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier);
        }

        var declaredField = LookupField(method.Name);
        if (declaredField.IsSome)
        {
            canBeDeclared = false;
            var sameNameFields = FieldTable.Symbols
                .Where(f => f.Name == method.Name)
                .ToList();

            diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(
                method.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier);
            foreach (var sameNameField in sameNameFields)
            {
                var fieldDeclarations = allDeclarations.LookupDeclarations<FieldDeclarationSyntax>(sameNameField)
                    .Add(sameNameField.DeclarationSyntax.UnwrapAs<FieldDeclarationSyntax>());
                foreach (var fieldDeclaration in fieldDeclarations)
                    diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(fieldDeclaration.Identifier);
            }
        }

        var declared = LookupMethod(method.Name);
        if (declared.Count > 0)
        {
            var methodsFromBaseTypes = declared
                .Where(m => m.ContainingType.IsSome && m.ContainingType.Unwrap() != this)
                .ToList();
            
            // TODO: create warning for overloading, if method is not override
            // but declared in base class methods with same name is virtual warn about overload possibility
            if ((declared.All(x => x.IsVirtual) && method.IsOverriding) is false)
            {
                canBeDeclared = false;
                foreach (var methodFromBase in methodsFromBaseTypes)
                {
                    diagnostics.ReportMethodAlreadyDeclaredInBaseClass(method, methodFromBase.ContainingType.Unwrap());
                }

                var alreadyReportedMethodDeclarations = methodsFromBaseTypes
                    .Select(x => x.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>());
                var existingMethodDeclarations = allDeclarations.LookupDeclarations<MethodDeclarationSyntax>(method)
                    .Except(alreadyReportedMethodDeclarations)
                    .ToList();

                // if there is more than one declaration left after reporting the base class methods redeclaration,
                // then there is a redeclaration in the current class
                if (existingMethodDeclarations.Count > 1)
                {
                    foreach (var existingMethodDeclaration in existingMethodDeclarations)
                    {
                        diagnostics.ReportMethodAlreadyDeclared(existingMethodDeclaration.Identifier);
                    }
                }
            }
        }

        if (canBeDeclared)
        {
            MethodTable.AddMethodDeclaration(method, new List<TypeSymbol>());
        }

        return canBeDeclared;
    }

    public BoundBlockStatement LookupMethodBody(MethodSymbol methodSymbol)
    {
        var method = LookupMethodBodyNullIfNotFound(methodSymbol);
        if (method is null)
            throw new InvalidOperationException($"'{methodSymbol.Name}' method body not found.");

        return method;
    }

    BoundBlockStatement? LookupMethodBodyNullIfNotFound(MethodSymbol methodSymbol)
    {
        var sameName = MethodTable.Where(x => x.MethodSymbol.Name == methodSymbol.Name).ToList();
        foreach (var declaration in sameName)
        {
            if (declaration.MethodSymbol.ReturnType.Equals(methodSymbol.ReturnType)
                && declaration.MethodSymbol.Parameters.SequenceEqual(methodSymbol.Parameters))
            {
                return declaration.Body.Unwrap();
            }
        }
        
        var baseMethod = BaseTypes.Select(x => x.LookupMethodBodyNullIfNotFound(methodSymbol))
            .Exclude(x=> x is null)
            .SingleOrDefault();
        
        return baseMethod;
    }
    public List<MethodSymbol> LookupMethod(string name) => LookupMethodInternal(name, new List<TypeSymbol>());

    List<MethodSymbol> LookupMethodInternal(string name, List<TypeSymbol> typesChecked)
    {
        if (typesChecked.Contains(this))
            return new List<MethodSymbol>();
        typesChecked.Add(this);
        
        var result = new List<MethodSymbol>();
        var methods = MethodTable.Where(x => x.MethodSymbol.Name == name).ToList();
        result.AddRange(methods.Select(x => x.MethodSymbol));

        var baseTypesMethods = BaseTypes.Select(x => x.LookupMethodInternal(name, typesChecked))
            .SelectMany(x => x)
            .ToList();
        
        if (baseTypesMethods.Any())
            result.AddRange(baseTypesMethods);

        return result;
    }

    public enum TryDeclareFieldResult
    {
        TypeNameSameAsFieldName,
        FieldWithSameNameExists,
        MethodWithSameNameExists,
    }
    public List<TryDeclareFieldResult> TryDeclareField(FieldSymbol field)
    {
        var errors = new List<TryDeclareFieldResult>();
        var fieldShouldBeDeclared = true;
        
        var declaredField = LookupField(field.Name);
        if (declaredField.IsSome)
        {
            fieldShouldBeDeclared = false;
            errors.Add(TryDeclareFieldResult.FieldWithSameNameExists);
        }

        if (Name == field.Name)
            errors.Add(TryDeclareFieldResult.TypeNameSameAsFieldName);
        
        
        var declaredMethods = LookupMethod(field.Name);
        if (declaredMethods.Count > 0)
            errors.Add(TryDeclareFieldResult.MethodWithSameNameExists);
        


        if (fieldShouldBeDeclared)
            FieldTable.Add(field);   
        
        return errors;
    }

    public Option<FieldSymbol> LookupField(string fieldName) => LookupFieldInternal(fieldName, new List<TypeSymbol>());

    Option<FieldSymbol> LookupFieldInternal(string fieldName, List<TypeSymbol> typesChecked)
    {
        if (typesChecked.Contains(this))
            return null;
        typesChecked.Add(this);
        
        var baseTypeField = BaseTypes
            .Select(x => x.LookupFieldInternal(fieldName, typesChecked))
            .Exclude(x => x.IsNone)
            .SingleOrDefault();
        
        if (baseTypeField.IsSome)
            return baseTypeField;
        
        var field = FieldTable.Symbols.FirstOrNone(x => x.Name == fieldName);
        return field;
    }
    
    public bool IsSubClassOf(TypeSymbol other)
    {
        if (BaseTypes.Any(x => x.Equals(other)))
            return true;

        if (BaseTypes.Any(x => x.IsSubClassOf(other)))
            return true;

        return false;
    }

    public bool CanBeCastedTo(TypeSymbol other)
    {
        if (other.Equals(this))
            return true;
        
        if (other.IsSubClassOf(this))
            return true;

        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TypeSymbol)
            return false;
        var other = (TypeSymbol)obj;

        if (this.IsGenericType != other.IsGenericType)
            return false;

        if (this.IsGenericType)
        {
            if (this.GenericParameters.Unwrap().Length != other.GenericParameters.Unwrap().Length)
                return false;
            
            bool genericParametersAreEqual = this.GenericParameters.Unwrap().Zip(other.GenericParameters.Unwrap())
                .All(x => x.First.Equals(x.Second));
            if (!genericParametersAreEqual)
                return false;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() + GenericParameters.GetHashCode();
    }

    public override string ToString()
    {
        var genericClause = GenericParameters.IsSome
            ?"<" + string.Join(",", GenericParameters.Unwrap().Select(x => x.ToString())) + ">"
            : string.Empty;
        return $"{Name}{genericClause}";
    }

    public static void CheckGenericConstraints(List<TypeSymbol> classGenericParameters, 
                                               List<NamedTypeExpressionSyntax> genericArguments,
                                               BoundScope scope,
                                               DiagnosticBag diagnostics, 
                                               NamespaceSymbol containingNamespace)
    {
        foreach (var genericArg in genericArguments)
        {
            if (scope.TryLookupType(genericArg.GetName(), containingNamespace).IsNone)
            {
                diagnostics.ReportUndefinedType(genericArg.Location, genericArg.GetName());
            }
        }

        foreach (var i in 0..classGenericParameters.Count)
        {
            var genericParameter = classGenericParameters[i];
            var genericArgumentSyntax = genericArguments[i];

            var fromNamedTypeExpression = FromNamedTypeExpression(genericArgumentSyntax, scope, diagnostics, containingNamespace);
            var constraintTypesOption = genericParameter.GenericParameterTypeConstraints;
            if (constraintTypesOption.IsNone)
                continue;

            var constraintTypes = constraintTypesOption.Unwrap();
            foreach (var constraintType in constraintTypes)
            {
                if (fromNamedTypeExpression.CanBeCastedTo(constraintType))
                {
                    continue;
                }

                diagnostics.ReportGenericMethodCallWithWrongTypeArgument(
                    genericArgumentSyntax,
                    fromNamedTypeExpression,
                    constraintType);
            }
        }
    }
}