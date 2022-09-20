using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class OITM_Ex:OITM
    {
        public decimal U_Weight { get; set; }

        public bool isChecked { get; set; }
    }
}
