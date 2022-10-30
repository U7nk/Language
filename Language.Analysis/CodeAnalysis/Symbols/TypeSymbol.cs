using System.Collections.Generic;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class TypeSymbol : Symbol
{
    public static readonly TypeSymbol Error = new("error", ImmutableArray<SyntaxNode>.Empty, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Any = new("any", ImmutableArray<SyntaxNode>.Empty, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Void = new("void", ImmutableArray<SyntaxNode>.Empty, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Bool = new("bool", ImmutableArray<SyntaxNode>.Empty, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Int = new("int", ImmutableArray<SyntaxNode>.Empty, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol String = new("string", ImmutableArray<SyntaxNode>.Empty, new MethodTable(), new FieldTable());
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
        MethodTable methodTable, FieldTable fieldTable) 
        => new(name, declaration, methodTable, fieldTable);

    TypeSymbol(string name, ImmutableArray<SyntaxNode> declaration, MethodTable methodTable, FieldTable fieldTable) : base(declaration, name)
    {
        MethodTable = methodTable;
        FieldTable = fieldTable;
    }

    public MethodTable MethodTable { get; }
    public FieldTable FieldTable { get; }

    public override SymbolKind Kind => SymbolKind.Type;
}