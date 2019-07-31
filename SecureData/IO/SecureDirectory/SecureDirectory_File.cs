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
            var fstream = new FileStream(filePath, FileMode.Open);

            int[] chunks = _directoryManager.AllocateBytes(fstream.Length);
            _directoryManager.SecureWrite(fstream, chunks, _cypher, _internalKey);

            var newFile = new SFile()
            {
                Name = Path.GetFileName(filePath),
                SecurePath = Path.Combine(ActiveDirectory.SecurePath, Path.GetFileName(filePath)),
                Parent = ActiveDirectory,

                Size = fstream.Length,
                MemoryChunks = chunks
            };

            var f = ActiveDirectory.Files;
            Array.Resize(ref f, f.Length + 1);

            f[f.Length - 1] = newFile;

            ActiveDirectory.Files = f;
            fstream.Dispose();

            return newFile;
        }

        public bool RemoveFile(SFile file)
        {
            var list = new List<SFile>(file.Parent.Files);
            bool success = list.Remove(file);

            success = _directoryManager.Deallocate(file.MemoryChunks) && success;

            if (success)
            {
                file.Parent.Files = list.ToArray();
            }

            return success;
        }

        public void LoadFile(SFile file, Stream output)
        {
            _directoryManager.SecureRead(output, file.Size, file.MemoryChunks, _cypher, _internalKey);
        }
    }
}
