using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Models.ReturnToCN;

namespace WMSWebAPI.Dtos
{
    public class DTO_ORIN
    {
        public ReturnHeader returnHeader { get; set; }
        public ReturnHeader[] returnHeaders { get; set; }

        public ReturnDetails[] returnLines{ get; set; }
        public CNReason cNReason{ get; set; }
    }
}
