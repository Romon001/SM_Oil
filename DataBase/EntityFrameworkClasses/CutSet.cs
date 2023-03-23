using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class CutSet
    {
        public int CutSetId { get; set; }
        public int CrudeId { get; set; }
        public int? CutSetType { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public bool isMainCutSet { get; set; }
    }
}
