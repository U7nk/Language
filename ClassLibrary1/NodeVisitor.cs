using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired.Nodes;

namespace Wired
{
    public abstract class NodeVisitor
    {
        public abstract void VisitPropertyNode(PropertyNode node);
        public abstract void VisitMethodNode(MethodNode node);

        public abstract void VisitTernaryIfNode(TernaryIfNode ternaryIfNode);
    }

    public class ReplacNodeVisitor : NodeVisitor
    {
        public Node From { get; private set;}
        public Node To { get; private set; }
        public bool IsReplaced { get; private set; }

        public ReplacNodeVisitor(Node from, Node to)
        {
            this.From = from;
            this.To = to;
        }
        public override void VisitPropertyNode(PropertyNode node)
        {
            if (node == To || IsReplaced) 
            {
                return;
            }
            if (node.Left == From)
            {
                node.Left = To;
                To.Parent = node;
                IsReplaced = true;
            }
            else if (node.Right == From)
            {
                node.Right = To;
                To.Parent = node;
                IsReplaced = true;
            }
            else
            {
                node.Right.AcceptVisit(this);
                node.Left.AcceptVisit(this);
            }
        }
        public override void VisitMethodNode(MethodNode node)
        {
            if (node == To || IsReplaced)
            {
                return;
            }
            var toReplace = default(Node);
            foreach (var param in node.Parameters)
            {
                if (param == From)
                {
                    toReplace = param;
                    break;
                }
            }
            if (toReplace != default(Node))
            {
                var indx = node.Parameters.IndexOf(toReplace);
                node.Parameters.Remove(toReplace);
                node.Parameters.Insert(indx, To);
                To.Parent = toReplace.Parent;
                IsReplaced = true;
            }
            else
            {
                foreach (var param in node.Parameters)
                {
                    param.AcceptVisit(this);
                }
            }
        }
        public bool Replace(AST ast)
        {
            if (ast.Root == From)
            {
                ast.Root = To;
                To.Parent = null;
                IsReplaced = true;
            }
            ast.Root.AcceptVisit(this);
            return IsReplaced;
        }

        public override void VisitTernaryIfNode(TernaryIfNode node)
        {
            if (node == To || IsReplaced)
            {
                return;
            }
            if (node.Statement == From)
            {
                node.Statement = To;
                To.Parent = From.Parent;
                IsReplaced = true;
            }
            else if (node.Left == From)
            {
                node.Left = To;
                To.Parent = From.Parent;
                IsReplaced = true;
            }
            else if (node.Right == From)
            {
                node.Right = To;
                To.Parent = From.Parent;
                IsReplaced = true;
            }
            else
            {
                node.Statement.AcceptVisit(this);
                node.Left.AcceptVisit(this);
                node.Right.AcceptVisit(this);
            }
        }
    }
}
