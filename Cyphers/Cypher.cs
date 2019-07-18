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
        void Encrypt(ref byte[] plaintext, int startIndex);

        /// <summary>
        /// Decrypts data with a decryption algorithm.
        /// </summary>
        /// <param name="cyphertex">Cyphertex, which is decrypted in place.</param>
        /// <param name="startIndex">Index to start the process from.</param>
        void Decrypt(ref byte[] cyphertex, int startIndex);

        /// <summary>
        /// Padds data to fit into an encryption algorithm.
        /// </summary>
        /// <param name="state">Reference to the current encryption state</param>
        void Padding(ref byte[] state);

        /// <summary>
        /// Updates the key within the algorithm.
        /// </summary>
        /// <param name="key">Key for en-/decryption</param>
        void UpdateKey(byte[] key);
    }
}
