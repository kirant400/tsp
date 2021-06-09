/**************************************************************************************************
 
	Form to allow the user to acknowledge all alarms in the TSP using SNMP

	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		FormAcknowledge.cs
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
using System.Threading;
using System.Diagnostics;

namespace TechnicalSupervisor
    {

    public partial class FormAcknowledge : Form
        {
        public LogClients AttachedLogClients;       //#50 Log clients passed into login form when invoked.
        public String UserName = "";
        public String Password = "";
        public LogInterface m_log;           // Log to disk/screen/etc interface
        public FormAcknowledge()
            {
            InitializeComponent();
            }

        private void button_CancelLogin_Click(object sender, EventArgs e)
            {

            }

        private void timer1_Tick(object sender, EventArgs e)
            {
            if((textBox_Password.Text.Length>=8)&&(textBox_UserName.Text.Length>1))
                {
                button_Acknowledge.Enabled = true;
                }
            else
                {
                button_Acknowledge.Enabled = false;
                }
            }

        private void button_Acknowledge_Click(object sender, EventArgs e)
            {
            textBox_Results.Text = "";
            textBox_Results.Visible = false;
            this.UserName = textBox_UserName.Text;
            this.Password = textBox_Password.Text;
           
            if (Password.Length < 8)
                {
                textBox_Results.ForeColor = Color.Orange;
                textBox_Results.Text = "The password must be 8 characters or more";
                textBox_Results.Visible = true;
                }
            else
                {
                this.progressBar1.Visible = true;
                this.UseWaitCursor = true;
                //if (false)      //TODO Always false - remove this
                if (this.AcknowledgeAllAlarms(this.UserName, this.Password))
                    {
                    this.UseWaitCursor = false;
                    this.progressBar1.Visible = false;
                    textBox_Results.ForeColor = Color.GreenYellow;
                    textBox_Results.Text = "All Events Acknowledged";
                    this.m_log.LogInfo("All events acknowledged by user " + this.UserName);
                    textBox_Results.Visible = true;
                    //this.Close();
                    }
                else
                    {
                    this.UseWaitCursor = false;
                    this.progressBar1.Visible = false;
                    textBox_Results.ForeColor = Color.Orange;
                    textBox_Results.Text = "Failed to Acknowledge All Events";
                    this.m_log.LogWarn("Failed to acknowledge all events for user " + this.UserName);
                    textBox_Results.Visible = true;
                    }
                //for (int i = 1; i <= 10; i++)
                //    {
                //    progressBar1.Value = i;
                //    Application.DoEvents();
                //    Thread.Sleep(1000);
                //    }
                
                }
            }


        public int NASCount = 0;
        public int NASCount_EventsAcknowledged = 0;

        private bool AcknowledgeAllAlarms(string UserName, string Password)
            {
            bool ReturnValue = false;
            this.NASCount = 0;
            this.NASCount_EventsAcknowledged = 0;
            if (this.AttachedLogClients != null)
                {
                foreach (LogClient lc in AttachedLogClients)
                    {
                    lc.SNMPManager.SNMPV3_UserName = UserName;
                    lc.SNMPManager.SNMPV3_AuthPassword = Password;
                    lc.SNMPManager.SNMPV3_PrivacyPassword = Password;
                    //lc.SNMPManager.UserRole = Program.TSPConfiguration.UserRole;
                    Debug.WriteLine("");
                    Debug.WriteLine("");
                    Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TRYING " + lc.Hw_name);
                    if (lc.SNMPManager.AckAllEvents() == true)
                        {
                        int i = 0;
                        while (lc.SNMPManager.FinishedAcknowledgingEvents == false)
                            {
                            System.Threading.Thread.Sleep(100);
                            if(progressBar1.Value == progressBar1.Maximum)
                                {
                                progressBar1.Value = progressBar1.Minimum;
                                }
                            else
                                {
                                progressBar1.Value++;
                                }
                            Application.DoEvents();
                            if (i++ > 20)
                                break;
                            }
                        if (lc.SNMPManager.EventsAcknowledged == true)
                            {
                            Debug.WriteLine("Events Acknowledged on " + lc.Ep.ToString());
                            this.NASCount_EventsAcknowledged++;
                            ReturnValue = true;
                            }
                        else
                            {
                            Debug.WriteLine("Events could NOT be Acknowledged on " + lc.Ep.ToString());
                            }
                        }
                    this.NASCount++;
                    }
                }
            return ReturnValue;
            }

        private void FormAcknowledge_Load(object sender, EventArgs e)
            {
            this.CenterToParent();
            }
        }
    }
