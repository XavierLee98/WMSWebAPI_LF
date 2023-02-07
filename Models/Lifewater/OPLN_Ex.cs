using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.Lifewater
{
    public class OPLN_Ex:OPLN
    {
        public string U_Module { get; set; }
        public string U_Value{ get; set; }
    }
}
