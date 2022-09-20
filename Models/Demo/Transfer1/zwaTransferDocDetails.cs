using System;
namespace WMSWebAPI.Models.Demo.Transfer1
{
    public class zwaTransferDocDetails
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid LineGuid { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public string FromWhsCode { get; set; }
        public string ToWhsCode { get; set; }
        public string Serial { get; set; }
        public string Batch { get; set; }
        public string SourceDocBaseType { get; set; }
        public int SourceBaseEntry { get; set; }
        public int SourceBaseLine { get; set; }
        public decimal ActualReceiptQty { get; set; }
         
    }
}
