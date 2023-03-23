using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using SM_Oil.Controllers;
using SM_Oil.Forms;

namespace SM_Oil
{
    public partial class MainForm : Form
    {
        public dbSMOilContext _context;
        MainFormController _controller { get; set; }

        ListViewItem rightClickItem { get; set; }
        public MainForm()
        {
            InitializeComponent();
            _context = new dbSMOilContext();
            _controller = new MainFormController(this);
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = MainFormController.tableOilInfo;

            
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            _controller.FillOilTree(treeView1);
            _controller.FillPrimaryLibraryList(listView1);
            tabControl1.SelectedTab = tabPage1;
            //MainFormController.testFUnc();
        }

        //Удаленный объект дерево
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var senderList = (TreeView)sender;
            TreeNode clickedNode = senderList.HitTest(e.Location).Node;
            
            //if (clickedNode.Nodes.Count == 0)
            //{
            //    var clickedCrude = _context.Crudes.Where(p => p.Name == clickedNode.Text).FirstOrDefault();
            //    var cutSet = _context.CutSets.Where(p => p.CrudeId == clickedCrude.CrudeId).FirstOrDefault();
            //    var oilProperties = from val in _context.Properties.Where(p => p.CutSet == cutSet.CutSetId)
            //                            join name in _context.PropertyTypes on val.PropertyType equals name.PropertyTypeId
            //                            select new
            //                            {
            //                                Value = val.Value,
            //                                PropertyName = name.Name,
            //                                Order = val.Order,
            //                                PropertyType = val.PropertyType
            //                            };
            //    var propertiesDictionary = _context.PropertyTypes.ToList();
            //    DataTable allCrudePropertiesTable = new DataTable();
            //    allCrudePropertiesTable.Columns.Add("Свойство");
            //    int numberOfCuts= oilProperties.Max(p => p.Order);

            //    for(int i =0; i<numberOfCuts; i++)
            //    {
            //        allCrudePropertiesTable.Columns.Add($"Фракц. №{i+1}");
            //    }
            //    foreach (var prop in propertiesDictionary)
            //    {
            //        allCrudePropertiesTable.Rows.Add();
            //        //allCrudeProperties.Rows[-1]["Property Name"] = prop.Name;
            //    }
            //    foreach(var prop in oilProperties)
            //    {
            //        allCrudePropertiesTable.Rows[Convert.ToInt32(prop.PropertyType)][prop.Order] = prop.Value;
            //        allCrudePropertiesTable.Rows[Convert.ToInt32(prop.PropertyType)]["Свойство"] = prop.PropertyName;
            //    }
            //    for (int i = allCrudePropertiesTable.Rows.Count - 1; i >= 0; i--)
            //    {
            //        DataRow row = allCrudePropertiesTable.Rows[i];
            //        if (row["Свойство"].ToString() == "")
            //            row.Delete();
            //    }
            //    var form = new Form1(allCrudePropertiesTable);
            //    form.Show();
            }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListView senderList = (ListView)sender;
                ListViewItem clickedItem = senderList.HitTest(e.Location).Item;
                _controller.OpenNodeContent(clickedItem);
            }
        }

        private void undoTreeButton_Click(object sender, EventArgs e)
        {
            _controller.Undo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _controller.OpenNewCrudeForm();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                var crudeIsExist = _context.Crudes.Where(p => MainFormController.selectedCrudeInfo.crude.CrudeId == p.CrudeId).FirstOrDefault();
                if (crudeIsExist != null)
                {
                    var window = MessageBox.Show(
                    "Сохранить внесенные изменения?",
                    "Сохранение данных",
                    MessageBoxButtons.YesNo);
                    if (window == DialogResult.Yes)
                    {
                        _controller.SaveChanges();
                    }
                    if (window == DialogResult.No)
                    {

                    }

                }
                else
                {
                    var window = MessageBox.Show(
                    $"Создать новую нефть '{MainFormController.selectedCrudeInfo.crude.Name}'?",
                    "Добавление новой нефти в бд",
                    MessageBoxButtons.YesNo);
                    if (window == DialogResult.Yes)
                    {
                        _controller.AddNewOilToDataBase();
                    }
                    if (window == DialogResult.No)
                    {

                    }
                }
            }
            catch
            {

            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (MainFormController.selectedCrudeInfo.crude == null)
            {
                return;
            }   
            string cutName = dataGridView1.Columns[e.ColumnIndex].HeaderText;
            if (cutName== "Свойство" || cutName == "Ед. изм.")
            {
                return;
            }
            //TODO избавиться от индексов колонок в сторону названий
            string propertyType = dataGridView1["Свойство", e.RowIndex].Value.ToString();
            int propertyTypeId = _context.PropertyTypes
                                        .Where(p => p.Name == propertyType).FirstOrDefault().PropertyTypeId;
            if (cutName == "Единицы Измерения")
            {
                var newUom = _context.Uoms
                                .Where(p => p.Name == dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString()).FirstOrDefault().Uomid;
                var properties = MainFormController.selectedCrudeInfo.selectedCutSetProperties
                                        .Where(p => p.PropertyTypeId == propertyTypeId).ToList();
                foreach (var prop in properties)
                {
                    prop.Uom = newUom;
                }
                dataGridView1["Ед. изм.", e.RowIndex].Value = dataGridView1[e.ColumnIndex, e.RowIndex].Value;
                return;
            }            
            try
            {
                if(dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString() != "")
                {
                    Convert.ToDouble(dataGridView1[e.ColumnIndex, e.RowIndex].Value);

                }
            }
            catch
            {
                MessageBox.Show("Некорректные данные  в ячейке","Ошибка",MessageBoxButtons.OK);
            }
            int index = MainFormController.selectedCrudeInfo.selectedCutSetProperties
                                .FindIndex(p => p.PropertyTypeId == propertyTypeId && p.CutName == cutName);
            
            if (index != -1)
            {
                if (dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString() != "")
                {
                    MainFormController.selectedCrudeInfo.selectedCutSetProperties[index].Value = Convert.ToDouble(dataGridView1[e.ColumnIndex, e.RowIndex].Value);

                }
                else
                {
                    //Первый вариант удаления ячейки
                    MainFormController.selectedCrudeInfo.selectedCutSetProperties[index].Value = -9999;

                }
            }
            else
            {
                var newProperty = new Property();
                newProperty.Value = Convert.ToDouble(dataGridView1[e.ColumnIndex, e.RowIndex].Value);

                newProperty.CutName = cutName;
                newProperty.CutId = MainFormController.selectedCrudeInfo.selectedCutSetCuts[e.ColumnIndex - 2].CutId;
                newProperty.PropertyTypeId = propertyTypeId;
                if (dataGridView1["Ед. изм.", e.RowIndex].Value.ToString() != "")
                {

                    newProperty.Uom = _context.Uoms
                    .Where(p => p.Name == dataGridView1["Ед. изм.", e.RowIndex].Value.ToString())
                    .FirstOrDefault().Uomid;

                }
                else
                {
                    newProperty.Uom = null;

                }
                MainFormController.selectedCrudeInfo.selectedCutSetProperties.Add(newProperty);
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //try
            //{
                var window = MessageBox.Show(
                $"Удалить '{MainFormController.selectedCrudeInfo.crude.Name}'?",
                "Удаление нефти",
                MessageBoxButtons.YesNo);
                if (window == DialogResult.Yes)
                {
                    _controller.DeleteSelectedCrude();
                    var window2 = MessageBox.Show(
                    $"Нефть успешно удалена",
                    "Удаление нефти",
                    MessageBoxButtons.OK);
                }
                if (window == DialogResult.No)
                {

                }
            //}
            //catch
            //{

            //}

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {

                var vaildateConroller = new ValidationFunctions(this);
                vaildateConroller.Validate(MainFormController.selectedCrudeInfo);
                vaildateConroller.SupplementData();
                tabControl1.SelectedTab = tabPage2;
            }
            catch
            {
                MessageBox.Show("Не удалось произвести восстановление. Возможно отсутствуют необходимые данные", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void действияToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //button Save
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var crudeIsExist = _context.Crudes.Where(p => MainFormController.selectedCrudeInfo.crude.CrudeId == p.CrudeId).FirstOrDefault();
                if (crudeIsExist != null)
                {
                    var window = MessageBox.Show(
                    "Сохранить внесенные изменения?",
                    "Сохранение данных",
                    MessageBoxButtons.YesNo);
                    if (window == DialogResult.Yes)
                    {
                        _controller.SaveChanges();
                    }
                    if (window == DialogResult.No)
                    {

                    }

                }
                else
                {
                    var window = MessageBox.Show(
                    $"Создать новую нефть '{MainFormController.selectedCrudeInfo.crude.Name}'?",
                    "Добавление новой нефти в бд",
                    MessageBoxButtons.YesNo);
                    if (window == DialogResult.Yes)
                    {
                        _controller.AddNewOilToDataBase();
                    }
                    if (window == DialogResult.No)
                    {

                    }
                }
            }
            catch
            {

            }
        }

        private void восстановлениеДанныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                var vaildateConroller = new ValidationFunctions(this);
                vaildateConroller.Validate(MainFormController.selectedCrudeInfo);
                vaildateConroller.SupplementData();
                tabControl1.SelectedTab = tabPage2;
            }
            catch
            {
                MessageBox.Show("Не удалось произвести восстановление. Возможно отсутствуют необходимые данные", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var window = MessageBox.Show(
                $"Удалить '{MainFormController.selectedCrudeInfo.crude.Name}'?",
                "Удаление нефти",
                MessageBoxButtons.YesNo);
                if (window == DialogResult.Yes)
                {
                    _controller.DeleteSelectedCrude();
                    var window2 = MessageBox.Show(
                    $"Нефть успешно удалена",
                    "Удаление нефти",
                    MessageBoxButtons.OK);
                }
                if (window == DialogResult.No)
                {

                }
            }
            catch
            {

            }
        }

        private void добавитьСвойствоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainFormController.selectedCrudeInfo.crude != null)
            {
                AddPropertyForm newForm = new AddPropertyForm();
                newForm.Show();

            }
        }

        private void перенарезкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainFormController.selectedCrudeInfo.crude != null)
            {
                try
                {

                    RecuttingForm newForm = new RecuttingForm();
                    newForm.Show();

                }
                catch
                {

                }
                }
        }   

        private void скопироватьТаблицуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.CopyDataFromTable();
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if( e.Context.ToString().Contains("Parsing")) 
            {
                MessageBox.Show("Ошибка", "Неверный формат данных", MessageBoxButtons.OK);
                e.Cancel = true;
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                var focusedItem = listView1.FocusedItem;
                ValueTuple<string, int> tag = (ValueTuple<string, int>)focusedItem.Tag;
                var name = focusedItem.Text;
                var type = tag.Item1;
                var id = tag.Item2;
                if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
                {
                    if (type == "SM_Oil.Crude")
                    {
                        contextMenuStrip1.Show(Cursor.Position);
                        rightClickItem = focusedItem;
                    }
                    
                }
            }
        }

        private void добавитьРазгонкуToolStripMenuItem_Click(object sender, EventArgs e)
        {

            AddCutSetForm newForm = new AddCutSetForm(this,rightClickItem);
            newForm.Show();
        }

        private void удалитьНефтьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ValueTuple<string, int> tag = (ValueTuple<string, int>)rightClickItem.Tag;
                var type = tag.Item1;
                var id = tag.Item2;
                var deletedCrude = _controller.CrudeById(id);


                var window = MessageBox.Show(
                $"Удалить '{deletedCrude.Name}' и все связанные с ней разгонки?",
                "Удаление нефти",
                MessageBoxButtons.YesNo);
                if (window == DialogResult.Yes)
                {
                    _controller.DeleteCrudeById(id);
                    listView1.Items.Remove(rightClickItem);
                    var window2 = MessageBox.Show(
                    $"Нефть успешно удалена",
                    "Удаление нефти",
                    MessageBoxButtons.OK);
                }
                if (window == DialogResult.No)
                {

                }
            }
            catch
            {

            }
        }

        private void новаяНефтьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.OpenNewCrudeForm();
        }

        private void управлениеРазгонкамиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutsManagmentForm newForm = new CutsManagmentForm();
            newForm.Show();
        }
    }
    }
//}
