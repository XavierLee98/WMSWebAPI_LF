using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class ORDR_Ex2:ORDR
    {
        public List<RDR1_Ex2> rDR1s { get; set; } = new List<RDR1_Ex2>();
    }
}
