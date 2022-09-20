using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.Demo.Transfer1
{
    public class zwaTransferDocHeader
    {
        public int Id { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime TaxDate { get; set; }
        public string FromWhsCode { get; set; }
        public string ToWhsCode { get; set; }
        public string JrnlMemo { get; set; }
        public string Comments { get; set; }
        public Guid Guid { get; set; }
        public string DocNumber { get; set; }
        public string DocStatus { get; set; }
        public string LastErrorMessage { get; set; }
    }
}
