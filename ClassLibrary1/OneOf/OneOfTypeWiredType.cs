using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Wired.OneOf
{
    public class OneOfTypeWiredType : OneOfTT<Type, WiredType>
    {
        public static implicit operator OneOf<Type, WiredType>(OneOfTypeWiredType t) { return t; }
        public static implicit operator OneOfTypeWiredType(OneOf<Type, WiredType> t) { return t; }
        public static implicit operator OneOfTypeWiredType(Type type) { return new OneOfTypeWiredType(0, type, null); }
        public static implicit operator OneOfTypeWiredType(WiredType wiredType) { return new OneOfTypeWiredType(1, null, wiredType); }

        public static implicit operator WiredType(OneOfTypeWiredType t) { return t.AsWiredType; }
        public static implicit operator Type(OneOfTypeWiredType t) { return t.AsCSharp; }
        public OneOfTypeWiredType(int index, Type type, WiredType wiredType) : base(index, type, wiredType)
        {

        }
    }

    public class OneOfTT<T0, T1> : IOneOf
    {
        protected readonly T0 _value0;
        protected readonly T1 _value1;
        protected readonly int _index;
        private int p;
        private Type type;
        private WiredType wiredType;

        public OneOfTT(int index, T0 value0 = default(T0), T1 value1 = default(T1))
        {
            _index = index;
            _value0 = value0;
            _value1 = value1;
        }

        public object Value
        {
            get
            {
                if (_index == 0)
                {
                    return _value0;
                }
                if (_index == 1)
                {
                    return _value1;
                }
                throw new InvalidOperationException();
            }
        }

        public int Index { get { return _index; } }

        public bool IsType { get { return _index == 0; } }
        public bool IsWiredType { get { return _index == 1; } }

        public T0 AsCSharp
        {
            get
            {
                if (_index == 0)
                {
                    return _value0;
                }
                throw new InvalidOperationException("Cannot return as T0 as result is T" + _index);
            }
        }

        public T1 AsWiredType
        {
            get
            {
                if (_index == 1)
                {
                    return _value1;
                }
                throw new InvalidOperationException("Cannot return as T1 as result is T" + _index);
            }
        }

        public static implicit operator OneOfTT<T0, T1>(T0 t) { return new OneOfTT<T0, T1>(0, value0: t); }
        public static implicit operator OneOfTT<T0, T1>(T1 t) { return new OneOfTT<T0, T1>(1, value1: t); }


        public void Switch(Action<T0> f0, Action<T1> f1)
        {
            if (_index == 0 && f0 != null)
            {
                f0(_value0);
                return;
            }
            if (_index == 1 && f1 != null)
            {
                f1(_value1);
                return;
            }
            throw new InvalidOperationException();
        }

        [DebuggerStepThrough]
        public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1)
        {
            if (_index == 0 && f0 != null)
            {
                return f0(_value0);
            }
            if (_index == 1 && f1 != null)
            {
                return f1(_value1);
            }
            throw new InvalidOperationException();
        }

        public static OneOfTT<T0, T1> FromT0(T0 input) { return input; }
        public static OneOfTT<T0, T1> FromT1(T1 input) { return input; }


        public OneOfTT<TResult, T1> MapT0<TResult>(Func<T0, TResult> mapFunc)
        {
            if (mapFunc == null)
            {
                throw new ArgumentNullException("mapFunc");
            }
            if (_index == 0)
            {
                return mapFunc(AsCSharp);
            }
            if (_index == 1)
            {
                return AsWiredType;
            }

            throw new InvalidOperationException();
        }

        public OneOfTT<T0, TResult> MapT1<TResult>(Func<T1, TResult> mapFunc)
        {
            if (mapFunc == null)
            {
                throw new ArgumentNullException("mapFunc");
            }
            if (_index == 0)
            {
                return AsCSharp;
            }
            if (_index == 1)
            {
                return mapFunc(AsWiredType);
            }
            throw new InvalidOperationException();
        }

        public bool TryPickType(out T0 value, out T1 remainder)
        {
            value = IsType ? AsCSharp : default(T0);
            if (_index == 0)
            {
                remainder = default(T1);
            }
            else if (_index == 1)
            {
                remainder = AsWiredType;
            }
            else
            {
                throw new InvalidOperationException();
            }
            return this.IsType;
        }

        public bool TryPickWiredType(out T1 value, out T0 remainder)
        {
            value = IsWiredType ? AsWiredType : default(T1);
            if (_index == 0)
            {
                remainder = AsCSharp;
            }
            else if (_index == 1)
            {
                remainder = default(T0);
            }
            else
            {
                throw new InvalidOperationException();
            }
            return this.IsWiredType;
        }

        bool Equals(OneOfTT<T0, T1> other)
        {
            if (_index == other._index)
            {
                if (_index == 0)
                {
                    return Equals(_value0, other._value0);
                }
                if (_index == 1)
                {
                    return Equals(_value1, other._value1);
                }
            }
            return false;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj is OneOfTT<T0, T1>)
            {
                return Equals(obj.As<OneOfTT<T0, T1>>());
            }

            return false;
        }

        public override string ToString()
        {
            if (_index == 0)
            {
                return Functions.Functions.FormatValue(_value0);
            }
            if (_index == 1)
            {
                return Functions.Functions.FormatValue(_value1);
            }
            throw new InvalidOperationException("Unexpected index, which indicates a problem in the OneOf codegen.");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                if (_index == 0)
                {
                    if (_value0 != null)
                    {
                        hashCode = _value0.GetHashCode();
                    }
                }
                if (_index == 1)
                {
                    if (_value1 != null)
                    {
                        hashCode = _value1.GetHashCode();
                    }
                }
                return (hashCode * 397) ^ _index;
            }
        }
    }
}