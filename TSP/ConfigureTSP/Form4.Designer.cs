namespace ConfigureTSP
{
    partial class Form4
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
            this.components = new System.ComponentModel.Container();
            this.panel4 = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectTSPImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBackgroundColourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetBorderColourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCustomButtonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSystemButtonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel4.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            this.panel4.AutoScroll = true;
            this.panel4.BackgroundImage = global::ConfigureTSP.Properties.Resources.tsp_backdrop;
            this.panel4.ContextMenuStrip = this.contextMenuStrip1;
            this.panel4.Controls.Add(this.pictureBox1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1089, 724);
            this.panel4.TabIndex = 6;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectTSPImageToolStripMenuItem,
            this.changeBackgroundColourToolStripMenuItem,
            this.resetBorderColourToolStripMenuItem,
            this.addCustomButtonToolStripMenuItem,
            this.addSystemButtonToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(225, 114);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // selectTSPImageToolStripMenuItem
            // 
            this.selectTSPImageToolStripMenuItem.Name = "selectTSPImageToolStripMenuItem";
            this.selectTSPImageToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.selectTSPImageToolStripMenuItem.Text = "Change Background Image..";
            this.selectTSPImageToolStripMenuItem.Visible = false;
            this.selectTSPImageToolStripMenuItem.Click += new System.EventHandler(this.SelectTSPImageToolStripMenuItem_Click);
            // 
            // changeBackgroundColourToolStripMenuItem
            // 
            this.changeBackgroundColourToolStripMenuItem.Name = "changeBackgroundColourToolStripMenuItem";
            this.changeBackgroundColourToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.changeBackgroundColourToolStripMenuItem.Text = "Change Border Colour..";
            this.changeBackgroundColourToolStripMenuItem.Visible = false;
            this.changeBackgroundColourToolStripMenuItem.Click += new System.EventHandler(this.ChangeBackgroundColourToolStripMenuItem_Click);
            // 
            // resetBorderColourToolStripMenuItem
            // 
            this.resetBorderColourToolStripMenuItem.Name = "resetBorderColourToolStripMenuItem";
            this.resetBorderColourToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resetBorderColourToolStripMenuItem.Text = "Reset Border Colour";
            this.resetBorderColourToolStripMenuItem.Visible = false;
            this.resetBorderColourToolStripMenuItem.Click += new System.EventHandler(this.ResetBorderColourToolStripMenuItem_Click);
            // 
            // addCustomButtonToolStripMenuItem
            // 
            this.addCustomButtonToolStripMenuItem.Name = "addCustomButtonToolStripMenuItem";
            this.addCustomButtonToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.addCustomButtonToolStripMenuItem.Text = "Add Custom Button";
            this.addCustomButtonToolStripMenuItem.Click += new System.EventHandler(this.addCustomButtonToolStripMenuItem_Click);
            // 
            // addSystemButtonToolStripMenuItem
            // 
            this.addSystemButtonToolStripMenuItem.Name = "addSystemButtonToolStripMenuItem";
            this.addSystemButtonToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.addSystemButtonToolStripMenuItem.Text = "Add System Button";
            this.addSystemButtonToolStripMenuItem.Click += new System.EventHandler(this.addSystemButtonToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.ErrorImage = global::ConfigureTSP.Properties.Resources.download;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDoubleClick);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.toolStripSeparator6,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem1});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(151, 76);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.renameToolStripMenuItem.Text = "Properties..";
            this.renameToolStripMenuItem.Visible = false;
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(147, 6);
            this.toolStripSeparator6.Visible = false;
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.deleteToolStripMenuItem.Text = "Delete Button";
            this.deleteToolStripMenuItem.ToolTipText = "Click to remove the button entirely";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click_1);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem2,
            this.toolStripSeparator2,
            this.deleteToolStripMenuItem1});
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(150, 22);
            this.toolStripMenuItem1.Text = "Image options";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(211, 22);
            this.toolStripMenuItem3.Text = "Resize button to fit image";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(211, 22);
            this.toolStripMenuItem2.Text = "Resize image to fit button";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(208, 6);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(211, 22);
            this.deleteToolStripMenuItem1.Text = "Delete image from button";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            this.colorDialog1.FullOpen = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "xml";
            this.openFileDialog1.Filter = "XML files|*.xml|All files|*.*";
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 724);
            this.Controls.Add(this.panel4);
            this.Name = "Form4";
            this.Text = "Form4";
            this.panel4.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectTSPImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBackgroundColourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetBorderColourToolStripMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        public System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem addCustomButtonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSystemButtonToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
    }
}