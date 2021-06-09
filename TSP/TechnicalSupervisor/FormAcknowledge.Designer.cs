namespace TechnicalSupervisor
    {
    partial class FormAcknowledge
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
            this.label_Error = new System.Windows.Forms.Label();
            this.button_CancelLogin = new System.Windows.Forms.Button();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_UserName = new System.Windows.Forms.TextBox();
            this.button_Acknowledge = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.textBox_Results = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_Error
            // 
            this.label_Error.AutoSize = true;
            this.label_Error.Location = new System.Drawing.Point(283, 280);
            this.label_Error.Name = "label_Error";
            this.label_Error.Size = new System.Drawing.Size(0, 13);
            this.label_Error.TabIndex = 13;
            // 
            // button_CancelLogin
            // 
            this.button_CancelLogin.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_CancelLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_CancelLogin.Location = new System.Drawing.Point(125, 147);
            this.button_CancelLogin.Name = "button_CancelLogin";
            this.button_CancelLogin.Size = new System.Drawing.Size(75, 27);
            this.button_CancelLogin.TabIndex = 12;
            this.button_CancelLogin.Text = "Close";
            this.button_CancelLogin.UseVisualStyleBackColor = true;
            this.button_CancelLogin.Click += new System.EventHandler(this.button_CancelLogin_Click);
            // 
            // textBox_Password
            // 
            this.textBox_Password.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Password.Location = new System.Drawing.Point(104, 47);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.PasswordChar = '•';
            this.textBox_Password.Size = new System.Drawing.Size(203, 26);
            this.textBox_Password.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(9, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 20);
            this.label2.TabIndex = 10;
            this.label2.Text = "Password";
            // 
            // textBox_UserName
            // 
            this.textBox_UserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_UserName.Location = new System.Drawing.Point(104, 17);
            this.textBox_UserName.Name = "textBox_UserName";
            this.textBox_UserName.Size = new System.Drawing.Size(203, 26);
            this.textBox_UserName.TabIndex = 9;
            // 
            // button_Acknowledge
            // 
            this.button_Acknowledge.BackColor = System.Drawing.SystemColors.Control;
            this.button_Acknowledge.Enabled = false;
            this.button_Acknowledge.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Acknowledge.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button_Acknowledge.Location = new System.Drawing.Point(13, 79);
            this.button_Acknowledge.Name = "button_Acknowledge";
            this.button_Acknowledge.Size = new System.Drawing.Size(295, 33);
            this.button_Acknowledge.TabIndex = 8;
            this.button_Acknowledge.Text = "Acknowledge All Events";
            this.toolTip1.SetToolTip(this.button_Acknowledge, "Enter User Name and Password");
            this.button_Acknowledge.UseVisualStyleBackColor = false;
            this.button_Acknowledge.Click += new System.EventHandler(this.button_Acknowledge_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(9, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "User Name";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.ForeColor = System.Drawing.Color.Green;
            this.progressBar1.Location = new System.Drawing.Point(13, 118);
            this.progressBar1.Maximum = 10;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(294, 23);
            this.progressBar1.TabIndex = 15;
            this.progressBar1.Visible = false;
            // 
            // textBox_Results
            // 
            this.textBox_Results.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(133)))), ((int)(((byte)(129)))));
            this.textBox_Results.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_Results.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Results.Location = new System.Drawing.Point(14, 118);
            this.textBox_Results.Name = "textBox_Results";
            this.textBox_Results.Size = new System.Drawing.Size(294, 19);
            this.textBox_Results.TabIndex = 16;
            this.textBox_Results.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox_Results.Visible = false;
            // 
            // FormAcknowledge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(133)))), ((int)(((byte)(129)))));
            this.ClientSize = new System.Drawing.Size(321, 179);
            this.ControlBox = false;
            this.Controls.Add(this.textBox_Results);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label_Error);
            this.Controls.Add(this.button_CancelLogin);
            this.Controls.Add(this.textBox_Password);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_UserName);
            this.Controls.Add(this.button_Acknowledge);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormAcknowledge";
            this.Text = "Acknowledge All Events";
            this.Load += new System.EventHandler(this.FormAcknowledge_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

            }

        #endregion
        private System.Windows.Forms.Label label_Error;
        private System.Windows.Forms.Button button_CancelLogin;
        private System.Windows.Forms.TextBox textBox_Password;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_UserName;
        private System.Windows.Forms.Button button_Acknowledge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textBox_Results;
        }
    }