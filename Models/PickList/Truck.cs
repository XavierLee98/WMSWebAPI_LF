using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class Truck
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string U_Model { get; set; }
        public decimal U_MaxLoad { get; set; }
        public decimal U_MinLoad { get; set; }
        public string U_Measurement { get; set; }

    }
}
