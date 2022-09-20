using DbClass;
using System.Collections.Generic;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.PickList;

namespace WMSWebAPI.Models.ReturnRequest
{
    public class INV1_Ex : INV1
    {
        public int DocNum { get; set; }
        public string CardName { get; set; }
        public int U_PickListNo { get; set; }
        public double U_PickedQty { get; set; }
        public decimal TotalPicked { get; set; }
        public List<BatchAllocateDocView> AllocatedBatches { get; set; } = new List<BatchAllocateDocView>();
        public List<RIHoldPickItem> OnHoldBatches { get; set; } = new List<RIHoldPickItem>();

        public List<OBTQ_Ex> oBTQList { get; set; } = new List<OBTQ_Ex>();

    }
}
