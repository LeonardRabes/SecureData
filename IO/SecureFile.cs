using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DataEncrypter.Cyphers;

namespace DataEncrypter.IO
{
    public class ChunkEventArgs : EventArgs
    {
        public enum ProcessType
        {
            Encryption,
            Decryption
        }

        /// <summary>
        /// Size of the chunk in bytes.
        /// </summary>
        public int ChunkSize { get; set; }
        /// <summary>
        /// Amount of completed chunks.
        /// </summary>
        public int CompletedChunks { get; set; }
        /// <summary>
        /// Amount of total chunks.
        /// </summary>
        public int TotalChunks { get; set; }
        /// <summary>
        /// Elapsed time since the process has been started.
        /// </summary>
        public TimeSpan TotalTime { get; set; }
        /// <summary>
        /// Time the chunk took for completion.
        /// </summary>
        public TimeSpan ChunkTime { get; set; }
        /// <summary>
        /// Type of process.
        /// </summary>
        public ProcessType Type { get; set; }
    }

    /// <summary>
    /// Provides functionality to handle files for encryption and decryption.
    /// </summary>
    public class SecureFile : IDisposable
    {
        public delegate void ChunkUpdateEventHandler(object sender, ChunkEventArgs e);
        public delegate void ProcessCompletedEventHandler(object sender, ChunkEventArgs e);

        /// <summary>
        /// Fires, when a chunk has been completed.
        /// </summary>
        public event ChunkUpdateEventHandler ChunkUpdate;
        /// <summary>
        /// Fires, when a process has been completed.
        /// </summary>
        public event ProcessCompletedEventHandler ProcessCompleted;

        public FileStream FileState { get; internal set; }

        private ICypher _cypher;
        private byte[] _cryptType;
        private string _filePath;
        private FileStream _fileStream;
        private string _fileName;
        private string _fileExtension;

        private static string _secureFileType = "SECF"; //abreviation: SECureFile
        private static int _secureHeaderSize = 80;
        private static int _chunkSize = 1_048_576; //int.MaxValue / 2048 => roughly 1mb
        private static string _decryptionValidation = "decryption_valid";

        /// <summary>
        /// Contructs basic data for file en-/decryption. This includes a temporary file to save data.
        /// </summary>
        /// <param name="filePath">File path to the file, which will be en-/decrypted.</param>
        /// <param name="key">The key for the Cypher</param>
        /// <param name="method">The method of en-/decryption</param>
        public SecureFile(string filePath, string key, Cypher method = Cypher.AES)
        {
            switch (method)
            {
                case Cypher.AES:
                    _cypher = new AES(ToByte(key));
                    _cryptType = ToByte(_secureFileType + "AES");
                    break;
                default:
                    throw new NotImplementedException();
            }

            //create a temporary file to save data to
            string stateTempName = $"$tmpSecFile{DateTime.Now.ToBinary().ToString()}";
            FileState = new FileStream(stateTempName, FileMode.Create);
            File.SetAttributes(stateTempName, FileAttributes.Hidden);

            //open target file
            UpdateFile(filePath);
        }

        /// <summary>
        /// Encrypts the file, which was provided to this instance.
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
            writer.Write(_cryptType);                                                //file type | 7bytes

            //secure header | 80 bytes
            List<byte> secureHeader = new List<byte>();
            secureHeader.AddRange(BitConverter.GetBytes(_fileStream.Length));        //length of orig file | 8bytes
            secureHeader.AddRange(ToFixSizedByte(_fileName, 40));                    //orig name of file | 40bytes
            secureHeader.AddRange(ToFixSizedByte(Path.GetExtension(_filePath), 16)); //orig file extension | 16bytes
            secureHeader.AddRange(ToByte(_decryptionValidation));                    //validation string to determine if decryption is valid | 16bytes

            byte[] sh = secureHeader.ToArray();
            _cypher.Encrypt(ref sh, 0);
            writer.Write(sh); //write encrypted secure header to stream

            //timer
            TimeSpan totalTime = new TimeSpan();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //encryption of file
            while (_fileStream.Length - _fileStream.Position > 1)
            {
                byte[] state = reader.ReadBytes((int)Math.Min(_fileStream.Length - _fileStream.Position, _chunkSize)); //read chunks from file or the entire file if file < 1mb
 
                _cypher.Padding(ref state); //add padding to have no incomplete blocks
                
                _cypher.Encrypt(ref state, 0);

                writer.Write(state);

                //Events and Measurement
                totalTime = totalTime.Add(stopWatch.Elapsed);
                OnChunkUpdate(_fileStream.Position, _fileStream.Length, state.Length, totalTime, stopWatch.Elapsed, ChunkEventArgs.ProcessType.Decryption);
                stopWatch.Restart();
            }

            stopWatch.Stop();
            OnProcessCompleted(_fileStream.Length, totalTime, ChunkEventArgs.ProcessType.Encryption);
        }

        /// <summary>
        /// Decrypts the file, which was provided to this instance.
        /// </summary>
        public void Decrypt()
        {
            FileState.Position = 0;
            _fileStream.Position = 0;

            var writer = new BinaryWriter(FileState);
            var reader = new BinaryReader(_fileStream);

            //check file type
            if (reader.ReadInt32() != BitConverter.ToInt32(ToByte(_secureFileType), 0))
            {
                throw new Exception("Not SECF FileType!");
            }
            string encryptionMethod = ToString(reader.ReadBytes(3)); //read encryption method

            //secure header | 80bytes
            byte[] secureHeader = reader.ReadBytes(_secureHeaderSize);
            _cypher.Decrypt(ref secureHeader, 0);

            long fileLength = BitConverter.ToInt64(secureHeader, 0);   //read length of original file | 8bytes
            _fileName = FromFixSizedByte(secureHeader, 8);             //name of original file | 40bytes
            _fileExtension = FromFixSizedByte(secureHeader, 48);       //file extension of original file | 16bytes
            string validation = ToString(secureHeader, 64);            //read validation | 16bytes

            if (validation != _decryptionValidation) //check if key is correct
            {
                throw new Exception("Incorrect Key!");
            }

            //timer
            TimeSpan totalTime = new TimeSpan();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //decryption of file
            while (_fileStream.Length - _fileStream.Position > 1)
            {
                byte[] state = reader.ReadBytes((int)Math.Min(_fileStream.Length - _fileStream.Position, _chunkSize)); //read chunks from file or the entire file if file < 1mb

                _cypher.Decrypt(ref state, 0);

                writer.Write(state);

                //Events and Measurement
                totalTime = totalTime.Add(stopWatch.Elapsed);
                OnChunkUpdate(_fileStream.Position, _fileStream.Length, state.Length, totalTime, stopWatch.Elapsed, ChunkEventArgs.ProcessType.Decryption);
                stopWatch.Restart();
            }
            FileState.SetLength(fileLength); //set stream to original length and remove padding

            stopWatch.Stop();
            OnProcessCompleted(_fileStream.Length, totalTime, ChunkEventArgs.ProcessType.Decryption);
        }

        /// <summary>
        /// Saves the file, which was provided to the constructor.
        /// </summary>
        /// <param name="dirPath">Path of the directory the file will be saved to</param>
        /// <param name="fileName">File name, without extension (null == name of the original file)</param>
        /// <param name="fileExtension">File extension (null == encrypted extension [.secf] or original extension of decrypted file)</param>
        public void Save(string filePath)
        {
            FileState.Position = 0;
            var fstream = new FileStream(filePath, FileMode.Create);
            FileState.CopyTo(fstream);
            fstream.Close();
        }

        public string SuggestSaveFileName()
        {
            return $@"{_fileName}{_fileExtension}";
        }

        /// <summary>
        /// Updates the key for en-/decryption.
        /// </summary>
        /// <param name="key">Key for en-/decryption</param>
        public void UpdateKey(string key)
        {
            _cypher.UpdateKey(ToByte(key));
        }

        /// <summary>
        /// Changes the file, which can be modified by this instance.
        /// </summary>
        /// <param name="filePath">Path to target file</param>
        public void UpdateFile(string filePath)
        {
            _fileStream?.Close();
            _filePath = filePath;
            _fileStream = new FileStream(filePath, FileMode.Open);
        }

        /// <summary>
        /// Checks if the current key is able to decrypt the file.
        /// </summary>
        /// <returns>Boolean, which indicates if the key is valid.</returns>
        public bool ValidateKeyForDecryption()
        {
            byte[] validation = new byte[_decryptionValidation.Length];
            _fileStream.Position = _cryptType.Length + _secureHeaderSize - _decryptionValidation.Length;
            _fileStream.Read(validation, 0, validation.Length);

            _cypher.Decrypt(ref validation, 0);

            return ToString(validation) == _decryptionValidation;
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
            FileState?.Close();
            _fileStream?.Close();
            File.Delete(name);
        }

        /// <summary>
        /// Called, when a chunk was completed.
        /// </summary>
        protected virtual void OnChunkUpdate(long streamPosition, long streamLength, int chunkSize, TimeSpan totalTime, TimeSpan chunkTime, ChunkEventArgs.ProcessType type)
        {
            var args = new ChunkEventArgs();
            args.ChunkSize = chunkSize;
            args.CompletedChunks = (int)(Math.Ceiling(streamPosition / (float)_chunkSize));
            args.TotalChunks = (int)(Math.Ceiling(streamLength / (float)_chunkSize));
            args.TotalTime = totalTime;
            args.ChunkTime = chunkTime;
            args.Type = type;

            ChunkUpdate?.Invoke(this, args);
        }

        /// <summary>
        /// Called, when a process has been completed.
        /// </summary>
        protected virtual void OnProcessCompleted(long streamLength, TimeSpan elapsedTime, ChunkEventArgs.ProcessType type)
        {
            var args = new ChunkEventArgs();
            args.CompletedChunks = (int)(Math.Ceiling(streamLength / (float)_chunkSize));
            args.TotalChunks = args.CompletedChunks;
            args.TotalTime = elapsedTime;
            args.Type = type;

            ProcessCompleted?.Invoke(this, args);
        }

        /// <summary>
        /// Checks if given file is a SecureFile.
        /// </summary>
        /// <param name="filePath">File path to target file</param>
        /// <returns>Boolean, which validates if its a .secf</returns>
        public static bool IsSecureFile(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            byte[] secf = new byte[_secureFileType.Length];
            fs.Read(secf, 0, secf.Length);
            fs.Close();

            return ToString(secf) == _secureFileType;
        }

        public static Cypher GetCypher(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            byte[] crypt = new byte[3];
            fs.Position = _secureFileType.Length;
            fs.Read(crypt, 0, crypt.Length);
            fs.Close();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="str">String to be converted</param>
        private static byte[] ToByte(string str)
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
        private static string ToString(byte[] bytes, int startIndex = 0)
        {
            string str = "";
            for (int i = startIndex; i < bytes.Length; i++)
            {
                str += (char)bytes[i];
            }

            return str;
        }

        /// <summary>
        /// Converts a string to a byte array with fixed size. Is null-terminated
        /// </summary>
        /// <param name="str">String to be converted</param>
        /// <param name="length">Length of the array</param>
        private static byte[] ToFixSizedByte(string str, int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < Math.Min(str.Length, length); i++)
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
        private static string FromFixSizedByte(byte[] bytes, int startIndex)
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
