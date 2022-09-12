using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding;

namespace Wired.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private int labelCount;

    private Lowerer()
    {
    }

    public static BoundBlockStatement Lower(BoundStatement statement)
    {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(result);
    }

    private static BoundBlockStatement Flatten(BoundStatement statement)
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
    
    private LabelSymbol GenerateLabel() 
        => new("Label" + this.labelCount++);

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
            var label = this.GenerateLabel();
            var conditionalGoto = new BoundConditionalGotoStatement(label, node.Condition, false);
            var endLabelStatement = new BoundLabelStatement(label);
            var block = new BoundBlockStatement(
                ImmutableArray.Create(conditionalGoto, node.ThenStatement, endLabelStatement));
            return this.RewriteStatement(block);
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
            var elseLabel = this.GenerateLabel();
            var endLabel = this.GenerateLabel();
            var conditionalGoto = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
            var gotoEnd = new BoundGotoStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var block = new BoundBlockStatement(
                ImmutableArray.Create(
                    conditionalGoto, node.ThenStatement,
                    gotoEnd, elseLabelStatement,
                    node.ElseStatement, endLabelStatement));
            return this.RewriteStatement(block);
        }
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        // rewrite structure
        // while <condition>
        //     <body>
        // -->
        // goto start
        // continue:
        // <body>
        // start:
        // gotoIfTrue <condition> continue
        
        var startLabel = this.GenerateLabel();
        var gotoStart = new BoundGotoStatement(startLabel);
        var continueLabel = this.GenerateLabel();
        var continueLabelStatement = new BoundLabelStatement(continueLabel);
        
        var startLabelStatement = new BoundLabelStatement(startLabel);
        var conditionalGoto = new BoundConditionalGotoStatement(continueLabel, node.Condition, true);
        var block = new BoundBlockStatement(
            ImmutableArray.Create(
                gotoStart, continueLabelStatement,
                node.Body, startLabelStatement,
                conditionalGoto));
        
        return this.RewriteStatement(block);
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
        var mutation = new BoundExpressionStatement(node.Mutation.ThrowIfNull());
        var body = new BoundBlockStatement(ImmutableArray.Create(node.Body, mutation));
        var whileStatement = new BoundWhileStatement(
            condition,
            body);

        var result = new BoundBlockStatement(ImmutableArray.Create(statement, whileStatement));
        return this.RewriteStatement(result);
    }
}