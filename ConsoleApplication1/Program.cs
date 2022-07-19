using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ConsoleApplication1
{

    public class TestContext 
    {
        public Type Type { get; set; }
        public string MemberName { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new TestContext() { Type = typeof(string), MemberName = "Length" };
            var input = "ctx.Type.GetMember(ctx.MemberName)".ToString();

            var propertyInput = "foo.Substring(t ? indx1 : indx2, indx2)";
            var tokens = new Tokenizer(propertyInput).Tokenize();
            /*Console.WriteLine("Tokens:");
            foreach (var tok in tokens)
            {
                Console.WriteLine(tok.Type + " : " + tok.StringValue);
            }*/
            EvaluationFrame.Keywords = new Dictionary<string, object>
            {
                { "ctx", ctx },
                { "foo", "system.runtimetype"},
                { "indx1", 1 },
                { "indx2", 2 },
                { "t", true },
                { "ff", new Func<char, bool>((ch) => { if (ch == 's'){ return true; } return false; }) }
            };
            Console.WriteLine("Evaluation result:");
            var parser = new Parser(tokens);
            var sw = Stopwatch.StartNew();
            var res = parser.Parse().Invoke();
            Console.WriteLine(res);
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            Console.ReadLine();
        }
    }

    public class EvaluationFrame
    {
        public static Dictionary<string, object> Keywords;
        public static Dictionary<string, object> Evaluated = new Dictionary<string, object>();
        public List<Token> Stack { get; set; }
        public EvaluationFrame()
        {
            this.Stack = new List<Token>();
        }
        private static int counter;
        public static int GenerateNextId()
        {
            counter++;
            return counter;
        }
        public Func<object> Evaluate()
        {
            if (Stack.Count == 0)
            {
                return null;
            }
            object evaluatedObject = null;
            Token curTok = null;
            for (int i = 0; i < Stack.Count; i++)
            {
                var tok = Stack[i];
                if (tok.Type == TokenType.Id)
                {
                    if (curTok == null)
                    {
                        curTok = tok;
                        evaluatedObject = Keywords[tok.StringValue];
                        continue;
                    }
                    if (i < Stack.Count - 1 && Stack[i + 1].Type == TokenType.OpFrame)
                    {
                        curTok = tok;
                        continue;
                    }
                    if (curTok.Type == TokenType.Dot)
                    {
                        var prop = evaluatedObject.GetType().GetProperty(tok.StringValue);
                        if (prop != null)
                        {
                            evaluatedObject = prop.GetGetMethod().Invoke(evaluatedObject, new object[] { });
                            continue;
                        }
                        var field = evaluatedObject.GetType().GetField(tok.StringValue);
                        if (field != null)
                        {
                            evaluatedObject = field.GetValue(evaluatedObject);
                            continue;
                        }
                        throw new Exception("Member " + tok.StringValue + " not found on type " + evaluatedObject.GetType());
                    }

                    continue;
                }
                if (tok.Type == TokenType.OpFrame)
                {
                    if (curTok.Type == TokenType.Id) 
                    {
                        var parameters = new List<object>();
                        i++;
                        tok = Stack[i];
                        while (tok.Type != TokenType.ClFrame)
                        {
                            if (tok.Type != TokenType.Id)
                            {
                                throw new Exception("unexpected parameter");
                            }
                            if (Keywords.ContainsKey(tok.StringValue))
                            {
                                parameters.Add(Keywords[tok.StringValue]);
                            }
                            else if (Evaluated.ContainsKey(tok.StringValue))
                            {
                                parameters.Add(Evaluated[tok.StringValue]);
                            }
                            else 
                            {
                                throw new Exception("undefined parameter");
                            }
                            i++;
                            tok = Stack[i];
                        }
                        var meth  = evaluatedObject.GetType().GetMethod(curTok.StringValue, parameters.Select(x => x.GetType()).ToArray());
                        evaluatedObject = meth.Invoke(evaluatedObject,parameters.ToArray());
                        continue;
                    }
                    throw new Exception();
                }
                if (tok.Type == TokenType.Dot)
                {
                    curTok = tok;
                    continue;
                }
            }
            return () => evaluatedObject;
        }
    }

    public class Parser
    {
        public Parser(List<Token> input)
        {
            this.input = input;
            this.stack = new Stack<Token>();
            evaluationStack = new Stack<EvaluationFrame>();
            evaluationStack.Push(new EvaluationFrame());
            state = State.Start;
        }

        enum State
        {
            Start,
            First,
            Second,
            Third,
            Fourth,
            Accept,
            Five,
        }
        private readonly List<Token> input;
        private readonly Stack<Token> stack;
        private readonly Stack<EvaluationFrame> evaluationStack;
        private State state;
        private int curIndex;

        private EvaluationFrame CurrentFrame
        {
            get
            {
                return this.evaluationStack.Peek();
            }
        }
        private Func<object> result;

        public Func<object> Parse()
        {
            curIndex = 0;
            for ( ;curIndex < input.Count; curIndex++)
            {
                var tok = input[curIndex];
                Transit(tok);
            }
            return result;
        }

        private void Transit(Token tok)
        {
            if (state == State.Start)
            {
                TransitFromStart(tok);
                return;
            }
            if (state == State.First)
            {
                TransitFromFirst(tok);
                return;
            }
            if (state == State.Second)
            {
                TransitFromSecond(tok);
                return;
            }
            if (state == State.Third)
            {
                TransitFromThird(tok);
                return;
            }
            if (state == State.Fourth) 
            {
                TransitFromFourth(tok);
                return;
            }

        }

        private void TransitFromFourth(Token tok)
        {
            if (tok.Type == TokenType.Id)
            {
                CurrentFrame.Stack.Add(tok);
                state = State.First;
                return;
            }
            throw new Exception();
        }

        private void TransitFromThird(Token tok)
        {
            if (tok.Type == TokenType.ClFrame)
            {
                if (stack.Peek().Type == TokenType.OpFrame)
                {
                    var evRes = CurrentFrame.Evaluate();
                    evaluationStack.Pop();
                    if (evRes != null)
                    {
                        var evTok = new Token(TokenType.Id, EvaluationFrame.GenerateNextId().ToString());
                        EvaluationFrame.Evaluated.Add(evTok.StringValue, evRes());
                        CurrentFrame.Stack.Add(evTok);
                    }

                    CurrentFrame.Stack.Add(new Token(TokenType.ClFrame, ")"));
                    stack.Pop();
                    state = State.First;
                    return;
                }
                throw new Exception("unexpected close frame");
            }
            if (tok.Type == TokenType.Id)
            {
                CurrentFrame.Stack.Add(tok);
                state = State.First;
                return;
            }
            throw new Exception("params not supported");
        }

        private void TransitFromSecond(Token tok)
        {
            if (tok.Type == TokenType.Id)
            {
                CurrentFrame.Stack.Add(tok);
                state = State.First;
                return;
            }
            if (tok.Type == TokenType.EOF)
            {
                if (evaluationStack.Count == 1 && stack.Count == 0)
                {
                    state = State.Accept;
                    result = CurrentFrame.Evaluate();
                    return;
                }
                throw new Exception();
            }
            throw new Exception();
        }

        private void TransitFromFirst(Token tok)
        {
            if (tok.Type == TokenType.Dot)
            {
                CurrentFrame.Stack.Add(tok);
                state = State.Second;
                return;
            }
            if (tok.Type == TokenType.OpFrame)
            {
                this.CurrentFrame.Stack.Add(tok);
                this.evaluationStack.Push(new EvaluationFrame());
                this.stack.Push(tok);
                state = State.Third;
                return;
            }
            if (tok.Type == TokenType.ClFrame)
            {
                if (stack.Peek().Type == TokenType.OpFrame)
                {
                    var evRes = CurrentFrame.Evaluate();
                    evaluationStack.Pop();
                    if (evRes != null)
                    {
                        var evTok = new Token(TokenType.Id, EvaluationFrame.GenerateNextId().ToString());
                        EvaluationFrame.Evaluated.Add(evTok.StringValue, evRes());
                        CurrentFrame.Stack.Add(evTok);
                    }

                    CurrentFrame.Stack.Add(new Token(TokenType.ClFrame, ")"));
                    stack.Pop();
                    return;
                }
            }
            if (tok.Type == TokenType.Comma)
            {
                if (stack.Peek().Type == TokenType.OpFrame)
                {
                    var evRes = CurrentFrame.Evaluate().Invoke();
                    evaluationStack.Pop();
                    if (evRes == null)
                    {
                        throw new Exception();
                    }
                    var evTok = new Token(TokenType.Id, EvaluationFrame.GenerateNextId().ToString());
                    EvaluationFrame.Evaluated.Add(evTok.StringValue, evRes);
                    CurrentFrame.Stack.Add(evTok);
                    evaluationStack.Push(new EvaluationFrame());

                    state = State.Fourth;
                    return;
                }
                throw new Exception();
            }
            if (tok.Type == TokenType.Question)
            {
                var evRes = (bool)CurrentFrame.Evaluate().Invoke();
                
                state = State.Second;
                evaluationStack.Pop();
                evaluationStack.Push(new EvaluationFrame());
                if (evRes)
                {
                    curIndex++;
                    var curTok = input[curIndex];
                    while (curTok.Type != TokenType.Colon)
                    {
                        Transit(curTok);
                        curIndex++;
                        curTok = input[curIndex];
                    }
                    curIndex++;
                    curTok = input[curIndex];
                    var localStack = new Stack<Token>();
                    while (curTok.Type != TokenType.EOF && curTok.Type != TokenType.Colon && curTok.Type != TokenType.Comma)
                    {
                        if (curTok.Type == TokenType.OpFrame)
                        {
                            localStack.Push(curTok);
                        }
                        else if (curTok.Type == TokenType.ClFrame)
                        {
                            if (localStack.Count == 0 || localStack.Peek().Type != TokenType.OpFrame)
                            {
                                break;
                            }
                            else if (localStack.Peek().Type == TokenType.OpFrame)
                            {
                                localStack.Pop();
                            }
                        }
                        curIndex++;
                        curTok = input[curIndex];
                    }
                    curIndex--;
                    return;
                }
                else
                {
                    curIndex++;
                    var curTok = input[curIndex];
                    bool endOfBlock = false;
                    var localStack = new Stack<Token>();
                    while (!endOfBlock)
                    {
                        if (curTok.Type == TokenType.Question)
                        {
                            localStack.Push(curTok);
                        }
                        else if (curTok.Type == TokenType.Colon)
                        {
                            if (localStack.Count == 0 || localStack.Peek().Type != TokenType.Question)
                            {
                                endOfBlock = true;
                            }
                            else if (stack.Peek().Type == TokenType.Question)
                            {
                                localStack.Pop();
                            }
                        }
                        curIndex++;
                        curTok = input[curIndex];
                    }

                    while (curTok.Type != TokenType.ClFrame && curTok.Type != TokenType.EOF && curTok.Type != TokenType.Colon)
                    {
                        Transit(curTok);
                        curIndex++;
                        curTok = input[curIndex];
                    }
                    curIndex--;
                    
                    return;
                }
            }
            if (tok.Type == TokenType.EOF) 
            {
                if (stack.Count == 0 && evaluationStack.Count == 1)
                {
                    result = CurrentFrame.Evaluate();
                    state = State.Accept;
                    return;
                }
                throw new Exception();
            }
            throw new Exception();
        }

        private void TransitFromStart(Token tok)
        {
            if (tok.Type == TokenType.Id)
            {
                CurrentFrame.Stack.Add(tok);
                state = State.First;
                return;
            }
            throw new Exception();
        }
    }

    public enum TokenType
    {
        Id,
        OpFrame,
        ClFrame,
        Dot,
        Comma,
        Question,
        Colon,
        EOF,
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string StringValue { get; set; }
        public Token(TokenType type, string value)
	    {
            this.Type = type;
            this.StringValue = value;
	    }
    }

    public sealed class Tokenizer 
    {
        private readonly string input;

        public Tokenizer(string input)
        {
            this.input = input;
        }

        public List<Token> Tokenize()
        {
            var result = new List<Token>();
            for (int i = 0; i < input.Length; i++) 
            {
                var ch = this.input[i];
                if (char.IsLetter(ch))
                {
                    var identifier = "";
                    while (char.IsLetter(ch) || char.IsDigit(ch))
                    {
                        identifier += ch;
                        i++;
                        if (i >= this.input.Length)
                        {
                            break;
                        }
                        ch = this.input[i];
                    }
                    result.Add(new Token(TokenType.Id, identifier));
                    i--;
                    continue;
                }
                if (ch == '(')
                {
                    result.Add(new Token(TokenType.OpFrame, "("));
                    continue;
                }
                if (ch == ')')
                {
                    result.Add(new Token(TokenType.ClFrame,")"));
                    continue;
                }
                if (ch == '.')
                {
                    result.Add(new Token(TokenType.Dot, "."));
                    continue;
                }
                if (ch == ',')
                {
                    result.Add(new Token(TokenType.Comma, ","));
                    continue;
                }
                if (ch == '?')
                {
                    result.Add(new Token(TokenType.Question, "?"));
                    continue;
                }
                if (ch == ':')
                {
                    result.Add(new Token(TokenType.Colon, ":"));
                    continue;
                }
            }
            result.Add(new Token(TokenType.EOF, ""));
            return result;
        }
    }
}
