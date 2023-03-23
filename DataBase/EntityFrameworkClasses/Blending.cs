using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class Blending
    {
        public int BlendingId { get; set; }
        public int Crude { get; set; }
        public int Basis { get; set; }
        public int CutSetType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ChangeDate { get; set; }
        public int Owner { get; set; }
        public int ChangeUser { get; set; }
        public string Description { get; set; }
        public string ResultInfo { get; set; }
    }
}
