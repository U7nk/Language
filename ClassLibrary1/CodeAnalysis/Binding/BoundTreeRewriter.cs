using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

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
            BoundNodeKind.CallExpression => RewriteCallExpression((BoundCallExpression)node),
            BoundNodeKind.ConversionExpression => RewriteConversionExpression((BoundConversionExpression)node),
            BoundNodeKind.ErrorExpression => RewriteErrorExpression((BoundErrorExpression)node),
            _ => throw new("Unexpected node " + node.Kind)
        };
    }

    protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
    {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;
        
        return new BoundConversionExpression(node.Type, expression);
    }

    protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
    {
        var statements = ImmutableArray.CreateBuilder<BoundExpression>();
        var changed = false;
        foreach (var statement in node.Arguments)
        {
            var rewritten = this.RewriteExpression(statement);
            changed |= rewritten != statement;
            statements.Add(rewritten);
        }
        
        if (!changed)
            return node;
        
        return new BoundCallExpression(node.FunctionSymbol, statements.ToImmutable());
    }

    protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
    {
        var expression = this.RewriteExpression(node.Operand);
        if (expression == node.Operand)
            return node;

        return new BoundUnaryExpression(node.Op, expression);
    }

    protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
    {
        var left = this.RewriteExpression(node.Left);
        var right = this.RewriteExpression(node.Right);
        if (left == node.Left && right == node.Right)
            return node;

        return new BoundBinaryExpression(left, node.Op, right);
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
        var expression = this.RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new BoundAssignmentExpression(node.Variable, expression);
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
            case BoundNodeKind.IfStatement:
                return RewriteIfStatement((BoundIfStatement)node);
            case BoundNodeKind.WhileStatement:
                return RewriteWhileStatement((BoundWhileStatement)node);
            case BoundNodeKind.ForStatement:
                return RewriteForStatement((BoundForStatement)node);
            case BoundNodeKind.LabelStatement:
                return this.RewriteLabelStatement((BoundLabelStatement)node);
            case BoundNodeKind.ConditionalGotoStatement:
                return this.RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node);
            case BoundNodeKind.GotoStatement:
                return RewriteGotoStatement((BoundGotoStatement)node);
            default:
                throw new("Unexpected node " + node.Kind);
        }
    }

    private BoundStatement RewriteGotoStatement(BoundGotoStatement node) 
        => node;

    private BoundStatement RewriteLabelStatement(BoundLabelStatement node) 
        => node;

    private BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
    {
        var condition = this.RewriteExpression(node.Condition);
        if (condition == node.Condition)
            return node;
        
        return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfTrue);
    }

    protected virtual BoundBlockStatement RewriteBlockStatement(BoundBlockStatement node)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        var changed = false;
        foreach (var statement in node.Statements)
        {
            var rewritten = this.RewriteStatement(statement);
            changed |= rewritten != statement;
            statements.Add(rewritten);
        }
        
        if (!changed)
            return node;
        
        return new(statements.ToImmutable());
    }

    protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        var condition = this.RewriteExpression(node.Condition);
        var thenStatement = this.RewriteStatement(node.ThenStatement);
        var elseStatement = node.ElseStatement is null 
            ? null
            : this.RewriteStatement(node.ElseStatement);
        
        if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
            return node;

        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        var condition = this.RewriteExpression(node.Condition);
        var body = this.RewriteStatement(node.Body);
        if (condition == node.Condition && body == node.Body)
            return node;
        
        return new BoundWhileStatement(condition, body);
    }

    protected virtual BoundVariableDeclarationStatement RewriteVariableDeclarationStatement(BoundVariableDeclarationStatement node)
    {
        var initializer = this.RewriteExpression(node.Initializer);
        if (initializer == node.Initializer)
            return node;

        return new(node.Variable, initializer);
    }

    protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
    {
        BoundVariableDeclarationStatement? declaration = node.VariableDeclaration;
        BoundExpression? expression = node.Expression;
        if (node.VariableDeclaration is not null)
            declaration = this.RewriteVariableDeclarationStatement(node.VariableDeclaration);
        else
            expression = RewriteExpression(node.Expression.ThrowIfNull());

        var condition = this.RewriteExpression(node.Condition);
        var mutation = this.RewriteExpression(node.Mutation);
        var body = RewriteStatement(node.Body);
        if (declaration == node.VariableDeclaration 
            && expression == node.Expression 
            && condition == node.Condition 
            && mutation == node.Mutation 
            && body == node.Body)
            return node;

        return new BoundForStatement(declaration, expression, condition, mutation, body);
    }

    protected virtual BoundExpressionStatement RewriteExpressionStatement(BoundExpressionStatement node)
    {
        var expression = this.RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new BoundExpressionStatement(expression);
    }
}