using System;

namespace WMSWebAPI.Models.Request
{
    public class zwaGRPO
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public string SourceCardCode { get; set; }
        public int SourceDocNum { get; set; }
        public int SourceLineNum { get; set; }
        public int SourceDocEntry { get; set; }
        public int SourceDocBaseType { get; set; }
        public int SourceBaseEntry { get; set; }
        public int SourceBaseLine { get; set; }
        public string Warehouse { get; set; } // add in 20200634T1834
        public string SourceDocType { get; set; }
        public Guid LineGuid { get; set; } // to identified a line in doc
        public string Remarks { get; set; } // add into alone line remarks 
        public string ReasonCode { get; set; }  // reason code 
        public string ReasonName { get; set; } // reason name by line 
        public string AcctCode { get; set; } // GR / BI acct code
        public string AcctName { get; set; }
        public decimal LineWeight { get; set; }
        public string Machine { get; set; }
    }
}
