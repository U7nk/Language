using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

static class BoundNodePrinter
{
    public static void WriteTo(this MethodSymbol methodSymbol, TextWriter writer)
    {
        writer.Write(SyntaxFacts.GetText(SyntaxKind.FunctionKeyword));
        writer.Write(" ");
        writer.Write(methodSymbol.Name);
        writer.Write("(");
        foreach (var parameter in methodSymbol.Parameters)
        {
            writer.Write(parameter.Name);
            if (!Equals(parameter, methodSymbol.Parameters.Last()))
            {
                writer.Write(", ");
            }
        }
        writer.Write(")");
        writer.Write(" ");
        writer.Write(SyntaxFacts.GetText(SyntaxKind.ColonToken));
        writer.Write(" ");
        writer.Write(methodSymbol.ReturnType);
    }
    
    public static void WriteTo(this TypeSymbol functionSymbol, TextWriter writer)
    {
        writer.Write(SyntaxFacts.GetText(SyntaxKind.ClassKeyword));
        writer.Write(" ");
        writer.Write(functionSymbol.Name);
    }
    
    public static void WriteTo(this BoundNode node, TextWriter writer)
    {
        if (writer is IndentedTextWriter indented)
            WriteTo(node, indented);
        else
            WriteTo(node, new IndentedTextWriter(writer));
    }

    public static void WriteTo(this BoundNode node, IndentedTextWriter writer)
    {
        switch (node.Kind)
        {
            // statements 
            case BoundNodeKind.BlockStatement:
                WriteBlockStatement((BoundBlockStatement)node, writer);
                break;
            case BoundNodeKind.ExpressionStatement:
                WriteExpressionStatement((BoundExpressionStatement)node, writer);
                break;
            case BoundNodeKind.VariableDeclarationStatement:
                WriteVariableDeclarationStatement((BoundVariableDeclarationStatement)node, writer);
                break;
            case BoundNodeKind.IfStatement:
                WriteIfStatement((BoundIfStatement)node, writer);
                break;
            case BoundNodeKind.WhileStatement:
                WriteWhileStatement((BoundWhileStatement)node, writer);
                break;
            case BoundNodeKind.ForStatement:
                WriteForStatement((BoundForStatement)node, writer);
                break;
            case BoundNodeKind.GotoStatement:
                WriteGotoStatement((BoundGotoStatement)node, writer);
                break;
            case BoundNodeKind.LabelStatement:
                WriteLabelStatement((BoundLabelStatement)node, writer);
                break;
            case BoundNodeKind.ReturnStatement:
                WriteReturnStatement((BoundReturnStatement)node, writer);
                break;
            case BoundNodeKind.ConditionalGotoStatement:
                WriteConditionalGotoStatement((BoundConditionalGotoStatement)node, writer);
                break;
            case BoundNodeKind.UnaryExpression:
                WriteUnaryExpression((BoundUnaryExpression)node, writer);
                break;
            case BoundNodeKind.LiteralExpression:
                WriteLiteralExpression((BoundLiteralExpression)node, writer);
                break;
            case BoundNodeKind.BinaryExpression:
                WriteBinaryExpression((BoundBinaryExpression)node, writer);
                break;
            case BoundNodeKind.VariableExpression:
                WriteVariableExpression((BoundVariableExpression)node, writer);
                break;
            case BoundNodeKind.AssignmentExpression:
                WriteAssignmentExpression((BoundAssignmentExpression)node, writer);
                break;
            case BoundNodeKind.ErrorExpression:
                WriteErrorExpression((BoundErrorExpression)node, writer);
                break;
            case BoundNodeKind.MethodCallExpression:
                WriteCallExpression((BoundMethodCallExpression)node, writer);
                break;
            case BoundNodeKind.ConversionExpression:
                WriteConversionExpression((BoundConversionExpression)node, writer);
                break;
            default:
                throw new Exception("Unexpected node " + node.Kind);
        }
    }

    static void WriteReturnStatement(BoundReturnStatement node, IndentedTextWriter writer)
    {
        writer.Write("return ");
        node.Expression?.WriteTo(writer);
        writer.WriteLine();
    }

    static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer)
    {
        var oldIndent = writer.Indent;
        writer.Indent = 0;
        writer.Write(node.Label.Name);
        writer.Write(":");
        writer.WriteLine();
        writer.Indent = oldIndent;
    }

    static void WriteConditionalGotoStatement(BoundConditionalGotoStatement node, IndentedTextWriter writer)
    {
        writer.Write("goto ");
        writer.Write(node.Label.Name);
        writer.Write(node.JumpIfTrue ? " if " : " unless ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer)
    {
        var value = node.Value?.ToString();
        if (node.Type == TypeSymbol.String) 
            value = "\"" + value?.Replace("\"", "\\\"") + "\"";
        
        writer.Write(value ?? "null");
    }
    
    static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer)
    {
        var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
        var precedence = node.Op.SyntaxKind.GetUnaryOperatorPrecedence();
        
        writer.Write(op);
        writer.WriteNestedExpression(precedence, node.Operand);
    }

    static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer)
    {
        var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
        var precedence = node.Op.SyntaxKind.GetBinaryOperatorPrecedence();
        
        writer.WriteNestedExpression(precedence, node.Left);
        writer.Write(op);
        writer.WriteNestedExpression(precedence, node.Right);
    }

    static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, BoundExpression expression)
    {
        var needsParens = parentPrecedence >= currentPrecedence;
        if (needsParens)
            writer.Write("(");
        
        expression.WriteTo(writer);
        
        if (needsParens)
            writer.Write(")");
    }
    static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, BoundExpression expression)
    {
        if (expression is BoundUnaryExpression unary)
        {
            writer.WriteNestedExpression(parentPrecedence, unary.Op.SyntaxKind.GetUnaryOperatorPrecedence(), unary);
        }
        else if (expression is BoundBinaryExpression binary)
        {
            writer.WriteNestedExpression(parentPrecedence, binary.Op.SyntaxKind.GetBinaryOperatorPrecedence(), binary);
        }
        else
        {
            expression.WriteTo(writer);
        }
    }
    static void WriteNestedStatement(this IndentedTextWriter writer, BoundStatement node)
    {
        if (node is BoundBlockStatement)
            WriteTo(node, writer);
        else
        {
            writer.Indent++;
            WriteTo(node, writer);
            writer.Indent--;
        }
    }

    static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer)
    {
        writer.Write(node.Variable.Name);
    }

    static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
    {
        writer.Write(node.Variable.Name);
        writer.Write(" = "); 
        node.Expression.WriteTo(writer);
    }

    static void WriteErrorExpression(BoundErrorExpression node, IndentedTextWriter writer)
    {
        writer.Write("?");
    }

    static void WriteCallExpression(BoundMethodCallExpression node, IndentedTextWriter writer)
    {
        writer.Write(node.MethodSymbol.Name);
        writer.Write("(");
        var isFirst = true;
        foreach (var arg in node.Arguments)
        {
            if (isFirst)
                isFirst = false;
            else
                writer.Write(", ");
            arg.WriteTo(writer);
        }
        writer.Write(")");
    }

    static void WriteConversionExpression(BoundConversionExpression node, IndentedTextWriter writer)
    {
        writer.Write(node.Type.Name);
        writer.Write("(");
        node.Expression.WriteTo(writer);
        writer.Write(")");
    }

    static void WriteGotoStatement(BoundGotoStatement node, IndentedTextWriter writer)
    {
        writer.Write("goto ");
        writer.Write(node.Label.Name);
        writer.WriteLine();
    }

    static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer)
    {
        writer.Write("for (");
        if (node.VariableDeclaration is not null)
        {
            writer.Write("var ");
            writer.Write(node.VariableDeclaration.Variable.Name);
            writer.Write(" = ");
            WriteTo(node.VariableDeclaration.Initializer, writer);
        }
        else
            node.Expression!.WriteTo(writer);

        writer.Write("; ");
        node.Condition.WriteTo(writer);
        writer.Write("; ");
        node.Mutation.WriteTo(writer);
        writer.WriteLine(")");
        node.Body.WriteNestedStatement(writer);
    }

    static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
    {
        writer.Write("while");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
        node.Body.WriteNestedStatement(writer);
    }

    static void WriteNestedStatement(this BoundStatement node, IndentedTextWriter writer)
    {
        if (node is BoundBlockStatement)
            WriteTo(node, writer);
        else
        {
            writer.Indent++;
            WriteTo(node, writer);
            writer.Indent--;
        }
    }

    static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer)
    {
        writer.Write("if ");
        writer.Write("(");
        node.Condition.WriteTo(writer);
        writer.Write(")");
        writer.WriteLine();
        node.ThenStatement.WriteNestedStatement(writer);
        if (node.ElseStatement != null)
        {
            writer.WriteLine();
            writer.Write("else");
            writer.WriteLine();
            node.ElseStatement.WriteNestedStatement(writer);
        }
    }

    static void WriteVariableDeclarationStatement(BoundVariableDeclarationStatement node, IndentedTextWriter writer)
    {
        writer.Write(node.Variable.IsReadonly ? "let " : "var ");
        writer.Write(node.Variable.Name);
        writer.Write(" = ");
        node.Initializer.WriteTo(writer);
        writer.WriteLine();
    }

    static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
    {
        node.Expression.WriteTo(writer);
        writer.WriteLine();
    }

    static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer)
    {
        writer.WriteLine("{");
        writer.Indent++;
        foreach (var statement in node.Statements)
            statement.WriteTo(writer);

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }
}