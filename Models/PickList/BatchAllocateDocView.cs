using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.PickList
{
    public class BatchAllocateDocView
    {
        public int SnBAllocateViewLogEntry { get; set; }
        public int SnBAllocateViewDocType { get; set; }
        public int SnBAllocateViewDocEntry { get; set; }
        public int SnBAllocateViewDocLine { get; set; }
        public int SnBAllocateViewMngBy { get; set; }
        public int SnBAllocateViewSnbMdAbs { get; set; }
        public string SnBAllocateViewItemCode { get; set; }
        public int SnBAllocateViewSnbSysNum { get; set; }
        public string SnBAllocateViewLocCode { get; set; }
        public decimal SnBAllocateViewAllocQty { get; set; }
        public string DistNumber { get; set; }
    }
}
