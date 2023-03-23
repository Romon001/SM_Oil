using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class Property
    {
        public int Id { get; set; }
        public int CutId { get; set; }
        public double Value { get; set; }
        public int? Uom { get; set; }
        public int PropertyTypeId { get; set; }
        public string CutName { get; set; }
    }
}
