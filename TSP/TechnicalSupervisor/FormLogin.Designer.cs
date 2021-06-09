namespace TechnicalSupervisor
    {
    partial class FormLogin
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
            this.label1 = new System.Windows.Forms.Label();
            this.button_Login = new System.Windows.Forms.Button();
            this.textBox_UserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.button_CancelLogin = new System.Windows.Forms.Button();
            this.label_Error = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button_Exit = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label_LoginError = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(124, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "User Name";
            // 
            // button_Login
            // 
            this.button_Login.Enabled = false;
            this.button_Login.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Login.Location = new System.Drawing.Point(255, 312);
            this.button_Login.Name = "button_Login";
            this.button_Login.Size = new System.Drawing.Size(115, 28);
            this.button_Login.TabIndex = 1;
            this.button_Login.Text = "Log In";
            this.button_Login.UseVisualStyleBackColor = true;
            this.button_Login.Click += new System.EventHandler(this.button_Login_Click);
            // 
            // textBox_UserName
            // 
            this.textBox_UserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_UserName.Location = new System.Drawing.Point(230, 185);
            this.textBox_UserName.Name = "textBox_UserName";
            this.textBox_UserName.Size = new System.Drawing.Size(215, 26);
            this.textBox_UserName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(124, 232);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // textBox_Password
            // 
            this.textBox_Password.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Password.Location = new System.Drawing.Point(230, 230);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.PasswordChar = '•';
            this.textBox_Password.Size = new System.Drawing.Size(215, 26);
            this.textBox_Password.TabIndex = 4;
            // 
            // button_CancelLogin
            // 
            this.button_CancelLogin.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_CancelLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_CancelLogin.Location = new System.Drawing.Point(121, 313);
            this.button_CancelLogin.Name = "button_CancelLogin";
            this.button_CancelLogin.Size = new System.Drawing.Size(115, 27);
            this.button_CancelLogin.TabIndex = 5;
            this.button_CancelLogin.Text = "Cancel";
            this.button_CancelLogin.UseVisualStyleBackColor = true;
            this.button_CancelLogin.Click += new System.EventHandler(this.button_CancelLogin_Click);
            // 
            // label_Error
            // 
            this.label_Error.AutoSize = true;
            this.label_Error.Location = new System.Drawing.Point(95, 284);
            this.label_Error.Name = "label_Error";
            this.label_Error.Size = new System.Drawing.Size(0, 13);
            this.label_Error.TabIndex = 6;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::TechnicalSupervisor.Properties.Resources.ThruputLogo;
            this.pictureBox1.Location = new System.Drawing.Point(96, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(416, 83);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // button_Exit
            // 
            this.button_Exit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Exit.Location = new System.Drawing.Point(388, 312);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(115, 28);
            this.button_Exit.TabIndex = 8;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = true;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label3.Location = new System.Drawing.Point(71, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(467, 37);
            this.label3.TabIndex = 9;
            this.label3.Text = "Technical Supervisor Position";
            // 
            // label_LoginError
            // 
            this.label_LoginError.AutoSize = true;
            this.label_LoginError.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_LoginError.ForeColor = System.Drawing.Color.Red;
            this.label_LoginError.Location = new System.Drawing.Point(130, 273);
            this.label_LoginError.Name = "label_LoginError";
            this.label_LoginError.Size = new System.Drawing.Size(0, 20);
            this.label_LoginError.TabIndex = 10;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(133)))), ((int)(((byte)(129)))));
            this.CancelButton = this.button_CancelLogin;
            this.ClientSize = new System.Drawing.Size(609, 390);
            this.ControlBox = false;
            this.Controls.Add(this.label_LoginError);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Exit);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label_Error);
            this.Controls.Add(this.button_CancelLogin);
            this.Controls.Add(this.textBox_Password);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_UserName);
            this.Controls.Add(this.button_Login);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormLogin";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Login";
            this.Load += new System.EventHandler(this.FormLogin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Login;
        private System.Windows.Forms.TextBox textBox_UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_Password;
        private System.Windows.Forms.Button button_CancelLogin;
        private System.Windows.Forms.Label label_Error;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_LoginError;
        private System.Windows.Forms.Timer timer1;
        }
    }