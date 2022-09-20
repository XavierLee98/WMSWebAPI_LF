using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Models.ReturnRequest;

namespace WMSWebAPI.Models.PickList
{
    public class OINV_Ex2 : OINV
    {
        public List<INV1_Ex> iNV1s { get; set; } = new List<INV1_Ex>();
        public bool IsChecked { get; set; }
    }
}
