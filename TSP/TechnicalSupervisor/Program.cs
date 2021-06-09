using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;                   //CRR

namespace TechnicalSupervisor
    {
    //CRR  A class to store 'global variables' to get around some of the issues in the TSP code
    public static class GLOBALS_COMMS_WARNING
        {
        // COMMS_ERROR_DISPLAY : Communications issue with NAS
        public static string DISPLAY_MESSAGE = "Waiting for Response...";
        public static bool STARTED = false;           // True if attempting communications with a NAS
        public static int TIMER_STATE = 0;            // State 0: Waiting for Comms to start
                                                      // State 1: Wait for intial timeout if comms not complete or goto state 0 if they complete
                                                      // State 2: Flash Message if comms still not complete else go back to state 0
        public static int TIMEOUT_COUNTER = 0;        // A  countdown counter used for the initial 5(?) second timeout

        //public static int COMMS_ERROR_DISPLAY_STATUS = 0;       //TODO
        //public static bool COMMS_ERROR_DISPLAY_NAS_OK = false;  //TODO
        }



    static class Program
        {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///  
        //CRR Issue #40
        public static bool UseSNMP = false;
        //public static ClassSNMPConfig SNMPConfig;
        public static int NoOfLogClients = 0;
        //public static string EventLogRootFolder = "c:\\temp\\logs";     //TODO get from the command line.
        public static ClassTSPConfiguration TSPConfiguration = new ClassTSPConfiguration();
        //public static LogInterface m_log; // Log to disk/screen/etc interface
        [STAThread]

        static void Main(string[] args)
            {
            m_log = LogInterface.Instance;
            bool install_tsp = false;

            // string install_base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Thruput\TSP");

            TSPConfiguration.DefaultConfigurationFolder =  m_log.Install_base + "\\config\\";
            string load_config = Path.Combine(m_log.Install_base, @"config\config.xml");
            string config = string.Join(" ", args).Trim();
            //string SNMP_Config = "snmp.xml"; //CRR Issue #40

            if (TSPConfiguration.ProcessCommandLineArguments(args) == false)
                {
                TSPConfiguration.ShowCommandLineHelp();
                return;
                }

            if(TSPConfiguration.ConfigurationFolder.Length>0)                   // If a configuration folder has been defined then use that
                {
                m_log.Install_base = TSPConfiguration.ConfigurationFolder;
                load_config = Path.Combine(m_log.Install_base, @"config\config.xml");
                config = "";
                }

            if (TSPConfiguration.uninstall_tsp)
                {
                if (MessageBox.Show("Press OK to continue", "Configuration will be deleted", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                    Directory.Delete(m_log.Install_base);
                    }
                return;
                }
            else if (TSPConfiguration.install_tsp)
                {
                install_tsp = true;
                }
            else if (TSPConfiguration.UseSNMP)
                {
                Program.UseSNMP = true;         // Todo Make config file configurable option on the command line.
                //SNMPConfig = new ClassSNMPConfig();
                //if (!SNMPConfig.ReadXML("snmp.xml"))
                //    {
                //    MessageBox.Show("Failed to find or process the SNMP Configuration File : " + SNMPConfig.XMLFilename, "TSP Help dialog");
                //    return;
                //    }
                }

            //######################################################################
            //OLD pre 1.8 version
            //if (config.Equals("-?", StringComparison.CurrentCultureIgnoreCase) || config.Equals("-h", StringComparison.CurrentCultureIgnoreCase))
            //{

            //    MessageBox.Show("TSP.exe [-hui] [config.xml]\r\n\t-h\tShow this help page\r\n\t-i\tInstall new configuration\r\n\t-u\tUninstall configuration\r\n\r\nPassing a valid CONFG.XML file will install a new configuration file.\r\n\r\nStart TSP.exe without command line arguments for normal operation.", "TSP Help dialog");
            //    return;
            //}
            //// Uninstall
            //else if (config.Equals("-u", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    if (MessageBox.Show("Press OK to continue", "Configuration will be deleted", MessageBoxButtons.OKCancel) == DialogResult.OK)
            //    {
            //        Directory.Delete(m_log.Install_base);
            //    }
            //    return;
            //}
            //// Install
            //else if (config.Equals("-snmp", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //    Program.UseSNMP = true;         // Todo Make config file configurable option on the command line.
            //    SNMPConfig = new ClassSNMPConfig();
            //    if(!SNMPConfig.ReadXML("snmp.xml"))
            //        {
            //        MessageBox.Show("Failed to find or process the SNMP Configuration File : " + SNMPConfig.XMLFilename, "TSP Help dialog");
            //        return;
            //        }
            //    }
            //else if ((config.Equals("-i", StringComparison.CurrentCultureIgnoreCase)) || (!File.Exists(load_config) || (config.Length > 0)))
            //{
            //    install_tsp = true;
            //}
            //####################################################################

            m_xml = XmlDatabaseInterface.Instance;
            if (install_tsp)
                {
                // If user has a bad syntax, or file not found
                if (!File.Exists(config))
                    {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Title = "Select a configuration file to configure the Technical Supervisor";
                    dlg.Filter = "Config XML (*.xml)|*.xml|All Files|*.*";
                    if (dlg.ShowDialog() != DialogResult.OK) return; // Early exit...
                    config = dlg.FileName;
                    }

                // Copy the config file into our local folder
                if (!m_xml.safe_file_copy(config, Path.GetDirectoryName(load_config), Path.GetFileName(load_config)))
                    {
                    MessageBox.Show(m_xml.last_err, "Error! Unable to copy configuration file into config folder");
                    return;
                    }

                // Extract all of the resources into our local folder
                if (!m_xml.ExtractXmlResouces(config, Path.GetDirectoryName(load_config)))
                    {
                    MessageBox.Show(m_xml.last_err, "Error! Unable to extract resources from config file");
                    return;
                    }

                MessageBox.Show("The TSP configuration has been successfully installed.\r\n\r\nPlease start the TSP again to start using the new configuration.\r\n\r\nTo install a new configuration use the -i option on the command line.", "TSP Configuration complete");
                return;
                }

            // Finally, load in the XML configuration - all filenames are assumed to be local
            if (LoadConfig(load_config))
                {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if(Program.UseSNMP)
                    {
                    FormLogin F = new FormLogin();
                    F.m_log = Program.m_log;
                    F.AttachedLogClients = m_xml.LogClients;
                        Application.Run(F);
                    while(F.CarryOn==false)
                        {
                        Application.DoEvents();
                        }
                    if (F.ExitApplication)
                        {
                        Application.Exit();
                        }
                    else
                        {
                        m_log.LogInfo("Login by User " + F.UserName);
                        Application.Run(new Form1());
                        }
                    }
                else
                    {
                    Application.Run(new Form1());
                    }
                }
            }

        static XmlDatabaseInterface m_xml;
        static LogInterface m_log;
        public static string ChrisVar = "Chris";
        public static List<LogClient> LogClientsList = new List<LogClient>();          //CRR Issue #40 Share the collected the log clients data
        public static List<LogClient.LogSummary> LogClientSummaryRecords = new List<LogClient.LogSummary>();
        static bool LoadConfig(string config)
            {
            try
                {
                if ((config.Length == 0) || (!File.Exists(config)))
                    {
                    string[] file_list = Directory.GetFiles(".", "*.xml");
                    if (file_list.Length == 1)
                        {
                        config = file_list[0];
                        }
                    else
                        {
                        MessageBox.Show("Usage: TechnicalSupervisor.exe config.xml", "Missing xml configuration file");
                        return false;
                        }
                    }
                // Process the selected XML configuration file
                m_xml = XmlDatabaseInterface.Instance;
                if (!m_xml.ParseXml(config))
                    {
                    MessageBox.Show(m_xml.last_err, string.Format("Error! Processing {0}", Path.GetFileName(config)));
                    return false;
                    }

                return true;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            return false;
            }

        }
    }
