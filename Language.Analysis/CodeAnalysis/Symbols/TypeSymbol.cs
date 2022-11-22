using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

static class BuiltInTypeSymbols
{
    public static readonly TypeSymbol Error = TypeSymbol.New("error", Option<SyntaxNode>.None,
                                                             inheritanceClauseSyntax: null,
                                                             new MethodTable(),
                                                             new FieldTable());

    public static readonly TypeSymbol Void = TypeSymbol.New("void", Option<SyntaxNode>.None,
                                                            inheritanceClauseSyntax: null,
                                                            new MethodTable(),
                                                            new FieldTable());

    public static readonly TypeSymbol Bool = TypeSymbol.New("bool", Option<SyntaxNode>.None,
                                                            inheritanceClauseSyntax: null,
                                                            new MethodTable(),
                                                            new FieldTable());

    public static readonly TypeSymbol Int = TypeSymbol.New("int", Option<SyntaxNode>.None,
                                                           inheritanceClauseSyntax: null,
                                                           new MethodTable(),
                                                           new FieldTable());

    public static readonly TypeSymbol String = TypeSymbol.New("string", Option<SyntaxNode>.None,
                                                              inheritanceClauseSyntax: null,
                                                              new MethodTable(),
                                                              new FieldTable());

    public static readonly TypeSymbol Object = InitializeObject();
    public static readonly IEnumerable<TypeSymbol> All = new[] { Error, Void, Bool, Int, String, Object };
    

    private static TypeSymbol InitializeObject()
    {
        var symbol = TypeSymbol.New("object", Option<SyntaxNode>.None,
                       inheritanceClauseSyntax: null,
                       new MethodTable(),
                       new FieldTable());

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
                                 MethodTable methodTable, FieldTable fieldTable)
        => new(name, declaration, inheritanceClauseSyntax, containingType: null, methodTable, fieldTable);

    TypeSymbol(string name, Option<SyntaxNode> declaration,
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