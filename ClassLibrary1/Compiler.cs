using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired.Nodes;
using System.Reflection;
using System.Linq.Expressions;
using Wired.Exceptions;

namespace Wired.Compilers
{
    public class WiredCompiler
    {
        private readonly Dictionary<string, object> context;
        private readonly Dictionary<string, object> keywords;
        public WiredCompiler(Dictionary<string, object> context)
        {
            this.context = context;
            this.keywords = new Dictionary<string, object>()
            {
                {"true", true},
                {"false", false},
            };
        }
        public Func<object> Compile(AST ast)
        {
            var root = ast.Root;
            var rootCompiled = ConditionalExpressionToNormal(CompileInternal(root));
            var lType = typeof(Func<>).MakeGenericType(GetTypeOfExpression(rootCompiled));
            var lambda = Expression.Lambda(lType, rootCompiled, Extensions.EmptyOf<ParameterExpression>());
            var compiledLambda = lambda.Compile();
            return () => compiledLambda.DynamicInvoke();
        }
        
        private Type GetTypeOfExpression(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Conditional)
            {
                return GetTypeOfExpression(exp.As<ConditionalExpression>().IfTrue);
            }
            
            return exp.Type;
        }
        private Expression ConditionalExpressionToNormal(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Conditional)
            {
                var condExp = exp.As<ConditionalExpression>();
                var lType = typeof(Func<>).MakeGenericType(GetTypeOfExpression(condExp));
                return Expression.Invoke(
                    Expression.Constant(
                        Expression.Lambda(lType, Expression.Condition(condExp.Test, condExp.IfTrue, condExp.IfFalse)).Compile()));
            }
            return exp;
        }
        private List<Expression> WrapWithConvert(List<Expression> wrappable, List<Type> wrapTypes)
        {
            if (wrappable.Count != wrapTypes.Count)
            {
                throw new Exception("cannot wrap");
            }
            var res = new List<Expression>(wrappable.Count);
            for (int i = 0; i < wrappable.Count; i++)
            {
                var toWrap = wrappable[i];
                var type = wrapTypes[i];
                if (toWrap.Type != type)
                {
                    res.Add(Expression.Convert(toWrap, type));
                }
                else
                {
                    res.Add(toWrap);
                }
            }
            return res;
        }
        private Expression MakeAdditionExpression(Expression left, Expression right)
        {
            var lType = GetTypeOfExpression(left);
            var rType = GetTypeOfExpression(right);
            if (rType == typeof(string))
            {
                return Expression.Add(left, right, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
            }
            return Expression.Add(left, right);
        }
        private Expression CompileInternal(Node node)
        {
            if (node is PropertyNode)
            {
                var binary = node.As<PropertyNode>();
                Node right = binary.Right;
                var left = CompileInternal(binary.Left);
                while (right is PropertyNode) 
                {
                    var rightLeft = right.As<PropertyNode>().Left;
                    if (rightLeft is MethodNode)
                    {
                        var args = rightLeft.As<MethodNode>().Parameters
                            .Select(x => ConditionalExpressionToNormal(CompileInternal(x)))
                            .ToList();
                        var methodNode = rightLeft.As<MethodNode>();
                        var method = methodNode.MethodInfo;
                        if (methodNode.IsExtension)
                        {
                            args.Insert(0, left);
                            left = Expression.Call(method, WrapWithConvert(args, method.GetParameters().Select(x => x.ParameterType).ToList()));
                        }
                        else
                        {
                            left = Expression.Call(left, method, WrapWithConvert(args, method.GetParameters().Select(x => x.ParameterType).ToList()));
                        }
                    }
                    else
                    {
                        left = Expression.PropertyOrField(left, right.As<PropertyNode>().Left.As<IdNode>().Name);
                    }
                    right = right.As<PropertyNode>().Right;
                }

                
                if (right is MethodNode)
                {
                    
                    var args = right.As<MethodNode>().Parameters
                        .Select(x => ConditionalExpressionToNormal(CompileInternal(x)))
                        .ToList();
                    var method = right.As<MethodNode>().MethodInfo;
                    if (method.IsStatic)
                    {
                        // TODO такая проверка на метод расширение, скорее всего глупая, пересмотреть ее
                        // когда делаем тайпчек можем определить что метод является методом расширения или статическим методом вызываемым с класса
                        if (method.GetParameters().Count() != args.Count)
                        // метод является методом расширения 
                        {
                            var stArgs = new List<Expression> { left };
                            stArgs.AddRange(args);
                            left = Expression.Call(method, WrapWithConvert(stArgs, method.GetParameters().Select(x => x.ParameterType).ToList()));
                        }
                        else
                        {
                            left = Expression.Call(method, WrapWithConvert(args, method.GetParameters().Select(x => x.ParameterType).ToList()));
                        }
                    }
                    else
                    {
                        left = Expression.Call(left, method, WrapWithConvert(args, method.GetParameters().Select(x => x.ParameterType).ToList()));
                    }
                }
                else
                {
                    left = Expression.PropertyOrField(left, right.As<IdNode>().Name);
                }
                return left;
            }
            if (node is LambdaNode)
            {
                return CompileLambda(node.As<LambdaNode>());
            }
            if (node is IdNode)
            {
                var id = (IdNode)node;
                var str = id.Name;
                return null;
            }
            if (node is TernaryIfNode)
            {
                var ternaryIf = (TernaryIfNode)node;
                var statement = CompileInternal(ternaryIf.Statement);
                var left = CompileInternal(ternaryIf.Left);
                var right = CompileInternal(ternaryIf.Right);
                return Expression.Condition(statement, left, right);
            }
            if (node is BinaryGreaterNode)
            {
                var binaryGreater = (BinaryGreaterNode)node;
                var left = CompileInternal(binaryGreater.Left);
                var right = CompileInternal(binaryGreater.Right);
                return Expression.GreaterThan(left, right);
            }
            if (node is MethodNode)
            {
                var methodNode = (MethodNode)node;
                var str = methodNode.MethodName;
                var args = methodNode.Parameters
                        .Select(x => ConditionalExpressionToNormal(CompileInternal(x)))
                        .ToList();
                if (!methodNode.MethodInfo.IsStatic)
                {
                    throw new WiredException("should be static");
                }

                return Expression.Call(
                    methodNode.MethodInfo, 
                    WrapWithConvert(args, methodNode.MethodInfo.GetParameters().Select(x => x.ParameterType).ToList())); ;
            }
            if (node is PlusNode)
            {
                var plusNode = (PlusNode)node;
                var left = CompileInternal(plusNode.Left);
                var right = CompileInternal(plusNode.Right);
                return MakeAdditionExpression(left, right);
            }
            if (node is MinusNode)
            {
                var minusNode = (MinusNode)node;
                var left = CompileInternal(minusNode.Left);
                var right = CompileInternal(minusNode.Right);
                return Expression.SubtractChecked(left, right);
            }
            if (node is MulNode)
            {
                var mulNode = (MulNode)node;
                var left = CompileInternal(mulNode.Left);
                var right = CompileInternal(mulNode.Right);
                return Expression.MultiplyChecked(left, right);
            }
            if (node is DivNode)
            {
                var divlNode = (DivNode)node;
                var left = CompileInternal(divlNode.Left);
                var right = CompileInternal(divlNode.Right);
                return Expression.Divide(left, right);
            }
            if (node is LiteralNode)
            {
                var litNode= node.As<LiteralNode>();
                return Expression.Constant(litNode.LiteralValue, litNode.LiteralValue.GetType());
            }
           
            if (node is CtxKeywordNode)
            {
                var ctxValue = context["$ctx"];
                return Expression.Constant(ctxValue, node.As<ITypedNode>().Type.AsCSharp);
            }
            if (node is LambdaParamNode)
            {
                var lambdaParamNode = node.As<LambdaParamNode>();
                return lambdaParameters[lambdaParamNode];
            }
            if (node is TypeOrNamespaceNode)
            {
                var typeOrNamespace = node.As<TypeOrNamespaceNode>();
                Expression expr = null;
                Type type = null;
                foreach (var part in typeOrNamespace.Parts)
                {
                    part.Type.Switch(
                        csharp => {
                            if (type == null)
                            {
                                type = csharp;
                                return;
                            }
                            if (expr == null)
                            {
                                expr = Expression.Field(null, type.GetField(part.Name));
                                return;
                            }
                            expr = Expression.PropertyOrField(expr, part.Name);
                        }, 
                        wired => { });
                }
                return expr;
            }
            
            throw new Exception();
        }
        private Dictionary<LambdaParamNode, ParameterExpression> lambdaParameters = new Dictionary<LambdaParamNode, ParameterExpression>();
        private LambdaExpression CompileLambda(LambdaNode lambdaNode)
        {
            var parameters = new List<ParameterExpression>();
            foreach (var param in lambdaNode.Parameters)
            {
                var paramExpr = Expression.Parameter(param.Type.AsCSharp, param.Name);
                parameters.Add(paramExpr);
                lambdaParameters.Add(param, paramExpr);
            }
            var body = CompileInternal(lambdaNode.Body);
            var delegateType = lambdaNode.Type.Match<Type>(
                csharpType => {
                    return Expression.GetFuncType(lambdaNode.Parameters
                        .Select(x=> x.Type.AsCSharp)
                        .Concat(new [] { csharpType })
                        .ToArray());
                },
                wiredType => {        
                    if (true/* TODO CHECK IF VOID*/)
                    {
                        return Expression.GetActionType(lambdaNode.Parameters.Select(x => x.Type.AsCSharp).ToArray());
                    }
                    throw new Wired.Exceptions.WiredException();
                });
            foreach (var param in lambdaNode.Parameters)
	        {
                lambdaParameters.Remove(param);
	        }

            return Expression.Lambda(delegateType, body, parameters);
        }
    }
}
