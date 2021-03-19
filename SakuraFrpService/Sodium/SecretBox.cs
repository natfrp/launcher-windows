using System;

namespace SakuraFrpService.Sodium
{
    class SecretBox
    {
        public const int KEY_BYTES = 32;
        public const int MAC_BYTES = 16;
        public const int NONCE_BYTES = 24;

        public static byte[] GenerateNonce()
        {
            var buffer = new byte[NONCE_BYTES];
            SodiumLibrary.randombytes_buf(buffer, NONCE_BYTES);
            return buffer;
        }

        public static byte[] Create(byte[] message, byte[] nonce, byte[] key)
        {
            if (key.Length != KEY_BYTES)
            {
                throw new Exception("Key size must be " + KEY_BYTES);
            }
            if (nonce.Length != NONCE_BYTES)
            {
                throw new Exception("Nonce size must be " + KEY_BYTES);
            }

            var buffer = new byte[MAC_BYTES + message.Length];
            if (SodiumLibrary.crypto_secretbox_easy(buffer, message, message.Length, nonce, key) != 0)
            {
                throw new Exception("Failed to create SecretBox");
            }
            return buffer;
        }

        public static byte[] Open(byte[] cipherText, byte[] nonce, byte[] key)
        {
            if (key.Length != KEY_BYTES)
            {
                throw new Exception("Key size must be " + KEY_BYTES);
            }
            if (nonce.Length != NONCE_BYTES)
            {
                throw new Exception("Nonce size must be " + KEY_BYTES);
            }

            if (cipherText[0] == 0)
            {
                //check to see if trim is needed
                var trim = true;
                for (var i = 0; i < MAC_BYTES - 1; i++)
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
                    var temp = new byte[cipherText.Length - MAC_BYTES];
                    Array.Copy(cipherText, MAC_BYTES, temp, 0, cipherText.Length - MAC_BYTES);
                    cipherText = temp;
                }
            }

            var buffer = new byte[cipherText.Length - MAC_BYTES];
            if (SodiumLibrary.crypto_secretbox_open_easy(buffer, cipherText, cipherText.Length, nonce, key) != 0)
            {
                throw new Exception("Failed to open SecretBox");
            }
            return buffer;
        }
    }
}
