using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TechnicalSupervisor
{
    public partial class Form4 : Form
    {
        List<int> m_server_list;
        XmlDatabaseInterface m_xml;
        List<MaintenanceEntry> m_events;
        public List<MaintenanceEntry> MaintenanceEvents
        {
            get {
                return m_events;
            }
            set
            {
                m_events = new List<MaintenanceEntry>(value.Count);
                foreach (MaintenanceEntry e in value)
                {
                    if ((e.EndDate > DateTime.MinValue) && (e.StartDate > DateTime.MinValue))
                    {
                        m_events.Add(new MaintenanceEntry(e));
                    }
                }

                if (m_events.Count > 0)
                {
                    UpdateCheckList(m_events[0]);
                }
                m_changes_made = false;
            }

        }
        public Form4()
        {
            m_xml = XmlDatabaseInterface.Instance;

            InitializeComponent();

            DateTime now = DateTime.Now.Date; // midnight //CRR issue #18 Use just local time
            dateTimePicker3.Value = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            now += TimeSpan.FromDays(1);
            dateTimePicker4.Value = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            m_channels = new List<int>();

            // Dictionary<int, List<int>> m_servers = new Dictionary<int, List<int>>();
            m_server_list = new List<int>();
            comboBox1.Items.Clear();
            foreach (LogClient lc in m_xml.LogClients)
            {
                if (lc.Enabled)
                {
                    m_server_list.Add(lc.Hw_id);
                    comboBox1.Items.Add(m_xml.GetNodeName(lc.Hw_id)); // list of servers
                }
            }
            comboBox1.SelectedIndex = 0; // Select first line item
        }

#if False // NOT YET IMPLEMENTED
        List<MaintenanceEntry> LoadDatabaseMaintenance()
        {
            List<MaintenanceEntry>  events = new List<MaintenanceEntry>();
            foreach (LogClient lc in m_xml.LogClients)
            {
                events.AddRange(lc.GetMaintenanceEvents());
            }
            return events;
        }
#endif

        void UpdateCheckList(MaintenanceEntry current)
            {
            try
                {
                checkedListBox1.Items.Clear();
                // m_events.Sort(SortEvents);

                checkedListBox1.Items.AddRange(m_events.ToArray());

                // Check the active items
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                    // MaintenanceEntry e = (MaintenanceEntry)checkedListBox1.Items[i];
                    checkedListBox1.SetItemChecked(i, m_events[i].active);
                    }
                // Select the current item, if given
                if (current != null)
                    {
                    checkedListBox1.SelectedItem = current;
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        void ShowColourDialog(ref TextBox src)
            {
            try
                {
                int colour = int.Parse(src.Text.TrimStart('#'), System.Globalization.NumberStyles.HexNumber);
                colorDialog1.Color = Color.FromArgb(colour);
                if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                    src.Text = "#" + colorDialog1.Color.ToArgb().ToString("X8");
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowColourDialog(ref textBox1);
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            ShowColourDialog(ref textBox2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ShowColourDialog(ref textBox3);
        }
        bool m_user_edit = false;
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            m_user_edit = true;
            dateTimePicker3.Value = dateTimePicker1.Value.Date + dateTimePicker3.Value.TimeOfDay;
            ValidateButtons();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            m_user_edit = true;
            dateTimePicker4.Value = dateTimePicker2.Value.Date + dateTimePicker4.Value.TimeOfDay;
            ValidateButtons();
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            m_user_edit = true;
            if (dateTimePicker1.Value.Date != dateTimePicker3.Value.Date)
            {
                dateTimePicker1.Value = dateTimePicker3.Value;
            }
            if (dateTimePicker3.Value > dateTimePicker4.Value)
            {
                dateTimePicker2.Value = dateTimePicker3.Value;
                dateTimePicker4.Value = dateTimePicker3.Value;
            }
            ValidateButtons();
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            m_user_edit = true;
            if (dateTimePicker2.Value.Date != dateTimePicker4.Value.Date)
            {
                dateTimePicker2.Value = dateTimePicker4.Value;
            }
            if (dateTimePicker4.Value < dateTimePicker3.Value)
            {
                dateTimePicker1.Value = dateTimePicker4.Value;
                dateTimePicker3.Value = dateTimePicker4.Value;
            }
            ValidateButtons();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
            try
                {
                if (checkedListBox1.SelectedIndex >= 0)
                    {
                    // Populate the box based on the contents of this dialog..
                    MaintenanceEntry me = m_events[checkedListBox1.SelectedIndex];
                    if ((me.StartDate > dateTimePicker3.MinDate) && (me.StartDate < dateTimePicker3.MaxDate))
                        dateTimePicker3.Value = me.StartDate;
                    if ((me.EndDate > dateTimePicker3.MinDate) && (me.EndDate < dateTimePicker3.MaxDate))
                        dateTimePicker4.Value = me.EndDate;
                    comboBox1.SelectedIndex = m_server_list.FindIndex(x => x == me.server_id);
                    if (me.channel >= 0)
                        {
                        int idx2 = m_channels.FindIndex(x => x == me.channel);
                        if (idx2 >= 0)
                            {
                            comboBox2.SelectedIndex = idx2;
                            }
                        }
                    textBox4.Text = me.Desc;
                    }
                m_user_edit = false; // Clear the edit flag
                ValidateButtons();
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        List<int> m_channels;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
            try
                {
                m_user_edit = true;
                // Update combobox2 with a new list of items
                comboBox2.Items.Clear();
                m_channels.Clear();
                if (comboBox1.SelectedIndex < 0) return;

                // Update combobox drop-down selection
                comboBox2.Items.Add("Full system");
                m_channels.Add(-1);
                int hw_id = m_server_list[comboBox1.SelectedIndex];
                List<ChannelItem> channels = m_xml.Db.m_channel_lookup.FindAll(x => x.server_id == hw_id);
                if (channels != null)
                    {
                    foreach (ChannelItem channel in channels)
                        {
                        m_channels.Add(channel.channel);
                        comboBox2.Items.Add(string.Format("CH{0} ({1})", channel.channel, m_xml.Db.m_hw_types[channel.hw_type]));
                        }
                    }
                comboBox2.SelectedIndex = 0;

                ValidateButtons();
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_user_edit = true;
            ValidateButtons();
        }

        void PromptColour(TextBox control)
        {
            colorDialog1.Color = ColorTranslator.FromHtml(control.Text);
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                control.Text = ColorTranslator.ToHtml(colorDialog1.Color);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            PromptColour(textBox1);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            PromptColour(textBox2);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            PromptColour(textBox3);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            PromptColour(textBox7);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            PromptColour(textBox6);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PromptColour(textBox5);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Any changes made to the maintenance schedule will be discarded. Press OK to continue.", "Are you sure you want to restore all factory defaults?", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Retry;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            m_user_edit = true;
            ValidateButtons();
        }

        void ValidateButtons()
            {
            try
                {
                bool valid = true;
                if (comboBox1.SelectedIndex < 0) valid = false;
                else if (comboBox2.SelectedIndex < 0) valid = false;
                else if (textBox4.Text.Length == 0) valid = false;
                button7.Enabled = valid; // Add (or replace)
                if (checkedListBox1.SelectedIndex < 0) valid = false;
                if (!m_user_edit) valid = false;
                button6.Enabled = valid; // Update
                button8.Enabled = (checkedListBox1.SelectedIndex >= 0); // Remove
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        int SortEvents(MaintenanceEntry b1, MaintenanceEntry b2)
        {
            int ret = b1.server_id.CompareTo(b2.server_id);
            if (ret == 0)
            {
                ret = b2.EndDate.CompareTo(b1.EndDate);
            }
            return ret;
        }

        private void button7_Click(object sender, EventArgs e)
            {
            try
                {
                // Add a new maintenance line
                m_changes_made = true;
                int hw_id = m_server_list[comboBox1.SelectedIndex];
                MaintenanceEntry entry = new MaintenanceEntry(hw_id, m_xml.GetNodeName(hw_id), m_channels[comboBox2.SelectedIndex])
                    {
                    active = true,
                    StartDate = dateTimePicker3.Value,
                    EndDate = dateTimePicker4.Value,
                    Desc = textBox4.Text,
                    };

                // Do we need to replace an existing entry?
                int idx = m_events.FindIndex(x => (x.server_id == entry.server_id) && (x.channel == entry.channel));
                if (idx != -1)
                    {
                    // Are you sure?
                    if (MessageBox.Show(this, "Press OK to replace the current maintenance entry with this update.", "Are you sure you want to replace entry?", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                        {
                        return;
                        }
                    m_events[idx] = entry;
                    checkedListBox1.Items[idx] = entry;
                    }
                else
                    {
                    m_events.Add(entry);
                    checkedListBox1.Items.Add(entry);
                    UpdateCheckList(entry);
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void button6_Click(object sender, EventArgs e)
            {
            try
                {
                // Update an existing maintenance line
                m_changes_made = true;
                m_user_edit = false;
                int hw_id = m_server_list[comboBox1.SelectedIndex];
                MaintenanceEntry me = m_events[checkedListBox1.SelectedIndex];
                me.server_id = hw_id;
                me.server_name = m_xml.GetNodeName(hw_id);
                me.channel = m_channels[comboBox2.SelectedIndex];
                me.StartDate = dateTimePicker3.Value;
                me.EndDate = dateTimePicker4.Value;
                me.Desc = textBox4.Text;

                UpdateCheckList(me);
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void button8_Click(object sender, EventArgs e)
            {
            try
                {
                // Remove a maintenance line
                m_changes_made = true;
                int idx = checkedListBox1.SelectedIndex;
                checkedListBox1.Items.RemoveAt(idx);
                m_events.RemoveAt(idx);
                if (idx >= checkedListBox1.Items.Count) idx = checkedListBox1.Items.Count - 1;
                if (idx >= 0)
                    {
                    checkedListBox1.SelectedIndex = idx;
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        bool m_changes_made = false;
        public bool MaintenanceChanged
        {
            get { return m_changes_made; }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (m_user_edit)
            {
                if (MessageBox.Show(this, "Press OK to lose the changes", "Warning! Changes made have not been saved", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                {
                    DialogResult = System.Windows.Forms.DialogResult.None; // Cancelled
                }
            }
            /*
            if (m_changes_made)
            {
                foreach (LogClient lc in m_xml.LogClients)
                {
                    lc.SetMaintenanceEvents(m_events.FindAll(x => x.server_id == lc.Hw_id));
                }
            }
             */
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
            {
            try
                {
                if (e.Index < 0) return;
                m_changes_made = true;
                m_events[e.Index].active = (e.NewValue == CheckState.Checked);
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void Form4_Load(object sender, EventArgs e)
            {
            if (Program.UseSNMP)         //CRR Issue #40 Hide Maintenance tab if using SNMP
                {
                if (tabControl1.TabPages.Count > 1)
                    tabControl1.TabPages.RemoveAt(1);
                }
            }

        private void button_ViewEvents_Click(object sender, EventArgs e)
            {
            FormViewEvents F = new FormViewEvents();
            F.ShowDialog();
            }
        }
}
