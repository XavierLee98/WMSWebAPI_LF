using System;
namespace WMSWebAPI.Models.Demo.Transfer1
{
    public class zwaTransferDocDetailsBin
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid LineGuid { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public string Serial { get; set; }
        public string Batch { get; set; }
        public string InternalSerialNumber { get; set; }
        public string ManufacturerSerialNumber { get; set; }
        public int BinAbs { get; set; }
        public string BinCode { get; set; }
        public int SnBMDAbs { get; set; }
        public string WhsCode { get; set; }
        public string Direction { get; set; }
    }
}
