using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEncrypter.CryptMethods
{
    public enum CryptMethod
    {
        AES
    }

    public interface ICryptMethod
    {
        /// <summary>
        /// Encrypts data with a encryption algorithm.
        /// </summary>
        /// <param name="plaintext">Plaintext, which is encrypted in place.</param>
        void Encrypt(ref byte[] plaintext);

        /// <summary>
        /// Decrypts data with a decryption algorithm.
        /// </summary>
        /// <param name="cyphertex">Cyphertex, which is decrypted in place.</param>
        void Decrypt(ref byte[] cyphertex);

        /// <summary>
        /// Padds data to fit into an encryption algorithm.
        /// </summary>
        /// <param name="state">Reference to the current encryption state</param>
        void Padding(ref byte[] state);
    }
}
