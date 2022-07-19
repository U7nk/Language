using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wired.OneOf
{
        public class OneOfBase<T0, T1> : IOneOf
    {
        readonly T0 _value0;
        readonly T1 _value1;
        readonly int _index;

        protected OneOfBase(OneOf<T0, T1> input)
        {
            _index = input.Index;
            switch (_index)
            {
                case 0: _value0 = input.AsT0; break;
                case 1: _value1 = input.AsT1; break;
                default: throw new InvalidOperationException();
            }
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

        public int Index { get{ return _index; }}

        public bool IsT0 {get{ return _index == 0;}}
        public bool IsT1 {get{ return _index == 1;}}

        public T0 AsT0 {
            get 
            {
                if (_index == 0)
                {
                    return _value0;
                }
                throw new InvalidOperationException("Cannot return as T0 as result is T" +  _index);
            }
        }
        public T1 AsT1 {
            get 
            {
                if (_index == 1)
                {
                    return _value1;
                }
                throw new InvalidOperationException("Cannot return as T1 as result is T" +  _index);
            }
        }
       

        

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

        

        

		public bool TryPickT0(out T0 value, out T1 remainder)
		{
			value = IsT0 ? AsT0 : default(T0);
            if (_index == 0){
                remainder = default(T1);
            }
            else if (_index == 1){
                remainder = AsT1;
            }

            else
            {
                throw new InvalidOperationException();
            }
			return this.IsT0;
		}
        
		public bool TryPickT1(out T1 value, out T0 remainder)
		{
			value = IsT1 ? AsT1 : default(T1);
            if (_index == 0){
                remainder = AsT0;
            }
            else if (_index == 1){
                remainder = default(T0);
            }
            else
            {
                throw new InvalidOperationException();
            }
			return this.IsT1;
		}

        bool Equals(OneOfBase<T0, T1> other) 
        {
            if (_index == other._index){
                if (_index == 0){
                    return Equals(_value0, other._value0);
                }
                if (_index == 1){
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
            if (obj is OneOfBase<T0, T1>)
            {
                return Equals(obj.As<OneOfBase<T0, T1>>());
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

