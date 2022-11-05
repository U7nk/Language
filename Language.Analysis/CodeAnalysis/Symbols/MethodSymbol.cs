using System;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class MethodSymbol : MemberSymbol
{
    public MethodSymbol(ImmutableArray<SyntaxNode> declaration, TypeSymbol? containingType, bool isStatic,
                        string name, ImmutableArray<ParameterSymbol> parameters,
                        TypeSymbol returnType) 
        : base(declaration, name,containingType, returnType)
    {
        Parameters = parameters;
        IsStatic = isStatic;
    }
    public bool IsStatic { get; } 
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType => Type;
    public override SymbolKind Kind => SymbolKind.Method;
}