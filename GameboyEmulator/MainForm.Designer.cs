namespace GameboyEmulator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.loadBtn = new System.Windows.Forms.ToolStripButton();
            this.widescreenBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.enlargeBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.palleteBtn = new System.Windows.Forms.ToolStripButton();
            this.pictureBox1 = new GameboyEmulator.PictureBoxWithInterpolationMode();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadBtn,
            this.widescreenBtn,
            this.toolStripSeparator2,
            this.enlargeBtn,
            this.toolStripSeparator1,
            this.palleteBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(482, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // loadBtn
            // 
            this.loadBtn.Image = global::GameboyEmulator.Properties.Resources.chip_icon__1_;
            this.loadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(53, 22);
            this.loadBtn.Text = "Load";
            this.loadBtn.Click += new System.EventHandler(this.LoadBtn_Click);
            // 
            // widescreenBtn
            // 
            this.widescreenBtn.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.widescreenBtn.Image = global::GameboyEmulator.Properties.Resources.widescreen_icon;
            this.widescreenBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.widescreenBtn.Name = "widescreenBtn";
            this.widescreenBtn.Size = new System.Drawing.Size(88, 22);
            this.widescreenBtn.Text = "Widescreen";
            this.widescreenBtn.Click += new System.EventHandler(this.widescreenBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // enlargeBtn
            // 
            this.enlargeBtn.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.enlargeBtn.Image = global::GameboyEmulator.Properties.Resources.enlarge;
            this.enlargeBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.enlargeBtn.Name = "enlargeBtn";
            this.enlargeBtn.Size = new System.Drawing.Size(66, 22);
            this.enlargeBtn.Text = "Enlarge";
            this.enlargeBtn.Click += new System.EventHandler(this.enlargeBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // palleteBtn
            // 
            this.palleteBtn.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.palleteBtn.Image = global::GameboyEmulator.Properties.Resources.color_palette;
            this.palleteBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.palleteBtn.Name = "palleteBtn";
            this.palleteBtn.Size = new System.Drawing.Size(62, 22);
            this.palleteBtn.Text = "Pallete";
            this.palleteBtn.Click += new System.EventHandler(this.palleteBtn_Click_1);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.pictureBox1.Location = new System.Drawing.Point(0, 25);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(482, 438);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(482, 463);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gameboy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ToolStrip toolStrip1;
        private ToolStripButton loadBtn;
        public PictureBoxWithInterpolationMode pictureBox1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton palleteBtn;
        private ToolStripButton enlargeBtn;
        private ToolStripButton widescreenBtn;
        private ToolStripSeparator toolStripSeparator2;
    }
}