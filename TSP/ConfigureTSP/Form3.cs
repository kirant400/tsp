using System;
using System.Windows.Forms;
using TechnicalSupervisor;
using System.Net;
using System.Drawing;
using System.IO;

namespace ConfigureTSP
{
    //public delegate void DatabaseChangedEventHandler(object sender, EventArgs e);

    public partial class Form3 : Form
    {
        private XmlDatabaseInterface m_xml;

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
            m_xml = XmlDatabaseInterface.Instance;
            m_xml.XmlChanged += new XmlChangedEventHandler(Frm3_DatabaseChanged);
            m_xml.XmlItemChanged += M_xml_XmlItemChanged;
            InitializeComponent();
        }

        private void M_xml_XmlItemChanged(object sender, TspItemEventArgs e)
        {
            string msg;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[e.Index];
            if (tsp_node.id >= 0)
            {
                Hw_id hw = m_xml.Db.GetHwId(tsp_node.id);
                msg = string.Format("{0} ({1})", tsp_node.text, hw.hw_name);
            }
            else
            {
                msg = tsp_node.text;
            }
            if (e.Index < listBox3.Items.Count)
            {
                if (listBox3.Items.Count == m_xml.Tsp_hw_nodes.Count)
                {
                    listBox3.Items[e.Index] = msg;
                }
                else
                {
                    listBox3.Items.Insert(e.Index, msg);
                }
            }
            else
            {
                listBox3.Items.Add(msg);
            }

            // Enable sort if two or more items available.
            button8.Enabled = (listBox3.Items.Count > 1);
        }

        private void Frm3_DatabaseChanged(object sender, EventArgs e)
        {
            textBox3.Text = m_xml.Name;
            textBox1.Text = Path.GetFileName(m_xml.Db_fname);
            textBox2.Text = Path.GetFileName(m_xml.Img_fname);

            if (m_xml.Db != null)
            {
                listBox1.Items.Clear();
                foreach (Hw_id hw_node in m_xml.Db.m_hw_list)
                {
                    listBox1.Items.Add(String.Format("{0} ({1})", hw_node.hw_name, hw_node.id));
                }
                comboBox1.Items.Clear();
                foreach (Hw_id hw_node in m_xml.Db.m_hw_list)
                {
                    string type_name = m_xml.Db.m_hw_types[hw_node.hw_type];
                    comboBox1.Items.Add(String.Format("{0} ({1})", hw_node.hw_name, type_name));
                }

                if (m_xml.LogClients != null)
                {
                    listBox2.Items.Clear();
                    foreach (LogClient lc in m_xml.LogClients)
                    {
                        Hw_id hw = m_xml.Db.GetHwId(lc.Hw_id);
                        listBox2.Items.Add(string.Format("{2} - {0}:{1}", lc.Ep.Address.ToString(), lc.Ep.Port, m_xml.GetNodeName(lc.Hw_id)), lc.Enabled);
                    }
                }
                
                if (m_xml.Tsp_hw_nodes != null)
                {
                    listBox3.Items.Clear();
                    foreach (Tsp_hw_node hw_node in m_xml.Tsp_hw_nodes)
                    {
                        if (hw_node.id >= 0)
                        {
                            Hw_id hw = m_xml.Db.GetHwId(hw_node.id);
                            listBox3.Items.Add(string.Format("{0} ({1})", hw_node.text, hw.hw_name));
                        } else
                        {
                            listBox3.Items.Add(hw_node.text);
                        }
                    }
                }
                button8.Enabled = (listBox3.Items.Count > 1);
                textBox5.Text = ColorTranslator.ToHtml(m_xml.Background);
            }
            if ((m_xml.Font_size >= (float)numericUpDown2.Minimum) && (m_xml.Font_size <= (float)numericUpDown2.Maximum))
            {
                numericUpDown2.Value = (decimal)m_xml.Font_size;
            }
        }

        public void AddNewButton(UserButton newButton)
        {
            Tsp_hw_node hw_node = m_xml.Tsp_hw_nodes[newButton.tsp_index];
            if (hw_node.id >= 0)
            {
                Hw_id hw = m_xml.Db.GetHwId(hw_node.id);
                listBox3.Items.Add(string.Format("{0} ({1})", hw_node.text, hw.hw_name));
            }
            else
            {
                listBox3.Items.Add(string.Format("hw_node.text"));
            }

        }

        public bool LoadSqlConfig()
        {
            openFileDialog1.Filter = "SQLite config files|*.db|All files|*.*";
            openFileDialog1.DefaultExt = "DB";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!m_xml.LoadSQLiteDatabase(openFileDialog1.FileName))
                {
                    return false;
                }

                // everything is OK let's proceed...
                textBox1.Text = openFileDialog1.FileName;
                return true;
            }

            return true;
        }

        public bool LoadBackgroundImage()
        {
            openFileDialog1.Filter = "Bitmap files|*.png;*.bmp|All files|*.*";
            openFileDialog1.DefaultExt = "PNG";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //m_xml.LoadSQLiteDatabase(openFileDialog1.FileName)
                //xml = new XmlDatabaseInterface(""); // only valid when non-null
                // xml.background = panel1.BackColor;
                if (!m_xml.LoadImage(openFileDialog1.FileName))
                {
                    return false;
                }

                //  everything is OK let's proceed...
                // textBox2.Text = openFileDialog1.FileName;
                return true;
            }

            // xml = null;
            return true;
        }

        private bool visible = true;
        public new bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (!visible && value)
                {
                    Show();
                }
                else if (visible && !value)
                {
                    Hide();
                }
                visible = value;
            }
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
        }

        private void ListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                return;
            }

            int index = listBox1.IndexFromPoint(e.X, e.Y);
            string s = String.Format("node_index={0}", index);
            listBox1.DoDragDrop(s, DragDropEffects.All);
        }

        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent("Text"))
                e.Effect = DragDropEffects.Copy;
            
            // string message = e.Data.GetData("Text") as string;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (!LoadSqlConfig())
            {
                MessageBox.Show(m_xml.last_err, "Error! Unable to read database");
            }
            // Call event - database changed
            // OnDatabaseChanged(EventArgs.Empty);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (!LoadBackgroundImage())
            {
                MessageBox.Show(m_xml.last_err, "Error! Unable to read database");
            }

            // Call event - database changed
            // OnDatabaseChanged(EventArgs.Empty);
        }

        private void listBox3_MouseClick(object sender, MouseEventArgs e)
        {
            Form2 Frm2 = Form2.Instance;
            Form4 Frm4 = Form4.Instance;
            int index = listBox3.SelectedIndex;
            if (index < 0) return;
            UserButton thisButton = Frm4.panel4.Controls[0].Controls[index] as UserButton;
            Frm2.UpdateForm(thisButton);
        }

        private void listBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CheckedListBox control = sender as CheckedListBox;
            int idx = control.SelectedIndex;
            if (idx >= 0)
            {
                LogClient lc = m_xml.LogClients[idx];
                lc.Enabled = (e.NewValue == CheckState.Checked);
                m_xml.LogClients[idx] = lc;
            }

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox control = sender as CheckedListBox;
            int idx = control.SelectedIndex;
            if (idx >= 0)
            {
                int i = 0;
                foreach (LogClient lc in m_xml.LogClients)
                {
                    if (idx == i)
                    {
                        textBox4.Text = lc.Ep.Address.ToString();
                        numericUpDown1.Value = lc.Ep.Port;
                        comboBox1.SelectedIndex = m_xml.Db.m_hw_list.FindIndex(x => x.id == lc.Hw_id);
                        break;
                    }
                    i++;
                }
            }
            button5.Enabled = (idx >= 0);
            ValidatePage2();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            IPAddress ipv4;
            if (!IPAddress.TryParse(textBox4.Text, out ipv4))
            {
                MessageBox.Show("Invalid IPv4 address entered. Please correct, and try again", "Invalid IPv4 address");
                return;
            }

            LogClient client = new LogClient()
            {
                Enabled = true,
                Ep = new IPEndPoint(ipv4, (int)numericUpDown1.Value),
                Hw_id = m_xml.Db.m_hw_list[comboBox1.SelectedIndex].id,
            };
            m_xml.LogClients.Add(ref client);
            listBox2.Items.Add(string.Format("{2} - {0}:{1}", client.Ep.Address.ToString(), client.Ep.Port, m_xml.GetNodeName(client.Hw_id)), client.Enabled);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            IPAddress ipv4;
            if (!IPAddress.TryParse(textBox4.Text, out ipv4))
            {
                MessageBox.Show("Invalid IPv4 address entered. Please correct, and try again", "Invalid IPv4 address");
                return;
            }

            int idx = listBox2.SelectedIndex;
            if (idx >= 0)
            {
                LogClient lc = m_xml.LogClients[idx];
                lc.Ep = new IPEndPoint(ipv4, (int)numericUpDown1.Value);
                lc.Hw_id = m_xml.Db.m_hw_list[comboBox1.SelectedIndex].id;
                listBox2.Items[idx] = string.Format("{2} - {0}:{1}", lc.Ep.Address.ToString(), lc.Ep.Port, m_xml.GetNodeName(lc.Hw_id));
                m_xml.LogClients[idx] = lc;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int idx = listBox2.SelectedIndex;
            if (idx >= 0)
            {
                m_xml.LogClients.RemoveAt(idx);
                listBox2.Items.RemoveAt(idx);
                if (idx >= listBox2.Items.Count) idx = (listBox2.Items.Count - 1);
                if (idx >= 0)
                {
                    listBox2.SelectedIndex = idx;
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            m_xml.Name = (sender as TextBox).Text;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value == 0) return;
            m_xml.Font_size = (float)numericUpDown2.Value;
            Font newFont = new Font(this.Font.FontFamily, (float)numericUpDown2.Value);
            foreach (Control ctrl in Form4.Instance.pictureBox1.Controls)
            {
                ctrl.Font = newFont;
            }
        }

        void ValidatePage2()
        {
            bool valid = true;

            IPAddress ipv4;
            if (!IPAddress.TryParse(textBox4.Text, out ipv4))
            {
                valid = false;
            }

            if (comboBox1.SelectedIndex < 0)
            {
                valid = false;
            }
            button4.Enabled = (valid && (listBox2.SelectedIndex >= 0)); // edit button
            button1.Enabled = valid; // Add button
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidatePage2();
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            button7.Enabled = (listBox3.SelectedIndex < 0);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            ValidatePage2();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ValidatePage2();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = ColorTranslator.FromHtml(textBox5.Text);
            if (colorDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox5.Text = ColorTranslator.ToHtml(colorDialog1.Color) ;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Delete items
            int idx = listBox2.SelectedIndex;
            if (idx >= 0)
            {
                m_xml.RemoveTspItem(idx);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            m_xml.SortTspItems();
        }
    }
}
