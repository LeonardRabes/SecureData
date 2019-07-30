using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DataEncrypter.IO;
using DataEncrypter.Cyphers;

namespace DataEncrypter.Forms
{
    public partial class DirectoryForm : Form
    {
        public DirectoryForm()
        {
            InitializeComponent();
        }

        private void DirectoryForm_Load(object sender, EventArgs e)
        {
            var fs = new FileStream("out.stream", FileMode.Create);
            var mem = new SecureDirectory.MemoryManager(fs, 0, 0, new int[0], new int[0]);
            
            var source = new FileStream("night.txt", FileMode.Open);

            int[] chunks = mem.AllocateBytes(source.Length);
            mem.SecureWrite(source, chunks, new AES(), new byte[] { 5, 7, 25, 75, 66 });

            mem.Deallocate(new int[] { 15, 16, 17, 18, 25, 36 });
            chunks = mem.AllocateBytes(source.Length);

            mem.SecureWrite(source, chunks, new AES(), new byte[] { 5, 7, 25, 75, 66 });

            var output = new FileStream("test.txt", FileMode.Create);
            mem.SecureRead(output, source.Length, chunks, new AES(), new byte[] { 5, 7, 25, 75, 66 });

            fs.Close();
            output.Close();
        }
    }
}
