using DbClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMSWebAPI.Dtos
{
    public class DTO_Master
    {
        public OWHS[] dtoWhs { get; set; }
        public OCRD[] Bp { get; set; }
        public OBIN[] DtoBins { get; set; }
    }
}
