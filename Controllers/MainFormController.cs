using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SM_Oil.Forms;
using SM_Oil.Models;
//using SmprFunctions;
namespace SM_Oil.Controllers
{
    public class MainFormController
    {

        public static dbSMOilContext _context = new dbSMOilContext();
        public static Stack<ListViewItem> folderStack { get; set; }
        public static DataTable tableOilInfo { get; set; }
        public static BindingSource tableOilInfoSourse = new BindingSource();

        ImageList imageList { get; set; }
        public static MainForm _form { get; set; }
        public static AllCrudeInfo selectedCrudeInfo = new AllCrudeInfo();

        public static DataTable copiedDataBuffer = new DataTable();
        public static AllCrudeInfo copiedCrudeBuffer = new AllCrudeInfo();

        public MainFormController(MainForm form)
        {
            _form = form;
            imageList = new ImageList();
            imageList.Images.Add("FolderImage",Properties.Resources.folder);            
            imageList.Images.Add("OilImage", Properties.Resources.oil);
            _form.listView1.SmallImageList = imageList;
        }
        public  void FillOilTree(TreeView treeView)
        {
            treeView.ImageList = imageList;
            TreeNode root = new TreeNode("Банк Нефтей");
            treeView.Nodes.Add(root);
            var librares = _context.Libraries.ToList();
            var folders = _context.Folders.ToList();
            var crudes = _context.Crudes.ToList();

            foreach (Library lib in librares)
            {

                TreeNode library = new TreeNode(lib.Name);
                library.ImageKey = "FolderImage";
                root.Nodes.Add(library);

                var foldersInLibrary = _context.Folders
                                               .Where(p => p.Library == lib.LibraryId).ToList();
                foreach (var fold in foldersInLibrary)
                {
                    TreeNode folder = new TreeNode(fold.Name);
                    folder.ImageKey = "FolderImage";

                    library.Nodes.Add(folder);
                    var crudesInFolder = _context.Crudes
                               .Where(p => p.FolderId == fold.FolderId).ToList();
                    foreach (var crud in crudesInFolder)
                    {
                        TreeNode crude = new TreeNode(crud.Name);
                        crude.ImageKey = "OilImage";
                        folder.Nodes.Add(crude);
                    }
                }
                var OilsInLibrary = _context.Crudes.Where(p => p.LibraryId == lib.LibraryId).ToList();
                foreach(var oil in OilsInLibrary)
                {
                    TreeNode crude = new TreeNode(oil.Name);
                    crude.ImageKey = "OilImage";
                    library.Nodes.Add(crude);
                }

            }
            treeView.Nodes[0].Expand();

        }
        public void FillPrimaryLibraryList(ListView listView)
        {
            if (folderStack == null)
            {
                folderStack = new Stack<ListViewItem>();
            }
            folderStack.Clear();
            listView.Items.Clear();
            var libraries = _context.Libraries.ToList();
            foreach (var library in libraries)
            {
                ListViewItem newLib = new ListViewItem();
                newLib.Text = library.Name;
                newLib.Tag = (library.GetType().ToString(),library.LibraryId);
                newLib.ImageKey = "FolderImage";
                listView.Items.Add(newLib);

            }
        }

        public static void RefreshListViewPage()
        {
            if (folderStack.Count == 0)
            {
                return;
            }
            ListViewItem page = folderStack.Peek();
            ValueTuple<string, int> tag = (ValueTuple<string, int>)page.Tag;
            var name = page.Text;
            var type = tag.Item1;
            var id = tag.Item2;
            if (type == "SM_Oil.Library")
            {

                _form.listView1.Items.Clear();
                var folders = _context.Folders.Where(p => p.Library == id);
                var oils = _context.Crudes.Where(p => p.LibraryId == id);
                foreach (var folder in folders)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = folder.Name;
                    item.Tag = (folder.GetType().ToString(), folder.FolderId);
                    item.ImageKey = "FolderImage";
                    _form.listView1.Items.Add(item);
                }
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    _form.listView1.Items.Add(item);
                }
                
            }
            if (type == "SM_Oil.Folder")
            {
                _form.listView1.Items.Clear();
                var oils = _context.Crudes.Where(p => p.FolderId == id);
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    _form.listView1.Items.Add(item);
                }
                
            }
        }
        public void OpenNodeContent(ListViewItem clickedItem, ListView listView = null,bool copy =false)
        {
            if (listView == null)
            {
                listView = _form.listView1;
            }
            List<ListViewItem> itemsList = new List<ListViewItem>();
            ValueTuple<string, int> tag = (ValueTuple<string,int>) clickedItem.Tag;
            var name = clickedItem.Text;
            var type = tag.Item1;
            var id = tag.Item2;
            
            if (type == "SM_Oil.Library")
            {
                listView.Items.Clear();
                var folders = _context.Folders.Where(p => p.Library == id);
                var oils = _context.Crudes.Where(p => p.LibraryId == id);
                foreach(var folder in folders)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = folder.Name;
                    item.Tag = (folder.GetType().ToString(), folder.FolderId);
                    item.ImageKey = "FolderImage";
                    listView.Items.Add(item);
                }
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    listView.Items.Add(item);
                }
                folderStack.Push(clickedItem);
            }
            if (type  == "SM_Oil.Folder")
            {
                listView.Items.Clear();
                var oils = _context.Crudes.Where(p => p.FolderId == id);
                foreach (var oil in oils)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = oil.Name;
                    item.Tag = (oil.GetType().ToString(), oil.CrudeId);
                    item.ImageKey = "OilImage";
                    listView.Items.Add(item);
                }
                folderStack.Push(clickedItem);
            }
            if (type == "SM_Oil.Crude")
            {
                //TODO открыть список с нарезками
                OpenCutSetList(id);
                //DataTable oilTable = CreateOilPropertyTable(id);
                //selectedCrudeInfo.SetInfoByCrudeId(id);
                //selectedCrudeInfo.selectedCutSetDataTable = oilTable;
                //OilPropertyForm oilPropertyForm = new OilPropertyForm(oilTable);
                //oilPropertyForm.Show();
            }
            
        }

        public void OpenCutSetList(int oilId, bool copy = false)
        {
            var list = _context.CutSets.Where(p=>p.CrudeId == oilId).ToList();
            var name = _context.Crudes.Where(p => p.CrudeId == oilId).FirstOrDefault().Name;
            
            if (list.Count == 1)
            {
                MainFormController controller = new MainFormController(MainFormController._form);

                if (!copy)
                {
                    DataTable oilTable = controller.CreateOilPropertyTable(oilId, list[0].CutSetId);
                    MainFormController.selectedCrudeInfo.SetInfoByCrudeId(oilId);
                    MainFormController.selectedCrudeInfo.selectedCutSetDataTable = oilTable;
                }
                else
                {
                    DataTable oilTable = controller.CreateOilPropertyTable(oilId, list[0].CutSetId);
                    MainFormController.selectedCrudeInfo.SetInfoByCrudeId(oilId);
                    //Crude 
                    Crude newCrude = new Crude();
                    newCrude.Name = name;
                    newCrude.LibraryId = 1;

                    //CutSet
                    CutSet newCutSet = new CutSet();
                    newCutSet.CutSetType = list[0].CutSetType;
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
                    MainFormController.selectedCrudeInfo.selectedCutSetDataTable = oilTable;
                    MainFormController _controller = new MainFormController(MainFormController._form);
                    _controller.AddNewOilToDataBase();
                }

                return;
            }
            CutSetListForm newForm = new CutSetListForm(oilId, list, name);
            newForm.Show();

        } 
        public void Undo()
        {
            if (folderStack.Count != 0)
            {
                folderStack.Pop();
                if (folderStack.Count != 0)
                {
                    OpenNodeContent(folderStack.Peek());
                    folderStack.Pop();
                }
                else
                {
                    FillPrimaryLibraryList(_form.listView1);
                }
            }
            else
            {
                FillPrimaryLibraryList(_form.listView1);
            }

        }
        //TODO: разобраться нужен ли тут статик
        public static DataTable CreateOilPropertyTable(AllCrudeInfo allCrudeInfo) 
        {
            _form.label1.Text = allCrudeInfo.crude.Name;
            allCrudeInfo = selectedCrudeInfo;
            var cuts = selectedCrudeInfo.selectedCutSetCuts;
            var properties = _context.PropertyTypes.ToList();

            DataTable propertyTable = new DataTable();
            //Add Columns
            propertyTable.Columns.Add("Свойство", System.Type.GetType("System.String"));
            propertyTable.Columns.Add("Ед. изм.", typeof(string));
            
            
            foreach (var cut in cuts)
            {
                propertyTable.Columns.Add(cut.Name, System.Type.GetType("System.Double"));
            }
            
            //Add Rows 
            foreach (var prop in properties)
            {
                propertyTable.Rows.Add();
            }

            //Заполнение данных
            for (int i = 0; i < cuts.Count; i++)
            {
                var cutProperties = selectedCrudeInfo.selectedCutSetProperties
                    .Where(p => p.CutName == cuts[i].Name).ToList();
                foreach (var property in cutProperties)
                {
                    if (property.PropertyTypeId <= propertyTable.Rows.Count)
                    {
                        propertyTable.Rows[property.PropertyTypeId - 1][i + 2] = property.Value;
                        if (property.Uom != null)
                        {
                            propertyTable.Rows[property.PropertyTypeId - 1][1] = _context.Uoms
                                .Where(p => p.Uomid == property.Uom).FirstOrDefault().Name;
                        }
                        propertyTable.Rows[property.PropertyTypeId - 1][0] = _context.PropertyTypes
                                                                                .Where(p => p.PropertyTypeId == property.PropertyTypeId).FirstOrDefault().Name;
                    }
                }
            }
            //Удаление пустых столбцов
            for (int i = propertyTable.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = propertyTable.Rows[i];
                if (row[0].ToString() == "")
                {
                    row.Delete();
                }

            }
            //TODO:убрать или придумать как реализовать общую переменную для данных для таблицы
            tableOilInfo = propertyTable;
            //_form.dataGridView1.Columns[0].Frozen = true;
            //_form.dataGridView1.Columns[1].Frozen = true;
            return propertyTable;
        }
        //TODO: Сделать единый метод  
        public DataTable CreateOilPropertyTable(int CrudeId, int CutSetId)
        {
            using (dbSMOilContext _context = new dbSMOilContext())
            {
                Crude crude = _context.Crudes.Where(p => p.CrudeId == CrudeId).FirstOrDefault();
                var neededCutSet = _context.CutSets.Where(p => p.CutSetId == CutSetId).FirstOrDefault();
                var cuts = _context.Cuts.Where(p => p.CutSetId == neededCutSet.CutSetId).ToList();
                var properties = _context.PropertyTypes.ToList();

                DataTable propertyTable = new DataTable();
                //Add Columns
                propertyTable.Columns.Add("Свойство", System.Type.GetType("System.String"));
                propertyTable.Columns.Add("Ед. изм.", typeof(string));

                foreach (var cut in cuts)
                {
                    propertyTable.Columns.Add(cut.Name, System.Type.GetType("System.Double"));
                }
                //Add Rows 
                foreach (var prop in properties)
                {
                    propertyTable.Rows.Add();
                }

                //Заполнение данных
                for (int i = 0; i < cuts.Count; i++)
                {
                    var cutProperties = _context.Properties
                        .Where(p => p.CutId == cuts[i].CutId).ToList();
                    foreach (var property in cutProperties)
                    {
                        if (property.PropertyTypeId <= propertyTable.Rows.Count)
                        {
                            propertyTable.Rows[property.PropertyTypeId - 1][i + 2] = property.Value;
                            if (property.Uom != null)
                            {
                                
                                propertyTable.Rows[property.PropertyTypeId - 1][1] = _context.Uoms
                                                            .Where(p => p.Uomid == property.Uom).FirstOrDefault().Name;
                            }
                            
                            propertyTable.Rows[property.PropertyTypeId - 1][0] = _context.PropertyTypes
                                                                                    .Where(p => p.PropertyTypeId == property.PropertyTypeId).FirstOrDefault().Name;
                        }
                    }
                }
                //Удаление пустых столбцов
                for (int i = propertyTable.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = propertyTable.Rows[i];
                    if (row[0].ToString() == "")
                    {
                        row.Delete();
                    }

                }

                _form.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                _form.dataGridView1.Columns.Clear();
                //selectedCrudeInfo.selectedCutSetDataTable= propertyTable;
                tableOilInfo = propertyTable;
                
                _form.dataGridView1.DataSource = tableOilInfo;
                _form.label1.Text = crude.Name;
                _form.dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                //TODO:вытащить настройки датагрида (и других статичных объектов в отдельный метод)
                //
                //Дроп список для единиц измерения
                DataGridViewComboBoxColumn column = new DataGridViewComboBoxColumn();
                column.HeaderText = "Единицы Измерения";
                column.Name = "Единицы Измерения";
                _form.dataGridView1.Columns.Add(column);
                for(int i = 0;i< _form.dataGridView1.Rows.Count-1;i++)
                {
                    //TODO сделать 1 запрос
                    var data2 = _context.PropertyTypes
                                    .Where(p => p.Name == _form.dataGridView1["Свойство", i].Value.ToString()).FirstOrDefault().PropertyGroupId;
                    var data = _context.PropertyGroups.Where(p => p.PropertyGroupId == data2).FirstOrDefault().Uomset;
                    if (data == null)
                    {
                        continue;
                    }
                    var data3 = _context.Uoms.Where(p => p.Uomset == data).Select(p=>p.Name).ToList();
                    DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
                    //cell.Items.AddRange(data);
                    DataGridViewComboBoxCell a = (DataGridViewComboBoxCell)_form.dataGridView1["Единицы Измерения", i];
                    //foreach(var b in data)
                    //{
                    //    a.Items.Add(b.Name);
                    //}
                    a.DataSource = data3;
                    int index = a.Items.IndexOf(_form.dataGridView1["Ед. изм.", i].Value.ToString());
                    //TODO содержание всякой шняги поменять с процентной доли на массовое количество
                    if(index == -1)
                    {
                        List<string> list = new List<string>();
                        list.Add(_form.dataGridView1["Ед. изм.", i].Value.ToString());
                        a.DataSource = list;
                        a.Value = _form.dataGridView1["Ед. изм.", i].Value.ToString();
                    }
                    else
                    {
                        a.Value = a.Items[index];
                    }
                    
                    //_form.dataGridView1["Единицы Измерения",i] = cell;
                }
                _form.dataGridView1.Columns["Единицы Измерения"].DisplayIndex = 2;
                _form.dataGridView1.Columns["Ед. изм."].Visible = false;
                //
                //
                for (int i = 1; i < _form.dataGridView1.ColumnCount; i++)
                {
                    _form.dataGridView1.Columns[i].DefaultCellStyle.Format = "##0.000";
                    _form.dataGridView1.Columns[i].Width = 60;
                    _form.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                _form.dataGridView1.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;

                _form.dataGridView1.Refresh();
                _form.dataGridView1.Columns[0].Frozen = true;

                _form.dataGridView1.Columns[1].Frozen = true;

                return propertyTable;
            }
            
        }
        public void SetUOM()
        {
            
        }
        public void OpenNewCrudeForm()
        {
            CreateNewCrudeForm form = new CreateNewCrudeForm(_form);
            form.Show();
        }
        public void AddNewOilToDataBase()
        {
            //Add Crude
            Crude newOil = new Crude();
            newOil.Name = selectedCrudeInfo.crude.Name;
            _context.Crudes.Add(newOil);
            _context.SaveChanges();
            selectedCrudeInfo.crude = newOil;
            //TODO: сделать нормальное добавление разгонки
            //Add CutSet
            CutSet newCutSet = new CutSet();
            newCutSet = selectedCrudeInfo.selectedCutSet;
            newCutSet.CrudeId = newOil.CrudeId;
            newCutSet.isMainCutSet = true;
            _context.CutSets.Add(newCutSet);
            _context.SaveChanges();

            //Add Cuts and Properties
            foreach(var cut in selectedCrudeInfo.selectedCutSetCuts)
            {
                var newCut = new Cut();
                newCut.CutSetId = newCutSet.CutSetId;
                newCut.CutType = cut.CutType;
                newCut.Description = cut.Description;
                newCut.Name = cut.Name;
                _context.Cuts.Add(newCut);
                _context.SaveChanges();

                List<Property> newProperties = new List<Property>();
                var cutProperties = selectedCrudeInfo.selectedCutSetProperties
                                                         .Where(p => p.CutName == newCut.Name).ToList();
                foreach(var property in cutProperties)
                {
                    Property newProperty = new Property();
                    newProperty.CutId = newCut.CutId;
                    newProperty.CutName = newCut.Name;
                    newProperty.PropertyTypeId = property.PropertyTypeId;
                    newProperty.Uom = property.Uom;
                    newProperty.Value = property.Value;
                    newProperties.Add(newProperty);


                }
                _context.Properties.AddRange(newProperties);

                _context.SaveChanges();
                foreach (var prop in newProperties)
                {
                    var entity = _context.Properties.Attach(prop);
                    entity.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                _context.SaveChanges();
                MainFormController._form.label1.Text = newOil.Name;
            }

        }

        public void AddNewCutSetToDataBase()
        {
            //Add CutSet
            CutSet newCutSet = new CutSet();
            newCutSet = selectedCrudeInfo.selectedCutSet;
            newCutSet.isMainCutSet = false;
            _context.CutSets.Add(newCutSet);
            _context.SaveChanges();

            //Add Cuts and Properties
            foreach (var cut in selectedCrudeInfo.selectedCutSetCuts)
            {
                var newCut = new Cut();
                newCut.CutSetId = newCutSet.CutSetId;
                newCut.CutType = cut.CutType;
                newCut.Description = cut.Description;
                newCut.Name = cut.Name;
                _context.Cuts.Add(newCut);
                _context.SaveChanges();

                List<Property> newProperties = new List<Property>();
                var cutProperties = selectedCrudeInfo.selectedCutSetProperties
                                                         .Where(p => p.CutName == newCut.Name).ToList();
                foreach (var property in cutProperties)
                {
                    Property newProperty = new Property();
                    newProperty.CutId = newCut.CutId;
                    newProperty.CutName = newCut.Name;
                    newProperty.PropertyTypeId = property.PropertyTypeId;
                    newProperty.Uom = property.Uom;
                    newProperty.Value = property.Value;
                    newProperties.Add(newProperty);


                }
                _context.Properties.AddRange(newProperties);
                _context.SaveChanges();
                MainFormController._form.label1.Text = selectedCrudeInfo.crude.Name;
            }
        }
        public int SaveChanges()
        {
            var newProperties = MainFormController.selectedCrudeInfo.selectedCutSetProperties;
            List<int> deletedProperties = new List<int>();
            foreach(var property in newProperties)
            {
                var state = _context.Entry(property).State.ToString();
                if (property.Value == -9999)
                {
                    _context.Properties.Remove(property);
                    deletedProperties.Add(MainFormController.selectedCrudeInfo.selectedCutSetProperties.IndexOf(property));
                    _context.SaveChanges();
                    continue;
                }
                if (state == "Modified")
                {

                    _context.Update(property);

                }
                if(state== "Added")
                {
                    _context.Properties.Add(property);

                }
                if (state == "Detached")
                {
                    _context.Properties.Add(property);

                }

            }
            foreach(int i in deletedProperties)
            {
                MainFormController.selectedCrudeInfo.selectedCutSetProperties.RemoveAt(i);
            }
            _context.SaveChanges();
            return 0;
        }

        //удаление нефти выведенной в таблицу
        public void DeleteSelectedCrude()
        {
            var crude = CrudeById(selectedCrudeInfo.crude.CrudeId);
            var cutSets = _context.CutSets.Where(p => p.CrudeId == selectedCrudeInfo.crude.CrudeId).ToList();
            foreach (var cutSet in cutSets)
            {
                var cuts = _context.Cuts.Where(p => p.CutSetId == cutSet.CutSetId).ToList();
                foreach (var cut in cuts)
                {
                    var properties = _context.Properties.Where(p => p.CutId == cut.CutId).ToList();
                    _context.Properties.RemoveRange(properties);
                }
                _context.Cuts.RemoveRange(cuts);

            }
            _context.CutSets.RemoveRange(cutSets);
            _context.Crudes.RemoveRange(crude);
            _context.SaveChanges();
            MainFormController.selectedCrudeInfo.Clear();
            
            _form.dataGridView1.Columns.Clear();
            _form.dataGridView2.Columns.Clear();
            _form.label1.Text = "";
            FillPrimaryLibraryList(_form.listView1);
        }
        //Копирование таблицы данных разгонки 
        public DataTable CopyDataFromTable()
        {
            DataTable table1 = (DataTable)_form.dataGridView1.DataSource;
            DataTable table2 = (DataTable)_form.dataGridView2.DataSource;
            DataTable newCopyTable = _form.tabControl1.SelectedIndex == 0 ? table1.Copy() : table2.Copy();
            return newCopyTable;
        }
        
        public Crude CrudeById(int id)
        {
            var crude = _context.Crudes.Where(p => p.CrudeId == id).FirstOrDefault();
            return crude;
        }
        
        public void DeleteCrudeById(int id)
        {
            if (selectedCrudeInfo.crude != null && selectedCrudeInfo.crude.CrudeId == id)
            {
                DeleteSelectedCrude();
                return;
            }
            var crude = CrudeById(id);
            var cutSets = _context.CutSets.Where(p => p.CrudeId == id).ToList();
            foreach (var cutSet in cutSets)
            {
                var cuts = _context.Cuts.Where(p => p.CutSetId == cutSet.CutSetId).ToList();
                foreach (var cut in cuts)
                {
                    var properties = _context.Properties.Where(p => p.CutId == cut.CutId).ToList();
                    _context.Properties.RemoveRange(properties);
                }
                _context.Cuts.RemoveRange(cuts);

            }
            _context.CutSets.RemoveRange(cutSets);
            _context.Crudes.RemoveRange(crude);
            _context.SaveChanges();

        }
        public static void testFUnc()
        {
            //var f = new SmprFunctions.Functions();
            //f.Language = 1049;
            //f.TemperatureC = true;
            //object a = new object[] { "FLS", 10 };
            //object b = null;
            //Console.WriteLine(f.Calculate("FlsToFli",ref a,ref b));
        }
    }
}
