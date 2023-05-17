using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Symbols;

static class BuiltInTypeSymbols
{
    public static readonly TypeSymbol Error = TypeSymbol.New("error", Option.None,
                                                             inheritanceClauseSyntax: null,
                                                             methodTable: new MethodTable(),
                                                             fieldTable: new FieldTable(),
                                                             baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                             isGenericMethodParameter: false,
                                                             isGenericClassParameter: false,
                                                            
                                                             genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false);

    public static readonly TypeSymbol Void = TypeSymbol.New("void", Option.None,
                                                            inheritanceClauseSyntax: null,
                                                            methodTable: new MethodTable(),
                                                            fieldTable: new FieldTable(), 
                                                            baseTypes: new SingleOccurenceList<TypeSymbol>(),
                                                            isGenericMethodParameter: false,
                                                            isGenericClassParameter: false,
                                                            genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false);

    public static readonly TypeSymbol Bool = TypeSymbol.New("bool", Option.None,
                                                            inheritanceClauseSyntax: null,
                                                            methodTable: new MethodTable(),
                                                            fieldTable: new FieldTable(),
                                                            baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                            isGenericMethodParameter: false,
                                                            isGenericClassParameter: false,
                                                            genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false);

    public static readonly TypeSymbol Int = TypeSymbol.New("int", Option.None,
                                                           inheritanceClauseSyntax: null,
                                                           methodTable: new MethodTable(),
                                                           fieldTable: new FieldTable(), 
                                                           baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                           isGenericMethodParameter: false,
                                                           isGenericClassParameter: false,
                                                           genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false);

    public static readonly TypeSymbol String = TypeSymbol.New("string", Option.None,
                                                              inheritanceClauseSyntax: null,
                                                              methodTable: new MethodTable(),
                                                              fieldTable: new FieldTable(),
                                                              baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                              isGenericMethodParameter: false,
                                                              isGenericClassParameter: false,
                                                              genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false);

    public static readonly TypeSymbol Object = InitializeObject();
    public static readonly IEnumerable<TypeSymbol> All = new[] { Error, Void, Bool, Int, String, Object };
    

    static TypeSymbol InitializeObject()
    {
        var symbol = TypeSymbol.New("object", Option.None,
                       inheritanceClauseSyntax: null,
                       methodTable: new MethodTable(),
                       fieldTable: new FieldTable(), baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                       isGenericMethodParameter: false,
                       isGenericClassParameter: false, genericParameters: Option.None, genericParameterTypeConstraints: Option.None, isGenericTypeDefinition: false);

        return symbol;
    }
}
public class TypeSymbol : Symbol, ITypedSymbol
{
    public static TypeSymbol FromLiteral(SyntaxToken literalToken)
    {
        if (literalToken.Kind is SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword)
            return BuiltInTypeSymbols.Bool;

        if (literalToken.Kind is SyntaxKind.NumberToken)
            return BuiltInTypeSymbols.Int;

        if (literalToken.Kind is SyntaxKind.StringToken)
            return BuiltInTypeSymbols.String;

        return BuiltInTypeSymbols.Error;
    }

    

    public static TypeSymbol FromNamedTypeExpression(NamedTypeExpressionSyntax namedTypeExpression, BoundScope lookupScope, DiagnosticBag diagnostics)
    {
        TypeSymbol ReportUndefinedAndReturnError(NamedTypeExpressionSyntax namedTypeExpressionLocal)
        {
            diagnostics.ReportUndefinedType(namedTypeExpressionLocal.Identifier.Location, namedTypeExpressionLocal.Identifier.Text);
            return BuiltInTypeSymbols.Error;
        }
        
        if (namedTypeExpression.GenericClause.IsNone)
        {
            return lookupScope.TryLookupType(namedTypeExpression.Identifier.Text, out var nonGenericType) 
                ? nonGenericType 
                : ReportUndefinedAndReturnError(namedTypeExpression);
        }
        
        lookupScope.TryLookupType(namedTypeExpression.Identifier.Text, out var genericTypeDefinition)
            .EnsureTrue();
        
        genericTypeDefinition.NullGuard().IsGenericTypeDefinition
            .EnsureTrue();
        
        var genericArguments = namedTypeExpression.GenericClause.Unwrap().Arguments
            .Select(x => x.GenericClause.IsSome
                        ? FromNamedTypeExpression(x, lookupScope, diagnostics)
                        : lookupScope.TryLookupType(x.Identifier.Text, out var type)
                            ? type
                            : ReportUndefinedAndReturnError(x))
            .ToImmutableArray();
        
        CheckGenericConstraints(genericTypeDefinition.GenericParameters.Unwrap().ToList(), namedTypeExpression.GenericClause.Unwrap().Arguments.ToList(), lookupScope, diagnostics);
        
        var type = new TypeSymbol(
            namedTypeExpression.Identifier.Text,
            Option.None, 
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
            genericTypeDefinition);

        return type;
    }

    public static TypeSymbol New(string name, Option<SyntaxNode> declaration,
                                 InheritanceClauseSyntax? inheritanceClauseSyntax,
                                 MethodTable methodTable, FieldTable fieldTable,
                                 SingleOccurenceList<TypeSymbol> baseTypes,
                                 bool isGenericMethodParameter, bool isGenericClassParameter,
                                 Option<ImmutableArray<TypeSymbol>> genericParameters,
                                 Option<ImmutableArray<TypeSymbol>> genericParameterTypeConstraints,
                                 bool isGenericTypeDefinition)
        => new(name, declaration, inheritanceClauseSyntax, containingType: null, methodTable: methodTable,
               fieldTable: fieldTable, 
               baseTypes: baseTypes, 
               isGenericMethodParameter: isGenericMethodParameter,
               isGenericClassParameter: isGenericClassParameter, 
               genericParameters: genericParameters,
               genericParameterTypeConstraints: genericParameterTypeConstraints,
               isGenericTypeDefinition: isGenericTypeDefinition, 
               genericTypeDefinition: Option.None);

    TypeSymbol(string name, Option<SyntaxNode> declaration,
               Option<InheritanceClauseSyntax> inheritanceClauseSyntax,
               Option<TypeSymbol> containingType, MethodTable methodTable,
               FieldTable fieldTable, SingleOccurenceList<TypeSymbol> baseTypes,
               bool isGenericMethodParameter, bool isGenericClassParameter,
               Option<ImmutableArray<TypeSymbol>> genericParameters,
               Option<ImmutableArray<TypeSymbol>> genericParameterTypeConstraints,
               bool isGenericTypeDefinition,
               Option<TypeSymbol> genericTypeDefinition)
        : base(declaration, name, containingType)
    {
        MethodTable = methodTable;
        FieldTable = fieldTable;
        InheritanceClauseSyntax = inheritanceClauseSyntax;
        BaseTypes = baseTypes;
        IsGenericMethodParameter = isGenericMethodParameter;
        GenericParameterTypeConstraints = genericParameterTypeConstraints;
        IsGenericTypeDefinition = isGenericTypeDefinition;
        GenericTypeDefinition = genericTypeDefinition;
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
        if (declaredField is { })
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
    public List<MethodSymbol> LookupMethod(string name) 
        => LookupMethodInternal(name, new List<TypeSymbol>());

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

    public bool TryDeclareField(FieldSymbol field)
    {
        if (Name == field.Name)
            return false;

        var declaredField = LookupField(field.Name);
        if (declaredField is { })
            return false;
        
        var declaredMethods = LookupMethod(field.Name);
        if (declaredMethods.Count > 0)
            return false;

        FieldTable.Add(field);
        return true;
    }

    public FieldSymbol? LookupField(string fieldName) 
        => LookupFieldInternal(fieldName, new List<TypeSymbol>());

    FieldSymbol? LookupFieldInternal(string fieldName, List<TypeSymbol> typesChecked)
    {
        if (typesChecked.Contains(this))
            return null;
        typesChecked.Add(this);
        
        var baseTypeField = BaseTypes
            .Select(x => x.LookupFieldInternal(fieldName, typesChecked))
            .Exclude(x => x is null)
            .SingleOrDefault();
        
        if (baseTypeField is not null)
            return baseTypeField;
        
        var field = FieldTable.Symbols.FirstOrDefault(x => x.Name == fieldName);
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

    public static void CheckGenericConstraints(List<TypeSymbol> genericParametersOfClass, 
                                               List<NamedTypeExpressionSyntax> genericArguments,
                                               BoundScope scope,
                                               DiagnosticBag diagnostics)
    {
        foreach (var genericArg in genericArguments)
        {
            if (!scope.TryLookupType(genericArg.Identifier.Text, out _))
            {
                diagnostics.ReportUndefinedType(genericArg.Location, genericArg.Identifier.Text);
            }
        }

        foreach (var i in 0..genericParametersOfClass.Count)
        {
            var genericParameter = genericParametersOfClass[i];
            var genericArgumentSyntax = genericArguments[i];

            var fromNamedTypeExpression = FromNamedTypeExpression(genericArgumentSyntax, scope, diagnostics);
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