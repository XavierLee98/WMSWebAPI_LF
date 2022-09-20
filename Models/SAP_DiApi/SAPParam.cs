    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Models.SAP_DiApi
{
    public class SAPParam
    {
        public string Server { get; set; }
        public string CompanyDB { get; set; }
        public string DbUserName { get; set; }
        public string DbPassword { get; set; }
        public string DbServerType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SLDServer { get; set; }
        public string LicenseServer { get; set; }
    }
}
