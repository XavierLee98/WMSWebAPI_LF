using DbClass;
using System.Collections.Generic;
using WMSWebAPI.Models.Do;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.ReturnRequest;

namespace WMSWebAPI.Dtos
{
    public class DTO_OPKL
    {

        public OPKL_Ex OPKL { get; set; }
        public OPKL_Ex[] OPKLs { get; set; }
        public List<PKL1_Ex> pKL1s { get; set; }
        public OIBT[] oIBTs { get; set; }

        //ReserveInvoice
        public OINV_Ex[] OINVs { get; set; }
        public INV1_Ex[] iNV1s { get; set; } 
        public NNM1[] DocSeries { get; set; }
        public List<OINV_Ex2> OINVs2 { get; set; } = new List<OINV_Ex2>();

        public PickerModel picker { get; set; }
        public Driver driver { get; set; }
        public Truck truck { get; set; }

        public PickerModel[] pickers { get; set; }
        public OWHS[] WarehouseList { get; set; }
        public Driver[] drivers { get; set; }
        public Truck[] trucks { get; set; }
        public string IsEnableItemValidate { get; set; }

    }
}
