using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis;

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
                    goto RESCAN; //-V3020
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
                        var thenBlock = _blockFromLabel[conditionalGoto.Label];
                        var elseBlock = next;
                        var negatedCondition = BoundUnaryExpression.Negate(conditionalGoto.Condition);
                        var thenCondition = conditionalGoto.JumpIfTrue
                            ? conditionalGoto.Condition
                            : negatedCondition;
                        var elseCondition = conditionalGoto.JumpIfTrue
                            ? negatedCondition
                            : conditionalGoto.Condition;
                        
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

    public BasicBlock Start { get; }
    public BasicBlock End { get; }
    public List<BasicBlock> Blocks { get; }
    public List<BasicBlockBranch> Branches { get; }


    public void WriteTo(TextWriter writer)
    {
        writer.WriteLine("digraph G {");
        var blockIds = new Dictionary<BasicBlock, string>();
        foreach(var i in 0..Blocks.Count)
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
                    var syntax = (NameExpressionSyntax)variableUseExpression.Syntax.NullGuard();
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
                if (child.Kind is BoundNodeKind.MemberAssignmentExpression)
                {
                    var assignmentExpression = (BoundMemberAssignmentExpression)child;
                    if (assignmentExpression.MemberAccess.Kind is BoundNodeKind.VariableExpression)
                    {
                        var variableExpression = (BoundVariableExpression)assignmentExpression.MemberAccess;
                        if (Equals(variableExpression.Variable, variableUseExpressions.Variable))
                            return true;
                    }
                }
                else if (child.Kind is BoundNodeKind.AssignmentExpression)
                {
                    var assignmentExpression = (BoundAssignmentExpression)child;
                    if (Equals(assignmentExpression.Variable, variableUseExpressions.Variable))
                        return true;
                }
                else if (child.Kind is BoundNodeKind.VariableDeclarationAssignmentStatement)
                {
                    var variableDeclarationAssignmentStatement = (BoundVariableDeclarationAssignmentStatement)child;
                    if (Equals(variableDeclarationAssignmentStatement.Variable, variableUseExpressions.Variable))
                        return true;
                }
            }
        }

        foreach (var incomingBlock in variablesUsagesBlock.Incoming.Select(x=> x.From))
        {
            if (TraverseUpCheckVariableIsInitialized(incomingBlock, variableUseExpressions))
                return true;
        }

        return false;
    }
}