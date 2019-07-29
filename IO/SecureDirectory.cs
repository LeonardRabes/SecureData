using System;
using System.IO;
using System.Security.Cryptography;
using DataEncrypter.Cyphers;

namespace DataEncrypter.IO
{
    public partial class SecureDirectory : IDisposable
    {
        public SDir Tree { get; private set; }
        public SDir ActiveDirectory { get; private set; }

        private ICypher _cypher;
        private string _directoryPath;
        private FileStream _directoryStream;
        private byte[] _userKey;
        private byte[] _internalKey;

        private static int _headerSize = 7;
        private static int _internalKeySize = 16;
        private static char _rootDirIdentifier = 'S';
        private static string _secureDirType = "SECD";
        private static string _secureDirExtension = ".secd";

        public SecureDirectory()
        {

        }

        public void Create(string filePath, string key, Cypher method = Cypher.AES)
        {
            _directoryPath = filePath;
            _directoryStream = new FileStream(filePath, FileMode.Create);

            var rng = new RNGCryptoServiceProvider();
            _userKey = BinaryTools.StringToBytes(key);
            _internalKey = new byte[_internalKeySize];
            rng.GetBytes(_internalKey);

            switch (method)
            {
                case Cypher.AES:
                    _cypher = new AES();
                    break;
                default:
                    throw new NotImplementedException();
            }

            Tree = new SDir
            {
                Name = _rootDirIdentifier + "",
                SecurePath = _rootDirIdentifier + "",
                Parent = null,
                Children = new SDir[0],
                Files = new SFile[0]
            };

            ActiveDirectory = Tree;
        }

        public void Open(string filePath, string key)
        {
            _directoryPath = filePath;
            _directoryStream = new FileStream(filePath, FileMode.Open);

            _userKey = BinaryTools.StringToBytes(key);

            byte[] header = new byte[7];
            _directoryStream.Read(header, 0, header.Length);

            //TODO: Get Cypher
            _cypher = new AES();

            ReadInternalKey(_headerSize);
            ReadTree(_headerSize + _internalKeySize);
        }

        public void Save()
        {
            WriteInternalKey(_headerSize);
            WriteTree(_headerSize + _internalKeySize);
        }

        public SDir SetActiveDir(string secureFilePath)
        {
            bool found = FindDirectory(secureFilePath, out SDir dir);

            if (found)
            {
                ActiveDirectory = dir;
            }

            return dir;
        }

        public bool MoveToParent()
        {
            SDir dir = ActiveDirectory.Parent;

            bool found = dir != null;

            if (found)
            {
                ActiveDirectory = dir;
            }

            return found;
        }

        public bool MoveToChild(string name)
        {
            var dir = Array.Find(ActiveDirectory.Children, d => d.Name == name);
            bool found = dir != null;

            if (found)
            {
                ActiveDirectory = dir;
            }

            return found;
        }

        public SDir AddDirectory(string name)
        {
            var c = ActiveDirectory.Children;
            Array.Resize(ref c, c.Length + 1);
            var newDir = new SDir
            {
                Name = name,
                SecurePath = Path.Combine(ActiveDirectory.SecurePath, name),
                Parent = ActiveDirectory,
                Children = new SDir[0],
                Files = new SFile[0]
            };

            c[c.Length - 1] = newDir;

            ActiveDirectory.Children = c;
            return newDir;
        }

        public SFile AddFile()
        {
            throw new NotImplementedException();
        }

        public bool FindDirectory(string securePath, out SDir foundDir)
        {
            string[] dirNames = securePath.Split(Path.DirectorySeparatorChar);

            SDir dir = null;
            bool found = false;

            findNext(0, Tree);

            void findNext(int depth, SDir currentDir)
            {
                if (Path.GetFileName(securePath) != currentDir.Name && depth < dirNames.Length)
                {
                    var next = Array.Find(currentDir.Children, d => d.Name == dirNames[depth]);
                    findNext(depth++, next);
                }
                else
                {
                    found = depth < dirNames.Length;
                    if (found)
                    {
                        dir = currentDir;
                    }
                }
            }

            foundDir = dir;
            return found;
        }

        public void Dispose()
        {
            _directoryStream.Close();
        }

        private void ReadInternalKey(int offset)
        {
            _directoryStream.Position = offset;
            _internalKey = new byte[_internalKeySize];
            _directoryStream.Read(_internalKey, 0, _internalKeySize);

            _cypher.Decrypt(ref _internalKey, 0, _userKey);
        }

        private void WriteInternalKey(int offset)
        {
            _directoryStream.Position = offset;
            byte[] key = new byte[_internalKeySize];
            _internalKey.CopyTo(key, 0);

            _cypher.Encrypt(ref key, 0, _userKey);

            _directoryStream.Write(key, 0, _internalKeySize);
        }

        private void ReadTree(int offset)
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

        private void WriteTree(int offset)
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
    }
}
