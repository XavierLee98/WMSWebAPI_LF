using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class OIBT
    {
        public string ItemCode { get; set; }
        public string DistNumber { get; set; }
        public string ItemName { get; set; }
        public int SysNumber { get; set; }
        public string WhsCode { get; set; }
        public string Status { get; set; }
        public DateTime ExpDate { get; set; }
        public DateTime MnfDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal CommitQty { get; set; }
        public decimal CountQty { get; set; }
        public int AbsEntry { get; set; }
        public int MdAbsEntry { get; set; }
        public int TrackingNt { get; set; }
        public decimal CCDQuant { get; set; }
        public double Picked { get; set; }

    }
}
