using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class SingleEnumerable<T> : IEnumerable<T>
    {
        private readonly SingleEnumerator<T> enumerator;

        public static SingleEnumerable<T> Of<T>(T _value)
        {
            return new SingleEnumerable<T>(_value);
        }

        private SingleEnumerable(T _value)
        {
            enumerator = new SingleEnumerator<T>(_value);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class SingleEnumerator<T> : IEnumerator<T>
        {
            public T Current { get; }
            object? IEnumerator.Current => Current;

            public SingleEnumerator(T _value)
            {
                Current = _value;
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}