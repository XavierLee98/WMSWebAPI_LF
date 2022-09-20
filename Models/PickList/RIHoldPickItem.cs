using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class RIHoldPickItem
    {
            public int RIDocEntry { get; set; }
            public int RILineNum { get; set; }
            public int PickListDocEntry { get; set; }
            public string ItemCode { get; set; }
            public string ItemDesc { get; set; }
            public string Batch { get; set; }
            public decimal Quantity { get; set; }
            public DateTime AllocatedDate { get; set; }
            public string PickStatus { get; set; }
    }
}
