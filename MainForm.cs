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
            AES aes = new AES(ToByte("Passwort01234567"));
            
            byte[] text = ToByte("TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest");

            aes.Encrypt(ref text);
            string str = ToString(text);

            aes.Decrypt(ref text);
            str = ToString(text);
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
