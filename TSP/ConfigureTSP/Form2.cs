using System;
using System.Drawing;
using System.Windows.Forms;
using TechnicalSupervisor;
using System.Net;
using System.IO;

namespace ConfigureTSP
{
    public partial class Form2 : Form
    {
        XmlDatabaseInterface m_xml; // Reference to the parent class
        private static Form2 instance;
        public static Form2 Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Form2();
                }
                return instance;
            }
        }

        private Form2()
        {
            m_xml = XmlDatabaseInterface.Instance;
            m_xml.XmlChanged += new XmlChangedEventHandler(Frm2_DatabaseChanged);
            m_xml.XmlItemChanged += M_xml_XmlItemChanged;

            InitializeComponent();
            panel1.Enabled = false; // start disabled
        }

        private void M_xml_XmlItemChanged(object sender, TspItemEventArgs e)
        {
            if ((m_button == null) || (e.Index == m_button.tsp_index))
            {
                if (e.Index >= m_xml.Tsp_hw_nodes.Count)
                {
                    m_button = null;
                }
                UpdateForm(m_button);
            }
        }

        private void Frm2_DatabaseChanged(object sender, EventArgs e)
        {
            hardwareIdList.Items.Clear();
            hardwareIdList.Items.Add("System Button (-2)");
            hardwareIdList.Items.Add("User Button (-1)");
            if (m_xml.Db != null)
            {
                foreach (Hw_id hw_id in m_xml.Db.m_hw_list)
                {
                    hardwareIdList.Items.Add(String.Format("{0} ({1})", hw_id.hw_name, hw_id.id));
                }
            }
            m_button = null;
            UpdateForm(m_button);
        }

        public UserButton Button {
            get { return m_button; }
        }

        private bool isUserUpdate = true;
        UserButton m_button = null; // reference to the current button selection
        public void UpdateForm(UserButton button)
        {
            if (button == null)
            {
                panel1.Enabled = false;
                m_button = null;
                return;
            }
            isUserUpdate = false;
            m_button = button;
            // m_button2 = new UserButton(button);

            Tsp_hw_node hw = m_xml.Tsp_hw_nodes[button.tsp_index]; 
            // Tsp_hw_node hw = button.tsp_hw_node;
            if (!hw.isActive)
            {
                hardwareIdList.SelectedIndex = (hw.isSystem) ? 0 : 1; // .Text = "User Button";
                if (hw.isSystem)
                {
                    groupBox2.Enabled = true;
                    groupBox1.Enabled = false;
                }
                else
                {
                    groupBox1.Enabled = true;
                    groupBox2.Enabled = true;
                }
            }
            else
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = true;

                hardwareIdList.SelectedIndex = m_button.hw_id.idx + 2; //  index + 2;
            }
            textBox1.Text = hw.text;

            // Set image up
            textBox6.Text = Path.GetFileName(hw.img_main_fname);
            
            isEnabled.Checked = hw.Enabled;

            numericUpDownX.Value = Math.Min(hw.rect.Location.X, numericUpDownX.Maximum);
            numericUpDownY.Value = Math.Min(hw.rect.Location.Y, numericUpDownY.Maximum);
            numericUpDownSX.Value = Math.Min(hw.rect.Size.Width, numericUpDownSX.Maximum);
            numericUpDownSY.Value = Math.Min(hw.rect.Size.Height, numericUpDownSY.Maximum);

            buttonActionsList.Items.Clear();
            if (hw.UserActions != null)
            {
                foreach (User_action action in hw.UserActions)
                {
                    buttonActionsList.Items.Add(String.Format("{0}:{1} - {2}", action.ep.Address, action.ep.Port, action.raw_sql), action.Enabled);
                }
            }

            contextMenuList.Items.Clear();
            if (hw.actions != null)
            {
                foreach (WebPage_action uri in hw.actions)
                {
                    contextMenuList.Items.Add(String.Format("{0} -> {1}", uri.desc, uri.URL), uri.Enabled);
                }
            }
            panel1.Enabled = true;
            isUserUpdate = true;
        }

        private void NumericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;

            if (isUserUpdate)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
                tsp_node.rect.X = (int)control.Value;
                m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
            }
        }

        private void NumericUpDownSX_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;

            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            tsp_node.rect.Width = (int)control.Value;
            if (isUserUpdate)
            {
                m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
            }
            Form4 frm4 = Form4.Instance;
            frm4.DefaultButtonSize = new Size(tsp_node.rect.Width, tsp_node.rect.Height);
        }

        private void NumericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;

            if (isUserUpdate)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
                tsp_node.rect.Y = (int)control.Value;
                m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
            }
        }

        private void NumericUpDownSY_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;

            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            tsp_node.rect.Height = (int)control.Value;
            if (isUserUpdate)
            {
                m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
            }
            Form4 frm4 = Form4.Instance;
            frm4.DefaultButtonSize = new Size(tsp_node.rect.Width, tsp_node.rect.Height);
        }

        private void IsEnabled_CheckStateChanged(object sender, EventArgs e)
        {
            if (!isUserUpdate) return;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            tsp_node.Enabled = (sender as CheckBox).Checked;
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!isUserUpdate) return;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            tsp_node.text = (sender as TextBox).Text;
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void hardwareIdList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUserUpdate) return;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            ComboBox control = sender as ComboBox;
            if (control.SelectedIndex == 0)
            {
                tsp_node.id = -2;
                tsp_node.isActive = false;
                tsp_node.isSystem = true;
            }
            else if (control.SelectedIndex == 1)
            {
                tsp_node.id = -1;
                tsp_node.isActive = false;
                tsp_node.isSystem = false;
            }
            else
            {
                Hw_id hw = m_xml.Db.m_hw_list[control.SelectedIndex - 2];
                tsp_node.id = hw.id;
                tsp_node.isSystem = false;
                tsp_node.isActive = true;
            }
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void buttonActionsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUserUpdate) return;
            CheckedListBox control = sender as CheckedListBox;
            int idx = control.SelectedIndex;
            if (idx >= 0)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
                textBox2.Text = tsp_node.UserActions[idx].ep.Address.ToString();
                numericUpDown1.Value = tsp_node.UserActions[idx].ep.Port;
                textBox3.Text = tsp_node.UserActions[idx].raw_sql;
            }
            button4.Enabled = (idx >= 0);
        }

        private void buttonActionsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!isUserUpdate) return;
            CheckedListBox control = sender as CheckedListBox;
            int idx = control.SelectedIndex;
            if (idx >= 0)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
                User_action action = tsp_node.UserActions[idx];
                action.Enabled = (e.NewValue == CheckState.Checked) ? true : false;
                tsp_node.UserActions[idx] = action;
                m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
            }
        }

        private void contextMenuList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUserUpdate) return;
            CheckedListBox control = sender as CheckedListBox;
            int idx = control.SelectedIndex;
            if (idx >= 0)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
                textBox5.Text = tsp_node.actions[idx].URL;
                textBox4.Text = tsp_node.actions[idx].desc;
            }
            button9.Enabled = (idx >= 0);
        }

        private void contextMenuList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!isUserUpdate) return;
            CheckedListBox control = sender as CheckedListBox;
            int idx = control.SelectedIndex;
            if (idx >= 0)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
                WebPage_action action = tsp_node.actions[idx];
                action.Enabled = (e.NewValue == CheckState.Checked) ? true : false;
                tsp_node.actions[idx] = action;
                m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            User_action action = new User_action()
            {
                Enabled = true,
                ep = new IPEndPoint(IPAddress.Parse(textBox2.Text), (int)numericUpDown1.Value),
                raw_sql = textBox3.Text,
            };

            tsp_node.UserActions.Add(action);
            //buttonActionsList.Items.Add(String.Format("{0}:{1} - {2}", action.ep.Address, action.ep.Port, action.raw_sql), true);
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            User_action action = new User_action()
            {
                Enabled = true,
                ep = new IPEndPoint(IPAddress.Parse(textBox2.Text), (int)numericUpDown1.Value),
                raw_sql = textBox3.Text,
            };

            tsp_node.UserActions[buttonActionsList.SelectedIndex] = action;
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            tsp_node.UserActions.RemoveAt(buttonActionsList.SelectedIndex);
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            WebPage_action action = new WebPage_action()
            {
                Enabled = true,
                desc = textBox4.Text,
                URL = textBox5.Text,
            };

            tsp_node.actions.Add(action);
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            WebPage_action action = new WebPage_action()
            {
                Enabled = true,
                desc = textBox4.Text,
                URL = textBox5.Text,
            };

            tsp_node.actions[contextMenuList.SelectedIndex] = action;
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[m_button.tsp_index];
            tsp_node.actions.RemoveAt(contextMenuList.SelectedIndex);
            m_xml.UpdateTspItem(m_button.tsp_index, tsp_node);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            validate();
        }

        private void validate()
        {
            bool valid = false;
            if ((textBox3.Text.Length > 0) && (textBox2.Text.Length > 6))
            {
                IPAddress address;
                if (IPAddress.TryParse(textBox2.Text, out address))
                {
                    valid = true;
                }
            }

            button3.Enabled = valid;
            button5.Enabled = (buttonActionsList.SelectedIndex >= 0);
        }

        private void validate2()
        {
            bool valid = false;
            if ((textBox4.Text.Length > 0) && (textBox5.Text.Length > 0))
            {
                valid = true;
            }

            button1.Enabled = valid;
            button2.Enabled = (contextMenuList.SelectedIndex >= 0);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            validate();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            validate2();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            validate2();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Image icon = Image.FromFile(openFileDialog1.FileName);

                    int idx = m_button.tsp_index;
                    var x = m_xml.Tsp_hw_nodes[idx];
                    x.img_main = icon;
                    x.img_main_fname = openFileDialog1.FileName;
                    m_xml.Tsp_hw_nodes[idx] = x;
                    m_xml.UpdateTspItem(idx, x);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Error! Unable to open bitmap...", MessageBoxButtons.OK);
                }
            }
        }

    }
}
