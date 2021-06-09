using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TechnicalSupervisor
{
    public partial class Form3 : Form
    {
        private static Form3 instance;

        public static Form3 Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Form3();
                }
                return instance;
            }
        }
        private Form3()
        {
            InitializeComponent();
        }

        

        public int id_src = -1;

        bool m_showForm = false;
        public bool ShowForm
        {
            set
            {
                if (m_showForm != value)
                {
                    if (value)
                    {
                        Show();
                        BringToFront();
                    }
                    else
                    {
                        Hide();
                    }
                    m_showForm = value;
                }
            }
            get
            {
                return m_showForm;
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                ShowForm = false;
                e.Cancel = true;
            }
        }

        public Color OverlayColour;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Brush b = new SolidBrush(OverlayColour);
            e.Graphics.FillEllipse(b, e.ClipRectangle);
        }

        private void toolStripStatusLabel3_Paint(object sender, PaintEventArgs e)
        {
            Brush b = new SolidBrush(OverlayColour);

            Rectangle rect = new Rectangle(e.ClipRectangle.Left, e.ClipRectangle.Top, e.ClipRectangle.Height, e.ClipRectangle.Height);
            e.Graphics.FillEllipse(b, rect);
        }

        delegate void UpdateListBoxConsumer(int id, List<TspStatusSubItem> items);

        List<string> m_names = new List<string>();
        int last_id = -1;
        List<TspStatusSubItem> m_items;
        public void UpdateListBox(int id, List<TspStatusSubItem> items)
            {
            try
                {
                if (listBox1.InvokeRequired)
                    {
                    listBox1.Invoke(new UpdateListBoxConsumer(UpdateListBox), id, items);
                    }
                else
                    {
                    if (id < 0) return; // No event to update..

                    if (last_id != id)
                        {
                        XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
                        m_names.Clear();
                        listBox1.Items.Clear();
                        foreach (var x in items)
                            {
                            string name = "";
                            if (x.channel >= 0)
                                {
                                name += string.Format("Channel {0}", x.channel);
                                }
                            else
                                {
                                name += xml.GetNodeName(x.id).Replace('_', ' ');
                                }
                            if (x.isBackup >= 0)
                                {
                                name += string.Format(" ({0})", xml.GetNodeName(x.id_src).Replace("_", " "));
                                }
                            m_names.Add(name);
                            listBox1.Items.Add(new Tuple<string, int>("", (int)TspStatusSubItem.status.pending));
                            }
                        last_id = id;
                        m_items = null;
                        UpdateRecentEvents(null);
                        }

                    for (int i = 0; i < items.Count; i++)
                        {
                        if ((m_items == null) || (m_items[i].state != items[i].state) || (m_items[i].iCount != items[i].iCount) || (m_items[i].wCount != items[i].wCount) || (m_items[i].eCount != items[i].eCount))
                            {
                            string status = "";
                            int count = items[i].eCount + items[i].wCount + items[i].iCount;
                            if (count > 0)
                                {
                                status = string.Format("{0} Event{1}", count, (count > 1) ? "s" : "");
                                }
                            else
                                {
                                status = "No Events";
                                }
                            if (items[i].state == TspStatusSubItem.status.maintenance)
                                {
                                status += " [MAINTENANCE]";
                                }
                            else if (items[i].state == TspStatusSubItem.status.unknown)
                                {
                                status += " [EXPIRED]";
                                }
                            else if (items[i].state == TspStatusSubItem.status.pending)
                                {
                                status += " [PENDING]";
                                }

                            int state = (int)items[i].state;
                            listBox1.Items[i] = new Tuple<string, int>(string.Format("{0} - {1}", m_names[i], status), state);
                            if (listBox1.SelectedIndex == i)
                                {
                                UpdateRecentEvents(items);
                                }
                            }
                        }

                    // Clone items to a new copy
                    m_items = items.ConvertAll(x => new TspStatusSubItem(x));
                    /*
                    new List<TspStatusSubItem>(items.Count);
                    for (int i = 0; i < items.Count; i++)
                    {
                        TspStatusSubItem sub_item = new TspStatusSubItem(items[i]);
                        m_items.Add(sub_item);
                    }
                    */
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRecentEvents(m_items);
        }

        void UpdateRecentEvents(List<TspStatusSubItem> items)
            {
            try
                {
                toolStripProgressBar1.Visible = false;
                if ((items == null) || (listBox1.SelectedIndex < 0))
                    {
                    textBox1.Text = "";
                    textBox1.Enabled = false;
                    button2.Enabled = false;
                    button1.Enabled = false;
                    button3.Enabled = false;
                    }
                else
                    {
                    XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
                    int idx = listBox1.SelectedIndex;

                    List<string> breakdown = new List<string>(3);
                    if (items[idx].eCount > 0)
                        {
                        breakdown.Add(string.Format("{0} Error{1}", items[idx].eCount, (items[idx].eCount > 1) ? "s" : ""));
                        }
                    if (items[idx].wCount > 0)
                        {
                        breakdown.Add(string.Format("{0} Warning{1}", items[idx].wCount, (items[idx].wCount > 1) ? "s" : ""));
                        }
                    if (items[idx].iCount > 0)
                        {
                        breakdown.Add(string.Format("{0} Info{1}", items[idx].iCount, (items[idx].iCount > 1) ? "s" : ""));
                        }
                    if (breakdown.Count == 0) breakdown.Add("No events");
                    textBox1.Text = String.Format("{0} Breakdown - {1}\r\n", xml.GetNodeName(items[idx].id), string.Join("; ", breakdown));
                    textBox1.Text += String.Format("Latest report: {0}\r\n", items[idx].lastUpdate);

                    // {1} Error{2}; {3} Warning{4}; {5} Infos{6}\r\n", xml.GetNodeName(items[idx].id), ;
                    if (items[idx].eCount > 0)
                        {
                        textBox1.Text += "\r\n" + items[idx].edesc.tstamp + "\r\n";
                        SqliteDatabaseInterface.Event_item event_item = xml.GetEventItem(items[idx].edesc.event_id);
                        textBox1.Text += string.Format("ERROR (1 OF {0}) {1}\r\n", items[idx].eCount, event_item.name.Replace('_', ' ')); // TODO - ADAM convert event ID to event name
                        textBox1.Text += items[idx].edesc.msg + "\r\n";
                        if (event_item.desc.Length > 0)
                            {
                            textBox1.Text += event_item.desc + "\r\n";
                            }
                        if (event_item.action.Length > 0)
                            {
                            textBox1.Text += event_item.action + "\r\n";
                            }
                        }
                    if (items[idx].wCount > 0)
                        {
                        textBox1.Text += "\r\n" + items[idx].wdesc.tstamp + "\r\n";
                        SqliteDatabaseInterface.Event_item event_item = xml.GetEventItem(items[idx].wdesc.event_id);
                        textBox1.Text += string.Format("WARNING (1 OF {0}) {1}\r\n", items[idx].wCount, event_item.name.Replace('_', ' ')); // TODO - ADAM convert event ID to event name
                        textBox1.Text += items[idx].wdesc.msg + "\r\n";
                        if (event_item.desc.Length > 0)
                            {
                            textBox1.Text += event_item.desc + "\r\n";
                            }
                        if (event_item.action.Length > 0)
                            {
                            textBox1.Text += event_item.action + "\r\n";
                            }
                        }
                    if (items[idx].iCount > 0)
                        {
                        textBox1.Text += "\r\n" + items[idx].idesc.tstamp + "\r\n";
                        SqliteDatabaseInterface.Event_item event_item = xml.GetEventItem(items[idx].idesc.event_id);
                        textBox1.Text += string.Format("INFORMATION (1 OF {0}) {1}\r\n", items[idx].iCount, event_item.name.Replace('_', ' ')); // TODO - ADAM convert event ID to event name
                        textBox1.Text += items[idx].idesc.msg + "\r\n";
                        if (event_item.desc.Length > 0)
                            {
                            textBox1.Text += event_item.desc + "\r\n";
                            }
                        if (event_item.action.Length > 0)
                            {
                            textBox1.Text += event_item.action + "\r\n";
                            }
                        }
                    textBox1.Enabled = true;
                    button2.Enabled = (items[idx].eCount > 0);
                    button1.Enabled = (items[idx].wCount > 0);
                    button3.Enabled = (items[idx].iCount > 0);
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }
        double GetBrightness(Color pen_colour)
        {
            double Y = (0.2126 * pen_colour.R + 0.7152 * pen_colour.G + 0.0722 * pen_colour.B) / 256.0;
            return Y; // (Y > 0.5) if very light
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
            {
            try
                {
                e.DrawBackground();
                if (e.Index == -1) return; // Early exit - nothing to do

                Color pencolour = Properties.Settings.Default.userColour;
                Tuple<string, int> lc = (Tuple<string, int>)listBox1.Items[e.Index];
                if (lc.Item2 == (int)TspStatusSubItem.status.maintenance)
                    {
                    pencolour = Properties.Settings.Default.maintenanceColour;
                    }
                else if (lc.Item2 == (int)TspStatusSubItem.status.error)
                    {
                    pencolour = Properties.Settings.Default.errorColour;
                    //                forecolour = new SolidBrush(Properties.Settings.Default.errorColour);
                    }
                else if (lc.Item2 == (int)TspStatusSubItem.status.warning)
                    {
                    pencolour = Properties.Settings.Default.warnColour;
                    //                forecolour = new SolidBrush(Properties.Settings.Default.warnColour);
                    }
                else if (lc.Item2 == (int)TspStatusSubItem.status.ok)
                    {
                    pencolour = Properties.Settings.Default.infoColour;
                    }
                else if (lc.Item2 == (int)TspStatusSubItem.status.unknown)
                    {
                    pencolour = Properties.Settings.Default.expireColour;
                    }

                Brush forecolour; //  = new SolidBrush(e.ForeColor);
                Brush backcolour = new SolidBrush(e.BackColor);
                Pen outline = null;

                // If very bright (> 50%) then use as an outline
                // otherwise, use as the main font
                if (GetBrightness(pencolour) > 0.5)
                    {
                    outline = new Pen(pencolour);
                    forecolour = Brushes.Black;
                    }
                else
                    {
                    forecolour = new SolidBrush(pencolour);
                    }

                // Swap
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                    // Brush tmp = backcolour;
                    backcolour = forecolour;
                    forecolour = Brushes.White;
                    }
                e.Graphics.FillRectangle(backcolour, e.Bounds);
                StringFormat s = new StringFormat();
                GraphicsPath p = new GraphicsPath();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                p.AddString(lc.Item1, e.Font.FontFamily, (int)e.Font.Style, (e.Font.SizeInPoints * (e.Graphics.DpiY / 72.0f)), new PointF(e.Bounds.X + 2, e.Bounds.Y), s);
                if (outline != null)
                    {
                    e.Graphics.DrawPath(outline, p);
                    }
                e.Graphics.FillPath(forecolour, p);
                e.Graphics.SmoothingMode = SmoothingMode.Default;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void button2_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = false;
            toolStripProgressBar1.Visible = true;
            AcknowledgeSelected(TspStatusItem.status.error); // Error
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = false;
            toolStripProgressBar1.Visible = true;
            AcknowledgeSelected(TspStatusItem.status.warning); // Warning
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = false;
            toolStripProgressBar1.Visible = true;
            AcknowledgeSelected(TspStatusItem.status.ok); // Info
        }

        private bool AcknowledgeSelected(TspStatusItem.status level)
        {
            int idx = listBox1.SelectedIndex;
            int id_src = m_items[idx].id_src;

            XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
            LogClient lc = xml.LogClients.AsList().Find(x => (x.Hw_id == id_src));
            int guid;
            if (level == TspStatusItem.status.error)
            {
                guid = m_items[idx].edesc.guid;
            }
            else if (level == TspStatusItem.status.warning)
            {
                guid = m_items[idx].wdesc.guid;
            }
            else
            {
                guid = m_items[idx].idesc.guid;
            }
            return lc.AcknowledgeByGuid(guid);
        }

        private void Form3_Load(object sender, EventArgs e)
            {
            if(Program.UseSNMP)         //CRR Issue #40 Hide buttons if using SNMP
                {
                button1.Hide();
                button2.Hide();
                button3.Hide();
                }
            }
        }
}
