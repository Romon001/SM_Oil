using System;
using System.Collections.Generic;

#nullable disable

namespace SM_Oil
{
    public partial class Cut 
    {
        public int CutId { get; set; }
        public int CutSetId { get; set; }
        public int? CutType { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }

        public Cut Clone()
        {
            Cut newCut = new Cut();
            newCut.CutId = this.CutId; 
            newCut.CutSetId= this.CutSetId; 
            newCut.CutType = this.CutType; 
            newCut.Description = this.Description; 
            newCut.Name = this.Name;
            return newCut;
        }
    }
}
