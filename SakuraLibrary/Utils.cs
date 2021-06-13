using System;
using System.Text;
using System.Security.Cryptography;

namespace SakuraLibrary
{
    public static class Utils
    {
        public static readonly DateTime SakuraTimeBase = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string Md5(byte[] data)
        {
            var result = new StringBuilder();
            using (var md5 = MD5.Create())
            {
                foreach (var b in md5.ComputeHash(data))
                {
                    result.Append(b.ToString("x2"));
                }
            }
            return result.ToString();
        }

        public static string Md5(string Data) => Md5(Encoding.UTF8.GetBytes(Data));

        public static uint GetSakuraTime() => (uint)DateTime.UtcNow.Subtract(SakuraTimeBase).TotalSeconds;

        public static DateTime ParseSakuraTime(uint seconds) => SakuraTimeBase.AddSeconds(seconds).ToLocalTime();
    }
}
