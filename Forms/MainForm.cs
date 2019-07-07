using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataEncrypter.CryptMethods;
using DataEncrypter.IO;

namespace DataEncrypter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var file = new SecureFile("testvid.secf", "Passwort01234567");
            //file.Encrypt();
            //file.Save("soundfile.secf");
            file.Decyrpt();

            file.Save();

            file.Dispose();

            int maxLength = 536_870_912;
            byte[] array = new byte[maxLength];
        }

        
    }
}
