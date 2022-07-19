using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired.Nodes;
using Wired.Exceptions;

namespace Wired
{
    public static class NodeHelp
    {
        public static void RearrangeExtensionMethodCall(MethodNode methodNode, AST ast)
        {
            /*
             *      binary
             *      /   \           ---->    ExtMethod(param)
             *   param  ExtMethod()
             */
            if (methodNode.Parent.Parent == null)
            {
                var binaryTop = Node.GetTopParent(methodNode);
                var astReplacer = new ReplaceNodeVisitor(binaryTop, methodNode);
                var parameter = methodNode.Parent.As<PropertyNode>().Left;
                parameter.Parent = null;
                methodNode.Parameters.Insert(0, parameter);
                astReplacer.Replace(ast);
                return;
            }
            /*
             *   binaryTop
             *   /   \
             *  x   binary         ---->  ExtMethod(  *  )
             *      /   \                           /   \
             *     y   ExtMethod()                 x     y
             */
            if (methodNode.Parent.Parent != null &&
                methodNode.Parent.As<BinaryNode>().Right == methodNode)
            {
                var binaryTop = Node.GetTopParent(methodNode);
                var parentParent = methodNode.Parent.Parent.As<PropertyNode>();
                var binary = methodNode.Parent.As<PropertyNode>();
                var y = binary.Left;
                parentParent.Right = y;
                y.Parent = parentParent;

                var astReplacer = new ReplaceNodeVisitor(binaryTop, methodNode);
                methodNode.Parameters.Insert(0, binaryTop);
                astReplacer.Replace(ast);
                return;
            }
            /*
             *       binaryTop                         binary
             *        /   \                            /   \ 
             *       x   binary         ----> ExtMethod(x)  y
             *           /   \                             
             *   ExtMethod()  y                      
             */
            if (methodNode.Parent.Parent.Parent == null &&
                methodNode.Parent.As<BinaryNode>().Left == methodNode)
            {
                var binaryTop = Node.GetTopParent(methodNode).As<PropertyNode>();
                var x = binaryTop.Left;
                var binary = methodNode.Parent.As<BinaryNode>();
                binary.Parent = null;
                x.Parent = null;
                methodNode.Parameters.Insert(0, x);
                var astReplacer = new ReplaceNodeVisitor(binaryTop, binary);
                astReplacer.Replace(ast);
                return;
            }
            /*
             *    binaryTop
             *    /     \
             *   x   binaryParent                               binary
             *        /   \                                     /   \ 
             *       y   binary          ----> ExtMethod(binaryTop)  z
             *           /   \                           /     \  
             *   ExtMethod()  z                         x       y
             */
            if (methodNode.Parent.Parent.Parent != null &&
                methodNode.Parent.As<BinaryNode>().Left == methodNode)
            {
                var binaryTop = Node.GetTopParent(methodNode).As<PropertyNode>();
                var binary = methodNode.Parent.As<BinaryNode>();
                var binaryParent = binary.Parent.As<BinaryNode>();
                var binaryParentParent = binaryParent.Parent.As<BinaryNode>();
                binaryParentParent.Right = binaryParent.Left;
                binaryParent.Left.Parent = binaryParentParent;
                methodNode.Parameters.Insert(0, binaryTop);
                var astReplacer = new ReplaceNodeVisitor(binaryTop, binary);
                astReplacer.Replace(ast);
                return;
            }
            throw new WiredException("Unexpected graph");
        }
    }
}
