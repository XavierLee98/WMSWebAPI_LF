namespace WMSWebAPI.ClassObj.Warehouse
{
    public class ItemObj
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string WhsCode { get; set; } = string.Empty;
        public string DistNumber { get; set; } = string.Empty;
        public string BinCode { get; set; } = string.Empty;

        public decimal OnHandQty { get; set; } = 0;
        public decimal OrderQty { get; set; } = 0;
        public decimal CommitQty { get; set; } = 0;
        public decimal CountQty { get; set; } = 0;

        public string request { get; set; } = string.Empty;

        //public decimal OnHand { get; set; }
        //public decimal Quantity { get; set; }
        //public decimal IsCommited { get; set; }
    }
}
