using System.Text;

using Security;
using Foundation;

namespace SakuraFrpService.Helper
{
    public class KeychainHelper
    {
        public const string Service = "moe.berd.SakuraLauncher.Service";

        public static SecStatusCode Save(string key, byte[] data)
        {
            var record = new SecRecord(SecKind.GenericPassword)
            {
                Service = Service,
                Account = key
            };

            SecKeyChain.Remove(record);
            if (data == null || data.Length == 0)
            {
                return SecStatusCode.Success;
            }

            record.Label = key;
            record.ValueData = NSData.FromArray(data);
            return SecKeyChain.Add(record);
        }

        public static SecStatusCode Save(string key, string data) => Save(key, Encoding.UTF8.GetBytes(data));

        public static byte[] Load(string key)
        {
            var stat = SecKeyChain.FindGenericPassword(Service, key, out byte[] data);
            if (stat == SecStatusCode.Success)
            {
                return data;
            }
            return null;
        }

        public static string LoadString(string key)
        {
            var k = Load(key);
            return k == null ? null : Encoding.UTF8.GetString(k);
        }
    }
}
