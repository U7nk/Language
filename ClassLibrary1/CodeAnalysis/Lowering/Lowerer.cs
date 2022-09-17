using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding;

namespace Wired.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    int _labelCount;

    Lowerer()
    {
    }

    public static BoundBlockStatement Lower(BoundStatement statement)
    {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(result);
    }

    static BoundBlockStatement Flatten(BoundStatement statement)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        var stack = new Stack<BoundStatement>();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current is BoundBlockStatement block)
                foreach (var boundNode in block.Statements.Reverse())
                    stack.Push(boundNode);
            else
                builder.Add(current);
        }

        return new BoundBlockStatement(builder.ToImmutable());
    }

    LabelSymbol GenerateLabel() 
        => new("Label" + _labelCount++);

    protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        if (node.ElseStatement is null)
        {
            // rewrite structure
            // if <condition>
            //     <thenStatement>
            // -->
            // gotoIfFalse <condition> end
            // <then>
            // end:
            var label = GenerateLabel();
            var conditionalGoto = new BoundConditionalGotoStatement(label, node.Condition, false);
            var endLabelStatement = new BoundLabelStatement(label);
            var block = new BoundBlockStatement(
                ImmutableArray.Create(conditionalGoto, node.ThenStatement, endLabelStatement));
            return RewriteStatement(block);
        }
        else
        {
            // rewrite structure
            // if <condition>
            //     <thenStatement>
            // else
            //     <elseStatement>
            // -->
            // gotoIfFalse <condition> else
            // <then>
            // goto end
            // else:
            // <else>
            // end:
            var elseLabel = GenerateLabel();
            var endLabel = GenerateLabel();
            var conditionalGoto = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
            var gotoEnd = new BoundGotoStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var block = new BoundBlockStatement(
                ImmutableArray.Create(
                    conditionalGoto, node.ThenStatement,
                    gotoEnd, elseLabelStatement,
                    node.ElseStatement, endLabelStatement));
            return RewriteStatement(block);
        }
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        // rewrite structure
        // while <condition>
        //     <body>
        // 
        // lowers to:
        //
        //      goto start
        // continue:
        //      <body>
        // start:
        //      gotoIfTrue <condition> continue
        // break:
        
        var startLabel = GenerateLabel();
        var gotoStart = new BoundGotoStatement(startLabel);
        var continueLabel = node.ContinueLabel;
        var continueLabelStatement = new BoundLabelStatement(continueLabel);
        var startLabelStatement = new BoundLabelStatement(startLabel);
        var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition, true);
        var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

        var block = new BoundBlockStatement(
            ImmutableArray.Create(
                gotoStart,
                continueLabelStatement,
                node.Body,
                startLabelStatement,
                gotoTrue,
                breakLabelStatement));
        
        return RewriteStatement(block);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement node)
    {
        // rewrite structure:
        // for (<declaration> | <expression>; <condition>; <mutation>)
        //     <body>
        // 
        // --->
        //
        // {
        //      <declaration> | <expression>;
        //      while <condition>
        //      {
        //          <body>
        //          <mutation>
        //      }
        // }

        BoundStatement? statement = null;
        if (node.VariableDeclaration is not null)
            statement = new BoundVariableDeclarationStatement(node.VariableDeclaration.Variable,
                node.VariableDeclaration.Initializer);
        else
            statement = new BoundExpressionStatement(node.Expression.ThrowIfNull());

        var condition = node.Condition.ThrowIfNull();
        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel); 
        var mutation = new BoundExpressionStatement(node.Mutation.ThrowIfNull());
        var body = new BoundBlockStatement(ImmutableArray.Create(
            node.Body,
            continueLabelStatement,
            mutation));
        var whileStatement = new BoundWhileStatement(
            condition,
            body,
            node.BreakLabel,
            new LabelSymbol("unused_need_to_refactor_for_loop_lowering"));

        var result = new BoundBlockStatement(ImmutableArray.Create(statement, whileStatement));
        return RewriteStatement(result);
    }
}