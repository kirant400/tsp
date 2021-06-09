using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;
using TechnicalSupervisor;

namespace ConfigureTSP
{
    public partial class Form4 : Form
    {
        XmlDatabaseInterface m_xml;

        private static Form4 instance;
        public static Form4 Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Form4();
                }
                return instance;
            }
        }


        public void CloseExisting()
        {
            m_xml = null;
           if (instance != null)
            {
                instance = new Form4(); // Force closure of the current form, and rebuild
            }
        }
        
        private Form4()
        {
            m_xml = XmlDatabaseInterface.Instance;
            m_xml.XmlChanged += Frm3_DatabaseChanged;
            m_xml.XmlItemChanged += M_xml_XmlItemChanged;
            InitializeComponent();

            // Make it small
            pictureBox1.Size = new Size(0, 0);
        }

        private void M_xml_XmlItemChanged(object sender, TspItemEventArgs e)
        {
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[e.Index];
            if ((e.Index < pictureBox1.Controls.Count) && (pictureBox1.Controls.Count == m_xml.Tsp_hw_nodes.Count))
            {
                UserButton editButton = pictureBox1.Controls[e.Index] as UserButton;

                // editButton.tsp_hw_node = tsp_node;
                if (tsp_node.img_main != null)
                {
                    editButton.BackgroundImage = tsp_node.img_main;
                    editButton.BackgroundImageLayout = ImageLayout.Tile;
                    editButton.Text = "";
                }
                else
                {
                    editButton.BackgroundImage = null;
                    editButton.BackgroundImageLayout = ImageLayout.Tile;
                    editButton.Text = tsp_node.text;
                }
                editButton.Location = tsp_node.rect.Location;
                editButton.Size = tsp_node.rect.Size;
                if (tsp_node.id < 0)
                {
                    editButton.hw_id = null;
                }
                else
                {
                    editButton.hw_id = m_xml.Db.GetHwId(tsp_node.id);
                }
                editButton.IsEnabled = tsp_node.Enabled;
            }
            else
            {
                UserButton editButton = new UserButton(e.Index, ref tsp_node);

                // editButton.tsp_hw_node = tsp_node;
                if (tsp_node.img_main != null)
                {
                    editButton.BackgroundImage = tsp_node.img_main;
                    editButton.BackgroundImageLayout = ImageLayout.Tile;
                    editButton.Text = "";
                }
                else
                {
                    editButton.Text = tsp_node.text;
                }
                editButton.Location = tsp_node.rect.Location;
                editButton.Size = tsp_node.rect.Size;
                if (tsp_node.id < 0)
                {
                    editButton.hw_id = null;
                }
                else
                {
                    editButton.hw_id = m_xml.Db.GetHwId(tsp_node.id);
                }
                editButton.IsEnabled = tsp_node.Enabled;
                UpdateButton(ref editButton);

                pictureBox1.Controls.Add(editButton);
            }
        }

        void UpdateBackgroundImage()
        {
            // Set TSP image
            panel4.AutoScrollPosition = new Point(0, 0);
            pictureBox1.Dock = DockStyle.None;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.Image = m_xml.Bg_image;

            // Reset background colour
            panel4.BackColor = m_xml.Background;

            // Turn on scroll bars if reqd
            panel4.PerformLayout();
        }

        private void Frm3_DatabaseChanged(object sender, EventArgs e)
        {
            UpdateBackgroundImage();

            if (m_xml.Tsp_hw_nodes != null)
            {
                int max_count = pictureBox1.Controls.Count;
                for (int i = 0; i<max_count; i++)
                {
                    RemoveUserButton(pictureBox1.Controls[0] as UserButton);
                }
                for (int i=0; i< m_xml.Tsp_hw_nodes.Count; i++)
                // foreach (XmlDatabaseInterface.Tsp_hw_node hw_node in m_xml.Tsp_hw_nodes)
                {
                    Tsp_hw_node hw_node = m_xml.Tsp_hw_nodes[i];
                    Hw_id hw_id = m_xml.Db.GetHwId(hw_node.id);
                    UserButton button;
                    if (hw_id == null)
                    {
                        button = new UserButton(i, ref hw_node);
                    }
                    else
                    {
                        button = new UserButton(i, hw_id, ref hw_node);
                    }
                    UpdateButton(ref button);
                    button.Size = hw_node.rect.Size;
                    pictureBox1.Controls.Add(button);
                }
            }

            //m_xml.Db = frm3.m_xml.Db; // Copy a reference of the changed database
        }

        public Size DefaultButtonSize
        {
            get { return defaultButtonSize;  }
            set { defaultButtonSize = value; }
        }
        Size defaultButtonSize = new Size(100, 100);
        private void UpdateButton(ref UserButton newButton)
        {
            //newButton.Location = pos;
            //newButton.Size = defaultSize;
            //newButton.MouseDown += new MouseEventHandler(M_button_MouseDown);
            //newButton.MouseMove += new MouseEventHandler(M_button_MouseMove);
            //newButton.MouseUp += new MouseEventHandler(M_button_MouseUp);
            newButton.Click += NewButton_Click;
            //newButton.MouseClick += new MouseEventHandler(M_button_MouseDoubleClick);
            newButton.PreviewKeyDown += NewButton_PreviewKeyDown;
            newButton.KeyDown += NewButton_KeyDown;
            this.KeyPreview = true;
            newButton.ContextMenuStrip = contextMenuStrip2;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            buttonAdjustInProgress = true;
        }

        bool buttonAdjustInProgress = false;
        Point startPoint;
        private void M_button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            Form2 Frm2 = Form2.Instance;

            UserButton thisButton = sender as UserButton;
            Frm2.UpdateForm(thisButton);
            defaultButtonSize = thisButton.Size;

            // Start the timer
            timer1.Enabled = true;
            startPoint.X = -e.Location.X;
            startPoint.Y = -e.Location.Y;
//            startPoint = new Point(thisButton.Location.X, thisButton.Location.Y);
            /*
            // thisButton.Location;

            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            if (buttonAdjustInProgress)
            {
                buttonAdjustInProgress = false;
                Point pos = pictureBox1.PointToClient(Cursor.Position);
                pos.Offset(startPoint);
                m_button.Location = pos;
                Hw_id hw_id = m_button.hw_id;
                m_button.Text = hw_id.hw_name; // Tag.ToString();
            }
            else
            {

                m_button = sender as UserButton;
                m_button.Text = "Drag into position";
                startPoint.X = -e.Location.X;
                startPoint.Y = -e.Location.Y;
                Point pos = pictureBox1.PointToClient(Cursor.Position);
                pos.Offset(startPoint);
                m_button.Location = pos;
                buttonAdjustInProgress = true;
            }
            */
        }

        private void M_button_MouseMove(object sender, MouseEventArgs e)
        {
            // if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            if (buttonAdjustInProgress)
            {
                Point pos = pictureBox1.PointToClient(Cursor.Position);
                pos.Offset(startPoint);
                m_button.Location = pos;
            }
        }

        private void M_button_MouseUp(object sender, MouseEventArgs e)
        {
            timer1.Enabled = false;
            if (buttonAdjustInProgress)
            {

            }
            /*
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            if (buttonAdjustInProgress)
            {
                buttonAdjustInProgress = false;
                Point pos = pictureBox1.PointToClient(Cursor.Position);
                pos.Offset(startPoint);
                m_button.Location = pos;
                Hw_id hw_id = m_button.hw_id;
                m_button.Text = hw_id.hw_name;
            }
            */
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            Form2 Frm2 = Form2.Instance;

            UserButton thisButton = sender as UserButton;
            Frm2.UpdateForm(thisButton);
            defaultButtonSize = thisButton.Size;
        }
        // bool firstClick = false;
        private void M_button_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Form2 Frm2 = Form2.Instance;
            //Frm3 = Form3.Instance;
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            UserButton thisButton = sender as UserButton;
            Frm2.UpdateForm(thisButton);
            defaultButtonSize = thisButton.Size;
        }

        private void NewButton_KeyDown(object sender, KeyEventArgs e)
        {
            Form2 Frm2 = Form2.Instance;
            //Frm3 = Form3.Instance;
            if (Frm2.Button != null)
            {
                int dx = 0;
                int dy = 0;
                int value = (e.Shift) ? 10 : 1;
                if (e.KeyCode == Keys.Up) dy = value;
                if (e.KeyCode == Keys.Down) dy = -value;
                if (e.KeyCode == Keys.Left) dx = -value;
                if (e.KeyCode == Keys.Right) dx = value;
                if (!e.Control)
                {
                    Frm2.numericUpDownX.Value += dx;
                    Frm2.numericUpDownY.Value -= dy;
                }
                else
                {
                    Frm2.numericUpDownSX.Value += dx;
                    Frm2.numericUpDownSY.Value -= dy;
                }
                e.Handled = ((dy != 0) || (dx != 0)) ? true : false;
                return;
            }

            e.Handled = false;
        }

        private void NewButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Up:
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 Frm2 = Form2.Instance;
            //Frm3 = Form3.Instance;
            UserButton thisButton = contextMenuStrip2.SourceControl as UserButton;
            Frm2.UpdateForm(thisButton);
        }


        UserButton m_button = null;
        int unique_id = 1;

        private void RemoveUserButton (UserButton thisButton)
        {
            // Remove all event handlers
            FieldInfo f1 = typeof(Control).GetField("EventClick",
            BindingFlags.Static | BindingFlags.NonPublic);
            object obj = f1.GetValue(thisButton);
            PropertyInfo pi = thisButton.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(thisButton, null);
            list.RemoveHandler(obj, list[obj]);
            pictureBox1.Controls.Remove(thisButton); // remove button
            thisButton.Dispose(); // Dispose of the memory
        }


        private void SelectTSPImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Bitmap Files|*.png;*.bmp|All files|*.*";
            openFileDialog1.DefaultExt = "PNG";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    m_xml.LoadImage(openFileDialog1.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show(m_xml.last_err, "Error! Invalid Bitmap file...");
                    return;
                }

#if false
                // Set TSP image
                pictureBox1.Dock = DockStyle.None;
                pictureBox1.Location = new Point(0, 0);
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox1.Image = m_xml.Bg_image;

                // Set background colour
                panel4.BackColor = m_xml.Background;

                // Turn on scroll bars if reqd
                panel4.PerformLayout();
#endif
            }
        }

        private void ChangeBackgroundColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panel4.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panel4.BackColor = colorDialog1.Color;
            }
        }

        private void ResetBorderColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Press OK to reset border colour back to default, or Cancel to keep existing", "Are you sure?") == System.Windows.Forms.DialogResult.OK)
            {
                panel4.BackColor = m_xml.Background;
            }
        }

        private void deleteToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            UserButton thisButton = contextMenuStrip2.SourceControl as UserButton;
            m_xml.RemoveTspItem(thisButton.tsp_index);
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            Tsp_hw_node tsp_node = new Tsp_hw_node()
            {
                text = "User Button " + unique_id++.ToString(),
                id = -1,
                rect = new Rectangle(me.Location, defaultButtonSize),
                isActive = false,
                Enabled = true,
            };

            m_xml.AddTspItem(tsp_node);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form2 Frm2 = Form2.Instance;
            Frm2.UpdateForm(null); // clear selection
        }

        private void addCustomButtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Point p = new Point(contextMenuStrip1.Left, contextMenuStrip1.Top);
            Point me = pictureBox1.PointToClient(m_last_pos); //  contextMenuStrip1.DisplayRectangle.Location;

            Tsp_hw_node tsp_node = new Tsp_hw_node()
            {
                text = "User Button " + unique_id++.ToString(),
                id = -1,
                rect = new Rectangle(me, defaultButtonSize),
                isActive = false,
                Enabled = true,
            };

            m_xml.AddTspItem(tsp_node);
        }

        Point m_last_pos;
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            m_last_pos = Cursor.Position;
        }

        private void addSystemButtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point me = pictureBox1.PointToClient(m_last_pos); //  contextMenuStrip1.DisplayRectangle.Location;

            Tsp_hw_node tsp_node = new Tsp_hw_node()
            {
                text = "System Button",
                id = -2,
                isSystem = true,
                rect = new Rectangle(me, defaultButtonSize),
                isActive = false,
                Enabled = true,
            };

            m_xml.AddTspItem(tsp_node);
        }

        UserButton m_caller;
        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            UserButton thisButton = contextMenuStrip2.SourceControl as UserButton;
            int idx = thisButton.tsp_index;
            toolStripMenuItem1.Enabled = (m_xml.Tsp_hw_nodes[idx].img_main != null);
            m_caller = thisButton;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // UserButton thisButton = contextMenuStrip2.SourceControl as UserButton;
            int idx = m_caller.tsp_index;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[idx];
            tsp_node.rect.Height = tsp_node.img_main.Height;
            tsp_node.rect.Width = tsp_node.img_main.Width;
            m_xml.UpdateTspItem(idx, tsp_node);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // UserButton thisButton = contextMenuStrip2.SourceControl as UserButton;
            int idx = m_caller.tsp_index;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[idx];
            tsp_node.img_main = ResizeImage(Image.FromFile(tsp_node.img_main_fname), m_caller.Width, m_caller.Height);
            m_xml.UpdateTspItem(idx, tsp_node);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // UserButton thisButton = contextMenuStrip2.SourceControl as UserButton;
            int idx = m_caller.tsp_index;
            Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[idx];
            tsp_node.img_main = null;
            tsp_node.img_main_fname = "";
            m_xml.Tsp_hw_nodes[idx] = tsp_node;
            m_xml.UpdateTspItem(idx, tsp_node);
        }

    }

    public class UserButton : Button
    {
        bool isUserButton = false;
        public bool IsUserButton
        {
            set { isUserButton = value; }
            get { return isUserButton; }
        }
        bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                int alpha = isEnabled ? 255 : 0;
                BackColor = Color.FromArgb(alpha, BackColor);
            }
        }
        public int tsp_index; // index to the xml database node list
        // public Tsp_hw_node tsp_hw_node; // Reference to the xml hardware node used to create it
        public Hw_id hw_id; // Reference to the database node used to create it
        public UserButton(int idx, ref Tsp_hw_node tsp_node)
        {
            tsp_index = idx;
            // tsp_hw_node = tsp_node;
            isUserButton = !tsp_node.isActive;
            Size = tsp_node.rect.Size;
            Location = tsp_node.rect.Location;
            if (tsp_node.img_main != null)
            {
                BackgroundImage = tsp_node.img_main;
                BackgroundImageLayout = ImageLayout.Tile;
            } else {
                Text = tsp_node.text;
            }
            FlatStyle = FlatStyle.Flat;
        }

        public UserButton(int idx,  Hw_id id, ref Tsp_hw_node tsp_node) : this(idx, ref tsp_node)
        {
            hw_id = id;
            //Text = id.hw_name;
            //FlatStyle = FlatStyle.Flat;
            //isUserButton = false;
        }
#if false
        public UserButton(UserButton button, XmlDatabaseInterface.Tsp_hw_node tsp_node) : this(button.hw_id, tsp_node)
        {
            Text = button.Text;
            Location = button.Location;
            FlatStyle = button.FlatStyle;
            IsUserButton = button.IsUserButton;
            Size = button.Size;
#if false
            PropertyInfo[] controlProperties = typeof(UserButton).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propInfo in controlProperties)
            {
                if (propInfo.CanWrite)
                {
                    if (propInfo.Name != "WindowTarget")
                        propInfo.SetValue(this, propInfo.GetValue(button, null), null);
                }
            }

#endif
        }
#endif

    }

}
