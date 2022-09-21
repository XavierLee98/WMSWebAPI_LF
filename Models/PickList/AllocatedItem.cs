using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class AllocatedItem
    {
        public int SnBAllocateViewLogEntry { get; set; }
        public int DocType { get; set; }
        public int DocEntry { get; set; }
        public int DocLine { get; set; }
        public int ManagedBy { get; set; }
        public int MdAbsEntry { get; set; }
        public string ItemCode { get; set; }
        public int SysNumber { get; set; }
        public string WhsCode { get; set; }
        public decimal AllocQty { get; set; }
        public string DistNumber { get; set; }
    }
}
