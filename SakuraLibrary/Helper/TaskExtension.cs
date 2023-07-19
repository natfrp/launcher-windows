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
    }
}
