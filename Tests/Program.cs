
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Tests
{
    [Serializable]
    public class AssertException : Exception
    {
        public AssertException() { }
        public AssertException(string message) : base(message) { }
        public AssertException(string message, Exception inner) : base(message, inner) { }
        protected AssertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public static class Assertions
    {
        public static void ShouldBe(this object real, object expected)
        {
            if (!real.Equals(expected))
            {
                throw new AssertException(real + " not equal to " + expected);
            }
        }
        public static void ShouldBeNot(this object real, object expected)
        {
            if (real.Equals(expected))
            {
                throw new AssertException(real + " equal to " + expected);
            }
        }
        public static void ShouldThrow<T>(this Action action)
        {
            Exception exception = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (exception == null || (exception.GetType() != typeof(T) && !typeof(T).IsAssignableFrom(exception.GetType())))
            {
                throw new AssertException("action should throw " + typeof(T) + " but no exceptions encountered.");
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TestGroupAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TestDataAttribute : Attribute
    {
        public TestDataAttribute(string testDataMethodName)
        {
            this.TestDataMethodName = testDataMethodName;
        }

        public string TestDataMethodName { get; set; }
    }

    public class TestGroup
    {
        public Type Type { get; set; }
        public List<MethodInfo> TestMethods { get; set; }
        public TestGroup()
        {
            TestMethods = new List<MethodInfo>();
        }
    }
    public class TestRunner
    {
        static TestRunner()
        {
            defaultConsoleWriter = Console.Out;
        }
        private List<TestGroup> tests = new List<TestGroup>();
        public TestRunner()
        {
            foreach (var testClass in typeof(TestRunner).Assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(TestGroupAttribute), false).Length > 0))
            {
                var testGroup = new TestGroup { Type = testClass };
                foreach (var test in testClass.GetMethods().Where(x => x.GetCustomAttributes(typeof(TestAttribute), false).Length > 0))
                {
                    testGroup.TestMethods.Add(test);
                }
                tests.Add(testGroup);
            }
        }
        private void Warmup()
        {
            DisableConsoleOut();
            foreach (var testGroup in tests)
            {
                var testClass = testGroup.Type.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes);
                foreach (var test in testGroup.TestMethods)
                {

                    if (test.GetCustomAttributes(typeof(TestDataAttribute), false).Length > 0)
                    {
                        var testDataAttribute = (TestDataAttribute)test.GetCustomAttributes(typeof(TestDataAttribute), false).First();
                        var testData = (IEnumerable<object[]>)testClass.GetType().GetMethod(testDataAttribute.TestDataMethodName).Invoke(testClass, Type.EmptyTypes);
                        var counter = 1;
                        foreach (var data in testData)
                        {
                            try
                            {
                                test.Invoke(testClass, data);
                            }
                            catch
                            {
                            }
                            counter++;
                        }
                        continue;
                    }
                    try
                    {
                        test.Invoke(testClass, Type.EmptyTypes);
                    }
                    catch { }
                }
            }
            EnableConsoleOut();
            Console.WriteLine("Warmed up");
        }
        public class NullTextWriter : TextWriter
        {

            public override Encoding Encoding
            {
                get { return ASCIIEncoding.ASCII; }
            }
        }

        private static TextWriter defaultConsoleWriter;
        public static void DisableConsoleOut()
        {
            Console.SetOut(new NullTextWriter());
        }
        public static void EnableConsoleOut()
        {
            Console.SetOut(defaultConsoleWriter);
        }
        [DebuggerStepThrough]
        public void Run()
        {
            EnableConsoleOut();
            foreach (var testGroup in tests)
            {
                Console.WriteLine(testGroup.Type.Name + " results: ");
                var testClass = testGroup.Type.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes);
                foreach (var test in testGroup.TestMethods)
                {

                    if (test.GetCustomAttributes(typeof(TestDataAttribute), false).Length > 0)
                    {
                        var testDataAttribute = (TestDataAttribute)test.GetCustomAttributes(typeof(TestDataAttribute), false).First();
                        var testData = (IEnumerable<object[]>)testClass.GetType().GetMethod(testDataAttribute.TestDataMethodName).Invoke(testClass, Type.EmptyTypes);
                        Console.WriteLine("  " + test.Name + " with custom test data");
                        var counter = 1;
                        foreach (var data in testData)
                        {
                            try
                            {
                                test.Invoke(testClass, data);
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("    v");
                            }
                            catch (AssertException ex)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("    " + counter + " assertion failed : " + ex.Message);
                            }
                            catch (Exception)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("    " + counter + " : ");
                            }
                            counter++;
                            Console.ResetColor();
                        }
                        continue;
                    }
                    try
                    {
                        test.Invoke(testClass, Type.EmptyTypes);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("  " + test.Name);
                    }
                    catch (AssertException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("  " + test.Name + " assertion failed : " + ex.Message);
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("  " + test.Name + " : ");
                    }
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }

        private void Timings()
        {
            var times = 1000;
            foreach (var testGroup in tests)
            {
                EnableConsoleOut();
                Console.WriteLine(testGroup.Type.Name + " timings on " + times + ": ");
                DisableConsoleOut();
                var testClass = testGroup.Type.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes);
                foreach (var test in testGroup.TestMethods)
                {

                    if (test.GetCustomAttributes(typeof(TestDataAttribute), false).Length > 0)
                    {
                        var testDataAttribute = (TestDataAttribute)test.GetCustomAttributes(typeof(TestDataAttribute), false).First();
                        var testData = (IEnumerable<object[]>)testClass.GetType().GetMethod(testDataAttribute.TestDataMethodName).Invoke(testClass, Type.EmptyTypes);
                        EnableConsoleOut();
                        Console.WriteLine("  " + test.Name + " with custom test data");
                        DisableConsoleOut();
                        var counter = 1;
                        foreach (var data in testData)
                        {
                            try
                            {

                                long total = 0;
                                for (int i = 0; i < times; i++)
                                {
                                    var sw = Stopwatch.StartNew();
                                    test.Invoke(testClass, data);
                                    total += sw.ElapsedTicks;
                                }

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                EnableConsoleOut();
                                Console.WriteLine("    " + counter + " in " + total / times + "ticks");
                                DisableConsoleOut();
                            }
                            catch (AssertException ex)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                EnableConsoleOut();
                                Console.WriteLine("    " + counter + " assertion failed : " + ex.Message);
                                DisableConsoleOut();
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                EnableConsoleOut();
                                Console.WriteLine("    " + counter + " : " + ex);
                                DisableConsoleOut();
                            }
                            counter++;
                            Console.ResetColor();
                        }
                        continue;
                    }
                    try
                    {
                        long total = 0;
                        for (int i = 0; i < times; i++)
                        {
                            var sw = Stopwatch.StartNew();
                            test.Invoke(testClass, Type.EmptyTypes);
                            total += sw.ElapsedTicks;
                        }
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        EnableConsoleOut();
                        Console.WriteLine("  " + test.Name + " in " + total / times + "ticks");
                        DisableConsoleOut();
                    }
                    catch (AssertException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        EnableConsoleOut();
                        Console.WriteLine("  " + test.Name + " assertion failed : " + ex.Message);
                        DisableConsoleOut();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        EnableConsoleOut();
                        Console.WriteLine("  " + test.Name + " : " + ex);
                        DisableConsoleOut();
                    }
                    Console.ResetColor();
                }
            }
            EnableConsoleOut();
            Console.WriteLine();
            DisableConsoleOut();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var runner = new TestRunner();
            runner.Run();
            new ExtensionsTests().TypeDistanceTest1();
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}
