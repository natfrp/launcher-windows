using System;
using System.IO;
using System.Runtime.InteropServices;
using ObjCRuntime;

using SakuraFrpService.Sodium;

namespace SakuraFrpService.Provider
{
    public class SodiumProvider : ISodiumProvider
    {
        public void Init()
        {
            var sodium = "Sodium/" + RuntimeInformation.ProcessArchitecture.ToString().ToLower() + "/libsodium.dylib";
            if (!File.Exists(sodium))
            {
                throw new Exception("未找到架构匹配的 libsodium, 当前系统可能不支持远程管理");
            }
            else if (Dlfcn.dlopen(Path.GetFullPath(sodium), 0x102) == IntPtr.Zero)
            {
                throw new Exception("libsodium 加载失败");
            }
        }

        public byte[] GenerateNonce()
        {
            var buffer = new byte[SECRETBOX_NONCE_BYTES];
            SodiumLibrary.randombytes_buf(buffer, SECRETBOX_NONCE_BYTES);
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
            if (SodiumLibrary.crypto_secretbox_easy(buffer, message, message.Length, nonce, key) != 0)
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
            if (SodiumLibrary.crypto_secretbox_open_easy(buffer, cipherText, cipherText.Length, nonce, key) != 0)
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
            if (SodiumLibrary.crypto_pwhash(buffer, buffer.Length, password, password.GetLongLength(0), salt, opsLimit, memLimit, ARGON_ALGORITHM_DEFAULT) != 0)
            {
                throw new Exception("Sodium hash failed");
            }
            return buffer;
        }

        #endregion
    }
}
