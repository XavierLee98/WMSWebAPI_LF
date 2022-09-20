using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Do;

namespace WMSWebAPI.Models.PickList
{
    public class PKL1_Ex: PKL1
    {
        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public decimal ReleaseQuantity { get; set; }
        public decimal TotalPicked { get; set; }
        public decimal U_Weight { get; set; }

        public List<OIBT> batchList { get; set; }
        public List<OBTQ_Ex> oBTQList { get; set; }

        public int SODocEntry { get; set; }
        public int SOEntry { get; set; }


        public RDR1_Ex rDR1 { get; set; }
        public ORDR_Ex oRDR { get; set; }

        public List<HoldPickItem> OnHoldBatches { get; set; } = new List<HoldPickItem>();

        public List<BatchAllocateDocView> AllocatedBatches { get; set; } = new List<BatchAllocateDocView>();



    }
}
