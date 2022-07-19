using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired;
using System.Diagnostics;
using System.IO;
using Wired.Nodes;

namespace Tests
{
    public class ASTDrawer
    {
        private readonly string JarPath = @"C:\PlantUml\plantuml.jar";
        private readonly string CodesPath = @"C:\PlantUml\codes\";
        private ITyper typer;
        public ASTDrawer(bool addTypes) 
        {
            if (addTypes)
            {
                typer = new DefaultTyper();
            }
            else
            {
                typer = new NullTyper();
            }
        }
        public void OpenImage(AST ast) 
        {
            var codePath = CodesPath + "puml.puml";
            File.WriteAllText(codePath, CreateCode(ast));
            Launch(codePath);
        }

        private void Launch(string umlFilePath)
        {
            Process.Start("cmd.exe", "/C java -jar " + JarPath + " " + umlFilePath).WaitForExit();
            Process.Start("cmd.exe", "/C "+ Path.ChangeExtension(umlFilePath,".png"));
        }
        private string CreateCode(AST ast)
        {
            var template = "@startuml\n{0}\n@enduml";
            var root = CreateCodeInternal(ast.Root);
            return string.Format(template, root);
        }

        private string CreateCodeInternal(Node root)
        {
            var result = "";
            if (root is BinaryNode)
            {
                var binary = root.As<BinaryNode>();
                var op = "";
                if (binary is PropertyNode)
                {
                    op = " dot ";
                }
                else if (binary is MinusNode)
                {
                    op = " - ";
                }
                else if (binary is PlusNode)
                {
                    op = " + ";
                }
                else if (binary is MulNode)
                {
                    op = " * ";
                }
                else if (binary is DivNode)
                {
                    op = " / ";
                }
                else if (binary is BinaryGreaterNode)
                {
                    op = " > ";
                }
                result += ":" + op + typer.GetTypeName(binary) + "; \n split \n" + CreateCodeInternal(binary.Left) + " \n split again \n " + CreateCodeInternal(binary.Right) + " \n end split \n";
            }
            else if (root is CtxKeywordNode)
            {
                result += ":context" + typer.GetTypeName(root) + ";\n kill";
            }
            else if (root is MethodNode)
            {
                var methodNode = root.As<MethodNode>();
                var args = "";
                result += ":m - " + methodNode.MethodName + typer.GetTypeName(root) + ";\n";
                if (methodNode.Parameters.Count == 0)
                {
                    result += "\nkill";
                    return result;
                }
                if (methodNode.Parameters.Count == 1)
                {
                    result += CreateCodeInternal(methodNode.Parameters.First());
                    return result;
                }
                args += "\n split \n";
                foreach (var argument in methodNode.Parameters)
                {
                    args += CreateCodeInternal(argument);
                   
                    if (argument != methodNode.Parameters.Last())
                    {
                        args += "\n split again \n";
                    }
                }
                args += "\n end split \n";
                result += args;
                result += "kill";
            }
            else if (root is IdNode)
            {
                result += ":" + root.As<IdNode>().Name + typer.GetTypeName(root) + ";";
                result += "\nkill";
            }
            else if (root is LiteralNode)
            {
                result += ":" + root.As<LiteralNode>().LiteralValue + " : " + typer.GetTypeName(root) + ";";
                result += "\nkill";
            }
            else if (root is LambdaNode)
            {
                var lambda = root.As<LambdaNode>();
                result += ":lambda" + typer.GetTypeName(lambda) + "; \n split \n";
                var args = ":params;";
                args += "\n split \n";
                if (lambda.Parameters.Count == 0)
                {
                    
                    args += ":no params;";
                    args += "\nkill";
                }
                foreach (var param in lambda.Parameters)
                {
                    args += CreateCodeInternal(param);
                    if (param != lambda.Parameters.Last())
                    {
                        args += "\n split again \n";
                    }
                }
                args += "\n end split \n";
                result += args;
                result += "\n split again \n :body; \n";
                result += CreateCodeInternal(lambda.Body) + "\n end split \n";
            }
            else if (root is LambdaParamNode)
            {
                var lambdaParam = root.As<LambdaParamNode>();
                result += ":" + lambdaParam.Name + typer.GetTypeName(lambdaParam) + ";";
                result += "\nkill";
            }
            return result;
        }
    }
    public class DefaultTyper : ITyper
    {
        public string GetTypeName(Node node)
        {
            if (node.As<ITypedNode>().Type != null && node.As<ITypedNode>().Type.IsT0)
            {
                return " : " + node.As<ITypedNode>().Type.AsT0.Name;
            }
            return " : no*";
        }
    }
    public class NullTyper : ITyper
    {
        public string GetTypeName(Node node)
        {
            return "";
        }
    }
    public interface ITyper
    {
        string GetTypeName(Node node);
    }
}
