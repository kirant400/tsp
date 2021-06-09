using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace TechnicalSupervisor
{
    public partial class Form5 : Form
    {
        LogChangedEventHandler m_delegate = null;
        public Form5()
        {
            InitializeComponent();

            m_delegate = new LogChangedEventHandler(log_Changed);
            LogInterface log = LogInterface.Instance;
            log.Changed += m_delegate;

            m_update_pending = new List<LogMessage>();
        }

        ~Form5()
        {
            if (m_delegate != null)
            {
                LogInterface log = LogInterface.Instance;
                log.Changed -= m_delegate;
            }
        }

        List<LogMessage> m_update_pending;
        private void  log_Changed(object sender, LogMessage e)
        {
            append_log(e);
        }

        delegate void UpdateLogsChangedInstance(LogMessage msg);
        private void append_log(LogMessage msg)
            {
            try
                {
                if (this.InvokeRequired)
                    {
                    this.Invoke(new UpdateLogsChangedInstance(append_log), msg);
                    }
                else
                    {
                    if (isLive > 1)
                        {
                        m_update_pending.Add(msg);
                        timer2.Enabled = true;
                        }
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void timer2_Tick(object sender, EventArgs e)
            {
            try
                {
                timer2.Enabled = false;
                if (m_update_pending.Count > 0)
                    {
                    listBox1.BeginUpdate();
                    int last_index = listBox1.TopIndex;
                    foreach (var item in m_update_pending.AsEnumerable().Reverse())
                        {
                        if (!checkBox1.Checked && item.type == LogMessage.LogMessageTypes.Error) continue;
                        if (!checkBox2.Checked && item.type == LogMessage.LogMessageTypes.Warning) continue;
                        if (!checkBox3.Checked && item.type == LogMessage.LogMessageTypes.Info) continue;
                        UpdateLogs(item);
                        }
                    listBox1.TopIndex = last_index + m_update_pending.Count();
                    listBox1.EndUpdate();
                    m_update_pending.Clear();
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
            {
            try
                {
                if (backgroundWorker1.IsBusy)
                    {
                    if (backgroundWorker1.CancellationPending)
                        {
                        return; // Ignore
                        }
                    backgroundWorker1.CancelAsync(); // Cancel previous sequence
                    return;
                    }
                timer1.Enabled = false; // Disable at the end
                listBox1.Items.Clear();
                toolStripStatusLabel1.Text = "Initialising. Please wait...";
                toolStripProgressBar1.Enabled = true;
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                isLive = dateTimePicker1.Value.Date.Equals(DateTime.Now.Date) ? 1 : 0; //CRR issue #18 Use just local time
                backgroundWorker1.RunWorkerAsync(dateTimePicker1.Value);
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
            {
            try
                {
                DateTime tstamp = (DateTime)e.Argument;
                LogInterface log = LogInterface.Instance;
                List<LogMessage> log_items = new List<LogMessage>(2500); // Cache 16 at a time
                foreach (LogMessage log_item in log.GetAllLogs(tstamp))
                    {
                    if (backgroundWorker1.CancellationPending)
                        {
                        e.Cancel = true;
                        break;
                        }
                    log_items.Add(log_item);
                    if (log_items.Count == log_items.Capacity)
                        {
                        UpdateLogs(log_items);
                        log_items.Clear();
                        }
                    }
                if (log_items.Count > 0)
                    {
                    UpdateLogs(log_items);
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                toolStripStatusLabel1.Text = "Cancelled";
            }
            else
            {
                toolStripStatusLabel1.Text = "Ready";
            }
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
        }

        public delegate void UpdateLogsInstance(List<LogMessage> items);

        int isLive = 0; // False
        private void UpdateLogs(List<LogMessage> items)
            {
            try
                {
                if (listBox1.InvokeRequired)
                    {
                    listBox1.Invoke(new UpdateLogsInstance(UpdateLogs), items);
                    }
                else
                    {
                    if (!checkBox1.Checked)
                        {
                        items = items.FindAll(x => x.type != LogMessage.LogMessageTypes.Error);
                        }
                    if (!checkBox2.Checked)
                        {
                        items = items.FindAll(x => x.type != LogMessage.LogMessageTypes.Warning);
                        }
                    if (!checkBox3.Checked)
                        {
                        items = items.FindAll(x => x.type != LogMessage.LogMessageTypes.Info);
                        }
                    listBox1.BeginUpdate();
                    listBox1.Items.AddRange(items.FindAll(x => x.type != LogMessage.LogMessageTypes.Error).ToArray());
                    listBox1.EndUpdate();
                    if (isLive > 0) isLive = 2;
                    }
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        public delegate void UpdateLogItemInstance(LogMessage item);
        private void UpdateLogs(LogMessage item)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new UpdateLogItemInstance(UpdateLogs), item);
            }
            else
            {
                listBox1.Items.Insert(0, item); // Append to the top of the list
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            // listBox1.Items[0].Enabled = false;
            //listBox1.Invalidate();
            //UpdateLogs();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            //listBox1.Invalidate();
            //UpdateLogs();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            //listBox1.Invalidate();
            //UpdateLogs();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
            {
            try
                {
                if (e.Index < 0) return;

                Brush forecolour = new SolidBrush(e.ForeColor);
                Brush backcolour = new SolidBrush(e.BackColor);

                e.DrawBackground();
                LogMessage lc = (LogMessage)listBox1.Items[e.Index];
                if (lc.type == LogMessage.LogMessageTypes.Error)
                    {
                    forecolour = new SolidBrush(Properties.Settings.Default.errorColour);
                    }
                else if (lc.type == LogMessage.LogMessageTypes.Warning)
                    {
                    forecolour = new SolidBrush(Properties.Settings.Default.warnColour);
                    }
                else if (lc.type == LogMessage.LogMessageTypes.Info)
                    {
                    forecolour = new SolidBrush(Properties.Settings.Default.userColour);
                    }

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                    backcolour = forecolour;
                    forecolour = Brushes.White;
                    }
                e.Graphics.FillRectangle(backcolour, e.Bounds);
                e.Graphics.DrawString(lc.tstamp.ToString() + "\t" + lc.type.ToString() + "! " + lc.msg, e.Font, forecolour, new PointF(e.Bounds.X, e.Bounds.Y));
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("EXCEPTION " + Ex.Message + " " + Ex.StackTrace);
                }
            }

        private void Form5_Load(object sender, EventArgs e)
        {

        }
    }

}
