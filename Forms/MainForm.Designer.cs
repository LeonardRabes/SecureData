namespace DataEncrypter
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
            this.SuspendLayout();
            // 
            // chunkUpdate_progressBar
            // 
            this.chunkUpdate_progressBar.Location = new System.Drawing.Point(71, 36);
            this.chunkUpdate_progressBar.Name = "chunkUpdate_progressBar";
            this.chunkUpdate_progressBar.Size = new System.Drawing.Size(633, 23);
            this.chunkUpdate_progressBar.TabIndex = 0;
            // 
            // eta_label
            // 
            this.eta_label.AutoSize = true;
            this.eta_label.Location = new System.Drawing.Point(68, 62);
            this.eta_label.Name = "eta_label";
            this.eta_label.Size = new System.Drawing.Size(34, 13);
            this.eta_label.TabIndex = 1;
            this.eta_label.Text = "ETA: ";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.eta_label);
            this.Controls.Add(this.chunkUpdate_progressBar);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar chunkUpdate_progressBar;
        private System.Windows.Forms.Label eta_label;
    }
}

