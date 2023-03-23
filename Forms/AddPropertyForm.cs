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
    public partial class AddPropertyForm : Form
    {
        public AddPropertyForm()
        {
            InitializeComponent();
        }

        private void AddPropertyForm_Load(object sender, EventArgs e)
        {
            
            var propertyNameColumn = MainFormController._form.dataGridView1.Columns[0];
            var properties = MainFormController._context.PropertyTypes.ToList();
            for(int i = 0; i < MainFormController._form.dataGridView1.Rows.Count - 1; i++)
            {
                string propertyName = MainFormController._form.dataGridView1[0, i].Value.ToString();
                var prop = properties.Where(p => p.Name == propertyName).FirstOrDefault();
                properties.Remove(prop);

            }
            foreach(var prop in properties)
            {
                var name = prop.Name;
                listBox1.Items.Add(name);

            }
            this.listBox1.Sorted = true;
            this.listBox2.Sorted = true;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox1.IndexFromPoint(e.Location);
            var name = listBox1.Items[index];
            listBox2.Items.Add(name);
            listBox1.Items.RemoveAt(index);
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox1.IndexFromPoint(e.Location);
            var name = listBox1.Items[index];
            listBox1.Items.Add(name);
            listBox2.Items.RemoveAt(index);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for(int i = 0; i< listBox2.Items.Count; i++)
            {
                MainFormController.tableOilInfo.Rows.Add();
                MainFormController._form.dataGridView1[0, MainFormController._form.dataGridView1.Rows.Count - 1].Value = listBox2.Items[i];

                var propertyType = MainFormController._context.PropertyTypes.Where(p => p.Name == listBox2.Items[i].ToString()).FirstOrDefault();
                var uomSet = MainFormController._context.PropertyGroups.Where(p => p.PropertyGroupId == propertyType.PropertyGroupId).FirstOrDefault().Uomset;
                var uom = MainFormController._context.Uoms.Where(p => p.Uomset == uomSet).FirstOrDefault();
                //создание выадающего списка ед.изм.
                if (uom != null)
                {
                    
                    MainFormController._form.dataGridView1[1, MainFormController._form.dataGridView1.Rows.Count - 1].Value = uom.Name;
                    var data2 = MainFormController._context.PropertyTypes
                                    .Where(p => p.Name == MainFormController._form.dataGridView1["Свойство", MainFormController._form.dataGridView1.Rows.Count - 1].Value.ToString()).FirstOrDefault().PropertyGroupId;
                    var data = MainFormController._context.PropertyGroups.Where(p => p.PropertyGroupId == data2).FirstOrDefault().Uomset;
                    if (data == null)
                    {
                        continue;
                    }
                    var data3 = MainFormController._context.Uoms.Where(p => p.Uomset == data).Select(p => p.Name).ToList();
                    DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
                    //cell.Items.AddRange(data);
                    DataGridViewComboBoxCell a = (DataGridViewComboBoxCell)MainFormController._form.dataGridView1["Единицы Измерения", MainFormController._form.dataGridView1.Rows.Count -1];
                    //foreach(var b in data)
                    //{
                    //    a.Items.Add(b.Name);
                    //}
                    a.DataSource = data3;
                    int index = a.Items.IndexOf(MainFormController._form.dataGridView1["Ед. изм.", MainFormController._form.dataGridView1.Rows.Count - 1].Value.ToString());
                    //TODO содержание всякой шняги поменять с процентной доли на массовое количество
                    if (index == -1)
                    {
                        List<string> list = new List<string>();
                        list.Add(MainFormController._form.dataGridView1["Ед. изм.", MainFormController._form.dataGridView1.Rows.Count - 1].Value.ToString());
                        a.DataSource = list;
                        a.Value = MainFormController._form.dataGridView1["Ед. изм.", MainFormController._form.dataGridView1.Rows.Count - 1].Value.ToString();
                    }
                    else
                    {
                        a.Value = a.Items[index];
                    }
                }
                //
                this.Close();

            }
            //foreach (var item in listBox2.Items)
            //{
            //    MainFormController.tableOilInfo.Rows.Add();
            //    var a = MainFormController._form.dataGridView1[0, MainFormController._form.dataGridView1.Rows.Count - 1].Value;
            //    MainFormController._form.dataGridView1[0, MainFormController._form.dataGridView1.Rows.Count - 1].Value = item;

            //    var propertyType = MainFormController._context.PropertyTypes.Where(p => p.Name == item.ToString()).FirstOrDefault();
            //    var uomSet = MainFormController._context.PropertyGroups.Where(p => p.PropertyGroupId == propertyType.PropertyGroupId).FirstOrDefault().Uomset;
            //    var uom = MainFormController._context.Uoms.Where(p => p.Uomset == uomSet).FirstOrDefault();
            //    if (uom!= null)
            //    {
            //        MainFormController._form.dataGridView1[1, MainFormController._form.dataGridView1.Rows.Count - 1].Value = uom.Name;

            //    }
            //    this.Close();

            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<object> selectedItems = new List<object>();
            foreach(object item in this.listBox1.SelectedItems)
            {
                selectedItems.Add(item);
            }
            
            foreach (var item in selectedItems)
            {
                this.listBox2.Items.Add(item.ToString());
                this.listBox1.Items.Remove(item.ToString());

            }
            this.listBox1.Sorted = true;
            this.listBox2.Sorted = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<object> selectedItems = new List<object>();
            foreach (object item in this.listBox2.SelectedItems)
            {
                selectedItems.Add(item);
            }

            foreach (var item in selectedItems)
            {
                this.listBox1.Items.Add(item.ToString());
                this.listBox2.Items.Remove(item.ToString());

            }
            this.listBox1.Sorted = true;
            this.listBox2.Sorted = true;
        }
    }
}
