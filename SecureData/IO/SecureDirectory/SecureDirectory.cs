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

        private enum FileIndices
        {
            Header = 0,
            InternalKey = 8,
            TreeRef = 24,
            MemoryRef = 32,
            MemoryData = 40
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

            //Internal key
            var rng = new RNGCryptoServiceProvider();
            _internalKey = new byte[_internalKeySize];
            rng.GetBytes(_internalKey);
            WriteInternalKey((long)FileIndices.InternalKey);

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
            _directoryStream.Position = (long)FileIndices.TreeRef;
            writer.Write((long)0);

            //Memory
            _directoryStream.Position = (long)FileIndices.MemoryRef;
            writer.Write((long)0);
            _directoryManager = new MemoryManager(_directoryStream, (long)FileIndices.MemoryData, 0, new int[0], new int[0]);
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
            _directoryStream.Position = (long)FileIndices.Header;
            reader.ReadBytes(_secureDirType.Length);
            reader.ReadBytes(cypher.CypherIdentifier.Length);

            //Internal key
            ReadInternalKey((long)FileIndices.InternalKey);

            //Tree
            _directoryStream.Position = (long)FileIndices.TreeRef;
            ReadTree(reader.ReadInt64());
            ActiveDirectory = Tree;

            //Memory
            _directoryStream.Position = (long)FileIndices.MemoryRef;
            ReadMemoryInfo(reader.ReadInt64());

            _userKey = BinaryTools.StringToBytes(key);

            byte[] header = new byte[7];
            _directoryStream.Read(header, 0, header.Length);
        }

        public void Save()
        {
            //Tree
            _directoryStream.Position = (long)FileIndices.TreeRef;
            _directoryStream.Write(BitConverter.GetBytes(_directoryStream.Length), 0, 8);
            WriteTree(_directoryStream.Length);

            //Memory
            _directoryStream.Position = (long)FileIndices.MemoryRef;
            _directoryStream.Write(BitConverter.GetBytes(_directoryStream.Length), 0, 8);
            WriteMemoryInfo(_directoryStream.Length);
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
            int intByteSize = 4;

            //read length 
            _directoryStream.Position = offset;
            int length = Math.Max(_cypher.MinDataSize, intByteSize);
            byte[] bytes = new byte[length];
            _directoryStream.Read(bytes, 0, length);
            _cypher.Decrypt(ref bytes, 0, _internalKey);

            //read encrypted tree data
            _directoryStream.Position = offset;
            length = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[_cypher.MinDataSize * (int)Math.Ceiling((length + intByteSize) / (float)_cypher.MinDataSize)];
            _directoryStream.Read(bytes, 0, bytes.Length);
            _cypher.Decrypt(ref bytes, 0, _internalKey);

            //shorten array copy all bytes, that have index >= lengthByteSize && index < length 
            byte[] tree = new byte[length];
            Array.Copy(bytes, intByteSize, tree, 0, length);

            Tree = BinaryTools.DeserializeObject<SDir>(tree);
        }

        private void WriteTree(long offset)
        {
            byte[] tree = BinaryTools.SerializeObject(Tree);
            byte[] length = BitConverter.GetBytes(tree.Length);

            //copy into single array
            byte[] bytes = new byte[length.Length + tree.Length];
            length.CopyTo(bytes, 0);
            tree.CopyTo(bytes, length.Length);

            //encrypt
            _cypher.Padding(ref bytes);
            _cypher.Encrypt(ref bytes, 0, _internalKey);

            //write to stream
            _directoryStream.Position = offset;
            _directoryStream.Write(bytes, 0, bytes.Length);
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
    }
}
