using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Binding.Binders;

namespace Language.Analysis.CodeAnalysis;

internal abstract class BoundTreeRewriter
{
    public virtual BoundExpression RewriteExpression(BoundExpression node)
    {
        return node.Kind switch
        {
            BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression)node),
            BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.MethodCallExpression => RewriteCallExpression((BoundMethodCallExpression)node),
            BoundNodeKind.ConversionExpression => RewriteConversionExpression((BoundConversionExpression)node),
            BoundNodeKind.ThisExpression => RewriteThisExpression((BoundThisExpression)node),
            BoundNodeKind.ObjectCreationExpression => RewriteObjectCreationExpression((BoundObjectCreationExpression)node),
            BoundNodeKind.MemberAccessExpression => RewriteMemberAccessExpression((BoundMemberAccessExpression)node),
            BoundNodeKind.MemberAssignmentExpression => RewriteMemberAssignmentExpression((BoundMemberAssignmentExpression)node),
            BoundNodeKind.FieldExpression => RewriteFieldAccessExpression((BoundFieldExpression)node),
            BoundNodeKind.ErrorExpression => RewriteErrorExpression((BoundErrorExpression)node),
            _ => throw new("Unexpected node " + node.Kind)
        };
    }

    protected virtual BoundExpression RewriteFieldAccessExpression(BoundFieldExpression node)
    {
        return node;
    }
    
    protected virtual BoundExpression RewriteMemberAssignmentExpression(BoundMemberAssignmentExpression node)
    {
        var member = RewriteExpression(node.MemberAccess);
        var rightValue = RewriteExpression(node.RightValue);
        if (member == node.MemberAccess && rightValue == node.RightValue)
            return node;
        
        return new BoundMemberAssignmentExpression(node.Syntax, member, rightValue);
    }

    protected virtual BoundExpression RewriteMemberAccessExpression(BoundMemberAccessExpression node)
    {
        var left = RewriteExpression(node.Left);
        if (left == node.Left)
            return node;
        
        return new BoundMemberAccessExpression(node.Syntax, node.Left, node.Member);
    }

    protected virtual BoundExpression RewriteObjectCreationExpression(BoundObjectCreationExpression node)
    {
        // node.Syntax: Rewrite arguments
        return node;
    }

    protected virtual BoundExpression RewriteThisExpression(BoundThisExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
    {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;
        
        return new BoundConversionExpression(node.Syntax, node.Type, expression);
    }

    protected virtual BoundExpression RewriteCallExpression(BoundMethodCallExpression node)
    {
        var statements = ImmutableArray.CreateBuilder<BoundExpression>();
        var changed = false;
        foreach (var statement in node.Arguments)
        {
            var rewritten = RewriteExpression(statement);
            changed |= rewritten != statement;
            statements.Add(rewritten);
        }
        
        if (!changed)
            return node;
        
        return new BoundMethodCallExpression(node.Syntax, node.MethodSymbol, statements.ToImmutable());
    }

    protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
    {
        var expression = RewriteExpression(node.Operand);
        if (expression == node.Operand)
            return node;

        return new BoundUnaryExpression(node.Syntax, node.Op, expression);
    }

    protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
    {
        var left = RewriteExpression(node.Left);
        var right = RewriteExpression(node.Right);
        if (left == node.Left && right == node.Right)
            return node;

        return new BoundBinaryExpression(node.Syntax, left, node.Op, right);
    }

    protected virtual BoundExpression RewriteVariableExpression(BoundExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteLiteralExpression(BoundExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
    {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new BoundAssignmentExpression(node.Syntax, node.Variable, expression);
    }

    
    public virtual BoundStatement RewriteStatement(BoundStatement node)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.BlockStatement:
                return RewriteBlockStatement((BoundBlockStatement)node);
            case BoundNodeKind.ExpressionStatement:
                return RewriteExpressionStatement((BoundExpressionStatement)node);
            case BoundNodeKind.VariableDeclarationStatement:
                return RewriteVariableDeclarationStatement((BoundVariableDeclarationStatement)node);
            case BoundNodeKind.VariableDeclarationAssignmentStatement:
                return RewriteVariableDeclarationAssignmentStatement((BoundVariableDeclarationAssignmentStatement)node);
            case BoundNodeKind.IfStatement:
                return RewriteIfStatement((BoundIfStatement)node);
            case BoundNodeKind.WhileStatement:
                return RewriteWhileStatement((BoundWhileStatement)node);
            case BoundNodeKind.ForStatement:
                return RewriteForStatement((BoundForStatement)node);
            case BoundNodeKind.LabelStatement:
                return RewriteLabelStatement((BoundLabelStatement)node);
            case BoundNodeKind.ConditionalGotoStatement:
                return RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node);
            case BoundNodeKind.GotoStatement:
                return RewriteGotoStatement((BoundGotoStatement)node);
            case BoundNodeKind.ReturnStatement:
                return RewriteReturnStatement((BoundReturnStatement)node);
            default:
                throw new("Unexpected node " + node.Kind);
        }
    }

    protected virtual BoundStatement RewriteVariableDeclarationStatement(BoundVariableDeclarationStatement node)
    {
        return node;
    }
    
    protected virtual BoundVariableDeclarationAssignmentStatement RewriteVariableDeclarationAssignmentStatement(BoundVariableDeclarationAssignmentStatement node)
    {
        var initializer = RewriteExpression(node.Initializer);
        if (initializer == node.Initializer)
            return node;

        return new(node.Syntax, node.Variable, initializer);
    }

    protected virtual BoundStatement RewriteReturnStatement(BoundReturnStatement node)
    {
        var expression = node.Expression is null 
            ? null 
            : RewriteExpression(node.Expression);
        if (expression == node.Expression && expression is not null)
            return node;

        return new BoundReturnStatement(node.Syntax, expression);
    }

    protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) 
        => node;

    protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) 
        => node;

    protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
    {
        var condition = RewriteExpression(node.Condition);
        if (condition == node.Condition)
            return node;
        
        return new BoundConditionalGotoStatement(node.Syntax,node.Label, condition, node.JumpIfTrue);
    }

    protected virtual BoundBlockStatement RewriteBlockStatement(BoundBlockStatement node)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        var changed = false;
        foreach (var statement in node.Statements)
        {
            var rewritten = RewriteStatement(statement);
            changed |= rewritten != statement;
            statements.Add(rewritten);
        }
        
        if (!changed)
            return node;
        
        return new(node.Syntax, statements.ToImmutable());
    }

    protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        var condition = RewriteExpression(node.Condition);
        var thenStatement = RewriteStatement(node.ThenStatement);
        var elseStatement = node.ElseStatement is null 
            ? null
            : RewriteStatement(node.ElseStatement);
        
        if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
            return node;

        return new BoundIfStatement(node.Syntax, condition, thenStatement, elseStatement);
    }

    protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        var condition = RewriteExpression(node.Condition);
        var body = RewriteStatement(node.Body);
        if (condition == node.Condition && body == node.Body)
            return node;
        
        return new BoundWhileStatement(node.Syntax, condition, body, node.BreakLabel, node.ContinueLabel);
    }

    protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
    {
        BoundVariableDeclarationAssignmentStatement? declaration = node.VariableDeclarationAssignment;
        BoundExpression? expression = node.Expression;
        if (node.VariableDeclarationAssignment is not null)
            declaration = RewriteVariableDeclarationAssignmentStatement(node.VariableDeclarationAssignment);
        else
            expression = RewriteExpression(node.Expression.NullGuard());

        var condition = RewriteExpression(node.Condition);
        var mutation = RewriteExpression(node.Mutation);
        var body = RewriteStatement(node.Body);
        if (declaration == node.VariableDeclarationAssignment 
            && expression == node.Expression 
            && condition == node.Condition 
            && mutation == node.Mutation 
            && body == node.Body)
            return node;

        return new BoundForStatement(node.Syntax, declaration, expression, condition, mutation, body, node.BreakLabel, node.ContinueLabel);
    }

    protected virtual BoundExpressionStatement RewriteExpressionStatement(BoundExpressionStatement node)
    {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new BoundExpressionStatement(node.Syntax, expression);
    }
}