using System;
using System.IO;
using System.Security.Cryptography;
using DataEncrypter.Cyphers;

namespace DataEncrypter.IO
{
    public partial class SecureDirectory
    {
        public SDir Tree { get; private set; }
        public SDir ActiveDirectory { get; private set; }

        private ICypher _cypher;
        private FileStream _directoryStream;
        private byte[] _userKey;
        private byte[] _internalKey;

        private static char _rootDirIdentifier = 'S';

        public SecureDirectory(string key, Cypher method = Cypher.AES)
        {
            switch (method)
            {
                case Cypher.AES:
                    _cypher = new AES(BinaryTools.StringToBytes(key));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Create(string filePath, string key)
        {
            var rng = new RNGCryptoServiceProvider();
            _userKey = BinaryTools.StringToBytes(key);
            _internalKey = new byte[16];
            rng.GetBytes(_internalKey);

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

        public void Open(string filePath)
        {
            throw new NotImplementedException();
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


        private void StreamToTree(int offset)
        {
            throw new NotImplementedException();
        }

        private void TreeToStream(int offset)
        {
            throw new NotImplementedException();
        }
    }
}
