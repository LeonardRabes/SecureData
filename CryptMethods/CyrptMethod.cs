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
        void Encrypt(ref byte[] plaintext);
        void Decrypt(ref byte[] cyphertex);
    }
}
