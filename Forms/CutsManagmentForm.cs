using SM_Oil.Controllers;
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
    public partial class CutsManagmentForm : Form
    {
        public CutsManagmentForm()
        {
            InitializeComponent();
        }
        public Cut SelectedCut { get; set; }
        public List<Cut> CurrentCuts { get; set; } 
        public int counter { get; set; }
        private void CutsManagmentForm_Load(object sender, EventArgs e)
        {
            counter = -1;
            SetCurrentCuts();
            SetListView();

        }
        public void SetListView()
        {
            listView1.Items.Clear();
            foreach (Cut cut in CurrentCuts)
            {
                ListViewItem item = new ListViewItem();
                item.Text = cut.Name;
                item.Tag = cut.CutId;

                listView1.Items.Add(item);
            }
        }
        public void SetCurrentCuts()
        {
            CurrentCuts = new List<Cut>();
            foreach(Cut cut in MainFormController.selectedCrudeInfo.selectedCutSetCuts)
            {
                CurrentCuts.Add(cut);
            }    
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedCut.Name = textBoxCutName.Text;
            var item = GetSelectedListViewItem();
            item.Text = SelectedCut.Name;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var item = GetSelectedListViewItem();
            listView1.Items.Remove(item);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            

            Cut newCut = new Cut();
            newCut.CutId = counter;
            newCut.Name = textBox1.Text;
            newCut.CutSetId = MainFormController.selectedCrudeInfo.selectedCutSet.CutSetId;
            CurrentCuts.Add(newCut);
            SetListView();
            counter--;

            textBox1.Clear();

        }


        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListView senderList = (ListView)sender;
                ListViewItem clickedItem = senderList.HitTest(e.Location).Item;
                int id = (int)clickedItem.Tag;
                SelectedCut = CurrentCuts.Where(p => p.CutId == id).FirstOrDefault();
                textBoxCutName.Text = SelectedCut.Name;
                
            }
        }
        public ListViewItem GetSelectedListViewItem()
        {
            IEnumerable<ListViewItem> lv = listView1.Items.Cast<ListViewItem>();

            var listViewItem = from xxx in lv
                               where (int)xxx.Tag == SelectedCut.CutId
                               select xxx;
            ListViewItem item = listViewItem.First();
            return item;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach(var cut in CurrentCuts)
            {
                if (MainFormController.selectedCrudeInfo.selectedCutSetCuts.FindIndex(p=>p.CutId == cut.CutId) == -1)
                {
                    Cut newCut = new Cut();
                    newCut.Name = cut.Name;
                    newCut.CutSetId= cut.CutSetId;

                    MainFormController.selectedCrudeInfo.selectedCutSetCuts.Add(newCut);
                    MainFormController._context.Cuts.Add(newCut);
                }
                else
                {
                    MainFormController.selectedCrudeInfo.selectedCutSetCuts.Find(p => p.CutId == cut.CutId).Name = cut.Name ;
                }
            }
            MainFormController._context.SaveChanges();
            this.Close();
        }
    }
}
