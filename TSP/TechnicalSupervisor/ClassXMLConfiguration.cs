/**************************************************************************************************
 
	Class encapsulating the additional configuration options read from an XML file for the TSP

	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		ClassXMLConfiguration.cs
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
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;

/// <summary>
/// Simple Generic Class to read the configuration from an XML file.
/// </summary>
class ClassXMLConfiguration
    {
    public string FileName = "";
    public XmlDocument ConfigDocument;
    public bool ConfigurationLoaded = false;
    public string ErrorMessage = "";

    public bool LoadConfiguration(string FileName)
        {
        this.ErrorMessage = "";
        this.ConfigurationLoaded = false;
        if (!File.Exists(FileName))
            {
            Debug.WriteLine("The File " + FileName + " could not be found");
            return false;
            }
        this.ConfigDocument = new XmlDocument();
        FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        try
            {
            this.ConfigDocument.Load(fs);
            this.FileName = FileName;
            this.ConfigurationLoaded = true;
            }
        catch (Exception Ex)
            {
            this.ErrorMessage = Ex.Message;
            }
        return this.ConfigurationLoaded;
        }

    public string GetSingleSetting(string SettingPath)
        {
        try
            {
            this.ErrorMessage = "";
            XmlNode SettingNode;
            if (this.ConfigurationLoaded)
                {
                SettingNode = this.ConfigDocument.SelectSingleNode(SettingPath);
                if (SettingNode != null)
                    {
                    if (SettingNode.InnerText.ToString().Trim().Length == 0)
                        {
                        return null;                                            //TODO: If empty return null ??                               
                        }
                    return SettingNode.InnerText;
                    }
                else
                    {
                    this.ErrorMessage = "Could not find " + SettingPath;
                    }
                }
            else
                {
                this.ErrorMessage = "Configuration file not loaded";
                }
            }
        catch (Exception Ex)
            {
            this.ErrorMessage = "Exception : " + Ex.ToString();
            }
        return null;
        }

    public bool GetSingleSetting(string SettingPath, ref string result)
        {
        string s;
        if ((s = this.GetSingleSetting(SettingPath)) != null)
            {
            result = s;
            return true;
            }
        return false;
        }

    public bool GetSingleSetting(string SettingPath, ref int result)
        {
        int i;
        string s;
        if ((s = this.GetSingleSetting(SettingPath)) != null)
            {
            if (int.TryParse(s.Trim(), out i))
                {
                result = i;
                return true;
                }
            }
        return false;
        }
    public bool GetSingleSetting(string SettingPath, ref bool result)
        {
        bool flag;
        string s;
        if ((s = this.GetSingleSetting(SettingPath)) != null)
            {
            s = s.Trim().ToLower();
            if (s.StartsWith("y"))
                {
                result = true;
                return true;
                }
            if (s.StartsWith("n"))
                {
                result = false;
                return true;
                }
            if (bool.TryParse(s, out flag))
                {
                result = flag;
                return true;
                }
            }
        return false;
        }

    }

