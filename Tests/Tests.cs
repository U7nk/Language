using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired;
using System.Diagnostics;
using Wired.Nodes;
using Wired.Compilers;
using System.Reflection;
using System.Linq.Expressions;
namespace Tests
{
    [TestGroup]
    public class Tests
    {
        private Dictionary<string, object> contextAvailableMembersDefault()
        {
            return new Dictionary<string, object>
            {
                { "$ctx", new TestContext() { Type = typeof(string), MemberName = "Length" } },
                { "foo", "system.runtimetype"},
                { "indx1", 1 },
                { "indx2", 2 },
                { "t", true },
            };
        }
        public static List<Token> Tokenize(string input)
        {
            return new Tokenizer(input).Tokenize();
        }

        public IEnumerable<object[]> EvaluatedCorrectTestData()
        {
            var ctx = new TestContext() 
            { 
                MemberName = "Tratata", 
                Type = typeof(bool),
                ListOfList = new List<List<int>> 
                { 
                    new List<int> {2, 22},
                    new List<int> {15, 24} 
                }, 
            };
            object expected = ctx.MemberName.Length > 1 ? ctx.MemberName.Substring(0, 2) : ctx.MemberName;
            var input = "$ctx.MemberName.Length > 1 ? $ctx.MemberName.Substring(0, 2) : $ctx.MemberName";
            yield return new object[] { ctx, expected, input, false, false};

            expected = ctx.MemberName.Length.CompareTo(2) > ctx.MemberName.Length.CompareTo(2) ? ctx.MemberName.Substring(0, 2) : ctx.MemberName;
            input = "$ctx.MemberName.Length.CompareTo(2) > $ctx.MemberName.Length.CompareTo(2) ? $ctx.MemberName.Substring(0, 2) : $ctx.MemberName";
            yield return new object[] { ctx, expected, input, false, false };

            expected = 5 > 2;
            input = "5 > 2";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5 - 2;
            input = "5 - 2";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5.0 - 2.5;
            input = "5.0 - 2.5";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5.0f - 2.5f;
            input = "5.0f - 2.5f";
            yield return new object[] { ctx, expected, input, false, false };
            //expected = 5 - 2.5; // TODO double не конвертится к инту в рантайме этот тест упадет
            //input = "5 - 2.5";
            //yield return new object[] { ctx, expected, input };
            expected = 10.Equals(ctx.MemberName.Length + 5 - 2);
            input = "10.Equals($ctx.MemberName.Length + 5 - 2)";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5 + 2 -1;
            input = "5 + 2 - 1";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5 - 2 + 5;
            input = "5 - 2 + 5";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5 - 2 + 5 * 2 + 10 / 2;
            input = "5 - 2 + 5 * 2 + 10 / 2";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.MemberName.Substring(5 - 2 + 5 * 2 + 10 / 2 - 17);
            input = "$ctx.MemberName.Substring(5 - 2 + 5 * 2 + 10 / 2 - 17)";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5 - 1 - (2 + 5);
            input = "5 - 1 - (2 + 5)";
            yield return new object[] { ctx, expected, input, false, false };
            expected = 5 - (2 + 5);
            input = "5 - (2 + 5)";
            yield return new object[] { ctx, expected, input, false, false };
            expected = (5 - 2) * 5;
            input = "(5 - 2) * 5";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.MemberName.Substring((4 - 2) * 3);
            input = "$ctx.MemberName.Substring((4 - 2) * 3)";
            yield return new object[] { ctx, expected, input, false, false };
            expected = (ctx.ToString().Length + 4).ToString();
            input = "($ctx.ToString().Length + 4).ToString()";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.ToString().Where((x) => x.ToString().Equals( "T" )).ToList().Count;
            input =  "$ctx.ToString().Where((x) => x.ToString().Equals(\"T\")).ToList().Count";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.MemberName.Last().ObjToList().First().GetType().IsGenericType ? ctx.MemberName.Length.ToString() : ctx.Type.Assembly.FullName.ToString();
            input = "$ctx.MemberName.Last().ObjToList().First().GetType().IsGenericType ? $ctx.MemberName.Length.ToString() : $ctx.Type.Assembly.FullName.ToString()";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.MemberName.Equals(ctx.Type.IsAbstract ? "true" : ctx.MemberName);
            input = "$ctx.MemberName.Equals($ctx.Type.IsAbstract ? \"true\" : $ctx.MemberName)";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.ListOfList.Select((x) => x.Where((inner) => inner > 20)).Sum((x) => x.Sum());
            input = "$ctx.ListOfList.Select((x) => x.Where((inner) => inner > 20)).Sum((x) => x.Sum())";
            yield return new object[] { ctx, expected, input, false, false };
            expected = ctx.ListOfList.Select(x => x.Where(inner => inner > 20)).Sum(x => x.Sum());
            input = "$ctx.ListOfList.Select(x => x.Where(inner => inner > 20)).Sum(x => x.Sum())";
            yield return new object[] { ctx, expected, input, false, false };
        }

        [Test]
        [TestData("EvaluatedCorrectTestData")]
        public void EvaluatedCorrect(TestContext ctx, object expected, string input, bool draw = false, bool withTypes = false)
        {
            Console.WriteLine("    " + input);
            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new WiredCompiler(new Dictionary<string, object> { { "$ctx", ctx } });
            if (draw && !withTypes)
            {
                new ASTDrawer(withTypes).OpenImage(ast);
            }
            
            new TypeResolver(AppDomain.CurrentDomain, new[] { "Wired" }).Resolve(ref ast, ctx.GetType());

            if (draw && withTypes)
            {
                new ASTDrawer(withTypes).OpenImage(ast);
            }
            var result = sut.Compile(ast).Invoke();
            result.ShouldBe(expected);
        }

        public IEnumerable<object[]> EvaluatedCorrectTestData2()
        {
            var ctx = new TestContext() { MemberName = "Tratata", Type = typeof(bool), ListOfList = new List<List<int>> { new List<int> { 2, 22 } } };
            object expected = ctx.MemberName.ToString().Where((x) => x.Equals(x) ? x.Equals(ctx) : true).ToList().Count;
            var input = "$ctx.MemberName.ToString().Where((x) => x.Equals(x) ? x.Equals($ctx) : true).ToList().Count";
            var check = new Action<AST,object>((ast, result) => {
                result.ShouldBe(expected);
            });
            yield return new object[] { ctx, check, input };
            expected = ctx.FuncTwoParams(5, (f, s) => s > f, 66);
            input = "$ctx.FuncTwoParams(5, (f, s) => s > f, 66)";
            check = new Action<AST, object>((ast, result) =>
            {
                result.ShouldBe(expected);
            });
            yield return new object[] { ctx, check, input };
            expected = ctx.FuncTwoParams(5, (f, s) => s.ToString().Length > f.ToString().Length, 66);
            input = "$ctx.FuncTwoParams(5, (f, s) => s.ToString().Length > f.ToString().Length, 66)";
            check = new Action<AST, object>((ast, result) =>
            {
                result.ShouldBe(expected);
            });
            yield return new object[] { ctx, check, input };
            expected = System.Int32.MaxValue;
            input = "System.Int32.MaxValue";
            check = new Action<AST, object>((ast, result) => {
                result.ShouldBe(expected);
            });
            yield return new object[] { ctx, check, input };
            expected = System.Int32.Parse("69");
            input = "System.Int32.Parse(\"69\")";
            check = new Action<AST, object>((ast, result) =>
            {
                result.ShouldBe(expected);
            });
            yield return new object[] { ctx, check, input };
        }
        [Test]
        [TestData("EvaluatedCorrectTestData2")]
        public void EvaluatedCorrect(TestContext ctx, Action<AST, object> check, string input)
        {
            Console.WriteLine("    " + input);
            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new WiredCompiler(new Dictionary<string, object> { { "$ctx", ctx } });
            new TypeResolver(AppDomain.CurrentDomain, new[] { "Wired" }).Resolve(ref ast, ctx.GetType());
            var result = sut.Compile(ast).Invoke();
            check(ast, result);
        }

        [Test]
        public void TypeResolverTest1()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };
 
            var input = "$ctx.MemberName.Length";

            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain);
            sut.Resolve(ref ast, ctx.GetType());

            var root = ast.Root.As<BinaryNode>();
            root.Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(TestContext));
            root.Right.As<BinaryNode>().Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(string));
            root.Right.As<BinaryNode>().Right.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(int));

        }

        [Test]
        public void TypeResolverTest2()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };

            var input = "$ctx.Type.ToString()";

            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain);
            sut.Resolve(ref ast, ctx.GetType());

            var root = ast.Root.As<BinaryNode>();
            root.Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(TestContext));
            root.Right.As<BinaryNode>().Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(Type));
            root.Right.As<BinaryNode>().Right.As<MethodNode>().ReturnType.AsCSharp.ShouldBe(typeof(string));
        }

        [Test]
        public void TypeResolverTest3()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };

            var input = "$ctx.OverloadedMethod(\"string overload\".ToString())";

            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain);
            sut.Resolve(ref ast, ctx.GetType());

            var root = ast.Root.As<BinaryNode>();
            root.Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(TestContext));
            root.Right.As<MethodNode>().ReturnType.AsCSharp.ShouldBe(typeof(string));
            root.Right.As<MethodNode>().Parameters.Single().As<BinaryNode>().Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(string));
            root.Right.As<MethodNode>().Parameters.Single().As<BinaryNode>().Right.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(string));
        }

        [Test]
        public void TypeResolverTest4()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };

            var input = "$ctx.OverloadedMethod(4)";

            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain);
            sut.Resolve(ref ast, ctx.GetType());

            var root = ast.Root.As<BinaryNode>();
            root.Left.As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(TestContext));
            root.Right.As<MethodNode>().ReturnType.AsCSharp.ShouldBe(typeof(int));
            root.Right.As<MethodNode>().Parameters.Single().As<ITypedNode>().Type.AsCSharp.ShouldBe(typeof(int));
        }

        [Test]
        public void TypeResolverTest5()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };

            var input = "$ctx.OverloadedMethod()";
            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain);
            new Action(() =>
            {
                sut.Resolve(ref ast, ctx.GetType());
            })
            .ShouldThrow<Exception>();
        }
        [Test]
        public void TypeResolverTest6()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };
            var input = "$ctx.ObjToList()";
            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain, new []{ "Wired.Extensions", "Wired" } );
            sut.Resolve(ref ast, ctx.GetType());

            var root = ast.Root.As<BinaryNode>();
            root.Left.As<CtxKeywordNode>().Type.AsCSharp.ShouldBe(typeof(TestContext));
            root.Right.As<MethodNode>().ReturnType.AsCSharp.ShouldBe(typeof(List<TestContext>)); 
        }

        [Test]
        public void TypeOrNamespaceParserTest()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };
            var input = "System.Int32";
            
            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));
            ast.Root.As<TypeOrNamespaceNode>().Parts[0].As<IdNode>().Name.ShouldBe("System");
            ast.Root.As<TypeOrNamespaceNode>().Parts[1].As<IdNode>().Name.ShouldBe("Int32");
        }

        [Test]
        public void TypeOrNamespaceParserTest2()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };
            var input = "System.Int32.Property.Method()";
            
            var ast = new WiredParser(ctx.GetType()).Parse(Tokenize(input));

            var root = ast.Root.As<PropertyNode>();
            root.Left.As<TypeOrNamespaceNode>().Parts[0].As<IdNode>().Name.ShouldBe("System");
            root.Left.As<TypeOrNamespaceNode>().Parts[1].As<IdNode>().Name.ShouldBe("Int32");
            root.Left.As<TypeOrNamespaceNode>().Parts[2].As<IdNode>().Name.ShouldBe("Property");
            root.Right.As<MethodNode>().MethodName.ShouldBe("Method");
        }
    }
}
