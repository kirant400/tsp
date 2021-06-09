/**************************************************************************************************
 
	Class to handle the SNMP configuration options


	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		ClassSNMPConfig.cs
	Project:		TSP
	Developer:		CRR 
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			

**************************************************************************************************/

using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;

namespace TechnicalSupervisor
    {

    /// <summary>
    /// Class to handle all of the SNMP configuration options
    /// </summary>
    /// <remarks>
    /// The initial idea was to have the configuration stored in an XML file. 
    /// However this idea is currently on hold.
    /// </remarks>
    public class ClassSNMPConfig
        {
        public XmlDocument XMLdoc;
        public XmlNode RootNode;
        public XElement XML;
        public string XMLFilename = "snmp.xml";
        public string LastErrorMessage = "";
        public XPathDocument XMLPath;
        public XPathNavigator XMLNavigator;


        public void Set_LastErrorMessage(string msg)
            {
            this.LastErrorMessage = msg;
            Debug.WriteLine(msg);
            }
        public void ClearLastErrorMessage()
            {
            this.LastErrorMessage = "";
            }

        public bool ReadXML()
            {
            try
                {
                ClearLastErrorMessage();
                XMLdoc = new XmlDocument();
                XMLdoc.Load(this.XMLFilename);
                XML = XElement.Parse(XMLdoc.OuterXml);
                RootNode = XMLdoc.SelectSingleNode("thruput");
                XMLPath = new XPathDocument(this.XMLFilename);
                XMLNavigator = XMLPath.CreateNavigator();
                return true;
                }
            catch (Exception Ex)
                {
                Set_LastErrorMessage("Failed to read file " + this.XMLFilename + " : " + Ex.ToString());
                return false;
                }
            }


        public bool ReadXML(string XMLFilename)
            {
            ClearLastErrorMessage();
            this.XMLFilename = XMLFilename;
            return this.ReadXML();
            }

        public List<string> Get_XMLElementGivenName(string XMLElementName)
            {
            ClearLastErrorMessage();
            List<string> ElementContents = new List<string>();
            XmlNodeList elemList = XMLdoc.GetElementsByTagName(XMLElementName);
            for (int i = 0; i < elemList.Count; i++)
                {
                Debug.WriteLine(elemList[i].InnerXml);
                ElementContents.Add(elemList[i].InnerXml);
                }
            return ElementContents;
            }

        public List<string> Get_XMLElementGivenPath(string XMLPath)
            {
            ClearLastErrorMessage();
            List<string> ElementContents = new List<string>();
            try
                {
                XPathNodeIterator nodes = XMLNavigator.Select(XMLPath);
                while (nodes.MoveNext())
                    {
                    Debug.WriteLine(nodes.Current.Name + " : " + nodes.Current.Value);
                    ElementContents.Add(nodes.Current.Value);
                    }
                }
            catch (Exception Ex)
                {
                Set_LastErrorMessage("Failed to find path " + XMLPath + " : " + Ex.ToString());
                }

            return ElementContents;

            }



        }
    }
