using System;

namespace DataEncrypter.Cyphers
{
    public enum Cypher
    {
        AES
    }

    public interface ICypher
    {
        /// <summary>
        /// Encrypts data with a encryption algorithm.
        /// </summary>
        /// <param name="plaintext">Plaintext, which is encrypted in place.</param>
        /// <param name="startIndex">Index to start the process from.</param>
        /// <param name="key">Key to encrypt</param>
        void Encrypt(ref byte[] plaintext, int startIndex, byte[] key);

        /// <summary>
        /// Decrypts data with a decryption algorithm.
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
}
