using System;
using System.Collections.Generic;
using System.IO;

namespace SecureData.IO
{
    public partial class SecureDirectory
    {
        [Serializable]
        public class SFile
        {
            public string Name { get; set; }
            public string SecurePath { get; set; }
            public SDir Parent { get; set; }

            public long Size { get; set; }
            public int[] MemoryChunks { get; set; }
        }

        public SFile AddFile(string filePath)
        {
            using (var fstream = new FileStream(filePath, FileMode.Open))
            {
                var file = AddFile(fstream, Path.GetFileName(filePath));

                return file;
            }
        }

        public SFile AddFile(Stream source, string fileName)
        {
            int[] chunks = _directoryManager.AllocateBytes(source.Length);
            _directoryManager.SecureWrite(source, chunks, _cypher, _internalKey);

            var newFile = new SFile()
            {
                Name = fileName,
                SecurePath = Path.Combine(ActiveDirectory.SecurePath, fileName),
                Parent = ActiveDirectory,

                Size = source.Length,
                MemoryChunks = chunks
            };

            var f = ActiveDirectory.Files;
            Array.Resize(ref f, f.Length + 1);

            f[f.Length - 1] = newFile;

            ActiveDirectory.Files = f;

            return newFile;
        }

        public bool RemoveFile(string securePath)
        {
            bool found = FindFile(securePath, out SFile file);
            bool removed = false;

            if (found)
            {
                removed = RemoveFile(file);
            }

            return found && removed;
        }

        public bool RemoveFile(SFile file)
        {
            var list = new List<SFile>(file.Parent.Files);
            bool success = list.Remove(file);

            _directoryManager.Deallocate(file.MemoryChunks);

            if (success)
            {
                file.Parent.Files = list.ToArray();
            }

            return success;
        }

        public SFile CopyFile(string currentSecurePath, string targetSecurePath)
        {
            throw new NotImplementedException();
        }

        public SFile MoveFile(string currentSecurePath, string targetSecurePath)
        {
            throw new NotImplementedException();
        }

        public void LoadFile(SFile file, Stream output)
        {
            _directoryManager.SecureRead(output, file.Size, file.MemoryChunks, _cypher, _internalKey);
        }

        public bool FindFile(string securePath, out SFile foundFile)
        {
            if (FindDirectory(Path.GetDirectoryName(securePath), out SDir dir))
            {
                string name = Path.GetFileName(securePath);
                foundFile = Array.Find(dir.Files, x => x.Name == name);

                return foundFile != null;
            }
            else
            {
                foundFile = null;
                return false;
            }
        }
    }
}
