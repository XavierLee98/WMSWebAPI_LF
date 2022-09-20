using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.ReturnToCN
{
    public class ReturnDetails
    {
        public int Id { get; set; }
        public string DocEntry { get; set; }
        public Guid Guid { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public decimal UnitPrice { get; set; }
        public string ToWhsCode { get; set; }
        public string Batch { get; set; }
        public string CNReason { get; set; }
        public bool IsChecked { get; set; }
        public decimal GoodQty { get; set; }
        public decimal QuantityPassed { get; set; }
        public DateTime ManufactureDate { get; set; }



    }
}
