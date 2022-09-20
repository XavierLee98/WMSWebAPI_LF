using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class BatchVariance
    {
        public string Batch { get; set; }
        public int PickListNo { get; set; }
        public string PickListType { get; set; }
        public string ItemCode { get; set; }
        public decimal SystemQty { get; set; }
        public decimal ActualQty { get; set; }
        public decimal Variance { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
