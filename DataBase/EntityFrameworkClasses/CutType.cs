using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class CutType
    {
        public int CutTypeId { get; set; }
        public string Name { get; set; }
        public double? Ivt { get; set; }
        public double? Fvt { get; set; }
        public string CutSetType { get; set; }
    }
}
