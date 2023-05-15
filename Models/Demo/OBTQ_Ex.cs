using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.Demo
{
    public class OBTQ_Ex
    { 
        public string ItemCode { get; set; }
        public int SysNumber { get; set; }
        public string WhsCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal CommitQty { get; set; }
        public decimal CountQty { get; set; }
        public decimal AvailableQty { get; set; }
        public int AbsEntry { get; set; }
        public int MdAbsEntry { get; set; }
        public int TrackingNt { get; set; }
        public int TrackiNtLn { get; set; }
        public int CCDQuant { get; set; }
        public string DistNumber { get; set; }
        public DateTime InDate { get; set; }
        public bool IsPickedItem { get; set; }

        public decimal TransferBatchQty { get; set; }
        public DateTime? ExpDate { get; set; }
        public string Status { get; set; }

        public decimal ActualPickQty { get; set; }
        public decimal DraftQty { get; set; }
        public decimal TotalQty { get; set; }

    }
}
