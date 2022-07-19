using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Wired
{
    internal static class Extensions
    {
        internal static string CutTail(this string str, int count)
        {
            return str.Substring(0, str.Length - count);
        }
        
        [DebuggerStepThrough]
        internal static T As<T>(this object toCast)
        {
            return (T)toCast;
        }

        [DebuggerStepThrough]
        internal static T AddTo<T>(this T obj, ICollection<T> collection)
        {
            collection.Add(obj);
            return obj;
        }

        [DebuggerStepThrough]
        internal static T[] EmptyOf<T>()
        {
            return new T[]{};
        }

        internal static T[] Slice<T>(this T[] collection, int fromIncluding, int toExcluding = -1)
        {
            if (toExcluding == -1)
            {
                toExcluding = collection.Length;
            }
            var res = new T[toExcluding - fromIncluding];
            var counter = 0;
            for (int i = 0; i < collection.Length; i++)
            {
                if (i < fromIncluding || i >= toExcluding)
                {
                    continue;
                }
                res[counter] = collection[i];
            }
            return res;
        }

        internal static IList<T> Slice<T>(this IList<T> collection, int fromIncluding, int toExcluding = -1)
        {
            if (toExcluding == -1)
            {
                toExcluding = collection.Count;
            }
            var res = new List<T>(toExcluding - fromIncluding);
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

        internal static List<T> ObjToList<T>(this T obj)
        {
            return new List<T>{ obj };
        }
        internal static T[] ObjToArray<T>(this T obj)
        {
            return new T[] { obj };
        }
        public static Type GetTypeThatAssignableToGenericOrDefault(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType.GetGenericTypeDefinition())
                    return it;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType.GetGenericTypeDefinition())
                return givenType;

            Type baseType = givenType.BaseType;
            if (baseType == null) return null;

            return GetTypeThatAssignableToGenericOrDefault(baseType, genericType);
        }
        public static bool TypeRespectsGenericParameterConstraints(Type type, Type[] constraints)
        {
            // TODO MAKE LOGIC BLYAT;
            if (!constraints.Any())
            {
                return true;
            }
            return false;
        }
        public static bool IsTypesCompatible(Type left, Type right)
        {
            if (right.IsGenericParameter)
            {
                if (TypeRespectsGenericParameterConstraints(left, right.GetGenericParameterConstraints()))
                {
                    return true;
                }
            }
            if (right.IsAssignableTo(left))
            {
                return true;
            }
            if (left.IsGenericType)
            {
                if (right.IsAssignableToGeneric(left))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        public static bool IsAssignableToGeneric(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType.GetGenericTypeDefinition())
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType.GetGenericTypeDefinition())
            {
                var genericArgs = givenType.GetGenericArguments();
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    if (!IsTypesCompatible(genericArgs[i], genericType.GetGenericArguments()[i]))
                    {
                        goto cont;
                    }
                }
                return true;
                cont:;
            }

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGeneric(baseType, genericType);
        }

        public static bool IsAssignableTo(this Type given, Type assignableTo)
        {
            return assignableTo.IsAssignableFrom(given);
        }
        public static ConditionalAdd<T> If<T>(this T obj, bool condition)
        {
            return new ConditionalAdd<T>(obj);
        }
    }
    internal class ConditionalAdd<T>
    {
        public bool IfState { get; set; }
        public bool Condition { get; set; }
        public List<Action> TrueActions { get; set; }
        public T obj;
        public ConditionalAdd(T obj)
        {
            this.obj = obj;
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
                    list.Add(obj);
                    return this;
                }
            }
            if (IfState == false)
            {
                if (Condition == true)
                {
                    list.Add(obj);
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
    internal class MoveNexter
    {
        private readonly bool checkResult;
        private readonly Action moveNextAction;
        public MoveNexter()
        {
            checkResult = false;
        }

        public MoveNexter(Action moveNextAction)
        {
            checkResult = true;
            this.moveNextAction = moveNextAction;
        }
        public static implicit operator bool(MoveNexter moveNexter)
        { 
            return moveNexter.checkResult; 
        }

        public MoveNexter OnTrue(Action action, int times = 1)
        {
            if (checkResult)
            {
                for (int i = 0; i < times; i++)
                {
                    action();
                }
                return this;
            }

            return this;
        }

        public bool OnFalse(Action action, int times = 1)
        {
            if (checkResult)
            {
                return true;
            }
            for (int i = 0; i < times; i++)
            {
                action();
            }
            return false;
        }


    }
}
