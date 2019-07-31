namespace DataEncryption.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.chunkUpdate_progressBar = new System.Windows.Forms.ProgressBar();
            this.eta_label = new System.Windows.Forms.Label();
            this.start_button = new System.Windows.Forms.Button();
            this.selectFile_button = new System.Windows.Forms.Button();
            this.key_textBox = new System.Windows.Forms.TextBox();
            this.mode_comboBox = new System.Windows.Forms.ComboBox();
            this.log_textBox = new System.Windows.Forms.RichTextBox();
            this.fileName_label = new System.Windows.Forms.Label();
            this.keyStatus_label = new System.Windows.Forms.Label();
            this.deleteOrig_checkBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chunkUpdate_progressBar
            // 
            this.chunkUpdate_progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chunkUpdate_progressBar.Location = new System.Drawing.Point(12, 445);
            this.chunkUpdate_progressBar.Name = "chunkUpdate_progressBar";
            this.chunkUpdate_progressBar.Size = new System.Drawing.Size(369, 23);
            this.chunkUpdate_progressBar.TabIndex = 0;
            // 
            // eta_label
            // 
            this.eta_label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.eta_label.AutoSize = true;
            this.eta_label.Location = new System.Drawing.Point(12, 429);
            this.eta_label.Name = "eta_label";
            this.eta_label.Size = new System.Drawing.Size(34, 13);
            this.eta_label.TabIndex = 1;
            this.eta_label.Text = "ETA: ";
            // 
            // start_button
            // 
            this.start_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.start_button.Enabled = false;
            this.start_button.Location = new System.Drawing.Point(15, 403);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(75, 23);
            this.start_button.TabIndex = 2;
            this.start_button.Text = "Start";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.StartProcess_button_Click);
            // 
            // selectFile_button
            // 
            this.selectFile_button.Location = new System.Drawing.Point(12, 12);
            this.selectFile_button.Name = "selectFile_button";
            this.selectFile_button.Size = new System.Drawing.Size(75, 23);
            this.selectFile_button.TabIndex = 3;
            this.selectFile_button.Text = "Select File";
            this.selectFile_button.UseVisualStyleBackColor = true;
            this.selectFile_button.Click += new System.EventHandler(this.SelectFile_button_Click);
            // 
            // key_textBox
            // 
            this.key_textBox.Enabled = false;
            this.key_textBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.key_textBox.Location = new System.Drawing.Point(12, 54);
            this.key_textBox.Name = "key_textBox";
            this.key_textBox.Size = new System.Drawing.Size(160, 20);
            this.key_textBox.TabIndex = 4;
            this.key_textBox.Text = "Key";
            this.key_textBox.TextChanged += new System.EventHandler(this.Key_textBox_TextChanged);
            this.key_textBox.Enter += new System.EventHandler(this.Key_textBox_Enter);
            this.key_textBox.Leave += new System.EventHandler(this.Key_textBox_Leave);
            // 
            // mode_comboBox
            // 
            this.mode_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mode_comboBox.FormattingEnabled = true;
            this.mode_comboBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.mode_comboBox.Items.AddRange(new object[] {
            "Encryption",
            "Decryption"});
            this.mode_comboBox.Location = new System.Drawing.Point(178, 54);
            this.mode_comboBox.Name = "mode_comboBox";
            this.mode_comboBox.Size = new System.Drawing.Size(121, 21);
            this.mode_comboBox.TabIndex = 5;
            this.mode_comboBox.SelectedIndexChanged += new System.EventHandler(this.Mode_comboBox_SelectedIndexChanged);
            // 
            // log_textBox
            // 
            this.log_textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.log_textBox.Location = new System.Drawing.Point(12, 97);
            this.log_textBox.Name = "log_textBox";
            this.log_textBox.ReadOnly = true;
            this.log_textBox.Size = new System.Drawing.Size(364, 282);
            this.log_textBox.TabIndex = 6;
            this.log_textBox.Text = "";
            // 
            // fileName_label
            // 
            this.fileName_label.AutoSize = true;
            this.fileName_label.Location = new System.Drawing.Point(12, 38);
            this.fileName_label.Name = "fileName_label";
            this.fileName_label.Size = new System.Drawing.Size(80, 13);
            this.fileName_label.TabIndex = 7;
            this.fileName_label.Text = "No file selected";
            // 
            // keyStatus_label
            // 
            this.keyStatus_label.AutoSize = true;
            this.keyStatus_label.ForeColor = System.Drawing.Color.Red;
            this.keyStatus_label.Location = new System.Drawing.Point(12, 77);
            this.keyStatus_label.Name = "keyStatus_label";
            this.keyStatus_label.Size = new System.Drawing.Size(76, 13);
            this.keyStatus_label.TabIndex = 9;
            this.keyStatus_label.Text = "Key is Missing!";
            // 
            // deleteOrig_checkBox
            // 
            this.deleteOrig_checkBox.AutoSize = true;
            this.deleteOrig_checkBox.Location = new System.Drawing.Point(96, 407);
            this.deleteOrig_checkBox.Name = "deleteOrig_checkBox";
            this.deleteOrig_checkBox.Size = new System.Drawing.Size(95, 17);
            this.deleteOrig_checkBox.TabIndex = 10;
            this.deleteOrig_checkBox.Text = "Delete Original";
            this.deleteOrig_checkBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 480);
            this.Controls.Add(this.deleteOrig_checkBox);
            this.Controls.Add(this.keyStatus_label);
            this.Controls.Add(this.fileName_label);
            this.Controls.Add(this.log_textBox);
            this.Controls.Add(this.mode_comboBox);
            this.Controls.Add(this.key_textBox);
            this.Controls.Add(this.selectFile_button);
            this.Controls.Add(this.start_button);
            this.Controls.Add(this.eta_label);
            this.Controls.Add(this.chunkUpdate_progressBar);
            this.Name = "MainForm";
            this.Text = "DataEncrypter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar chunkUpdate_progressBar;
        private System.Windows.Forms.Label eta_label;
        private System.Windows.Forms.Button start_button;
        private System.Windows.Forms.Button selectFile_button;
        private System.Windows.Forms.TextBox key_textBox;
        private System.Windows.Forms.ComboBox mode_comboBox;
        private System.Windows.Forms.RichTextBox log_textBox;
        private System.Windows.Forms.Label fileName_label;
        private System.Windows.Forms.Label keyStatus_label;
        private System.Windows.Forms.CheckBox deleteOrig_checkBox;
    }
}

