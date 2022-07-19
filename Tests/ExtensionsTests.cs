using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wired;
using System.Collections;
using Wired.Nodes;

namespace Tests
{
    [TestGroup]
    public class ExtensionsTests
    {
        [Test]
        public void CutTail()
        {
            var str = "aaa.";
            var expected = "aaa";


            var res = str.CutTail(1);
            res.ShouldBe(expected);
        }

        [Test]
        public void TypeDistanceTest1()
        {
            var tFrom = typeof(string);
            var tTo = typeof(IEnumerable<char>);
            var sut = new TypeResolver(AppDomain.CurrentDomain);
            
            var distance = -2;
            var result = sut.TryTypesDistance(tFrom, tTo, out distance);

            result.ShouldBe(true);
            distance.ShouldBe(1);
        }

        [Test]
        public void TypeDistanceTest2()
        {
            var tFrom = typeof(string);
            var tTo = typeof(IEnumerable);
            var sut = new TypeResolver(AppDomain.CurrentDomain);

            var distance = -2;
            var result = sut.TryTypesDistance(tFrom, tTo, out distance);

            result.ShouldBe(true);
            distance.ShouldBe(1);
        }
        [Test]
        public void ASTDrawerManualTest()
        {
            var ctx = new TestContext { MemberName = "boo boo ba", Type = typeof(string) };
            var input = "$ctx.MemberName.Substring((4 - 2) * 3)";
            var ast = new WiredParser(ctx.GetType()).Parse(Tests.Tokenize(input));
            var sut = new TypeResolver(AppDomain.CurrentDomain, new[] { "Wired.Extensions", "Wired" });
            sut.Resolve(ref ast, ctx.GetType());

            //new ASTDrawer(true).OpenImage(ast);
        }

    }
}
