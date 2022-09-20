using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.ClassObject.Warehouse
{
    public class AvailableItemObj
    {
        public string WhsCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItmsGrpNam { get; set; }
        public decimal OnHand { get; set; }
    }
}
