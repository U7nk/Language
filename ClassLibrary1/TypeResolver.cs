using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired.Nodes;
using System.Reflection;
using Wired.OneOf;
using Wired.Exceptions;
using Wired;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Wired
{
    internal static class TypeResolverExtensions
    {
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }
    public class TypeResolver
    {
        private List<System.Reflection.Assembly> assemblies;
        private List<Type> types;
        private List<string> namespaces;
        private Type ctxType;
        private Dictionary<LambdaParamNode, Type> lambdaParamContext;
        private AST ast;
        public TypeResolver(AppDomain appDomain, string[] additionalUsings = null)
        {
            lambdaParamContext = new Dictionary<LambdaParamNode, Type>();
            InitializeContext(appDomain, additionalUsings);
        }
        public void Resolve(ref AST ast, Type ctxType)
        {
            this.ctxType = ctxType;
            this.ast = ast;
            ResolveInternal(ast.Root);
        }
        private void ResolveInternal(Node root)
        {
            if (root is PropertyNode)
            {
                ResolveBinaryNode(root.As<PropertyNode>());
            }
            else if (root is TypeOrNamespaceNode)
            {
                ResolveTypeOrNamespaceNode(root.As<TypeOrNamespaceNode>());
            }
            else if (root is LambdaNode)
            {
                root.As<ITypedNode>().Type = new DelegateWiredType(root.As<LambdaNode>().Parameters.Count);
                // nothing, will be resolved when working with context
            }
            else if (root is LambdaParamNode)
            {
                root.As<ITypedNode>().Type = lambdaParamContext[root.As<LambdaParamNode>()];
            }
            else if (root is CtxKeywordNode)
            {
                root.As<ITypedNode>().Type = ctxType;
            }
            else if (root is LiteralNode)
            {
                // literal type already resolved
            }
            else if (root is MinusNode
                || root is PlusNode
                || root is MulNode
                || root is DivNode)
            {
                var minNode = root.As<BinaryNode>();
                ResolveInternal(minNode.Left);
                ResolveInternal(minNode.Right);
                if (!TypesIsSame(minNode.Left.As<ITypedNode>().Type, minNode.Right.As<ITypedNode>().Type))
                {
                    throw new TypeResolverException("these objects cannot be used here.");
                }
                minNode.Type = GetSameTypes(minNode.Left.As<ITypedNode>().Type, minNode.Right.As<ITypedNode>().Type);
            }
            else if (root is BinaryGreaterNode)
            {
                var minNode = root.As<BinaryNode>();
                ResolveInternal(minNode.Left);
                ResolveInternal(minNode.Right);
                if (!TypesIsSame(minNode.Left.As<ITypedNode>().Type, minNode.Right.As<ITypedNode>().Type))
                {
                    throw new TypeResolverException("these objects cannot be used here.");
                }
                minNode.Type = typeof(bool);
            }
            else if (root is TernaryIfNode)
            {
                var ternIf = root.As<TernaryIfNode>();

                ResolveInternal(ternIf.Left);
                ResolveInternal(ternIf.Right);
                if (!TypesIsSame(ternIf.Left.As<ITypedNode>().Type, ternIf.Right.As<ITypedNode>().Type))
                {
                    throw new TypeResolverException("these objects cannot be used here.");
                }
                ternIf.Type = GetSameTypes(ternIf.Left.As<ITypedNode>().Type, ternIf.Right.As<ITypedNode>().Type);
                ResolveInternal(ternIf.Statement);
            }
            else
            {
                throw new TypeResolverException("oops... we should check is everything ok in calling code? if everything is ok we should implement type resolve for " + root.GetType());
            }
        }

        private void ResolveTypeOrNamespaceNode(TypeOrNamespaceNode typeOrNamespace)
        {
            if (typeOrNamespace.Parts.Count == 0)
            {
                throw new Exception("cant be");
            }
            var @namespace = "";
            Type type = null;
            for (int i = 0; i < typeOrNamespace.Parts.Count; i++)
            {
                var part = typeOrNamespace.Parts[i];
                if (type == null)
                {
                    if (this.types.Select(x => x.FullName).Contains(@namespace + "." + part.Name))
                    {
                        type = this.types.Single(x => x.FullName == @namespace + "." + part.Name);
                        part.Type = type;
                        continue;
                    }
                    if (this.namespaces.Any(x => x.Contains(@namespace + part.Name) || x.Contains(@namespace + "." + part.Name)))
                    {
                        part.Type = new NamespaceWiredType();
                        if (@namespace == string.Empty)
                        {
                            @namespace = part.Name;
                            continue;
                        }
                        @namespace += "." + part.Name;
                        continue;
                    }
                    throw new TypeResolverException("no type or namespace");
                }
                var prop = type.GetProperty(part.Name);
                if (prop != null)
                {
                    part.Type = prop.PropertyType;
                    continue;
                }
                var field = type.GetField(part.Name, BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    part.Type = field.FieldType;
                    continue;
                }
                throw new TypeResolverException("bad prop");
            }
        }

        private OneOf<Type, WiredType> GetSameTypes(OneOf<Type, WiredType> left, OneOf<Type, WiredType> right)
        {
            // TODO create actual logic
            return left;
        }

        
        private void ResolveBinaryNode(BinaryNode node)
        {
            ResolveInternal(node.Left);
            var left = node.Left;
            var right = node.Right;
            Node outterLeft = left;
            while (right is BinaryNode)
            {
                var rightAsBinary = right.As<BinaryNode>();
                var innerLeft = rightAsBinary.Left;
                innerLeft.As<ITypedNode>().Type = ResolveMemberCall(outterLeft.As<ITypedNode>(), innerLeft);
                right = rightAsBinary.Right;
                outterLeft = innerLeft;
            }
            right.As<ITypedNode>().Type = ResolveMemberCall(outterLeft.As<ITypedNode>(), right);
        }

        private OneOf<Type, WiredType> ResolveMemberCall<T>(T left, Node right) 
            where T : ITypedNode
        {
            if (left.Type == null)
            {
                throw new TypeResolverException("left node should have type to resolve member call.");
            }
            if (right is IdNode)
            {
                return GetTypeOfPropertyOrFieldOfName(left.Type.AsCSharp, right.As<IdNode>().Name);
            }
            if (right is MethodNode)
            {
                foreach (var param in right.As<MethodNode>().Parameters)
                {
                    ResolveInternal(param);
                }
                return GetReturnTypeOfMethod(left.Type.AsCSharp, right.As<MethodNode>());
            }
            throw new TypeResolverException("no support");
        }

        private OneOf<Type, WiredType> GetReturnTypeOfMethod(Type type, MethodNode methodNode)
        {
            // concat with extension methods 
            // and check how c# choosing between extension and class methods with the same name
            // c# prefer class methods over extension methods
            var methods = type.GetMethods().Where(x => x.Name == methodNode.MethodName).ToList();
            var methodParameters = new List<OneOf<Type, Node>>();
            methodNode.Parameters.ForEach(x => methodParameters.Add(x));
            if (methods.Count == 0)
            {
                var staticMethods = types
                    .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(m => m.IsStatic)
                    .Where(m => m.Name == methodNode.MethodName)
                    .ToList();
                if (staticMethods.Count == 0)
                {
                    throw new TypeResolverException("no method with dis name");
                }
                methodNode.IsStatic = true;
                methodNode.IsExtension = true;
                methods = staticMethods;
                methodParameters.Insert(0, type);
            }
            MethodInfo method = null;
            if (TryResolveRightMethod(methods, methodParameters, out method))
            {
                methodNode.MethodInfo = method;
                return method.ReturnType;
            }
            throw new TypeResolverException("failed to resolve method.");
        }

        private bool TryResolveRightMethod(
            IEnumerable<MethodInfo> methods, 
            List<OneOf<Type, Node>> methodParameters, 
            out MethodInfo resolvedMethod)
        {
            resolvedMethod = null;
            var methodCandidates = GetMethodCandidatesParamTypesUndefined(methods, methodParameters);
            if (methodCandidates.Count == 0)
            {
                throw new TypeResolverException("No compatible methods found");
            }
            var method = FindMostCompatibleMethod(
                methodCandidates,
                methodParameters);

            for (var i = 0; i < method.GetParameters().Length; i++)
		    {
                if (methodParameters[i].IsT1 && methodParameters[i].AsT1 is LambdaNode)
                {
                    var lambdaInvoke = method.GetParameters()[i].ParameterType.GetMethod("Invoke");
                    TryResolveLambdaWith(methodParameters[i].AsT1.As<LambdaNode>(), lambdaInvoke);
                }
		    }
            resolvedMethod = method;
            return true;
            
        }

        private OneOf<MethodInfo, Nothing> MakeGenericMethodOf(
            MethodInfo method, 
            List<OneOf<Type, Node>> passedParameters)
        {
            var methodParameters = passedParameters;
            if (!method.IsGenericMethod)
            {
                throw new TypeResolverException("cannot make generic method of non generic: " + method);
            }
            var genericArgs = method.GetGenericArguments();
            var resolvedGenericArgs = new List<Type>(genericArgs.Length);
            var resolvedByGenericArg = new Dictionary<Type,Type>();
            foreach (var genericArg in genericArgs)
            {
                foreach (var param in method.GetParameters())
                {
                    if (param.ParameterType.GetGenericArguments().Contains(genericArg))
                    {
                        var methodParamType = methodParameters[param.Position].Match<Type>(
                            csType => { return csType; },
                            node => {
                                if (node is LambdaNode && param.ParameterType.IsAssignableTo(typeof(Delegate)))
                                {
                                    return node.As<ITypedNode>().Type.Match<Type>(
                                        csharp => { return csharp; },
                                        wired => {
                                            var delegateWiredType = node.As<ITypedNode>().Type.AsWiredType.As<DelegateWiredType>();
                                            var paramTypes = new List<Type>();
                                            var invokeParamTypes = param.ParameterType
                                                .GetMethod("Invoke")
                                                .GetParameters()
                                                .Select(x=> x.ParameterType);
                                            foreach (var item in invokeParamTypes)
	                                        {
                                                if (item.IsGenericParameter)
                                                {
                                                    paramTypes.Add(resolvedByGenericArg[item]);
                                                    continue;
                                                }
                                                paramTypes.Add(item);
	                                        }
                                            var lambdaResult = TryResolveLambdaBodyReturn(
                                                node.As<LambdaNode>(),
                                                paramTypes);
                                            if (lambdaResult == null)
                                            {
                                                return null;
                                            }
                                            paramTypes.Add(lambdaResult);
                                            return Expression.GetFuncType(paramTypes.ToArray());
                                        });
                                }
                                return null;
                            });
                        if (methodParamType == null)
                        {
                            return Nothing.Inst;
                        }
                        var ga = GetGenericArgument(
                            param.ParameterType,
                            methodParamType,
                            genericArg);
                        if (ga.IsT1)
                        {
                            return ga.AsT1;
                        }
                        resolvedByGenericArg.Add(genericArg, ga);
                        resolvedGenericArgs.Add(ga);
                        break;
                    }
                    else if (param.ParameterType.IsGenericParameter && param.ParameterType == genericArg)
                    {
                        var ga = GetGenericArgument(param.ParameterType, methodParameters[param.Position], genericArg);
                        if (ga.IsT1)
                        {
                            return ga.AsT1;
                        }
                        resolvedGenericArgs.Add(ga);
                        break;
                    }
                }
            }
            return method.GetGenericMethodDefinition().MakeGenericMethod(resolvedGenericArgs.ToArray());
        }


        /// <summary>
        /// Where<TSource>(IEnumerable<TSource> collection)
        /// _.Where(myColl); // myColl - string
        /// genericArgToResolve - TSource;
        /// passedParam - myColl;
        /// genericParameter - IEnumerable<TSource> collection;
        /// </summary>
        private OneOf<Type, Nothing> GetGenericArgument(Type genericParameter, Type passedParam, Type genericArgToResolve)
        {
            if (genericParameter == genericArgToResolve)
            {
                if (Extensions.TypeRespectsGenericParameterConstraints(passedParam, genericParameter.GetGenericParameterConstraints()))
                {
                    return passedParam;
                }
            }
            OneOf<Type, Nothing> generalType = GetCompatibleType(genericParameter, passedParam).Match<OneOf<Type, Nothing>>(
                type =>
                {
                    if (type.IsGenericType)
                    {
                        return type.GetGenericArguments()[genericArgToResolve.GenericParameterPosition];
                    }
                    return type;
                }, 
                nothing => {
                    return nothing;
                });
            
            return generalType;
        }

        private OneOf<Type, Nothing> GetCompatibleType(Type compatibleTo, Type hasUnderlyingCompatible)
        {
            if (!Extensions.IsTypesCompatible(compatibleTo, hasUnderlyingCompatible))
            {
                return Nothing.Inst;
            }
            if (compatibleTo.IsGenericParameter)
            {
                if (!compatibleTo.GetGenericParameterConstraints().Any())
                {
                    return hasUnderlyingCompatible;
                }
            }
            if (compatibleTo == hasUnderlyingCompatible)
            {
                return hasUnderlyingCompatible;
            }
            if (compatibleTo.IsGenericType && hasUnderlyingCompatible.IsGenericType)
            {
                if (compatibleTo.GetGenericTypeDefinition() == hasUnderlyingCompatible.GetGenericTypeDefinition())
                {
                    return hasUnderlyingCompatible;
                }
            }
            if (compatibleTo.IsInterface)
            {
                foreach (var @interface in hasUnderlyingCompatible.GetInterfaces())
                {
                    if (@interface.IsGenericType && compatibleTo.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == compatibleTo.GetGenericTypeDefinition())
                    {
                        return @interface;
                    }
                }
            }
            return GetCompatibleType(compatibleTo, hasUnderlyingCompatible.BaseType);
        }

        private MethodInfo FindMostCompatibleMethod(
            List<MethodInfo> methodCandidates, 
            List<OneOf<Type, Node>> methodParameters)
        {
            var parametersTypesMask = methodParameters
                .Select(x => x.Match<OneOf<Type, WiredType>>(
                    type => { return type; },
                    node => { return node.As<ITypedNode>().Type; }))
                .ToList();
            if (methodCandidates.Count == 1)
            {
                return methodCandidates.Single();
            }
            var differenceScoreByCandidate = new Dictionary<MethodInfo, int>();
            foreach (var candidate in methodCandidates)
            {
                var candidateParametersTypes = candidate
                    .GetParameters()
                    .Select(x => x.ParameterType)
                    .ToList();
                int candidateDifferenceScore = 0;
                for (int i = 0; i < candidateParametersTypes.Count; i++)
                {
                    var candidateParameterType = candidateParametersTypes[i];
                    var matchingTo = parametersTypesMask[i];

                    int differenceScore = matchingTo.Match<int>(
                        csharpType => {
                            var distance = 0;
                            if (TryTypesDistance(csharpType, candidateParameterType, out distance))
                            {
                                return distance;
                            }
                            throw new TypeResolverException("uncompatible types");
                        },
                        wiredType => { 
                            if (wiredType is DelegateWiredType) 
                            {
                                return 0;
                            }
                            throw new TypeResolverException("cannot count type difference score for " + wiredType.GetType() + " type.");
                        });
                    
                    candidateDifferenceScore += differenceScore;
                }
                differenceScoreByCandidate.Add(candidate, candidateDifferenceScore);
            }
            return differenceScoreByCandidate
                .Single(x => x.Value == differenceScoreByCandidate.Values.Min())
                .Key;
        }
        private List<MethodInfo> GetMethodCandidatesParamTypesUndefined(
            IEnumerable<MethodInfo> methods,
            List<OneOf<Type, Node>> passedParameters) 
        {
            var result = new List<MethodInfo>();
            foreach (var method in methods)
            {
                
                var specificMethod = method;
                if (method.IsGenericMethod)
                {
                    var genMethod = MakeGenericMethodOf(method, passedParameters);
                    if (genMethod.IsT1)
                        continue;
                    specificMethod = genMethod;
                }
                var goodMethod = true;

                var methodParameters = specificMethod.GetParameters();
                if (methodParameters.Length != passedParameters.Count)
                {
                    continue;
                }
                foreach (var methodParam in methodParameters)
                {
                    var passedParam = passedParameters[methodParam.Position];
                    if (methodParam.ParameterType.IsGenericParameter)
                    {
                        if (!methodParam.ParameterType.GetGenericParameterConstraints().Any())
                        {
                            continue;
                        }
                        goodMethod = false;
                        break;
                    }
                    else // non generic param
                    {
                        var compatibility = passedParam.Match<bool>(
                            (csharpType) => { 
                                return Extensions.IsTypesCompatible(methodParam.ParameterType, csharpType); 
                            },
                            node => {
                                if (node is LambdaNode)
                                {
                                    return IsTypesCompatible(
                                        methodParam.ParameterType,
                                        node.As<LambdaNode>()); 
                                }
                                return Extensions.IsTypesCompatible(methodParam.ParameterType, node.As<ITypedNode>().Type);
                            });

                        if (compatibility)
                        {
                            continue;
                        }
                        goodMethod = false;
                        break;
                    }
                }
                if (goodMethod)
                {
                    result.Add(specificMethod);
                }
            }
            return result;
        }
        protected internal bool TypesIsSame(OneOf<Type, WiredType> left, OneOf<Type, WiredType> right) 
        {
            // TODO MAKE actual check
            return true;
        }
        private bool IsTypesCompatible(Type type, LambdaNode lambdaNode)
        {
            if (type.IsAssignableTo(typeof(Delegate)))
            {
                var invok = type.GetMethod("Invoke");
                if (lambdaNode.Parameters.Count != invok.GetParameters().Length)
                {
                    return false;
                }


                if (TryResolveLambdaWith(lambdaNode, invok))
                {
                    return true;
                }
                return false;
            }
            throw new TypeResolverException("not supported check");
        }


        private bool TryResolveLambdaWith(
            LambdaNode lambdaNode, 
            MethodInfo invokeMethod)
        {
            try
            {
                if (invokeMethod.GetParameters().Length != lambdaNode.Parameters.Count)
                {
                    return false;
                }
                foreach (var p in invokeMethod.GetParameters())
                {
                    var resolvedType = p.ParameterType;
                    
                    lambdaParamContext.Add(lambdaNode.Parameters[p.Position], resolvedType);
                }
                ResolveInternal(lambdaNode.Body);
                if (lambdaNode.Body.As<ITypedNode>().Type.AsCSharp == invokeMethod.ReturnType)
                {
                    lambdaNode.Type = invokeMethod.ReturnType;
                    return true;
                }
                return false;
            }
            catch (TypeResolverException)
            {
                return false;
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
                foreach (var p in lambdaNode.Parameters)
                {
                    lambdaParamContext.Remove(p);
                }
            }
        }

        private Type TryResolveLambdaBodyReturn(
            LambdaNode lambdaNode, 
            List<Type> parametersTypes)
        {
            try
            {
                if (parametersTypes.Count!= lambdaNode.Parameters.Count)
                {
                    return null;
                }
                var i = 0;
                foreach (var p in parametersTypes)
                {
                    var resolvedType = p;
                    lambdaParamContext.Add(lambdaNode.Parameters[i], resolvedType);
                    i++;
                }
                ResolveInternal(lambdaNode.Body);
                return lambdaNode.Body.As<ITypedNode>().Type.AsCSharp;
            }
            catch (TypeResolverException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                foreach (var p in lambdaNode.Parameters)
                {
                    lambdaParamContext.Remove(p);
                }
            }
        }

        
        private List<MethodInfo> GetMethodCandidates(IEnumerable<MethodInfo> methods, List<OneOf<Type, WiredType>> passedParameters, MethodNode node)
        {
            var result = new List<MethodInfo>();
            foreach(var method in methods)
            {
                var goodMethod = true;
                
                var methodParameters = method.GetParameters();
                if (methodParameters.Length != passedParameters.Count){
                    continue;
                }
                foreach (var methodParam in methodParameters)
                {
                    var passedParam = passedParameters[methodParam.Position].AsT0;
                    if (methodParam.ParameterType.IsGenericParameter)
                    {
                        if (!methodParam.ParameterType.GetGenericParameterConstraints().Any())
                        {
                            continue;
                        }
                        goodMethod = false;
                        break;
                    }
                    else // non generic param
                    {
                        if (Extensions.IsTypesCompatible(methodParam.ParameterType, passedParam))
                        {
                            continue;
                        }
                        goodMethod = false;
                        break;
                    }
                }
                if (goodMethod)
                {
                    result.Add(method);
                }
            }
            return result;
        }
        private OneOf<Type, WiredType> GetTypeOfPropertyOrFieldOfName(Type type, string name)
        {
            var props = type.GetProperties().Where(x => x.Name == name).ToList();
            if (props.Count > 1)
            {
                throw new TypeResolverException("bad prop");
            }
            if (props.Count == 1)
            {
                return props.First().GetUnderlyingType();
            }
            var fields = type.GetFields().Where(x => x.Name == name).ToList();
            if (fields.Count > 1)
            {
                throw new TypeResolverException("bad prop");
            }
            if (fields.Count == 1)
            {
                return fields.First().GetUnderlyingType();
            }
            throw new TypeResolverException("field or prop not found");
        }

        private void InitializeContext(AppDomain appDomain, string[] additionalUsings = null)
        {
            var usings = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "System.Linq",
            };
            if (additionalUsings != null)
            {
                usings = usings.Concat(additionalUsings).ToList();
            }
            assemblies = appDomain.GetAssemblies().ToList();
            types = assemblies.SelectMany(x => x.GetTypes().Where(t=> usings.Contains(t.Namespace))).ToList();
            namespaces = types.Select(x => x.Namespace).Distinct().ToList();
        }

        internal bool IsTypesCompatible(List<Type> left, List<Type> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }
            for (int i = 0; i < left.Count; i++)
            {
                var lType = left[i];
                var rType = right[i];
                if (!Extensions.IsTypesCompatible(lType, rType))
                {
                    return false;
                }
            }
            return true;
        }

        protected internal bool TryTypesDistance(Type distanceFrom, Type distanceTo, out int outDistance)
        {
            outDistance = int.MaxValue;
            if (distanceFrom == null)
            {
                return false;
            }
            if (distanceFrom == distanceTo)
            {
                outDistance = 0;
                return true;
            }
            if (distanceTo.IsGenericType && distanceFrom.IsGenericType)
            {
                if (distanceFrom.GetGenericTypeDefinition() == distanceTo.GetGenericTypeDefinition())
                {
                    outDistance = 0;
                    return true;
                }
            }
            
            foreach (var @interface in distanceFrom.GetInterfaces())
            {
                var interfaceDistance = 0;
                if (TryTypesDistance(@interface, distanceTo, out interfaceDistance))
                {
                    if (outDistance > interfaceDistance)
                    {
                        outDistance = interfaceDistance;
                    }
                }
            }
            var classDistance = 0;
            if (TryTypesDistance(distanceFrom.BaseType, distanceTo, out classDistance))
            {
                if (outDistance > classDistance)
                {
                    outDistance = classDistance;
                }
            }
            if (outDistance == int.MaxValue)
            {
                return false;
            }
            outDistance++;
            return true;
        }
        
    }
}
