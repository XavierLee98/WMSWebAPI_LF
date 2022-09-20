using DbClass;
using WMSWebAPI.Models.Do;

namespace WMSWebAPI.Controllers
{
    public class DTO_ORDR
    {
        public ORDR[] ORDRs { get; set; }
        public RDR1[] RDR1s { get; set; }
        public ORDR_Ex[] ORDR_Exs { get; set; }
        public RDR1_Ex[] RDR1_Exs { get; set; }
        public NNM1[] DocSeries { get; set; }
        public OCRD[] Bp { get; set; }
    }
}
