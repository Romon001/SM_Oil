using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SM_Oil.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Form1(DataTable table)
        {
            InitializeComponent();

            dataGridView1.DataSource = table;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.Width = 50;
            }
            dataGridView1.Columns[0].Width = 100;
        }
    }
}
