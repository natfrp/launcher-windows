using Grpc.Core;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static T WaitResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public static async Task<Exception> WaitException<T>(this Task<T> task)
        {
            try
            {
                await task.ConfigureAwait(false);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static async Task<Exception> WaitException<T>(this AsyncUnaryCall<T> task)
        {
            try
            {
                await task.ConfigureAwait(false);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public static void Then(this Task<Exception> task, Action<Exception> callback) => task.ContinueWith(t => callback(t.Result));

        public static async Task<Task> InitStream<T>(this AsyncServerStreamingCall<T> request, Action<T> callback, CancellationToken token) where T : class
        {
            var stream = request.ResponseStream;

            if (!await stream.MoveNext(token).ConfigureAwait(false))
            {
                throw new Exception("Stream ended unexpectedly");
            }
            callback(stream.Current);

            return Task.Run(async () =>
            {
                while (await stream.MoveNext(token).ConfigureAwait(false))
                {
                    callback(stream.Current);
                }
            });
        }
    }
}
