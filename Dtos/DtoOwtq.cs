using DbClass;
using WMSWebAPI.Models.Lifewater;

namespace WMSWebAPI.Dtos
{
    public class DtoOwtq
    {
        public NNM1[] DocSeries { get; set; }
        public OPLN_Ex[] PriceList { get; set; }
    }
}
