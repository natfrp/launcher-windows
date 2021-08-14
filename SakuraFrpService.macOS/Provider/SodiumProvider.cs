using System;
using System.Runtime.InteropServices;

namespace SakuraFrpService.Provider
{
    public class SodiumProvider : ISodiumProvider
    {
        private const string Name = "libsodium.dylib";

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void sodium_init();

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void randombytes_buf(byte[] buffer, int size);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern int crypto_pwhash(byte[] buffer, long bufferLen, byte[] password, long passwordLen, byte[] salt, long opsLimit, int memLimit, int alg);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern int crypto_secretbox_easy(byte[] buffer, byte[] message, long messageLength, byte[] nonce, byte[] key);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern int crypto_secretbox_open_easy(byte[] buffer, byte[] cipherText, long cipherTextLength, byte[] nonce, byte[] key);

        public void Init()
        {
            sodium_init();
        }

        public byte[] GenerateNonce()
        {
            var buffer = new byte[SECRETBOX_NONCE_BYTES];
            randombytes_buf(buffer, SECRETBOX_NONCE_BYTES);
            return buffer;
        }

        #region Secret Box

        public const int SECRETBOX_KEY_BYTES = 32;
        public const int SECRETBOX_MAC_BYTES = 16;
        public const int SECRETBOX_NONCE_BYTES = 24;

        public byte[] SecretBoxCreate(byte[] message, byte[] nonce, byte[] key)
        {
            if (key.Length != SECRETBOX_KEY_BYTES)
            {
                throw new Exception("Key size must be " + SECRETBOX_KEY_BYTES);
            }
            if (nonce.Length != SECRETBOX_NONCE_BYTES)
            {
                throw new Exception("Nonce size must be " + SECRETBOX_KEY_BYTES);
            }

            var buffer = new byte[SECRETBOX_MAC_BYTES + message.Length];
            if (crypto_secretbox_easy(buffer, message, message.Length, nonce, key) != 0)
            {
                throw new Exception("Failed to create SecretBox");
            }
            return buffer;
        }

        public byte[] SecretBoxOpen(byte[] cipherText, byte[] nonce, byte[] key)
        {
            if (key.Length != SECRETBOX_KEY_BYTES)
            {
                throw new Exception("Key size must be " + SECRETBOX_KEY_BYTES);
            }
            if (nonce.Length != SECRETBOX_NONCE_BYTES)
            {
                throw new Exception("Nonce size must be " + SECRETBOX_KEY_BYTES);
            }

            if (cipherText[0] == 0)
            {
                //check to see if trim is needed
                var trim = true;
                for (var i = 0; i < SECRETBOX_MAC_BYTES - 1; i++)
                {
                    if (cipherText[i] != 0)
                    {
                        trim = false;
                        break;
                    }
                }

                //if the leading MAC_BYTES are null, trim it off before going on.
                if (trim)
                {
                    var temp = new byte[cipherText.Length - SECRETBOX_MAC_BYTES];
                    Array.Copy(cipherText, SECRETBOX_MAC_BYTES, temp, 0, cipherText.Length - SECRETBOX_MAC_BYTES);
                    cipherText = temp;
                }
            }

            var buffer = new byte[cipherText.Length - SECRETBOX_MAC_BYTES];
            if (crypto_secretbox_open_easy(buffer, cipherText, cipherText.Length, nonce, key) != 0)
            {
                throw new Exception("Failed to open SecretBox");
            }
            return buffer;
        }

        #endregion

        #region Password Hash

        public const int ARGON_ALGORITHM_DEFAULT = 1;

        public const uint ARGON_SALTBYTES = 16;

        public byte[] ArgonHashBinary(byte[] password, byte[] salt, long opsLimit, int memLimit, long outputLength = ARGON_SALTBYTES)
        {
            if (salt.Length != ARGON_SALTBYTES)
            {
                throw new Exception("Salt size must be " + ARGON_SALTBYTES);
            }
            var buffer = new byte[outputLength];
            if (crypto_pwhash(buffer, buffer.Length, password, password.GetLongLength(0), salt, opsLimit, memLimit, ARGON_ALGORITHM_DEFAULT) != 0)
            {
                throw new Exception("Sodium hash failed");
            }
            return buffer;
        }

        #endregion
    }
}
