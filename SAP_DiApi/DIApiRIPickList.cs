using Dapper;
using Microsoft.Extensions.Configuration;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.ReturnRequest;
using WMSWebAPI.Models.SAP_DiApi;

namespace WMSWebAPI.SAP_DiApi
{
    public class DIApiRIPickList : IDisposable
    {
        public void Dispose() => GC.Collect();
        Company oCom { get; set; }
        public string LastErrorMessage { get; private set; }

        IConfiguration _configuration;
        string _dbConnectionStr;

        public DIApiRIPickList(IConfiguration configuration, string dbConnectionStr)
        {
            _configuration = configuration;
            _dbConnectionStr = dbConnectionStr;
        }


    }
}

//    public SAPParam GetSAPSetting()
//    {
//        try
//        {
//            string query = "SELECT TOP 1 * FROM ft_SAPSettings";

//            var conn = new SqlConnection(_dbConnectionStr);
//            var result = conn.Query<SAPParam>(query).FirstOrDefault();

//            return result;
//        }
//        catch (Exception excep)
//        {
//            LastErrorMessage = "Fail to Get SAP Setting.";
//            Console.WriteLine(excep);
//            return null;
//        }
//    }

//    public int GetSAPCompany()
//    {
//        try
//        {
//            var company = GetSAPSetting();
//            if (company == null) return -1;

//            oCom = new SAPbobsCOM.Company();

//            oCom.Server = company.Server;
//            oCom.CompanyDB = company.CompanyDB;
//            oCom.DbUserName = company.DbUserName;
//            oCom.DbPassword = company.DbPassword;
//            oCom.DbServerType = (SAPbobsCOM.BoDataServerTypes)int.Parse(company.DbServerType);
//            oCom.UserName = company.UserName;
//            oCom.Password = company.Password;
//            oCom.SLDServer = company.SLDServer;

//            oCom.Connect();

//            if (oCom.Connected)
//                return 0;
//        }
//        catch (Exception e)
//        {
//            LastErrorMessage = "Fail to Connect SAP";

//            Console.WriteLine(e);
//        }
//        return -1;
//    }

//    /// <summary>
//    ///  Assign Batches Numbers to the Items (Reserve Invoice)
//    /// </summary>
//    /// <param name="selectedINV1"></param>
//    /// <param name="oIBT"></param>
//    /// <param name="Picked"></param>
//    /// <returns></returns>
//    public int AssignBatchToRI(INV1_Ex selectedINV1, List<OBTQ_Ex> oBTQs)
//    {
//        try
//        {
//            //var connect = GetSAPCompany();
//            //if (connect == -1) return -1;


//            //SAPbobsCOM.Documents oDocuments = null;
//            //SAPbobsCOM.Document_Lines oDocument_Lines = null;
//            //oDocuments = (SAPbobsCOM.Documents)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

//            //oDocuments.GetByKey(selectedINV1.DocEntry);
//            //oDocument_Lines = oDocuments.Lines;
//            //oDocument_Lines.SetCurrentLine(selectedINV1.LineNum);
//            //oDocument_Lines.BatchNumbers.BatchNumber = oIBT.BatchNum;
//            //oDocument_Lines.BatchNumbers.Quantity = (double)Picked;
//            //oDocument_Lines.BatchNumbers.Add();

//            //int RetVal = oDocuments.Update();
//            //if (RetVal < 0)
//            //{
//            //    LastErrorMessage = "Fail Assign Batch in Reserve Invoice \n";
//            //    LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//            //    return -1;
//            //}
//            //return RetVal;
//            var connect = GetSAPCompany();
//            if (connect == -1) return -1;


//            SAPbobsCOM.Documents oDocuments = null;
//            SAPbobsCOM.Document_Lines oDocument_Lines = null;
//            oDocuments = (SAPbobsCOM.Documents)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

//            oDocuments.GetByKey(selectedINV1.DocEntry);
//            oDocument_Lines = oDocuments.Lines;
//            oDocument_Lines.SetCurrentLine(selectedINV1.LineNum);
//            foreach(var line in oBTQs)
//            {
//                oDocument_Lines.BatchNumbers.BatchNumber = line.DistNumber;
//                oDocument_Lines.BatchNumbers.Quantity = (double)line.TransferBatchQty;
//                oDocument_Lines.BatchNumbers.Add();
//            }


//            int RetVal = oDocuments.Update();
//            if (RetVal < 0)
//            {
//                LastErrorMessage = "Fail Assign Batch in Reserve Invoice \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return -1;
//            }
//            return RetVal;
//        }
//        catch (Exception e)
//        {
//            {
//                LastErrorMessage = $"{e} \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return -1;
//            }
//        }

//    }

//    /// <summary>
//    /// Cancel Allocated Batch For all PickLiset ItemLine(INV1) (Reserve Invoice)
//    /// </summary>
//    /// <param name="batches"></param>
//    /// <returns></returns>
//    public int RICancelAssignAllBatchesForAllItem(Cio bag)
//    {
//        try
//        {
//            var connect = GetSAPCompany();
//            if (connect == -1) return -1;
//            SAPbobsCOM.Documents oDocuments = null;
//            SAPbobsCOM.Document_Lines oDocument_Lines = null;
//            oDocuments = (SAPbobsCOM.Documents)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);


//            foreach(var line in bag.INV1s)
//            {
//                oDocuments.GetByKey(line.DocEntry);
//                oDocument_Lines = oDocuments.Lines;
//                oDocument_Lines.SetCurrentLine(line.LineNum);
//                if (line.oBTQList != null)
//                {
//                    foreach (var batch in line.oBTQList)
//                    {
//                        oDocument_Lines.BatchNumbers.BatchNumber = batch.DistNumber;
//                        oDocument_Lines.BatchNumbers.Quantity = (double)0;
//                        oDocument_Lines.BatchNumbers.Add();
//                    }
//                }

//            }

//            int RetVal = oDocuments.Update();
//            if (RetVal < 0)
//            {
//                LastErrorMessage = "Fail Assign Batch in Reserve Invoice \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return -1;
//            }
//            return RetVal;
//        }
//        catch (Exception e)
//        {
//            LastErrorMessage = $"{e} \n";
//            LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//            return -1;
//        }
//    }

//    /// <summary>
//    /// Cancel Allocated Single Batch(INV1) (Reserve Invoice)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    public int RICancelAssignSingleBatch(Cio bag)
//    {
//        try
//        {
//            var connect = GetSAPCompany();
//            if (connect == -1) return -1;
//            SAPbobsCOM.Documents oDocuments = null;
//            oDocuments = (SAPbobsCOM.Documents)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

//            oDocuments.GetByKey(bag.RIItemLine.DocEntry);
//            oDocuments.Lines.SetCurrentLine(bag.RIItemLine.LineNum);
//            oDocuments.Lines.BatchNumbers.BatchNumber = bag.oIBT.DistNumber;
//            oDocuments.Lines.BatchNumbers.Quantity = 0;

//            int RetVal = oDocuments.Update();
//            if (RetVal < 0)
//            {
//                LastErrorMessage = "Fail Assign Batch in Reserve Invoice \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return -1;
//            }
//            return RetVal;
//        }
//        catch (Exception e)
//        {
//            LastErrorMessage = $"{e} \n";
//            LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//            return -1;
//        }
//    }

//    /// <summary>
//    /// Cancel Allocated All Batches For Single Item(INV1) (Reserve Invoice)
//    /// </summary>
//    /// <param name="batches"></param>
//    /// <returns></returns>
//    public int RICancelAssignBatchForSingleItem(Cio bag)
//    {
//        try
//        {
//            var connect = GetSAPCompany();
//            if (connect == -1) return -1;
//            SAPbobsCOM.Documents oDocuments = null;
//            oDocuments = (SAPbobsCOM.Documents)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

//            oDocuments.GetByKey(bag.RIItemLine.DocEntry);
//            oDocuments.Lines.SetCurrentLine(bag.RIItemLine.LineNum);
//            foreach (var line in bag.oBTQs)
//            {
//                oDocuments.Lines.BatchNumbers.BatchNumber = line.DistNumber;
//                oDocuments.Lines.BatchNumbers.Quantity = 0;
//            }

//            int RetVal = oDocuments.Update();
//            if (RetVal < 0)
//            {
//                LastErrorMessage = "Fail Remove Batch in Reserve Invoice \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return -1;
//            }
//            return RetVal;
//        }
//        catch (Exception e)
//        {
//            LastErrorMessage = $"{e} \n";
//            LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//            return -1;
//        }

//    }

//    /// <summary>
//    /// Create Actual PickList From RI(INIV1)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    public string RICreatePickList(Cio bag)
//    {
//        try
//        {
//            var connect = GetSAPCompany();
//            if (connect == -1) return null;

//            SAPbobsCOM.Documents oDocuments = null;
//            SAPbobsCOM.Document_Lines oDocument_Lines = null;
//            SAPbobsCOM.PickLists oPickLists = null;
//            SAPbobsCOM.PickLists_Lines oPickLists_Lines = null;

//            oPickLists = (SAPbobsCOM.PickLists)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPickLists);
//            oPickLists_Lines = oPickLists.Lines;
//            oPickLists.Remarks = bag.PickHead.Remarks;
//            oPickLists.UserFields.Fields.Item("U_Picker").Value = bag.PickHead.Name;
//            oPickLists.UserFields.Fields.Item("U_Driver").Value = bag.PickHead.U_Driver;
//            oPickLists.UserFields.Fields.Item("U_TruckNo").Value = bag.PickHead.U_TruckNo;
//            oPickLists.UserFields.Fields.Item("U_DeliveryType").Value = bag.PickHead.U_DeliveryType;
//            oPickLists.UserFields.Fields.Item("U_Weight").Value = (double)bag.PickHead.U_Weight;
//            var invoiceDocs = bag.INV1s.Select(x => x.DocEntry).Distinct().ToList();


//            foreach (var line in invoiceDocs)
//            {
//                oDocuments = (SAPbobsCOM.Documents)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

//                oDocuments.GetByKey(line);

//                oDocument_Lines = oDocuments.Lines;
//                for (int i = 0; i < oDocument_Lines.Count; i++)
//                {
//                    oDocument_Lines.SetCurrentLine(i);
//                    oPickLists_Lines.BaseObjectType = "13";
//                    oPickLists_Lines.OrderEntry = oDocuments.DocEntry;
//                    oPickLists_Lines.OrderRowID = i;
//                    oPickLists_Lines.ReleasedQuantity = oDocument_Lines.Quantity;
//                    var Qty = oDocument_Lines.Quantity;
//                    oPickLists_Lines.Add();

//                }
//            }
//            int RetVal = oPickLists.Add();
//            if (RetVal < 0)
//            {
//                LastErrorMessage = "Fail Assign Batch in Reserve Invoice \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return null;
//            }
//            var PickDoc = oCom.GetNewObjectKey();
//            return PickDoc;
//        }
//        catch (Exception e)
//        {
//            LastErrorMessage = $"{e}\n";
//            LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//            return null;
//        }

//    }

//    /// <summary>
//    /// Pick and Assign Batches from INV1 to actual PickList
//    /// </summary>
//    /// <param name="PickDoc"></param>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    public int RIPickBatchAfterCreated(string PickDoc, Cio bag)
//    {
//        try
//        {
//            var connect = GetSAPCompany();
//            if (connect == -1) return -1;

//            SAPbobsCOM.PickLists oPickLists = null;
//            SAPbobsCOM.PickLists_Lines oPickLists_Lines = null;

//            oPickLists = (SAPbobsCOM.PickLists)oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPickLists);

//            oPickLists.GetByKey(Int32.Parse(PickDoc));
//            oPickLists_Lines = oPickLists.Lines;
//            var text = oPickLists.Lines.Count;

//                for (int i = 0; i < oPickLists_Lines.Count; i++)
//                {
//                    oPickLists_Lines.SetCurrentLine(i);

//                         foreach (var line in bag.INV1s)
//                         {
//                             if (line.DocEntry == oPickLists_Lines.OrderEntry && line.LineNum == oPickLists_Lines.OrderRowID)
//                             {
//                                 oPickLists_Lines.PickedQuantity = (double)line.U_PickedQty;

//                                 if (line.oBTQList.Count != 0)
//                                 {
//                                     for (int j = 0; j < line.oBTQList.Count; j++)
//                                     {
//                                         oPickLists_Lines.BatchNumbers.BaseLineNumber = i;
//                                         oPickLists_Lines.BatchNumbers.BatchNumber = line.oBTQList[j].DistNumber;
//                                         oPickLists_Lines.BatchNumbers.Quantity = (double)line.oBTQList[j].TransferBatchQty;
//                                         oPickLists_Lines.BatchNumbers.Add();
//                                     }
//                                 }
//                             }
//                         }
//                }

//            int RetVal = oPickLists.Update();
//            if (RetVal < 0)
//            {
//                LastErrorMessage = "Fail Assign Batch to Pick List \n";
//                LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//                return -1;
//            }
//            return RetVal;
//        }
//        catch (Exception e)
//        {
//            LastErrorMessage = $"{e}\n";
//            LastErrorMessage += $"{oCom.GetLastErrorCode()} - {oCom.GetLastErrorDescription()}";
//            return -1;
//        }
//    }
//}


