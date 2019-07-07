using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using DataEncrypter.CryptMethods;

namespace DataEncrypter.IO
{
    /// <summary>
    /// Provides functionality to handle files for encryption and decryption.
    /// </summary>
    public class SecureFile : IDisposable
    {
        public FileStream FileState { get; internal set; }

        private ICryptMethod _cryptMethod;
        private byte[] _cryptName;
        private string _filePath;
        private FileStream _fileStream;
        private string _fileName;
        private string _fileExtension;

        /// <summary>
        /// Contructs basic data for file en-/decryption. This includes a temporary file to save data.
        /// </summary>
        /// <param name="filePath">File path to the file, which will be en-/decrypted.</param>
        /// <param name="key">The key for the CryptMethod</param>
        /// <param name="method">The method of en-/decryption</param>
        public SecureFile(string filePath, string key, CryptMethod method = CryptMethod.AES)
        {
            switch (method)
            {
                case CryptMethod.AES:
                    _cryptMethod = new AES(ToByte(key));
                    _cryptName = ToByte("SECFAES"); //abreviation: SECureFile AdvancedEncryptionStandard
                    break;
                default:
                    throw new NotImplementedException();
            }

            //create a temporary file to save data to
            string stateTempName = $"$tmpSecFile{DateTime.Now.ToBinary().ToString()}";
            FileState = new FileStream(stateTempName, FileMode.Create);
            File.SetAttributes(stateTempName, FileAttributes.Hidden);

            //open target file
            _filePath = filePath;
            _fileStream = new FileStream(filePath, FileMode.Open);
        }

        /// <summary>
        /// Encrypts the file, which was provided to the constructor.
        /// </summary>
        public void Encrypt()
        {
            FileState.Position = 0;
            _fileStream.Position = 0;
            _fileName = Path.GetFileNameWithoutExtension(_filePath);
            _fileExtension = ".secf"; //new file extension after encryption

            var writer = new BinaryWriter(FileState);
            var reader = new BinaryReader(_fileStream);

            //write unsecure header
            writer.Write(_cryptName);                                                //file type | 7bytes

            //secure header | 64 bytes
            List<byte> secureHeader = new List<byte>();
            secureHeader.AddRange(BitConverter.GetBytes(_fileStream.Length));        //length of orig file | 8bytes
            secureHeader.AddRange(ToFixSizedByte(_fileName, 40));                    //orig name of file | 40bytes
            secureHeader.AddRange(ToFixSizedByte(Path.GetExtension(_filePath), 16)); //orig file extension | 16bytes

            byte[] sh = secureHeader.ToArray();
            _cryptMethod.Encrypt(ref sh);
            writer.Write(sh); //write encrypted secure header to stream

            //encryption of file
            while (_fileStream.Length - _fileStream.Position > 1)
            {
                int maxLength = 1_048_576; //int.MaxValue / 2048 => roughly 1mb

                byte[] state = reader.ReadBytes((int)Math.Min(_fileStream.Length - _fileStream.Position, maxLength)); //read chunks from file or the entire file if file < 1mb
 
                _cryptMethod.Padding(ref state); //add padding to have no incomplete blocks
                
                _cryptMethod.Encrypt(ref state);

                writer.Write(state);
            }
        }

        /// <summary>
        /// Decrypts the file, which was provided to the constructor.
        /// </summary>
        public void Decyrpt()
        {
            FileState.Position = 0;
            _fileStream.Position = 0;
            var writer = new BinaryWriter(FileState);
            var reader = new BinaryReader(_fileStream);

            //check file type
            if (reader.ReadInt32() != BitConverter.ToInt32(ToByte("SECF"), 0))
            {
                throw new Exception("Not SECF FileType!");
            }
            string encryptionMethod = ToString(reader.ReadBytes(3)); //write encryption method and file type in header

            //secure header | 64bytes
            byte[] secureHeader = reader.ReadBytes(64);
            _cryptMethod.Decrypt(ref secureHeader);

            long fileLength = BitConverter.ToInt64(secureHeader, 0); //read length of original file | 8bytes
            _fileName = FromFixSizedByte(secureHeader, 8);             //name of original file | 40bytes
            _fileExtension = FromFixSizedByte(secureHeader, 48);       //file extension of original file | 16bytes

            //decryption of file
            while (_fileStream.Length - _fileStream.Position > 1)
            {
                int maxLength = 1_048_576; //int.MaxValue / 2048 => roughly 1mb

                byte[] state = reader.ReadBytes((int)Math.Min(_fileStream.Length - _fileStream.Position, maxLength));

                _cryptMethod.Decrypt(ref state);

                writer.Write(state);
                writer.Flush();
            }
            FileState.SetLength(fileLength); //set stream to original length
        }

        /// <summary>
        /// Saves the file, which was provided to the constructor.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="fileName"></param>
        /// <param name="fileExtension"></param>
        public void Save(string dirPath = "", string fileName = null, string fileExtension = null)
        {
            if (fileName == null)
            {
                fileName = _fileName;
            }
            if (fileExtension == null)
            {
                fileExtension = _fileExtension;
            }

            FileState.Position = 0;
            var fstream = new FileStream($@"{dirPath}{fileName}{fileExtension}", FileMode.Create);
            FileState.CopyTo(fstream);
            fstream.Close();
        }

        /// <summary>
        /// Disposes this instance and removes any created temporary files.
        /// </summary>
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
            _fileStream.Close();
            File.Delete(name);
        }

        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="str">String to be converted</param>
        private byte[] ToByte(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }

            return bytes;
        }

        /// <summary>
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="str">Bytes to be converted</param>
        private string ToString(byte[] bytes)
        {
            string str = "";
            foreach (var b in bytes)
            {
                str += (char)b;
            }

            return str;
        }

        /// <summary>
        /// Converts a string to a byte array with fixed size. Is null-terminated
        /// </summary>
        /// <param name="str">String to be converted</param>
        /// <param name="length">Length of the array</param>
        private byte[] ToFixSizedByte(string str, int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }
            bytes[length - 1] = 0;

            return bytes;
        }

        /// <summary>
        /// Converts a byte array with fixed size to a string. Is null-terminated
        /// </summary>
        /// <param name="bytes">Bytes to be converted</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private string FromFixSizedByte(byte[] bytes, int startIndex)
        {
            string str = "";
            for (int i = startIndex; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                {
                    str += (char)bytes[i];
                }
                else
                {
                    break;
                }
            }

            return str;
        }  
    }
}
