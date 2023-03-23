using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class Folder
    {
        public int FolderId { get; set; }
        public string Name { get; set; }
        public int? Library { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ChangeDate { get; set; }
        public int Owner { get; set; }
        public int ChangeUser { get; set; }
    }
}
