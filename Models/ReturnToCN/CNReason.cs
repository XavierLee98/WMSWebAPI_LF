using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.ReturnToCN
{
    public class CNReason
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string U_WhseCode { get; set; }
        public string U_ShowPortal { get; set; }
        public string U_ShowReport { get; set; }
    }
}
