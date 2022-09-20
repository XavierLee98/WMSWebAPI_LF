using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class PickListHeader
    {
        public Guid Guid { get; set; }
        public int AbsEntry { get; set; }
        public string Name { get; set; }
        public short OwnerCode { get; set; }
        public string OwnerName { get; set; }
        public DateTime PickDate { get; set; }
        public string Remarks { get; set; }
        public string Canceled { get; set; }
        public short ShipType { get; set; }
        public string Status { get; set; }
        public string Printed { get; set; }
        public int LogInstac { get; set; }
        public string ObjType { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public short UserSign { get; set; }
        public short UserSign2 { get; set; }
        public string UseBaseUn { get; set; }
        public string U_Driver { get; set; }
        public string U_TruckNo { get; set; }
        public string U_DeliveryType { get; set; }
        public decimal U_Weight { get; set; }
        public string U_Cancel { get; set; }
        public string DocStatus { get; set; }
    }
}
