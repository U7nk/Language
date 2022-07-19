using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Wired;
using Wired.OneOf;
using System.Reflection;

namespace Wired.Nodes
{
    public abstract class Node
    {
        public static Node GetTopParent(Node node)
        {
            var cur = node;
            while (cur.Parent != null)
            {
                cur = cur.Parent;
            }
            return cur;
        }
        public static Node FromLiteral(Token tok)
        {
            if (tok.Type == TokenType.IntLiteral)
            {
                return new IntNode(tok.StringValue);
            }
            if (tok.Type == TokenType.StringLiteral)
            {
                return new StringNode(tok.StringValue);
            }
            if (tok.Type == TokenType.DoubleLiteral)
            {
                return new DoubleNode(tok.StringValue);
            }
            if (tok.Type == TokenType.FloatLiteral)
            {
                return new FloatNode(tok.StringValue);
            }
            throw new Exception();
        }

        public static Node FromBinaryOp(Token tok, Node left, Node right)
        {
            if (tok.Type == TokenType.Plus)
            {
                return new PlusNode(left, right);
            }
            if (tok.Type == TokenType.Minus)
            {
                return new MinusNode(left, right);
            }
            
            throw new Exception();
        }

        public static Node FromBinaryOp(Token tok)
        {
            if (tok.Type == TokenType.Plus)
            {
                return new PlusNode();
            }
            if (tok.Type == TokenType.Minus)
            {
                return new MinusNode();
            }

            throw new Exception();
        }

        public void AcceptVisit(NodeVisitor visitor)
        {
            if (this is MethodNode)
            {
                visitor.VisitMethodNode(this.As<MethodNode>());
            }
            else if (this is PropertyNode)
            {
                visitor.VisitPropertyNode(this.As<PropertyNode>());
            }
            else if (this is TernaryIfNode)
            {
                visitor.VisitTernaryIfNode(this.As<TernaryIfNode>());
            }
        }

        public Node Parent { get; set; }
    }

    public interface ITypedNode
    {
        OneOfTypeWiredType Type { get; set; }
    }

    public abstract class LiteralNode : Node, ITypedNode
    {
        public OneOfTypeWiredType Type { get; set; }
        public object LiteralValue { get;  set; }
        public LiteralNode(object value)
        {
            Type = value.GetType();
            LiteralValue = value;
        }
    }



    [DebuggerDisplay("$ctx")]
    public class CtxKeywordNode : Node, ITypedNode
    {
        public OneOfTypeWiredType Type { get; set; }
        public CtxKeywordNode(OneOf<Type, WiredType> type)
        {
            this.Type = type;
        }
    }

    [DebuggerDisplay("true")]
    public class TrueKeywordNode : LiteralNode
    {
        public TrueKeywordNode() : base(true){ }
    }
    [DebuggerDisplay("false")]
    public class FalseKeywordNode : LiteralNode
    {
        public FalseKeywordNode() : base(false) { }
    }

    [DebuggerDisplay("NamespaceOrType : {Name}")]
    public class NamespaceOrTypeNode : Node
    {
        public string Name { get; private set; }
        public NamespaceOrTypeNode(string name)
        {
            this.Name = name;
        }

    }

    [DebuggerDisplay("ID : {Name}")]
    public class IdNode : Node, ITypedNode
    {
        public OneOfTypeWiredType Type { get; set; }
        public string Name { get; private set; }
        public IdNode(string name)
        {
            this.Name = name;
        }

    }
    [DebuggerDisplay("LambdaParam : {Name}")]
    public class LambdaParamNode : Node, ITypedNode
    {
        public OneOfTypeWiredType Type { get; set; }
        public string Name { get; private set; }
        public LambdaParamNode(string name, OneOf<Type, WiredType> type)
        {
            this.Type = type;
            this.Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is LambdaParamNode)
            {
                return this.Equals(obj.As<LambdaParamNode>());
            }
            return false;
        }
        public bool Equals(LambdaParamNode node)
        {
            if (node.Name == this.Name)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    [DebuggerDisplay("Lambda ")]
    public class LambdaNode : Node, ITypedNode
    {
        public OneOfTypeWiredType Type { get; set; }
        public List<LambdaParamNode> Parameters { get; private set; }
        public Node Body { get; private set; }
        public LambdaNode(List<LambdaParamNode> parameters, Node body)
        {
            this.Parameters = parameters;
            this.Body = body;
        }
    }
    [DebuggerDisplay("Int : {Value}")]
    public class IntNode : LiteralNode
    {
        public IntNode(string value) : base(int.Parse(value))
        {
        }
    }
    [DebuggerDisplay("String : {Value}")]
    public class StringNode : LiteralNode
    {
        public StringNode(string value) : base(value)
        {
        }
    }
    [DebuggerDisplay("Double : {Value}")]
    public class DoubleNode : LiteralNode
    {
        public DoubleNode(string value) : base(double.Parse(value))
        {
        }
    }
    [DebuggerDisplay("Float : {Value}")]
    public class FloatNode : LiteralNode
    {
        public FloatNode(string value) : base(float.Parse(value))
        {
        }
    }

    [DebuggerDisplay("Binary")]
    public abstract class BinaryNode : Node, ITypedNode
    {
        private OneOfTypeWiredType type;
        private Node left;
        private Node right;
        public Node Left 
        { 
            get { return left; } 
            set 
            {
                if (value != null)
                {
                    value.Parent = this;
                }
                left = value;
            }
        }
        public Node Right
        {
            get { return right; }
            set
            {
                if (value != null)
                {
                    value.Parent = this;
                }
                right = value;
            }
        }
        public BinaryNode(Node left, Node right)
        {
            Left = left;
            Right = right;
        }

        internal Node GetRightLeaf()
        {
            if (this.Right is BinaryNode)
            {
                return this.Right.As<BinaryNode>().GetRightLeaf();
            }
            return this.Right;
        }

        public OneOfTypeWiredType Type
        {
            get
            {
                if (type == null)
                {
                    return GetRightLeaf().As<ITypedNode>().Type;
                }
                return type;
            }
            set
            {
                type = value;
            }
        }
    }
    
    [DebuggerDisplay("Property")]
    public class PropertyNode : BinaryNode
    {
        public PropertyNode(Node left, Node right) : base(left, right) {}

    }
    public class TypeOrNamespaceNode : Node, ITypedNode
    {
        public List<IdNode> Parts { get; set; }
        public TypeOrNamespaceNode()
        {
            Parts = new List<IdNode>();
        }

        public OneOfTypeWiredType Type
        {
            get { return Parts.Last().As<ITypedNode>().Type; }
            set { throw new InvalidOperationException(); }
        }
    }
    
    [DebuggerDisplay("Plus")]
    public class PlusNode : BinaryNode
    {
        public PlusNode(Node left, Node right) : base(left, right)
        {
            Left = left;
            Right = right;
        }

        public PlusNode() : base(null, null) { }
    }
    [DebuggerDisplay("Div")]
    public class DivNode : BinaryNode
    {
        public DivNode(Node left, Node right) : base(left, right)
        {
            Left = left;
            Right = right;
        }

        public DivNode() : base(null, null) { }
    }
    [DebuggerDisplay("Mul")]
    public class MulNode : BinaryNode
    {
        public MulNode(Node left, Node right) : base(left, right)
        {
            Left = left;
            Right = right;
        }

        public MulNode() : base(null, null) { }
    }
    [DebuggerDisplay("Minus L:{Left} R:{Right}")]
    public class MinusNode : BinaryNode
    {
        public MinusNode(Node left, Node right) : base(left, right) 
        {
            Left = left;
            Right = right;
        }

        public MinusNode() : base(null, null) { }
    }

    [DebuggerDisplay("TernaryIf S:{Statement} L:{Left} R:{Right}")]
    public class TernaryIfNode : Node, ITypedNode
    {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public Node Statement { get; set; }
        public OneOfTypeWiredType Type { get; set; }

        public TernaryIfNode(Node statement, Node left, Node right)
        {
            Statement = statement;
            Left = left;
            Right = right;
        }

    }
    [DebuggerDisplay("BinaryGreater L:{Left} R:{Right}")]
    public class BinaryGreaterNode : BinaryNode
    {
        public BinaryGreaterNode(Node left, Node right) : base(left, right)
        {
        }
    }
    [DebuggerDisplay("Method {MethodName}, {Parameters.Count}")]
    public class MethodNode : Node, ITypedNode
    {
        public bool IsStatic { get; set; }
        public bool IsExtension { get; set; }
        public OneOfTypeWiredType ReturnType { get; set; }
        public MethodInfo MethodInfo { get; set; }
        OneOfTypeWiredType ITypedNode.Type
        {
            get
            {
                return ReturnType;
            }
            set 
            {
                ReturnType = value;
            }
        }
        public List<Node> Parameters { get; private set; }
        public string MethodName { get; private set; }
        public MethodNode(string methodName, List<Node> parameters)
        {
            MethodName = methodName;
            Parameters = parameters;
        }
    }
}
