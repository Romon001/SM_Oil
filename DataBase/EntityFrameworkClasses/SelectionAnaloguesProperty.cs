using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class SelectionAnaloguesProperty
    {
        public int Id { get; set; }
        public int PropertyType { get; set; }
        public int SelectionAnalogues { get; set; }
        public int Uom { get; set; }
        public double Value { get; set; }
    }
}
