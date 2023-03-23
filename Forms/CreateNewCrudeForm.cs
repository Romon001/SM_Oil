using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SM_Oil.Models;
using SM_Oil.Controllers;
namespace SM_Oil.Forms
{
    public partial class CreateNewCrudeForm : Form
    {
        dbSMOilContext _context;
        MainForm _mainForm;
        MainFormController _controller;
        public CreateNewCrudeForm(MainForm mainForm)
        {
            _context = new dbSMOilContext();
            //TODO: Сделвть общий объект mainForm
            _mainForm = mainForm;
            _controller = new MainFormController(mainForm);
            InitializeComponent();
            textBox1.Text = "Новая нефть";

            comboBox1.Items.Add("Пустая разгонка");
            comboBox1.Items.Add("Подбор аналогов");
            comboBox1.Items.Add("Скопировать существующую нефть");
            comboBox1.SelectedIndex = 0;
            

            //Заполнение возможных разгонок
            var cutSetTypes = _context.CutSetTypes.ToList();
            foreach (var cutSet in cutSetTypes)
            {
                comboBox2.Items.Add(cutSet.Name);
            }
            comboBox2.Items.Add("Без разгонки");
            comboBox2.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString()== "Пустая разгонка")
            {
                CreateEmptyCrude();
                _mainForm.dataGridView1.DataSource = null;
                //
                //
                //
                MainFormController._form.dataGridView1.Columns.Clear();
                DataGridViewComboBoxColumn column = new DataGridViewComboBoxColumn();
                column.HeaderText = "Единицы Измерения";
                column.Name = "Единицы Измерения";
                _mainForm.dataGridView1.Columns.Add(column);

                //
                //
                //
                MainFormController._form.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                DataTable oilDataTable = MainFormController.CreateOilPropertyTable(MainFormController.selectedCrudeInfo);
                MainFormController.selectedCrudeInfo.selectedCutSetDataTable = oilDataTable;
                _mainForm.dataGridView1.DataSource = MainFormController.selectedCrudeInfo.selectedCutSetDataTable;

                //Создание выпадающего списка единиц измерения
                for (int i = 0; i < _mainForm.dataGridView1.Rows.Count - 1; i++)
                {
                    //TODO сделать 1 запрос
                    var data2 = _context.PropertyTypes
                                    .Where(p => p.Name == _mainForm.dataGridView1["Свойство", i].Value.ToString()).FirstOrDefault().PropertyGroupId;
                    var data = _context.PropertyGroups.Where(p => p.PropertyGroupId == data2).FirstOrDefault().Uomset;
                    if (data == null)
                    {
                        continue;
                    }
                    var data3 = _context.Uoms.Where(p => p.Uomset == data).Select(p => p.Name).ToList();
                    DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
                    //cell.Items.AddRange(data);
                    DataGridViewComboBoxCell UOMComboBox = (DataGridViewComboBoxCell)_mainForm.dataGridView1["Единицы Измерения", i];
                    //foreach(var b in data)
                    //{
                    //    a.Items.Add(b.Name);
                    //}
                    UOMComboBox.DataSource = data3;
                    int index = UOMComboBox.Items.IndexOf(_mainForm.dataGridView1["Ед. изм.", i].Value.ToString());
                    //TODO содержание всякой шняги поменять с процентной доли на массовое количество
                    if (index == -1)
                    {
                        List<string> list = new List<string>();
                        list.Add(_mainForm.dataGridView1["Ед. изм.", i].Value.ToString());
                        UOMComboBox.DataSource = list;
                        UOMComboBox.Value = _mainForm.dataGridView1["Ед. изм.", i].Value.ToString();
                    }
                    else
                    {
                        UOMComboBox.Value = UOMComboBox.Items[index];
                    }

                    //_form.dataGridView1["Единицы Измерения",i] = cell;
                }
                _mainForm.dataGridView1.Columns["Единицы Измерения"].DisplayIndex = 2;
                _mainForm.dataGridView1.Columns["Ед. изм."].Visible = false;
                //

                MainFormController._form.dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                for (int i = 1; i < MainFormController._form.dataGridView1.ColumnCount; i++)
                {
                    MainFormController._form.dataGridView1.Columns[i].DefaultCellStyle.Format = "##0.000";
                    MainFormController._form.dataGridView1.Columns[i].Width = 50;
                    MainFormController._form.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                }
                MainFormController._form.dataGridView1.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;

                _mainForm.dataGridView1.Refresh();
                MainFormController.RefreshListViewPage();
                this.Close();
            }
            if (comboBox1.SelectedItem.ToString()== "Подбор аналогов")
            {
                CreateEmptyCrude();
                SelectionAnalogsForm form = new SelectionAnalogsForm();
                form.Show();
                this.Close();
            }
            if (comboBox1.SelectedItem.ToString() == "Скопировать существующую нефть")
            {
                ChooseCutSetForCopyForm form = new ChooseCutSetForCopyForm(_mainForm, textBox1.Text);
                form.Show();
                this.Close();
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Возможность менять разгонку при разных способах заполнения
            if (comboBox1.SelectedItem.ToString() == "Подбор аналогов")
            {
                comboBox2.SelectedItem = comboBox2.Items[4];
                comboBox2.Enabled = false;
            }
            if (comboBox1.SelectedItem.ToString() == "Пустая разгонка")
            {
                comboBox2.Enabled = true;
            }
            if (comboBox1.SelectedItem.ToString() == "Скопировать существующую нефть")
            {
                comboBox2.Enabled = false;
            }
        }
        
        //To Control
        public void CreateEmptyCrude()
        {
            AllCrudeInfo allCrudeInfo = new AllCrudeInfo();
            // Add crude to CRUDE table
            Crude newCrude = new Crude();
            newCrude.Name = textBox1.Text;
            newCrude.LibraryId = 1;
            allCrudeInfo.crude = newCrude;

            // Add selected cut set to CutSets Table
            CutSet newCutSet = new CutSet();
            newCutSet.CutSetType = comboBox2.SelectedIndex + 1;
            newCutSet.Name = "Исходная нефть";
            allCrudeInfo.selectedCutSet = newCutSet;
            allCrudeInfo.allCutSets.Add(newCutSet);

            if (comboBox2.Text != "Без разгонки")
            {
                //Add cuts and properties
                List<Cut> listOFCuts = new List<Cut>();
                List<CutType> listofCutTypes = _context.CutTypes.Where(
                                                p => p.CutSetType == comboBox2.SelectedItem.ToString()).OrderBy(p => p.CutTypeId).ToList();

                List<Property> listOfProperties = new List<Property>();

                foreach (CutType cutType in listofCutTypes)
                {
                    Cut newCut = new Cut();
                    newCut.Name = cutType.Name;
                    newCut.CutType = cutType.CutTypeId;
                    listOFCuts.Add(newCut);

                    if (cutType.Ivt != null)
                    {
                        Property newIVT = new Property();
                        newIVT.Value = (double)cutType.Ivt;
                        newIVT.Uom = _context.CutSetTypes.Where(p => p.CutSetTypeId == newCutSet.CutSetType).FirstOrDefault().UOM;
                        newIVT.PropertyTypeId = 1;
                        newIVT.CutId = cutType.CutTypeId;
                        newIVT.CutName = newCut.Name;
                        listOfProperties.Add(newIVT);
                    }
                    if (cutType.Fvt != null)
                    {
                        Property newFVT = new Property();
                        newFVT.Value = (double)cutType.Fvt;
                        newFVT.Uom = _context.CutSetTypes.Where(p => p.CutSetTypeId == newCutSet.CutSetType).FirstOrDefault().UOM;
                        newFVT.PropertyTypeId = 2;
                        newFVT.CutId = cutType.CutTypeId;
                        newFVT.CutName = newCut.Name;

                        listOfProperties.Add(newFVT);
                    }

                }
                allCrudeInfo.selectedCutSetCuts = listOFCuts;
                allCrudeInfo.selectedCutSetProperties = listOfProperties;

            }
            else
            {
                Cut newCut = new Cut();
                newCut.Name = "Вся нефть";
                newCut.CutType = null;
                allCrudeInfo.selectedCutSetCuts.Add(newCut);
            }

            MainFormController.selectedCrudeInfo = allCrudeInfo;
            _controller.AddNewOilToDataBase();
        }

    }
}
