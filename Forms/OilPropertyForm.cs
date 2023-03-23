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
    public partial class OilPropertyForm : Form
    {
        public OilPropertyForm()
        {
            InitializeComponent();
        }
        public OilPropertyForm(DataTable propertyTable)
        {
            InitializeComponent();
            dataGridView1.DataSource = propertyTable;
            dataGridView1.ColumnHeadersHeight = 30;
            dataGridView1.Columns[0].Width = 100;
            for(int i=1;i<dataGridView1.Columns.Count;i++)
            {
                dataGridView1.Columns[i].Width = 60;
            }
        }
    }
}
