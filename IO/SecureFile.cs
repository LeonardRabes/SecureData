using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using DataEncrypter.CryptMethods;

namespace DataEncrypter.IO
{
    public class SecureFile : IDisposable
    {
        public FileStream FileState { get; internal set; }

        private ICryptMethod _cryptMethod;
        private byte[] _cryptName;
        private string _filepath;
        private FileStream _fileStream;

        public SecureFile(string filepath, string key, CryptMethod method = CryptMethod.AES)
        {
            switch (method)
            {
                case CryptMethod.AES:
                    _cryptMethod = new AES(ToByte(key));
                    _cryptName = ToByte("AESF");
                    break;
                default:
                    throw new NotImplementedException();
            }

            string stateTempName = $"$tmpSecFile{DateTime.Now.ToBinary().ToString()}";
            FileState = new FileStream(stateTempName, FileMode.Create);
            File.SetAttributes(stateTempName, FileAttributes.Hidden);

            _filepath = filepath;
            _fileStream = new FileStream(filepath, FileMode.Open);
        }

        public void Encrypt()
        {
            FileState.Position = 0;

            var writer = new BinaryWriter(FileState);
            var reader = new BinaryReader(_fileStream);

            //write unsecure header
            writer.Write(_cryptName);                                                               //file type | 4bytes

            //secure header | 48 bytes
            List<byte> secureHeader = new List<byte>();
            secureHeader.AddRange(BitConverter.GetBytes(_fileStream.Length));                       //length of orig file | 8bytes
            secureHeader.AddRange(ToFixSizedByte(Path.GetFileNameWithoutExtension(_filepath), 24)); //orig name of file | 24bytes
            secureHeader.AddRange(ToFixSizedByte(Path.GetExtension(_filepath), 16));                //orig file extension | 16bytes

            byte[] sh = secureHeader.ToArray();
            _cryptMethod.Encrypt(ref sh);
            writer.Write(sh); //write encrypted secure header to stream

            while (_fileStream.Length - _fileStream.Position > 1)
            {
                int maxLength = 536_870_912; //int.MaxValue / 4 => roughly 500mb

                byte[] state = reader.ReadBytes((int)Math.Min(_fileStream.Length, maxLength));
                if (state.Length % 16 != 0)
                {
                    Padding(ref state);
                }

                _cryptMethod.Encrypt(ref state);

                writer.Write(state);
            }
        }

        public void Save(string filename)
        {
            var fstream = new FileStream(filename, FileMode.Create);
            FileState.CopyTo(fstream);
            fstream.Close();
        }

        public void Dispose()
        {
            //Dispose temp file by overwriting with 0
            FileState.Position = 0;
            byte[] bytes = new byte[16384];
            long amount = (FileState.Length / bytes.Length + 1);

            for (long i = 0; i < amount; i++)
            {
                FileState.Write(bytes, 0, bytes.Length);
            }

            string name = FileState.Name;
            FileState.Close();
            File.Delete(name);
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

        private byte[] ToFixSizedByte(string str, int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }

            return bytes;
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
