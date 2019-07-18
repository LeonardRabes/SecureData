using System;
using System.IO;
using DataEncrypter.Cyphers;

namespace DataEncrypter.IO
{
    public partial class SecureDirectory
    {
        public SDir Tree { get; private set; }
        public SDir ActiveDirectory { get; private set; }

        private ICypher _cypher;
        private FileStream _directoryStream;
        

        private static char _rootDirIdentifier = 'S';

        public SecureDirectory(string key, Cypher method = Cypher.AES)
        {
            switch (method)
            {
                case Cypher.AES:
                    _cypher = new AES(Misc.StringToBytes(key));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Create(string filePath)
        {
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
            throw new NotImplementedException();
        }

        public void MoveToParent()
        {
            ActiveDirectory = ActiveDirectory.Parent;
        }

        public void MoveToChild(string name)
        {
            ActiveDirectory = Array.Find(ActiveDirectory.Children, d => d.Name == name);
        }

        public SDir CreateDirectory(string name)
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

        public bool FindDirectory(string securePath, out SDir foundDir)
        {
            string[] dirNames = securePath.Split(Path.DirectorySeparatorChar);

            SDir dir = null;
            bool found = false;

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

        }

        private void TreeToStream(int offset)
        {

        }
    }
}
