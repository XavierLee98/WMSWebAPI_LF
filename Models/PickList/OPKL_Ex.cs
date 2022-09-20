using DbClass;
using System;

namespace WMSWebAPI.Models.PickList
{
    public class OPKL_Ex : OPKL
    {
        public Guid Guid { get; set; }
        public string U_Driver { get; set; }
        public string U_TruckNo { get; set; }
        public string U_Picker { get; set; }
        public string WhsCode { get; set; }
        public string U_DeliveryType { get; set; }
        public decimal U_Weight { get; set; }
        public string U_Cancel { get; set; }
        public string DocStatus { get; set; }
        public int SAPPickDoc { get; set; }

    }
}
