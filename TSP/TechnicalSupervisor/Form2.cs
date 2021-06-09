using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TechnicalSupervisor
{
    public partial class Form2 : Form
    {
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
                    }
                    else
                    {
                        Hide();
                    }
                    m_showForm = value;
                }
                BringToFront();
            }
            get
            {
                return m_showForm;
            }
        }

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
            InitializeComponent();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Disable close buttons - hide instead!! ;-)
            ShowForm = false;
            e.Cancel = true;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {

            }
        }
}
