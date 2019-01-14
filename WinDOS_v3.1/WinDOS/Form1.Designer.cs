namespace WinDOS
{
    partial class DiscOperatingSystem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiscOperatingSystem));
            this.inputField = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // inputField
            // 
            this.inputField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.inputField.Location = new System.Drawing.Point(0, 0);
            this.inputField.MaxLength = 1000000;
            this.inputField.Multiline = true;
            this.inputField.Name = "inputField";
            this.inputField.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.inputField.Size = new System.Drawing.Size(820, 453);
            this.inputField.TabIndex = 0;
            // 
            // DiscOperatingSystem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.inputField);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DiscOperatingSystem";
            this.Text = "WinDOS";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox inputField;
    }
}

