using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using DataEncrypter.CryptMethods;

namespace DataEncrypter.IO
{
    public class SecureFile
    {
        public MemoryStream FileState { get; internal set; }

        private ICryptMethod _cryptMethod;
        private string _filepath;
        private FileStream _fileStream;

        public SecureFile(string filepath, string key, CryptMethod method = CryptMethod.AES)
        {
            switch (method)
            {
                case CryptMethod.AES:
                    _cryptMethod = new AES(ToByte(key));
                    break;
                default:
                    throw new NotImplementedException();
            }

            _filepath = filepath;

            FileState = new MemoryStream();
            _fileStream = new FileStream(filepath, FileMode.Open);
        }

        public void Encrypt()
        {
            var writer = new BinaryWriter(FileState);
            var reader = new BinaryReader(_fileStream);
            writer.Write(ToByte("AESF")); //file type
            writer.Write(_fileStream.Length); //original length

            while (_fileStream.Length - _fileStream.Position > 1)
            {
                int maxLength = 536_870_912; //int.MaxValue / 4 => roughly 500mb

                byte[] state = reader.ReadBytes((int)Math.Min(_fileStream.Length, maxLength));
                if (state.Length % 16 != 0)
                {
                    Padding(ref state);
                }

                _cryptMethod.Encrypt(ref state);
            }
        }

        private byte[] ToByte(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }

            return bytes;
        }

        private string ToString(byte[] bytes)
        {
            string str = "";
            foreach (var b in bytes)
            {
                str += (char)b;
            }

            return str;
        }

        private static byte[] Padding(ref byte[] state)
        {
            int l = state.Length;

            if (l % 128 != 0)
            {
                Array.Resize(ref state, 16 * (int)Math.Ceiling(l / 16F));
            }

            return state;
        }
    }
}
