using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis;

internal class PhiFunctionReplacer : BoundTreeRewriter
{
      public Option<VariableSymbol> VariableSymbol { get; set; }
      public Option<BoundPhiFunctionExpression> PhiFunctionToReplace { get; set; }

      protected override BoundExpression RewritePhiFunctionExpression(BoundPhiFunctionExpression node)
      {
            if (VariableSymbol.IsNone)
                  throw new Exception(nameof(VariableSymbol) + " is not set");
            if (PhiFunctionToReplace.IsNone)
                  throw new Exception(nameof(PhiFunctionToReplace) + " is not set");
            if (!PhiFunctionToReplace.Equals(node))
                  return node;

            return new BoundVariableExpression(Option.None, VariableSymbol.Unwrap());
      }
}

static class BasicBlockExtensions
{
      
      public class TraverseController
      {
            public enum DirectionEnum
            {
                  Up,
                  Down
            }

            public void DirectionUp() => Direction = DirectionEnum.Up;
            public void DirectionDown() => Direction = DirectionEnum.Down;
            public void Stop() => StopTraversing = true;
            public DirectionEnum Direction { get; set; } = DirectionEnum.Down;
            
            public bool StopTraversing { get; set; }
            public bool TraverseChildrenOfCurrentBlock { get; set; } = true;
            public void DoNotTraverseChildrenOfCurrentBlock() => TraverseChildrenOfCurrentBlock = false;
      }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="block"></param>
      /// <param name="action">
      /// action to perform on block,
      /// returns false if you need to drop current branch,
      /// returns true if you need to continue traverse current branch. </param>
      /// <param name="traverseUp"></param>
      /// <param name="preventRecursion"></param>
      public static void TraverseBlocksBreadthFirst(this ControlFlowGraph.BasicBlock block, 
                                                    Action<ControlFlowGraph.BasicBlock, TraverseController> action,
                                                    bool preventRecursion = true)
      {

            var traverseControl = new TraverseController();
            var blocksToCheck = new Stack<ControlFlowGraph.BasicBlock>([ block ]);
            var checkedBlocks = new HashSet<ControlFlowGraph.BasicBlock>();

            while (!blocksToCheck.Empty())
            {
                  var currentBlock = blocksToCheck.Pop();
                  action(currentBlock, traverseControl);
                  if (traverseControl.StopTraversing)
                        break;
                  
                  if (!traverseControl.TraverseChildrenOfCurrentBlock)
                  {
                        traverseControl.TraverseChildrenOfCurrentBlock = true;
                        continue;
                  }
                  

                  if (preventRecursion)
                        checkedBlocks.Add(currentBlock);

                  var nextBlocks = traverseControl.Direction == TraverseController.DirectionEnum.Up
                        ? currentBlock.Incoming.Select(x => x.From)
                        : currentBlock.Outgoing.Select(x => x.To);
                  foreach (var basicBlock in nextBlocks)
                  {
                        if (!checkedBlocks.Contains(basicBlock))
                              blocksToCheck.Push(basicBlock);
                  }
            }
      }
}

class ControlFlowGraph
{
      internal class BasicBlock
      {
            public BasicBlock()
            {
            }

            public BasicBlock(bool isStart)
            {
                  IsStart = isStart;
                  IsEnd = !isStart;
            }

            public bool IsStart { get; }
            public bool IsEnd { get; }

            public List<BoundStatement> Statements { get; } = new();
            public List<BasicBlockBranch> Incoming { get; } = new();
            public List<BasicBlockBranch> Outgoing { get; } = new();

            public override string ToString()
            {
                  if (IsStart)
                        return "(START)";

                  if (IsEnd)
                        return "(END)";

                  var textWriter = new StringWriter();
                  foreach (var statement in Statements)
                        statement.WriteTo(textWriter);

                  return textWriter.ToString();
            }
      }

      internal class BasicBlockBranch
      {
            public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression? condition)
            {
                  From = from;
                  To = to;
                  Condition = condition;
            }

            public BasicBlock From { get; set; }
            public BasicBlock To { get; set; }
            public BoundExpression? Condition { get; set; }
      }

      public sealed class BasicBlockBuilder
      {
            readonly List<BasicBlock> _blocks = new();
            readonly List<BoundStatement> _statements = new();
            BasicBlock _current = new();

            public List<BasicBlock> Build(BoundBlockStatement block)
            {
                  foreach (var statement in block.Statements)
                  {
                        switch (statement.Kind)
                        {
                              case BoundNodeKind.LabelStatement:
                                    StartBlock();
                                    _statements.Add(statement);
                                    break;
                              case BoundNodeKind.GotoStatement:
                              case BoundNodeKind.ConditionalGotoStatement:
                              case BoundNodeKind.ReturnStatement:
                                    _statements.Add(statement);
                                    StartBlock();
                                    break;
                              case BoundNodeKind.ExpressionStatement:
                              case BoundNodeKind.VariableDeclarationStatement:
                              case BoundNodeKind.VariableDeclarationAssignmentStatement:
                                    _statements.Add(statement);
                                    break;
                              default:
                                    throw new Exception($"Unexpected statement {statement.Kind}");
                        }
                  }

                  EndBlock();
                  return _blocks.ToList();
            }

            public sealed class GraphBuilder
            {
                  readonly List<BasicBlockBranch> _branches = new();
                  readonly Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new();
                  readonly Dictionary<LabelSymbol, BasicBlock> _blockFromLabel = new();

                  BasicBlock _start = new(isStart: true);
                  readonly BasicBlock _end = new(isStart: false);

                  public ControlFlowGraph Build(List<BasicBlock> blocks)
                  {
                        if (!blocks.Any())
                              Connect(_start, _end, null);
                        else
                              Connect(_start, blocks[0], null);

                        foreach (var block in blocks)
                        {
                              foreach (var statement in block.Statements)
                              {
                                    _blockFromStatement.Add(statement, block);
                                    if (statement is BoundLabelStatement label)
                                          _blockFromLabel.Add(label.Label, block);
                              }
                        }

                        foreach (var i in 0..blocks.Count)
                        {
                              var block = blocks[i];
                              var next = i == blocks.Count - 1 ? _end : blocks[i + 1];
                              foreach (var statement in block.Statements)
                              {
                                    var isLast = block.Statements.Last() == statement;
                                    Walk(statement, block, next, isLast);
                              }
                        }

                        RESCAN:
                        foreach (var block in blocks.Where(x => x.Incoming.Empty()))
                        {
                              RemoveBlock(blocks, block);
                              goto RESCAN;
                        }

                        blocks.Insert(0, _start);
                        blocks.Add(_end);
                        return new ControlFlowGraph(_start, _end, blocks, _branches.ToList());
                  }

                  void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
                  {
                        foreach (var branch in block.Incoming)
                        {
                              branch.From.Outgoing.Remove(branch);
                              _branches.Remove(branch);
                        }

                        foreach (var branch in block.Outgoing)
                        {
                              branch.To.Incoming.Remove(branch);
                              _branches.Remove(branch);
                        }

                        blocks.Remove(block);
                  }

                  void Walk(BoundStatement statement, BasicBlock current, BasicBlock next, bool isLast)
                  {
                        switch (statement.Kind)
                        {
                              case BoundNodeKind.LabelStatement:
                                    if (isLast)
                                          Connect(current, next, null);
                                    break;
                              case BoundNodeKind.GotoStatement:
                                    var gotoStatement = (BoundGotoStatement)statement;
                                    var toBlock = _blockFromLabel[gotoStatement.Label];
                                    Connect(current, toBlock, null);
                                    break;
                              case BoundNodeKind.ConditionalGotoStatement:
                                    var conditionalGoto = (BoundConditionalGotoStatement)statement;
                                    var thenBlock = _blockFromLabel[conditionalGoto.OnTrueLabel];
                                    var elseBlock = _blockFromLabel[conditionalGoto.OnFalseLabel];
                                    var negatedCondition = BoundUnaryExpression.Negate(conditionalGoto.Condition);
                                    var thenCondition = conditionalGoto.Condition;
                                    var elseCondition = negatedCondition;

                                    Connect(current, thenBlock, thenCondition);
                                    Connect(current, elseBlock, elseCondition);
                                    break;
                              case BoundNodeKind.ReturnStatement:
                                    Connect(current, _end, null);
                                    break;
                              case BoundNodeKind.ExpressionStatement:
                              case BoundNodeKind.VariableDeclarationStatement:
                              case BoundNodeKind.VariableDeclarationAssignmentStatement:
                                    if (isLast)
                                          Connect(current, next, null);
                                    break;
                              default:
                                    throw new Exception($"Unexpected statement {statement.Kind}");
                        }
                  }

                  void Connect(BasicBlock from, BasicBlock to, BoundExpression? condition)
                  {
                        if (condition is BoundLiteralExpression literal)
                        {
                              var value = (bool)(literal.Value ?? throw new InvalidOperationException());
                              if (value)
                                    condition = null;
                              else
                                    return;
                        }

                        var branch = new BasicBlockBranch(from, to, condition);
                        from.Outgoing.Add(branch);
                        to.Incoming.Add(branch);
                        _branches.Add(branch);
                  }
            }

            void EndBlock()
            {
                  if (!_statements.Any())
                        return;

                  var block = new BasicBlock();
                  block.Statements.AddRange(_statements);
                  _blocks.Add(block);
                  _statements.Clear();
            }

            void StartBlock()
            {
                  EndBlock();
            }
      }

      public ControlFlowGraph(
            BasicBlock start, BasicBlock end,
            List<BasicBlock> blocks, List<BasicBlockBranch> branches)
      {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
      }

      public bool IsInSSAForm { get; private set; }
      public BasicBlock Start { get; }
      public BasicBlock End { get; }
      public List<BasicBlock> Blocks { get; }
      public List<BasicBlockBranch> Branches { get; }


      private int _number = 0;
      private int GetNextNumber() => _number++;


      private List<VariableSymbol> FindDeclarations(BasicBlock findFromBlock, 
                                                    BoundStatement findFromStatement, 
                                                    VariableSymbol variableBeforeSSA, 
                                                    Dictionary<VariableSymbol, 
                                                          List<VariableSymbol>> variableDeclarations)
      {
            List<VariableSymbol> result = [ ];
            bool first = true;
            findFromBlock.TraverseBlocksBreadthFirst(
                  (block, traverse) =>
                  {
                        traverse.DirectionUp();
                        
                        var i = block.Statements.Count - 1;
                        if (first)
                        {
                              var indexOf = block.Statements.IndexOf(findFromStatement);
                              i = indexOf - 1;
                              first = false;
                        }

                        for (; i >= 0 && i < block.Statements.Count; i--)
                        {
                              var statement = block.Statements[i];
                              if (statement is not BoundVariableDeclarationAssignmentStatement declarationStatement)
                                    continue;
                              var variableSSADeclarations = variableDeclarations[variableBeforeSSA];
                              if (!variableSSADeclarations.Contains(declarationStatement.Variable))
                                    continue;
                              result.Add(declarationStatement.Variable);
                              
                              traverse.DoNotTraverseChildrenOfCurrentBlock();
                              return;
                        }
                  },
                  preventRecursion: false);

            return result.Distinct().ToList();
      }

      private void ReplaceAssignmentsWithNewVariables(Dictionary<VariableSymbol, List<VariableSymbol>> variableDefinitions)
      {
            Start.TraverseBlocksBreadthFirst(((block, traverse) =>
            {
                  // add label statement to the beginning of the block if it is not there
                  if ((block.Statements.Count == 0 || block.Statements.First().Kind is not BoundNodeKind.LabelStatement)
                      && block != Start && block != End)
                  {
                        var label = new BoundLabelStatement(Option.None, LabelSymbol.GenerateLabel("block"));
                        block.Statements.Insert(0, label);
                  }


                  for (var i = 0; i < block.Statements.Count; i++)
                  {
                        var oldStatement = block.Statements[i];
                        if (oldStatement is BoundVariableDeclarationAssignmentStatement declaration)
                        {
                              if (variableDefinitions.TryGetValue(declaration.Variable, out var found))
                                    found.Add(declaration.Variable);
                              else
                                    variableDefinitions[declaration.Variable] = [ declaration.Variable ];
                              continue;
                        }

                        if (oldStatement.Kind is not BoundNodeKind.ExpressionStatement)
                              continue;
                        
                        var expression = oldStatement.As<BoundExpressionStatement>().Expression;
                        if (expression.Kind is not BoundNodeKind.AssignmentExpression)
                              continue;
                        
                        var assignmentExpression = expression.As<BoundAssignmentExpression>();
                        if (assignmentExpression.Left.Kind is not BoundNodeKind.VariableExpression)
                              continue;
                        
                        var variableExpression = assignmentExpression.Left.As<BoundVariableExpression>();
                        var variable = variableExpression.Variable;

                        var newVariable = new VariableSymbol(Option.None,
                                                             variable.Name + "_" + GetNumberForVariable(variableDefinitions, variable),
                                                             variable.Type,
                                                             true);

                        variableDefinitions[variable].Add(newVariable);
                        variableDefinitions[newVariable] = [ newVariable ];
                        var newStatement = new BoundVariableDeclarationAssignmentStatement(oldStatement.Syntax,
                                                                                           newVariable,
                                                                                           assignmentExpression.Initializer);
                        block.Statements.ReplaceFirst(oldStatement, newStatement);
                  }
            }));
      }

      private void AddGotoStatementToTheEndOfBlockIfNotExistsAlready()
      {
            // copilot explain this: we need to add goto statement to the end of the block if it is not there
            Start.TraverseBlocksBreadthFirst((block, traverse) =>
            {
                  if ((block.Statements.Count == 0
                       || block.Statements.Last().Kind is not BoundNodeKind.GotoStatement and not BoundNodeKind.ConditionalGotoStatement and not BoundNodeKind.ReturnStatement)
                      && block != Start && block != End
                      && block.Outgoing.Single().To != End)
                  {
                        var gotoStatement = new BoundGotoStatement(Option.None, block.Outgoing.Single().To.Statements.First().As<BoundLabelStatement>().Label);
                        block.Statements.Add(gotoStatement);
                  }
            });
      }

      /// <summary>
      /// transforming to ssa form, and adding label statements to the beginning of the block if it is not there,
      /// and adding goto statement to the end of the block if it is not there 
      /// </summary>
      public void TransformToSSA()
      {
            var variableDefinitions = new Dictionary<VariableSymbol, List<VariableSymbol>>();

            ReplaceAssignmentsWithNewVariables(variableDefinitions);

            AddGotoStatementToTheEndOfBlockIfNotExistsAlready();
            
            var retraverse = true;
            while (retraverse)
            {
                  retraverse = false;
                  Start.TraverseBlocksBreadthFirst((block, traverse) =>
                  {
                        var rewriter = new VariableUsagePhiInserter();

                        for (var i = 0; i < block.Statements.Count; i++)
                        {
                              var statement = block.Statements[i];
                              var children = statement.GetChildren(true);
                              var variableExpressions = children.Where(x => x.Kind is BoundNodeKind.VariableExpression).Cast<BoundVariableExpression>().ToList();
                              if (variableExpressions.Count == 0)
                                    continue;

                              var rewrittenStatement = statement;
                              foreach (var variableExpr in variableExpressions)
                              {
                                    rewriter.VariableExpressionToReplace = variableExpr;
                                    rewriter.VariableSymbols = FindDeclarations(block, statement, variableExpr.Variable, variableDefinitions);

                                    // if no declarations found and variable is parameter, then declaration is parameter and we just use it
                                    if (rewriter.VariableSymbols.Unwrap().Count == 0 && variableExpr.Variable.Kind == SymbolKind.Parameter)
                                          rewriter.VariableSymbols.Unwrap().Add(variableExpr.Variable);

                                    rewrittenStatement = rewriter.RewriteStatement(rewrittenStatement);
                              }

                              block.Statements.ReplaceFirst(statement, rewrittenStatement);
                        }

                        var phiReplacer = new PhiFunctionReplacer();
                        for (var i = 0; i < block.Statements.Count; i++)
                        {
                              var statement = block.Statements[i];
                              var statementPhiFunctions = statement.GetChildren(recursion: true).OfType<BoundPhiFunctionExpression>().ToList();
                              if (statementPhiFunctions.Empty())
                                    continue;
                              if (statement is BoundVariableDeclarationAssignmentStatement variableDeclaration 
                                  && variableDeclaration.Variable.Name.StartsWith("phi_"))
                              {
                                    continue;
                              }
                              
                              var rewrittenStatement = statement;
                              foreach (var phiFunction in statementPhiFunctions)
                              {
                                    
                                    var phiResultVariable = new VariableSymbol(Option.None, "phi_" + GetNextNumber(), phiFunction.Type, true);
                                    var phiResultVariableDeclaration = new BoundVariableDeclarationAssignmentStatement(
                                          Option.None,
                                          phiResultVariable,
                                          phiFunction
                                    );
                                    block.Statements.Insert(i, phiResultVariableDeclaration);
                                    i++;
                                    
                                    var beforeSSAVariable = FindBeforeSSAVariableSymbol(phiFunction.VariableSymbols, variableDefinitions);
                                    variableDefinitions[beforeSSAVariable.Unwrap()].Add(phiResultVariableDeclaration.Variable);
                                    variableDefinitions[phiResultVariable] = [ phiResultVariable ];
                                          
                                    phiReplacer.PhiFunctionToReplace = phiFunction;
                                    phiReplacer.VariableSymbol = phiResultVariable;
                                    rewrittenStatement = phiReplacer.RewriteStatement(rewrittenStatement);
                              }

                              
                              block.Statements.ReplaceFirst(statement, rewrittenStatement);
                              traverse.Stop();
                              retraverse = true;
                        }
                  });
            }


            foreach (var block in End.Incoming.Select(x => x.From))
            {
                  if (block.Statements.Last().Kind is not BoundNodeKind.ReturnStatement)
                  {
                        var returnStatement = new BoundReturnStatement(Option.None, Option.None);
                        block.Statements.Add(returnStatement);
                  }
            }

            IsInSSAForm = true;
      }

      private Option<VariableSymbol> FindBeforeSSAVariableSymbol(List<VariableSymbol> ssaDeclarations, Dictionary<VariableSymbol,List<VariableSymbol>> variableDefinitions)
      {
            foreach (var (beforeSSA, knownSSADeclarations) in variableDefinitions)
            {
                  foreach (var ssaDeclaration in ssaDeclarations)
                  {
                        if (knownSSADeclarations.Contains(ssaDeclaration))
                              return beforeSSA;
                  }
            }
            return Option.None;
      }

      
      
      private int GetNumberForVariable(Dictionary<VariableSymbol, List<VariableSymbol>> variableDefinitions, VariableSymbol variable)
      {
            if (variableDefinitions.TryGetValue(variable, out var found))
            {
                  return found.Count + 1;
            }

            variableDefinitions[variable] = new List<VariableSymbol>();
            return 0;
      }


      public void WriteTo(TextWriter writer)
      {
            writer.WriteLine("digraph G {");
            var blockIds = new Dictionary<BasicBlock, string>();
            foreach (var i in 0..Blocks.Count)
            {
                  var block = Blocks[i];
                  blockIds.Add(block, $"N{i}");
            }

            foreach (var block in Blocks)
            {
                  var id = blockIds[block];
                  var label = block.ToString()
                        .ReplaceLineEndings("\\l")
                        .Replace("\"", "\\\"");
                  writer.WriteLine($"  {id} [label=\"{label}\" shape = box]");
            }

            foreach (var branch in Branches)
            {
                  var fromId = blockIds[branch.From];
                  var toId = blockIds[branch.To];
                  var label = branch.Condition == null
                        ? "\"\""
                        : $"\"{branch.Condition}\"";


                  writer.WriteLine($"  {fromId} -> {toId} [label={label}] ");
            }

            writer.WriteLine("}");
      }

      public static ControlFlowGraph Create(BoundBlockStatement body)
      {
            var blockBuilder = new BasicBlockBuilder();
            var blocks = blockBuilder.Build(body);

            var graphBuilder = new BasicBlockBuilder.GraphBuilder();
            var graph = graphBuilder.Build(blocks);
            return graph;
      }

      public static bool AllPathsReturn(BoundBlockStatement body)
      {
            var graph = Create(body);

            return graph.End.Incoming.All(x => x.From.Statements.LastOrDefault() is BoundReturnStatement);
      }

      public static void AllVariablesInitializedBeforeUse(BoundBlockStatement body, DiagnosticBag diagnostics)
      {
            var graph = Create(body);
            var variablesUsagesBlocks = graph.Blocks.Where(x =>
                                                                 x.Statements
                                                                       .Any(s => s.GetChildren(recursion: true).Any(c => c.Kind is BoundNodeKind.VariableExpression)))
                  .ToList();

            foreach (var variablesUsagesBlock in variablesUsagesBlocks)
            {
                  var variableUseExpressions = variablesUsagesBlock.Statements.SelectMany(
                        bs => bs.GetChildren(recursion: true).Where(bn => bn.Kind is BoundNodeKind.VariableExpression)
                  ).ToList();
                  foreach (var variableUseExpression in variableUseExpressions.Cast<BoundVariableExpression>())
                  {
                        bool isInitialized = TraverseUpCheckVariableIsInitialized(
                              variablesUsagesBlock,
                              variableUseExpression);
                        if (!isInitialized)
                        {
                              var syntax = variableUseExpression.Syntax.UnwrapAs<NameExpressionSyntax>();
                              diagnostics.ReportCannotUseUninitializedVariable(syntax.Identifier);
                        }
                  }
            }
      }

      static bool TraverseUpCheckVariableIsInitialized(BasicBlock variablesUsagesBlock, BoundVariableExpression variableUseExpressions)
      {
            if (variableUseExpressions.Variable.Kind is SymbolKind.Parameter)
                  return true;


            foreach (var boundStatement in variablesUsagesBlock.Statements)
            {
                  var childrenFlatten = boundStatement.GetChildren(recursion: true).ToList();
                  foreach (var child in childrenFlatten)
                  {
                        if (child.Kind is BoundNodeKind.AssignmentExpression)
                        {
                              var assignmentExpression = (BoundAssignmentExpression)child;
                              if (assignmentExpression.Left.Kind is BoundNodeKind.VariableExpression)
                              {
                                    var variableExpression = (BoundVariableExpression)assignmentExpression.Left;
                                    if (Equals(variableExpression.Variable, variableUseExpressions.Variable))
                                          return true;
                              }
                        }
                        else if (child.Kind is BoundNodeKind.VariableDeclarationAssignmentStatement)
                        {
                              var variableDeclarationAssignmentStatement = (BoundVariableDeclarationAssignmentStatement)child;
                              if (Equals(variableDeclarationAssignmentStatement.Variable, variableUseExpressions.Variable))
                                    return true;
                        }
                  }
            }

            foreach (var incomingBlock in variablesUsagesBlock.Incoming.Select(x => x.From))
            {
                  if (TraverseUpCheckVariableIsInitialized(incomingBlock, variableUseExpressions))
                        return true;
            }

            return false;
      }
}