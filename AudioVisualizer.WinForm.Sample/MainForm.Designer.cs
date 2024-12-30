namespace AudioVisualizer.WinForm.Sample
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
            audioVisualizer1 = new AudioVisualizer();
            SuspendLayout();
            // 
            // audioVisualizer1
            // 
            audioVisualizer1.Dock = System.Windows.Forms.DockStyle.Fill;
            audioVisualizer1.Location = new System.Drawing.Point(0, 0);
            audioVisualizer1.Name = "audioVisualizer1";
            audioVisualizer1.Size = new System.Drawing.Size(800, 450);
            audioVisualizer1.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(audioVisualizer1);
            Name = "MainForm";
            Text = "MainForm";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private AudioVisualizer audioVisualizer1;
    }
}