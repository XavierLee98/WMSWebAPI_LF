using System;
using System.Collections.Generic;
using System.Data;
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
        SAPCompany _company;

        public DiApiUpdatePickList( string dbConnectionStr, SAPCompany company)
        {
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
                bool isfound = false;

                oDocuments = (SAPbobsCOM.Documents)_company.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                oDocuments.GetByKey(pickLine.OrderEntry);
                for (int x = 0; x < oDocuments.Lines.Count; x++)
                {
                    oDocuments.Lines.SetCurrentLine(x);
                    if (oDocuments.Lines.DocEntry == pickLine.OrderEntry && oDocuments.Lines.LineNum == pickLine.OrderLine)
                    {
                        isfound = true;
                        break;
                    }
                }

                if (isfound)
                {
                    if (!string.IsNullOrEmpty(oDocuments.Lines.BatchNumbers.BatchNumber))
                    {
                        for (int v = 0; v < oDocuments.Lines.BatchNumbers.Count; v++)
                        {
                            oDocuments.Lines.BatchNumbers.SetCurrentLine(v);

                            if (oDocuments.Lines.BatchNumbers.BatchNumber == batch.DistNumber)
                            {
                                if (oDocuments.Lines.BatchNumbers.Quantity <= double.Parse(batch.DraftQty.ToString()))
                                {
                                    oDocuments.Lines.BatchNumbers.Quantity = 0;
                                    continue;
                                }

                                oDocuments.Lines.BatchNumbers.Quantity -= double.Parse(batch.DraftQty.ToString());
                            }
                        }
                    }
                }

                int RetVal = oDocuments.Update();
                if (RetVal != 0)
                {
                    LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";
                    return -1;
                }

                RemoveBatch(pickLine, batch);

                if (oDocuments != null) Marshal.ReleaseComObject(oDocuments);
                oDocuments = null;

                return RetVal;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message} \n";
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

                bool isfound = false;

                oDocuments = (SAPbobsCOM.Documents)_company.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                oDocuments.GetByKey(pKL1Line.OrderEntry);
                for (int x = 0; x < oDocuments.Lines.Count; x++)
                {
                    oDocuments.Lines.SetCurrentLine(x);
                    if (oDocuments.Lines.DocEntry == pKL1Line.OrderEntry && oDocuments.Lines.LineNum == pKL1Line.OrderLine)
                    {
                        isfound = true;
                        break;
                    }
                }

                if (isfound)
                {
                    if (!string.IsNullOrEmpty(oDocuments.Lines.BatchNumbers.BatchNumber)) oDocuments.Lines.BatchNumbers.Add();
                    oDocuments.Lines.BatchNumbers.BatchNumber = batch.DistNumber;
                    oDocuments.Lines.BatchNumbers.Quantity += (double)batch.TransferBatchQty;
                }

                int RetVal = oDocuments.Update();

                if (RetVal != 0)
                {
                    LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";

                    return -1;
                }

                AddBatch(pKL1Line, batch);

                return RetVal;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message} \n";
                return -1;
            }
            finally
            {
                if (oDocuments != null) Marshal.ReleaseComObject(oDocuments);
                oDocuments = null;
            }
        }

        int AddBatch(PKL1_Ex pickLine, OBTQ_Ex batch)
        {
            try
            {
                using (var conn = new SqlConnection(_dbConnectionStr))
                {
                    int result = conn.Execute("sp_InsertPickListAllocateItem",
                        new
                        {
                            SODocEntry = pickLine.OrderEntry,
                            SOLineNum = pickLine.OrderLine,
                            PickListDocEntry = pickLine.AbsEntry,
                            PickListLineNum = pickLine.PickEntry,
                            ItemCode = pickLine.ItemCode,
                            ItemDesc = pickLine.Dscription,
                            Batch = batch.DistNumber,
                            WhsCode = batch.WhsCode,
                            Quantity = batch.TransferBatchQty,
                        }, commandType: CommandType.StoredProcedure);

                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        int RemoveBatch(PKL1_Ex pickLine, OBTQ_Ex batch)
        {
            try
            {
                using (var conn = new SqlConnection(_dbConnectionStr))
                {
                    int result = conn.Execute("sp_InsertPickListAllocateItem",
                        new
                        {
                            SODocEntry = pickLine.OrderEntry,
                            SOLineNum = pickLine.OrderLine,
                            PickListDocEntry = pickLine.AbsEntry,
                            PickListLineNum = pickLine.PickEntry,
                            ItemCode = pickLine.ItemCode,
                            ItemDesc = pickLine.Dscription,
                            Batch = batch.DistNumber,
                            WhsCode = pickLine.WhsCode,
                            Quantity = -batch.DraftQty,
                        }, commandType: CommandType.StoredProcedure);

                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        //void InsertPickAlloactedLog(PKL1_Ex pKL1Line, OBTQ_Ex batch, string action, string status, string errmsg)
        //{
        //    try
        //    {
        //        var conn = new SqlConnection(_dbConnectionStr);

        //        var result = conn.Execute("sp_InsertPickListAllocateItemTransaction",
        //            new
        //            {
        //                SODocEntry = pKL1Line.OrderEntry,
        //                SOLineNum = pKL1Line.OrderLine,
        //                PickListDocEntry = pKL1Line.AbsEntry,
        //                PickListLineNum = pKL1Line.PickEntry,
        //                ItemCode = pKL1Line.ItemCode,
        //                ItemDesc = pKL1Line.Dscription,
        //                Batch = batch.DistNumber,
        //                Quantity = batch.TransferBatchQty,
        //                PickAction = action,
        //                status = status,
        //                Errormsg = errmsg
        //            },  
        //            commandType: System.Data.CommandType.StoredProcedure);
        //    }
        //    catch (Exception excep)
        //    {
        //        LastErrorMessage = $"{excep.Message} \n";
        //    }
        //}
     
    }
}

