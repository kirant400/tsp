
/**************************************************************************************************
 
	The main form displaying the mimic in the TSP

	Copyright (C) 2016-2020 Thruput Ltd
    All rights reserved
	
	Filename:		ClassTSPConfig.cs
	Project:		TSP
	Developer:		AC with additions by CRR since June 2019
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			

**************************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Media;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using Dart.Snmp;

namespace TechnicalSupervisor
    {


    public partial class Form1 : Form
        {
        XmlDatabaseInterface m_xml; // Read all of the database
        TspStatusList m_status; // Internal state engine for the TSP status
        LogInterface m_log; // Log to disk/screen/etc interface
        string maintenance_fname; // Saved Maintenance filename

        List<LogClient.LogSummary> SNMP_LogSummary_TEST = new List<LogClient.LogSummary>();
        List<LogClient.LogSummary> LogClient_LogSummary_TEST = new List<LogClient.LogSummary>();

        // Initialise the TSP main screen
        public Form1()
            {
            try
                {
                m_xml = XmlDatabaseInterface.Instance;
                m_log = LogInterface.Instance;
                maintenance_fname = Path.Combine(m_log.Install_base, @"maintenance\entries.xml");
                m_maintenance = LoadFileMaintenance();
                UpdateFileMaintenance(); // Update from the servers
                player = new System.Media.SoundPlayer(Properties.Resources.Electronic_Chime_KevanGC_495939803);

                Debug.WriteLine("Starting Technical Supervisor");
                m_log.LogInfo("Starting Technical Supervisor");

                // Create the entire TSP status list
                m_status = new TspStatusList();
                foreach (Hw_id hw_node in m_xml.Db.m_hw_list)
                    {
                    // Create a new top-level node
                    if (!m_xml.Tsp_hw_nodes.Exists(x => x.id == hw_node.id))
                        {
                        Hw_id parent = hw_node;
                        // hardware doesn't exist in the TSP mimic, so let's check the parent node tree
                        bool found = false;
                        while (true)
                            {
                            if (parent.parent_type < 0) break; // not found :-(
                            if (parent.channels.Count == 0) break; // parent not found :-(
                            List<Hw_id> parents;
                            parents = m_xml.Db.m_hw_list.FindAll(x => (x.channels.Contains(parent.channels[0]) && (x.hw_type == parent.parent_type)));
                            if (parents.Count == 0)
                                {
                                parent.parent_type = m_xml.Db.m_parent_lookup[parent.parent_type];
                                continue; // Parent not found, so try again with their parent
                                }

                            m_log.LogWarn(string.Format("Hardware node {0} not found on TSP mimic. Parent node will be used if event occurs.", hw_node.id));
                            /*
                            foreach (Hw_id parent_node in parents)
                            {
                                m_status.AddSubItem(hw_node, parent_node.id);
                            }
                             */
                            found = true;
                            break;
                            //Otherwise, try the parent
                            }
                        if (!found)
                            {
                            m_log.LogError(string.Format("Cannot find a suitable parent node for hardware {0}. Check SQLite config database", hw_node.id));
                            continue;
                            }
                        }
                    else
                        {
                        m_status.AddSubItem(hw_node);
                        }
                    }

                /*
                foreach (var sw_item in m_xml.Db.m_sw_list)
                {
                    TspStatusItem item = m_status.items.Find(x => x.Id == sw_item.hw_id);
                    if (item != null)
                    {
                        TspStatusSubItem subitem;
                        subitem = new TspStatusSubItem(sw_item.id, sw_item.hw_id);
                        item.Sub_items.Add(subitem);
                    }
                }
                 */
                Program.NoOfLogClients = 0;       //CRR Issue #40
                foreach (LogClient lc in m_xml.LogClients)
                    {
                    if (!lc.Enabled)
                        {
                        LogInterface log = LogInterface.Instance;
                        log.LogWarn(string.Format("Server {0} ({1}) has been disabled in the config.", m_xml.GetNodeName(lc.Hw_id), lc.Hw_id));
                        continue;
                        }

                    lc.Acknowledged += new LogClient.EventAcknowledgedHandler(lc_Acknowledged);

                        {
                        // Create a base array to capture every expected node ID for a given server
                        lc.base_array = new List<LogClient.LogSummary>();
                        foreach (TspStatusItem item in m_status.items)
                            {
                            // TspStatusItem item = m_status.items.Find(x => x.Id == lc.Hw_id);
                            foreach (TspStatusSubItem sub_item in item.Sub_items.FindAll(x => (x.id_src == lc.Hw_id)))
                                {
                                List<int> ls1 = m_xml.GetHardwareNodeList().ToList();
                                if (!ls1.Exists(x => (x == sub_item.id))) continue; // Only add items that exist in our TSP

                                SqliteDatabaseInterface.Sw_id sw_id = m_xml.LookupNodeId(sub_item.id);
                                for (int i = 0; i < 3; i++)
                                    {
                                    LogClient.LogSummary ls = new LogClient.LogSummary()
                                        {
                                        level = i,
                                        hw_id = sub_item.id,
                                        targetID = sub_item.id,
                                        sw_type = sw_id.sw_type,
                                        channel = sub_item.channel,
                                        };
                                    lc.base_array.Add(ls);
                                    }
                                }
                            }
                        }
                    Program.NoOfLogClients++;
                    }


                m_status.Changed += new ChangedEventHandler(tspStateChange);

                // Setup the gui based on m_xml
                InitializeComponent();

                Font = new Font(FontFamily.GenericSansSerif, 1 * m_xml.Font_size);

                contextMenuStrip1.Font = Font;
                contextMenuStrip2.Font = Font;

                BackColor = m_xml.Background;
                pictureBox1.Dock = DockStyle.None;
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox1.Location = new Point(0, 0);
                pictureBox1.Image = m_xml.Bg_image;

                panel1.Anchor = AnchorStyles.None;
                panel1.Size = ClientSize;
                panel1.Location = new Point(50, 50);
                panel1.Dock = DockStyle.Fill; // Fill;// None; // .None;

                // BackgroundImage = m_xml.bg_image;
                int unique_id = 0;
                foreach (Tsp_hw_node hw in m_xml.Tsp_hw_nodes)
                    {
                    if (hw.isSystem)
                        {
                        UserButton button = new UserButton();
                        button.Text = hw.text;
                        button.Location = hw.rect.Location;
                        button.Size = hw.rect.Size;
                        button.Name = "_USER_" + unique_id++.ToString();
                        // button.BackgroundImage = hw.img_main;
                        button.MainImage = hw.img_main;
                        button.ContextMenuStrip = contextMenuStrip2;
                        button.Click += new EventHandler(button_Click);
                        button.tsp_node = hw;
                        // button.Font = this.Font;
                        //button.Font = new Font(FontFamily.GenericSansSerif, hw.font_size);
                        pictureBox1.Controls.Add(button);
                        }
                    else if (hw.isActive)
                        {
                        SqliteDatabaseInterface.Sw_id item = m_xml.LookupNodeId(hw.id);
                        if (item.id < 0)
                            {
                            m_log.LogError(String.Format("Unknown node {0} found in XML database", hw.id));
                            continue;
                            }

                        TspButton button = new TspButton();
                        button.Text = hw.text;
                        button.Location = hw.rect.Location;
                        button.Size = hw.rect.Size;
                        button.Click += new EventHandler(tspButton_Click);
                        button.hw_id = m_xml.Db.GetHwId(hw.id);
                        //button.hw_id = hw.id;
                        button.Name = "_TSP_" + hw.id.ToString();
                        //button.Font = new Font(FontFamily.GenericSansSerif, hw.font_size);
                        if (hw.actions.Count > 0)
                            {
                            button.ContextMenuStrip = contextMenuStrip1;
                            button.Actions = hw.actions;
                            }
                        button.tsp_node = hw;
                        pictureBox1.Controls.Add(button);
                        }
                    else
                        {
                        UserButton button = new UserButton();
                        button.Text = hw.text;
                        button.Location = hw.rect.Location;
                        button.Size = hw.rect.Size;
                        button.Name = "_USER_" + unique_id++.ToString();
                        button.MainImage = null;
                        //button.Font = new Font(FontFamily.GenericSansSerif, hw.font_size);
                        if (hw.UserActions.Count > 0)
                            {
                            button.Click += new EventHandler(userButton_Click);
                            button.UserActions = hw.UserActions;
                            }
                        if (hw.actions.Count > 0)
                            {
                            button.ContextMenuStrip = contextMenuStrip1;
                            button.Actions = hw.actions;
                            }
                        button.tsp_node = hw;
                        pictureBox1.Controls.Add(button);
                        }
                    }
                panel1.PerformLayout();

                //Text = m_xml.Name;
                this.DisplayTitleAndUser();


                //CRR Issue #40 If SNMP disable acknowledge menu facility and replace with the Acknowledge All dialog
                // 
                if (Program.UseSNMP)
                    {
                    toolStripMenuItem1.Text = "Acknowledge all Events";
                    toolStripMenuItem1.DropDownItems.Clear();
                    toolStripMenuItem1.Click += new EventHandler(toolStripMenuItem1_Click);
                    toolStripMenuItem_ChangeUser.Visible = true;
                    //contextMenuStrip2.Items.Remove(toolStripMenuItem1);
                    contextMenuStrip2.Items.Remove(toolStripSeparator3);
                    if(Program.TSPConfiguration.ShowDebug)
                        {
                        groupBox_Debug.Visible = true;                                  //TODO Just for SNMP DEBUG
                        button_ViewSnmpEvents.Visible = true;                           //TODO Just for SNMP DEBUG
                        }
                    else
                        {
                        groupBox_Debug.Visible = false;                                  //TODO Just for SNMP DEBUG
                        }
                    }

                Form3 frm3 = Form3.Instance;
                RefreshTsp(m_xml.LogClients.AsList());
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        List<MaintenanceEntry> m_maintenance;

#if true // Load/save maintenance remotely
        List<MaintenanceEntry> LoadFileMaintenance()
            {
            try
                {
                DateTime utcnow = DateTime.Now;  //CRR issue #18 Use just local time

                // Update all of the maintenance events from all servers
                List<MaintenanceEntry> events = new List<MaintenanceEntry>();
                foreach (LogClient lc in m_xml.LogClients)
                    {
                    // Add the system event (-1)
                        {
                        MaintenanceEntry e = new MaintenanceEntry(lc.Hw_id, lc.Hw_name);
                        //{
                        //    channel = -1,
                        //    active = false,
                        //    StartDate = utcnow,
                        //    EndDate = utcnow,
                        //    server_id = lc.Hw_id,
                        //    server_name = lc.Hw_name,
                        //    Desc = "",
                        //};
                        events.Add(e);
                        }

                    // Add all of the per-channel events
                    foreach (int channel in lc.channels)
                        {
                        MaintenanceEntry e = new MaintenanceEntry(lc.Hw_id, lc.Hw_name, channel);
                        //{
                        //    active = false,
                        //    channel = channel,
                        //    StartDate = utcnow,
                        //    EndDate = utcnow,
                        //    server_id = lc.Hw_id,
                        //    server_name = lc.Hw_name,
                        //    Desc = "",

                        //};
                        events.Add(e);
                        }
                    }
                // UpdateFileMaintenance();

                return events;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            return null;
            }

        // Call at regular intervals to poll servers
        void UpdateFileMaintenance()
            {
            if (Program.UseSNMP)                //CRR Issue #40 If using SNMP then ignore Maintenance
                return;
            try
                {
                List<MaintenanceEntry> events = new List<MaintenanceEntry>(); // Get latest events from all servers
                foreach (LogClient lc in m_xml.LogClients)
                    {
                    foreach (MaintenanceEntry e in lc.GetMaintenanceEvents(false))
                        {
                        events.Add(e);
                        }
                    }

                for (int i = 0; i < m_maintenance.Count; i++)
                    {
                    MaintenanceEntry e = events.Find(x => (x.server_id == m_maintenance[i].server_id) && (x.channel == m_maintenance[i].channel));
                    if (e == null) continue;


                    if (!m_maintenance[i].active.Equals(e.active))
                        m_maintenance[i].active = e.active;
                    if (m_maintenance[i].Desc != e.Desc)
                        m_maintenance[i].Desc = e.Desc;
                    if (!m_maintenance[i].StartDate.Equals(e.StartDate))
                        m_maintenance[i].StartDate = e.StartDate;
                    if (!m_maintenance[i].EndDate.Equals(e.EndDate))
                        m_maintenance[i].EndDate = e.EndDate;
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        void SaveFileMaintenance(List<MaintenanceEntry> events)
            {
            try
                {
                foreach (MaintenanceEntry e in m_maintenance)
                    {
                    MaintenanceEntry matched = events.Find(x => ((x.server_id == e.server_id) && (x.channel == e.channel)));
                    if (matched == null) // Clear entries
                        {
                        e.active = false;
                        e.StartDate = DateTime.MinValue;
                        e.EndDate = DateTime.MinValue;
                        e.Desc = null;
                        }
                    else
                        { // Otherwise update
                        e.active = matched.active;
                        e.Desc = matched.Desc;
                        e.StartDate = matched.StartDate;
                        e.EndDate = matched.EndDate;
                        }
                    }

                // Update all of the maintenance events
                foreach (LogClient lc in m_xml.LogClients)
                    {
                    // Update each server in turn
                    lc.SetMaintenanceEvents(m_maintenance);
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

#else // Load/save maintenance locally
        List<MaintenanceEntry> LoadFileMaintenance()
        {
            try
            {
                if (!File.Exists(maintenance_fname)) return new List<MaintenanceEntry>();

                List<MaintenanceEntry> events = new List<MaintenanceEntry>();
                XmlSerializer formatter = new XmlSerializer(events.GetType());
                using (FileStream fs = new FileStream(maintenance_fname, FileMode.Open))
                {
                    byte[] buf = new byte[fs.Length];
                    fs.Read(buf, 0, (int)fs.Length);
                    using (MemoryStream ms = new MemoryStream(buf))
                    {
                        return (List<MaintenanceEntry>)formatter.Deserialize(ms);
                    }
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(this, er.Message, "Error! Unable to load maintenance logs");
            }
            return new List<MaintenanceEntry>();
        }

        void SaveFileMaintenance(List<MaintenanceEntry> events)
        {
            try
            {
                string folder = Path.GetDirectoryName(maintenance_fname);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                using (FileStream fs = new FileStream(maintenance_fname, FileMode.Create))
                {
                    XmlSerializer formatter = new XmlSerializer(events.GetType());
                    formatter.Serialize(fs, events);
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(this, er.Message, "Error! Unable to save maintenance logs");
            }
        }
#endif

        bool user_ack = false;
        delegate void CallAcknowledged(object sender, EventArgs e);
        void lc_Acknowledged(object sender, EventArgs e)
            {
            try
                {


                if (this.InvokeRequired)
                    {
                    this.Invoke(new CallAcknowledged(lc_Acknowledged), sender, e);
                    }
                else
                    {
                    List<LogClient> lc = new List<LogClient>();
                    lc.Add((LogClient)sender);
                    // timer1_count = 0; // Wait up to 10 seconds
                    // RefreshTsp(lc, true);
                    timer3.Enabled = true; // Wait 3 seconds, and then perform an update
                    timer3.Tag = lc;

                    /*
                    if (!backgroundWorker1.IsBusy)
                    {
                        backgroundWorker1.RunWorkerAsync((Object)lc);
                    }
                    else
                    {
                        user_ack = true;
                    }
                     */
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        void RestoreDefaults()
            {
            SaveForm();
            Properties.Settings.Default.errorColour = Color.Red;
            Properties.Settings.Default.warnColour = Color.FromArgb(255, 128, 0);
            Properties.Settings.Default.infoColour = Color.FromArgb(0, 192, 0);
            Properties.Settings.Default.maintenanceColour = Color.Yellow;
            Properties.Settings.Default.expireColour = Color.Gray;
            Properties.Settings.Default.userColour = Color.Blue;
            Properties.Settings.Default.TimeoutAlarm = 90; // Update every 90 seconds
            Properties.Settings.Default.RefreshInterval = 30; // Update every 30 seconds
            Properties.Settings.Default.VisualAlarms = true;
            Properties.Settings.Default.AudibleAlarms = true;
            Properties.Settings.Default.Save();
            }

        void SaveForm()
            {
            if (this.WindowState != FormWindowState.Minimized) // Do not save when minimized
                {
                Properties.Settings.Default.FormSize = this.Size;
                Properties.Settings.Default.FormLocation = this.Location;
                Properties.Settings.Default.FullScreen = this.FullScreen;
                Properties.Settings.Default.Maximised = (this.WindowState == FormWindowState.Maximized);
                }
            }

        void button_Click(object sender, EventArgs e)
            {
            MouseEventArgs args = e as MouseEventArgs;
            Button button = sender as Button;
            contextMenuStrip2.Show(button, args.Location);
            }


        TspButton last_button = null;
        protected void tspButton_Click(object sender, EventArgs e)
            {
            TspButton button = sender as TspButton;
            MouseEventArgs ptr = e as MouseEventArgs;
            if (ptr == null) return; // No click

            last_button = button; // Update to show that the button has been updated..

            UpdateOverlay();
            Form3 frm = Form3.Instance;
            frm.ShowForm = true;
            // frm.id_src = m_status.items.Find(x => x.Sub_items.Exists(y => y.id == button.hw_id.id)).Id;

            }

        public delegate void UpdateOverlayConsumer();
        public void UpdateOverlay()
            {
            try
                {

                if (this.InvokeRequired)
                    {
                    this.Invoke(new UpdateOverlayConsumer(UpdateOverlay));
                    }
                else
                    {

                    if (last_button == null) return;
                    Form3 overlay = Form3.Instance;

                    // Database lookup
                    int hw_type = m_xml.Db.m_hw_list[last_button.hw_id.idx].hw_type;
                    string hw_location = m_xml.Db.m_hw_list[last_button.hw_id.idx].location;
                    string hw_name = m_xml.Db.m_hw_types[hw_type];
                    string db_name = m_xml.GetNodeName(last_button.hw_id.id);
                    string button_name = last_button.Text;
                    bool isRedundant = m_xml.Db.m_hw_list[last_button.hw_id.idx].isRedundant;

                    overlay.Text = last_button.Text;
                    overlay.textBox3.Text = m_xml.Db.m_hw_list[last_button.hw_id.idx].location;

                    // int hw_id = last_button.hw_id.id;
                    TspStatusItem sts = m_status.GetStatus(last_button.hw_id.id);
                    if ((sts.state == TspStatusItem.status.ok) || (sts.state == TspStatusItem.status.pending))
                        {
                        overlay.OverlayColour = Color.FromArgb(150, Properties.Settings.Default.infoColour);
                        }
                    else if (sts.state == TspStatusItem.status.warning)
                        {
                        overlay.OverlayColour = Color.FromArgb(150, Properties.Settings.Default.warnColour);
                        }
                    else if (sts.state == TspStatusItem.status.error)
                        {
                        overlay.OverlayColour = Color.FromArgb(150, Properties.Settings.Default.errorColour);
                        }
                    else // Unknown
                        {
                        overlay.OverlayColour = Color.Transparent;
                        }
                    if (db_name != hw_name)
                        {
                        overlay.textBox4.Text = String.Format("{0} ({1})", db_name, hw_name);
                        }
                    else
                        {
                        overlay.textBox4.Text = String.Format("{0}", db_name);
                        }
                    if ((sts.state == TspStatusItem.status.unknown) || (sts.state == TspStatusItem.status.pending))
                        {
                        if (sts.state == TspStatusItem.status.unknown)
                            {
                            overlay.Text += " [EXPIRED]";
                            overlay.toolStripStatusLabel2.Text = "Time out waiting for status update";
                            }
                        else
                            {
                            overlay.Text += " [INITIALISING]";
                            overlay.toolStripStatusLabel2.Text = "Waiting for status update..";
                            }
                        }
                    else
                        {
                        overlay.Text += string.Format(" [{0}]", sts.state.ToString().ToUpper());
                        }


                    // else
                        {
                        int count = sts.iCount + sts.wCount + sts.eCount;
                        if (count > 0)
                            {
                            overlay.Text += String.Format(" {0} Event{1}", count, (count > 1) ? "s" : "");
                            overlay.textBox7.Text = string.Format("{0} Error{1}; {2} Warning{3}, {4} Info{5}", sts.eCount, (sts.eCount > 1) ? "s" : "", sts.wCount, (sts.wCount > 1) ? "s" : "", sts.iCount, (sts.iCount > 1) ? "s" : "");
                            overlay.textBox7.Enabled = true;
                            }
                        else
                            {
                            overlay.Text += " No Events";
                            if (sts.state == TspStatusItem.status.pending)
                                {
                                overlay.textBox7.Text = "Pending";
                                }
                            else if (sts.state == TspStatusItem.status.unknown)
                                {
                                overlay.textBox7.Text = "Expired";
                                }
                            else
                                {
                                overlay.textBox7.Text = "No events";
                                }


                            TimeSpan since = (DateTime.Now - sts.lastUpdate); //CRR issue #18 Use just local time
                            if (since.TotalDays >= 1)
                                {
                                overlay.textBox7.Text += string.Format(" for {0} day{1}", (int)since.TotalDays, (since.TotalDays >= 2) ? "s" : "");
                                }
                            else if (since.TotalHours >= 1)
                                {
                                overlay.textBox7.Text += string.Format(" for {0} hour{1}", (int)since.TotalHours, (since.TotalHours >= 2) ? "s" : "");
                                }
                            else if (since.TotalMinutes >= 1)
                                {
                                overlay.textBox7.Text += string.Format(" for {0} minute{1}", (int)since.TotalMinutes, (since.TotalMinutes >= 2) ? "s" : "");
                                }
                            }

                        overlay.toolStripStatusLabel2.Text = sts.lastUpdate.ToString();
                        overlay.listBox1.Enabled = true;
                        }

                    overlay.UpdateListBox(sts.Id, sts.Sub_items);
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        bool Popups
            {
            set
                {
                Form3 overlay = Form3.Instance;
                if (overlay.ShowForm) overlay.Visible = value;
                if (web.ShowForm) web.Visible = value;
                }
            }

        protected void userButton_Click(object sender, EventArgs e)
            {
            try
                {
                List<string> errors = new List<string>();
                UserButton button = sender as UserButton;
                Tsp_hw_node tsp_node = button.tsp_node;
                if (tsp_node.prompt.Length > 0)
                    {
                    Popups = false;
                    bool cancel = (MessageBox.Show(this, "Press OK to accept", tsp_node.prompt, MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK);
                    Popups = true;

                    if (cancel)
                        {
                        return; // Cancelled
                        }
                    }
                List<User_action> actions = button.UserActions;
                foreach (User_action action in actions)
                    {
                    if (!action.Enabled) continue; // Skip actions that are not enabled

                    LogClient lc = new LogClient();
                    lc.Hw_id = -1;
                    lc.Ep = action.ep;
                    lc.Enabled = true;
                    string reply;
                    if (!lc.Execute(action.raw_sql, out reply))
                        {
                        errors.Add(lc.Last_err);
                        m_log.LogError(lc.Last_err);
                        }
                    }
                if (errors.Count > 0)
                    {
                    MessageBox.Show(this, string.Join("\r\n", errors), "Errors detected");
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
            {
            TspButton button = contextMenuStrip1.SourceControl as TspButton;
            contextMenuStrip1.Items.Clear();

            ToolStripLabel title = new ToolStripLabel(button.Text + " Actions");
            title.Font = new Font(Font, FontStyle.Bold | FontStyle.Underline);
            title.ForeColor = Properties.Settings.Default.userColour;
            contextMenuStrip1.Items.Add(title);

            foreach (WebPage_action menu_item in button.Actions)
                {
                if (menu_item.Enabled)
                    {
                    ToolStripItem item = contextMenuStrip1.Items.Add(menu_item.desc);
                    item.Tag = new Tuple<string, WebPage_action>(button.Text, menu_item);
                    }
                }
            e.Cancel = false;
            }

        Form2 web = Form2.Instance;
        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
            {
            // TspButton button = (TspButton)e.ClickedItem.Tag;
            Tuple<string, WebPage_action> clicked_item = (Tuple<string, WebPage_action>)e.ClickedItem.Tag;

            if (clicked_item != null)
                {
                web.Text = clicked_item.Item1 + ": " + clicked_item.Item2.desc;
                web.webBrowser1.Navigate(clicked_item.Item2.URL as string);
                web.ShowForm = true;
                }
            }

        private void tspStateChange(object sender, TspEventArgs e)
            {
            TspStatusItem sts = e.item;
            TspButton but = pictureBox1.Controls["_TSP_" + sts.Id.ToString()] as TspButton;
            if (but != null)
                {
                Debug.WriteLine("tspStateChange:" + but.Name + " " + e.item.state.ToString());
                but.Status = (TspButton.states)sts.state;
                }

            // If the user is currently watching this button, then update the overlay at the same time
            if (last_button == but)
                {
                UpdateOverlay();
                }
            }

        private void timer1_Tick(object sender, EventArgs e)
            {
            timer1.Enabled = false;
            Debug.WriteLine("Timer1 Started " + DateTime.Now.ToString("hh:mm:ss fff tt"));
            if (!backgroundWorker2.IsBusy)
                {
                backgroundWorker2.RunWorkerAsync();         // Run Do_RefreshTSP_InBackground()
                }
            else
                {
                Debug.WriteLine("backgroundWorker2.IsBusy " + DateTime.Now.ToString("hh:mm:ss fff tt"));
                }
            Debug.WriteLine("Timer1 Ended " + DateTime.Now.ToString("hh:mm:ss fff tt"));
            timer1.Enabled = true;
            if (Program.UseSNMP && Program.TSPConfiguration.ShowDebug)
                {
	            if (!SNMP_DebugMessage.Trim().Equals(""))
	                {
	                label_DebugMessage2.Text = SNMP_DebugMessage;
	                label_DebugMessage2.Visible = true;
                    label_NameAndRole.Text = "Role: " + Program.TSPConfiguration.UserRole + " User: " + Program.TSPConfiguration.V3Username;
	                }
	            }
            }


        private void Do_RefreshTSP_InBackground()
            {
            List<LogClient> job_list = null;
            if (timer1_count <= 0)
                {
                timer1_count = timer1.Interval * (int)(Properties.Settings.Default.RefreshInterval / timer1.Interval);
                job_list = m_xml.LogClients.AsList();
                }

            RefreshTsp(job_list);     //As this contacts the servers it can hang the GUI if the network is down unless it's off the GUI thread.

            // decrement the interval
            if (!backgroundWorker1.IsBusy)
                {
                timer1_count -= timer1.Interval;
                }
            // timer1.Enabled = false;

            }

        object user_job_list = null;
        int timer1_count = 0;
        private void RefreshTsp(List<LogClient> job_list, bool queue_req = false)
            {

            try                                                                     //CRR #13
                {
                Debug.WriteLine("RefreshTsp Started " + DateTime.Now.ToString("hh:mm:ss fff tt"));
                // GetActiveMaintenance list
                m_status.updateMaintenance(m_maintenance);

                if (job_list != null)
                    {
                    if (!backgroundWorker1.IsBusy)
                        {
                        backgroundWorker1.RunWorkerAsync(job_list);
                        }
                    else
                        {
                        Debug.WriteLine("!*!*!*!*! backgroundWorker1.IsBusy");
                        user_ack = queue_req;
                        user_job_list = job_list;
                        }
                    }

                // Check for timed out nodes
                m_status.checkWatchdog();

                // Update the overlay each click to keep it live
                // UpdateOverlay();
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("XXXX EXCEPTION in RefreshTSP " + Ex.Message);               //CRR #13
                Debug.WriteLine(Ex.StackTrace);                                         //CRR #13
                }
            Debug.WriteLine("RefreshTSP Ended " + DateTime.Now.ToString("hh:mm:ss fff tt"));
            }

        private void pictureBox1_Click(object sender, EventArgs e)
            {
            /*
            MouseEventArgs args = e as MouseEventArgs;
            if (args.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Form3 overlay = Form3.Instance;
                overlay.ShowForm = false;
                last_button = null;
            }
             */
            }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
            if (user_ack)
                {
                user_ack = false;
                backgroundWorker1.RunWorkerAsync(user_job_list); // Re-start full sweep everything
                }
            else
                {
                timer1.Enabled = true;

                // Update the overlay each click to keep it live
                //UpdateOverlay();
                }


            }


        #region "Get EventLog Data"
        // Get the eventlog data using up to 4 techniques
        // The hard coded variable COLLECTION_MODE currently defines the modes:
        // Mode = 1 - Get log data from log.db via logclient
        // Mode = 2 - Get log data from a file
        // Mode = 3 - Do both
        // Mode = 4 - Get Log data via SNMP ( Issue #40 )



        #region "SNMP Event Log collection"
        /// <summary>
        /// This region contains he code used to fetch the events from the NAS via snmp ( See Issue #40 )
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="ls_list"></param>

        public void DebugDisplayLogSummary(string Title, List<LogClient.LogSummary> ls_list)
            {
            Debug.WriteLine("==============================");
            Debug.WriteLine("======== Log Summary for " + Title + "  ============");
            Debug.WriteLine("channel,count,eventID,guid,hw_id,level,msg,sw_type,targetID,tstamp,LastUpdated ");
            foreach (LogClient.LogSummary record in ls_list)
                {
                string Row = "";
                Row += "\"" + record.channel.ToString() + "\",";
                Row += "\"" + record.count.ToString() + "\",";
                Row += "\"" + record.eventID.ToString() + "\",";
                Row += "\"" + record.guid.ToString() + "\",";
                Row += "\"" + record.hw_id.ToString() + "\",";
                Row += "\"" + record.level.ToString() + "\",";
                Row += "\"" + record.msg + "\",";
                Row += "\"" + record.sw_type.ToString() + "\",";
                Row += "\"" + record.targetID.ToString() + "\",";
                Row += "\"" + record.tstamp.ToString() + "\"";
                Row += "\"" + record.LastUpdated.ToString() + "\"";
                Debug.WriteLine(Row);
                }
            Debug.WriteLine("==============================");
            Debug.WriteLine("==============================");
            }

        public string SNMP_DebugMessage = "";
        public List<LogClient.LogSummary> GetEventLogViaSNMP(LogClient lc)
            {
            List<LogClient.LogSummary> ls_list = new List<LogClient.LogSummary>();
            try
                {
                //InitialiseSNMPEventCollection();

                //GIVE EACH LC it's own ' SNMPMANAGER
                //    DO a conditional init
                //    keep them seperate.

                //CRR #48 - Get the user password we should be using
                lc.SNMPManager.SNMPV3_UserName = Program.TSPConfiguration.V3Username;
                lc.SNMPManager.SNMPV3_AuthPassword = Program.TSPConfiguration.V3AuthPassword;
                lc.SNMPManager.SNMPV3_PrivacyPassword = Program.TSPConfiguration.V3PrivPassword;

                lc.SNMPManager.LoadMibsIfNotAllReadyLoaded();
                lc.SNMPManager.IPAddress = lc.Ep.Address.ToString();            //TODO A Cheat - get into config
                lc.SNMPManager.Port = 161;                                      //TODO A Cheat get into config
                lc.SNMPManager.UseSNMPV3 = Program.TSPConfiguration.UseSNMPV3;  //TODO A Cheat get into config
                Debug.WriteLine("Getting the EventLog via SNMP for " + lc.SNMPManager.IPAddress + ":" + lc.SNMPManager.Port.ToString());
                //@"SELECT el.hw_id, el.level, count(*) as count, el.tstamp, el.channel, el.eventID, el.id, el.desc FROM eventLog as el
                if (lc.SNMPManager.RequestEventTable())
                    {
                    foreach (Dart.Snmp.Variable EventRecord in lc.SNMPManager.LastResponse.Variables)
                        {
                        string EventRecordAsString = EventRecord.Value.ToString();
                        List<string> fields = lc.SNMPManager.ParseCSV(EventRecordAsString);
                        LogClient.LogSummary LogSummary_Record;
                        if (!int.TryParse(fields[4], out LogSummary_Record.channel))
                            LogSummary_Record.channel = -1;
                        LogSummary_Record.count = int.Parse(fields[2]);
                        LogSummary_Record.eventID = int.Parse(fields[5]);
                        LogSummary_Record.guid = int.Parse(fields[6]);
                        LogSummary_Record.hw_id = int.Parse(fields[0]);
                        LogSummary_Record.level = int.Parse(fields[1]);
                        LogSummary_Record.msg = fields[7];
                        LogSummary_Record.sw_type = 0; //TODO Convert.ToInt32(fields[7]);
                        LogSummary_Record.targetID = LogSummary_Record.hw_id;

                        fields[3] = fields[3].Replace("T", " ");
                        fields[3] = fields[3].Replace("Z", "");

                        DateTime TimeStamp = DateTime.ParseExact(fields[3], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        LogSummary_Record.tstamp = TimeStamp;
                        LogSummary_Record.maintenance = int.Parse(fields[8]);       // #48
                        // Extra fields - if missing just carry on
                        try
                            {
                            DateTime LastUpdated = DateTime.ParseExact(fields[9], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            LogSummary_Record.LastUpdated = LastUpdated;
                            }
                        catch(Exception Ex)
                            {
                            LogSummary_Record.LastUpdated = DateTime.Now;
                            }
                        //Debug.WriteLine(fields[0] + "," + fields[1] + "," + fields[2] + "," + fields[3] + "," + fields[4] + "," + fields[5] + "," + fields[6] + "," + fields[7] + "," + fields[8] + "," + fields[9]);
                        if (LogSummary_Record.maintenance == 0)
                            {
                            ls_list.Add(LogSummary_Record);             //CRR # 48 Do not add if in Maintenance mode
                            }
                        }
                    lc.SNMPManager.LogEventsToDisk(lc.Hw_name);
                    SNMP_DebugMessage += " ' " + ls_list.Count.ToString() + " Events collected from " + lc.SNMPManager.IPAddress + ":" + lc.SNMPManager.Port + " via SNMP at " + DateTime.Now.ToString();
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("Exception in GetEventLogViaSNMP : " + Ex.ToString());
                ls_list = new List<LogClient.LogSummary>();
                }
            return ls_list;
            }

        #endregion "SNMP Event Log collection"

        /// <summary>
        /// Live and experimental code to fetch the data from the NAS(s) in a variety of ways 
        /// </summary>
        /// <remarks>
        /// The COLLECTION_MODE variable is used to define the mode
        /// It defaults to 1 ( i.e direct to the logClient )
        /// It is set to 4 ( snmp ) if -SNMP is defined on the command line
        /// Mode = 1 - Get event log data from log.db via logclient
        /// Mode = 2 - Get event log data from a file
        /// Mode = 3 - Do both ( For testing )
        /// Mode = 4 - Get event log data via SNMP
        /// Mode = 5 - Get evnet oog data via SNMP AND via log Client ( For Testing by comparison )
        /// </remarks>

        #region "Collect the Event Log data from the NAS"

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
            {
            try
                {
                List<LogClient> LocalLogClientList = new List<LogClient>();
                // List<LogClient.LogSummary> LocalLogClientSummaryList = new List<LogClient.LogSummary>();

                int COLLECTION_MODE = 1;            // Mode = 1 - Get log data from log.db via logclient
                                                    // Mode = 2 - Get log data from a file
                                                    // Mode = 3 - Do both ( For testing )
                                                    // Mode = 4 - Get Log data via SNMP
                                                    // Mode = 5 - Get Log data via SNMP AND via log Client ( For Testing )
                if (Program.UseSNMP)
                    {
                    COLLECTION_MODE = 4;
                    SNMP_DebugMessage = "";
                    }
                // COLLECTION_MODE = 5;            //TODO Temp
                string CSVFileName = "";

                Do_RefreshTSP_InBackground();         // Do Refresh TSP in the background

                List<LogClient> job_list = (List<LogClient>)e.Argument;
                //Debug.WriteLine("");
                Debug.WriteLine("++++ backgroundWorker1_DoWork " + DateTime.Now.ToString("h: mm:ss fff tt"));
                // Contact each server in turn
                foreach (LogClient lc in job_list)
                    {
                    List<LogClient.LogSummary> ls_list = new List<LogClient.LogSummary>();

                    if (!lc.Enabled) continue; // Skip nodes that are not enabled

                    CSVFileName = "c:\\shared\\eventlog_for_" + lc.Hw_name + ".csv";

                    if ((COLLECTION_MODE == 4) || (COLLECTION_MODE == 5))                          // SNMP Collection mode
                        {
                        Debug.WriteLine("Collection Mode 4 - Via SNMP");
                        ls_list.Clear();
                        ls_list = GetEventLogViaSNMP(lc);
                        DebugDisplayLogSummary("SNMP via " + lc.Ep.Address.ToString(), ls_list);

                        }

                    if ((COLLECTION_MODE == 1) || (COLLECTION_MODE == 3) || (COLLECTION_MODE == 5)) //Get data from LogClients and copy to a csv file
                        {
                        Debug.WriteLine("Collection Mode 1 or 3 Both via logclient and a file");
                        if (!lc.Connected)          //CRR #9 - Try to reconnect
                            {
                            if (!TryToReconnectLogClient(lc))
                                {
                                continue;
                                }
                            }
                        DateTime now = DateTime.Now; //CRR issue #18 Use just local time
                                                     // if (m_maintenance.Exists(x => (x.server_id == lc.Hw_id) && (x.channel == -1) && (x.active) && (x.StartDate < now) && (x.EndDate > now))) continue; // Skip nodes that are in maintenace

                        //List<LogClient.LogSummary> ls_list;
                        if (!lc.GetLogSummary(out ls_list))
                            {
                            m_log.LogError(lc.Last_err);
                            continue;
                            }


                        // CRR Issue #34 - Show list of records collected from the eventLog

                        DebugDisplayLogSummary("LOGCLIENT via " + lc.Ep.Address.ToString(), ls_list);

                        //Debug.WriteLine("========Getting Event Data for " + lc.Hw_name + " from LogClient ============");
                        //Debug.WriteLine("channel,count,eventID,guid,hw_id,level,msg,sw_type,targetID,tstamp ");

                        if (false) //TODO - Test for Mike  Write to file if specified - Should we remove it ?
                            {
                            try
                                {
                        string FileContents = "";
                        foreach (LogClient.LogSummary record in ls_list)
                            {
                            string Row = "";
                            Row += "\"" + record.channel.ToString() + "\",";
                            Row += "\"" + record.count.ToString() + "\",";
                            Row += "\"" + record.eventID.ToString() + "\",";
                            Row += "\"" + record.guid.ToString() + "\",";
                            Row += "\"" + record.hw_id.ToString() + "\",";
                            Row += "\"" + record.level.ToString() + "\",";
                            Row += "\"" + record.msg + "\",";
                            Row += "\"" + record.sw_type.ToString() + "\",";
                            Row += "\"" + record.targetID.ToString() + "\",";
                            Row += "\"" + record.tstamp.ToString() + "\"";
                            //Debug.WriteLine(Row);
                            FileContents += Row + "\r\n";
                            }
                        //Debug.WriteLine("==============================");
                        File.WriteAllText(CSVFileName, FileContents);
                        }
                            catch(Exception Ex)
                                {
                                Debug.WriteLine("Failed to write to " + CSVFileName + " " + Ex.ToString());
                                }
                            }
                        }


                    // CRR Issue #35 - Pickup and use the list of records in a file collected from the eventLog

                    if ((COLLECTION_MODE == 2) || (COLLECTION_MODE == 3)) //Get data from LogClients and copy to a csv file
                        {
                        Debug.WriteLine("Collection Mode 2 or 3 Both via logclient and a file");
                        Debug.WriteLine("========Getting Event Data for " + lc.Hw_name + " from File " + CSVFileName + " ============");
                        Debug.WriteLine("channel,count,eventID,guid,hw_id,level,msg,sw_type,targetID,tstamp ");
                        List<LogClient.LogSummary> ls_list_from_file = new List<LogClient.LogSummary>();
                        try
                            {
                            if (File.Exists(CSVFileName))
                                {
                                using (TextFieldParser parser = new TextFieldParser(CSVFileName))
                                    {
                                    parser.TextFieldType = FieldType.Delimited;
                                    parser.SetDelimiters(",");
                                    while (!parser.EndOfData)
                                        {
                                        //Processing row
                                        string[] fields = parser.ReadFields();
                                        LogClient.LogSummary LogSummary_Record = new LogClient.LogSummary();
                                        LogSummary_Record.channel = Convert.ToInt32(fields[0]);
                                        LogSummary_Record.count = Convert.ToInt32(fields[1]);
                                        LogSummary_Record.eventID = Convert.ToInt32(fields[2]);
                                        LogSummary_Record.guid = Convert.ToInt32(fields[3]);
                                        LogSummary_Record.hw_id = Convert.ToInt32(fields[4]);
                                        LogSummary_Record.level = Convert.ToInt32(fields[5]);
                                        LogSummary_Record.msg = fields[6];
                                        LogSummary_Record.sw_type = Convert.ToInt32(fields[7]);
                                        LogSummary_Record.targetID = Convert.ToInt32(fields[8]);
                                        LogSummary_Record.tstamp = DateTime.ParseExact(fields[9], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        LogSummary_Record.LastUpdated = DateTime.Now;
                                        Debug.WriteLine(fields[0] + "," + fields[1] + "," + fields[2] + "," + fields[3] + "," + fields[4] + "," + fields[5] + "," + fields[6] + "," + fields[7] + "," + fields[8] + "," + fields[9]);
                                        ls_list_from_file.Add(LogSummary_Record);
                                        }
                                    }
                                }
                            }
                        catch (Exception Ex)
                            {
                            Debug.WriteLine("Failed to parse " + CSVFileName + " " + Ex.ToString());
                            }
                        Debug.WriteLine("==============================");
                        ls_list.Clear();                // Clear the list so that we get the data from the file
                        ls_list = ls_list_from_file;
                        }


                    lc.LatestEvents = ls_list.ToList<LogClient.LogSummary>();

                    List<string> warnings;
                    if (lc.CleanUpSummary(ref ls_list, out warnings)) // Clean up the ls_list to help remove invalid nodes
                        {
                        foreach (string warning in warnings)
                            {
                            m_log.LogWarn(warning);
                            }
                        }

                    foreach (LogClient.LogSummary item in lc.base_array)
                        {
                        LogClient.LogSummary ls;
                        int idx = ls_list.FindIndex(x => ((x.hw_id == item.hw_id) && (x.sw_type == item.sw_type) && (x.level == item.level) && (x.channel == item.channel)));
                        if (idx >= 0)
                            {
                            ls = ls_list[idx];
                            }
                        else
                            {
                            idx = ls_list.FindIndex(x => ((x.hw_id == item.hw_id) && (x.level == item.level) && (x.channel == item.channel))); // All except sw type
                            if (idx >= 0)
                                {
                                ls = ls_list[idx];
                                }
                            else
                                {
                                ls = item; // Default
                                }
                            }
                        if (!m_status.Update(lc.Hw_id, ls)) // m_status.Update(lc.Hw_id, ls))
                            {
                            m_log.LogError(m_status.last_err);
                            }
                        if (idx != -1)
                            {
                            ls_list.RemoveAt(idx); // Slowly clear the list
                            }
                        }

                    foreach (var item in ls_list)
                        {
                        LogClient.LogSummary ls = item;
                        m_status.Update(lc.Hw_id, ls);
                        m_log.LogWarn(string.Format("Unexpected event for node {0} found", ls.targetID));
                        }
                    LocalLogClientList.Add(lc);

                   //TODO Do wee need this ??? Thread.Sleep(6000);
                    //break;  //TODO Just once
                    }
                Program.LogClientsList = LocalLogClientList;        //CRR issue #40 Update a shared copy of the collected events
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("XXXX EXCEPTION in backgroundWorker1_DoWork " + Ex.Message); //CRR #13
                Debug.WriteLine(Ex.StackTrace);                                         //CRR #13
                }


            Debug.WriteLine("++++ backgroundWorker1_DoWork END" + DateTime.Now.ToString("h: mm:ss fff tt"));
            //Debug.WriteLine("");
            }

        #endregion

        #endregion

        /*
         * Name             TryToReconnectLogClient
         * Description      Given a Logclient - try to reconnect to it and if so set the connected flag to true
         * Returns          Trueif reconnected, False if not
         * Notes            Could return void.
         */
        private bool TryToReconnectLogClient(LogClient lc)
            {
            try
                {
                Debug.WriteLine(">>>> Try to reconnect to " + lc.Ep.Address);
                TcpClient tcp = new TcpClient();
                tcp.Connect(lc.Ep);
                lc.Connected = true;
                Debug.Write(">>>> RECONNECTED OK to " + lc.Ep.Address);
                tcp.Close();
                return true;
                }
            catch (Exception Ex)
                {
                
                Debug.Write(">>>>  RECONNECTED to " + lc.Ep.Address + " FAILED " + Ex.Message);
                }
            return false;
            }



        FormWindowState last_state;
        bool fullScreen = false;
        bool FullScreen
            {
            set
                {
                fullScreen = value;
                if (fullScreen)
                    {
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    last_state = WindowState;
                    if (WindowState == FormWindowState.Maximized) WindowState = FormWindowState.Normal;
                    WindowState = FormWindowState.Maximized;
                    }
                else // revert to last state
                    {
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                    WindowState = last_state;
                    }
                }
            get
                {
                return fullScreen;
                }
            }
        void ToggleFullScreen()
            {
            FullScreen = !FullScreen;
            }
        private void toggleFullScreenToolStripMenuItem_Click(object sender, EventArgs e)
            {
            ToggleFullScreen();
            }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
            {
            if (e.KeyCode == Keys.F10)
                {
                ToggleFullScreen();
                }
            else if (e.KeyCode == Keys.Escape)
                {
                FullScreen = false;
                }
            }

        private void quitApplicationToolStripMenuItem_Click(object sender, EventArgs e)
            {
            this.Close();
            }

        private void viewTSPLogsToolStripMenuItem_Click(object sender, EventArgs e)
            {
            Popups = false;
            Form5 frm5 = new Form5();
            frm5.ShowDialog();
            Popups = true;
            }





        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
            {
            Popups = false;
            while (true)
                {
                Form4 frm4 = new Form4();
                frm4.numericUpDown1.Value = Properties.Settings.Default.TimeoutAlarm;
                frm4.numericUpDown2.Value = Properties.Settings.Default.RefreshInterval;
                frm4.textBox1.Text = ColorTranslator.ToHtml(Properties.Settings.Default.infoColour);
                frm4.textBox2.Text = ColorTranslator.ToHtml(Properties.Settings.Default.warnColour);
                frm4.textBox3.Text = ColorTranslator.ToHtml(Properties.Settings.Default.errorColour);
                frm4.textBox7.Text = ColorTranslator.ToHtml(Properties.Settings.Default.expireColour);
                frm4.textBox6.Text = ColorTranslator.ToHtml(Properties.Settings.Default.maintenanceColour);
                frm4.textBox5.Text = ColorTranslator.ToHtml(Properties.Settings.Default.userColour);
                frm4.checkBox1.Checked = Properties.Settings.Default.AudibleAlarms;
                frm4.checkBox2.Checked = Properties.Settings.Default.VisualAlarms;
                UpdateFileMaintenance();// Update maintenance events
                frm4.MaintenanceEvents = m_maintenance.ConvertAll(x => new MaintenanceEntry(x));
                DialogResult result = frm4.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.Retry)
                    {
                    RestoreDefaults();
                    continue;
                    }
                if (result == System.Windows.Forms.DialogResult.OK)
                    {
                    Properties.Settings.Default.TimeoutAlarm = (int)frm4.numericUpDown1.Value;
                    Properties.Settings.Default.RefreshInterval = (int)frm4.numericUpDown2.Value;
                    Properties.Settings.Default.infoColour = ColorTranslator.FromHtml(frm4.textBox1.Text);
                    Properties.Settings.Default.warnColour = ColorTranslator.FromHtml(frm4.textBox2.Text);
                    Properties.Settings.Default.errorColour = ColorTranslator.FromHtml(frm4.textBox3.Text);
                    Properties.Settings.Default.expireColour = ColorTranslator.FromHtml(frm4.textBox7.Text);
                    Properties.Settings.Default.maintenanceColour = ColorTranslator.FromHtml(frm4.textBox6.Text);
                    Properties.Settings.Default.userColour = ColorTranslator.FromHtml(frm4.textBox5.Text);
                    Properties.Settings.Default.AudibleAlarms = frm4.checkBox1.Checked;
                    Properties.Settings.Default.VisualAlarms = frm4.checkBox2.Checked;
                    Properties.Settings.Default.Save();

                    if (frm4.MaintenanceChanged)
                        {
                        SaveFileMaintenance(frm4.MaintenanceEvents);
                        //m_maintenance = frm4.MaintenanceEvents.ConvertAll(x => new MaintenanceEntry(x));
                        m_status.updateMaintenance(m_maintenance);
                        }

                    // Update the overlay form
                    Form3 overlay = Form3.Instance;
                    overlay.Refresh();

                    foreach (Control ctrl in pictureBox1.Controls)
                        {

                        if (ctrl is TspButton)
                            {
                            ((TspButton)ctrl).Status = ((TspButton)ctrl).Status;
                            }
                        else if (ctrl is UserButton)
                            {
                            ctrl.BackColor = Properties.Settings.Default.userColour;
                            }
                        }

                    }
                break;
                }
            Popups = true;
            }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
            {
            // We are closing form, so save the defaults
            SaveForm(); // Store last position, etc
            Properties.Settings.Default.Save();
            }

        private void Form1_Shown(object sender, EventArgs e)
            {
            /// Load settings from config
            if (Properties.Settings.Default.FormSize.Height == 0)
                {
                RestoreDefaults();
                }
            timer1.Interval = 1000 * 10;
            Debug.WriteLine("Timer1 set to " + timer1.Interval.ToString() + " " + DateTime.Now.ToString("h:mm:ss.fff tt"));
            this.Location = Properties.Settings.Default.FormLocation;
            this.Size = Properties.Settings.Default.FormSize;
            if (Properties.Settings.Default.Maximised)
                {
                this.WindowState = FormWindowState.Maximized;
                }
            if (Properties.Settings.Default.FullScreen)
                {
                FullScreen = true;
                }

            //Properties.Settings.Default.maintenanceColour = IncreaseWhiteContrast(Properties.Settings.Default.maintenanceColour);
            //Properties.Settings.Default.userColour = IncreaseWhiteContrast(Properties.Settings.Default.userColour);
            //Properties.Settings.Default.infoColour = IncreaseWhiteContrast(Properties.Settings.Default.infoColour);
            //Properties.Settings.Default.warnColour = IncreaseWhiteContrast(Properties.Settings.Default.warnColour);
            //Properties.Settings.Default.errorColour = IncreaseWhiteContrast(Properties.Settings.Default.errorColour);
            }

        private FormWindowState lastState; //  = WindowState; // FormWindowState.Normal;
        private void Form1_Resize(object sender, EventArgs e)
            {
            // Are we being minimized?
            if (lastState != WindowState)
                {
                if (WindowState == FormWindowState.Minimized)
                    {
                    Popups = false;
                    }
                else
                    {
                    Popups = true;
                    }
                lastState = WindowState;
                }

            if (this.ClientSize.Height > pictureBox1.Image.Size.Height)
                {
                this.pictureBox1.Top = (this.ClientSize.Height - pictureBox1.Image.Size.Height) / 2;
                }
            if (this.ClientSize.Width > pictureBox1.Image.Size.Width)
                {
                this.pictureBox1.Left = (this.ClientSize.Width - pictureBox1.Image.Size.Width) / 2;
                }
            }

        private void allErrorsToolStripMenuItem_Click(object sender, EventArgs e)
            {
            foreach (LogClient lc in m_xml.LogClients)
                {
                lc.AcknowledgeByLevel((int)LogMessage.LogMessageTypes.Error);
                }
            }

        private void allWarningsToolStripMenuItem_Click(object sender, EventArgs e)
            {
            foreach (LogClient lc in m_xml.LogClients)
                {
                lc.AcknowledgeByLevel((int)LogMessage.LogMessageTypes.Warning);
                }
            }

        private void allInfoToolStripMenuItem_Click(object sender, EventArgs e)
            {
            foreach (LogClient lc in m_xml.LogClients)
                {
                lc.AcknowledgeByLevel((int)LogMessage.LogMessageTypes.Info);
                }
            }

        int timer2_count = 0;
        bool timer2_flash = false;
        SoundPlayer player;
        private void timer2_Tick(object sender, EventArgs e)
            {
            int counter = 0;
            try
                {
                //Debug.WriteLine("Timer2 - Audible Stuff");
                if (Properties.Settings.Default.VisualAlarms)
                    {
                    timer2_flash = !timer2_flash;
                    }
                else
                    {
                    timer2_flash = false;
                    }
                foreach (Control c in pictureBox1.Controls)
                    {
                    TspButton ub = c as TspButton;
                    if (ub == null) continue; // Not a tsp button
                                              // TechnicalSupervisor.TspStatusItem item = m_status.items.Find(x => x.Id == ub.hw_id.id);
                    if (ub.Alarm)
                        {
                        counter++;
                        ub.Highlight = timer2_flash;
                        }
                    else
                        {
                        ub.Highlight = false;
                        }
                    }
                if ((timer2_count == 0))
                    {
                    timer2_count = 6;
                    if (Properties.Settings.Default.AudibleAlarms && (counter > 0))
                        {
                        player.Play();
                        }
                    }
                timer2_count--;
                if (counter == 0)
                    {
                    timer2_flash = false;
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void alarmToolStripMenuItem_Click(object sender, EventArgs e)
            {
            // Ack all alarms...
            foreach (Control c in pictureBox1.Controls)
                {
                TspButton ub = c as TspButton;
                if (ub == null) continue; // Not a tsp button
                if (ub.Alarm)
                    {
                    ub.Alarm = false;
                    }
                }
            }

        private void timer3_Tick(object sender, EventArgs e)
            {
            Debug.WriteLine("Timer3 " + DateTime.Now.ToString("h:mm:ss fff tt"));
            List<LogClient> lc = (List<LogClient>)timer3.Tag;
            RefreshTsp(lc, true);
            timer3.Enabled = false; //CRR #13 Allow to run once only 
            }

        private void Panel1_Paint(object sender, PaintEventArgs e)
            {

            }


        private void BackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
            {
            Debug.WriteLine("++++++++Worker2 started");
            Do_RefreshTSP_InBackground();
            Debug.WriteLine("++++++++Worker2 finished");
            }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
            Debug.WriteLine("++++++++Worker2 Completed **********************************");
            }

        private void Form1_Load(object sender, EventArgs e)
            {
            if (Program.UseSNMP && Program.TSPConfiguration.ShowDebug)
                {
                groupBox_Debug.Visible = true;                                  //TODO Just for SNMP DEBUG
                groupBox_Debug.BringToFront();
                label_DebugMessage.Text = "TSP (SNMP) V" + Application.ProductVersion;
                label_CurrentUser.Text = "Current User: " + Program.TSPConfiguration.V3Username;
                label_DebugMessage.Visible = true;
                label_CurrentUser.Visible = true;
                label_NameAndRole.Visible = true;
                button_LoginDialog.Visible = true;
                button_ViewSnmpEvents.Visible = true;
                button_Acknowledge.Visible = true;
                }
            else
                {
                groupBox_Debug.Visible = false;                                  //TODO Just for SNMP DEBUG
                label_DebugMessage.Visible = false;
                label_CurrentUser.Visible = false;
                button_LoginDialog.Visible = false;
                button_ViewSnmpEvents.Visible = false;
                button_Acknowledge.Visible = false;
                label_NameAndRole.Visible = false;
                }
            }

        private void button1_Click(object sender, EventArgs e)
            {
            FormViewEvents F = new FormViewEvents();
            F.Show();
            }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
            {

            }

        public void ShowLoginDialog()
            {
            FormLogin F = new FormLogin();
            //this.Visible = false;
            this.Enabled = false;
            F.AttachedLogClients = m_xml.LogClients;
            F.StartPosition = FormStartPosition.CenterParent;
            F.m_log = this.m_log;
            F.ShowDialog();
            try
                {
                if (F.ExitApplication == false)
                    {
                    this.Visible = true;
                    string Role = "";
                    switch(Program.TSPConfiguration.UserRole)
                        {
                        case 0: Role = "Invalid"; break;
                        case 1: Role = "User"; break;
                        case 2: Role = "Super User"; break;
                        case 3: Role = "Administrator"; break;
                        case 4: Role = "Developer"; break;
                        default: Role = ""; break;
                        }
                    this.label_CurrentUser.Text = "Current User : " + Program.TSPConfiguration.V3Username + " (" + Role + ")";
                    }
                else
                    {
                    this.Enabled = true;
                    Application.Exit();
                    }
                }
            catch (Exception Ex)
                {
                // The application is probably closing down.
                }
            this.DisplayTitleAndUser();
            this.Enabled = true;
            }
        private void button_LoginDialog_Click(object sender, EventArgs e)
            {
            ShowLoginDialog();
            }

        private void button_Acknowledge_Click(object sender, EventArgs e)
            {
            FormAcknowledge F = new FormAcknowledge();
            F.m_log = this.m_log;
            F.AttachedLogClients = m_xml.LogClients;
            F.ShowDialog();
            }

        private void  DisplayTitleAndUser()
            {
            if(Program.TSPConfiguration.UseSNMP)
                {
                this.Text = m_xml.Name + " (User: " + Program.TSPConfiguration.V3Username + ")";
                }
            else
                {
                this.Text = m_xml.Name;
                }
            
            }

        //CRR #50
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
            {
            if(Program.UseSNMP)
                {
                FormAcknowledge F = new FormAcknowledge();
                F.m_log = this.m_log;
                F.AttachedLogClients = m_xml.LogClients;
                F.ShowDialog();
                }
            }

        private void toolStripMenuItem_ChangeUser_Click(object sender, EventArgs e)
            {
            this.ShowLoginDialog();
            }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
            {

            }
        }

    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, TspEventArgs e);


    public class TspStatusSubItem
        {
        public struct log_event
            {
            public int guid; // Unique ID for this event
            public string msg;
            public int event_id; // Event ID (-1 if N/A)
            public int target_id; // Target ID (-1 if N/A)
            public int channel; // Channel number (-1 if N/A)
            public DateTime tstamp;
            }
        public int id
            {
            get;
            set;
            }
        public int id_src // Source is important if main/backup is used
            {
            get;
            set;
            }

        public int channel;
        public int isBackup;

        public TspStatusSubItem(TspStatusSubItem clone)
            {
            eCount = clone.eCount;
            wCount = clone.wCount;
            iCount = clone.iCount;
            edesc = clone.edesc;
            wdesc = clone.wdesc;
            idesc = clone.idesc;
            lastUpdate = clone.lastUpdate;
            isBackup = clone.isBackup;
            id = clone.id;
            id_src = clone.id_src;
            channel = clone.channel;
            }

        public TspStatusSubItem(int node_id, int src, int backup = -1, int ch = -1)
            {
            id = node_id; // Hardware ID for this item
            isBackup = backup;
            channel = ch;
            if (src == -1) src = node_id;
            id_src = src; // Source is the server that provides updates for this sub-item
            }

        public enum status
            {
            maintenance = -3,
            pending = -2,
            unknown = -1,
            ok,
            warning,
            error,
            }

        public DateTime lastUpdate = DateTime.Now; //CRR issue #18 Use just local time
        public int iCount = 0;
        public int wCount = 0;
        public int eCount = 0;
        public status state
            {
            get
                {
                if (Maintenance) return status.maintenance;
                if (TimeOut) return status.unknown;
                if (Pending) return status.pending;
                if (eCount > 0) return status.error;
                if (wCount > 0) return status.warning;
                return status.ok;
                }
            }

        public log_event idesc;
        public log_event wdesc;
        public log_event edesc;
        public log_event desc
            {
            get
                {
                if (eCount > 0) return edesc;
                else if (wCount > 0) return wdesc;
                return idesc;
                }
            }
        public bool TimeOut = false;
        public bool Pending = true;
        public bool Maintenance = false; // Is node in maintenance
        }

    public class TspStatusItem //  : TspStatusSubItem
        {
        public TspStatusItem(int hw_id, int backup, List<int> ch_list)
            {
            id = hw_id;
            isBackup = backup;
            channels = ch_list;
            Sub_items = new List<TspStatusSubItem>();
            }

        public List<TspStatusSubItem> Sub_items;
        public List<int> channels;
        public int isBackup;

        int id;
        public int Id
            {
            get { return id; }
            }

        public enum status
            {
            maintenance = -3,
            pending = -2,
            unknown = -1,
            ok,
            warning,
            error
            }

        public DateTime lastUpdate = DateTime.Now; //CRR issue #18 Use just local time
        public int iCount
            {
            get
                {
                int count = 0;
                foreach (var item in Sub_items)
                    {
                    count += item.iCount;
                    }
                return count;
                }
            }
        public int wCount
            {
            get
                {
                int count = 0;
                foreach (var item in Sub_items)
                    {
                    count += item.wCount;
                    }
                return count;
                }
            }
        public int eCount
            {
            get
                {
                int count = 0;
                foreach (var item in Sub_items)
                    {
                    count += item.eCount;
                    }
                return count;
                }
            }
        public status state
            {
            get
                {
                if (Maintenance) return status.maintenance;
                if (TimeOut) return status.unknown;
                if (Pending) return status.pending;
                if (eCount > 0) return status.error;
                if (wCount > 0) return status.warning;
                return status.ok;
                }
            }

        public TspStatusSubItem priority
            {
            get
                {
                int last_state = -3;
                DateTime newest_event = new DateTime(); // Epoch
                // string newest_msg = "";
                TspStatusSubItem newest_item = null;
                foreach (var item in Sub_items)
                    {
                    if ((int)item.state > last_state)
                        {
                        last_state = (int)item.state;
                        newest_event = item.lastUpdate;
                        newest_item = item;
                        }
                    else if (((int)item.state == last_state) && (item.lastUpdate > newest_event))
                        {
                        newest_event = item.lastUpdate;
                        newest_item = item;
                        }
                    }
                return newest_item;
                }
            }

        public TspStatusSubItem.log_event desc
            {
            get
                {
                TspStatusSubItem item = priority;
                if (item != null)
                    {
                    return item.desc;
                    }
                return new TspStatusSubItem.log_event();
                }
            }

        public bool TimeOut = false;        //#50 = Default to TimeOut=True to show grey ? - Did not work
        public bool Pending = true;
        public bool Maintenance = false;    // Is node in maintenance
        }


    public class TspEventArgs : EventArgs
        {
        public TspStatusItem item { get; set; }
        }

    public class TspStatusList
        {
        public TspStatusList()
            {
            items = new List<TspStatusItem>();
            }
        public List<TspStatusItem> items;

        public int Length
            {
            get
                {
                return items.Count;
                }
            }

        // An event that clients can use to be notified whenever the
        // elements of the list change
        public event ChangedEventHandler Changed;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnChanged(TspEventArgs e)
            {
            if (Changed != null)
                Changed(this, e);
            }

        /*
        public void SetMaintenance(int hw_id, int channel = -1, bool enable = false)
        {
            TspStatusItem item = items.Find(x => x.Id == hw_id);
            if (channel < 0)
            {
                item.Maintenance = enable;
                foreach (TspStatusSubItem sub_item in item.Sub_items)
                {
                    sub_item.Maintenance = enable;
                }
            }
            else
            {
                item.Maintenance = false; // Remove top-level constraint (if set)
                foreach (TspStatusSubItem sub_item in item.Sub_items.FindAll(x => x.channel == channel))
                {
                    sub_item.Maintenance = enable;
                }
            }
        }
        */

        public string last_err = "";
        public bool Update(int server_id, LogClient.LogSummary ls, bool accumulate = false)
            {
            try
                {
                var xml = XmlDatabaseInterface.Instance;
                // SqliteDatabaseInterface.Sw_id sw_id = xml.LookupNodeId(ls.targetID, ls.sw_type);

                int idx = items.FindIndex(x => x.Id == ls.hw_id);
                if (idx < 0)
                    {
                    last_err = String.Format("Unable to find Hardware ID {0} - please check config database", ls.hw_id);
                    return false;
                    }

                int sub_idx = items[idx].Sub_items.FindIndex(x => (x.channel == ls.channel) && (x.id == ls.hw_id) && (x.id_src == server_id));
                if (sub_idx < 0)
                    {
                    last_err = String.Format("Unable to find Node ID {0} - please check config database", ls.hw_id);
                    return false;
                    }
                TspStatusSubItem subitem = items[idx].Sub_items[sub_idx];

                TspStatusSubItem.status lastLevel = subitem.state;
                DateTime lastDesc = subitem.desc.tstamp;
                bool tsp_changed = false;
                // subitem.TimeOut = false;
                subitem.Pending = false; // After first update..

                if (ls.level == (int)TspStatusItem.status.error)
                    {
                    int last_guid = subitem.edesc.guid;
                    if (!accumulate) subitem.eCount = 0;
                    subitem.eCount += ls.count;
                    if (!accumulate || subitem.edesc.tstamp < ls.tstamp)
                        {
                        subitem.edesc = new TspStatusSubItem.log_event()
                            {
                            msg = (ls.msg == null) ? "" : ls.msg.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\"),
                            channel = ls.channel,
                            event_id = ls.eventID,
                            target_id = ls.targetID,
                            tstamp = ls.tstamp,
                            guid = ls.guid,
                            };
                        }
                    tsp_changed = (last_guid != subitem.edesc.guid);
                    }
                else if (ls.level == (int)TspStatusItem.status.warning)
                    {
                    int last_guid = subitem.wdesc.guid;
                    if (!accumulate) subitem.wCount = 0;
                    subitem.wCount += ls.count;
                    if (!accumulate || subitem.wdesc.tstamp < ls.tstamp)
                        {
                        subitem.wdesc = new TspStatusSubItem.log_event()
                            {
                            msg = (ls.msg == null) ? "" : ls.msg.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\"),
                            channel = ls.channel,
                            event_id = ls.eventID,
                            target_id = ls.targetID,
                            tstamp = ls.tstamp,
                            guid = ls.guid,
                            };
                        }
                    tsp_changed = (last_guid != subitem.wdesc.guid);
                    }
                else /* if (level == (int)TspStatusItem.status.ok) */
                    {
                    int last_guid = subitem.idesc.guid;
                    if (!accumulate) subitem.iCount = 0;
                    subitem.iCount += ls.count;
                    if (!accumulate || subitem.idesc.tstamp < ls.tstamp)
                        {
                        subitem.idesc = new TspStatusSubItem.log_event()
                            {
                            msg = (ls.msg == null) ? "" : ls.msg.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\"),
                            channel = ls.channel,
                            event_id = ls.eventID,
                            target_id = ls.targetID,
                            tstamp = ls.tstamp,
                            guid = ls.guid,
                            };
                        }
                    if (ls.level != (int)TspStatusItem.status.ok)
                        {
                        last_err = string.Format("Unexpected level: {0}", ls.level);
                        }
                    tsp_changed = (last_guid != subitem.idesc.guid);
                    }

                // Update the top-level every time
                items[idx].lastUpdate = DateTime.Now;  //CRR issue #18 Use just local time
                // items[idx].TimeOut = false;
                items[idx].Pending = false; // After first update..

                subitem.lastUpdate = DateTime.Now; //CRR issue #18 Use just local time
                items[idx].Sub_items[sub_idx] = subitem; // Update every time

                // Check for a change in event status
                if (tsp_changed)
                    {
                    // items[idx] = item; // Update the master list

                    TspEventArgs args = new TspEventArgs();
                    args.item = items[idx];
                    OnChanged(args);
                    }
                return true;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            return false;
            }

        public void AddSubItem(Hw_id hw_node, int id = -1)
            {
            try
                {
                if (id == -1) id = hw_node.id; // If set, parent will be different to the main item

                // Add either one or two hardware nodes
                if (!items.Exists(x => x.Id == id))
                    {
                    TspStatusItem item = new TspStatusItem(id, hw_node.isBackup, hw_node.channels); // Create a new parent node
                    items.Add(item);
                    }

                int idx = items.FindIndex(x => x.Id == id);
                if (hw_node.channels.Count > 0)
                    {
                    foreach (int channel in hw_node.channels)
                        {
                        if (hw_node.isRedundant && (items[idx].isBackup < 0))
                            {
                            items[idx].Sub_items.Add(new TspStatusSubItem(hw_node.id, hw_node.id_main, 0, channel));
                            items[idx].Sub_items.Add(new TspStatusSubItem(hw_node.id, hw_node.id_backup, 1, channel));
                            }
                        else if (items[idx].isBackup == hw_node.isBackup)
                            {
                            int parent = (hw_node.isBackup == 1) ? hw_node.id_backup : hw_node.id_main;
                            items[idx].Sub_items.Add(new TspStatusSubItem(hw_node.id, parent, hw_node.isBackup, channel));
                            }
                        }
                    }
                else if (hw_node.isRedundant && (items[idx].isBackup < 0))
                    {
                    items[idx].Sub_items.Add(new TspStatusSubItem(hw_node.id, hw_node.id_main, 0));
                    items[idx].Sub_items.Add(new TspStatusSubItem(hw_node.id, hw_node.id_backup, 1));
                    }
                else if (items[idx].isBackup == hw_node.isBackup)
                    {
                    int parent = (hw_node.isBackup == 1) ? hw_node.id_backup : hw_node.id_main;
                    items[idx].Sub_items.Add(new TspStatusSubItem(hw_node.id, parent));
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        public TspStatusItem GetStatus(int hw_id)
            {
            int idx = items.FindIndex(x => x.Id == hw_id);

            if (idx < 0)
                {
                last_err = string.Format("Unexpected hardware ID: {0}", hw_id);
                return null;
                }

            return items[idx];
            }

        // This function is called at regular intervals
        // The primary role is to update the maintenance events that have occurred.
        // As part of this, the logs database will be polled at regular intervals (e.g. every 5 minutes) to check for maintenance
        public void updateMaintenance(List<MaintenanceEntry> entries)
            {
            if (Program.UseSNMP)                //CRR Issue #40 If using SNMP then ignore Maintenance
                return;
            try
                {
                // Poll all of the attached log client databases
                // If they respond, then update the 
                XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
                foreach (LogClient lc in xml.LogClients)
                    {
                    foreach (MaintenanceEntry item in lc.GetMaintenanceEvents())
                        {
                        MaintenanceEntry me = entries.Find(x => ((x.server_id == lc.Hw_id) && (x.channel == item.channel)));
                        if (me != null)
                            {
                            me.active = item.active;
                            me.Desc = item.Desc;
                            me.StartDate = item.StartDate;
                            me.EndDate = item.EndDate;
                            }
                        }
                    // TODO - ADAM what happens if we cannot contact the LogClient application?
                    }


                // Now that we have the latest set of maintenance events, let's update the TSP view
                DateTime now = DateTime.Now; //CRR issue #18 Use just local time
                List<MaintenanceEntry> entry_list = entries.FindAll(x => (x.active == true) && (x.StartDate < now) && (x.EndDate > now));
                foreach (TspStatusItem item in items)
                    {
                    int count = 0;
                    foreach (TspStatusSubItem sub_item in item.Sub_items)
                        {
                        bool isMaintence = false;
                        foreach (MaintenanceEntry entry in entry_list.FindAll(x => (x.server_id == sub_item.id_src)))
                            {
                            if ((entry.channel == -1) || (sub_item.channel == entry.channel))
                                {
                                isMaintence = true;
                                break;
                                }
                            }
                        sub_item.Maintenance = isMaintence;
                        if (isMaintence) count++;
                        }
                    bool newState = (count == item.Sub_items.Count);
                    if (newState != item.Maintenance)
                        {
                        item.Maintenance = (count == item.Sub_items.Count);
                        TspEventArgs args = new TspEventArgs();
                        args.item = item;
                        OnChanged(args);
                        }
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        public void checkWatchdog()
            {
            try
                {
                DateTime current = DateTime.Now; //CRR issue #18 Use just local time
                TimeSpan timeout = TimeSpan.FromSeconds(Properties.Settings.Default.TimeoutAlarm);

                foreach (TspStatusItem item in items)
                    {
                    int changes = 0;
                    int count = 0;
                    bool expired;
                    foreach (var sub_item in item.Sub_items)
                        {
                        expired = false;
                        if ((!sub_item.Maintenance) && ((current - sub_item.lastUpdate) > timeout))
                            {
                            expired = true;
                            count++;
                            }

                        if (sub_item.TimeOut != expired)
                            {
                            sub_item.TimeOut = expired;
                            changes++;
                            }
                        }

                    // Were there any top-level changes to make?
                    expired = false;
                    if ((count > 0) && (count == item.Sub_items.FindAll(x => !x.Maintenance).Count)) // Excluding nodes marked as "maintenance"
                        {
                        expired = true;
                        }
                    if (item.TimeOut != expired)
                        {
                        changes++;
                        item.TimeOut = expired;
                        }

                    // Were there any changes to publish?
                    if (changes > 0)
                        {
                        TspEventArgs args = new TspEventArgs()
                            {
                            item = item,
                            };
                        OnChanged(args);
                        }
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }
        }

    public class UserButton : Button
        {

        Image mainImage = null; // , pressedImage, hoverImage;
        bool pressed = false;
        bool hover = false;
        public string toolTipText = "";
        protected bool alarm = false; //false;
        bool isHighlight = false;
        public bool Highlight
            {
            get { return isHighlight; }
            set
                {
                isHighlight = value;
                Invalidate();
                }
            }

        public string ToolTipText
            {
            get { return toolTipText; }
            set { toolTipText = value; }
            }
        public bool Alarm
            {
            get { return alarm; }
            set { alarm = value; }
            }

        // Property for the background image to be drawn behind the button text. 
        public Image MainImage
            {
            get
                {
                return this.mainImage;
                }
            set
                {
                this.mainImage = value;
                }
            }


        // When the mouse button is pressed, set the "pressed" flag to true  
        // and invalidate the form to cause a repaint.  The .NET Compact Framework  
        // sets the mouse capture automatically. 
        protected override void OnMouseDown(MouseEventArgs e)
            {
            this.pressed = true;
            this.Invalidate();
            base.OnMouseDown(e);
            }

        // When the mouse is released, reset the "pressed" flag 
        // and invalidate to redraw the button in the unpressed state. 
        protected override void OnMouseUp(MouseEventArgs e)
            {
            this.pressed = false;
            this.Invalidate();
            base.OnMouseUp(e);
            }
        // When the mouse button is pressed, set the "pressed" flag to true  
        // and invalidate the form to cause a repaint.  The .NET Compact Framework  
        // sets the mouse capture automatically. 
        protected override void OnMouseEnter(EventArgs e)
            {
            this.hover = true;
            this.Invalidate();
            base.OnMouseEnter(e);
            }
        // When the mouse button is pressed, set the "pressed" flag to true  
        // and invalidate the form to cause a repaint.  The .NET Compact Framework  
        // sets the mouse capture automatically. 
        protected override void OnMouseLeave(EventArgs e)
            {
            this.hover = false;
            this.pressed = false;
            this.Invalidate();
            base.OnMouseLeave(e);
            }

        ToolTip tip = new ToolTip();

        // When the mouse button is pressed, set the "pressed" flag to true  
        // and invalidate the form to cause a repaint.  The .NET Compact Framework  
        // sets the mouse capture automatically. 
        protected override void OnMouseHover(EventArgs e)
            {
            if (toolTipText != "")
                {
                tip.SetToolTip(this, this.toolTipText);
                }
            base.OnMouseHover(e);
            }

        // Override the OnPaint method to draw the background image and the text. 
        protected override void OnPaint(PaintEventArgs e)
            {
            //base.OnPaint(e);
            bool isBasic = false;

            if (this.mainImage != null)
                {
                Brush b = null;
                e.Graphics.DrawImage(this.mainImage, 0, 0, mainImage.Width, mainImage.Height);
                if (this.pressed)
                    {
                    b = new SolidBrush(Color.FromArgb(50, Color.Black));
                    }
                else if (isHighlight)
                    {
                    b = new SolidBrush(Color.FromArgb(50, Color.White));
                    }
                if (b != null) e.Graphics.FillRectangle(b, ClientRectangle);
                }
            else
                {
                isBasic = true;
                SolidBrush b = new SolidBrush(BackColor);
                if (pressed)
                    {
                    b = new SolidBrush(Color.FromArgb(BackColor.A,
                    (int)(BackColor.R * 0.8), (int)(BackColor.G * 0.8), (int)(BackColor.B * 0.8)));
                    }
                else if (isHighlight)
                    {

                    b = new SolidBrush(Color.FromArgb(BackColor.A,
                    (byte)Math.Min((double)BackColor.R + 50, 255), (byte)Math.Min((double)BackColor.G + 50, 255), (byte)Math.Min((double)BackColor.B + 50, 255)));
                    }
                e.Graphics.FillRectangle(b, 0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
                }

            // Draw the text if there is any. 
            if (isBasic && (this.Text.Length > 0))
                {
                //SizeF size = e.Graphics.MeasureString(this.Text, this.Font);

                StringFormat sf = new StringFormat()
                    {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    };
                // Center the text inside the client area of the PictureButton.
                Rectangle rect = new Rectangle()
                    {
                    Height = this.ClientSize.Height - 2,
                    Width = this.ClientSize.Width - 2,
                    X = 2,
                    Y = 2,
                    };
                double Y = (0.2126 * BackColor.R + 0.7152 * BackColor.G + 0.0722 * BackColor.B) / 256.0;
                Brush f = (Y > 0.4) ? Brushes.Black : Brushes.White;
                e.Graphics.DrawString(this.Text,
                    this.Font, f, rect, sf);
                }

            // Draw a border around the outside
                {
                Pen p = (hover) ? Pens.Yellow : Pens.Black;

                e.Graphics.DrawRectangle(p, 0, 0,
                    this.ClientSize.Width - 1, this.ClientSize.Height - 1);
                }
            }

        public Hw_id hw_id = null; // Hardware ID

        //Timer timer;
        public UserButton() : base()
            {
            BackColor = Properties.Settings.Default.userColour;
            //BackColor = Properties.Settings.Default.expireColour;   //CRR #50 - Did not work
            //timer = new Timer();
            //timer.Interval = 500;
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Enabled = true;
            }

        /*
        void timer_Tick(object sender, EventArgs e)
        {
            if (alarm)
            {
                isHighlight = !isHighlight;
                Invalidate();
            }
        }
         */

        public List<User_action> UserActions;
        public List<WebPage_action> Actions
            {
            get;
            set;
            }
        public Tsp_hw_node tsp_node; // Copy of the tsp_node used to create it
        }

    public class TspButton : UserButton
        {
        public enum states
            {
            MAINTENANCE = -3,
            PENDING = -2,
            EXPIRED = -1,
            OK = 0,
            WARN,
            ERR,
            }

        states m_status;
        public states Status
            {
            get
                {
                return m_status;
                }
            set
                {
                m_status = value; // OK=green, WARN=orange, ERR=red, UNKNOWN=grey
                if (m_status == states.ERR) BackColor = Properties.Settings.Default.errorColour;
                else if (m_status == states.WARN) BackColor = Properties.Settings.Default.warnColour;
                else if (m_status == states.EXPIRED) BackColor = Properties.Settings.Default.expireColour;
                else if (m_status == states.MAINTENANCE) BackColor = Properties.Settings.Default.maintenanceColour;
                //else if (m_status == states.PENDING) BackColor = Properties.Settings.Default.expireColour;      //CRR #50
                else BackColor = Properties.Settings.Default.infoColour; // INFO or PENDING
                alarm = ((m_status == states.ERR) || (m_status == states.EXPIRED));
                }
            }

        public TspButton() : base()
            {
            Status = states.OK;             //CRR #50 - Decideded not too in the end as it wopuld be necessary to set all object to green if you did NOT from them.
                                            //Status = states.PENDING;          //CRR #50
                                            //Status = states.EXPIRED;          //CRR #50
            }
        }
    }
