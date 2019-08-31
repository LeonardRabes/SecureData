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

            return ActiveDirectory;
        }

        public void SetActiveDir(SDir dir)
        {
            ActiveDirectory = dir;
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

        public bool MoveToChild(int childID)
        {
            bool exists = childID >= 0 && childID < ActiveDirectory.Children.Length;
            if (exists)
            {
                ActiveDirectory = ActiveDirectory.Children[childID];
            }

            return exists;
        }

        public SDir AddDirectory(string securePath)
        {
            string name = Path.GetDirectoryName(securePath);
            bool success = FindDirectory(securePath.Replace(name, ""), out SDir parent);

            var newDir = new SDir
            {
                Name = name,
                SecurePath = securePath,
                Parent = parent,
                Children = new SDir[0],
                Files = new SFile[0]
            };

            if (success)
            {
                var c = ActiveDirectory.Children;
                Array.Resize(ref c, c.Length + 1);
                c[c.Length - 1] = newDir;

                ActiveDirectory.Children = c;
                return newDir;
            }
            else
            {
                return null;
            }
        }

        public bool DeleteDirectory(string securePath)
        {
            var success = FindDirectory(securePath, out SDir foundDir);

            if (success)
            {
                DeleteDirectory(foundDir);
            }

            return success;
        }

        public void DeleteDirectory(SDir dir)
        {
            deleteFiles(dir);
            var children = dir.Parent.Children;
            var newChildren = new SDir[children.Length - 1];

            int count = 0;
            for (int i = 0; i < children.Length; i++)
            {
                if (dir != children[i])
                {
                    newChildren[count++] = children[i];
                }
            }

            dir.Parent.Children = newChildren;

            void deleteFiles(SDir target)
            {
                foreach (var file in target.Files)
                {
                    RemoveFile(file);
                }

                foreach (var child in target.Children)
                {
                    deleteFiles(child);
                }
            }
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

        public SDir CopyDirectory(string currentSecurePath, string targetSecurePath)
        {
            throw new NotImplementedException();
        }

        public SDir MoveDirectory(string currentSecurePath, string targetSecurePath)
        {
            throw new NotImplementedException();
        }

        public string RelativeToAbsolutePath(string relative)
        {
            if (relative[0] == '.')
            {
                return Path.Combine(ActiveDirectory.SecurePath, relative.Substring(1));
            }
            else
            {
                return relative;
            }
        }
    }
}
