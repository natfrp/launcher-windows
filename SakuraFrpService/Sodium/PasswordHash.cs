using System;

namespace SakuraFrpService.Sodium
{
    class PasswordHash
    {
        private const int ARGON_ALGORITHM_DEFAULT = 1;

        private const uint ARGON_SALTBYTES = 16;

        public static byte[] ArgonHashBinary(byte[] password, byte[] salt, long opsLimit, int memLimit, long outputLength = ARGON_SALTBYTES)
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
    }
}
