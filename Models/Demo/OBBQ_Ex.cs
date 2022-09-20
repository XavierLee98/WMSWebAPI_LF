using System;
namespace WMSWebAPI.Models.Demo
{
    public class OBBQ_Ex
    {
        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public int SnBMDAbs { get; set; }
        public int BinAbs { get; set; }
        public decimal OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public string BinCode { get; set; }
        public string DistNumber { get; set; }
        public DateTime InDate { get; set; }
    }
}
