namespace BSA_Browser
{
    partial class ProgressForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressForm));
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.pbRatio = new System.Windows.Forms.ProgressBar();
            this.lProgress = new System.Windows.Forms.Label();
            this.lRatio = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pbProgress
            // 
            this.pbProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbProgress.ForeColor = System.Drawing.Color.Lime;
            this.pbProgress.Location = new System.Drawing.Point(12, 12);
            this.pbProgress.Maximum = 10000;
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(255, 15);
            this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbProgress.TabIndex = 0;
            // 
            // pbRatio
            // 
            this.pbRatio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbRatio.ForeColor = System.Drawing.Color.Lime;
            this.pbRatio.Location = new System.Drawing.Point(12, 33);
            this.pbRatio.Maximum = 10000;
            this.pbRatio.Name = "pbRatio";
            this.pbRatio.Size = new System.Drawing.Size(255, 15);
            this.pbRatio.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbRatio.TabIndex = 1;
            // 
            // lProgress
            // 
            this.lProgress.AutoSize = true;
            this.lProgress.Location = new System.Drawing.Point(273, 10);
            this.lProgress.Name = "lProgress";
            this.lProgress.Size = new System.Drawing.Size(0, 13);
            this.lProgress.TabIndex = 2;
            // 
            // lRatio
            // 
            this.lRatio.AutoSize = true;
            this.lRatio.Location = new System.Drawing.Point(273, 31);
            this.lRatio.Name = "lRatio";
            this.lRatio.Size = new System.Drawing.Size(0, 13);
            this.lRatio.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(107, 54);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ProgressForm
            // 
            this.ClientSize = new System.Drawing.Size(309, 88);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lRatio);
            this.Controls.Add(this.lProgress);
            this.Controls.Add(this.pbRatio);
            this.Controls.Add(this.pbProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.ProgressBar pbRatio;
        private System.Windows.Forms.Label lProgress;
        private System.Windows.Forms.Label lRatio;
        private System.Windows.Forms.Button btnCancel;
    }
}