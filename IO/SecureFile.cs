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
    public class SecureFile
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

        private ICypher _cypher;
        private byte[] _cryptType;

        private static string _secureFileType = "SECF"; //abreviation: SECureFile
        private static string _secureFileExtension = ".secf";
        private static int _secureHeaderSize = 80;
        private static int _chunkSize = 1_048_576; //int.MaxValue / 2048 => roughly 1mb
        private static string _decryptionValidation = "decryption_valid";

        /// <summary>
        /// Contructs basic data for file en-/decryption. This includes a temporary file to save data.
        /// </summary>
        /// <param name="filePath">File path to the file, which will be en-/decrypted.</param>
        /// <param name="key">The key for the Cypher</param>
        /// <param name="method">The method of en-/decryption</param>
        public SecureFile(string key, Cypher method = Cypher.AES)
        {
            switch (method)
            {
                case Cypher.AES:
                    _cypher = new AES(BinaryTools.StringToBytes(key));
                    _cryptType = BinaryTools.StringToBytes(_secureFileType + "AES");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Encrypts the file, which was provided to this instance.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="saveDir">Directory, where the encrypted file is saved</param>
        /// <param name="deleteOriginal">Determines if the file of filePath is deleted</param>
        public void Encrypt(string filePath, string saveDir = "", bool deleteOriginal = false)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            FileStream targetFile = OpenTargetFile(filePath);
            FileStream saveFile = CreateSaveFile(Path.Combine(saveDir, fileName + _secureFileExtension));

            var writer = new BinaryWriter(saveFile);
            var reader = new BinaryReader(targetFile);

            #region write header
            //write unsecure header
            writer.Write(_cryptType);                                                //file type | 7bytes

            //secure header | 80 bytes
            List<byte> secureHeader = new List<byte>();
            secureHeader.AddRange(BitConverter.GetBytes(targetFile.Length));        //length of orig file | 8bytes
            secureHeader.AddRange(BinaryTools.StringToFixSizedByte(fileName, 40));                    //orig name of file | 40bytes
            secureHeader.AddRange(BinaryTools.StringToFixSizedByte(fileExtension, 16));               //orig file extension | 16bytes
            secureHeader.AddRange(BinaryTools.StringToBytes(_decryptionValidation));                   //validation string to determine if decryption is valid | 16bytes

            byte[] sh = secureHeader.ToArray();
            _cypher.Encrypt(ref sh, 0);
            writer.Write(sh); //write encrypted secure header to stream
            #endregion

            #region encryption
            //timer
            TimeSpan totalTime = new TimeSpan();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //encryption of file
            while (targetFile.Length - targetFile.Position > 1)
            {
                byte[] state = reader.ReadBytes((int)Math.Min(targetFile.Length - targetFile.Position, _chunkSize)); //read chunks from file or the entire file if file < 1mb
 
                _cypher.Padding(ref state); //add padding to have no incomplete blocks
                
                _cypher.Encrypt(ref state, 0);

                writer.Write(state);

                //Events and Measurement
                totalTime = totalTime.Add(stopWatch.Elapsed);
                OnChunkUpdate(targetFile.Position, targetFile.Length, state.Length, totalTime, stopWatch.Elapsed, ChunkEventArgs.ProcessType.Decryption);
                stopWatch.Restart();
            }

            stopWatch.Stop();
            OnProcessCompleted(targetFile.Length, totalTime, ChunkEventArgs.ProcessType.Encryption);
            #endregion

            CloseTargetFile(targetFile, deleteOriginal);
            saveFile.Close();
        }

        /// <summary>
        /// Decrypts the file, which was provided to this instance.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="saveDir">Directory, where the decrypted file is saved</param>
        /// <param name="deleteOriginal">Determines if the file of filePath is deleted</param>
        public void Decrypt(string filePath, string saveDir = "", bool deleteOriginal = false)
        {
            FileStream targetFile = OpenTargetFile(filePath);
            var reader = new BinaryReader(targetFile);

            #region read secure header
            //check file type
            if (reader.ReadInt32() != BitConverter.ToInt32(BinaryTools.StringToBytes(_secureFileType), 0))
            {
                throw new Exception("Not SECF FileType!");
            }
            string encryptionMethod = BinaryTools.BytesToString(reader.ReadBytes(3)); //read encryption method

            //secure header | 80bytes
            byte[] secureHeader = reader.ReadBytes(_secureHeaderSize);
            _cypher.Decrypt(ref secureHeader, 0);

            long fileLength = BitConverter.ToInt64(secureHeader, 0);              //read length of original file | 8bytes
            string fileName = BinaryTools.StringFromFixSizedByte(secureHeader, 8);       //name of original file | 40bytes
            string fileExtension = BinaryTools.StringFromFixSizedByte(secureHeader, 48); //file extension of original file | 16bytes
            string validation = BinaryTools.BytesToString(secureHeader, 64);             //read validation | 16bytes

            if (validation != _decryptionValidation) //check if key is correct
            {
                throw new Exception("Incorrect Key!");
            }
            #endregion

            FileStream saveFile = CreateSaveFile(Path.Combine(saveDir, fileName + fileExtension));
            var writer = new BinaryWriter(saveFile);

            #region decryption
            //timer
            TimeSpan totalTime = new TimeSpan();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //decryption of file
            while (targetFile.Length - targetFile.Position > 1)
            {
                byte[] state = reader.ReadBytes((int)Math.Min(targetFile.Length - targetFile.Position, _chunkSize)); //read chunks from file or the entire file if file < 1mb

                _cypher.Decrypt(ref state, 0);

                writer.Write(state);

                //Events and Measurement
                totalTime = totalTime.Add(stopWatch.Elapsed);
                OnChunkUpdate(targetFile.Position, targetFile.Length, state.Length, totalTime, stopWatch.Elapsed, ChunkEventArgs.ProcessType.Decryption);
                stopWatch.Restart();
            }
            saveFile.SetLength(fileLength); //set stream to original length and remove padding

            stopWatch.Stop();
            OnProcessCompleted(targetFile.Length, totalTime, ChunkEventArgs.ProcessType.Decryption);
            #endregion

            CloseTargetFile(targetFile, deleteOriginal);
            saveFile.Close();
        }


        /// <summary>
        /// Updates the key for en-/decryption.
        /// </summary>
        /// <param name="key">Key for en-/decryption</param>
        public void UpdateKey(string key)
        {
            _cypher.UpdateKey(BinaryTools.StringToBytes(key));
        }

        /// <summary>
        /// Checks if the current key is able to decrypt the file.
        /// </summary>
        /// <returns>Boolean, which indicates if the key is valid.</returns>
        public bool ValidateKeyForDecryption(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            byte[] validation = new byte[_decryptionValidation.Length];

            fs.Position = _cryptType.Length + _secureHeaderSize - _decryptionValidation.Length;
            fs.Read(validation, 0, validation.Length);
            fs.Close();

            _cypher.Decrypt(ref validation, 0);

            return BinaryTools.BytesToString(validation) == _decryptionValidation;
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        private FileStream OpenTargetFile(string filePath)
        {
            var workFile = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return workFile;
        }

        /// <summary>
        /// Closes a FileStream
        /// </summary>
        /// <param name="targetFile">FileStream to be closed</param>
        /// <param name="delete">Delete file afterwards</param>
        private void CloseTargetFile(FileStream targetFile, bool delete = false)
        {
            string path = targetFile.Name;
            targetFile.Close();

            if (delete)
            {
                bool deleted = SecureDelete.DeleteFile(path);
                if (!deleted)
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Creates a temporary file for saving data to.
        /// </summary>
        private FileStream CreateSaveFile(string filePath)
        {
            //create a temporary file to save data to
            var tempFile = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            return tempFile;
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

            return BinaryTools.BytesToString(secf) == _secureFileType;
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
    }
}
