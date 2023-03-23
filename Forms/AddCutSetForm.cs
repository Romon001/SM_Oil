using SM_Oil.Controllers;
using SM_Oil.Models;
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
    public partial class AddCutSetForm : Form
    {
        dbSMOilContext _context;
        MainForm _mainForm;
        MainFormController _controller;
        Crude _crude;
        public AddCutSetForm()
        {
            InitializeComponent();
        }
        public AddCutSetForm(MainForm mainForm, ListViewItem clickedItem)
        {
            ValueTuple<string, int> tag = (ValueTuple<string, int>)clickedItem.Tag;
            var type = tag.Item1;
            var id = tag.Item2;
           
            _context = new dbSMOilContext();
            _crude = _context.Crudes.Where(p => p.CrudeId == id).FirstOrDefault();
            //TODO: Сделвть общий объект mainForm
            _mainForm = mainForm;
            _controller = new MainFormController(mainForm);
            InitializeComponent();
            textBox1.Text = "";
            this.Text = clickedItem.Text + " нефть : новая разгонка";

            comboBox1.Items.Add("Пустая разгонка");
            comboBox1.Items.Add("Подбор аналогов");
            comboBox1.Items.Add("Скопировать существующую нефть");
            comboBox1.SelectedIndex = 0;

            var cutSetTypes = _context.CutSetTypes.ToList();
            foreach (var cutSet in cutSetTypes)
            {
                comboBox2.Items.Add(cutSet.Name);
            }
            comboBox2.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Пустая разгонка")
            {
                CreateNewCutSet(_crude);
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
                    DataGridViewComboBoxCell a = (DataGridViewComboBoxCell)_mainForm.dataGridView1["Единицы Измерения", i];
                    //foreach(var b in data)
                    //{
                    //    a.Items.Add(b.Name);
                    //}
                    a.DataSource = data3;
                    int index = a.Items.IndexOf(_mainForm.dataGridView1["Ед. изм.", i].Value.ToString());
                    //TODO содержание всякой шняги поменять с процентной доли на массовое количество
                    if (index == -1)
                    {
                        List<string> list = new List<string>();
                        list.Add(_mainForm.dataGridView1["Ед. изм.", i].Value.ToString());
                        a.DataSource = list;
                        a.Value = _mainForm.dataGridView1["Ед. изм.", i].Value.ToString();
                    }
                    else
                    {
                        a.Value = a.Items[index];
                    }

                    //_form.dataGridView1["Единицы Измерения",i] = cell;
                }
                _mainForm.dataGridView1.Columns["Единицы Измерения"].DisplayIndex = 2;
                _mainForm.dataGridView1.Columns["Ед. изм."].Visible = false;

                MainFormController._form.dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                for (int i = 1; i < MainFormController._form.dataGridView1.ColumnCount; i++)
                {
                    MainFormController._form.dataGridView1.Columns[i].DefaultCellStyle.Format = "##0.000";
                    MainFormController._form.dataGridView1.Columns[i].Width = 50;
                    MainFormController._form.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                }
                MainFormController._form.dataGridView1.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;

                _mainForm.dataGridView1.Refresh();
                this.Close();
            }
            if (comboBox1.SelectedItem.ToString() == "Подбор аналогов")
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
        public void CreateEmptyCrude()
        {
            AllCrudeInfo allCrudeInfo = new AllCrudeInfo();
            // Add crude to CRUDE table
            Crude newCrude = new Crude();
            newCrude.Name = textBox1.Text;
            newCrude.LibraryId = 1;

            // Add selected cut set to CutSets Table
            CutSet newCutSet = new CutSet();
            newCutSet.CutSetType = comboBox2.SelectedIndex + 1;
            newCutSet.Name = "Исходная нефть";

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
            allCrudeInfo.crude = newCrude;
            allCrudeInfo.selectedCutSetCuts = listOFCuts;
            allCrudeInfo.selectedCutSet = newCutSet;
            allCrudeInfo.allCutSets.Add(newCutSet);
            allCrudeInfo.selectedCutSetProperties = listOfProperties;
            MainFormController.selectedCrudeInfo = allCrudeInfo;
            _controller.AddNewOilToDataBase();
        }
        public void CreateNewCutSet(Crude crude)
        {
            AllCrudeInfo allCrudeInfo = new AllCrudeInfo();


            // Add selected cut set to CutSets Table
            CutSet newCutSet = new CutSet();
            newCutSet.CutSetType = comboBox2.SelectedIndex + 1;
            newCutSet.CrudeId = crude.CrudeId;
            newCutSet.Name = textBox1.Text;

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
            allCrudeInfo.crude = crude;
            allCrudeInfo.selectedCutSetCuts = listOFCuts;
            allCrudeInfo.selectedCutSet = newCutSet;
            allCrudeInfo.allCutSets.Add(newCutSet);
            allCrudeInfo.selectedCutSetProperties = listOfProperties;
            MainFormController.selectedCrudeInfo = allCrudeInfo;
            _controller.AddNewCutSetToDataBase();
        }
    }
}
