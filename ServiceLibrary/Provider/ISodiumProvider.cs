namespace SakuraFrpService.Provider
{
    public interface ISodiumProvider
    {
        void Init();

        byte[] GenerateNonce();

        byte[] SecretBoxCreate(byte[] message, byte[] nonce, byte[] key);

        byte[] SecretBoxOpen(byte[] cipherText, byte[] nonce, byte[] key);

        byte[] ArgonHashBinary(byte[] password, byte[] salt, long opsLimit, int memLimit, long outputLength);
    }
}
