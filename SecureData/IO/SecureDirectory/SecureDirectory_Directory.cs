using System;
using System.IO;

namespace SecureData.IO
{
    public partial class SecureDirectory
    {
        [Serializable]
        public class SDir
        {
            public string Name { get; set; }
            public string SecurePath { get; set; }
            public SDir Parent { get; set; }
            public SDir[] Children { get; set; }
            public SFile[] Files { get; set; }
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
            var newDir = new SDir
            {
                Name = name,
                SecurePath = Path.Combine(ActiveDirectory.SecurePath, name),
                Parent = ActiveDirectory,
                Children = new SDir[0],
                Files = new SFile[0]
            };

            var c = ActiveDirectory.Children;
            Array.Resize(ref c, c.Length + 1);
            c[c.Length - 1] = newDir;

            ActiveDirectory.Children = c;
            return newDir;
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
    }
}
