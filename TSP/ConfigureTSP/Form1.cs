using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TechnicalSupervisor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConfigureTSP
{
    public partial class Form1 : Form
    {
        XmlDatabaseInterface m_xml;
        Form4 frm4;
        Form3 frm3;
        Form2 frm2;
        public Form1()
        {
            InitializeComponent();

            //Guid guid = Guid.NewGuid();
            //, guid.ToString()
            config_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Thruput", "TSP Config Tool", "cache");

            // Create a change handler to update form when the database is changed
            m_xml = XmlDatabaseInterface.Instance; //  new XmlDatabaseInterface();
            m_xml.XmlChanged += new XmlChangedEventHandler(m_xml_XmlChanged);

            // Start up the tool bar
            frm3 = Form3.Instance;
            panel2.AutoSize = true;
            panel2.Controls.Add(frm3.tabControl1);
            frm3.tabControl1.Show();

            // Start up the properties bar
            frm2 = Form2.Instance;
            panel3.AutoSize = true;
            panel3.Controls.Add(frm2.panel1);
            frm2.panel1.Show();

            // Start up the background
            frm4 = Form4.Instance;
            panel4.AutoSize = false;
            panel4.Controls.Add(frm4.panel4);
            // Properties.Settings.Default.Reload();

            SetTitle();
        }

        private void SetTitle(string fname = "[Unsaved]")
        {
            this.Text = string.Format("TSP Configuration Tool -- {0}", fname);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Clean up the settings
            UserSettingsCleanup();

            this.Location = Properties.Settings.Default.FormLocation;
            this.Size = Properties.Settings.Default.FormSize;
            if (Properties.Settings.Default.isMaximised)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        bool first_time = true;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            while (!m_xml.IsSaved && first_time)
            {
                DialogResult reply = MessageBox.Show(this, "Would you like to save before exiting?", "Warning! Some changes have been made to the project..", MessageBoxButtons.YesNoCancel);
                if (reply == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (reply == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveProject(false);
                    continue;
                }

                if (reply == System.Windows.Forms.DialogResult.No)
                {
                    // Otherwise break the loop
                    break;
                }
            }

            /*
            first_time = false;
            if (frm3 != null)
            {
                frm3.Close();
                frm3.Dispose();
                frm3 = null;
                Close();
            }
            */
            UserSettingsSave();
        }

        void UserSettingsCleanup()
        {
            if ((Properties.Settings.Default.FormLocation.X < 0) || (Properties.Settings.Default.FormLocation.Y < 0))
            {
                Properties.Settings.Default.FormLocation = this.RestoreBounds.Location;
                Properties.Settings.Default.FormSize = this.RestoreBounds.Size;
                Properties.Settings.Default.isMaximised = (this.WindowState == FormWindowState.Maximized);
                // Properties.Settings.Default.recentFiles.Clear();
            }

            // Clean up the recent file list (delete bad links and populate the menu)
            int last_idx = -1; //
            if (Properties.Settings.Default.recentFiles != null)
                last_idx = Properties.Settings.Default.recentFiles.Count - 1;
            int idx = 1;
            toolStripMenuItem1.DropDownItems.Clear();
            for (int i = 0; i <= last_idx; i++)
            {
                if (!File.Exists(Properties.Settings.Default.recentFiles[last_idx - i]))
                {
                    Properties.Settings.Default.recentFiles.RemoveAt(last_idx - i);
                    continue;
                }

                ToolStripItem item = new ToolStripMenuItem();
                item.Text = string.Format("{0} {1}", idx++, Properties.Settings.Default.recentFiles[last_idx - i]);
                item.Tag = Properties.Settings.Default.recentFiles[last_idx - i];
                toolStripMenuItem1.DropDownItems.Add(item);
            }
            toolStripMenuItem1.Enabled = (toolStripMenuItem1.DropDownItems.Count > 0);

        }

        void UserSettingsNewFile(string new_file)
        {
            // If file already in list, delete...
            if (Properties.Settings.Default.recentFiles.Contains(new_file))
            {
                Properties.Settings.Default.recentFiles.Remove(new_file);
            }
            // And then re-add to put it at the top of the list (i.e. most recent)
            Properties.Settings.Default.recentFiles.Add(new_file);

            // Clean up the recent file list (max. 10 items)
            while (Properties.Settings.Default.recentFiles.Count > 9)
            {
                Properties.Settings.Default.recentFiles.RemoveAt(0); // Remove the oldest item
            }

            // Re-populate the popup-menu
            UserSettingsCleanup();
            SetTitle(new_file);
        }

        void UserSettingsSave()
        {
            Properties.Settings.Default.FormLocation = this.RestoreBounds.Location;
            Properties.Settings.Default.FormSize = this.RestoreBounds.Size;
            Properties.Settings.Default.isMaximised = (this.WindowState == FormWindowState.Maximized);
            Properties.Settings.Default.Save();
        }

        void m_xml_XmlChanged(object sender, EventArgs e)
        {
            if (m_xml.Font_size > 0)
            {
                frm4.pictureBox1.Font = new Font(this.Font.FontFamily, m_xml.Font_size);
                frm4.pictureBox1.Invalidate();
                frm4.pictureBox1.Refresh();
            }

            if ((m_xml.Img_fname != null) && (m_xml.Db_fname != null))
            {
                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
            }
            else
            {
                saveToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_xml.IsSaved || MessageBox.Show(this, "Press Ok to lose the current work, and open a new XML file.", "Warning! Opening a new XML file will lose the exisiting..", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                OpenExisting();
            }
        }

        private bool OpenExisting(string fname)
        {
            // CloseExisting(); // Delete the current

            // Clean cache folder
            try
            {
                if (Directory.Exists(config_folder))
                {
                    Directory.Delete(config_folder, true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error! Unable to clean cache folder");
                return false;
            }

            if (!m_xml.safe_file_copy(fname, config_folder))
            {
                MessageBox.Show(m_xml.last_err, "Error! Unable to copy XML into cache folder");
                return false;
            }

            if (!m_xml.ExtractXmlResouces(fname, config_folder))
            {
                MessageBox.Show(m_xml.last_err, "Error! Unable to extract resources from XML file");
                return false;
            }

            string local_file = Path.Combine(config_folder, Path.GetFileName(fname));
            if (!m_xml.ParseXml(local_file))
            {
                MessageBox.Show(m_xml.last_err, "Error! Unable to parse XML file");
                return false;
            }

            // Set the save/open filenames to match the file just opened...
            saveFileDialog1.FileName = fname;
            openFileDialog1.FileName = fname;
            UserSettingsNewFile(fname);
            return true;
        }

        string config_folder;
        private bool OpenExisting()
        {
            openFileDialog1.Filter = "XML files|*.xml|All files|*.*";
            openFileDialog1.DefaultExt = "XML";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return OpenExisting(openFileDialog1.FileName);
            }
            return false;
        }

        private void CloseExisting()
        {
#if NOT_YET
            // Remove form4
            panel4.Controls.Remove(frm4.panel4);
            frm4.Close();
            frm4.CloseExisting();

            // And then re-add
            frm4 = Form4.Instance;
            panel4.AutoSize = false;
            panel4.Controls.Add(frm4.panel4);
#endif
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public bool restart_required = false;
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            restart_required = true;
            Close();
        }

        private void ShowHideToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel3.Visible = !panel3.Visible;
            // Visible = !frm3.Visible;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject(false);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Text"))
                return;

            int x = this.PointToClient(new Point(e.X, e.Y)).X;
            int y = this.PointToClient(new Point(e.X, e.Y)).Y;

            if (x >= frm4.pictureBox1.Location.X && x <= frm4.pictureBox1.Location.X + frm4.pictureBox1.Width && y >= frm4.pictureBox1.Location.Y && y <= frm4.pictureBox1.Location.Y + frm4.pictureBox1.Height)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Text"))
                return;

            int x = frm4.pictureBox1.PointToClient(new Point(e.X, e.Y)).X;
            int y = frm4.pictureBox1.PointToClient(new Point(e.X, e.Y)).Y;

            if (x >= frm4.pictureBox1.Location.X && x <= frm4.pictureBox1.Location.X + frm4.pictureBox1.Width && y >= frm4.pictureBox1.Location.Y && y <= frm4.pictureBox1.Location.Y + frm4.pictureBox1.Height)
            {
                string str = (string)e.Data.GetData(DataFormats.Text);
                if (str.StartsWith("node_index="))
                {
                    int idx = int.Parse(str.Split('=')[1]);
                    Hw_id hw_id = m_xml.Db.m_hw_list[idx];
                    Tsp_hw_node tsp_node = new Tsp_hw_node()
                    {
                        text = hw_id.hw_name,
                        Enabled = true,
                        rect = new Rectangle(frm4.pictureBox1.PointToClient(new Point(e.X, e.Y)), frm4.DefaultButtonSize),
                        isActive = true,
                        actions = new List<WebPage_action>(),
                        id = hw_id.id,
                    };

                    //m_xml.Db.m_hw_list
                    //m_xml.Tsp_hw_nodes[idx];
                    m_xml.AddTspItem(tsp_node);
                }

                // string data = e.Data.ToString();
                // m_xml.AddTspItem()
                int i = 0;
                i++;
            }
        }

        void SaveProject(bool save_as = false)
        {
            if (save_as || (saveFileDialog1.FileName == ""))
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    SaveProject();
                }
            }
            else
            {
                if (!m_xml.SaveXml(saveFileDialog1.FileName))
                {
                    MessageBox.Show(this, m_xml.last_err + "\r\nPress OK to continue", "Error! Saving file to disk failed.");
                }
                UserSettingsNewFile(saveFileDialog1.FileName);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject(true);
        }

        private void toolStripMenuItem1_DropDownOpening(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItem1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            OpenExisting(e.ClickedItem.Tag as string);
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            about dlg = new about();
            dlg.ShowDialog();
        }

        private void repositionAllButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form5 dlg = new Form5();
            var reply = dlg.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
            {

            }
        }

}
