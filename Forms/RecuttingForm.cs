using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SM_Oil.Controllers;

namespace SM_Oil.Forms
{
    public partial class RecuttingForm : Form
    {
        string volumeUOM;
        string tempUOM;
        public RecuttingForm()
        {
            InitializeComponent();

            DataTable oldRecut = new DataTable();


            label1.Text = MainFormController.selectedCrudeInfo.crude.Name;
            
            List<double?> volume = GetRowData("Выход фракции");
            List<double?> temperatures = GetRowData("Конец кипения");
            List<double> volumeWithoutEmp = GetRowDataWithoutEmpties("Выход фракции");
            List<double> temperaturesWithoutEmp = GetRowDataWithoutEmpties("Конец кипения");
            SM_Oil.Controllers.InterpolationFunctions.CubicInterpolation(temperaturesWithoutEmp, volumeWithoutEmp);
            oldRecut.Columns.Add($"Температура, {tempUOM}",typeof(double));
            oldRecut.Columns.Add($"Выход, {volumeUOM}", typeof(double));

            double sum = 0;
            for (int i = 0; i < volume.Count; i++)
            {
                sum += volume[i].Value;
                oldRecut.Rows.Add(temperatures[i], sum);
            }

            dataGridView1.DataSource = oldRecut;
            dataGridView1.Columns[1].ValueType = System.Type.GetType("System.Double");
            dataGridView1.Columns[1].DefaultCellStyle.Format = "##0.000";

        }
        public List<double?> GetRowData(string name)
        {
            List<double?> result = new List<double?>();
            DataRow ourRow = MainFormController.tableOilInfo.NewRow();
            foreach (DataRow row in MainFormController.tableOilInfo.Rows)
            {
                if (row[0].ToString() == name)
                {
                    ourRow = row;
                }
            }

            if (name == "Выход фракции")
            {
                volumeUOM = ourRow[1].ToString();
            }
            if (name == "Конец кипения")
            {
                tempUOM = ourRow[1].ToString();
            }

            for (int i = 3; i < MainFormController.tableOilInfo.Columns.Count; i++)
            {
                if (ourRow[i].ToString() != "")
                {

                    result.Add(Convert.ToDouble(ourRow[i]));

                }
                else
                {
                    result.Add(null);
                }
            }
            return result;
        }
        public List<double> GetRowDataWithoutEmpties(string name)
        {
            List<double> result = new List<double>();
            DataRow ourRow = MainFormController.tableOilInfo.NewRow();
            foreach (DataRow row in MainFormController.tableOilInfo.Rows)
            {
                if (row[0].ToString() == name)
                {
                    ourRow = row;
                }
            }
            for (int i = 3; i < MainFormController.tableOilInfo.Columns.Count; i++)
            {
                if (ourRow[i].ToString() != "")
                {

                    result.Add(Convert.ToDouble(ourRow[i]));

                }
            }
            return result;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //var control = new InterpolationFunctions();


            var oldX = GetDataList(dataGridView1.Columns[0]);
            var oldY = GetDataList(dataGridView1.Columns[1]);
            var newX = GetDataList(dataGridView2.Columns[0]);
            double maxX = oldX.Max();
            double minX = oldX.Min();
            List<double> newY = new List<double>();
            newX.Sort();
            newX.Remove(0);
            //var curveAngles = GetLinearAngles(oldX, oldY);
            var spline = InterpolationFunctions.CubicInterpolation(oldX, oldY);
            newY = InterpolationFunctions.Recutting(spline, newX);
            //foreach(double? x in newX)
            //{
            //    for(int i = 0; i < oldX.Count-1; i++)
            //    {
            //        if (x >= oldX[i] & x <= oldX[i+1])
            //        {
            //            newY.Add(oldY[i] + (x - oldX[i])* curveAngles[i]);
            //            break;
            //        }
            //    }
            //}
            DataTable newRecut = new DataTable();
            newRecut.Columns.Add("Температура");
            newRecut.Columns.Add("Выход", typeof(double));
            for (int i = 0; i < newY.Count; i++)
            {
                if (newX[i] > maxX || newY[i] > 100)
                {
                    newRecut.Rows.Add(newX[i], 100);
                }
                else if (newX[i] < minX || newY[i] < 0)
                {
                    newRecut.Rows.Add(newX[i], 0);
                }
                else
                {
                    newRecut.Rows.Add(newX[i], newY[i]);
                }
            }

            dataGridView3.DataSource = newRecut;
            dataGridView3.Columns["Выход"].DefaultCellStyle.Format = "##0.000";
        }
        List<double> GetDataList(DataGridViewColumn column)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < column.DataGridView.Rows.Count; i++)
            {
                result.Add(Convert.ToDouble(column.DataGridView[column.Name, i].Value));
            }

            return result;
        }
        List<double?> GetLinearAngles(List<double?> x, List<double?> y)
        {
            List<double?> result = new List<double?>();
            for (int i = 0; i < x.Count - 1; i++)
            {
                result.Add((y[i + 1] - y[i]) / (x[i + 1] - x[i]));
            }
            return result;
        }

        private void dataGridView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void dataGridView2_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (dataGridView1.CurrentCell.ColumnIndex == 0) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }
            }
        }

        private void Column1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar)) && !((e.KeyChar == '-')) && !((e.KeyChar == ',')))
            {
                if (e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
