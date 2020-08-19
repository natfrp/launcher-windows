using System;
using System.Collections.Concurrent;

using SakuraLibrary.Proto;

namespace SakuraFrpService
{
    public class LogManager : ConcurrentQueue<Log>
    {
        public static LogManager Instance = null;

        public int RotateSize;

        public LogManager(int bufferSize)
        {
            if (Instance != null)
            {
                throw new Exception("WTF");
            }
            RotateSize = bufferSize;
        }

        public void Log(string source, string data)
        {
            if (data == null)
            {
                return;
            }
            while (Count > RotateSize)
            {
                TryDequeue(out Log _);
            }
            Enqueue(new Log()
            {
                Source = source,
                Data = data
            });
        }
    }
}
