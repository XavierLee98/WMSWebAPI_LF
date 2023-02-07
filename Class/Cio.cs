using DbClass;
using System;
using System.Collections.Generic;
using WMSWebAPI.Dtos;
using WMSWebAPI.Models;
using WMSWebAPI.Models.BWY;
using WMSWebAPI.Models.Company;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Demo.Transfer1;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.InventoryCount;
using WMSWebAPI.Models.Lifewater;
using WMSWebAPI.Models.Lifewater.GRGI;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.Request;
using WMSWebAPI.Models.ReturnRequest;
using WMSWebAPI.Models.ReturnToCN;

namespace WMSWebAPI.Class
{
    /// <summary>
    /// Serve as the bag to transmit data between app and server
    /// </summary>
    public class Cio
    {
        public string request { get; set; }
        public string token { get; set; }
        public string sap_logon_name { get; set; }
        public string sap_logon_pw { get; set; }
        public string phoneRegID { get; set; }
        public string companyName { get; set; }
        //public int companyId { get; set; }
        public zwaUser newzwaUser { get; set; }
        public string currentUser { get; set; }
        public string currentUserRole { get; set; } // added 20200626 for indentity admin group 

        // ----------------- current user permission reference, infor from server 

        public zwaUserGroup1[] currentPermissions { get; set; }
        public zwaUserGroup currentGroup { get; set; }

        // for bearer token 
        //public BearerToken bearerToken { get; set; }

        /// <summary>
        /// For WMS Purchase order entities in bag
        /// </summary>

        public OCRD[] PoBp { get; set; }

        //public OPOR[] Po { get; set; }

        //public POR1[] PoLines { get; set; }

        // public int[] PODocEntries { get; set; }

        // public string[] CardCodes { get; set; }

        public string getPoType { get; set; } //<--- aded by johnny on 20200614
                                              // public int poDocEntry { get; set; }// 20200613T 2154 for query the PO lines 

        // 20200615 
        public zwaRequest dtoRequest { get; set; } // data transfer object for request create ERP doc
                                                   // public POR1_Ex[] dtoPOR1_Ex { get; set; } // data transfer object for request create ERP doc

        public zwaGRPO[] dtoGRPO { get; set; } // data transfer object for request create ERP doc

        // 20200616
        //public ORDR[] So { get; set; }
        public string getSoType { get; set; } //<--- aded by johnny on 20200616
        //public RDR1[] SoLines { get; set; }
        //public int soDocEntry { get; set; }
        //public zwaGRPO[] dtoDeliveryOrder { get; set; }

        public string checkedItemCodeWhsQty { get; set; } // for checking the item code whs qty
                                                          //  public OITW[] dtoWhsItemQtys { get; set; } // for checking the item code whs qty

        // 20200616T0946
        public OWHS[] dtoWhs { get; set; } // for checking the item code whs qty
                                           // 20200616T0946        
        public zmwRequest dtoDocStatus { get; set; } // for replied the check of the doc status

        public string checkDocGuid { get; set; } // use by App to check the guid creation

        // 20200618T0927
        public OITM[] Items { get; set; } // get list if the items from database
        public string QueryItemCode { get; set; } // get single item of the item code
        public OITM Item { get; set; } // get list if the items from database

        // 20200619
        public OINC_Ex[] InvenCountDocs { get; set; } // return list of the inventory counting open doc
        public string OINCStatus { get; set; } //<--- aded by johnny on 20200616
        public INC1_Ex[] InvenCountDocsLines { get; set; } // return list of the inventory counting open doc lines
        public int OINCDocEntry { get; set; } /// use to query the oinc doc line, based doc entry

        public OBIN[] LocationBin { get; set; } // return list of the inventory counting open doc lines

        // 20200621T1950
        // add in the user and user group into the app
        public zwaUser[] zwAppUsers { get; set; }
        //public OADM_Ex[] oADM_CompanyInfoList { get; set; }
        public OADM[] oADM_CompanyInfoList { get; set; }
        public zwaUserGroup[] zwaGroupList { get; set; }

        public int newUserGroupTempId { get; set; }
        public zwaUserGroup newUserGroup { get; set; }
        public zwaUserGroup1[] newUserGroupPermission { get; set; }
        public zwaUser[] newUserGroupUsr { get; set; }
        public int groupId { get; set; }
        public zwaUserGroup1[] zwaUserGroupsPermission { get; set; }

        //  public string PersonName { get; set; } // added for customized menu

        // 20200624T1046
        public OPLN[] PriceList { get; set; } // for app price list selection
        public int ExistingGrPriceListId { get; set; } // for keeping the exiting prices list setup
        public int UpdateGrPriceListId { get; set; } // for keeping the exiting prices list setup
        public string UpdateGrDocSeries { get; set; } /// use to update the goods receipt doc series
        public string UpdateIssueDocSeries { get; set; } // use to update good issue doc series
        public string ExistingGrDocSeries { get; set; }
        public string ExistingGIDocSeries { get; set; }
        public int ExistingGiPriceListId { get; set; } // for keeping the exiting prices list setup // 202006282330
        public int UpdateGiPriceListId { get; set; } // for keeping the exiting prices list setup // 20200628T2330
        // 20200718T1023
        public OBIN[] DtoBins { get; set; } // for query the bins location for warehouse
        public string QueryWhs { get; set; }
        // 20200719T1037
        public zwaItemBin[] dtoItemBins { get; set; }
        // 20200920T1321
        public OWTQ[] TransferRequestList { get; set; }
        public WTQ1[] TransferRequestLine { get; set; }
        public int TransferRequestDocEntry { get; set; }
        public DateTime RequestTransferStartDt { get; set; }
        public DateTime RequestTransferEndDt { get; set; }

        public zwaLicencse2 compamyLic { get; internal set; }

        //// 20200621T2005 add in for the app user management
        //public zwaUser[] zwAppUsers { get; set; }
        public string keys { get; set; }
        public zwaCompany zwaCompanyNewEdit { get; set; }

        public zmwRequest[] ProblemsRequest { get; set; }

        ///// for bwyapp setup
        public string QueryDocNumber { get; set; } // App scan PO doc, Doc Doc and so on
        public OPOR DtoPo { get; set; }
        public POR1[] DtoPoLines { get; set; }

        //// for IBT In // 20200707T1049
        public OWTR DtoTransferDoc { get; set; }
        public WTR1[] DtoTransferDocLines { get; set; }

        //// for 20200708T1527 for inventory counting
        //public zwainvtCount[] DtoInvtCountList { get; set; }
        //public zwaInvtCount1[] DtoInvtCounters { get; set; }

        //// 20200708T1254
        public AppsStockCounts[] DtoAppInvtCountList { get; set; } // based on web portal adjusment
        public AppsStockCounts[] DtoAppInvtCounters { get; set; }  // for insert the stock count list
        public AppsGRPO[] DtoGrPoAndLines { get; set; } // for insert the grpo po and lines
        public AppsIBTIn[] DtoIBTInAndLines { get; set; } // for insert the grpo po and lines
                                                          //// 20200716T2149
        public string InvCountOutLet { get; set; } // for different the out let stock list
        public string NewEncrptedPw { get; set; } // for reset user password, the value in encryted

        //// 20200726T1559
        public string InvtCntItemCode { get; set; }
        public ItemMasters FoundItem { get; set; }
        //// 20200729T2208
        public string GrpoDoInvVerificationDocNum { get; set; }
        //// 20200730R1114
        public string CurrentIBTRequestGroupName { get; set; } // for control the doc checking for own outlet
        // 20200921T1706
        public zwaInventoryRequest[] dtoInventoryRequest { get; set; }
        public zwaInventoryRequestHead dtoInventoryRequestHead { get; set; }
        public string RequestTransferDocFilter { get; set; } // for filter all, open and close
        // 20200924T1147
        public string QueryItemWhsCode { get; set; }
        public OITW oITW { get; set; }
        //public FTS_vw_IMApp_ItemWhsBin[] ItemWhsBinList { get; set; }
        // 20200925
        public zwaTransferDetails[] dtoTransferDetails { get; set; }
        public zwaTransferHead dtoTransferHead { get; set; }
        public zwaItemTransferBin[] dtoDetailsBins { get; set; }
        // 20200927
        public BinContent[] dtoBinContents { get; set; }
        public StockTransactionLog[] dtoStockTransLogs { get; set; }
        public DateTime QueryStartDate { get; set; }
        public DateTime QueryEndDate { get; set; }
        public CommonStockInfo[] CommonStockInfos { get; set; } // 20201002
        // 20201011 for transfer 2 
        public string TransferItemCode { get; set; }
        public string TransferQueryCode { get; set; }
        public string TransferWhsCode { get; set; }
        public OITM TransferFoundItem { get; set; }
        public OBIN TransferFoundBin { get; set; }
        public OSRN TransferFoundSerial { get; set; }
        public OBTN TransferFoundBatch { get; set; }
        public OSBQ TransferBinSerialAccumulator { get; set; }
        public OBBQ TransferBinBatchAccumulator { get; set; }
        public OIBQ TransferBinAccumulator { get; set; }
        public OSRI TransferOSRI { get; set; } // 20201016T1428
        public string TransSerialCode { get; set; }
        public OBTQ TransferBatch { get; set; } // 20201016T1633
        public int TransBatchAbs { get; set; }
        public OSBQ_Ex[] TransferBinContentSerial { get; set; } // 20201018
        public OSRQ_Ex[] TransferContentSerial { get; set; } // 20201018T1139
        public OBBQ_Ex[] TransferBinContentBatch { get; set; } // 20201019T
        public OBTQ_Ex[] TransferContentBatch { get; set; } // 20201018T1139
        public OIBQ_Ex[] TransferBinItems { get; set; } // 20201019T1946
        public zwaHoldRequest TransferHoldRequest { get; set; }
        public zwaTransferDocHeader TransferDocHeader { get; set; }
        public zwaTransferDocDetails[] TransferDocDetails { get; set; }
        public zwaTransferDocDetailsBin[] TransferDocDetailsBins { get; set; }
        //public int TransferDocRequestEntry { get; set; }
        public int TransferDocRequestBaseLine { get; set; }
        public OBIN[] WarehouseBin { get; set; }
        public zwaTransferDocDetails TransferDocDetail { get; set; }
        public int STAHoldRequestId { get; set; } // 20201031T1547 return the inserted row id for show in app
        public zwaHoldRequest[] dtozwaHoldRequests { get; set; } // 20201031T1909
        public zwaTransferDocDetails[] dtozmwTransferDocDetails { get; set; }// 20201101T1111
        public zwaTransferDocDetailsBin[] dtozwaTransferDocDetailsBin { get; set; } // 20201101T1111
        public OPLN_Ex[] dtoPriceList { get; set; }
        public Guid TransferDocRequestGuid { get; set; }
        public Guid TransferDocRequestGuidLine { get; set; }
        public int[] PoDocEntries { get; set; }
        public zmwDocHeaderField dtozmwDocHeaderField { get; set; }
        public int QueryDocEntry { get; set; }
        public int QueryDocLineNum { get; set; }
        public string QueryDistNum { get; set; }

        public string QueryFromWhs { get; set; }
        public string QueryToWhs { get; set; }

        public string FromWhsItemAval { get; set; }
        public string ToWhsItemAval { get; set; }

        //KX
        public string PickStatus { get; set; }
        public DTO_OPKL[] OPKLs { get; set; }
        public int PickDoc { get; set; }
        public string ItemCodeInput { get; set; }
        public INV1_Ex RIItemLine { get; set; }
        public decimal Weight { get; set; }
        public List<INV1_Ex> INV1s { get; set; }
        public OIBT oIBT { get; set; }
        public PKL1_Ex PickItemLine { get; set; }

        public decimal Picked { get; set; }
        public PKL1_Ex[] pKL1List { get; set; }
        public List<OBTQ_Ex> oBTQs { get; set; }
        public OBTQ_Ex oBTQ { get; set; }
        public int InvoiceDoc { get; set; }
        public OPKL_Ex PickHead { get; set; }
        public PickListHeader PickHeader { get; set; }
        public List<OINV_Ex2> oINVs { get; set; }
        public List<OITM_Ex> ItemDetails { get; set; }
        public string IsEnableItemValidate { get; set; }

        public List<HoldPickItem> holdPickItems { get; set; }
        public BatchVariance SinglebatchVariance { get; set; }
        public string QueryPicker { get; set; }


        //Market Return

        public string QueryStringDocEntry { get; set; }
        public Guid QueryGuid { get; set; }

        public ReturnHeader ReturnHeader { get; set; }
        public List<ReturnDetails> ReturnDetails { get; set; }

        public Cio() { }
    }
}
