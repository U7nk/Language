using System;
using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis;

internal class VariableUsagePhiInserter : BoundTreeRewriter
{
      public Option<List<VariableSymbol>> VariableSymbols { get; set; }
      public Option<BoundVariableExpression> VariableExpressionToReplace { get; set; }


      protected override BoundExpression RewriteVariableExpression(BoundVariableExpression node)
      {
            if (VariableSymbols.IsNone)
                  throw new Exception(nameof(VariableSymbols) + " is not set");

            if (VariableExpressionToReplace.IsNone)
                  throw new Exception(nameof(VariableExpressionToReplace) + " is not set");

            if (!Equals(node, VariableExpressionToReplace.Unwrap()))
                  return node;

            if (VariableSymbols.Unwrap().Count == 1)
                  return new BoundVariableExpression(node.Syntax, VariableSymbols.Unwrap().Single());


            return new BoundPhiFunctionExpression(Option.None, VariableSymbols.Unwrap(), VariableSymbols.Unwrap().First().Type);
      }
}