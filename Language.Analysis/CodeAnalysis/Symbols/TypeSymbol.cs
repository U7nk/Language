using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class TypeSymbol : Symbol, ITypedSymbol
{
    public static readonly TypeSymbol Error = new("error", ImmutableArray<SyntaxNode>.Empty,
                                                  inheritanceClauseSyntax: null,
                                                  containingType: null,
                                                  new MethodTable(),
                                                  new FieldTable());

    public static readonly TypeSymbol Any = new("any",
                                                ImmutableArray<SyntaxNode>.Empty,
                                                inheritanceClauseSyntax: null,
                                                containingType: null,
                                                new MethodTable(),
                                                new FieldTable());

    public static readonly TypeSymbol Void = new("void", ImmutableArray<SyntaxNode>.Empty,
                                                 inheritanceClauseSyntax: null,
                                                 containingType: null,
                                                 new MethodTable(),
                                                 new FieldTable());

    public static readonly TypeSymbol Bool = new("bool", ImmutableArray<SyntaxNode>.Empty,
                                                 inheritanceClauseSyntax: null,
                                                 containingType: null,
                                                 new MethodTable(),
                                                 new FieldTable());

    public static readonly TypeSymbol Int = new("int", ImmutableArray<SyntaxNode>.Empty,
                                                inheritanceClauseSyntax: null,
                                                containingType: null,
                                                new MethodTable(),
                                                new FieldTable());

    public static readonly TypeSymbol String = new("string", ImmutableArray<SyntaxNode>.Empty,
                                                   inheritanceClauseSyntax: null,
                                                   containingType: null,
                                                   new MethodTable(),
                                                   new FieldTable());

    public static TypeSymbol FromLiteral(SyntaxToken literalToken)
    {
        if (literalToken.Kind is SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword)
            return Bool;

        if (literalToken.Kind is SyntaxKind.NumberToken)
            return Int;

        if (literalToken.Kind is SyntaxKind.StringToken)
            return String;

        return Error;
    }

    public static TypeSymbol New(string name, ImmutableArray<SyntaxNode> declaration,
                                 InheritanceClauseSyntax? inheritanceClauseSyntax,
                                 MethodTable methodTable, FieldTable fieldTable)
        => new(name, declaration, inheritanceClauseSyntax, containingType: null, methodTable, fieldTable);

    TypeSymbol(string name, ImmutableArray<SyntaxNode> declaration,
               InheritanceClauseSyntax? inheritanceClauseSyntax,
               TypeSymbol? containingType, MethodTable methodTable,
               FieldTable fieldTable)
        : base(declaration, name, containingType)
    {
        MethodTable = methodTable;
        FieldTable = fieldTable;
        InheritanceClauseSyntax = inheritanceClauseSyntax;
    }

    public MethodTable MethodTable { get; }
    public FieldTable FieldTable { get; }

    public TypeSymbol? BaseType
    {
        get => _baseType;
        internal set
        {
            if (_baseType is null)
                _baseType = value;
            else
                throw new InvalidOperationException("Base type is already set.");
        }
    }

    public InheritanceClauseSyntax? InheritanceClauseSyntax { get; }
    TypeSymbol ITypedSymbol.Type => this;
    TypeSymbol? _baseType;

    public override SymbolKind Kind => SymbolKind.Type;

    public bool TryDeclareMethod(MethodSymbol method)
    {
        if (Name == method.Name)
            return false;

        var declaredField = LookupField(method.Name);
        if (declaredField is { })
            return false;
        
        var declared = LookupMethod(method.Name);
        if (declared.Count > 0)
            return false;
        
        MethodTable.Add(method, null);
        return true;
    }

    public BoundBlockStatement LookupMethodBody(MethodSymbol methodSymbol)
    {
        var sameName = MethodTable.Where(x => x.Key.Name == methodSymbol.Name).ToList();
        foreach (var (method, body) in sameName)
        {
            if (method.ReturnType == methodSymbol.ReturnType && method.Parameters.SequenceEqual(methodSymbol.Parameters))
                return body.NG();
        }
        
        var baseMethod = BaseType?.LookupMethodBody(methodSymbol);
        if (baseMethod is { })
            return baseMethod;

        throw new InvalidOperationException($"'{methodSymbol.Name}' method body not found.");
    }
    public List<MethodSymbol> LookupMethod(string name)
    {
        var result = new List<MethodSymbol>();
        var methods = MethodTable.Symbols.Where(x => x.Name == name).ToList();
        result.AddRange(methods);
        
        var baseTypesMethods = BaseType?.LookupMethod(name);
        if (baseTypesMethods is { })
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
    {
        var baseTypeField = BaseType?.LookupField(fieldName);
        if (baseTypeField is not null)
            return baseTypeField;
        
        var field = FieldTable.Symbols.FirstOrDefault(x => x.Name == fieldName);
        if (field is not null)
            return field;

        return null;
    }

    public bool IsSubClassOf(TypeSymbol other)
    {
        if (BaseType is null)
            return false;

        if (BaseType == other)
            return true;
        
        return BaseType.IsSubClassOf(other);
    }
}