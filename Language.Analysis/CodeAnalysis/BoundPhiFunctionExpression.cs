using System;
using System.Collections.Generic;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis;

public class BoundPhiFunctionExpression : BoundExpression
{
      public BoundPhiFunctionExpression(Option<SyntaxNode> syntax, List<VariableSymbol> variableSymbols, TypeSymbol type) : base(syntax)
      {
            VariableSymbols = variableSymbols;
            Type = type;
            if (variableSymbols.Count == 0)
                  throw new ArgumentException("Value cannot be an empty collection.", nameof(variableSymbols));

            variableSymbols.ForEach(x =>
            {
                  if (!Equals(x.Type, type))
                        throw new ArgumentException("All variables should have the same type as the phi function", nameof(variableSymbols));
            });
      }

      internal List<VariableSymbol> VariableSymbols { get; }
      internal override BoundNodeKind Kind => BoundNodeKind.PhiFunctionExpression;

      internal override TypeSymbol Type { get; }
}