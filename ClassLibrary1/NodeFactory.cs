using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired.Nodes;

namespace Wired
{
    public class NodeFactory
    {
        public NodeFactory()
        {
        }

        public PropertyNode MethodCall(Nodes.Node curNode, string name, List<Nodes.Node> arguments)
        {
            var methodNode = new MethodNode(name, arguments);
            var propertyNode = new PropertyNode(curNode, methodNode);
            curNode.Parent = propertyNode;
            methodNode.Parent = propertyNode;
            return propertyNode;
        }

        public PropertyNode PropertyCall(Node curNode, string name)
        {
            var idNode = new IdNode(name);
            var propertyNode =new PropertyNode(
               curNode, 
               idNode);
            curNode.Parent = propertyNode;
            idNode.Parent = propertyNode;
            return propertyNode;
        }
    }
}
