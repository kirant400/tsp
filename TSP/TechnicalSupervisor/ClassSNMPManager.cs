/**************************************************************************************************
 
	Class encapsulating the SNMP Manager functionality for the TSP


	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		ClassSNMPManager.cs
	Project:		TSP
	Developer:		CRR 
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			Portions based upon code supplied in Powersnmp from Dart Communications

**************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Dart.Snmp;
using System.Data;
using System.Windows.Forms;


namespace TechnicalSupervisor
    {
    /// <summary>
    /// A class to encapsulate all of the SNMP functionality in the TSP
    /// </summary>
    public class ClassSNMPManager
        {
        public Manager manager = new Manager();
        public string IPAddress = "127.0.0.1";
        public int Port = 161;
        //public List<string> Mibs = new List<string>(new string[] { "IF-MIB.mib", "THRUPUT2v0.mib" });
        public List<string> Mibs = new List<string>(new string[] { "THRUPUT2v1.mib" });
        public DateTime EventsLastUpdated = DateTime.MinValue;
        public DataTable DataTable_Events = new DataTable();
        public List<string> CSV_Events = new List<string>(); 
        public ResponseMessage LastResponse;
        public bool ShowDebug = true;
        public int NoOfMibsLoaded = 0;
        public ClassDebug MyDebug = new ClassDebug();
        public bool UseSNMPV3 = false;
        public int UserRole = 0;    //0= none, 1 = user, 2 = super user, 3 = admin, 4 = developer
        //public string SNMPV3_UserName = "xKi(D/7lm?210DF|m45";                   //TODO - Get from Config
        //public string SNMPV3_AuthPassword = " pEI{sdN?398r>fAd#iSda";            //TODO - Get from Config
        //public string SNMPV3_PrivacyPassword = "kl-2WMF fYIt|wA8~";              //TODO - Get from Config


        //public string SNMPV3_UserName = "agentAuthPriv";    //Default
        //public string SNMPV3_AuthPassword = "auth";         //Default
        //public string SNMPV3_PrivacyPassword = "priv";      //Default

        public string SNMPV3_UserName = "john";                 //Default
        public string SNMPV3_AuthPassword = "abcdefgh";         //Default
        public string SNMPV3_PrivacyPassword = "abcdefgh";      //Default

        public bool LoadMib(string MibFilename)
            {
            try
                {
                using (FileStream fs = new FileStream(MibFilename, FileMode.Open, FileAccess.Read))
                    this.manager.Mib.Parse(fs);
                this.manager.Mib.GenerateNodes();
                this.NoOfMibsLoaded++;
                return true;
                }
            catch (Exception EX)
                {
                Debug.WriteLine("Failed to load " + MibFilename + " " +EX.ToString());
                return false;
                }
            }
        public int LoadMibs()
            {
            int MibCount = 0;
            foreach (string MibFilename in Mibs)
                {
                try
                    {
                    if (!this.LoadMib(MibFilename))
                        return -1;
                    MibCount++;
                    this.NoOfMibsLoaded++;
                    }
                catch (Exception Ex)
                    {
                    Debug.WriteLine("Failed to load " + MibFilename + " " + Ex.ToString());
                    return -1;
                    }
                }
            return MibCount;
            }

        public int LoadMibsIfNotAllReadyLoaded()
            {
            if(this.NoOfMibsLoaded==0)
                {
                return LoadMibs();
                }
            return this.NoOfMibsLoaded;
            }


        public bool RequestEventTable()
            {
            try
                {
                string Request = "eTable";
                if (this.manager.IsActive)
                    this.manager.Close();
        
                this.manager.Start(this.manager_SendGetRequest, new Variable(this.manager.Mib[Request]));
                Debug.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ RequestEvent Table from " + this.IPAddress);
                //textBox_Results.Text = "Request Sent";
                return true;
                }
            catch (Exception EX)
                {
                string msg = "manager.start Exception : " + EX.ToString();
                //MessageBox.Show(msg);
                //textBox_Results.Text = msg;
                Debug.WriteLine(msg);
                return false;
                }
            }

        public List<string> ParseCSV(string CSV)
            {
            List<string> CSVFields = new List<string>();
            string s = "";
            bool InString = false;
            foreach (char C in CSV)
                {
                if (C == '"')
                    {
                    if (InString)
                        {
                        CSVFields.Add(s);
                        s = "";
                        InString = false;
                        }
                    else
                        {
                        s = "";
                        InString = true;
                        }
                    }
                else
                    {
                    s += C.ToString();
                    }
                }
            return CSVFields;
            }

        private void manager_SendGetRequest(SnmpSocket managerSocket, object state)
            {
            try
                {
                //Create Get Request
                GetMessage request = new GetMessage();
                Debug.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ _SendGetRequest return on " + this.IPAddress);
                request.Variables.Add(state as Variable);

                if (this.UseSNMPV3)
                    {
                    request.Version = SnmpVersion.Three;
                    request.Security.User.Name = this.SNMPV3_UserName;                          //ToDo there is only 1 user defined.
                    request.Security.User.AuthenticationPassword = this.SNMPV3_AuthPassword;
                    request.Security.User.PrivacyPassword = this.SNMPV3_PrivacyPassword;

                    request.Security.User.AuthenticationProtocol = AuthenticationProtocol.Sha;
                    request.Security.User.PrivacyProtocol = PrivacyProtocol.Aes128;
                    }
                else
                    {
                    request.Version = SnmpVersion.One;
                    }


                //Send request and get response

                ResponseMessage response = managerSocket.GetResponse(request, this.IPAddress, this.Port);
                this.LastResponse = response;

                if(this.ShowDebug) Debug.WriteLine("The response to " + request.ToString() + " was " + response.ToString());
                if (response.ErrorStatus == Dart.Snmp.ErrorStatus.Success)
                    {
                    Debug.WriteLine("Response from " + response.Origin.Address.ToString() + " we requested " + this.IPAddress + ":" + this.Port.ToString());
                    this.CSV_Events.Clear();
                    this.DataTable_Events = new DataTable();
                    this.DataTable_Events.Columns.Add("HWID");
                    this.DataTable_Events.Columns.Add("Level");
                    this.DataTable_Events.Columns.Add("No");
                    this.DataTable_Events.Columns.Add("Time");
                    this.DataTable_Events.Columns.Add("Channel");
                    this.DataTable_Events.Columns.Add("EventID");
                    this.DataTable_Events.Columns.Add("ID");
                    this.DataTable_Events.Columns.Add("Description");
                    this.DataTable_Events.Columns.Add("Maintenance");
                    this.DataTable_Events.Columns.Add("Time Fetched from NAS");
                    foreach (Dart.Snmp.Variable EventRecord in response.Variables)
                        {
                        string EventRecordAsString = EventRecord.Value.ToString();
                        this.CSV_Events.Add(EventRecordAsString);
                        List<string> EventFields = this.ParseCSV(EventRecordAsString);
                        if(this.ShowDebug) Debug.WriteLine(EventRecord.Id + " : " + EventRecord.Value);
                        this.DataTable_Events.Rows.Add(EventFields.ToArray());
                        }
                    this.EventsLastUpdated = DateTime.Now;
                    }
                manager.Close();
                }
            catch (Exception EX)
                {
                string msg = "manager.Marshal Exception : " + EX.ToString();
                Debug.WriteLine(msg);
                }
            return;
            }



        public bool DisplayEventsInDataGrid(DataGridView DataGrid)
            {
            try
                {
                DataGrid.DataSource = this.DataTable_Events;

                return true;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("Exception loading DataGridView : " + Ex.ToString());
                return false;
                }
            }



        #region "Write to Log File"

        public bool LogEventsToDisk(string NodeName="")
            {
            try
                {
                if(Program.TSPConfiguration.EventLogRootFolder.Length>0)                //Only log if logging specified.
                    {
                    string FolderName = this.GenerateEventLogFolderName(Program.TSPConfiguration.EventLogRootFolder, NodeName, this.EventsLastUpdated);
                    string FileName = this.GenerateEventLogFileName(FolderName, this.EventsLastUpdated);
                    CreateEventLogFolder(FolderName);
                    CreateEventLogFile(FileName);
                    }
                return true;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("Failed to write eventlog file " + Ex.ToString());
                return false;
                }

            }

        public string GenerateEventLogFolderName(string LogRootFolder, string NodeName, DateTime TimeAndDate )
            {
            string Folder = LogRootFolder + "\\" +NodeName + "\\" + TimeAndDate.ToString("yyyy") + "\\" + TimeAndDate.ToString("MM") + "\\" + TimeAndDate.ToString("dd") + "\\" + TimeAndDate.ToString("HH");
            return Folder;
            }

        public string GenerateEventLogFileName(string LogFolder, DateTime DateAndTime)
            {
            string Filename = "events_" + "_" + DateAndTime.ToString("yyyy") + "_" + DateAndTime.ToString("MM") + "_" + DateAndTime.ToString("dd") + "_" + DateAndTime.ToString("HH_mm_ss") + ".csv";
            return  LogFolder + "\\" + Filename;
            }

        public bool CreateEventLogFolder(string FolderName)
            {
            try
                {
                Directory.CreateDirectory(FolderName);
                return true;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("Failed to create EventLogFolder " + Ex.ToString());
                return false;
                }
            }
        public bool CreateEventLogFile(string FileName)
            {
            try
                {
                string FileContents = "\"HW_ID\",\"Level\",\"No.\",\"Time\",\"Channel\",\"EventID\",\"ID\",\"Description\",\"Maintenance\",\"Last Fetched from NAS\"" + "\r\n";
                foreach (string CSVLine in this.CSV_Events)
                    {
                    FileContents += CSVLine + "\r\n";
                    }
                File.WriteAllText(FileName, FileContents);
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("Failed to create EventLogFile " + Ex.ToString());
                return false;
                }
            return true;
            }

                #endregion

        public bool TryToLogin()
            {
            Debug.WriteLine("Trying to log in to " + this.IPAddress);
            return true;
            }

        #region "Verify the user"

        public bool UserChecked = false;

        public bool VerifyCurrentUser()
            {
            this.UserChecked = false;
            try
                {
                string Request = "VerifyUser";
                if (this.manager.IsActive)
                    this.manager.Close();

                this.manager.Start(this.manager_SendVerifyUser, new Variable(this.manager.Mib[Request]));
                Debug.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ SNMP Request VerifyUser from " + this.IPAddress);
                return true;
                }
            catch (Exception EX)
                {
                string msg = "manager.start Exception : " + EX.ToString();
                Debug.WriteLine(msg);
                this.UserChecked = true;
                return false;
                }
            }



        private void manager_SendVerifyUser(SnmpSocket managerSocket, object state)
            {
            try
                {
                //Create Get Request
                GetMessage request = new GetMessage();
                this.UserRole = 0;
                request.Variables.Add(state as Variable);

                if (this.UseSNMPV3)
                    {
                    request.Version = SnmpVersion.Three;
                    request.Security.User.Name = this.SNMPV3_UserName;                          //ToDo there is only 1 user defined.
                    request.Security.User.AuthenticationPassword = this.SNMPV3_AuthPassword;
                    request.Security.User.PrivacyPassword = this.SNMPV3_PrivacyPassword;

                    Debug.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ SNMP V3 _SendVerifyUserRequest for user " + request.Security.User.Name + " to " + this.IPAddress);

                    request.Security.User.AuthenticationProtocol = AuthenticationProtocol.Sha;
                    request.Security.User.PrivacyProtocol = PrivacyProtocol.Aes128;
                    }
                else
                    {
                    request.Version = SnmpVersion.One;
                    }


                //Send request and get response

                ResponseMessage response = managerSocket.GetResponse(request, this.IPAddress, this.Port);
                this.LastResponse = response;
                if (this.ShowDebug) Debug.WriteLine("The response to " + request.ToString() + " was " + response.ToString());

                //string reponsename = response.GetType().FullName;
                if (response.GetType().FullName == "Dart.Snmp.ResponseMessage" ) // eg :Dart.Snmp.ReportType.UnknownUsername)
                    {
                    if (response.ErrorStatus == Dart.Snmp.ErrorStatus.Success)
                        {
                        Debug.WriteLine("Verify User SUCCESS Returned");
                        if (response.Variables.Count > 0)
                            {
                            string temp = response.Variables[0].Value.ToString();
                            Debug.WriteLine("@@@@ UserRole=" + temp);
                            this.UserRole = int.Parse(temp);
                            }
                        }
                    else
                        {
                        Debug.WriteLine("Verify User ERROR Returned");
                        }
                    }
                else
                    {
                    Debug.WriteLine("Verify User incorrect message received : " + response.GetType().FullName);
                    }

                manager.Close();
                }
            catch (Exception EX)
                {
                string msg = "manager.Marshal Exception : " + EX.ToString();
                Debug.WriteLine(msg);
                }
            this.UserChecked = true;
            return;
            }





        #endregion


        #region "Acknowledge All Outstanding Events"
        public bool EventsAcknowledged = false;             // True if some events have been acknowledged
        public bool FinishedAcknowledgingEvents = false;    // We have completed acknowleding the events

        public bool AckAllEvents()
            {
            this.EventsAcknowledged = false;
            this.FinishedAcknowledgingEvents = false;
            try
                {
                string Request = "AckAllEvents";
                if (this.manager.IsActive)
                    this.manager.Close();

                this.manager.Start(this.manager_SendAckAllEvents, new Variable(this.manager.Mib[Request]));
                Debug.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ SNMP Request AckAllEvents from " + this.IPAddress);
                return true;
                }
            catch (Exception EX)
                {
                string msg = "manager.start Exception : " + EX.ToString();
                Debug.WriteLine(msg);
                this.UserChecked = true;
                return false;
                }
            }



        private void manager_SendAckAllEvents(SnmpSocket managerSocket, object state)
            {
            try
                {
                //Create Get Request
                GetMessage request = new GetMessage();

                request.Variables.Add(state as Variable);

                if (this.UseSNMPV3)
                    {
                    request.Version = SnmpVersion.Three;
                    request.Security.User.Name = this.SNMPV3_UserName;                          //ToDo there is only 1 user defined.
                    request.Security.User.AuthenticationPassword = this.SNMPV3_AuthPassword;
                    request.Security.User.PrivacyPassword = this.SNMPV3_PrivacyPassword;

                    Debug.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ SNMP V3 _SendAckAllEvents for user " + request.Security.User.Name + " to " + this.IPAddress);

                    request.Security.User.AuthenticationProtocol = AuthenticationProtocol.Sha;
                    request.Security.User.PrivacyProtocol = PrivacyProtocol.Aes128;
                    }
                else
                    {
                    request.Version = SnmpVersion.One;
                    }


                //Send request and get response

                ResponseMessage response = managerSocket.GetResponse(request, this.IPAddress, this.Port);
                this.LastResponse = response;
                if (this.ShowDebug) Debug.WriteLine("The response to " + request.ToString() + " was " + response.ToString());

                //string reponsename = response.GetType().FullName;
                if (response.GetType().FullName == "Dart.Snmp.ResponseMessage") // eg :Dart.Snmp.ReportType.UnknownUsername)
                    {
                    if (response.ErrorStatus == Dart.Snmp.ErrorStatus.Success)
                        {
                        Debug.WriteLine("AckAllEvents SUCCESS Returned");
                        if (response.Variables.Count > 0)
                            {
                            string temp = response.Variables[0].Value.ToString();
                            Debug.WriteLine("@@@@ Response to AckAllEvents =" + temp);
                            if(int.Parse(temp)==1)
                                {
                                this.EventsAcknowledged = true;
                                }
                            }
                        }
                    else
                        {
                        Debug.WriteLine("AckAllEvents ERROR Returned");
                        }
                    }
                else
                    {
                    Debug.WriteLine("AckAllEvents User incorrect message received : " + response.GetType().FullName);
                    }

                manager.Close();
                }
            catch (Exception EX)
                {
                string msg = "manager.Marshal Exception : " + EX.ToString();
                Debug.WriteLine(msg);
                }
            this.FinishedAcknowledgingEvents = true;
            return;
            }


        #endregion



        }
    }
