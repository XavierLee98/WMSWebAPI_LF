using DbClass;
using System.Collections.Generic;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.PickList;

namespace WMSWebAPI.Models.Do
{
    public class RDR1_Ex : RDR1
    {
        public List<OBTQ_Ex> BatchesInWhs { get; set; }


    }
}
