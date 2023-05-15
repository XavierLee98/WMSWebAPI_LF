using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Do;

namespace WMSWebAPI.Models.PickList
{
    public class PKL1_Ex: PKL1
    {
        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public decimal TotalPicked { get; set; }
        public decimal U_Weight { get; set; }

        public List<OBTQ_Ex> oBTQList { get; set; } = new List<OBTQ_Ex>();

        //Refactored New Code
        public int VisOrder { get; set; }
        public int BaseDocNum { get; set; }

        public string WhsCode { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public decimal ItemWeight { get; set; }

        public char ManBtchNum { get; set; }
        public char ManSerNum { get; set; }
    }
}
