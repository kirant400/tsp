using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TechnicalSupervisor;

namespace ConfigureTSP
{
    public partial class Form5 : Form
    {
        XmlDatabaseInterface m_xml;
        List<Point> origins;
        public Form5()
        {
            InitializeComponent();
            m_xml = XmlDatabaseInterface.Instance;

            origins = m_xml.Tsp_hw_nodes.ConvertAll(x => new Point(x.rect.X, x.rect.Y));
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            AdjustAll((int)numericUpDown1.Value, (int)numericUpDown2.Value);
        }

        void AdjustAll(int x = 0, int y = 0)
        {
            for (int i = 0; i < m_xml.Tsp_hw_nodes.Count; i++)
            {
                Tsp_hw_node tsp_node = m_xml.Tsp_hw_nodes[i];
                tsp_node.rect.X = origins[i].X + x;
                tsp_node.rect.Y = origins[i].Y + y;
                m_xml.UpdateTspItem(i, tsp_node);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdjustAll(); // Reset adjustments
        }

    }
}
