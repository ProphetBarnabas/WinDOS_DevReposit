namespace WinDOS
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.IOField = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // IOField
            // 
            this.IOField.BackColor = System.Drawing.Color.Black;
            this.IOField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.IOField.ForeColor = System.Drawing.Color.Silver;
            this.IOField.Location = new System.Drawing.Point(0, 0);
            this.IOField.Multiline = true;
            this.IOField.Name = "IOField";
            this.IOField.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.IOField.Size = new System.Drawing.Size(818, 450);
            this.IOField.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.IOField);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "WinDOS";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox IOField;
    }
}

