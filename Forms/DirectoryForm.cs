using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataEncrypter.IO;

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
            var dir = new SecureDirectory();
            dir.Create("dir.secd", "test");
            dir.AddDirectory("test");
            dir.MoveToChild("test");
            dir.AddDirectory("intest");

            dir.Save();

            dir = new SecureDirectory();
            dir.Open("dir.secd");
        }
    }
}
