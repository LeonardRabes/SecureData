using System;
using System.IO;
using System.Security.Cryptography;
using SecureData.Cyphers;

namespace SecureData.IO
{
    public partial class SecureDirectory : IDisposable
    {
        public SDir Tree { get; private set; }
        public SDir ActiveDirectory { get; private set; }

        private ICypher _cypher;
        private string _directoryPath;
        private FileStream _directoryStream;
        private MemoryManager _directoryManager;
        private byte[] _userKey;
        private byte[] _internalKey;

        private const int _internalKeySize = 16;
        private const char _rootDirIdentifier = 'S';
        private const string _secureDirType = "SECD";
        private const string _secureDirExtension = ".secd";
        private const string _decryptionValidation = "decryption_valid";

        private struct FileIndices
        {
            public const long Header = 0;
            public const long DecryptionValidation = 8;
            public const long InternalKey = 24;
            public const long TreeRef = 40;
            public const long MemoryRef = 48;
            public const long MemoryData = 56;
        }

        public SecureDirectory()
        {

        }

        public void Create(string filePath, ICypher cypher, string key)
        {
            _directoryPath = filePath;
            _directoryStream = new FileStream(filePath, FileMode.Create);
            var writer = new BinaryWriter(_directoryStream);

            //Cypher
            _cypher = cypher;
            _userKey = BinaryTools.StringToBytes(key);

            //Header
            writer.Write(BinaryTools.StringToBytes(_secureDirType));
            writer.Write(BinaryTools.StringToBytes(_cypher.CypherIdentifier));

            //Decryption Validation (encyrpted with _userKey)
            _directoryStream.Position = FileIndices.DecryptionValidation;
            byte[] validation = BinaryTools.StringToBytes(_decryptionValidation);
            _cypher.Encrypt(ref validation, 0, _userKey);
            _directoryStream.Write(validation, 0, _decryptionValidation.Length);

            //Internal key
            var rng = new RNGCryptoServiceProvider();
            _internalKey = new byte[_internalKeySize];
            rng.GetBytes(_internalKey);
            WriteInternalKey(FileIndices.InternalKey);

            //Tree
            Tree = new SDir
            {
                Name = _rootDirIdentifier + "",
                SecurePath = _rootDirIdentifier + "",
                Parent = null,
                Children = new SDir[0],
                Files = new SFile[0]
            };
            ActiveDirectory = Tree;
            _directoryStream.Position = FileIndices.TreeRef;

            //Memory
            _directoryStream.Position = FileIndices.MemoryRef;
            _directoryManager = new MemoryManager(_directoryStream, FileIndices.MemoryData, 0, new int[0], new int[0]);
        }

        public void Open(string filePath, ICypher cypher, string key)
        {
            _directoryPath = filePath;
            _directoryStream = new FileStream(filePath, FileMode.Open);
            var reader = new BinaryReader(_directoryStream);

            //Cypher
            _cypher = cypher;
            _userKey = BinaryTools.StringToBytes(key);

            //Header
            _directoryStream.Position = FileIndices.Header;
            reader.ReadBytes(_secureDirType.Length);
            reader.ReadBytes(cypher.CypherIdentifier.Length);

            //Internal key
            ReadInternalKey(FileIndices.InternalKey);

            //Tree
            _directoryStream.Position = FileIndices.TreeRef;
            ReadTree(reader.ReadInt64());
            ActiveDirectory = Tree;

            //Memory
            _directoryStream.Position = FileIndices.MemoryRef;
            ReadMemoryInfo(reader.ReadInt64());

            _userKey = BinaryTools.StringToBytes(key);

            byte[] header = new byte[7];
            _directoryStream.Read(header, 0, header.Length);
        }

        public void Save()
        {
            long offset = FileIndices.MemoryRef + _directoryManager.SectorCount * (long)MemoryManager.SectorSize;

            //Tree
            _directoryStream.Position = FileIndices.TreeRef;
            _directoryStream.Write(BitConverter.GetBytes(offset), 0, 8);
            WriteTree(offset);

            offset = _directoryStream.Position;

            //Memory
            _directoryStream.Position = FileIndices.MemoryRef;
            _directoryStream.Write(BitConverter.GetBytes(offset), 0, 8);
            WriteMemoryInfo(offset);

            _directoryStream.Flush();
        }

        

        public void Dispose()
        {
            _directoryStream.Close();
        }

        private void ReadInternalKey(long offset)
        {
            _directoryStream.Position = offset;
            _internalKey = new byte[_internalKeySize];
            _directoryStream.Read(_internalKey, 0, _internalKeySize);

            _cypher.Decrypt(ref _internalKey, 0, _userKey);
        }

        private void WriteInternalKey(long offset)
        {
            _directoryStream.Position = offset;
            byte[] key = new byte[_internalKeySize];
            _internalKey.CopyTo(key, 0);

            _cypher.Encrypt(ref key, 0, _userKey);

            _directoryStream.Write(key, 0, _internalKeySize);
        }

        private void ReadTree(long offset)
        {
            var reader = new BinaryReader(_directoryStream);
            _directoryStream.Position = offset;

            //read length 
            int length = reader.ReadInt32();

            //read encrypted tree data
            byte[] bytes = new byte[length];
            _cypher.Padding(ref bytes);
            _directoryStream.Read(bytes, 0, bytes.Length);

            _cypher.Decrypt(ref bytes, 0, _internalKey);

            //shorten array copy all bytes, that have index >= lengthByteSize && index < length 
            byte[] tree = new byte[length];
            Array.Copy(bytes, 0, tree, 0, length);

            Tree = BinaryTools.DeserializeObject<SDir>(tree);
        }

        private void WriteTree(long offset)
        {
            var writer = new BinaryWriter(_directoryStream);
            _directoryStream.Position = offset;

            //deserialize tree
            byte[] tree = BinaryTools.SerializeObject(Tree);

            writer.Write(tree.Length);

            //encrypt
            _cypher.Padding(ref tree);
            _cypher.Encrypt(ref tree, 0, _internalKey);

            //write to stream
            writer.Write(tree);
        }


        private void ReadMemoryInfo(long offset)
        {
            _directoryStream.Position = offset;
            var reader = new BinaryReader(_directoryStream);

            long startIndex = reader.ReadInt64();
            int sectorCount = reader.ReadInt32();

            int length = reader.ReadInt32();
            int[] allocSectors = new int[length];
            for (int i = 0; i < length; i++)
            {
                allocSectors[i] = reader.ReadInt32();
            }

            length = reader.ReadInt32();
            int[] occSectors = new int[length];
            for (int i = 0; i < length; i++)
            {
                occSectors[i] = reader.ReadInt32();
            }

            _directoryManager = new MemoryManager(_directoryStream, startIndex, sectorCount, allocSectors, occSectors);
        }

        private void WriteMemoryInfo(long offset)
        {
            _directoryStream.Position = offset;
            var writer = new BinaryWriter(_directoryStream);

            writer.Write(_directoryManager.AllocationStartIndex);
            writer.Write(_directoryManager.SectorCount);

            writer.Write(_directoryManager.AllocatableSectors.Length);
            foreach (var c in _directoryManager.AllocatableSectors)
            {
                writer.Write(c);
            }

            writer.Write(_directoryManager.OccupiedSectors.Length);
            foreach (var c in _directoryManager.OccupiedSectors)
            {
                writer.Write(c);
            }
        }

        /// <summary>
        /// Checks if the current key is able to decrypt the file.
        /// </summary>
        /// <returns>Boolean, which indicates if the key is valid.</returns>
        public static bool ValidateKeyForDecryption(string filePath, ICypher cypher, string key)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            byte[] validation = new byte[_decryptionValidation.Length];

            fs.Position = (long)FileIndices.DecryptionValidation;
            fs.Read(validation, 0, validation.Length);
            fs.Close();

            cypher.Decrypt(ref validation, 0, BinaryTools.StringToBytes(key));

            return BinaryTools.BytesToString(validation) == _decryptionValidation;
        }
    }
}
