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
using DataEncrypter.CryptMethods;
using DataEncrypter.IO;

namespace DataEncrypter
{
    public partial class MainForm : Form
    {
        private SecureFile _secureFile;
        private string _filePath = "";
        private bool _isSecureFile = false;
        private bool _canBeSaved = false;
        private string _key = "Key";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {

        }

        private void SecureFile_ProcessCompleted(object sender, ChunkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                _canBeSaved = true;
                save_button.Enabled = true;
                ChangeAllEnabled(true);
                LogMessage($"{e.Type.ToString()} Successfully Completed in {e.TotalTime.ToString("c")}");
            }));
        }

        private void SecureFile_ChunkUpdate(object sender, ChunkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                chunkUpdate_progressBar.Value = Convert.ToInt32(e.CompletedChunks / (float)e.TotalChunks * 100F);
                double eta = e.TotalTime.TotalMilliseconds / e.CompletedChunks * (e.TotalChunks - e.CompletedChunks) / 1000;
                eta_label.Text = $"ETA: {eta.ToString("0.0")} Seconds";

                LogMessage($"ChunkCompleted[{e.CompletedChunks}/{e.TotalChunks}]:" +
                    $" Size: {e.ChunkSize} bytes Time: {e.ChunkTime.TotalMilliseconds.ToString("0.0")}ms");
            }));
        }

        private void StartProcess_button_Click(object sender, EventArgs e)
        {
            ChangeAllEnabled(false);
            chunkUpdate_progressBar.Value = 0;

            if (mode_comboBox.SelectedIndex == 0)
            {
                Task.Run(() => _secureFile.Encrypt());
            }
            else if (mode_comboBox.SelectedIndex == 1)
            {
                Task.Run(() => _secureFile.Decrypt());
            }     
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _secureFile?.Dispose();
        }

        private void SelectFile_button_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.InitialDirectory = "";
                fileDialog.Filter = "All files (*.*)|*.*";
                fileDialog.FilterIndex = 0;
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_filePath != fileDialog.FileName)
                    {
                        //Get the path of specified file
                        _filePath = fileDialog.FileName;
                        fileName_label.Text = Path.GetFileName(_filePath);

                        //check if its a secure file
                        if (SecureFile.IsSecureFile(_filePath))
                        {
                            mode_comboBox.SelectedItem = mode_comboBox.Items[1];
                            _isSecureFile = true;
                        }
                        else
                        {
                            mode_comboBox.SelectedItem = mode_comboBox.Items[0];
                        }

                        //set file
                        _secureFile?.UpdateFile(_filePath);

                        if (_secureFile == null)
                        {
                            _secureFile = new SecureFile(_filePath, _key);
                            _secureFile.ChunkUpdate += SecureFile_ChunkUpdate;
                            _secureFile.ProcessCompleted += SecureFile_ProcessCompleted;
                        }

                        key_textBox.Enabled = true;
                    }   
                }
            }

            CheckKey();
        }

        private void Key_textBox_TextChanged(object sender, EventArgs e)
        {
            CheckKey();
        }

        private void Key_textBox_Enter(object sender, EventArgs e)
        {
            if (key_textBox.Text == "Key")
            {
                key_textBox.Text = "";
                key_textBox.ForeColor = SystemColors.WindowText;
            }
        }

        private void Key_textBox_Leave(object sender, EventArgs e)
        {
            if (key_textBox.Text.Length == 0)
            {
                key_textBox.Text = "Key";
                key_textBox.ForeColor = SystemColors.GrayText;
            }
        }

        private void Save_button_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new SaveFileDialog())
            {
                string suggestion = _secureFile.SuggestSaveFileName();
                string extensionFilter = $"{Path.GetExtension(suggestion)} | *{Path.GetExtension(suggestion)}";

                fileDialog.InitialDirectory = "";
                fileDialog.Filter = extensionFilter + "| All files (*.*)|*.*";
                fileDialog.FilterIndex = 0;
                fileDialog.RestoreDirectory = true;
                fileDialog.FileName = Path.GetFileNameWithoutExtension(suggestion);

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    _secureFile.Save(fileDialog.FileName);
                }
            }
        }

        private void Mode_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckKey();
        }

        private void CheckKey()
        {
            if (_filePath != "" && key_textBox.Text != "Key" && key_textBox.Text != "")
            {
                _key = key_textBox.Text;
                _secureFile.UpdateKey(_key);

                if (mode_comboBox.SelectedIndex == 1)
                {
                    if (_secureFile.ValidateKeyForDecryption())
                    {
                        keyStatus_label.Text = "Key is Correct!";
                        keyStatus_label.ForeColor = Color.Green;
                        start_button.Enabled = true;
                    }
                    else
                    {
                        keyStatus_label.Text = "Key is Incorrect!";
                        keyStatus_label.ForeColor = Color.Red;
                        start_button.Enabled = false;
                    }
                }
                else if (mode_comboBox.SelectedIndex == 0)
                {
                    keyStatus_label.Text = "Key is Viable!";
                    keyStatus_label.ForeColor = Color.Green;

                    start_button.Enabled = true;
                }
            }
            else if (key_textBox.Text == "Key" || key_textBox.Text == "")
            {
                keyStatus_label.Text = "Key is Missing!";
                keyStatus_label.ForeColor = Color.Red;
                start_button.Enabled = false;
            }
        }

        private void ChangeAllEnabled(bool enabled)
        {
            selectFile_button.Enabled = enabled;
            key_textBox.Enabled = enabled;
            mode_comboBox.Enabled = enabled;
        }

        private void LogMessage(string message)
        {
            int maxLines = 100;
            int resetTo = 30;

            if (log_textBox.Lines.Length >= maxLines)
            {
                string[] lines = new string[resetTo];
                for (int i = maxLines - resetTo; i < log_textBox.Lines.Length; i++)
                {
                    lines[i - (maxLines - resetTo)] = log_textBox.Lines[i];
                }

                log_textBox.Lines = lines;
            }

            log_textBox.Text += message + "\n";
            log_textBox.SelectionStart = log_textBox.Text.Length;
            log_textBox.ScrollToCaret();
        }
    }
}
