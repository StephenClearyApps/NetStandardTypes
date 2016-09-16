using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetCatalog
{
    internal static class AsyncEnumerableEx
    {
        public static IAsyncEnumerable<T> Generate<TState, T>(Func<TState> initialState, Func<TState, Task<Tuple<bool, TState, T>>> callback)
        {
            return new AnonymousAsyncEnumerable<T>(() =>
            {
                var current = default(T);
                var state = initialState();
                return new AnonymousAsyncEnumerator<T>(async _ =>
                {
                    var result = await callback(state).ConfigureAwait(false);
                    state = result.Item2;
                    if (result.Item1)
                        current = result.Item3;
                    return result.Item1;
                }, () => current, () => { });
            });
        }
    }

    internal sealed class AnonymousAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly Func<IAsyncEnumerator<T>> _getEnumerator;

        public AnonymousAsyncEnumerable(Func<IAsyncEnumerator<T>> getEnumerator)
        {
            _getEnumerator = getEnumerator;
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return _getEnumerator();
        }
    }

    internal sealed class AnonymousAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly Func<CancellationToken, Task<bool>> _moveNext;
        private readonly Func<T> _current;
        private readonly Action _dispose;

        public AnonymousAsyncEnumerator(Func<CancellationToken, Task<bool>> moveNext, Func<T> current, Action dispose)
        {
            _moveNext = moveNext;
            _current = current;
            _dispose = dispose;
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return _moveNext(cancellationToken);
        }

        public T Current => _current();

        public void Dispose()
        {
            _dispose();
        }
    }
}