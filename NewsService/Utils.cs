using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NewsService
{
    public class SingleAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly SingleAsyncEnumerator<T> enumerator;

        public static SingleAsyncEnumerable<T> Of<T>(T _value)
        {
            return new SingleAsyncEnumerable<T>(_value);
        }

        private SingleAsyncEnumerable(T _value)
        {
            enumerator = new SingleAsyncEnumerator<T>(_value);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken _cancellationToken = new CancellationToken())
        {
            return enumerator;
        }

        private class SingleAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            public T Current { get; }

            public SingleAsyncEnumerator(T _t)
            {
                Current = _t;
            }

            public ValueTask DisposeAsync()
            {
                return new ValueTask();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(false);
            }
        }
    }
}