﻿using System;
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
            bool isSECF = SecureFile.IsSecureFile("soundfile.mp3");
            var file = new SecureFile("out.secf", "Passwort01234567abc");

            bool keyValid = file.ValidateKeyForDecryption();
            //file.Encrypt();
            file.Decyrpt();

            file.Save("", "out");

            file.Dispose();

            int maxLength = 536_870_912;
            byte[] array = new byte[maxLength];
        }
    }
}
