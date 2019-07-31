using System;

namespace SecureData.Cyphers
{
    /// <summary>
    /// Interface for Cyphers.
    /// </summary>
    public interface ICypher
    {
        /// <summary>
        /// Size of minimal Data input to the Cypher
        /// </summary>
        int MinDataSize { get; }

        /// <summary>
        /// String to identify Cyphers
        /// </summary>
        string CypherIdentifier { get; }

        /// <summary>
        /// Encrypts data with an encryption algorithm.
        /// </summary>
        /// <param name="plaintext">Plaintext, which is encrypted in place.</param>
        /// <param name="startIndex">Index to start the process from.</param>
        /// <param name="key">Key to encrypt</param>
        void Encrypt(ref byte[] plaintext, int startIndex, byte[] key);

        /// <summary>
        /// Decrypts data with an decryption algorithm.
        /// </summary>
        /// <param name="cyphertex">Cyphertex, which is decrypted in place.</param>
        /// <param name="startIndex">Index to start the process from.</param>
        /// <param name="key">Key to decrypt</param>
        void Decrypt(ref byte[] cyphertex, int startIndex, byte[] key);

        /// <summary>
        /// Padds data to fit into an encryption algorithm.
        /// </summary>
        /// <param name="state">Reference to the current encryption state</param>
        void Padding(ref byte[] state);
    }

    public static class Cypher
    {
        public static ICypher GetByIdentifier(string identifier)
        {
            switch (identifier)
            {
                case "AES":
                    return new AES();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
