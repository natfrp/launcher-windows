using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace SakuraLibrary.Helper
{
    public static class AsyncStreamExtensions
    {
        public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> streamReader, Action<T> action) where T : class
        {
            while (await streamReader.MoveNext().ConfigureAwait(false))
            {
                action(streamReader.Current);
            }
        }
    }
}
