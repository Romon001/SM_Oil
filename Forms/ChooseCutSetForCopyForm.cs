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
using SM_Oil.Models;
namespace SM_Oil.Forms
{
    public partial class ChooseCutSetForCopyForm : Form
    {
        public MainFormController _controller { get; set; }
        public static string oilName;
        public Stack<ListViewItem> folderStack { get; set; }
        
        public ChooseCutSetForCopyForm()
        {
            InitializeComponent();
        }
        public ChooseCutSetForCopyForm(MainForm mainForm, string name)
        {
            InitializeComponent();
            folderStack = new Stack<ListViewItem>();
            oilName = name;
            _controller = new MainFormController(mainForm);
            _controller.FillPrimaryLibraryList(listView1);
        }

        public void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView senderList = (ListView)sender;
            ListViewItem clickedItem = senderList.HitTest(e.Location).Item;

            List<ListViewItem> itemsList = new List<ListViewItem>();
            ValueTuple<string, int> tag = (ValueTuple<string, int>)clickedItem.Tag;
            var name = clickedItem.Text;
            var type = tag.Item1;
            var id = tag.Item2;

            if (type == "SM_Oil.Library")
            {
                listView1.Items.Clear();
                var folders = MainFormController._context.Folders.Where(p => p.Library == id);
                var oils = MainFormController._context.Crudes.Where(p => p.LibraryId == id);
                foreach (var folder in folders)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = folder.Name;
                    item.Tag = (folder.GetType().ToString(), folder.FolderId);
                    item.ImageKey = "FolderImage";
                    listView1.Items.Add(item);
                }
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    listView1.Items.Add(item);
                }
                folderStack.Push(clickedItem);
            }
            if (type == "SM_Oil.Folder")
            {
                listView1.Items.Clear();
                var oils = MainFormController._context.Crudes.Where(p => p.FolderId == id);
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    listView1.Items.Add(item);
                }
                 folderStack.Push(clickedItem);
            }
            if (type == "SM_Oil.Crude")
            {
                var list = MainFormController._context.CutSets.Where(p => p.CrudeId == id).ToList();
                var nameOfOil = MainFormController._context.Crudes.Where(p => p.CrudeId == id).FirstOrDefault().Name;
                Form newForm = new CutSetListForm(id, list, oilName,true, this);
                newForm.Show();

            }


        }
        public void CreateCopyCrude()
        {
            AllCrudeInfo allCrudeInfo = new AllCrudeInfo();
            Crude newCrude = new Crude();
            newCrude.Name = oilName;
            //todo path
            newCrude.LibraryId = 1;
            CutSet newCutSet = new CutSet();
            newCutSet.CutSetType = MainFormController.selectedCrudeInfo.selectedCutSet.CutSetType;
            newCutSet.Description = MainFormController.selectedCrudeInfo.selectedCutSet.Description;
            newCutSet.Name = "Исходная нефть";

            //add cuts and properties 
            List<Cut> listOFCuts = new List<Cut>();
            List<Property> listOfProperties = new List<Property>();
            foreach(Cut cut in MainFormController.selectedCrudeInfo.selectedCutSetCuts)
            {
                Cut newCut = new Cut();
                newCut.CutType = cut.CutType;
                newCut.Description = cut.Description;
                newCut.Name = cut.Name;
                listOFCuts.Add(newCut);
            }
            foreach(Property prop in MainFormController.selectedCrudeInfo.selectedCutSetProperties)
            {
                Property newProperty = new Property();
                newProperty.Value = prop.Value;
                newProperty.Uom = prop.Uom;
                newProperty.PropertyTypeId = prop.PropertyTypeId;
                newProperty.CutName = prop.CutName;
                listOfProperties.Add(newProperty);
            }
            allCrudeInfo.crude = newCrude;
            allCrudeInfo.selectedCutSet= newCutSet;
            allCrudeInfo.selectedCutSetCuts= listOFCuts;
            allCrudeInfo.allCutSets.Clear();
            allCrudeInfo.allCutSets.Add(newCutSet);
            allCrudeInfo.selectedCutSetProperties = listOfProperties;
            MainFormController.selectedCrudeInfo = allCrudeInfo;
            _controller.AddNewOilToDataBase();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListViewItem clickedItem = listView1.SelectedItems[0];
            List<ListViewItem> itemsList = new List<ListViewItem>();
            ValueTuple<string, int> tag = (ValueTuple<string, int>)clickedItem.Tag;
            var name = clickedItem.Text;
            var type = tag.Item1;
            var id = tag.Item2;

            if (type == "SM_Oil.Library")
            {
                listView1.Items.Clear();
                var folders = MainFormController._context.Folders.Where(p => p.Library == id);
                var oils = MainFormController._context.Crudes.Where(p => p.LibraryId == id);
                foreach (var folder in folders)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = folder.Name;
                    item.Tag = (folder.GetType().ToString(), folder.FolderId);
                    item.ImageKey = "FolderImage";
                    listView1.Items.Add(item);
                }
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    listView1.Items.Add(item);
                }
                folderStack.Push(clickedItem);
            }
            if (type == "SM_Oil.Folder")
            {
                listView1.Items.Clear();
                var oils = MainFormController._context.Crudes.Where(p => p.FolderId == id);
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    listView1.Items.Add(item);
                }
                folderStack.Push(clickedItem);
            }
            if (type == "SM_Oil.Crude")
            {
                var list = MainFormController._context.CutSets.Where(p => p.CrudeId == id).ToList();
                var nameOfOil = MainFormController._context.Crudes.Where(p => p.CrudeId == id).FirstOrDefault().Name;
                Form newForm = new CutSetListForm(id, list, oilName, true, this);
                newForm.Show();

            }
        }
    }
}
