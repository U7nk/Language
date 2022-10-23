using System;
using System.Collections.Generic;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis;

/// <summary>
/// Function symbol -> Lowered function body
/// </summary>
public class MethodTable : Dictionary<FunctionSymbol, BoundBlockStatement?>
{
    public IEnumerable<FunctionSymbol> Symbols => Keys;
    public IEnumerable<BoundBlockStatement?> Bodies => Values;
    
    public void Declare(FunctionSymbol symbol, BoundBlockStatement? body)
    {
        if (ContainsKey(symbol))
        {
            if (this[symbol] is { })
            {
                throw new Exception($"Function {symbol.Name} is already declared");
            }

            this[symbol] = body;
        }
        else
        {
            Add(symbol, body);
        }
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