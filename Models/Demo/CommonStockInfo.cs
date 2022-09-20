using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.Demo
{
    public class CommonStockInfo
    {
        public int SysNumber { get; set; }
        public string DistNumber { get; set; }
        public string MnfSerial { get; set; }
        public string LotNumber { get; set; }
        public DateTime InDate { get; set; }
        public string Status { get; set; }
        public int AbsEntry { get; set; }
        public int BinAbs { get; set; }
        public int SnBMDAbs { get; set; }
        public string ItemCode { get; set; }
        public string itemName { get; set; }
        public string WhsCode { get; set; }
        public decimal OnHandQty { get; set; }
        public string BinCode { get; set; }
    }
}
