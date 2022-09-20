using Dapper;
using Microsoft.Extensions.Configuration;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Models.ReturnToCN;
using WMSWebAPI.Models.SAP_DiApi;

namespace WMSWebAPI.SAP_DiApi
{
    public class DiApiCN:IDisposable
    {
        public void Dispose() => GC.Collect();
        //Company oCom { get; set; }
        public string LastErrorMessage { get; private set; }

        IConfiguration _configuration;
        SAPCompany _company;
        FileLogger _fileLogger = new FileLogger();
        string _sapdbConnectionStr;
        string _midwareConnectionStr;
        string _company_Prefix;

        public DiApiCN(IConfiguration configuration, string dbConnectionStr, string midwareConnectionStr, SAPCompany company)
        {
            _configuration = configuration;
            _midwareConnectionStr = midwareConnectionStr;
            _sapdbConnectionStr = dbConnectionStr;
            _company = company;
            _company_Prefix = _configuration.GetSection("CompanyPrefix").Value;
        }

        public CNWarehouses GetCNWarehouse()
        {
            try
            {
                string query = "SELECT * FROM CNWarehouse; ";

                var conn = new SqlConnection(_midwareConnectionStr);
                var result = conn.Query<CNWarehouses>(query).FirstOrDefault();

                return result;
            }
            catch (Exception excep)
            {
                LastErrorMessage = "Fail to Get CN Warehouse Setup.";
                Log(excep.ToString());
                return null;
            }
        }

        /// <summary>
        /// Create Actual PickList From RI(INIV1)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public string RICreateCN(ReturnHeader returnHeader, List<ReturnDetails> returnLines)
        {
            try
            {
                if (!_company.connectSAP())
                {
                    throw new Exception(_company.errMsg);
                }

                var CNWarehouse = GetCNWarehouse();
                if (CNWarehouse == null) return null;

                if (!_company.oCom.InTransaction)
                    _company.oCom.StartTransaction();

                SAPbobsCOM.Documents oCreditNote = null;
                SAPbobsCOM.Document_Lines oCreditNote_Lines = null;

                oCreditNote = (SAPbobsCOM.Documents)_company.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);
                oCreditNote.CardCode = returnHeader.CustCode;
                oCreditNote.CardName = returnHeader.CustName;
                oCreditNote.NumAtCard = returnHeader.DocEntry;
                oCreditNote.DocDate = returnHeader.DocDate;
                oCreditNote.Comments = returnHeader.Remark;
                oCreditNote_Lines = oCreditNote.Lines;

                for (int i = 0; i < returnLines.Count; i++)
                {
                    //Good Condition
                    if (returnLines[i].GoodQty > 0)
                    {
                        if (i > 0)
                        {
                            oCreditNote_Lines.Add();
                        }
                        oCreditNote_Lines.ItemCode = returnLines[i].ItemCode;
                        oCreditNote_Lines.ItemDescription = returnLines[i].ItemDesc;
                        oCreditNote_Lines.Quantity = (double)returnLines[i].GoodQty;
                        oCreditNote_Lines.UnitPrice = (double)returnLines[i].UnitPrice;
                        oCreditNote_Lines.WarehouseCode = CNWarehouse.GoodWarehouse;
                        oCreditNote_Lines.BatchNumbers.Add();
                        oCreditNote_Lines.BatchNumbers.BatchNumber = $"{returnHeader.DocEntry}_{_company_Prefix}";
                        oCreditNote_Lines.BatchNumbers.Quantity = (double)returnLines[i].GoodQty;
                        oCreditNote_Lines.BatchNumbers.ManufacturingDate = returnLines[i].ManufactureDate;
                    }

                    //Bad Condition
                    if ((returnLines[i].Quantity - returnLines[i].GoodQty) > 0)
                    {
                        if (i > 0)
                        {
                            oCreditNote_Lines.Add();
                        }
                        if (i == 0)
                        {
                           if(returnLines[i].GoodQty > 0)
                                oCreditNote_Lines.Add();
                        }

                        oCreditNote_Lines.ItemCode = returnLines[i].ItemCode;
                        oCreditNote_Lines.ItemDescription = returnLines[i].ItemDesc;
                        oCreditNote_Lines.Quantity = (double)(returnLines[i].Quantity - returnLines[i].GoodQty);
                        oCreditNote_Lines.UnitPrice = (double)returnLines[i].UnitPrice;
                        oCreditNote_Lines.WarehouseCode = CNWarehouse.BadWarehouse;
                        oCreditNote_Lines.BatchNumbers.Add();
                        oCreditNote_Lines.BatchNumbers.BatchNumber = $"{returnHeader.DocEntry}_{_company_Prefix}";
                        oCreditNote_Lines.BatchNumbers.Quantity = (double)(returnLines[i].Quantity - returnLines[i].GoodQty);
                        oCreditNote_Lines.BatchNumbers.ManufacturingDate = returnLines[i].ManufactureDate;
                    }
                }

                int RetVal = oCreditNote.Add();
                if (RetVal != 0)
                {
                    if (_company.oCom.InTransaction)
                        _company.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                    LastErrorMessage += $"{_company.oCom.GetLastErrorCode()} - {_company.oCom.GetLastErrorDescription()}";
                    return null;
                }
                else
                {
                    if (_company.oCom.InTransaction)
                        _company.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                    var CNDocNo = _company.oCom.GetNewObjectKey();
                    return CNDocNo;

                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
                LastErrorMessage = $"{e.Message}\n";
                return null;
            }

        }

        void Log(string message)
        {
            _fileLogger.WriteLog(message);
        }
    }
}
