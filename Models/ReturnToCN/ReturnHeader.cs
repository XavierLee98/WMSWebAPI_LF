using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.ReturnToCN
{
    public class ReturnHeader
    {
        public string DocEntry { get; set; }
        public Guid Guid { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CustName { get; set; }
        public string CustCode { get; set; }
        public string Driver { get; set; }
        public string DriverName { get; set; }
        public string Remark { get; set; }
        public decimal TotalPrice { get; set; }
        public string NumAtCard { get; set; }
        public string DocStatus { get; set; }
        public string DocNum { get; set; }
        public string CreatedUser { get; set; }


    }
}
