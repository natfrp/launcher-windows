using System.Runtime.InteropServices;

namespace SakuraFrpService.Sodium
{
    public static class SodiumLibrary
    {
        private const string Name = "Sodium/libsodium.dylib";

        static SodiumLibrary()
        {
            sodium_init();
        }

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sodium_init();

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void randombytes_buf(byte[] buffer, int size);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int crypto_pwhash(byte[] buffer, long bufferLen, byte[] password, long passwordLen, byte[] salt, long opsLimit, int memLimit, int alg);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_secretbox_easy(byte[] buffer, byte[] message, long messageLength, byte[] nonce, byte[] key);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_secretbox_open_easy(byte[] buffer, byte[] cipherText, long cipherTextLength, byte[] nonce, byte[] key);
    }
}
