using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Lowering;

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
        
        return new BoundBlockStatement(statement.Syntax, builder.ToImmutable());
    }

    

    protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        if (node.ElseStatement is null)
        {
            // rewrite structure
            // if <condition>
            //     <thenStatement>
            // -->
            // goto <condition> then, end
            // then:
            // <then>
            // end:
            var endLabel = LabelSymbol.GenerateLabel("end");
            var thenLabel = LabelSymbol.GenerateLabel("then");
            var conditionalGoto = new BoundConditionalGotoStatement(null, thenLabel, endLabel, node.Condition);
            var endLabelStatement = new BoundLabelStatement(null,endLabel);
            var thenLabelStatement = new BoundLabelStatement(null, thenLabel);
            var block = new BoundBlockStatement(
                null,
                [ conditionalGoto, thenLabelStatement, node.ThenStatement, endLabelStatement ]);
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
            // goto <condition> then, else
            // then:
            // <then>
            // goto end
            // else:
            // <else>
            // end:
            var thenLabel = LabelSymbol.GenerateLabel("then");
            var elseLabel = LabelSymbol.GenerateLabel("else");
            var endLabel = LabelSymbol.GenerateLabel("end");
            var conditionalGoto = new BoundConditionalGotoStatement(null, thenLabel, elseLabel, node.Condition);
            var gotoEnd = new BoundGotoStatement(null, endLabel);
            var thenLabelStatement = new BoundLabelStatement(null, thenLabel);
            var elseLabelStatement = new BoundLabelStatement(null, elseLabel);
            var endLabelStatement = new BoundLabelStatement(null, endLabel);
            var block = new BoundBlockStatement(null,
                ImmutableArray.Create(
                    conditionalGoto, 
                    thenLabelStatement,
                    node.ThenStatement,
                    gotoEnd,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement));
            return RewriteStatement(block);
        }
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        // rewrite structure
        // while <condition>
        //     <body>
        // 
        // ---- lowers to ----
        //
        //      goto loop_start
        // loop_body:
        //      <body>
        //      goto loop_start
        // loop_start:
        //      goto <condition> loop_body, loop_break
        // loop_break:
        
        var startLabel = LabelSymbol.GenerateLabel("loop_body");
        var startLabelStatement = new BoundLabelStatement(null, startLabel);
        
        var continueLabel = node.LoopStartLabel;
        var continueLabelStatement = new BoundLabelStatement(null, continueLabel);
        var gotoContinue = new BoundGotoStatement(null, continueLabel);
        var gotoContinue2 = new BoundGotoStatement(null, continueLabel);
        var breakLabelStatement = new BoundLabelStatement(null, node.LoopBreakLabel);
        var gotoStartOnTrue = new BoundConditionalGotoStatement(null, startLabel, node.LoopBreakLabel, node.Condition);
        
        var block = new BoundBlockStatement(
            null,
            ImmutableArray.Create(
                gotoContinue,
                startLabelStatement,
                node.Body,
                gotoContinue2,
                continueLabelStatement,
                gotoStartOnTrue,
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
        if (node.VariableDeclarationAssignment is not null)
            statement = new BoundVariableDeclarationAssignmentStatement(
                node.VariableDeclarationAssignment.Syntax,
                node.VariableDeclarationAssignment.Variable,
                node.VariableDeclarationAssignment.Initializer);
        else
            statement = new BoundExpressionStatement(node.Expression.NullGuard().Syntax, node.Expression.NullGuard());

        var condition = node.Condition.NullGuard();
         
        var mutation = new BoundExpressionStatement(node.Mutation.Syntax, node.Mutation.NullGuard());
        var body = new BoundBlockStatement(
            node.Body.Syntax, // i dont know if this is correct
            ImmutableArray.Create(node.Body, mutation));
        var whileStatement = new BoundWhileStatement(
            null,
            condition,
            body,
            node.LoopBreakLabel,
            node.LoopStartLabel);

        var result = new BoundBlockStatement(null, ImmutableArray.Create(statement, whileStatement));
        return RewriteStatement(result);
    }
}