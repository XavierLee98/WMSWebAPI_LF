using Dapper;
using DbClass;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        readonly IConfiguration _configuration;
        SqlConnection SQLconn;
        SqlTransaction SQLtrans;
        string databaseConnStr { get; set; } = "";
        string midwareConnStr { get; set; } = "";
        public string LastErrorMessage { get; private set; } = string.Empty;
        public void Dispose() => GC.Collect();

        public SQL_OPKL(string dbConnStr) => databaseConnStr = dbConnStr;

        public SQL_OPKL(IConfiguration configuration, string dbConnStr)
        {
            _configuration = configuration;
            midwareConnStr = dbConnStr;
        }

        public SQL_OPKL(string dbConnStr, string _MiddbConnStr)
        {
            databaseConnStr = dbConnStr;
            midwareConnStr = _MiddbConnStr;
        }

        /// <summary>
        /// Remove Variance for all item
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int DeleteBatchVarianceForAllItem(Cio bag)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {

                    foreach (var pkl1line in bag.pKL1List)
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


        /// <summary>
        /// Remove Variance batch for single itemline
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int DeleteBatchVarianceForSingleItem(Cio bag)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {

                    foreach (var line in bag.oBTQs)
                    {
                        result = conn.Execute(query, new { AbsEntry = bag.PickItemLine.AbsEntry, Batch = line.DistNumber, ItemCode = bag.PickItemLine.ItemCode });
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


        /// <summary>
        /// change Picker
        /// </summary>
        /// <param name="PickHead"></param>
        /// <param name="picker"></param>
        /// <returns></returns>
        public int UpdatePicker(string picker, OPKL_Ex PickHead)
        {
            try
            {
                var query = "Update OPKL SET U_PICKER = @Picker WHERE AbsEntry = @AbsEntry";
                int result = -1;
                using (var conn = new SqlConnection(midwareConnStr))
                {
                    result = conn.Execute(query, new { Picker= picker, AbsEntry = PickHead.AbsEntry});
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        /// <summary>
        /// Get Item Details
        /// </summary>
        /// <param name="bag"></param>
        public List<OITM_Ex> GetItemdetails(Cio bag)
        {
            try
            {
                var queryUpdate = "SELECT * FROM OITM " +
                                  "WHERE ItemCode = @ItemCode; ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    List<OITM_Ex> Items = new List<OITM_Ex>();
                    foreach (var line in bag.pKL1List)
                    {
                        Items.AddRange(conn.Query<OITM_Ex>(queryUpdate, new { ItemCode = line.ItemCode }).ToList());
                    }
                    return Items;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        //Insert Batch Variance for record
        /// </summary>
        /// <param name="batchVariance"></param>
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

                using (var conn = new SqlConnection(databaseConnStr))
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

        /// <summary>
        /// Remove Batch Variance
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int DeleteBatchVariance(Cio bag)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(query, new { AbsEntry = bag.PickItemLine.AbsEntry, Batch = bag.oIBT.DistNumber, ItemCode = bag.PickItemLine.ItemCode });
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        /// <summary>
        //Insert Allocate Batch Into Midware DB(Holding)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int InsertHoldingBatch(PKL1_Ex pKL1Line, OBTQ_Ex oBTQ)
        {
            int result = -1;
            try
            {
                var insertquery =
                    @"INSERT INTO [dbo].[zmwSOHoldPickItem]
                      ([SODocEntry] 
                      ,[SOLineNum] 
                      ,[PickListDocEntry] 
                      ,[PickListLineNum]
                      ,[ItemCode] 
                      ,[ItemDesc] 
                      ,[Batch] 
                      ,[Quantity] 
                      ,[AllocatedDate] 
                      ,[PickStatus]) 
                       VALUES 
                     ( @SODocEntry,
                       @SOLineNum,
                       @PickListDocEntry,
                       @PickListLineNum, 
                       @ItemCode,
                       @ItemDesc,
                       @Batch, 
                       @Quantity,
                       @AllocatedDate,
                       @PickStatus ); ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(insertquery,
                              new
                              {
                                  SODocEntry = pKL1Line.OrderEntry,
                                  SOLineNum = pKL1Line.OrderLine,
                                  PickListDocEntry = pKL1Line.AbsEntry,
                                  PickListLineNum = pKL1Line.PickEntry,
                                  ItemCode = pKL1Line.ItemCode,
                                  ItemDesc = pKL1Line.Dscription,
                                  Batch = oBTQ.DistNumber,
                                  Quantity = oBTQ.TransferBatchQty,
                                  AllocatedDate = DateTime.Now,
                                  PickStatus = "OnHold"
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

        /// <summary>
        ///Reset Picker
        /// </summary>
        /// <param name="PickHead"></param>
        /// <returns></returns>
        public int ResetPicker(OPKL_Ex PickHead)
        {
            try
            {
                var query = "Update OPKL SET U_PICKER = '-' WHERE AbsEntry = @AbsEntry";
                int result = -1;
                using (var conn = new SqlConnection(midwareConnStr))
                {
                    result = conn.Execute(query, new { AbsEntry = PickHead.AbsEntry });
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        /// <summary>
        /// Remove all Onhold Batch for all item
        /// </summary>
        /// <param name="pKL1s"></param>
        /// <returns></returns>
        public int RemoveAllBatchesforPickList(PKL1_Ex[] pKL1s)
        {
            try
            {
                var query = "Delete [dbo].[zmwSOHoldPickItem] WHERE PickListDocEntry = @PickDoc and Batch = @Batch and ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    foreach (var pkl1line in pKL1s)
                    {
                        foreach (var i in pkl1line.oBTQList)
                        {
                            result = conn.Execute(query, new { PickDoc = pkl1line.AbsEntry, Batch = i.DistNumber, ItemCode = pkl1line.ItemCode });
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


        /// <summary>
        /// Remove multi Onhold Batch for single item line
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int RemoveMultiBatch(Cio bag)
        {
            try
            {
                var query = "Delete [dbo].[zmwSOHoldPickItem] WHERE PickListDocEntry = @PickDoc and Batch = @Batch and ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    foreach(var line in bag.oBTQs)
                    {
                        result = conn.Execute(query, new { PickDoc = bag.PickItemLine.AbsEntry, Batch = line.DistNumber, ItemCode = bag.PickItemLine.ItemCode });
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

        /// <summary>
        /// Remove Single Onhold Batch
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int RemoveHoldingSingleBatch(Cio bag)
        {
            try 
            {
                var query = "Delete [dbo].[zmwSOHoldPickItem] WHERE PickListDocEntry = @PickDoc and Batch = @Batch and ItemCode = @ItemCode";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    int result = conn.Execute(query, new { PickDoc = bag.PickItemLine.AbsEntry, Batch = bag.oIBT.DistNumber, ItemCode = bag.PickItemLine.ItemCode });
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        /// <summary>
        /// Remove Onhold Batch
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int RemoveHoldingBatchesForSingleItem(Cio bag)
        {
            try
            {
                var query = "Delete [dbo].[zmwSOHoldPickItem] WHERE PickDoc = @PickDoc and ItemCode = @ItemCode";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    int result = conn.Execute(query, new { PickDoc = bag.PickItemLine.PickEntry, ItemCode = bag.PickItemLine.ItemCode });
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }



        /// <summary>
        /// Get Pick Details With Batch (SO)
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public DTO_OPKL GetPickDetailsFromSOWithOnholdBatch(int PickDoc)
        {
            try
            {
                DTO_OPKL dtoopkl = new DTO_OPKL();

                var Midwareconn = new SqlConnection(midwareConnStr);
                var queryone = "SELECT T1.*, T2.ItemCode, T2.Dscription, T4.U_Weight, T2.Quantity as ReleaseQuantity FROM OPKL T0 WITH (NOLOCK) " +
                            "INNER JOIN PKL1 T1 WITH (NOLOCK) ON T0.AbsEntry = T1.AbsEntry " +
                            "INNER JOIN RDR1 T2 WITH (NOLOCK) ON T2.DocEntry = T1.OrderEntry AND T1.OrderLine = T2.LineNum AND BaseObject = 17 " +
                            "LEFT JOIN ORDR T3 WITH (NOLOCK) ON T2.DocEntry = T3.DocEntry and T3.ObjType = T1.BaseObject " +
                            "INNER JOIN OITM T4 WITH (NOLOCK) ON T2.ItemCode =T4.ItemCode " +
                            "WHERE T0.AbsEntry=@PickDoc;";

                var querytwo = "SELECT T0.DocEntry,T1.AbsEntry,T1.PickEntry,T0.* FROM RDR1 T0 WITH (NOLOCK) " +
                    "INNER JOIN PKL1 T1 WITH (NOLOCK) ON T0.DocEntry = T1.OrderEntry " +
                    "INNER JOIN OPKL T2 WITH (NOLOCK) ON T1.AbsEntry = T2.AbsEntry " +
                    "WHERE T2.AbsEntry=@PickDoc;";

                var querythree = "SELECT T0.* FROM ORDR T0 WITH (NOLOCK) " +
                    "INNER JOIN RDR1 T1 WITH (NOLOCK) ON T0.DocEntry = T1.DocEntry " +
                    "RIGHT JOIN PKL1 T2 WITH (NOLOCK) ON T1.DocEntry = T2.OrderEntry AND T1.LineNum = T2.OrderLine AND T1.ObjType = T2.BaseObject " +
                    "WHERE T2.AbsEntrY=@PickDoc; ";

                var queryfour = @"SELECT * FROM zmwSOHoldPickItem WHERE PickListDocEntry = @PickDoc;";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    using (var multi = conn.QueryMultiple(queryone + " " + querytwo + " " + querythree, new { PickDoc }))
                    {
                        //List<BatchAllocateDocView> batchItem = new List<BatchAllocateDocView>();
                        var PickLines = multi.Read<PKL1_Ex>().ToArray();

                        var OrderLines = multi.Read<RDR1_Ex>().ToList();
                        var Orders = multi.Read<ORDR_Ex>().ToList();
                        var OrderDoc = Orders.Select(x => x.DocEntry).Distinct();
                        foreach (var line in PickLines)
                        {
                            line.rDR1 = OrderLines.Where(x => x.DocEntry == line.OrderEntry && x.LineNum == line.OrderLine).FirstOrDefault();
                            line.oRDR = Orders.Where(x => x.DocEntry == line.rDR1.DocEntry).FirstOrDefault();
                        }
                        var holdItemList = Midwareconn.Query<HoldPickItem>(queryfour, new { PickDoc }).ToList();

                        foreach (var line in holdItemList) { 
                                PickLines.Where(x => x.AbsEntry == line.PickListDocEntry && x.PickEntry == line.PickListLineNum).ToList().ForEach(y =>
                                {
                                    y.OnHoldBatches.Add(line);
                                });
                        }
                        dtoopkl.pKL1_Exs = PickLines;
                        return dtoopkl;
                    }
                }
                
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get Pick Details With Batch (SO)
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public DTO_OPKL GetPickDetailsFromSOWithBatch(int PickDoc)
        {
            try
            {
                DTO_OPKL dtoopkl = new DTO_OPKL();
                var queryone = "SELECT T1.*, T2.ItemCode, T2.Dscription, T4.U_Weight FROM OPKL T0 WITH (NOLOCK) " +
                            "INNER JOIN PKL1 T1 WITH (NOLOCK) ON T0.AbsEntry = T1.AbsEntry " +
                            "INNER JOIN RDR1 T2 WITH (NOLOCK) ON T2.DocEntry = T1.OrderEntry AND T1.OrderLine = T2.LineNum AND BaseObject = 17 " +
                            "LEFT JOIN ORDR T3 WITH (NOLOCK) ON T2.DocEntry = T3.DocEntry and T3.ObjType = T1.BaseObject " +
                            "INNER JOIN OITM T4 WITH (NOLOCK) ON T2.ItemCode =T4.ItemCode " +
                            "WHERE T0.AbsEntry=@PickDoc; ";

                var querytwo = "SELECT T0.DocEntry,T1.AbsEntry,T1.PickEntry,T0.* FROM RDR1 T0 WITH (NOLOCK) " +
                    "INNER JOIN PKL1 T1 WITH (NOLOCK) ON T0.DocEntry = T1.OrderEntry " +
                    "INNER JOIN OPKL T2 WITH (NOLOCK) ON T1.AbsEntry = T2.AbsEntry " +
                    "WHERE T2.AbsEntry = @PickDoc;";

                var querythree = "SELECT T0.* FROM ORDR T0 WITH (NOLOCK) " +
                    "INNER JOIN RDR1 T1 WITH (NOLOCK) ON T0.DocEntry = T1.DocEntry " +
                    "RIGHT JOIN PKL1 T2 WITH (NOLOCK) ON T1.DocEntry = T2.OrderEntry AND T1.LineNum = T2.OrderLine AND T1.ObjType = T2.BaseObject " +
                    "WHERE T2.AbsEntrY=@PickDoc; ";

                var queryfour = "SELECT MIN(T0.LogEntry) AS SnBAllocateViewLogEntry, T0.AllocateTp AS SnBAllocateViewDocType, T0.AllocatEnt AS SnBAllocateViewDocEntry, T0.AllocateLn AS SnBAllocateViewDocLine, T0.ManagedBy AS SnBAllocateViewMngBy, " +
                            "T1.MdAbsEntry AS SnBAllocateViewSnbMdAbs, T1.ItemCode AS SnBAllocateViewItemCode, T1.SysNumber AS SnBAllocateViewSnbSysNum, T0.LocCode AS SnBAllocateViewLocCode, SUM(T1.AllocQty) AS SnBAllocateViewAllocQty, T2.DistNumber  " +
                            "FROM dbo.OITL AS T0 WITH (NOLOCK) " +
                            "INNER JOIN dbo.ITL1 WITH (NOLOCK) AS T1 ON T1.LogEntry = T0.LogEntry " +
                            "INNER JOIN dbo.OBTN WITH (NOLOCK) AS T2 ON T1.MdAbsEntry = T2.AbsEntry " +
                            "WHERE  (T1.AllocQty <> 0) and  T0.AllocateTp = 17 AND T0.AllocatEnt = @AllocatEnt " +
                            "GROUP BY T0.AllocateTp, T0.AllocatEnt, T0.AllocateLn, T0.ManagedBy, T1.MdAbsEntry, T1.ItemCode, T1.SysNumber, T0.LocCode, T0.DocEntry, T0.DocLine, T0.DocNum, T2.DistNumber " +
                            "HAVING(SUM(T1.AllocQty) > 0); ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    using (var multi = conn.QueryMultiple(queryone + " " + querytwo + " " + querythree, new { PickDoc }, commandTimeout: 0))
                    {
                        List<BatchAllocateDocView> batchItem = new List<BatchAllocateDocView>();
                        var PickLines = multi.Read<PKL1_Ex>().ToArray();
                        var OrderLines = multi.Read<RDR1_Ex>().ToList();
                        var Orders = multi.Read<ORDR_Ex>().ToList();
                        var OrderDoc = Orders.Select(x => x.DocEntry).Distinct();
                        foreach (var order in OrderDoc)
                        {
                            batchItem.AddRange(conn.Query<BatchAllocateDocView>(queryfour, new { AllocatEnt = order }).ToList());
                        }
                        foreach (var line in PickLines)
                        {
                            line.rDR1 = OrderLines.Where(x => x.DocEntry == line.OrderEntry && x.LineNum == line.OrderLine).FirstOrDefault();
                            line.oRDR = Orders.Where(x => x.DocEntry == line.rDR1.DocEntry).FirstOrDefault();
                            line.AllocatedBatches.AddRange(batchItem.Where(x => x.SnBAllocateViewDocLine == line.rDR1.LineNum && x.SnBAllocateViewDocEntry == line.rDR1.DocEntry).ToList());
                        }
                        dtoopkl.pKL1_Exs = PickLines;
                        return dtoopkl;
                    }
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get all Pick Lists (SO)
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public OPKL_Ex[] GetOPKLLists(Cio bag)
        {
            try
            {
                var query = @"SELECT T0.*, T2.WhsCode FROM OPKL T0 WITH (NOLOCK) 
                             INNER JOIN PKL1 T1 WITH (NOLOCK) ON T0.AbsEntry = T1.AbsEntry 
                             INNER JOIN RDR1 T2 WITH (NOLOCK) ON T1.OrderEntry = T2.DocEntry AND T1.OrderLine = T2.LineNum 
                             WHERE T0.PickDate >= @QueryStartDate 
                             AND T0.PickDate <= @QueryEndDate 
                             AND T1.BaseObject = 17; ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OPKL_Ex>(query, new { bag.QueryStartDate, bag.QueryEndDate },commandTimeout:0).ToArray();
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public CNWarehouses GetCNWarehouse()
        {
            try
            {
                string query = "SELECT * FROM CNWarehouse; ";

                var conn = new SqlConnection(databaseConnStr);
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

        /// <summary>
        /// Get Available batch Quantity(OIBT)
        /// </summary>
        /// <param name="Itemcode"></param>
        /// <returns></returns>
        public OIBT[] getOIBTs(string Itemcode, string warehouse)
        {
            try
            {
                var CNWarehouse = GetCNWarehouse();
                if (CNWarehouse == null) return null;
                //var query = "SELECT * FROM OIBT " +
                //    "WHERE [ItemCode] = @Itemcode " +
                //    "AND [WhsCode] = 'LWFGW' " +
                //    "AND (Isnull(Quantity,0) - Isnull(IsCommited,0)) > 0 AND Status = 0 AND Direction = 0;";

                var query = "SELECT T0.ItemCode, T0.DistNumber, T0.ItemName, T1.SysNumber, T1.WhsCode, T0.Status, T0.ExpDate, T0.MnfDate,  T1.Quantity, T1.CommitQty, T1.CountQty, T1.AbsEntry, T1.MdAbsEntry, T1.TrackingNt, T1.CCDQuant FROM OBTN T0 WITH (NOLOCK)" +
                    "INNER JOIN  OBTQ T1 WITH (NOLOCK) on T1.SysNumber = T0.SysNumber and T1.ItemCode = T0.ItemCode " +
                    "WHERE T1.WhsCode = @Warehouse " +
                    "AND T0.ItemCode = @Itemcode " +
                    "AND (Isnull(T1.Quantity,0) - Isnull(T1.CommitQty,0)) > 0 " +
                    "AND T0.Status = 0; ";

                using (var conn = new SqlConnection(midwareConnStr))
                {
                    return conn.Query<OIBT>(query, new { Itemcode = Itemcode,  Warehouse = warehouse}, commandTimeout: 0).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Update configure Item Validation For PickList
        /// </summary>
        /// <param name="Setupvalue"></param>
        /// <returns></returns>
        public int UpdateValidateItemConfiguration(string Setupvalue)
        {
            try
            {
                var query = "UPDATE dbo.zmwSetupConfig SET SetupValue = @Setupvalue ";

                using (var conn = new SqlConnection(databaseConnStr))
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

        /// <summary>
        /// Get Validate Item Configuration
        /// </summary>
        /// <returns></returns>
        public string GetValidateItemConfiguration()
        {
            try
            {
                var query = "SELECT [SetupValue] FROM [dbo].[zmwSetupConfig] WHERE Id = 1; ";

                using (var conn = new SqlConnection(databaseConnStr))
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

        /// <summary>
        /// Get all Truck
        /// </summary>
        /// <returns></returns>
        public PickerModel[] GetPickers()
        {
            try
            {
                var query = "SELECT * FROM [dbo].[@PICKER]";

                using (var conn = new SqlConnection(databaseConnStr))
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

        /// <summary>
        /// Get all Driver
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public Driver[] GetDrivers(Cio bag)
        {
            try
            {
                var query = "SELECT * FROM [dbo].[@DRIVER]";

                using (var conn = new SqlConnection(databaseConnStr))
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

        /// <summary>
        /// Get all Truck
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public Truck[] GetTrucks(Cio bag)
        {
            try
            {
                var query = "SELECT * FROM [dbo].[@TRUCK]";

                using (var conn = new SqlConnection(databaseConnStr))
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
                using (var conn = new SqlConnection(databaseConnStr))
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

        public int CreateUpdatePickList_Midware(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine)
        {
            try
            {

                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;

                SQLconn = new SqlConnection(midwareConnStr);
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








