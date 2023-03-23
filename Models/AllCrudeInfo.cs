using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM_Oil.Controllers;
namespace SM_Oil.Models
{
    public class AllCrudeInfo
    {
        public AllCrudeInfo()
        {
            crude = new Crude();
            allCutSets = new List<CutSet>();
            selectedCutSetCuts = new List<Cut>();
            selectedCutSetProperties = new List<Property>();
            selectedCutSet = new CutSet();
        }
        public Crude crude { get; set; }
        public List<CutSet> allCutSets { get; set;}
        public List<Cut> selectedCutSetCuts { get; set;}
        public List<Property> selectedCutSetProperties { get; set;}
        public CutSet selectedCutSet { get; set; } 
        public DataTable selectedCutSetDataTable = new DataTable();
        public void SetInfoByCrudeId(int id)
        {
            crude = MainFormController._context.Crudes.Where(p => p.CrudeId == id).FirstOrDefault();
            allCutSets = MainFormController._context.CutSets.Where(p => p.CrudeId == id).ToList();
            selectedCutSet = allCutSets[0];
            selectedCutSetCuts = MainFormController._context.Cuts.Where(p => p.CutSetId == selectedCutSet.CutSetId).ToList();
            var c = (from a in MainFormController._context.Properties.ToList()
                     join b in selectedCutSetCuts
                     on a.CutId equals b.CutId
                     select a);
            selectedCutSetProperties = c.ToList();
        }
        public void Clear()
        {
            crude = new Crude();
            allCutSets.Clear();
            selectedCutSetCuts.Clear();
            selectedCutSetProperties.Clear();
            selectedCutSet = new CutSet();
            selectedCutSetDataTable.Clear();
        }
    }
}
