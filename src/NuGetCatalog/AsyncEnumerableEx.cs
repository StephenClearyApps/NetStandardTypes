using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetCatalog
{
    internal static class AsyncEnumerableEx
    {
        public static IAsyncEnumerable<T> Generate<TState, T>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, Task<T>> resultSelector)
        {
            return AsyncEnumerable.CreateEnumerable(() =>
            {
                var state = initialState;
                var current = default(T);
                return AsyncEnumerable.CreateEnumerator(async _ =>
                {
                    if (!condition(state))
                        return false;
                    current = await resultSelector(state);
                    state = iterate(state);
                    return true;
                }, () => current, () => { });
            });
        }
    }
}