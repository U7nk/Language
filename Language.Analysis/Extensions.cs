using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis;

internal static class Extensions
{
    public class Condition<T>
    { 
        public static implicit operator bool(Condition<T> c) => c.Value;
        public static implicit operator Condition<T>(bool c) => new(c, default!);
        public bool Value { get; internal set; }
        public T Obj { get; }

        public Condition(bool value, T obj)
        {
            Value = value;
            Obj = obj;
        }
    }
    public static Condition<T> OrEquals<T, TY>(this Condition<T> first, TY equalsTo)
    {
        if (first.Obj is null)
            throw new ArgumentNullException(nameof(first));
        
        first.Value = first.Value || first.Obj.As<TY>().Equals(equalsTo);
        return first;
    }

    public static Condition<T> Or<T>(this Condition<T> first, bool second)
    {
        first.Value = first.Value || second;
        return first;
    }
    public static Condition<T> And<T>(this Condition<T> first, bool second)
    {
        first.Value = first.Value && second;
        return first;
    }

    public static Condition<T> IsInside<T>(this T obj, IEnumerable<T> enumerable) => enumerable.Contains(obj);
    public static Condition<T> IsOutside<T>(this T obj, IEnumerable<T> enumerable) => new(!enumerable.Contains(obj), obj);

    public static RangeEnumerator GetEnumerator(this Range range) => new(range);

    public ref struct RangeEnumerator
    {
        int _current;
        readonly int _end;
        readonly sbyte _step;

        public RangeEnumerator(Range range)
        {
            if (range.Start.IsFromEnd)
            {
                throw new NotSupportedException("Start from end is not supported");
            }
            
            if (range.Start.Value > range.End.Value)
            {
                _current = range.Start.Value;
                _end = range.End.Value - 1;
                _step = -1;
            }
            else
            {
                _current = range.Start.Value - 1;
                _end = range.End.Value;
                _step = 1;
            }
        }
        
        public bool MoveNext()
        {
            _current += _step;
            return _current != _end;
        }
        
        public int Current => _current;
    }
    
    /// <summary>
    /// Null guard
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="objExpression"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [StackTraceHidden]
    [DebuggerHidden]
    [DebuggerStepThrough]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static T NullGuard<T>(
        [System.Diagnostics.CodeAnalysis.NotNull]this T? obj,
        [CallerArgumentExpression("obj")] string objExpression = "")
        where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(objExpression);
        }

        return obj;
    }
    
    /// <summary>
    /// Null guard
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="objExpression"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [StackTraceHidden]
    [DebuggerHidden]
    [DebuggerStepThrough]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static T NullGuard<T>(
        [System.Diagnostics.CodeAnalysis.NotNull]this T? obj,
        [CallerArgumentExpression("obj")] string objExpression = "")
        where T : struct
    {
        if (obj is null)
        {
            throw new ArgumentNullException(objExpression);
        }

        return obj.Value;
    }
    
    

    internal static bool Empty<T>(this IEnumerable<T> enumerable) 
        => !enumerable.Any();

    internal static IEnumerable<T> Exclude<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        => enumerable.Where(x => !predicate(x));

    internal static string CutTail(this string str, int count)
    {
        return str.Substring(0, str.Length - count);
    }
        
    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(toCast))]
    internal static T As<T>(this object? toCast)
    {
        toCast.NullGuard();
        return ((T)toCast!);
    }
    

    [DebuggerStepThrough]
    internal static T AddTo<T>(this T obj, ICollection<T> collection)
    {
        collection.Add(obj);
        return obj;
    }
    
    [DebuggerStepThrough]
    internal static IEnumerable<T> AddRangeTo<T>(this ICollection<T> enumerable, ICollection<T> collection)
    {
        foreach (var obj in enumerable)
        {
            collection.Add(obj);
        }
        
        return enumerable;
    }
    
    [DebuggerStepThrough]
    internal static IEnumerable<T> Only<T>(this IEnumerable<SyntaxTree> syntaxTrees)
    {
        return syntaxTrees
            .SelectMany(st => st.Root.Members)
            .OfType<T>();
    }
    
    
    
    [DebuggerStepThrough]
    internal static void ForEach<T>(this ICollection<T> enumerable, Action<T> action)
    {
        foreach (var obj in enumerable)
        {
            action(obj);
        }
    }

    internal static T[] Slice<T>(this T[] collection, int fromIncluding, int? toExcluding = null)
    {
        if (toExcluding is null)
        {
            toExcluding = collection.Length;
        }
        
        var res = new T[toExcluding.Value - fromIncluding];
        var counter = 0;
        foreach(var i in 0..collection.Length)
        {
            if (i < fromIncluding || i >= toExcluding)
            {
                continue;
            }
            res[counter] = collection[i];
        }
        return res;
    }

    internal static IList<T> Slice<T>(this IList<T> collection, int fromIncluding, int? toExcluding = null)
    {
        if (toExcluding is null)
        {
            toExcluding = collection.Count;
        }
        var res = new List<T>(toExcluding.Value - fromIncluding);
        for (int i = 0; i < collection.Count; i++)
        {
            if (i < fromIncluding || i >= toExcluding)
            {
                continue;
            }
            res.Add(collection[i]);
        }
        return res;
    }
    

    public static bool CanBeConvertedTo(this Type givenType, Type type) => 
        type.IsAssignableFrom(givenType);
    public static bool CanBeConvertedTo<T>(this Type givenType) => 
        typeof(T).IsAssignableFrom(givenType);
    
    public static bool InRange(this int value, int left, int right)
    {
        if (left <= value && right >= value)
            return true;
        return false;
    }
    
    
    [ContractAnnotation("condition:false => halt")]
    public static bool ThrowIfFalse(
        [DoesNotReturnIf(false)] this bool condition, 
        string? message = "",
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = "")
    {
        if (!condition)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new Exception($"Condition '{conditionExpression}' is failed.");
            }

            throw new Exception($"Condition '{conditionExpression}' is failed: " + message);
        }

        return condition;
    }
    
    [DebuggerStepThrough]
    [StackTraceHidden]
    public static bool ThrowIfTrue(
        [DoesNotReturnIf(true)] this bool condition, 
        string message = "",
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = "")
    {
        if (condition)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new Exception($"Condition {conditionExpression} is {true}. But it should be {false}");
            }

            throw new Exception($"Condition {conditionExpression} expected to be {false}: " + message);
        }

        return condition;
    }
    
    public static ConditionalAdd<T> If<T>(this T obj, bool condition) => new ConditionalAdd<T>(obj);
}
internal class ConditionalAdd<T>
{
    bool IfState { get; set; }
    bool Condition { get; set; }
    List<Action> TrueActions { get; }
    readonly T _obj;
    public ConditionalAdd(T obj)
    {
        this._obj = obj;
        TrueActions = new List<Action>();
    }
    public ConditionalAdd<T> If(bool condition)
    {
        Condition = condition;
        IfState = true;
        return this;
    }
    public ConditionalAdd<T> AddTo(IList list)
    {
        if (IfState)
        {
            if (Condition == true)
            {
                list.Add(_obj);
                return this;
            }
        }
        if (IfState == false)
        {
            if (Condition == false)
            {
                list.Add(_obj);
                return this;
            }
        }
        return this;
    }

    public ConditionalAdd<T> Else
    {
        get
        {
            IfState = false;
            return this;
        }
    }
}