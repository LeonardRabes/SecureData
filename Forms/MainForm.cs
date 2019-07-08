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
            
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {

            var file = new SecureFile("testvid.mp4", "Passwort01234567abc");
            file.ChunkUpdate += File_ChunkUpdate;
            file.ProcessCompleted += File_ProcessCompleted;
            bool keyValid = file.ValidateKeyForDecryption();
            file.Encrypt();
            //file.Decyrpt();

            file.Save("", "out");

            file.Dispose();

            int maxLength = 536_870_912;
            byte[] array = new byte[maxLength];
        }

        private void File_ProcessCompleted(object sender, ChunkEventArgs e)
        {
            
        }

        private void File_ChunkUpdate(object sender, ChunkEventArgs e)
        {
            chunkUpdate_progressBar.Value = Convert.ToInt32(e.CompletedChunks / (float)e.TotalChunks * 100F);
            double eta = e.ElapsedTime.TotalMilliseconds / e.CompletedChunks * (e.TotalChunks - e.CompletedChunks) / 1000;
            eta_label.Text = $"ETA: {eta.ToString("0.0")} Seconds";
            Refresh();
        }
    }
}
