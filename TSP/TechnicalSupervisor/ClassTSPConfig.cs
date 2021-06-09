/**************************************************************************************************
 
	Class encapsulating the additional configuration options for the TSP

	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		ClassTSPConfig.cs
	Project:		TSP
	Developer:		CRR 
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			

**************************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Deployment;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

namespace TechnicalSupervisor
    {
    /// <summary>
    /// A class to encapsulate the TSP configuration derived from the command line options
    /// </summary>
    public class ClassTSPConfiguration
        {

        public string ApplicationName = "TSP";
        public string[] CommandLineargs;
        public string ErrorMessage = "";

        public string ConfigurationFolder = "";                         //CRR if empty use the default
        public string EventLogRootFolder = "";                          //CRR If empty then logging is not enabled
        public bool ShowDebug = true;                                       //CRR #41 Show debug
        public string DefaultConfigurationFolder = "";                  //CRR #41 As used by the TSP as a default
        public bool install_tsp = false;                                //CRR #41 If command line option -i is specified
        public bool uninstall_tsp = false;                              //CRR #41 If command line option -u is specified
        public bool UseSNMP = false;                                    //CRR #41 If command line option -snmp is specified
        public bool UseSNMPV3 = false;                                  //CRR #41 If command line option -v3 is specified

        private ClassXMLConfiguration XMLConfiguration = new ClassXMLConfiguration();
        public string V3Username = "";      //"agentAuth";
        public string V3AuthPassword = "";  //"auth";
        public string V3PrivPassword = "";  // priv";
        public int UserRole = 0;            //0 = none, 1 = user, 2 = super user, 3 = administrator, 4 = developer
        public bool V3UsernameAndPasswordDefined = false;

        public void ClearOptions()
            {
            this.ConfigurationFolder = "";
            this.EventLogRootFolder = "";
            this.ShowDebug = false;
            this.ErrorMessage = "";
            this.install_tsp = false;
            this.uninstall_tsp = false;
            this.UseSNMP = false;
            this.UseSNMPV3 = false;
            }

        /// <summary>
        ///  Check to see if -XML is specified in the command line arguments and if so then process the XML configuration file.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>

        /* Example Configuration File
        <?xml version="1.0" encoding="utf-8" ?>
        <thruput>
          <tsp>
            <debug>Y</debug>
            <usesnmp>Y</usesnmp>
            <logfolder>c:\temp</logfolder>
            <snmpv3only>Y</snmpv3only>
          </snmpagent>
        </thruput>
        */

        private bool ReadXMLConfigurationFileIfDefined(string[] args)
            {
            int NoOfArgs = args.Length;
            this.ErrorMessage = "";
            for (int i = 0; i < args.Length; i++)
                {
                if (args[i].Trim().ToLower() == "-xml")            // Now uses -snmp filename for specifying SNMP
                    {
                    if ((i + 1) < NoOfArgs)
                        {
                        this.UseSNMP = true;
                        string XMLFilename = args[i + 1];
                        string temp = "";
                        if (File.Exists(XMLFilename))
                            {
                            if (XMLConfiguration.LoadConfiguration(XMLFilename))
                                {
                                if (XMLConfiguration.GetSingleSetting("//thruput") == null)
                                    {
                                    this.ErrorMessage = "Invalid thruput SNMP XML configuration file " + XMLFilename;
                                    Console.WriteLine(this.ErrorMessage);
                                    //MessageBox.Show(this.ErrorMessage);
                                    return false;
                                    }
                                XMLConfiguration.GetSingleSetting("//thruput/tsp/debug", ref this.ShowDebug);

                                XMLConfiguration.GetSingleSetting("//thruput/tsp/snmpv3only", ref this.UseSNMPV3);
                                XMLConfiguration.GetSingleSetting("//thruput/tsp/usesnmp", ref this.UseSNMP);

                                if (XMLConfiguration.GetSingleSetting("//thruput/tsp/logfolder", ref temp))
                                    {
                                    this.EventLogRootFolder = temp.Trim();
                                    if (!Directory.Exists(this.EventLogRootFolder))
                                        {
                                        this.ErrorMessage = "The Log folder " + this.EventLogRootFolder + " does not exist";
                                        Console.WriteLine(this.ErrorMessage);
                                        //MessageBox.Show(this.ErrorMessage);
                                        return false;
                                        }
                                    }

                                if (XMLConfiguration.GetSingleSetting("//thruput/tsp/config", ref temp))
                                    {
                                    this.ConfigurationFolder = temp.Trim();
                                    if (!Directory.Exists(this.ConfigurationFolder))
                                        {
                                        this.ErrorMessage = "The configuration folder " + this.ConfigurationFolder + " does not exist";
                                        Console.WriteLine(this.ErrorMessage);
                                        return false;
                                        }
                                    ClassDebug.DebugPrintLine("The configuration folder is set to " + this.ConfigurationFolder);
                                    if (!File.Exists(this.ConfigurationFolder + "\\config\\config.xml"))
                                        {
                                        this.install_tsp = true;                    // if the config/config folder does not exist ask for the XML config.
                                        }
                                    }



                               // XMLConfiguration.GetSingleSetting("//thruput/tsp/snmpusername", ref this.V3Username);
                               // XMLConfiguration.GetSingleSetting("//thruput/tsp/snmppassword", ref this.V3AuthPassword);       // Both Auth and Priv passwords are the same
                               // XMLConfiguration.GetSingleSetting("//thruput/tsp/snmppassword", ref this.V3PrivPassword);       // Both Auth and Priv passwords are the same       
                                //if ((this.V3Username.Length > 0) && (this.V3AuthPassword.Length > 0) && (this.V3PrivPassword.Length > 0))
                                //    {
                                //    this.V3UsernameAndPasswordDefined = true;
                                //    }
                                }
                            else
                                {
                                this.ErrorMessage = "Invalid SNMP XML configuration file " + XMLFilename;
                                //MessageBox.Show(this.ErrorMessage);
                                Console.WriteLine(this.ErrorMessage);
                                return false;
                                }
                            }
                        else
                            {
                            this.ErrorMessage = "SNMP XML configuration file " + XMLFilename + " not found";
                            Console.WriteLine(this.ErrorMessage);
                            //MessageBox.Show(this.ErrorMessage);
                            return false;
                            }
                        }
                    else
                        {
                        this.ErrorMessage = "SNMP XML configuration file not specified";
                        //MessageBox.Show(this.ErrorMessage);
                        Console.WriteLine(this.ErrorMessage);
                        return false;
                        }
                    }
                }
            return true;
            }




        public bool ProcessCommandLineArguments(string[] args)
            {
            try
                {
                ClearOptions();
                this.CommandLineargs = args;

                if (this.ReadXMLConfigurationFileIfDefined(args) == false)            //TODO Do this now or later ?
                    {
                    return false;
                    }

                int NoOfArgs = args.Length;
                for (int i = 0; i < args.Length; i++)
                    {
                    switch (args[i].Trim().ToLower())
                        {
                        // Command Line Options from Previous Versions:
                        case "-i":
                            this.install_tsp = true;
                            break;
                        case "-u":
                            this.uninstall_tsp = true;
                            break;
                        case "-h":
                        case "-?":
                            return false;
                        // Command line options introduced in version 1.8 for #40 and #41
                        case "":
                            break;
                        case "-config":
                        case "-c":
                            if ((i + 1) < NoOfArgs)
                                {
                                this.ConfigurationFolder = args[i + 1];
                                i++;
                                if (!Directory.Exists(this.ConfigurationFolder))
                                    {
                                    this.ErrorMessage = "The configuration folder " + this.ConfigurationFolder + " does not exist";
                                    Console.WriteLine(this.ErrorMessage);
                                    return false;
                                    }
                                ClassDebug.DebugPrintLine("The configuration folder is set to " + this.ConfigurationFolder);
                                if (!File.Exists(this.ConfigurationFolder + "\\config\\config.xml"))
                                    {
                                    this.install_tsp = true;                    // if the config/config folder does not exist ask for the XML config.
                                    }
                                }
                            else
                                {
                                this.ErrorMessage = "The configuration folder has not been specified";
                                Console.WriteLine(this.ErrorMessage);
                                return false;
                                }
                            break;
                        case "-l":
                            if ((i + 1) < NoOfArgs)
                                {
                                this.EventLogRootFolder = args[i + 1];
                                if (!Directory.Exists(this.EventLogRootFolder))
                                    {
                                    this.ErrorMessage = "The log file folder " + this.EventLogRootFolder + " does not exist";
                                    Console.WriteLine(this.ErrorMessage);
                                    return false;
                                    }
                                ClassDebug.DebugPrintLine("The log file folder is set to " + this.EventLogRootFolder);
                                i++;
                                }
                            else
                                {
                                this.ErrorMessage = "The configuration folder has not been specified";
                                Console.WriteLine(this.ErrorMessage);
                                return false;
                                }
                            break;
                        case "-d":
                        case "-debug":
                            this.ShowDebug = true;
                            break;
                        case "-snmp":                     
                            this.UseSNMP = true;
                            break;
                        case "-v3":
                            this.UseSNMPV3 = true;
                            break;
                        case "-xml":            // Ignore as this is handled earlier
                            i++;                // Skip xml config filename
                            break;
                        default:
                            // Just ignore at the moment
                            break;
                        }
                    }
                }
            catch (Exception ex)
                {
                Debug.WriteLine("Exception processing the commandline " + ex.ToString());
                return false;
                }
            return true;
            }


        public void ShowCommandLineHelp()
            {
            string v = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string Title = "Thruput " + this.ApplicationName + " V" + v;
            string Help = "Command line options:" + Environment.NewLine;
            Help += this.GetCommandLineOptions() + Environment.NewLine;
            if (this.ErrorMessage.Length > 0)
                MessageBox.Show("Error: " + ErrorMessage + Environment.NewLine + Environment.NewLine + Help, Title);
            else
                MessageBox.Show(Help, Title);
            }


        public String GetCommandLineOptions()
            {
            string Options =
                            "  -config [folder]\tSpecify the configuration folder " + Environment.NewLine +
                            "  -d\t\tEnable debug " + Environment.NewLine +
                            "  -h\t\tDisplay help and exit" + Environment.NewLine +
                            "  -i [filename]\tInstall a new TSP configuration" + Environment.NewLine +
                            "  -l [folder]\tSpecify the log file folder " + Environment.NewLine +
                            "  -snmp\t\tUse SNMP to communicate with the NAS(s)" + Environment.NewLine +
                            "  -u\t\tUninstall an existing configuration" + Environment.NewLine +
                            "  -v3\t\tUse encrypted SNMP v3 protocol" + Environment.NewLine +
                            "  -xml [filename]\tSpecify (XML) SNMP configuration file " + Environment.NewLine + Environment.NewLine +
                            " If no configuration folder is specified the following will be used:" + Environment.NewLine +
                            this.DefaultConfigurationFolder + Environment.NewLine;

            return Options;
            }

        public string GetCreationDate()
            {
            return File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString();
            }



        }
    }
