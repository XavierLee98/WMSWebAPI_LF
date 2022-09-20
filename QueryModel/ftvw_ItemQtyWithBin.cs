using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.QueryModel
{
    public class ftvw_ItemQtyWithBin
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal OnHand { get; set; }
        public decimal OnOrder { get; set; }
        public decimal IsCommited { get; set; }
        public string WhsCode { get; set; }
        public string BinCode { get; set; }
        public string DistNumber { get; set; }
    }
}
