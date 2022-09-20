using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.InventoryCount
{
    public class INC1_DIST
    {
        public Guid guid { get; set; }
        public int SourceBaseEntry { get; set; }
        public int SourceBaseLine { get; set; }
        public string ItemCode { get; set; }
        public decimal TotalQty { get; set; }
        public decimal Quantity { get; set; }
        public string BatchNumber { get; set; }
        public string Warehouse { get; set; }
    }
}
