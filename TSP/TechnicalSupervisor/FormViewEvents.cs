/**************************************************************************************************
 
	Form to view the events from the TSP either via SNMP or LogClient 

	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		FormViewEvents.cs
	Project:		TSP
	Developer:		CRR 
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			
**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

/// <summary>
/// Display the events collected from the SNMPAgent
/// Primarily developed to aid in debugging
/// </summary>
/// 
namespace TechnicalSupervisor
    {
    public partial class FormViewEvents : Form
        {
        public DataGridView[] DataGridViews;
        public GroupBox[] GroupBoxes;
        public FormViewEvents()
            {
            InitializeComponent();
            }

        private void FormViewEvents_Load(object sender, EventArgs e)
            {
            CreateDataGridViews();                                      // Create the datagridviews one for each NAS
            }
        /// <summary>
        ///  Create the datagridviews one for each NAS
        /// </summary>
        public void CreateDataGridViews()
            {
            int x = 10;
            int y = 10;
            int Height = 300;
            int Width = 1000;
            int Spacing = 20;
            int Margin = 20;
            int NoOfDataGridViews = Program.NoOfLogClients;
            this.DataGridViews = new DataGridView[NoOfDataGridViews];
            this.GroupBoxes = new GroupBox[NoOfDataGridViews];
            for(int i=0;i<NoOfDataGridViews;i++)
                {
                GroupBoxes[i] = new GroupBox();
                GroupBoxes[i].Size = new Size(Width,Height);
                GroupBoxes[i].Location = new Point(x, y);
                GroupBoxes[i].Text = "No Data";
                GroupBoxes[i].Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                GroupBoxes[i].Visible = true;
                DataGridViews[i] = new DataGridView();
                DataGridViews[i].Size = new Size(Width-(Margin*2), Height-(Margin*2));
                DataGridViews[i].Location = new Point(Margin, Margin);
                DataGridViews[i].Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom);
                DataGridViews[i].ReadOnly = true;
                DataGridViews[i].Visible = true;
                GroupBoxes[i].Controls.Add(DataGridViews[i]);
                this.Controls.Add(GroupBoxes[i]);
                y += Height + Spacing;
                }
            this.Size = new Size(Width + (Margin * 2), y+Spacing);
            this.Invalidate();
            }

        public int TimerTickCount = 0;
        /// <summary>
        /// Update the Datagrids with the data collected via SNMP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
            {
            if (TimerTickCount > 5)
                timer1.Interval = 5000;                         // Run faster for the first 5 ticks
            else
                TimerTickCount++;
            int i = 0;
            foreach (LogClient lc in Program.LogClientsList)
                {
                int NoOfEvents = lc.LatestEvents.Count();
                if (NoOfEvents > 0)
                    {
                    ShowSummaryInADataGridView(lc, DataGridViews[i]);
                    GroupBoxes[i].Text = NoOfEvents.ToString() + " events  from " + lc.Hw_name + "(" + lc.Ep.Address.ToString() + ")" + " at " + DateTime.Now.ToString();
                    }
                else
                    {
                    GroupBoxes[i].Text = " No events  from " + lc.Hw_name + "(" + lc.Ep.Address.ToString() + ")" + " at " + DateTime.Now.ToString();
                    }

                ////Debug.WriteLine("LOGCLIENT " + lc.Ep.ToString());
                //if (i == 0)                                                 //TODO - Too Simplistic - Just for Debug ?
                //    {
                //    groupBox1.Text = lc.Ep.ToString() + " at " + DateTime.Now.ToString();
                //    ShowSummaryInADataGridView(lc, dataGridView1);
                //    }
                //else
                //    {
                //    groupBox2.Text = lc.Ep.ToString() + " at " + DateTime.Now.ToString();
                //    ShowSummaryInADataGridView(lc, dataGridView2);
                //    }
                i++;
                }
            }


        //ToDo - Too Verbose - Need a generic version
        public void ShowSummaryInADataGridView(LogClient lc, DataGridView dgv)
            {
            DataTable DT = new DataTable();

            DT.Columns.Add("HWID");
            DT.Columns.Add("Level");
            DT.Columns.Add("No");
            DT.Columns.Add("Time");
            DT.Columns.Add("Channel");
            DT.Columns.Add("EventID");
            DT.Columns.Add("ID");
            DT.Columns.Add("Description");
            //DT.Columns.Add("Maintenance");
            DT.Columns.Add("Time Fetched from NAS");

            foreach (LogClient.LogSummary Summary in lc.LatestEvents)
                {
                List<string> Fields = new List<string>();
                Fields.Add(Summary.hw_id.ToString());
                Fields.Add(Summary.level.ToString());
                Fields.Add(Summary.count.ToString());
                Fields.Add(Summary.tstamp.ToString());
                Fields.Add(Summary.channel.ToString());
                Fields.Add(Summary.eventID.ToString());
                Fields.Add(Summary.guid.ToString());
                Fields.Add(Summary.msg.ToString());
                //Fields.Add(Summary.maintenance.ToString());                 //#48
                if (Summary.LastUpdated == DateTime.MinValue)
                    {
                    Fields.Add(DateTime.Now.ToString());                    // If no date then show current time & date
                    }
                else
                    {
                    Fields.Add(Summary.LastUpdated.ToString());
                    }
                DT.Rows.Add(Fields.ToArray());
                }
            dgv.DataSource = DT;
            }

        }
    }
