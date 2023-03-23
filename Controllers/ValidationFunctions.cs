using SM_Oil.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM_Oil.Controllers;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SM_Oil.Controllers
{
    class ValidationFunctions
    {

        public ValidationFunctions(MainForm mainForm)
        {
            _mainForm = mainForm;
        }
        dbSMOilContext _context = MainFormController._context;
        MainForm _mainForm;
        private AllCrudeInfo _crudeInfo { get; set; }
        public AllCrudeInfo _supplementedCrudeInfo { get; set; }

        DataTable currentValidateMessages { get; set; }
        DataTable previousValidateMessages { get; set; }
        public void SupplementData()
        {
            _supplementedCrudeInfo = new AllCrudeInfo();
            _supplementedCrudeInfo = _crudeInfo;
            
            _mainForm.dataGridView2.DataSource = MainFormController.CreateOilPropertyTable(_supplementedCrudeInfo);
            foreach(DataGridViewColumn col in _mainForm.dataGridView2.Columns)
            {
                col.Width = 60;
            }
            _mainForm.dataGridView2.Columns[0].Width = 100;
            _mainForm.dataGridView2.RowHeadersVisible = false;
            _mainForm.dataGridView2.Refresh();


        }
        public void Validate(AllCrudeInfo crudeInfo)
        {
                currentValidateMessages = new DataTable();
                currentValidateMessages.Columns.Add("Код Ошибки");
                currentValidateMessages.Columns.Add("Сообщение");

                SetCrudeInfo(crudeInfo);
                //var prop2 = _context.PropertyTypes.Where(p=>p.PropertyTypeId==1).FirstOrDefault();
                //var prop = _crudeInfo.selectedCutSetProperties[0];
                //ExistenceCheck(prop2);
                //MinimumValueCheck(prop);
                //MaximunValueCheck(prop);

                //Проверка на наличие основыных свойств
                List<int> mainPropsID = new List<int> { 3, 5 };

                foreach (var prop in mainPropsID)
                {
                    ExistenceCheck(prop);
                }

                //Сумма выхода узких фракций
                CheckQuantityOfOil();


                //Проверка минимальных и максимальных значений в ячейках
                foreach (var prop in _crudeInfo.selectedCutSetProperties)
                {
                    MinimumValueCheck(prop);
                }

                //кубическая интерполяция
                //SplitingCurve();

                //Линейная интерполяция
                for (int i = 3; i < MainFormController._form.dataGridView1.Rows.Count - 1; i++)
                {
                    var name = MainFormController._form.dataGridView1[0, i].Value.ToString();
                    var propertyType = MainFormController._context.PropertyTypes.Where(p => p.Name == name).FirstOrDefault();
                    LinearInterpolation(propertyType.PropertyTypeId);
                }
                _mainForm.dataGridView3.DataSource = currentValidateMessages;
                _mainForm.dataGridView3.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _mainForm.dataGridView3.Refresh();

            
        }
        public void SetCrudeInfo(AllCrudeInfo crudeInfo)
        {
            _crudeInfo = crudeInfo;
        }

        //сумма фракции = 100%
        public bool CheckQuantityOfOil()
        {
            var allQuantityProperties = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == 3).ToList();
            double finalQuantity = 0.0;
            var mainCutID = _crudeInfo.selectedCutSetCuts.Where(p => p.Name == "НЕФТЬ В ЦЕЛОМ").FirstOrDefault().CutId;
            foreach(var prop in allQuantityProperties)
            {
                if (prop.CutId != mainCutID)
                {
                    finalQuantity += prop.Value;
                }
            }
            if (finalQuantity>99.8 && finalQuantity < 100.2)
            {
                return true;
            }
            else
            {
                CreateValidationMessage("SM-002");
                return false;
            }

        }
        public bool ExistenceCheck( PropertyType property)
        {
            var wholeCrudeCutID = _crudeInfo.selectedCutSetCuts.Where(p => p.Name == "НЕФТЬ В ЦЕЛОМ").FirstOrDefault().CutId;
            var crudeProperties = _crudeInfo.selectedCutSetProperties;
            var foundProperty = crudeProperties.Where(p => p.PropertyTypeId == property.PropertyTypeId & p.CutId ==wholeCrudeCutID);
            if (foundProperty.Count() == 0)
            {
                CreateValidationMessage("SM-001", propertyName: property.Name);
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool ExistenceCheck(int prop)
        {
            //TODO СМЕНИТЬ ВСЕ СРАВНЕНИЯ по имени cut НА СРАВНЕНия по id cuttype
            var wholeCrudeCutID = _crudeInfo.selectedCutSetCuts.Where(p => p.Name == "НЕФТЬ В ЦЕЛОМ" || p.Name == "Вся нефть").FirstOrDefault().CutId;
            var crudeProperties = _crudeInfo.selectedCutSetProperties;
            var propName = _context.PropertyGroups.Where(p => p.PropertyGroupId == prop).FirstOrDefault().Name;
            var crudeProperties2 =   from a in crudeProperties
                                join b in _context.PropertyTypes.Where(p=>p.PropertyGroupId==prop) on a.PropertyTypeId equals b.PropertyTypeId
                                select new
                                {
                                    CutId=a.CutId,
                                    CutName=a.CutName,
                                    Id=a.Id,
                                    PropertyTypeId=a.PropertyTypeId,
                                    Uom=a.Uom,
                                    Value=a.Value,
                                    PropertyGroupId=b.PropertyGroupId
                                };
            var foundProperty = crudeProperties2.Where(p => p.PropertyTypeId == prop & p.CutId == wholeCrudeCutID);
            if (foundProperty.Count() == 0)
            {
                CreateValidationMessage("SM-001", propertyName: propName);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool MinimumValueCheck(Property property)
        {
            if (!minValues.ContainsKey(property.PropertyTypeId))
            {
                return true;
            }

            double minValue = minValues[property.PropertyTypeId];
            if (minValue <= property.Value)
            {
                return true;
            }
            else
            {
                var propName = _context.PropertyTypes.Where(p => p.PropertyTypeId == property.PropertyTypeId).FirstOrDefault().Name;
                string message = $"{property.CutName}; {propName}";
                CreateValidationMessage("SM-003", message);
                return false;
            }
        }
        public bool MaximunValueCheck(Property property)
        {
            if (!maxValues.ContainsKey(property.PropertyTypeId))
            {
                return true;
            }

            double maxValue = maxValues[property.PropertyTypeId];
            if (maxValue > property.Value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool MinTemperatureCheck(Cut cut)
        {   
            var selectedCutProperies = _crudeInfo.selectedCutSetProperties
                                        .Where(p => p.CutId == cut.CutId).ToList();
            var IVT = selectedCutProperies.Where(p => p.PropertyTypeId == 1).FirstOrDefault().Value;
            foreach(var prop in selectedCutProperies)
            {
                if (IVT > minPropertyTemperature[prop.PropertyTypeId])
                {
                    return false;
                }
            }
            return true;
        }
        public bool MaxTemperatureCheck(Cut cut)
        {
            var selectedCutProperies = _crudeInfo.selectedCutSetProperties
                                        .Where(p => p.CutId == cut.CutId).ToList();
            var FVT = selectedCutProperies.Where(p => p.PropertyTypeId == 2).FirstOrDefault().Value;
            foreach (var prop in selectedCutProperies)
            {
                if (FVT < minPropertyTemperature[prop.PropertyTypeId])
                {
                    return false;
                }
            }
            return true;
        }

        void CreateValidationMessage(string messageID, string propertyName = "Undefined" )
        {
            string messageText=""; 
            switch (messageID)
            {
                case "SM-001":
                    messageText= $"Отсутствует свойство '{propertyName}' нефти";
                    break;
                case "SM-002":
                    messageText = "Суммарный выход узких фракций не равен 100%";
                    break;        
                case "SM-003":
                    messageText = $"Значение в ячейке '{propertyName}' меньше своего минимального допустимого значения";
                    break;
                default:
                    break;
            }
            DataRow newRow = currentValidateMessages.NewRow();
            newRow[0] = messageID;
            newRow[1] = messageText;
            currentValidateMessages.Rows.Add(newRow);
        }
        private void SplitingCurve()
        {
            var pointsY = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == 16).ToList();
            var mainProp = pointsY.Where(p => p.CutName == "НЕФТЬ В ЦЕЛОМ").FirstOrDefault();
            pointsY.Remove(mainProp);
            List<Property> pointsX = new List<Property>();
            foreach (var point in pointsY)
            {
                var pointX = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == 2 && p.CutId == point.CutId).FirstOrDefault();
                pointsX.Add(pointX);
            }

            
            int n = pointsY.Count;
            //Матрица коэффициентов c_i сплайнов. Коэффициент a равен f[x], b,d выражаюстя через c (см.Вики Кубический_сплайн)
            SparseMatrix matrix = new SparseMatrix(n);

            double[] rightSide = new double[n];
            //Заполнение матрицы и вектора правой части
            for (int i = 0; i < n; i++)
            {
                if ((i == 0) || (i == n - 1))
                {
                    matrix[i, i] = 1;
                    rightSide[i] = 0;
                }
                else
                {
                    matrix[i, i - 1] = pointsX[i].Value- pointsX[i - 1].Value;
                    matrix[i, i] = 2 * (pointsX[i + 1].Value - pointsX[i - 1].Value);
                    matrix[i, i + 1] = pointsX[i + 1].Value - pointsX[i].Value;
                    rightSide[i] = 3 * ((pointsY[i + 1].Value - pointsY[i].Value) / (pointsX[i + 1].Value - pointsX[i].Value)
                                       - (pointsY[i].Value - pointsY[i - 1].Value) / (pointsX[i].Value - pointsX[i - 1].Value));
                }
            }
            var coefC = matrix.Solve(DenseVector.Build.DenseOfArray(rightSide)).ToArray();

            double[] coefA = new double[n];
            double[] coefB = new double[n];
            double[] coefD = new double[n];
            //Нулевые коэффициенты нам не нужны, заполнили нулями
            (coefB[0], coefD[0]) = (0, 0);
            coefA[0] = pointsY[0].Value;
            for (int i = 1; i < n; i++)
            {
                coefA[i] = pointsY[i].Value;
                coefB[i] = (coefA[i] - coefA[i - 1]) / (pointsX[i].Value - pointsX[i - 1].Value)
                           + (2 * coefC[i] + coefC[i - 1]) * (pointsX[i].Value - pointsX[i - 1].Value) / 3;
                coefD[i] = (coefC[i] - coefC[i - 1]) / (pointsX[i].Value - pointsX[i - 1].Value) / 3;
            }

            //
            var allCutsFVT = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == 2).ToList();
            allCutsFVT.Remove(mainProp);
            int counter = 0;
            foreach (var fvt in allCutsFVT)
            {
                if (pointsX.IndexOf(fvt) != -1)
                {
                    counter++;
                    continue;
                }
                if ((counter<=0) || (counter > pointsX.Count))
                {
                    continue;
                }
                var value = coefD[counter] * Math.Pow(fvt.Value, 3)
                                + coefC[counter] * Math.Pow(fvt.Value, 2)
                                + coefB[counter] * (fvt.Value)
                                + coefA[counter];
                Property newProperty = new Property();
                newProperty.Value = value;
                newProperty.Uom = 5;
                newProperty.PropertyTypeId = 16;
                newProperty.CutId = fvt.CutId;
                newProperty.CutName = fvt.CutName;
                _crudeInfo.selectedCutSetProperties.Add(newProperty);
                _mainForm.dataGridView2.DataSource = MainFormController.CreateOilPropertyTable(_crudeInfo);
                _mainForm.dataGridView2.Refresh();
            }
                        
        }
        public void LinearInterpolation(int propertyTypeID)
        {
            var pointsY = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == propertyTypeID).ToList();
            var mainProp = pointsY.Where(p => p.CutName == "НЕФТЬ В ЦЕЛОМ").FirstOrDefault();
            if (mainProp != null)
            {

                pointsY.Remove(mainProp);

            }

            List<Property> pointsX = new List<Property>();
            foreach (var point in pointsY)
            {
                var pointX = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == 2 && p.CutId == point.CutId).FirstOrDefault();
                pointsX.Add(pointX);
            }


            int n = pointsY.Count;
            var allCutsFVT = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == 2).ToList();
            allCutsFVT.Remove(mainProp);
            var allCutsProp = _crudeInfo.selectedCutSetProperties.Where(p => p.PropertyTypeId == propertyTypeID).ToList();
            allCutsProp.Remove(mainProp);
            int counter = 0;
            allCutsFVT.Sort((a, b) => a.Value.CompareTo(b.Value));
            foreach (var fvt in allCutsFVT)
            {
                if (pointsX.IndexOf(fvt) != -1)
                {
                    counter++;
                    continue;
                }
                if ((counter <= 0) || (counter >= pointsX.Count))
                {
                    continue;
                }
                var value = pointsY[counter - 1].Value + (pointsY[counter].Value - pointsY[counter - 1].Value) * (fvt.Value - pointsX[counter - 1].Value) / (pointsX[counter].Value - pointsX[counter - 1].Value);
                Property newProperty = new Property();
                newProperty.Value = value;
                newProperty.PropertyTypeId = propertyTypeID;
                newProperty.CutId = fvt.CutId;
                newProperty.CutName = fvt.CutName;
                var propertyType = MainFormController._context.PropertyTypes.Where(p => p.PropertyTypeId == propertyTypeID).FirstOrDefault();
                var uomSet = MainFormController._context.PropertyGroups.Where(p => p.PropertyGroupId == propertyType.PropertyGroupId).FirstOrDefault().Uomset;
                int? uom = (uomSet!=null)? MainFormController._context.Uoms.Where(p => p.Uomset == uomSet).FirstOrDefault().Uomid : null;
                _crudeInfo.selectedCutSetProperties.Add(newProperty);
                _mainForm.dataGridView2.DataSource = MainFormController.CreateOilPropertyTable(_crudeInfo);
                _mainForm.dataGridView2.Refresh();
            }

        }


        public void ReCutting()
        {

        }

        public void CopyOil()
        {
            MainFormController.copiedDataBuffer = MainFormController.tableOilInfo.Clone();

        }
        ////////////////////////////////////////////////////////////////////////////////////////////////

        private Dictionary<int, double> minValues = new Dictionary<int, double>
        {
            {1,0 },
            {2,0 },
            {3,0 },
            {4,0 },
            {5,0 },
            {6,0 },
            {7,0 },
            {8,0 },
            {9,0 },
        };
        private Dictionary<int, double> maxValues = new Dictionary<int, double>
        {
            {1,0 },
            {2,0 },
            {3,0 },
            {4,0 },
            {5,0 },
            {6,0 },
            {7,0 },
            {8,0 },
            {9,0 },
        };
        private Dictionary<int, double> minPropertyTemperature = new Dictionary<int, double>
        {
            {1,0 },
            {2,0 },
            {3,0 },
            {4,0 },
            {5,0 },
            {6,0 },
            {7,0 },
            {8,0 },
            {9,0 },
        };
        private Dictionary<int, double> maxPropertyTemperature = new Dictionary<int, double>
        {
            {1,0 },
            {2,0 },
            {3,0 },
            {4,0 },
            {5,0 },
            {6,0 },
            {7,0 },
            {8,0 },
            {9,0 },
        };
    }
}
