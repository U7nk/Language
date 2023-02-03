using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
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
                                                             isGenericMethodParameter: false, genericParameterTypeConstraints: Option.None);

    public static readonly TypeSymbol Void = TypeSymbol.New("void", Option.None,
                                                            inheritanceClauseSyntax: null,
                                                            methodTable: new MethodTable(),
                                                            fieldTable: new FieldTable(), 
                                                            baseTypes: new SingleOccurenceList<TypeSymbol>(),
                                                            isGenericMethodParameter: false, genericParameterTypeConstraints: Option.None);

    public static readonly TypeSymbol Bool = TypeSymbol.New("bool", Option.None,
                                                            inheritanceClauseSyntax: null,
                                                            methodTable: new MethodTable(),
                                                            fieldTable: new FieldTable(),
                                                            baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                            isGenericMethodParameter: false, genericParameterTypeConstraints: Option.None);

    public static readonly TypeSymbol Int = TypeSymbol.New("int", Option.None,
                                                           inheritanceClauseSyntax: null,
                                                           methodTable: new MethodTable(),
                                                           fieldTable: new FieldTable(), 
                                                           baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                           isGenericMethodParameter: false, genericParameterTypeConstraints: Option.None);

    public static readonly TypeSymbol String = TypeSymbol.New("string", Option.None,
                                                              inheritanceClauseSyntax: null,
                                                              methodTable: new MethodTable(),
                                                              fieldTable: new FieldTable(),
                                                              baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                                              isGenericMethodParameter: false, genericParameterTypeConstraints: Option.None);

    public static readonly TypeSymbol Object = InitializeObject();
    public static readonly IEnumerable<TypeSymbol> All = new[] { Error, Void, Bool, Int, String, Object };
    

    static TypeSymbol InitializeObject()
    {
        var symbol = TypeSymbol.New("object", Option.None,
                       inheritanceClauseSyntax: null,
                       methodTable: new MethodTable(),
                       fieldTable: new FieldTable(), baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                       isGenericMethodParameter: false, genericParameterTypeConstraints: Option.None);

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

    public static TypeSymbol New(string name, Option<SyntaxNode> declaration,
                                 InheritanceClauseSyntax? inheritanceClauseSyntax,
                                 MethodTable methodTable, FieldTable fieldTable,
                                 SingleOccurenceList<TypeSymbol> baseTypes, bool isGenericMethodParameter, Option<ImmutableArray<TypeSymbol>> genericParameterTypeConstraints)
        => new(name, declaration, inheritanceClauseSyntax, containingType: null, methodTable, fieldTable, baseTypes, isGenericMethodParameter, genericParameterTypeConstraints);

    TypeSymbol(string name, Option<SyntaxNode> declaration,
               InheritanceClauseSyntax? inheritanceClauseSyntax,
               TypeSymbol? containingType, MethodTable methodTable,
               FieldTable fieldTable, SingleOccurenceList<TypeSymbol> baseTypes, bool isGenericMethodParameter, Option<ImmutableArray<TypeSymbol>> genericParameterTypeConstraints)
        : base(declaration, name, containingType)
    {
        MethodTable = methodTable;
        FieldTable = fieldTable;
        InheritanceClauseSyntax = inheritanceClauseSyntax;
        BaseTypes = baseTypes;
        IsGenericMethodParameter = isGenericMethodParameter;
        GenericParameterTypeConstraints = genericParameterTypeConstraints;
    }

    public bool IsGenericMethodParameter { get; }
    public Option<ImmutableArray<TypeSymbol>> GenericParameterTypeConstraints { get; }
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
        MethodSignatureBinderLookup lookup)
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
                var fieldDeclarations = lookup.LookupDeclarations<FieldDeclarationSyntax>(sameNameField)
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
                var existingMethodDeclarations = lookup.LookupDeclarations<MethodDeclarationSyntax>(method)
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

        return true;
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
}