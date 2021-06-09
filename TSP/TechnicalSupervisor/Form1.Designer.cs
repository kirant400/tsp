namespace TechnicalSupervisor
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox_Debug = new System.Windows.Forms.GroupBox();
            this.button_ViewSnmpEvents = new System.Windows.Forms.Button();
            this.label_NameAndRole = new System.Windows.Forms.Label();
            this.button_LoginDialog = new System.Windows.Forms.Button();
            this.label_CurrentUser = new System.Windows.Forms.Label();
            this.button_Acknowledge = new System.Windows.Forms.Button();
            this.label_DebugMessage = new System.Windows.Forms.Label();
            this.label_DebugMessage2 = new System.Windows.Forms.Label();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toggleFullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTSPLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.alarmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.allErrorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allWarningsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_ChangeUser = new System.Windows.Forms.ToolStripMenuItem();
            this.quitApplicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox_Debug.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            this.contextMenuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStrip1_ItemClicked);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.groupBox_Debug);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.label_DebugMessage2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 561);
            this.panel1.TabIndex = 2;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel1_Paint);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(138, 71);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(200, 200);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // groupBox_Debug
            // 
            this.groupBox_Debug.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Debug.BackColor = System.Drawing.Color.Transparent;
            this.groupBox_Debug.Controls.Add(this.button_ViewSnmpEvents);
            this.groupBox_Debug.Controls.Add(this.label_NameAndRole);
            this.groupBox_Debug.Controls.Add(this.button_LoginDialog);
            this.groupBox_Debug.Controls.Add(this.label_CurrentUser);
            this.groupBox_Debug.Controls.Add(this.button_Acknowledge);
            this.groupBox_Debug.Controls.Add(this.label_DebugMessage);
            this.groupBox_Debug.ForeColor = System.Drawing.Color.Yellow;
            this.groupBox_Debug.Location = new System.Drawing.Point(3, 3);
            this.groupBox_Debug.Name = "groupBox_Debug";
            this.groupBox_Debug.Size = new System.Drawing.Size(778, 109);
            this.groupBox_Debug.TabIndex = 9;
            this.groupBox_Debug.TabStop = false;
            this.groupBox_Debug.Text = "SNMP Debug";
            this.groupBox_Debug.Visible = false;
            // 
            // button_ViewSnmpEvents
            // 
            this.button_ViewSnmpEvents.ForeColor = System.Drawing.Color.Yellow;
            this.button_ViewSnmpEvents.Location = new System.Drawing.Point(6, 19);
            this.button_ViewSnmpEvents.Name = "button_ViewSnmpEvents";
            this.button_ViewSnmpEvents.Size = new System.Drawing.Size(106, 28);
            this.button_ViewSnmpEvents.TabIndex = 4;
            this.button_ViewSnmpEvents.Text = "View Events";
            this.button_ViewSnmpEvents.UseVisualStyleBackColor = true;
            this.button_ViewSnmpEvents.Visible = false;
            this.button_ViewSnmpEvents.Click += new System.EventHandler(this.button1_Click);
            // 
            // label_NameAndRole
            // 
            this.label_NameAndRole.AutoSize = true;
            this.label_NameAndRole.ForeColor = System.Drawing.Color.Yellow;
            this.label_NameAndRole.Location = new System.Drawing.Point(6, 77);
            this.label_NameAndRole.Name = "label_NameAndRole";
            this.label_NameAndRole.Size = new System.Drawing.Size(53, 13);
            this.label_NameAndRole.TabIndex = 8;
            this.label_NameAndRole.Text = "Unknown";
            this.label_NameAndRole.Visible = false;
            // 
            // button_LoginDialog
            // 
            this.button_LoginDialog.ForeColor = System.Drawing.Color.Yellow;
            this.button_LoginDialog.Location = new System.Drawing.Point(118, 19);
            this.button_LoginDialog.Name = "button_LoginDialog";
            this.button_LoginDialog.Size = new System.Drawing.Size(106, 28);
            this.button_LoginDialog.TabIndex = 5;
            this.button_LoginDialog.Text = "Login";
            this.button_LoginDialog.UseVisualStyleBackColor = true;
            this.button_LoginDialog.Visible = false;
            this.button_LoginDialog.Click += new System.EventHandler(this.button_LoginDialog_Click);
            // 
            // label_CurrentUser
            // 
            this.label_CurrentUser.AutoSize = true;
            this.label_CurrentUser.ForeColor = System.Drawing.Color.Yellow;
            this.label_CurrentUser.Location = new System.Drawing.Point(6, 50);
            this.label_CurrentUser.Name = "label_CurrentUser";
            this.label_CurrentUser.Size = new System.Drawing.Size(53, 13);
            this.label_CurrentUser.TabIndex = 7;
            this.label_CurrentUser.Text = "Unknown";
            this.label_CurrentUser.Visible = false;
            // 
            // button_Acknowledge
            // 
            this.button_Acknowledge.ForeColor = System.Drawing.Color.Yellow;
            this.button_Acknowledge.Location = new System.Drawing.Point(230, 19);
            this.button_Acknowledge.Name = "button_Acknowledge";
            this.button_Acknowledge.Size = new System.Drawing.Size(128, 28);
            this.button_Acknowledge.TabIndex = 6;
            this.button_Acknowledge.Text = "Acknowledge";
            this.button_Acknowledge.UseVisualStyleBackColor = true;
            this.button_Acknowledge.Visible = false;
            this.button_Acknowledge.Click += new System.EventHandler(this.button_Acknowledge_Click);
            // 
            // label_DebugMessage
            // 
            this.label_DebugMessage.AutoSize = true;
            this.label_DebugMessage.ForeColor = System.Drawing.Color.Yellow;
            this.label_DebugMessage.Location = new System.Drawing.Point(364, 19);
            this.label_DebugMessage.Name = "label_DebugMessage";
            this.label_DebugMessage.Size = new System.Drawing.Size(35, 13);
            this.label_DebugMessage.TabIndex = 2;
            this.label_DebugMessage.Text = "label1";
            this.label_DebugMessage.Visible = false;
            // 
            // label_DebugMessage2
            // 
            this.label_DebugMessage2.AutoSize = true;
            this.label_DebugMessage2.ForeColor = System.Drawing.Color.Yellow;
            this.label_DebugMessage2.Location = new System.Drawing.Point(124, 33);
            this.label_DebugMessage2.Name = "label_DebugMessage2";
            this.label_DebugMessage2.Size = new System.Drawing.Size(19, 13);
            this.label_DebugMessage2.TabIndex = 3;
            this.label_DebugMessage2.Text = "....";
            this.label_DebugMessage2.Visible = false;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleFullScreenToolStripMenuItem,
            this.toolStripSeparator2,
            this.optionsToolStripMenuItem,
            this.viewTSPLogsToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripMenuItem1,
            this.toolStripSeparator3,
            this.toolStripMenuItem_ChangeUser,
            this.quitApplicationToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(195, 154);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // toggleFullScreenToolStripMenuItem
            // 
            this.toggleFullScreenToolStripMenuItem.Name = "toggleFullScreenToolStripMenuItem";
            this.toggleFullScreenToolStripMenuItem.ShortcutKeyDisplayString = "F10";
            this.toggleFullScreenToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.toggleFullScreenToolStripMenuItem.Text = "Toggle Full Screen";
            this.toggleFullScreenToolStripMenuItem.Click += new System.EventHandler(this.toggleFullScreenToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(191, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.ShortcutKeyDisplayString = "F9";
            this.optionsToolStripMenuItem.ShowShortcutKeys = false;
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.optionsToolStripMenuItem.Text = "&Manage TSP";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // viewTSPLogsToolStripMenuItem
            // 
            this.viewTSPLogsToolStripMenuItem.Name = "viewTSPLogsToolStripMenuItem";
            this.viewTSPLogsToolStripMenuItem.ShortcutKeyDisplayString = "F6";
            this.viewTSPLogsToolStripMenuItem.ShowShortcutKeys = false;
            this.viewTSPLogsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewTSPLogsToolStripMenuItem.Text = "View Text Logs";
            this.viewTSPLogsToolStripMenuItem.Click += new System.EventHandler(this.viewTSPLogsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(191, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alarmToolStripMenuItem,
            this.toolStripSeparator4,
            this.allErrorsToolStripMenuItem,
            this.allWarningsToolStripMenuItem,
            this.allInfoToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShowShortcutKeys = false;
            this.toolStripMenuItem1.Size = new System.Drawing.Size(194, 22);
            this.toolStripMenuItem1.Text = "Acknowledge";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click_1);
            // 
            // alarmToolStripMenuItem
            // 
            this.alarmToolStripMenuItem.Name = "alarmToolStripMenuItem";
            this.alarmToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.alarmToolStripMenuItem.Text = "Alarm";
            this.alarmToolStripMenuItem.ToolTipText = "If clicked, the local alarm will be acknowledged. However if a new event is recei" +
    "ved, then the alarm will resume.";
            this.alarmToolStripMenuItem.Click += new System.EventHandler(this.alarmToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(138, 6);
            // 
            // allErrorsToolStripMenuItem
            // 
            this.allErrorsToolStripMenuItem.Name = "allErrorsToolStripMenuItem";
            this.allErrorsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.allErrorsToolStripMenuItem.Text = "All Errors";
            this.allErrorsToolStripMenuItem.Click += new System.EventHandler(this.allErrorsToolStripMenuItem_Click);
            // 
            // allWarningsToolStripMenuItem
            // 
            this.allWarningsToolStripMenuItem.Name = "allWarningsToolStripMenuItem";
            this.allWarningsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.allWarningsToolStripMenuItem.Text = "All Warnings";
            this.allWarningsToolStripMenuItem.Click += new System.EventHandler(this.allWarningsToolStripMenuItem_Click);
            // 
            // allInfoToolStripMenuItem
            // 
            this.allInfoToolStripMenuItem.Name = "allInfoToolStripMenuItem";
            this.allInfoToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.allInfoToolStripMenuItem.Text = "All Info";
            this.allInfoToolStripMenuItem.Click += new System.EventHandler(this.allInfoToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(191, 6);
            // 
            // toolStripMenuItem_ChangeUser
            // 
            this.toolStripMenuItem_ChangeUser.Name = "toolStripMenuItem_ChangeUser";
            this.toolStripMenuItem_ChangeUser.Size = new System.Drawing.Size(194, 22);
            this.toolStripMenuItem_ChangeUser.Text = "Change User";
            this.toolStripMenuItem_ChangeUser.Visible = false;
            this.toolStripMenuItem_ChangeUser.Click += new System.EventHandler(this.toolStripMenuItem_ChangeUser_Click);
            // 
            // quitApplicationToolStripMenuItem
            // 
            this.quitApplicationToolStripMenuItem.Name = "quitApplicationToolStripMenuItem";
            this.quitApplicationToolStripMenuItem.ShortcutKeyDisplayString = "Alt+F4";
            this.quitApplicationToolStripMenuItem.ShowShortcutKeys = false;
            this.quitApplicationToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.quitApplicationToolStripMenuItem.Text = "Exit TSP";
            this.quitApplicationToolStripMenuItem.Click += new System.EventHandler(this.quitApplicationToolStripMenuItem_Click);
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 1000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // timer3
            // 
            this.timer3.Interval = 3000;
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker2_DoWork);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "Form1";
            this.Text = "Technical Supervisor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox_Debug.ResumeLayout(false);
            this.groupBox_Debug.PerformLayout();
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleFullScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewTSPLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem quitApplicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem allErrorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allWarningsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.ToolStripMenuItem alarmToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.Timer timer3;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.Label label_DebugMessage;
        private System.Windows.Forms.Label label_DebugMessage2;
        private System.Windows.Forms.Button button_ViewSnmpEvents;
        private System.Windows.Forms.Button button_LoginDialog;
        private System.Windows.Forms.Button button_Acknowledge;
        private System.Windows.Forms.Label label_CurrentUser;
        private System.Windows.Forms.Label label_NameAndRole;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ChangeUser;
        private System.Windows.Forms.GroupBox groupBox_Debug;
        }
}

