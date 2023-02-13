using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using SAPbobsCOM;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.ReturnRequest;
using WMSWebAPI.Models.SAP_DiApi;

namespace WMSWebAPI.SAP_DiApi
{
    public class DiApiUpdatePickList : IDisposable
    {
        public void Dispose() => GC.Collect();
        readonly string _success = "Success";
        readonly string _fail = "Fail";
        public string LastErrorMessage { get; private set; }
        string _dbConnectionStr;
        IConfiguration _configuration;
        SAPCompany _company;

        public DiApiUpdatePickList(IConfiguration configuration, string dbConnectionStr, SAPCompany company)
        {
            _configuration = configuration;
            _dbConnectionStr = dbConnectionStr;
            _company = company;
        }

        /// <summary>
        /// Cancel Allocated Single Batch(RDR1) (Sale Order)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int SOCancelAssignSingleBatch(PKL1_Ex pickLine, OBTQ_Ex batch)
        {
            try
            {
                if (!_company.connectSAP())
                    throw new Exception(_company.errMsg);

                SAPbobsCOM.Documents oDocuments = null;

                oDocuments = (SAPbobsCOM.Documents)_company.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                oDocuments.GetByKey(pickLine.OrderEntry);
                for (int x = 0; x < oDocuments.Lines.Count; x++)
                {
                    oDocuments.Lines.SetCurrentLine(x);
                    if (oDocuments.Lines.DocEntry == pickLine.OrderEntry && oDocuments.Lines.LineNum == pickLine.OrderLine)
                        break;
                }

                if (!string.IsNullOrEmpty(oDocuments.Lines.BatchNumbers.BatchNumber))
                {
                    for (int v = 0; v < oDocuments.Lines.BatchNumbers.Count; v++)
                    {
                        oDocuments.Lines.BatchNumbers.SetCurrentLine(v);

                        if (oDocuments.Lines.BatchNumbers.BatchNumber == batch.DistNumber)
                        {
                            if(oDocuments.Lines.BatchNumbers.Quantity <= double.Parse(batch.TransferBatchQty.ToString()))
                            {
                                oDocuments.Lines.BatchNumbers.Quantity = 0;
                                continue;
                            }

                            oDocuments.Lines.BatchNumbers.Quantity -= double.Parse(batch.TransferBatchQty.ToString());
                        } 
                    }
                }

                int RetVal = oDocuments.Update();
                if (RetVal != 0)
                {
                    LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";
                    InsertPickAlloactedLog(pickLine, batch, "Remove", _fail, LastErrorMessage);
                    return -1;
                }
                InsertPickAlloactedLog(pickLine, batch, "Remove", _success, LastErrorMessage);

                if (oDocuments != null) Marshal.ReleaseComObject(oDocuments);
                oDocuments = null;

                return RetVal;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message} \n";
                InsertPickAlloactedLog(pickLine, batch, "Remove", _fail, LastErrorMessage);
                return -1;
            }
        }

        /// <summary>
        ///  Assign Single Batch to the (SO)
        /// </summary>
        /// <param name="pKL1Line"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public int AssignBatchToSo(PKL1_Ex pKL1Line, OBTQ_Ex batch)
        {
            SAPbobsCOM.Documents oDocuments = null;

            try
            {
                if (!_company.connectSAP())
                    throw new Exception(_company.errMsg);

                oDocuments = (SAPbobsCOM.Documents)_company.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                oDocuments.GetByKey(pKL1Line.OrderEntry);
                for (int x = 0; x < oDocuments.Lines.Count; x++)
                {
                    oDocuments.Lines.SetCurrentLine(x);
                    if (oDocuments.Lines.DocEntry == pKL1Line.OrderEntry && oDocuments.Lines.LineNum == pKL1Line.OrderLine)
                        break;
                }

                if(!string.IsNullOrEmpty(oDocuments.Lines.BatchNumbers.BatchNumber)) oDocuments.Lines.BatchNumbers.Add();
                oDocuments.Lines.BatchNumbers.BatchNumber = batch.DistNumber;
                oDocuments.Lines.BatchNumbers.Quantity += (double)batch.TransferBatchQty;

                int RetVal = oDocuments.Update();

                if (RetVal != 0)
                {
                    LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";
                    InsertPickAlloactedLog(pKL1Line, batch, "Insert", _fail, LastErrorMessage);

                    return -1;
                }

                InsertPickAlloactedLog(pKL1Line, batch, "Insert", _success, LastErrorMessage);

                return RetVal;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message} \n";
                InsertPickAlloactedLog(pKL1Line, batch, "Insert", _fail, LastErrorMessage);

                return -1;
            }
            finally
            {
                if (oDocuments != null) Marshal.ReleaseComObject(oDocuments);
                oDocuments = null;
            }
        }

        void InsertPickAlloactedLog(PKL1_Ex pKL1Line, OBTQ_Ex batch, string action, string status, string errmsg)
        {
            try
            {
                var conn = new SqlConnection(_dbConnectionStr);

                var result = conn.Execute("sp_InsertPickListAllocateItemTransaction",
                    new
                    {
                        SODocEntry = pKL1Line.OrderEntry,
                        SOLineNum = pKL1Line.OrderLine,
                        PickListDocEntry = pKL1Line.AbsEntry,
                        PickListLineNum = pKL1Line.PickEntry,
                        ItemCode = pKL1Line.ItemCode,
                        ItemDesc = pKL1Line.Dscription,
                        Batch = batch.DistNumber,
                        Quantity = batch.TransferBatchQty,
                        PickAction = action,
                        status = status,
                        Errormsg = errmsg
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep.Message} \n";
            }
        }

        /// <summary>
        /// Update PickList Header
        /// </summary>
        /// <param name="PickHead"></param>
        /// <returns></returns>
        //public int UpdatePickListHeader(OPKL_Ex PickHead)
        //{

        //    try
        //    {
        //        if (!_company.connectSAP())
        //        {
        //            throw new Exception(_company.errMsg);
        //        }

        //        PickLists oPickLists = null;
        //        PickLists_Lines oPickLists_Lines = null;
        //        oPickLists = (PickLists)_company.oCom.GetBusinessObject(BoObjectTypes.oPickLists);
        //        oPickLists.GetByKey(PickHead.AbsEntry);
        //        oPickLists.Name = PickHead.U_Picker;
        //        oPickLists.Remarks = PickHead.Remarks;
        //        oPickLists.UserFields.Fields.Item("U_Driver").Value = PickHead.U_Driver;
        //        oPickLists.UserFields.Fields.Item("U_TruckNo").Value = PickHead.U_TruckNo;
        //        oPickLists.UserFields.Fields.Item("U_DeliveryType").Value = PickHead.U_DeliveryType;
        //        oPickLists_Lines = oPickLists.Lines;

        //        int RetVal = oPickLists.Update();

        //        if (RetVal != 0)
        //        {
        //            LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";

        //            if (oPickLists != null) Marshal.ReleaseComObject(oPickLists);
        //            oPickLists = null;

        //            return -1;
        //        }

        //        if (oPickLists != null) Marshal.ReleaseComObject(oPickLists);
        //        oPickLists = null;

        //        return RetVal;
        //    }
        //    catch (Exception e)
        //    {
        //        LastErrorMessage = $"{e.Message} \n";
        //        return -1;
        //    }
        //}
    }
}

///// <summary>
///// Update Pick List and Partially Pick Items in Pick List
///// </summary>
///// <param name="PicDoc"></param>
///// <param name="pKL1s"></param>
///// <param name="PickHead"></param>
///// <returns></returns>
//public int PartialUpdatePickList(int PicDoc, PKL1_Ex[] pKL1s, OPKL_Ex PickHead)
//{
//    try
//    {
//        if (!_company.connectSAP())
//        {
//            throw new Exception(_company.errMsg);
//        }

//        PickLists oPickLists = null;
//        PickLists_Lines oPickLists_Lines = null;

//        oPickLists = (PickLists)_company.oCom.GetBusinessObject(BoObjectTypes.oPickLists);
//        oPickLists.GetByKey(PicDoc);
//        oPickLists.UserFields.Fields.Item("U_Weight").Value = (double)PickHead.U_Weight;

//        oPickLists_Lines = oPickLists.Lines;
//        pKL1s.OrderBy(x => x.PickEntry);
//        foreach (var line in pKL1s)
//        {
//            oPickLists_Lines.SetCurrentLine(line.PickEntry);
//            if (oPickLists_Lines.PickStatus != BoPickStatus.ps_Closed)
//            {
//                oPickLists_Lines.PickedQuantity = (double)line.TotalPicked;
//                oPickLists_Lines.UserFields.Fields.Item("U_Weight").Value = (double)line.U_Weight;
//                if (line.TotalPicked != 0)
//                {
//                    foreach (var batch in line.oBTQList)
//                    {
//                        if (batch.TransferBatchQty != 0)
//                        {
//                            oPickLists_Lines.BatchNumbers.BatchNumber = batch.DistNumber;
//                            oPickLists_Lines.BatchNumbers.Quantity = (double)batch.TransferBatchQty;
//                            oPickLists_Lines.BatchNumbers.BaseLineNumber = line.PickEntry;
//                            oPickLists_Lines.BatchNumbers.Add();
//                        }
//                    }
//                }
//            }
//        }
//        int RetVal = oPickLists.Update();

//        if (RetVal != 0)
//        {
//            LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";
//            return -1;
//        }
//        return RetVal;
//    }
//    catch (Exception e)
//    {
//        LastErrorMessage += $"{e.Message}";
//        return -1;
//    }
//}

//public DiApiUpdatePickList(IConfiguration configuration, string dbConnectionStr, SAPCompany company)
//{
//    _configuration = configuration;
//    _dbConnectionStr = dbConnectionStr;
//    _company = company;
//}

//public SAPParam GetSAPSetting()
//{
//    try
//    {
//        string query = "SELECT TOP 1 UserName, DBUser [DbUserName],DBPass [DbPassword], SAPCompany [CompanyDB], DBType [DbServerType], LicenseServer [LicenseServer], Server, SAPUser [UserName], SAPPass [Password]  FROM ft_SAPSettings";

//        var conn = new SqlConnection(_dbConnectionStr);
//        var result = conn.Query<SAPParam>(query).FirstOrDefault();

//        return result;
//    }
//    catch (Exception excep)
//    {
//        LastErrorMessage = "Fail to Get SAP Setting.";
//        return null;
//    }
//}

//public int GetSAPCompany()
//{
//    try
//    {
//        var company = GetSAPSetting();
//        if (company == null) return -1;

//        oCom = new SAPbobsCOM.Company();

//        if (oCom.Connected) return 0;

//        oCom.Server = company.Server;
//        oCom.CompanyDB = company.CompanyDB;
//        oCom.DbUserName = company.DbUserName;
//        oCom.DbPassword = company.DbPassword;
//        oCom.DbServerType = (SAPbobsCOM.BoDataServerTypes)int.Parse(company.DbServerType);
//        oCom.UserName = company.UserName;
//        oCom.Password = company.Password;
//        oCom.LicenseServer = company.LicenseServer;

//        if (oCom.Connect() != 0)
//        {
//            LastErrorMessage = oCom.GetLastErrorDescription();
//            return -1;
//        }

//        return 0;
//    }
//    catch (Exception e)
//    {
//        LastErrorMessage = $"Fail to Connect SAP. {e.Message}";
//        return -1;
//    }
//}