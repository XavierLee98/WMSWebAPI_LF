using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.Demo.Transfer1
{
    /// <summary>
    /// Use by the transfer request on hold 
    /// 20201020
    /// </summary>
    public class zwaHoldRequest // database 
    {
        public int Id { get; set; }
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string Picker { get; set; }
        public DateTime TransDate { get; set; }
        public Guid HeadGuid { get; set; }
        public string Status { get; set; }
    }
}
