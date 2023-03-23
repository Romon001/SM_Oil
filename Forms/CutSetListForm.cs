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
    public partial class CutSetListForm : Form
    {
        public int _oilId;
        public CutSet selectedCutSet { get; set; }
        public List<CutSet> _list { get; set; }
        public bool isCopy;
        public string _newOilName { get; set; }
        public CutSetListForm()
        {
            InitializeComponent();
        }
        public CutSetListForm(int oilId, List<CutSet> list, string newOilName, bool copy = false, Form owner= null)
        {
            Owner = owner; 
            _oilId = oilId;
            _list = list;
            isCopy = copy;
            _newOilName = newOilName;
            dbSMOilContext _context = new dbSMOilContext();

            InitializeComponent();
            foreach(var i in list)
            {
                listView1.Items.Add(i.Name);

            }
            
            label1.Text = _context.Crudes.Where(p=>p.CrudeId==_oilId).FirstOrDefault().Name;
            listView1.SelectedIndices.Add(0);
        }

        public void button1_Click(object sender, EventArgs e)
        {
            MainFormController controller = new MainFormController(MainFormController._form);
            
            if (!isCopy)
            {
                DataTable oilTable = controller.CreateOilPropertyTable(_oilId, selectedCutSet.CutSetId);
                MainFormController.selectedCrudeInfo.SetInfoByCrudeId(_oilId);
                MainFormController.selectedCrudeInfo.selectedCutSetDataTable = oilTable;
            }
            else
            {
                DataTable oilTable = controller.CreateOilPropertyTable(_oilId, selectedCutSet.CutSetId);
                MainFormController.selectedCrudeInfo.SetInfoByCrudeId(_oilId);
                //Crude 
                Crude newCrude = new Crude();
                newCrude.Name = _newOilName;
                newCrude.LibraryId = 1;

                //CutSet
                CutSet newCutSet = new CutSet();
                newCutSet.CutSetType = selectedCutSet.CutSetType;
                newCutSet.Name = "Исходная нефть";

                //Cuts and props
                List<Cut> listOFCuts = new List<Cut>();
                List<Property> listOfProperties = new List<Property>();
                foreach (Cut cut in MainFormController.selectedCrudeInfo.selectedCutSetCuts)
                {
                    Cut newCut = new Cut();
                    newCut.CutType = cut.CutType;
                    newCut.Description = cut.Description;
                    newCut.Name = cut.Name;
                    listOFCuts.Add(newCut);
                }
                foreach (Property prop in MainFormController.selectedCrudeInfo.selectedCutSetProperties)
                {
                    Property newProperty = new Property();
                    newProperty.Value = prop.Value;
                    newProperty.Uom = prop.Uom;
                    newProperty.PropertyTypeId = prop.PropertyTypeId;
                    newProperty.CutName = prop.CutName;
                    listOfProperties.Add(newProperty);
                }
                MainFormController.selectedCrudeInfo.crude = newCrude;
                MainFormController.selectedCrudeInfo.selectedCutSet = newCutSet;
                MainFormController.selectedCrudeInfo.allCutSets.Clear();
                MainFormController.selectedCrudeInfo.allCutSets.Add(newCutSet);
                MainFormController.selectedCrudeInfo.selectedCutSetCuts = listOFCuts;
                MainFormController.selectedCrudeInfo.selectedCutSetProperties = listOfProperties;
                MainFormController.selectedCrudeInfo.selectedCutSetDataTable= oilTable;
                MainFormController _controller = new MainFormController(MainFormController._form);
                _controller.AddNewOilToDataBase();
                MainFormController.RefreshListViewPage();
;            }
            if (Owner != null)
            {
                Owner.Close();
            }
            
            this.Close();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            string listName = this.listView1.SelectedItems[0].Text;
            var cutSet = _list.Where(p => p.Name.ToString() == listName).FirstOrDefault();
            selectedCutSet = cutSet;
        }
    }
}
