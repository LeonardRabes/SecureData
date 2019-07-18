using System;
using System.IO;
using DataEncrypter.Cyphers;

namespace DataEncrypter.IO
{


    public partial class SecureDirectory
    {
        public class SDir
        {
            public string Name { get; set; }
            public string SecurePath { get; set; }
            public SDir Parent { get; set; }
            public SDir[] Children { get; set; }
            public SFile[] Files { get; set; }
        }

        public class SFile
        {
            public string Name { get; set; }
            public string SecurePath { get; set; }
            public SDir Parent { get; set; }
            public int StreamOffset { get; set; }
        }
    }
}
