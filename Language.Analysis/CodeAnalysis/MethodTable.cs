using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis;

public class MethodDeclaration
{
    public MethodDeclaration(MethodSymbol methodSymbol, List<TypeSymbol> genericArgumentsTypeSymbols, Option<BoundBlockStatement> body)
    {
        MethodSymbol = methodSymbol;
        GenericArgumentsTypeSymbols = genericArgumentsTypeSymbols;
        Body = body;
    }

    public MethodSymbol MethodSymbol { get; init; }
    public List<TypeSymbol> GenericArgumentsTypeSymbols { get; init; }
    public Option<BoundBlockStatement> Body { get; set; }
    
    
}

/// <summary>
/// Function symbol -> Lowered function body
/// </summary>
public class MethodTable : IEnumerable<MethodDeclaration>
{
    readonly List<MethodDeclaration> _methodDeclarations = new();
    public void SetMethodBody(MethodSymbol symbol, BoundBlockStatement? body)
    {
        var declaration = _methodDeclarations.FirstOrNone(x => x.MethodSymbol.Equals(symbol));
        if (declaration.IsSome)
        {
            declaration.Unwrap().Body = body;
        }
        else
        {
            throw new Exception($"Method {symbol.Name} is not declared");
        }
    }
    
    public void AddMethodDeclaration(MethodSymbol symbol, List<TypeSymbol> genericArgumentsTypeSymbols)
    {
        if (_methodDeclarations.Any(x => x.MethodSymbol.Equals(symbol)))
        {
            throw new Exception($"Method {symbol.Name} is already declared");
        }
        
        _methodDeclarations.Add(new MethodDeclaration(symbol, genericArgumentsTypeSymbols, body: Option.None));
    }

    public IEnumerator<MethodDeclaration> GetEnumerator()
    {
        return _methodDeclarations.GetEnumerator(); 
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


public class FieldTable : List<FieldSymbol>
{
    public IEnumerable<FieldSymbol> Symbols => this;
    public void Declare(FieldSymbol symbol)
    {
        if (this.Contains(symbol))
        {
            throw new Exception($"Field {symbol.Name} is already declared");
        }
        
        Add(symbol);
        
    }
}