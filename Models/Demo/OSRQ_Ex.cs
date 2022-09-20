using System;

namespace WMSWebAPI.Models.Demo
{
    public class OSRQ_Ex
    {
        public string ItemCode { get; set; }
        public int SysNumber { get; set; }
        public string WhsCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal CommitQty { get; set; }
        public decimal CountQty { get; set; }
        public int AbsEntry { get; set; }
        public int MdAbsEntry {get; set;}
        public int TrackingNt { get; set; }
        public int TrackiNtLn { get; set; }
        public int CCDQuant { get; set; }
        public string DistNumber { get; set; }
        public DateTime InDate { get; set; }
    }
}
