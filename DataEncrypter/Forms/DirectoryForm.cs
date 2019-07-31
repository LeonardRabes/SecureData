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
using SecureData.IO;
using SecureData.Cyphers;

namespace DataEncryption.Forms
{
    public partial class DirectoryForm : Form
    {
        Bitmap bmp;
        public DirectoryForm()
        {
            InitializeComponent();
        }

        private void DirectoryForm_Load(object sender, EventArgs e)
        {

        }

        private void DirectoryForm_Paint(object sender, PaintEventArgs e)
        {
            if (bmp != null)
            {
                e.Graphics.DrawImage(bmp, new Rectangle(20, 20, 300, 300));
            }
        }

        private void Open()
        {
            var dir = new SecureDirectory();
            dir.Open("test.secf", new AES(), "Test");
            dir.MoveToChild("New Folder");
            dir.MoveToChild("TopSecret");

            var mstream = new MemoryStream();
            //var fstream = new FileStream("test.txt", FileMode.Create);
            dir.LoadFile(dir.ActiveDirectory.Files[1], mstream);
            bmp = new Bitmap(mstream);

            this.Refresh();
            mstream.Close();
        }

        private void Create()
        {
            var dir = new SecureDirectory();

            dir.Create("test.secf", new AES(), "Test");
            dir.AddDirectory("New Folder");
            dir.MoveToChild("New Folder");
            dir.AddDirectory("TopSecret");
            dir.MoveToChild("TopSecret");

            var file = dir.AddFile("night.txt");
            dir.AddFile("night.jpg");

            dir.RemoveFile(file);
            dir.AddFile("desert.jpg");

            dir.Save();
            dir.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Create();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Open();
        }


    }
}
