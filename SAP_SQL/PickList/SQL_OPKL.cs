using Dapper;
using DbClass;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Do;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.Request;
using WMSWebAPI.Models.ReturnRequest;
using WMSWebAPI.Models.SAP_DiApi;

namespace WMSWebAPI.SAP_SQL.PickList
{
    public class SQL_OPKL : IDisposable
    {
        SqlConnection SQLconn;
        SqlTransaction SQLtrans;
        string midConnStr { get; set; } = "";
        string sapConnStr { get; set; } = "";
        public string LastErrorMessage { get; private set; } = string.Empty;
        public void Dispose() => GC.Collect();

        public SQL_OPKL(string dbConnStr) => midConnStr = dbConnStr;

        public SQL_OPKL(string dbConnStr, string _SapConnStr)
        {
            midConnStr = dbConnStr;
            sapConnStr = _SapConnStr;
        }

        public OPKL_Ex[] GetOPKLLists(DateTime startDate, DateTime endDate, string QueryWhs)
        {
            try
            {
                using (var conn = new SqlConnection(midConnStr))
                {
                    return conn.Query<OPKL_Ex>($"sp_PickList_GetList",
                                                new { startdate = startDate, enddate = endDate, whscode = QueryWhs },
                                                commandType:CommandType.StoredProcedure,
                                                commandTimeout: 0).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public List<PKL1_Ex> GetPickDetails(int absentry)
        {
            try
            {
                var conn = new SqlConnection(midConnStr);

                var result = conn.Query<PKL1_Ex>($"sp_PickList_GetPickDetails",
                            new { absentry = absentry },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: 0).ToList();

                if (result == null) throw new Exception("Fail to Get Pick Details");

                foreach(var line in result)
                {
                    line.oBTQList = new List<OBTQ_Ex>();
                    line.oBTQList = GetBatchItem(line);
                }

                return result;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message}";
                return null;
            }
        }

        public int AddBatch(PKL1_Ex pickLine, OBTQ_Ex batch)
        {
            try
            {
                using (var conn = new SqlConnection(midConnStr))
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

        public int RemoveBatch(PKL1_Ex pickLine, OBTQ_Ex batch)
        {
            try
            {
                using (var conn = new SqlConnection(midConnStr))
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

        public List<OBTQ_Ex> GetBatchItem_Onhold(PKL1_Ex PickItemLine)
        {
            var SAPConn = new SqlConnection(sapConnStr);

            List<OBTQ_Ex> oBTQs = new List<OBTQ_Ex>();

            if (PickItemLine == null) throw new Exception("Pick Detail Line not found [Batch]. Please try again");


            var result = SAPConn.Query<HoldPickItem>("zwa_IMApp_PickList_spGetOnholdBatch",
                new { PickListDocEntry = PickItemLine.AbsEntry, PickListLineNum = PickItemLine.PickEntry },
                commandType: CommandType.StoredProcedure, commandTimeout: 0).ToList();

            if (result == null) return null;

            foreach (var line in result)
            {
                oBTQs.Add(new OBTQ_Ex
                {
                    ItemCode = line.ItemCode,
                    DistNumber = line.Batch,
                    TransferBatchQty = line.Quantity
                });
            }
            return oBTQs;
        }

        public List<OBTQ_Ex> GetBatchItem(PKL1_Ex PickItemLine)
        {

            var SAPConn = new SqlConnection(midConnStr);

            if (PickItemLine == null) throw new Exception("Pick Detail Line not found. Please try again");

            string json = JsonConvert.SerializeObject(new { absentry = PickItemLine.AbsEntry, pickentry = PickItemLine.PickEntry });

            var result = SAPConn.Query<OBTQ_Ex>($"sp_PickList_GetPickLineBatches",
                            new { absentry = PickItemLine.AbsEntry, linenum = PickItemLine.PickEntry },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: 0).ToList();

            return result;
        }

        public List<OBTQ_Ex> GetAvailableBatches(string itemcode, string warehouse, int absentry, int pickentry)
        {
            try
            {
                using (var conn = new SqlConnection(midConnStr))
                {
                    string json = JsonConvert.SerializeObject(new { itemcode = itemcode, whscode = warehouse, absentry = absentry, pickentry = pickentry });

                    return conn.Query<OBTQ_Ex>($"sp_PickList_GetAvailableBatches",
                            new { itemcode = itemcode, whscode = warehouse, absentry = absentry, picklinenum = pickentry },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: 0).ToList();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public int UpdatePickHeader(OPKL_Ex pickhead)
        {
            try
            {
                var conn = new SqlConnection(sapConnStr);

                var result = conn.Execute("zwa_IMApp_PickList_spUpdatePickHeader",
                    new { 
                        AbsEntry = pickhead.AbsEntry,
                        Picker = pickhead.U_Picker, 
                        Driver = pickhead.U_Driver, 
                        TruckNo = pickhead.U_TruckNo, 
                        DeliveryType = pickhead.U_DeliveryType, 
                        Remark = pickhead.Remarks
                    },
                    commandType:CommandType.StoredProcedure
                    );

                return 0;

            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                Console.WriteLine(excep);
                return -1;
            }
        }

        public CNWarehouses GetCNWarehouse()
        {
            try
            {
                string query = "SELECT * FROM CNWarehouse; ";

                var conn = new SqlConnection(midConnStr);
                var result = conn.Query<CNWarehouses>(query).FirstOrDefault();

                return result;
            }
            catch (Exception excep)
            {
                LastErrorMessage = "Fail to Get CN Warehouse Setup.";
                Console.WriteLine(excep);
                return null;
            }
        }

        public int DeleteBatchVarianceForAllItem(PKL1_Ex[] pkl1List)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(midConnStr))
                {

                    foreach (var pkl1line in pkl1List)
                    {
                        foreach (var i in pkl1line.oBTQList)
                        {
                            result = conn.Execute(query, new { AbsEntry = pkl1line.AbsEntry, Batch = i.DistNumber, ItemCode = pkl1line.ItemCode });
                        }
                    }
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int DeleteBatchVarianceForSingleItem(PKL1_Ex pkl1Line,List<OBTQ_Ex> oBTQs)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(midConnStr))
                {

                    foreach (var line in oBTQs)
                    {
                        result = conn.Execute(query, new { AbsEntry = pkl1Line.AbsEntry, Batch = line.DistNumber, ItemCode = pkl1Line.ItemCode });
                    }
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int InsertBatchVariance(BatchVariance batchVariance)
        {
            int result = -1;
            try
            {
                var insertquery =
                    @"INSERT INTO [dbo].[zmwBatchVariance]
                      ([Batch] 
                      ,[PickListType] 
                      ,[PickListNo] 
                      ,[ItemCode] 
                      ,[SystemQty]
                      ,[ActualQty] 
                      ,[Variance] 
                      ,[CreatedDate]) 
                       VALUES 
                     ( @Batch,
                       @PickListType,
                       @PickListNo,
                       @ItemCode, 
                       @SystemQty,
                       @ActualQty,
                       @Variance,
                       @CreatedDate ); ";

                using (var conn = new SqlConnection(midConnStr))
                {
                    result = conn.Execute(insertquery,
                             new
                             {
                                 Batch = batchVariance.Batch,
                                 PickListType = batchVariance.PickListType,
                                 PickListNo = batchVariance.PickListNo,
                                 ItemCode = batchVariance.ItemCode,
                                 SystemQty = batchVariance.SystemQty,
                                 ActualQty = batchVariance.SystemQty + batchVariance.Variance,
                                 Variance = batchVariance.Variance,
                                 CreatedDate = DateTime.Now,
                             });
                }
                return result;

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return result;
            }
        }

        public int DeleteBatchVariance(PKL1_Ex pickLine, OBTQ_Ex batch)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(midConnStr))
                {
                    result = conn.Execute(query, new { AbsEntry = pickLine.AbsEntry, Batch = batch.DistNumber, ItemCode = pickLine.ItemCode });
                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int ResetPicker(OPKL_Ex PickHead)
        {
            try
            {
                var query = "Update OPKL SET U_PICKER = '' WHERE AbsEntry = @AbsEntry";
                int result = -1;
                using (var conn = new SqlConnection(sapConnStr))
                {
                    result = conn.Execute(query, new { AbsEntry = PickHead.AbsEntry }, commandTimeout: 0);
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int UpdatePicker(string picker, OPKL_Ex PickHead)
        {
            try
            {
                var query = "Update OPKL SET U_PICKER = @Picker WHERE AbsEntry = @AbsEntry";
                int result = -1;
                using (var conn = new SqlConnection(sapConnStr))
                {
                    result = conn.Execute(query, new { Picker = picker, AbsEntry = PickHead.AbsEntry },commandTimeout:0);
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int UpdateValidateItemConfiguration(string Setupvalue)
        {
            try
            {
                var query = "UPDATE dbo.zmwSetupConfig SET SetupValue = @Setupvalue ";

                using (var conn = new SqlConnection(midConnStr))
                {
                   int result = conn.Execute(query, new {Setupvalue = Setupvalue });
                   return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public string GetValidateItemConfiguration()
        {
            try
            {
                var query = "SELECT [SetupValue] FROM [dbo].[zmwSetupConfig] WHERE Id = 1; ";

                using (var conn = new SqlConnection(midConnStr))
                {
                    return conn.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public DTO_OPKL GetPickHeadProperties(Cio bag)
        {
            try
            {
                DTO_OPKL dTO_OPKL = new DTO_OPKL();
                var querypicker = "SELECT * FROM [dbo].[@PICKER] WHERE Code = @PickerCode;";
                var querydriver = "SELECT * FROM [dbo].[@DRIVER] WHERE Code = @DriverCode;";
                var querytrucknum = "SELECT * FROM [dbo].[@TRUCK] WHERE Code = @TruckCode;";
                using (var conn = new SqlConnection(sapConnStr))
                {
                    using (var multi = conn.QueryMultiple(querypicker + " " + querydriver + " " + querytrucknum, new { PickerCode = bag.PickHead.U_Picker, DriverCode = bag.PickHead.U_Driver, TruckCode = bag.PickHead.U_TruckNo }))
                    {
                        dTO_OPKL.picker = multi.Read<PickerModel>().FirstOrDefault();
                        dTO_OPKL.driver = multi.Read<Driver>().FirstOrDefault();
                        dTO_OPKL.truck = multi.Read<Truck>().FirstOrDefault();
                    }
                }
                return dTO_OPKL;
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public PickerModel[] GetPickers()
        {
            try
            {
                var query = "SELECT * FROM [dbo].[@PICKER]";

                using (var conn = new SqlConnection(midConnStr))
                {
                    return conn.Query<PickerModel>(query).ToArray();
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public Driver[] GetDrivers(Cio bag)
        {
            try
            {
                var query = "SELECT * FROM [dbo].[@DRIVER]";

                using (var conn = new SqlConnection(midConnStr))
                {
                    return conn.Query<Driver>(query).ToArray();
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public Truck[] GetTrucks(Cio bag)
        {
            try
            {
                var query = "SELECT * FROM [dbo].[@TRUCK]";

                using (var conn = new SqlConnection(midConnStr))
                {
                    return conn.Query<Truck>(query).ToArray();
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public OWHS[] GetPickWarehouse()
        {
            try
            {
                var query = "EXEC GetPickWarehouse";
                using (var conn = new SqlConnection(midConnStr))
                {
                    return conn.Query<OWHS>(query).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public int ResetPickList_Midware(zwaRequest dtoRequest)
        {
            try
            {
                if (dtoRequest == null) return -1;
                SQLconn = new SqlConnection(sapConnStr);

                #region insert request
                string insertSql = $"INSERT INTO zmwRequest  (" +
                                             $"request" +
                                             $",sapUser " +
                                             $",sapPassword" +
                                             $",requestTime" +
                                             $",phoneRegID" +
                                             $",status" +
                                             $",guid" +
                                             $",sapDocNumber" +
                                             $",completedTime" +
                                             $",attachFileCnt" +
                                             $",tried" +
                                             $",createSAPUserSysId " +
                                             $")VALUES(" +
                                             $"@request" +
                                             $",@sapUser" +
                                             $",@sapPassword" +
                                             $",GETDATE()" +
                                             $",@phoneRegID" +
                                             $",@status" +
                                             $",@guid" +
                                             $",@sapDocNumber" +
                                             $",GETDATE()" +
                                             $",@attachFileCnt" +
                                             $",@tried" +
                                             $",@createSAPUserSysId)";

                var result = SQLconn.Execute(insertSql, dtoRequest, SQLtrans);
                if (result < 0)
                { SQLtrans?.Rollback(); return -1; }
                #endregion

                return result;
            }

            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int CreateUpdatePickList_Midware(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine)
        {
            try
            {

                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;

                SQLconn = new SqlConnection(sapConnStr);
                SQLconn.Open();
                SQLtrans = SQLconn.BeginTransaction();

                using (SQLconn)
                using (SQLtrans)
                {
                    #region insert request
                    string insertSql = $"INSERT INTO zmwRequest  (" +
                                                 $"request" +
                                                 $",sapUser " +
                                                 $",sapPassword" +
                                                 $",requestTime" +
                                                 $",phoneRegID" +
                                                 $",status" +
                                                 $",guid" +
                                                 $",sapDocNumber" +
                                                 $",completedTime" +
                                                 $",attachFileCnt" +
                                                 $",tried" +
                                                 $",createSAPUserSysId " +
                                                 $")VALUES(" +
                                                 $"@request" +
                                                 $",@sapUser" +
                                                 $",@sapPassword" +
                                                 $",GETDATE()" +
                                                 $",@phoneRegID" +
                                                 $",@status" +
                                                 $",@guid" +
                                                 $",@sapDocNumber" +
                                                 $",GETDATE()" +
                                                 $",@attachFileCnt" +
                                                 $",@tried" +
                                                 $",@createSAPUserSysId)";

                    var result = SQLconn.Execute(insertSql, dtoRequest, SQLtrans);
                    if (result < 0)
                    { SQLtrans?.Rollback(); return -1; }
                    #endregion

                    #region Insert zmwInventoryCountGRPO
                    /// perform insert of all the GRPO item 
                    ///     
                    string insertGrpo = $"INSERT INTO zmwGRPO " +
                         $"(Guid" +
                         $",LineGuid" +
                         $",ItemCode" +
                         $",Qty" +
                         $",SourceDocNum" +
                         $",SourceDocEntry" +
                         $",SourceDocBaseType" +
                         $",SourceBaseEntry" +
                         $",SourceBaseLine" +
                         $",SourceLineNum" +
                         $",Warehouse" +
                         $",SourceDocType" +
                         $",LineWeight" +
                         $") VALUES (" +
                         $"@Guid" +
                         $",@LineGuid" +
                         $",@ItemCode" +
                         $",@Qty" +
                         $",@SourceDocNum" +
                         $",@SourceDocEntry" +
                         $",@SourceDocBaseType" +
                         $",@SourceBaseEntry" +
                         $",@SourceBaseLine " +
                         $",@SourceLineNum" +
                         $",@Warehouse" +
                         $",@SourceDocType" +
                         $",@LineWeight" +
                         $")";

                    result = SQLconn.Execute(insertGrpo, grpoLines, SQLtrans);
                    if (result < 0) { SQLtrans?.Rollback(); return -1; }
                    #endregion

                    #region insert zwaItemBin
                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
                        string insertItemBinLine =
                       $"INSERT INTO zmwItemBin ( " +
                       $"Guid" +
                       $",LineGuid" +
                       $",ItemCode" +
                       $",Quantity" +
                       $",BinCode" +
                       $",BinAbsEntry" +
                       $",BatchNumber" +
                       $",SerialNumber" +
                       $",TransDateTime " +
                       $")VALUES(" +
                       $"@Guid" +
                       $",@LineGuid" +
                       $",@ItemCode" +
                       $",@Quantity" +
                       $",@BinCode" +
                       $",@BinAbsEntry" +
                       $",@BatchNumber" +
                       $",@SerialNumber" +
                       $",GETDATE()" +
                       $")";

                        result = SQLconn.Execute(insertItemBinLine, itemBinLine, SQLtrans);
                        if (result < 0) { SQLtrans?.Rollback(); return -1; }
                    }
                    #endregion

                    SQLtrans?.Commit();
                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                SQLtrans?.Rollback();
                return -1;
            }
        }

    }
}






