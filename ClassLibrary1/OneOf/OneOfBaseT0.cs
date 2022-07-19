using System;
using Wired.OneOf.Functions;

namespace Wired.OneOf
{
    public class OneOfBase<T0> : IOneOf
    {
        readonly T0 _value0;
        readonly int _index;

        protected OneOfBase(OneOf<T0> input)
        {
            _index = input.Index;
            switch (_index)
            {
                case 0: _value0 = input.AsT0; break;
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
                throw new InvalidOperationException();
            }
        }

        public int Index {
            get { return _index; }
        }

        public bool IsT0 { get { return _index == 0; }}

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
        

        public void Switch(Action<T0> f0)
        {
            if (_index == 0 && f0 != null)
            {
                f0(_value0);
                return;
            }
            throw new InvalidOperationException();
        }

        public TResult Match<TResult>(Func<T0, TResult> f0)
        {
            if (_index == 0 && f0 != null)
            {
                return f0(_value0);
            }
            throw new InvalidOperationException();
        }

        

        

        bool Equals(OneOfBase<T0> other) 
        {
            if (_index != other._index){
                return false;
            }
            
            if (_index == 0){
                return Equals(_value0, other._value0);
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                    return true;
            }
            if (obj is OneOfBase<T0>){
                return Equals(obj.As<OneOfBase<T0>>());
            }

            return false;
        }

        public override string ToString()
        {
            if (_index == 0)
            {
                return Functions.Functions.FormatValue(_value0);
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
                return (hashCode*397) ^ _index;
            }
        }
    }
}