using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class PropertyType
    {
        public int PropertyTypeId { get; set; }
        public bool? IsIndexProperty { get; set; }
        public int? PropertyGroupId { get; set; }
        public double? ExtendedValue { get; set; }
        public string Name { get; set; }
    }
}
