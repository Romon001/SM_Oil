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
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Solvers;
using Microsoft.SolverFoundation.Services;
using CenterSpace.NMath.Core;
using SM_Oil.Models;
namespace SM_Oil.Forms
{
    public partial class SelectionAnalogsForm : Form
    {
        dbSMOilContext _context;
        public DataTable fullPropertiesTable { get; set; }
        //public bool isOilMixed { get; set; } = false;
        public DataRow mixedOil { get; set; }
        DataRow initialOil { get; set; }

        List<string> logMessage = new List<string>();
        public DataTable tableOfAnalogues { get; set; }

        public bool isVolumeCalculation = false;
        public DataTable _closenessAnalogs { get; set; }
        public DataRow _finalAnalog { get; set; }
        public SelectionAnalogsForm()
        {
            InitializeComponent();
            _context = new dbSMOilContext();
            this.Text = $"Подбор аналогов: {MainFormController.selectedCrudeInfo.crude.Name}";
        }
        private void CreateClosenessSelectionTable()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.ColumnHeadersVisible = true;
        }
        private DataTable GetFullPropertiesTable()
        {
            //if (!(fullPropertiesTable is null))
            //{
            //    return fullPropertiesTable;
            //}
            DataTable propTable = new DataTable();
            propTable.Columns.Add("Название", System.Type.GetType("System.String"));
            propTable.Columns.Add("Сера", System.Type.GetType("System.Double"));
            //propTable.Columns.Add("Выход 350С", System.Type.GetType("System.Double"));
            propTable.Columns.Add("Парафины", System.Type.GetType("System.Double"));
            propTable.Columns.Add("Плотность", System.Type.GetType("System.Double"));
            propTable.Columns.Add("Вязкость");
            propTable.Columns.Add("Расстояние");
            propTable.Columns.Add("Рецепт");
            propTable.Columns["Рецепт"].DefaultValue = 0;
            propTable.Columns["Расстояние"].DefaultValue = Double.PositiveInfinity;

            var indexLibraryOils = _context.Crudes.Where(p => p.LibraryId == 2).ToList();
            foreach (var oil in indexLibraryOils)
            {
                //TODO: поменять id нефти на id разгонки
                var fractions = _context.Cuts.Where(p => p.CutSetId == oil.CrudeId).ToList();
                var oilCutId = fractions.Where(p => p.Name == "НЕФТЬ В ЦЕЛОМ").Select(p => p.CutId).FirstOrDefault();
                var oilProperties = _context.Properties.Where(p => p.CutId == oilCutId);

                DataRow workRow = propTable.NewRow();

                workRow["Название"] = oil.Name;
                workRow["Сера"] = oilProperties.Where(p => p.PropertyTypeId == 16)
                                                                    .Select(p => p.Value).FirstOrDefault();
                workRow["Парафины"] = oilProperties.Where(p => p.PropertyTypeId == 17)
                                                                    .Select(p => p.Value).FirstOrDefault();
                workRow["Плотность"] = oilProperties.Where(p => p.PropertyTypeId == 5)
                                                                    .Select(p => p.Value).FirstOrDefault();
                workRow["Вязкость"] = oilProperties.Where(p => p.PropertyTypeId == 27 || p.PropertyTypeId == 30)
                                                                    .Select(p => p.Value).FirstOrDefault();

                if (workRow["Вязкость"].ToString() == "")
                {
                    workRow["Вязкость"] = 0;
                }

                //var frac350CutId = fractions.Where(p => p.TTL == "320...350")
                //                                                    .Select(p => p.CUT_ID).FirstOrDefault();
                //workRow["Выход 350С"] = fractions.Where(p => p.CUT_ID > oilCutId && p.CUT_ID <= frac350CutId)
                //                                                    .Sum(p => p.YLD);
                propTable.Rows.Add(workRow);

            }
            fullPropertiesTable = propTable;
            return fullPropertiesTable;
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            label13.Text = "";

            var sul = Convert.ToDouble(textBoxSUL.Text);
            //var _350 = Convert.ToDouble(textBox350.Text);
            var par = Convert.ToDouble(textBoxPAR.Text);
            var spg = Convert.ToDouble(textBoxSPG.Text);
            var cst = Convert.ToDouble(textBoxCST.Text);

            label7.Text = "Нефти аналоги по близости ";

            List<string> properties = new List<string> { "Сера", "Парафины", "Плотность", "Вязкость" };//"Выход 350С", 

            DataTable closenessAnalogs = new DataTable();
            closenessAnalogs.Columns.Add("Название", System.Type.GetType("System.String"));
            closenessAnalogs.Columns.Add("Сера", System.Type.GetType("System.Double"));
            //closenessAnalogs.Columns.Add("Выход 350С", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Парафины", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Плотность", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Вязкость");
            closenessAnalogs.Columns.Add("Расстояние", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Рецепт", System.Type.GetType("System.Double"));

            initialOil = closenessAnalogs.NewRow();
            initialOil["Название"] = "Исходная нефть";
            initialOil["сера"] = Convert.ToDouble(textBoxSUL.Text);
            //initialOil["Выход 350С"] = Convert.ToDouble(textBox350.Text);
            initialOil["Парафины"] = Convert.ToDouble(textBoxPAR.Text);
            initialOil["Плотность"] = Convert.ToDouble(textBoxSPG.Text);
            initialOil["Вязкость"] = Convert.ToDouble(textBoxCST.Text);
            initialOil["Расстояние"] = "0,0000";
            initialOil["Рецепт"] = "1,000";

            var coefSul = Convert.ToDouble(textBox11.Text);
            //var coef350 = Convert.ToDouble(textBox7.Text);
            var coefPar = Convert.ToDouble(textBox8.Text);
            var ceofSpg = Convert.ToDouble(textBox9.Text);
            var coefCst = Convert.ToDouble(textBox10.Text);

            var coefSum = coefSul  + coefPar + ceofSpg + coefCst;//+coef350
            coefSul /= coefSum;
            //coef350 /= coefSum;
            coefPar /= coefSum;
            ceofSpg /= coefSum;
            coefCst /= coefSum;
            textBox11.Text = coefSul.ToString();
            textBox8.Text = coefPar.ToString();
            textBox9.Text = ceofSpg.ToString();
            textBox10.Text = coefCst.ToString();

            //var answer = MessageBox.Show("Решить с помощью линейного программирования?",
            //                             "Сообщение",
            //                             MessageBoxButtons.YesNo,
            //                             MessageBoxIcon.Information,
            //                             MessageBoxDefaultButton.Button1,
            //                             MessageBoxOptions.DefaultDesktopOnly);

            CreateClosenessSelectionTable();

            DataTable propertiesTable = GetFullPropertiesTable();


            int numberOfAnalogs = Convert.ToInt32(textBox16.Text);
            List<DataRow> analogs = new List<DataRow>(numberOfAnalogs);
            List<double> analogsDistance = new List<double>(numberOfAnalogs);
            if (!propertiesTable.Columns.Contains("Расстояние"))
            {
                propertiesTable.Columns.Add("Расстояние", System.Type.GetType("System.Double"));
                propertiesTable.Columns.Add("Рецепт", System.Type.GetType("System.Double"));

            }
            int count = 0;
            int indexOfDistance;
            foreach (DataRow b in propertiesTable.Rows)
            {
                count++;

                b["Расстояние"] = CalculateDistance(b);

                double distance = Convert.ToDouble(b["Расстояние"]);
                b["Рецепт"] = 0;
                if (count <= numberOfAnalogs)
                {
                    analogs.Add(b);
                    analogsDistance.Add(distance);
                }
                else
                {
                    if (distance < analogsDistance.Max())
                    {
                        indexOfDistance = analogsDistance.IndexOf(analogsDistance.Max());
                        analogs[indexOfDistance] = b;
                        analogsDistance[indexOfDistance] = distance;
                    }
                }


            }


            foreach (var a in analogs)
            {
                closenessAnalogs.ImportRow(a);
            }



            //Поиск минимальных и максимальных значений свойств аналогов
            DataRow maxAnalog = closenessAnalogs.NewRow();
            DataRow minAnalog = closenessAnalogs.NewRow();
            maxAnalog.ItemArray = closenessAnalogs.Rows[0].ItemArray.Clone() as object[];
            minAnalog.ItemArray = closenessAnalogs.Rows[0].ItemArray.Clone() as object[];

            foreach (string property in properties)
            {
                foreach (DataRow a in closenessAnalogs.Rows)
                {
                    minAnalog[property] = (Convert.ToDouble(a[property]) < Convert.ToDouble(minAnalog[property])) ? a[property] : minAnalog[property];
                    maxAnalog[property] = (Convert.ToDouble(a[property]) > Convert.ToDouble(maxAnalog[property])) ? a[property] : maxAnalog[property];
                }
            }



            //Сообщение о том что наша нефть не в рамках свойств аналогов
            Dictionary<string, string> checkProperties = new Dictionary<string, string>{
                                                                              {"Сера", "Норма"},
                                                                              //{"Выход 350С", "Норма"},
                                                                              {"Парафины", "Норма"},
                                                                              {"Плотность", "Норма"},
                                                                              {"Вязкость", "Норма"}
                                                                              };
            if (sul > Convert.ToDouble(maxAnalog["Сера"])) { checkProperties["Сера"] = "Больше"; }
            if (sul < Convert.ToDouble(minAnalog["Сера"])) { checkProperties["Сера"] = "Меньше"; }
            //if (_350 > Convert.ToDouble(maxAnalog["Выход 350С"])) { checkProperties["Выход 350С"] = "Больше"; }
            //if (_350 < Convert.ToDouble(minAnalog["Выход 350С"])) { checkProperties["Выход 350С"] = "Меньше"; }
            if (par > Convert.ToDouble(maxAnalog["Парафины"])) { checkProperties["Парафины"] = "Больше"; }
            if (par < Convert.ToDouble(minAnalog["Парафины"])) { checkProperties["Парафины"] = "Меньше"; }
            if (spg > Convert.ToDouble(maxAnalog["Плотность"])) { checkProperties["Плотность"] = "Больше"; }
            if (spg < Convert.ToDouble(minAnalog["Плотность"])) { checkProperties["Плотность"] = "Меньше"; }
            if (cst > Convert.ToDouble(maxAnalog["Вязкость"])) { checkProperties["Вязкость"] = "Больше"; }
            if (cst < Convert.ToDouble(minAnalog["Вязкость"])) { checkProperties["Вязкость"] = "Меньше"; }


            //Валидация на то, что наша нефть в рамках свойств аналогов
            //DataTable supportingAnalogs = CreateRangeException(checkProperties, propertiesTable);

            //closenessAnalogs.Merge(supportingAnalogs);

            //Колонка галочки
            DataGridViewCheckBoxColumn col = new DataGridViewCheckBoxColumn();
            col.Name = "Checked";
            col.HeaderText = " ";
            //col.CellTemplate = new TrueCheckBoxTemplate();
            dataGridView1.Columns.Add(col);

            dataGridView1.DataSource = closenessAnalogs;
            for (int i = 0; i < Convert.ToInt32(textBox16.Text); i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = 1;
            }
            dataGridView1.Refresh();

            dataGridView1.Columns[0].Width = 20;
            dataGridView1.Columns[1].Width = 150;
            dataGridView1.Columns[2].Width = 100;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 80;
            dataGridView1.Columns[5].Width = 80;
            dataGridView1.Columns[6].Width = 80;
            dataGridView1.Columns[7].Width = 80;


            dataGridView1.Refresh();
            calculateButton.Enabled = true;

            // конец подбора аналогов



        }
        private void calculateButtonClick(object sender, EventArgs e)
        {

            DataTable closenessAnalogs = new DataTable();
            closenessAnalogs.Columns.Add("Название", System.Type.GetType("System.String"));
            closenessAnalogs.Columns.Add("Сера", System.Type.GetType("System.Double"));
            //closenessAnalogs.Columns.Add("Выход 350С", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Парафины", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Плотность", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Вязкость", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Расстояние", System.Type.GetType("System.Double"));
            closenessAnalogs.Columns.Add("Рецепт", System.Type.GetType("System.Double"));
            tableOfAnalogues = closenessAnalogs.Clone();
            tableOfAnalogues.Rows.Clear();
            //DataTable gridTable = (DataTable)dataGridView1.DataSource;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if ((row.Cells[0].Value.ToString() =="1"))
                {
                    DataRow a = ((DataRowView)row.DataBoundItem).Row;
                    closenessAnalogs.ImportRow(a);
                    tableOfAnalogues.ImportRow(a);
                }
            }
            DataRow finalAnalog = closenessAnalogs.NewRow();
            finalAnalog["Название"] = "Смесь нефтей";
            finalAnalog["Сера"] = 0;
            //finalAnalog["Выход 350С"] = 0;
            finalAnalog["Парафины"] = 0;
            finalAnalog["Плотность"] = 0;
            finalAnalog["Вязкость"] = 0;
            finalAnalog["Расстояние"] = 0;
            finalAnalog["Рецепт"] = 0;
            
            _closenessAnalogs = closenessAnalogs;
            _finalAnalog = finalAnalog;
            CalculateRecipe(finalAnalog, closenessAnalogs);


            _closenessAnalogs.Rows.Add(finalAnalog);
            _closenessAnalogs.Rows.Add("Исходная нефть",
                                      Convert.ToDouble(textBoxSUL.Text),
                                      //Convert.ToDouble(textBox350.Text),
                                      Convert.ToDouble(textBoxPAR.Text),
                                      Convert.ToDouble(textBoxSPG.Text),
                                      Convert.ToDouble(textBoxCST.Text),
                                      "0,0000",
                                      "1,000");
            dataGridView1.DataSource = _closenessAnalogs;
            for(int i = 1;i< dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].DefaultCellStyle.Format = "##0.000";
            }
            dataGridView1.Columns[0].Width = 175;
            dataGridView1.Columns[1].Width = 165;
            dataGridView1.Columns[2].Width = 160;
            dataGridView1.Columns[3].Width = 110;
            dataGridView1.Columns[4].Width = 110;
            dataGridView1.Columns[5].Width = 110;
            dataGridView1.Columns[6].Width = 110;
            dataGridView1.Columns[7].Width = 110;

            dataGridView1.Refresh();
            dataGridView1.Columns[0].Visible = false;
            calculateButton.Enabled = false;
            mixedOil = finalAnalog;
        }
        public double ObjectiveFunction(DoubleVector x)
        {
            double coefSul = Convert.ToDouble(textBox11.Text);
            double coefPar = Convert.ToDouble(textBox8.Text);
            double coefSPG = Convert.ToDouble(textBox9.Text);
            double coefCST = Convert.ToDouble(textBox10.Text);
            double sul_0 = Convert.ToDouble(textBoxSUL.Text);
            double par_0 = Convert.ToDouble(textBoxPAR.Text);
            double SPG_0 = Convert.ToDouble(textBoxSPG.Text);
            double CST_0 = Convert.ToDouble(textBoxCST.Text);
            double mixSul = 0;
            double mixPar = 0;
            double mixSPG = 0;
            double mixCST = 0;

            for (int i = 0; i < x.Length; i++)
            {
                mixSul += x[i] * Convert.ToDouble(_closenessAnalogs.Rows[i]["Сера"]);
                mixPar += x[i] * Convert.ToDouble(_closenessAnalogs.Rows[i]["Парафины"]);
                mixSPG += x[i] / Convert.ToDouble(_closenessAnalogs.Rows[i]["Плотность"]);
                mixCST += x[i] * ViscosityIndex(Convert.ToDouble(_closenessAnalogs.Rows[i]["Вязкость"]))
                                / Convert.ToDouble(_closenessAnalogs.Rows[i]["Плотность"]);
            }

            mixSPG = 1 / mixSPG;
            mixCST = mixCST * mixSPG;

            return coefSul * Math.Abs((mixSul - sul_0) / sul_0)
                 + coefPar* Math.Abs((mixPar - par_0) / par_0)
                 + coefSPG * Math.Abs((mixSPG - SPG_0) / SPG_0)
                 + coefCST * Math.Abs((mixCST - ViscosityIndex(CST_0)) / ViscosityIndex(CST_0));
        }
        public void CalculateRecipe(DataRow finalAnalog, DataTable closenessAnalogs)
        {
            List<CenterSpace.NMath.Core.Constraint> constraints = new List<CenterSpace.NMath.Core.Constraint>();

            int xDim = Convert.ToInt32(closenessAnalogs.Rows.Count);
            DoubleVector recipes = new DoubleVector(xDim);
            DoubleVector unitVector = new DoubleVector(xDim, 1);
            Func<DoubleVector, double> objective = ObjectiveFunction;
            var problem = new NonlinearProgrammingProblem(xDim, objective);
            for (int i = 0; i < xDim; i++)
            {
                problem.AddBounds(i, 0.0, 1.0);
            }
            problem.AddLinearConstraint(unitVector, 1.0, 1.0);

            var solver = new StochasticHillClimbingSolver();
            solver.RandomSeed = 0x248;

            var solverParams = new StochasticHillClimbingParameters
            {
                TimeLimitMilliSeconds = 15000,
                Presolve = false
            };
            solver.Solve(problem, solverParams);

            var solutions = solver.OptimalX.ToList();
            for (int i = 0; i < solutions.Count; i++)
            {
                solutions[i] /= solutions.Sum();
            }


            for (int i = 0; i < solutions.Count(); i++)
            {
                _closenessAnalogs.Rows[i]["Рецепт"] = solutions[i];
            }
            foreach (DataRow row in _closenessAnalogs.Rows)
            {
                finalAnalog["Сера"] = Convert.ToDouble(finalAnalog["Сера"]) + Convert.ToDouble(row["Рецепт"]) * Convert.ToDouble(row["Сера"]);
                finalAnalog["Парафины"] = Convert.ToDouble(finalAnalog["Парафины"]) + Convert.ToDouble(row["Рецепт"]) * Convert.ToDouble(row["Парафины"]);
                finalAnalog["Плотность"] = Convert.ToDouble(finalAnalog["Плотность"]) + Convert.ToDouble(row["Рецепт"]) / Convert.ToDouble(row["Плотность"]);
                finalAnalog["Вязкость"] = Convert.ToDouble(finalAnalog["Вязкость"]) + Convert.ToDouble(row["Рецепт"]) * ViscosityIndex(Convert.ToDouble(row["Вязкость"])) / Convert.ToDouble(row["Плотность"]);
                finalAnalog["Рецепт"] = Convert.ToDouble(finalAnalog["Рецепт"]) + Convert.ToDouble(row["Рецепт"]);
                row["Рецепт"] = Convert.ToDouble(row["Рецепт"]);
            }

            finalAnalog["Рецепт"] = Convert.ToDouble(finalAnalog["Рецепт"]);
            finalAnalog["Плотность"] = 1 / Convert.ToDouble(finalAnalog["Плотность"]);
            finalAnalog["Вязкость"] = ViscosityIndexReverse(Convert.ToDouble(finalAnalog["Плотность"]) * Convert.ToDouble(finalAnalog["Вязкость"]));
            finalAnalog["Расстояние"] = CalculateDistance(finalAnalog);
            button5.Enabled = true;
        }

        public double CalculateDistance(DataRow analog)
        {
            var coefSul = Convert.ToDouble(textBox11.Text);
            //var coef350 = Convert.ToDouble(textBox7.Text);
            var coefPar = Convert.ToDouble(textBox8.Text);
            var coefSpg = Convert.ToDouble(textBox9.Text);
            var coefCst = Convert.ToDouble(textBox10.Text);

            var sul = Convert.ToDouble(textBoxSUL.Text);
           // var _350 = Convert.ToDouble(textBox350.Text);
            var par = Convert.ToDouble(textBoxPAR.Text);
            var spg = Convert.ToDouble(textBoxSPG.Text);
            var cst = Convert.ToDouble(textBoxCST.Text);

            return Math.Sqrt(coefSul * (sul - Convert.ToDouble(analog["Сера"])) * (sul - Convert.ToDouble(analog["Сера"])) / sul / sul +
                             //coef350 * (_350 - Convert.ToDouble(analog["Выход 350С"])) * (_350 - Convert.ToDouble(analog["Выход 350С"])) / _350 / _350 +
                             coefPar * (par - Convert.ToDouble(analog["Парафины"])) * (par - Convert.ToDouble(analog["Парафины"])) / par / par +
                             coefSpg * (spg - Convert.ToDouble(analog["Плотность"])) * (spg - Convert.ToDouble(analog["Плотность"])) / spg / spg +
                             coefCst * (ViscosityIndex(cst) - ViscosityIndex(Convert.ToDouble(analog["Вязкость"]))) * (ViscosityIndex(cst) - ViscosityIndex(Convert.ToDouble(analog["Вязкость"]))) / ViscosityIndex(cst) / ViscosityIndex(cst));
            //TODO:Проверка на 0
        }
        private double ViscosityIndex(double x)
        {
            double result = -(41.10743 - 49.08258 * Math.Log10(Math.Log10(x + 0.8)));
            return result;
        }
        private double ViscosityIndexReverse(double x)
        {

            double result;
            result = (x + 41.10743) / 49.08258;
            result = Math.Pow(10, result);
            result = Math.Pow(10, result);
            result = result - 0.8;
            return result;
        }

        //Create new Oil Button
        private void button5_Click(object sender, EventArgs e)
        {
            List<(int,double)> blendedOilsId = new List<(int, double)>();
            List<AllCrudeInfo> blendedOils = new List<AllCrudeInfo>();
            for (int i = 0; i<_closenessAnalogs.Rows.Count-2; i++)
            {
                if (Convert.ToDouble(_closenessAnalogs.Rows[i]["Рецепт"] )> 0.001)
                {
                    AllCrudeInfo newCrude = new AllCrudeInfo();
                    int id = _context.Crudes.Where(p => p.Name == _closenessAnalogs.Rows[i]["Название"].ToString()).FirstOrDefault().CrudeId;
                    blendedOilsId.Add((id, Convert.ToDouble(_closenessAnalogs.Rows[i]["Рецепт"])));
                    newCrude.SetInfoByCrudeId(id);

                    blendedOils.Add(newCrude);
                }
            }

            List<Property> requiredProperties = new List<Property>();
            //Поиск общих свойств 

            for (int i =0;i< blendedOils.Count;i++)
            {
                if (i == 0)
                {
                    requiredProperties = (from a in blendedOils[i].selectedCutSetProperties
                                          join b in blendedOils[i].selectedCutSetProperties
                                          on (a.CutName, a.PropertyTypeId) equals (b.CutName, b.PropertyTypeId)
                                          select a).ToList();
                }
                else
                {
                    requiredProperties = (from a in requiredProperties
                                          join b in blendedOils[i].selectedCutSetProperties
                                          on (a.CutName, a.PropertyTypeId) equals (b.CutName, b.PropertyTypeId)
                                          select a).ToList();
                }


            }

            //Заполнение смешенных свойств
            MainFormController.selectedCrudeInfo.selectedCutSetProperties.Clear();
            for (int i = 0;i<blendedOils.Count;i++)
            {
                var recipe = blendedOilsId.Where(p => p.Item1 == blendedOils[i].crude.CrudeId)
                                          .Select(p => p.Item2).FirstOrDefault();
                foreach (var prop in requiredProperties)
                {
                    Property newProp = new Property();
                    newProp = blendedOils[i].selectedCutSetProperties
                        .Where(p => p.CutName == prop.CutName && p.PropertyTypeId == prop.PropertyTypeId).FirstOrDefault();

                    if (MainFormController.selectedCrudeInfo.selectedCutSetProperties
                        .Where(p => p.CutName == prop.CutName && p.PropertyTypeId == prop.PropertyTypeId).Count() == 0)
                    {

                        newProp.Value = newProp.Value * recipe;
                        MainFormController.selectedCrudeInfo.selectedCutSetProperties.Add(newProp);
                    }
                    else
                    {
                        Property property = MainFormController.selectedCrudeInfo.selectedCutSetProperties
                            .Find(p => p.CutName == prop.CutName && p.PropertyTypeId == prop.PropertyTypeId);

                        property.Value += newProp.Value * recipe;
                    }
                }

            }

            MainFormController._form.dataGridView1.DataSource = null;
            DataTable oilDataTable = MainFormController.CreateOilPropertyTable(MainFormController.selectedCrudeInfo);
            MainFormController.selectedCrudeInfo.selectedCutSetDataTable = oilDataTable;
            MainFormController._form.dataGridView1.DataSource = MainFormController.selectedCrudeInfo.selectedCutSetDataTable;
            for(int i=1;i< MainFormController._form.dataGridView1.ColumnCount; i++)
            {
                MainFormController._form.dataGridView1.Columns[i].DefaultCellStyle.Format = "##0.000";
                MainFormController._form.dataGridView1.Columns[i].Width = 50;
            }
            MainFormController._form.dataGridView1.Refresh();
            //this.Close();
        }
    }
}
