using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Wired.Nodes;
using System.Linq.Expressions;
using System.Reflection;
using Wired.Exceptions;

namespace Wired
{

    public class AST
    {
        public Node Root { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((AST)obj);
        }
        public bool Equals(AST other)
        {
            return this.Root.Equals(other.Root);
        }
        public override int GetHashCode()
        {
            return Root.GetHashCode();
        }
    }

    public class WiredParser
    {
        public WiredParser(Type contextType)
        {
            ctxType = contextType;
            this.nodeFor = new NodeFactory();
            lambdaParams = new List<LambdaParamNode>();
        }
        private NodeFactory nodeFor;
        private Type ctxType;
        private List<Token> input;
        private List<LambdaParamNode> lambdaParams;
        private int curentIndex;
        private Token tok { get { return input[curentIndex]; } }

        private void JumpTo(int index)
        {
            if (index >= this.input.Count || index < 0)
            {
                throw new WiredException(
                    "impossible jump. Jump index should be in range from 0 to " 
                    + (this.input.Count - 1));
            }
            curentIndex = index;
        }
        private void Forward()
        {
            curentIndex++;
            if (curentIndex >= input.Count)
            {
                throw new Exception();
            }
        }
        private void Back()
        {
            curentIndex--;
            if (curentIndex < 0)
            {
                throw new Exception();
            }
        }
        private MoveNexter IsNext(TokenType type)
        {
            Forward();
            if (tok.Type == type)
            {
                Back();
                return new MoveNexter(Forward);
            }

            Back();
            return new MoveNexter();
        }
        private MoveNexter IsCurrent(TokenType type)
        {
            if (tok.Type == type)
            {
                return new MoveNexter(Forward);
            }
            return new MoveNexter();
        }

        public AST Parse(List<Token> input)
        {   
            curentIndex = 0;
            this.input = input;
            
            return new AST() { Root = ParseExpression() };
        }

        private List<Node> ParseArgumentList()
        {
            var args = new List<Node>();
            if (tok.Type == TokenType.ClFrame)
            {
                return args;
            }
            while (true)
            {
                args.Add(ParseExpression());
                if (IsCurrent(TokenType.Comma).OnTrue(Forward))
                {
                    continue;
                }
                break;
            }
            return args;
        }

        private Node ParseMemberCallChain(Node chainStart)
        {
            Node curNode = chainStart;
            var binNode = new PropertyNode(null, null);
            if (IsCurrent(TokenType.Dot).OnTrue(Forward))
            {
                var name = tok.StringValue;
                if (IsNext(TokenType.OpFrame).OnTrue(Forward,2))
                {
                    var arguments = ParseArgumentList();
                    binNode = nodeFor.MethodCall(curNode, name, arguments);  
                    Forward();
                }
                else
                {
                    binNode = nodeFor.PropertyCall(curNode, name);
                    Forward();
                }
            }
            var root = binNode;
            while(IsCurrent(TokenType.Dot).OnTrue(Forward))
            {
                if (IsCurrent(TokenType.Id))
                {
                    var name = tok.StringValue;
                    if (IsNext(TokenType.OpFrame).OnTrue(Forward, 2))
                    {
                       var arguments = ParseArgumentList();
                       var binNodeRight = nodeFor.MethodCall(binNode.Right, name, arguments);
                       binNodeRight.Parent = binNode;

                       binNode.Right = binNodeRight;
                       binNode = binNodeRight;
                       
                    }
                    else
                    {
                        var binNodeRight = nodeFor.PropertyCall(binNode.Right, name);
                        binNodeRight.Parent = binNode;

                        binNode.Right = binNodeRight;
                        binNode = binNodeRight;
                    }
                    
                    Forward();
                }
            }
            return root;
        }

        private Node ParseMemberCall()
        {
            Node res = null;
            if (tok.Type == TokenType.CtxKeyword)
            {
                res = new CtxKeywordNode(this.ctxType);
                Forward();
            }
            else if (tok.Type == TokenType.Id)
            {
                if (lambdaParams.Any(x => x.Name == tok.StringValue))
                {
                    res = lambdaParams.Single(x => x.Name == tok.StringValue);
                    Forward();
                }
                else
                {
                    var typeOrNamespace = new TypeOrNamespaceNode();
                    typeOrNamespace.Parts.Add(new IdNode(tok.StringValue));
                    Forward();
                    while (IsCurrent(TokenType.Dot).OnTrue(Forward))
                    {
                        if (IsCurrent(TokenType.Id))
                        {
                            if (IsNext(TokenType.OpFrame).OnTrue(Back))
                            {
                                break;
                            }
                            typeOrNamespace.Parts.Add(new IdNode(tok.StringValue));
                            Forward();
                        }
                    }
                    res = typeOrNamespace;
                }
            }
            else  if (tok.Type == TokenType.StringLiteral)
            {
                res = new StringNode(tok.StringValue);
                Forward();
            }
            else if (IsCurrent(TokenType.TrueKeyword))
            {
                res = new TrueKeywordNode();
                Forward();
            }
            else if (IsCurrent(TokenType.FalseKeyword))
            {
                res = new FalseKeywordNode();
                Forward();
            }
            else if (IsCurrent(TokenType.IntLiteral))
            {
                res = new IntNode(tok.StringValue);
                Forward();
            }
            else if (IsCurrent(TokenType.DoubleLiteral))
            {
                res = new DoubleNode(tok.StringValue);
                Forward();
            }
            else if (IsCurrent(TokenType.FloatLiteral))
            {
                res = new FloatNode(tok.StringValue);
                Forward();
            }

            if (tok.Type == TokenType.Dot)
            {
                return ParseMemberCallChain(res);
            }
            if (res == null)
            {
                throw new Exception();
            }
            return res;
        }

        private Node ParseComparisonRightSide()
        {
            var member = ParseMemberCall();
            
            if (tok.Type == TokenType.ClFrame ||
                tok.Type == TokenType.Comma ||
                tok.Type == TokenType.Plus ||
                tok.Type == TokenType.Colon ||
                tok.Type == TokenType.BinaryGreater ||
                tok.Type == TokenType.Question)
            {
                return member;
            }
            if (tok.Type == TokenType.EOF)
            {
                Back();
                return member;
            }
            throw new Exception("id undexpected");
        }

        private Node ParseLambdaExpression(List<LambdaParamNode> parameters)
        {
            lambdaParams.AddRange(parameters);
            var member = ParseExpression();
            var lambda = new LambdaNode(parameters, member);
            lambdaParams.RemoveAll(x => parameters.Contains(x));
            return lambda;
        }

        private Node ParseLambdaBody(List<LambdaParamNode> parameters)
        {
            Node res = null;
            if (tok.Type == TokenType.CtxKeyword)
            {
                res = new CtxKeywordNode(ctxType);
            }
            else if (tok.Type == TokenType.Id)
            {
                if (parameters.Any(x => x.Name == tok.StringValue))
                {
                    res = parameters.First(x => x.Name == tok.StringValue);
                }
                else
                {
                    throw new Exception();
                    var namespaceOrType = "";
                    while (tok.Type == TokenType.Id)
                    {
                        namespaceOrType += tok.StringValue + ".";
                        // TODO CHECK TYPE
                        if (!IsNext(TokenType.Dot))
                        {
                            namespaceOrType = namespaceOrType.CutTail(1);
                            break;
                        }
                    }
                    //TODO IMPLEMENTATION OF NAMESPACEORTYPE PARSING
                    throw new Exception();
                }
            }
            else if (tok.Type == TokenType.StringLiteral)
            {
                res = new StringNode(tok.StringValue);
            }
            else if (IsCurrent(TokenType.TrueKeyword))
            {
                res = new TrueKeywordNode();
            }
            else if (IsCurrent(TokenType.FalseKeyword))
            {
                res = new FalseKeywordNode();
            }
            else if (IsCurrent(TokenType.IntLiteral))
            {
                res = new IntNode(tok.StringValue);
            }
            else if (IsCurrent(TokenType.DoubleLiteral))
            {
                res = new DoubleNode(tok.StringValue);
            }
            else if (IsCurrent(TokenType.FloatLiteral))
            {
                res = new FloatNode(tok.StringValue);
            }
            Forward();
            if (tok.Type == TokenType.Dot)
            {
                return ParseMemberCallChain(res);
            }
            if (IsCurrent(TokenType.BinaryGreater))
            {
                Forward();
                var binaryGreater = new BinaryGreaterNode(res, ParseComparisonRightSide());
                if (tok.Type == TokenType.Question)
                {
                    Forward();
                    var statement = binaryGreater;
                    var left = ParseExpression();
                    if (tok.Type != TokenType.Colon)
                    {
                        throw new Exception("Colon expected");
                    }
                    Forward();
                    var right = ParseExpression();
                    return new TernaryIfNode(statement, left, right);
                }
                return binaryGreater;
            }
            if (res == null)
            {
                throw new Exception();
            }
            return res;
        }

        private Node ParseExpression()
        {
            Node member = null;
            if (IsCurrent(TokenType.OpFrame).OnTrue(Forward))
            {
                if (IsCurrent(TokenType.Id))
                {
                    var parsedLambdaParameters = default(List<LambdaParamNode>);
                    if (TryParseLambdaParameters(out parsedLambdaParameters))
                    {
                        if (IsCurrent(TokenType.ClFrame).OnTrue(Forward).OnFalse(Back))
                        {
                            if (IsCurrent(TokenType.LambdaArrow).OnTrue(Forward).OnFalse(Back, 2))
                            {
                                return ParseLambdaExpression(parsedLambdaParameters);
                            }
                        }
                    }
                }
                member = ParseExpression();
                if (tok.Type != TokenType.ClFrame)
                {
                    throw new Exception();
                }
                Forward();
            }
            if (IsCurrent(TokenType.Id))
            {
                var lParam = new LambdaParamNode(tok.StringValue, null);
                Forward();
                if (IsCurrent(TokenType.LambdaArrow).OnTrue(Forward).OnFalse(Back))
                {
                    return ParseLambdaExpression(new List<LambdaParamNode> { lParam });
                }
            }
            if (member == null)
            {
                member = ParseMemberCall();
            }
            if (tok.Type == TokenType.Dot)
            {
                return ParseMemberCallChain(member);
            }
            if (tok.Type == TokenType.BinaryGreater)
            {
                Forward();
                var binaryGreater = new BinaryGreaterNode(member, ParseComparisonRightSide());
                if (tok.Type == TokenType.Question)
                {
                    Forward();
                    var statement = binaryGreater;
                    var left = ParseExpression();
                    if (tok.Type != TokenType.Colon)
                    {
                        throw new Exception("Colon expected");
                    }
                    Forward();
                    var right = ParseExpression();
                    return new TernaryIfNode(statement, left, right);
                }
                return binaryGreater;
            }
            if (tok.Type == TokenType.Plus)
            {
                return ParseArithmetics(member);
            }
            if (tok.Type == TokenType.Minus)
            {
                return ParseArithmetics(member);
            }
            if (tok.Type == TokenType.Mul)
            {
                return ParseArithmetics(member);
            }
            if (tok.Type == TokenType.Div)
            {
                return ParseArithmetics(member);
            }
            if (tok.Type == TokenType.Question)
            {
                Forward();
                var statement = member;
                var left = ParseExpression();
                if (tok.Type != TokenType.Colon)
                {
                    throw new Exception("Colon expected");
                }
                Forward();
                var right = ParseExpression();
                return new TernaryIfNode(statement, left, right);
            }
            
            if (tok.Type == TokenType.ClFrame ||
                tok.Type == TokenType.Comma ||
                tok.Type == TokenType.Plus ||
                tok.Type == TokenType.Colon ||
                tok.Type == TokenType.BinaryGreater)
            {
                return member;
            }
            if (tok.Type == TokenType.EOF)
            {
                Back();
                return member;
            }
            throw new Exception("id undexpected");
        }

        private bool TryParseLambdaParameters(out List<LambdaParamNode> parsedLambdaParameters)
        {
            var startingPoint = this.curentIndex;
            parsedLambdaParameters = null;
            var result = new List<LambdaParamNode>();
            while (IsCurrent(TokenType.Id))
            {
                result.Add(new LambdaParamNode(
                    name: tok.StringValue, 
                    type: null));
                Forward();
                if (IsCurrent(TokenType.Comma).OnTrue(Forward))
                {
                    continue;
                }
                break;
            }
            if (IsCurrent(TokenType.ClFrame) && result.Count > 0)
            {
                parsedLambdaParameters = result;
                return true;
            }
            JumpTo(startingPoint);
            return false;
        }

        private Node ParseArithmetics(Node enterNode)
        {
            ArithmeticNode firstNode;
            if (tok.Type == TokenType.Plus)
            {
                Forward();
                firstNode = new PlusNode() { Left = enterNode, Right = ParseArithmeticsRightSide() };
            }
            else if (tok.Type == TokenType.Minus)
            {
                Forward();
                firstNode = new MinusNode() { Left = enterNode, Right = ParseArithmeticsRightSide() };
            }
            else if (tok.Type == TokenType.Mul)
            {
                Forward();
                firstNode = new MulNode() { Left = enterNode, Right = ParseArithmeticsRightSide() };
            }
            else if (tok.Type == TokenType.Div)
            {
                Forward();
                firstNode = new DivNode() { Left = enterNode, Right = ParseArithmeticsRightSide() };
            }
            else
            {
                throw new Exception();
            }
            var rootNode = firstNode;
            ArithmeticNode lastNode = firstNode;
            while (tok.Type == TokenType.Plus || tok.Type == TokenType.Minus || tok.Type == TokenType.Mul || tok.Type == TokenType.Div)
            {
                if (tok.Type == TokenType.Plus)
                {
                    Forward();
                    rootNode = new PlusNode() { Left = rootNode, Right = ParseArithmeticsRightSide() };
                    lastNode = rootNode;
                    continue;
                }
                if (tok.Type == TokenType.Minus)
                {
                    Forward();
                    rootNode = new MinusNode() { Left = rootNode, Right = ParseArithmeticsRightSide() };
                    lastNode = rootNode;
                    continue;
                }
                if (tok.Type == TokenType.Mul)
                {
                    Forward();
                    var mulNode = new MulNode() { Left = lastNode.Right, Right = ParseArithmeticsRightSide() };
                    lastNode.Right = mulNode;
                    lastNode = mulNode;
                    continue;
                }
                if (tok.Type == TokenType.Div)
                {
                    Forward();
                    var divNode = new DivNode() { Left = lastNode.Right, Right = ParseArithmeticsRightSide() };
                    lastNode.Right = divNode;
                    lastNode = divNode;
                    continue;
                }
            }
            return rootNode;
        }
        private Node ParseArithmeticsRightSide()
        {
            Node right = null;
            if (tok.Type == TokenType.OpFrame)
            {
                Forward();
                right = ParseArithmetics(ParseMemberCall());
                if (tok.Type != TokenType.ClFrame)
                {
                    throw new Exception();
                }
                Forward();
            }
            if (right == null)
            {
                right = ParseMemberCall();
            }

            return right;
        }
    }
}
