/**************************************************************************************************
 
	Form to allow the user to log into the TSP when using SNMP

	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		FormLogin.cs
	Project:		TSP
	Developer:		CRR 
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			
**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;


namespace TechnicalSupervisor
    {
    public partial class FormLogin : Form
        {
        public String UserName = "";
        public String Password = "";
        public LogClients AttachedLogClients;       //#50 Log clients passed into login form when invoked.
        public bool ExitApplication=false;
        public bool LoggedIn = false;
        public bool CarryOn = false;                // If true carry on with further processing
        public LogInterface m_log;           // Log to disk/screen/etc interface

        public FormLogin()
            {
            InitializeComponent();
            }

        private void button_CancelLogin_Click(object sender, EventArgs e)
            {
            this.UseWaitCursor = false;
            this.CarryOn = true;
            this.Close();
            }

        private void button_Login_Click(object sender, EventArgs e)
            {
            this.UseWaitCursor=true;
            button_CancelLogin.Enabled = false;
            UserName = textBox_UserName.Text;
            Password = textBox_Password.Text;
            if(Password.Length<8)
                {
                label_LoginError.Text = "The password must be 8 characters or more";
                label_LoginError.BackColor = Color.White;
                }
            else
                {
                Program.TSPConfiguration.V3Username = UserName;
                Program.TSPConfiguration.V3AuthPassword = Password;
                Program.TSPConfiguration.V3PrivPassword = Password;
                if(ValidateUser())
                    {
                    label_LoginError.Text = "";
                    this.m_log.LogInfo("Successful login by user " + UserName);
                    this.UseWaitCursor = false;
                    this.LoggedIn = true;
                    this.CarryOn = true;
                    this.Close();
                    }
                else
                    {
                    this.m_log.LogWarn("Failed login attempt by user " + UserName);
                    this.UseWaitCursor = false;
                    label_LoginError.Text = "The user name and password could not be verified";
                    }
                }
            }

        private void FormLogin_Load(object sender, EventArgs e)
            {
            }

        private bool ValidateUser()
            {
            bool ReturnValue = false;
            if(this.AttachedLogClients!=null)
                {
                foreach (LogClient lc in AttachedLogClients)
                    {
                    lc.SNMPManager.LoadMibsIfNotAllReadyLoaded();
                    lc.SNMPManager.SNMPV3_UserName = this.UserName;
                    lc.SNMPManager.SNMPV3_AuthPassword = this.Password;
                    lc.SNMPManager.SNMPV3_PrivacyPassword = this.Password;
                    lc.SNMPManager.UserRole = 0;
                    lc.SNMPManager.UseSNMPV3 = Program.TSPConfiguration.UseSNMPV3;
                    lc.SNMPManager.IPAddress = lc.Ep.Address.ToString();
                    lc.SNMPManager.Port = 161;  //ToDo make variable
                    Debug.WriteLine("");
                    Debug.WriteLine("");
                    Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TRYING " + lc.Hw_name);
                    if(lc.SNMPManager.VerifyCurrentUser()==true)
                        {
                        int i = 0;
                        while (lc.SNMPManager.UserChecked==false)
                            {
                            System.Threading.Thread.Sleep(100);
                            Application.DoEvents();
                            if (i++ > 20)
                                break;
                            }
                        if(lc.SNMPManager.UserChecked==true)
                            {
                            if (lc.SNMPManager.UserRole>0)
                                {
                                Program.TSPConfiguration.V3Username = this.UserName;
                                Program.TSPConfiguration.V3AuthPassword = this.Password;
                                Program.TSPConfiguration.V3PrivPassword = this.Password;
                                Program.TSPConfiguration.UserRole = lc.SNMPManager.UserRole;
                                Debug.WriteLine("|||| User " + this.UserName + "VERIFIED - ROLE " + lc.SNMPManager.UserRole);
                                return true;
                                }
                            }
                        }
                    }
                Debug.WriteLine("XXXXXXX Could not verify user " + this.UserName);
                Program.TSPConfiguration.V3Username = "";
                Program.TSPConfiguration.V3AuthPassword = "";

                Program.TSPConfiguration.V3PrivPassword = "";
                Program.TSPConfiguration.UserRole = 0;
                }
            return ReturnValue;
            }

        private void button_Exit_Click(object sender, EventArgs e)
            {
            this.UseWaitCursor = false;
            this.CarryOn = true;
            ExitApplication = true;
            Application.Exit();
            Application.DoEvents();
            }

        private void timer1_Tick(object sender, EventArgs e)
            {
            if ((textBox_Password.Text.Length >= 8) && (textBox_UserName.Text.Length > 1))
                {
                button_Login.Enabled = true;
                }
            else
                {
                button_Login.Enabled = false;
                }
            }
        }
    }
