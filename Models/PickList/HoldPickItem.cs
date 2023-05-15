using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class HoldPickItem
    {
        public int Id { get; set; }
        public int SODocEntry { get; set; }
        public int SOLineNum { get; set; }
        public int PickListDocEntry { get; set; }
        public int PickListLineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public string Batch { get; set; }
        public decimal Quantity { get; set; }
        public decimal SAPPickedQty { get; set; }
        public DateTime AllocatedDate { get; set; }
        public string LineStatus { get; set; }
        public bool IsCancelled { get; set; }
    }
}
