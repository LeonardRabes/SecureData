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
            byte[] text = ToByte("Hallo du welt ic");
            AES aes = new AES(ToByte("test"));

            aes.ShiftRows(ref text, 0);
            aes.MixColumns(ref text, 0);

            aes.InvMixColumns(ref text, 0);
            aes.InvShiftRows(ref text, 0);

            

            MessageBox.Show(ToString(text));

        }

        private byte[] ToByte(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }

            return bytes;
        }

        private string ToString(byte[] bytes)
        {
            string str = "";
            foreach (var b in bytes)
            {
                str += (char)b;
            }

            return str;
        }
    }
}
