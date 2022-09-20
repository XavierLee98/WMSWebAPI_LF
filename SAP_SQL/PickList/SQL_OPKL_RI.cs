using Dapper;
using DbClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.ReturnRequest;

namespace WMSWebAPI.SAP_SQL.PickList
{
    public class SQL_OPKL_RI : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string MiddatabaseConnStr { get; set; } = "";
        public string LastErrorMessage { get; private set; } = string.Empty;
        public void Dispose() => GC.Collect();
        public SQL_OPKL_RI(string dbConnStr)
        {
            databaseConnStr = dbConnStr;
        }

        public SQL_OPKL_RI(string dbConnStr,string _MiddbConStr)
        {
            databaseConnStr = dbConnStr;
            MiddatabaseConnStr = _MiddbConStr;
        }

        /// <summary>
        /// Cancel RI PickList
        /// </summary>
        /// <param name="PickID"></param>
        /// <returns></returns>
        public int CancelReleasedPickList(int PickID)
        {
            try
            {
                var queryUpdate = "UPDATE zmwPickHead SET DocStatus = 'Cancelled', UpdateDate = @UpdateDate " +
                                  "WHERE AbsEntry = @DocEntry; ";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(queryUpdate, new { DocEntry = PickID, UpdateDate = DateTime.Now});
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return -1;
            }
        }

        public int CancelPickListToRI(List<INV1_Ex> iNV1s)
        {
            try
            {
                var queryUpdate = "UPDATE INV1 SET U_PickListNo = NULL, U_PickedQty = 0 " +
                                  "WHERE DocEntry = @DocEntry; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var invsdoc = iNV1s.Select(x => x.DocEntry).Distinct();
                    foreach(var doc in invsdoc)
                    {
                        result = conn.Execute(queryUpdate, new { DocEntry = doc });
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
        /// Remove Variance for all items
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int DeleteBatchVarianceForAllItem(Cio bag)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {

                    foreach (var line in bag.INV1s)
                    {
                        foreach (var batch in line.oBTQList)
                        {
                            result = conn.Execute(query, new { AbsEntry = line.U_PickListNo, Batch = batch.DistNumber, ItemCode = line.ItemCode });
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
        /// Remove Variances for item line
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int DeleteBatchVarianceForSingleItem(Cio bag)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {

                    foreach (var batch in bag.oBTQs)
                    {
                        result = conn.Execute(query, new { AbsEntry = bag.RIItemLine.U_PickListNo, Batch = batch.DistNumber, ItemCode = bag.RIItemLine.ItemCode });
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
        /// Remove Single Variance
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int DeleteBatchVariance(Cio bag)
        {
            try
            {
                var query = "DELETE FROM zmwBatchVariance WHERE PickListNo = @AbsEntry AND Batch = @Batch AND ItemCode = @ItemCode";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(query, new { AbsEntry = bag.RIItemLine.U_PickListNo, Batch = bag.oIBT.DistNumber, ItemCode = bag.RIItemLine.ItemCode });
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
        /// Update Picker
        /// </summary>
        /// <param name="QueryPicker"></param>
        /// <returns></returns>
        public int UpdatePicker(string QueryPicker,int AbsEntry)
        {
            try
            {
                var query = "Update [zmwPickHead] SET Name = @Picker WHERE AbsEntry = @AbsEntry";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(query, new { Picker = QueryPicker, AbsEntry = AbsEntry });
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
        /// Reset Picker
        /// </summary>
        /// <param name="PickHead"></param>
        /// <returns></returns>
        public int ResetPicker(OPKL_Ex PickHead)
        {
            try
            {
                var query = "Update [zmwPickHead] SET Name = '-' WHERE AbsEntry = @AbsEntry";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
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
        /// Remove Onhold Batch For Whole PickList
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int RemoveHoldingMultiBatchForAllItem(Cio bag)
        {
            try
            {
                int result = -1;
                var query = "Delete [dbo].[zmwRIHoldPickItem] WHERE PickListDocEntry = @PickDoc and Batch = @Batch and ItemCode = @ItemCode";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    foreach(var line in bag.INV1s)
                    {
                        foreach (var batch in line.oBTQList)
                        {
                            result = conn.Execute(query, new { PickDoc = line.U_PickListNo, Batch = batch.DistNumber, ItemCode = line.ItemCode });
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
        /// Remove Onhold Batch For Single Item Line
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int RemoveHoldingMultiBatchForSingleItem(Cio bag)
        {
            try
            {
                int result = -1;
                var query = "Delete [dbo].[zmwRIHoldPickItem] WHERE PickListDocEntry = @PickDoc and Batch = @Batch and ItemCode = @ItemCode";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    foreach(var batch in bag.oBTQs)
                    {
                         result = conn.Execute(query, new { PickDoc = bag.RIItemLine.U_PickListNo, Batch = batch.DistNumber, ItemCode = bag.RIItemLine.ItemCode });
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
                var query = "Delete [dbo].[zmwRIHoldPickItem] WHERE PickListDocEntry = @PickDoc and Batch = @Batch and ItemCode = @ItemCode";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    int result = conn.Execute(query, new { PickDoc = bag.RIItemLine.U_PickListNo, Batch = bag.oIBT.DistNumber, ItemCode = bag.RIItemLine.ItemCode });
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
        public int InsertHoldingBatch(INV1_Ex iNV1, OBTQ_Ex oBTQ)
        {
            int result = -1;
            try
            {
                var insertquery =
                    @"INSERT INTO [dbo].[zmwRIHoldPickItem]
                      ([RIDocEntry] 
                      ,[RiLineNum] 
                      ,[PickListDocEntry] 
                      ,[ItemCode] 
                      ,[ItemDesc] 
                      ,[Batch] 
                      ,[Quantity] 
                      ,[AllocatedDate] 
                      ,[PickStatus]) 
                       VALUES 
                     ( @RIDocEntry,
                       @RILineNum,
                       @PickListDocEntry,
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
                                     RIDocEntry = iNV1.DocEntry,
                                     RILineNum = iNV1.LineNum,
                                     PickListDocEntry = iNV1.U_PickListNo,
                                     ItemCode = iNV1.ItemCode,
                                     ItemDesc = iNV1.Dscription,
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
        /// Get all Pick Lists header From MidwareDB (RI)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OPKL_Ex[] GetRIPickHead(Cio bag)
        {
            try
            {
                var query = "SELECT * FROM zmwPickHead  " +
                            "WHERE PickDate >= @QueryStartDate " +
                            "AND PickDate <= @QueryEndDate " +
                            "AND DocStatus != 'Cancelled'; ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OPKL_Ex>(query, new { bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get all OINV (RI)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public DTO_OPKL GetReservedInvoiceList(Cio bag)
        {
            try
            {
                DTO_OPKL dTO_OPKL = new DTO_OPKL();
                //Check Pick Status For INV1 OR OINV ONLY EQUAL TO N???
                var query1 = "SELECT * FROM OINV WHERE DocStatus = 'O' " +
                        "AND DocDate >= @QueryStartDate AND DocDate <= @QueryEndDate ";
                var query2 = "SELECT T0.* FROM INV1 T0 " +
                             "INNER JOIN OINV T1 ON T0.DocEntry = T1.DocEntry " +
                             "WHERE T1.DocStatus = 'O' AND T1.DocDate >= @QueryStartDate AND T1.DocDate <= @QueryEndDate ;  ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    dTO_OPKL.OINVs2 = conn.Query<OINV_Ex2>(query1, new { bag.QueryStartDate, bag.QueryEndDate }).ToList();
                    dTO_OPKL.iNV1s = conn.Query<INV1_Ex>(query2, new { bag.QueryStartDate, bag.QueryEndDate }).ToArray();

                    return dTO_OPKL;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Create new Pick List to Midware DB
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int CreatePickList(Cio bag)
        {
            try
            {
                var PickHead = bag.PickHeader;
                string queryInsert = $"INSERT INTO zmwPickHead "
                                     + "(Guid  "
                                     + ",Name "
                                     + ",OwnerCode "
                                     + ",OwnerName "
                                     + ",PickDate "
                                     + ",Remarks "
                                     + ",Canceled "
                                     + ",ShipType "
                                     + ",Status "
                                     + ",Printed "
                                     + ",LogInstac "
                                     + ",ObjType "
                                     + ",UpdateDate "
                                     + ",CreateDate "
                                     + ",UserSign "
                                     + ",UserSign2 "
                                     + ",UseBaseUn "
                                     + ",U_Driver "
                                     + ",U_TruckNo "
                                     + ",U_DeliveryType "
                                     + ",U_Weight "
                                     + ",U_Cancel "
                                     + ",DocStatus) "
                                     + "VALUES " +
                                     " ( @Guid, " +
                                     "@Name, " +
                                     "@OwnerCode, " +
                                     "@OwnerName, " +
                                     "@PickDate, " +
                                     "@Remarks, " +
                                     "@Canceled, " +
                                     "@ShipType, " +
                                     "@Status, " +
                                     "@Printed, " +
                                     "@LogInstac, " +
                                     "@ObjType, " +
                                     "@UpdateDate, " +
                                     "@CreateDate, " +
                                     "@UserSign, " +
                                     "@UserSign2, " +
                                     "@UseBaseUn, " +
                                     "@U_Driver, " +
                                     "@U_TruckNo, " +
                                     "@U_DeliveryType, " +
                                     "@U_Weight, " +
                                     "@U_Cancel, " +
                                     "@DocStatus); " +
                                     "SELECT CAST(SCOPE_IDENTITY() as int);";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var result = conn.QuerySingle<int>(queryInsert, PickHead);
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
        /// Update U_PickListNo In INV1
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public int UpdateRIPickItem(Cio bag, int PickID)
        {
            try
            {
                var queryUpdate = "UPDATE INV1 SET U_PickListNo = @U_PickListNo, U_PickedQty = 0  " +
                                  "WHERE DocEntry = @DocEntry; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    foreach (var line in bag.oINVs)
                    {
                        result = conn.Execute(queryUpdate, new { U_PickListNo = PickID, DocEntry = line.DocEntry });
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
        /// Get INV1 ItemLine From Ri (RI)
        /// </summary>
        /// <param name="PickDoc"></param>
        /// <returns></returns>
        public INV1_Ex[] GetPickDetailsFromRI(int PickDoc)
        {
            try
            {
                var Midwareconn = new SqlConnection(MiddatabaseConnStr);

                INV1_Ex[] iNV1_Ices = null;

                var query = "SELECT T1.DocNum, T1.CardName,T0.* FROM INV1 T0 INNER JOIN OINV T1 ON T0.DocEntry = T1.DocEntry WHERE U_PickListNo = @PickDoc AND LineStatus = 'O'";

                var querytwo = "SELECT * FROM zmwRIHoldPickItem WHERE PickListDocEntry = @PickDoc; ";

                using (var conn = new SqlConnection(databaseConnStr))
                {

                        iNV1_Ices = conn.Query<INV1_Ex>(query, new { PickDoc }).ToArray();

                        var holditemList = Midwareconn.Query<RIHoldPickItem>(querytwo,new { PickDoc }).ToList();
                        //if (iNV1_Ices == null) return null;
                        foreach(var line in holditemList)
                        {
                        iNV1_Ices.Where(x => x.DocEntry == line.RIDocEntry && x.U_PickListNo == line.PickListDocEntry).ToList().ForEach(y =>
                           {
                               y.OnHoldBatches.Add(line);
                           }
                        );
                        }
                    return iNV1_Ices;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get Allocated batch In RI (RI)
        /// </summary>
        /// <param name="iNVs"></param>
        /// <returns></returns>
        public BatchAllocateDocView[] GetAllocatedBatchFromRI(INV1_Ex[] iNVs)
        {
            try
            {
                var query = "SELECT MIN(T0.LogEntry) AS SnBAllocateViewLogEntry, T0.AllocateTp AS SnBAllocateViewDocType, T0.AllocatEnt AS SnBAllocateViewDocEntry, T0.AllocateLn AS SnBAllocateViewDocLine, T0.ManagedBy AS SnBAllocateViewMngBy, " +
                            "T1.MdAbsEntry AS SnBAllocateViewSnbMdAbs, T1.ItemCode AS SnBAllocateViewItemCode, T1.SysNumber AS SnBAllocateViewSnbSysNum, T0.LocCode AS SnBAllocateViewLocCode, SUM(T1.AllocQty) AS SnBAllocateViewAllocQty, T2.DistNumber  " +
                            "FROM dbo.OITL AS T0 " +
                            "INNER JOIN dbo.ITL1 AS T1 ON T1.LogEntry = T0.LogEntry " +
                            "INNER JOIN dbo.OBTN AS T2 ON T1.MdAbsEntry = T2.AbsEntry " +
                            "WHERE  (T1.AllocQty <> 0) and  T0.AllocateTp = 13 AND T0.AllocatEnt = @AllocatEnt " +
                            "GROUP BY T0.AllocateTp, T0.AllocatEnt, T0.AllocateLn, T0.ManagedBy, T1.MdAbsEntry, T1.ItemCode, T1.SysNumber, T0.LocCode, T0.DocEntry, T0.DocLine, T0.DocNum, T2.DistNumber " +
                            "HAVING(SUM(T1.AllocQty) > 0); ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    List<BatchAllocateDocView> batchItem = new List<BatchAllocateDocView>();
                    foreach (var line in iNVs)
                    {
                        batchItem.AddRange(conn.Query<BatchAllocateDocView>(query, new { AllocatEnt = line.DocEntry }).ToList());
                    }
                    return batchItem.ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get Allocated Batches For Single Item
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public BatchAllocateDocView[] RIGetBatchForSingleItem(Cio bag)
        {
            try
            {
                var query = "SELECT MIN(T0.LogEntry) AS SnBAllocateViewLogEntry, T0.AllocateTp AS SnBAllocateViewDocType, T0.AllocatEnt AS SnBAllocateViewDocEntry, T0.AllocateLn AS SnBAllocateViewDocLine, T0.ManagedBy AS SnBAllocateViewMngBy, " +
                            "T1.MdAbsEntry AS SnBAllocateViewSnbMdAbs, T1.ItemCode AS SnBAllocateViewItemCode, T1.SysNumber AS SnBAllocateViewSnbSysNum, T0.LocCode AS SnBAllocateViewLocCode, SUM(T1.AllocQty) AS SnBAllocateViewAllocQty, T2.DistNumber  " +
                            "FROM dbo.OITL AS T0 " +
                            "INNER JOIN dbo.ITL1 AS T1 ON T1.LogEntry = T0.LogEntry " +
                            "INNER JOIN dbo.OBTN AS T2 ON T1.MdAbsEntry = T2.AbsEntry " +
                            "WHERE  (T1.AllocQty <> 0) and  T0.AllocateTp = 13 AND T0.AllocatEnt = @AllocatEnt AND T0.DocLine = @DocLine " +
                            "GROUP BY T0.AllocateTp, T0.AllocatEnt, T0.AllocateLn, T0.ManagedBy, T1.MdAbsEntry, T1.ItemCode, T1.SysNumber, T0.LocCode, T0.DocEntry, T0.DocLine, T0.DocNum, T2.DistNumber " +
                            "HAVING(SUM(T1.AllocQty) > 0); ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var batch = conn.Query<BatchAllocateDocView>(query, new { AllocatEnt = bag.RIItemLine.DocEntry, DocLine = bag.RIItemLine.LineNum }).ToArray();
                    return batch;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Update U_PickListNo In INV1
        /// </summary>
        /// <param name="PickID"></param>
        /// <param name="iNV1s"></param>
        /// <returns></returns>
        public int RIUpdatePickList(int PickID, List<INV1_Ex> iNV1s)
        {
            try
            {
                var queryUpdate = "UPDATE INV1 SET U_PickedQty = @U_PickedQty  " +
                                  "WHERE U_PickListNo = @PickID AND  DocEntry = @DocEntry AND LineNum = @LineNum; ";
                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    foreach (var line in iNV1s)
                    {
                        result = conn.Execute(queryUpdate, new { PickID, DocEntry = line.DocEntry, LineNum = line.LineNum, U_PickedQty = line.U_PickedQty });
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
        /// Update DocStatus to Completed and Weight
        /// </summary>
        /// <param name="PickID"></param>
        /// <param name="Weight"></param>
        public int UpdateRIPickItemStatus(int PickID, decimal Weight)
        {
            try
            {
                var queryUpdate = "UPDATE zmwPickHead SET DocStatus = 'Completed', U_Weight = @U_Weight , UpdateDate = @UpdateDate " +
                                  "WHERE AbsEntry = @DocEntry; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(queryUpdate, new { DocEntry = PickID, U_Weight = Weight, UpdateDate = DateTime.Now });
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
        /// Update zmwPickHead 
        /// </summary>
        /// <param name="bag"></param>
        public int UpdateRIPickItemHeader(Cio bag)
        {
            try
            {
                var queryUpdate = "UPDATE zmwPickHead SET Name = @Name, PickDate = @PickDate, U_Driver = @Driver, U_TruckNo = @TruckNo, U_DeliveryType = @DeliveryType, UpdateDate = @UpdateDate, Remarks = @Remark " +
                                  "WHERE AbsEntry = @DocEntry; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(queryUpdate, new { DocEntry = bag.PickHead.AbsEntry, Name = bag.PickHead.Name, PickDate = bag.PickHead.PickDate, Driver = bag.PickHead.U_Driver, TruckNo = bag.PickHead.U_TruckNo, DeliveryType = bag.PickHead.U_DeliveryType, UpdateDate = DateTime.Now, Remark = bag.PickHead.Remarks });
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
        /// Update zmwPickHead to Approved Status and SAP actual Pick List
        /// </summary>
        /// <param name="PickID"></param>
        /// <param name="SAPPickNo"></param>
        public int UpdateRIPickItemStatusToApproved(int PickID, int SAPPickNo)
        {
            try
            {
                var queryUpdate = "UPDATE zmwPickHead SET DocStatus = 'Approved', UpdateDate = @UpdateDate, SAPPickDoc = @SAPPickNo " +
                                  "WHERE AbsEntry = @DocEntry; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(queryUpdate, new { DocEntry = PickID, UpdateDate = DateTime.Now, SAPPickNo = SAPPickNo });
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
                    foreach (var line in bag.INV1s)
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
        /// Remove U_PickedQty and U_PickListNo UDF IN INV1
        /// </summary>
        /// <param name="PickID"></param>
        public int RemovePickNoFromRI(int PickID)
        {
            try
            {
                var queryUpdate = "UPDATE INV1 SET U_PickedQty = 0, U_PickListNo = 0 " +
                                  "WHERE U_PickListNo = @PickID; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(queryUpdate, new { PickID });
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
        /// Update zmwPickHead to Reject Status
        /// </summary>
        /// <param name="PickID"></param>
        public int UpdateRIPickItemStatusToReject(OPKL_Ex Pickhead)
        {
            try
            {
                var queryUpdate = "UPDATE zmwPickHead SET DocStatus = 'Rejected', UpdateDate = @UpdateDate " +
                                  "WHERE AbsEntry = @DocEntry; ";

                int result = -1;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result = conn.Execute(queryUpdate, new { DocEntry = Pickhead.AbsEntry, UpdateDate = DateTime.Now });
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
        /// Get Pick List ItemLine with bacthes
        /// </summary>
        /// <param name="PickDoc"></param>
        public DTO_OPKL GetPickDetailsFromRIWithBatch(int PickDoc)
        {
            try
            {
                DTO_OPKL dTO_OPKL = new DTO_OPKL();

                var query1 = "SELECT T1.DocNum, T1.CardName ,T0.* FROM INV1 T0 INNER JOIN OINV T1 ON T0.DocEntry = T1.DocEntry WHERE U_PickListNo = @PickDoc AND LineStatus = 'O'; ";
                var query2 = "SELECT MIN(T0.LogEntry) AS SnBAllocateViewLogEntry, T0.AllocateTp AS SnBAllocateViewDocType, T0.AllocatEnt AS SnBAllocateViewDocEntry, T0.AllocateLn AS SnBAllocateViewDocLine, T0.ManagedBy AS SnBAllocateViewMngBy, " +
                            "T1.MdAbsEntry AS SnBAllocateViewSnbMdAbs, T1.ItemCode AS SnBAllocateViewItemCode, T1.SysNumber AS SnBAllocateViewSnbSysNum, T0.LocCode AS SnBAllocateViewLocCode, SUM(T1.AllocQty) AS SnBAllocateViewAllocQty, T2.DistNumber  " +
                            "FROM dbo.OITL AS T0 " +
                            "INNER JOIN dbo.ITL1 AS T1 ON T1.LogEntry = T0.LogEntry " +
                            "INNER JOIN dbo.OBTN AS T2 ON T1.MdAbsEntry = T2.AbsEntry " +
                            "WHERE  (T1.AllocQty <> 0) and  T0.AllocateTp = 13 AND T0.AllocatEnt = @AllocatEnt " +
                            "GROUP BY T0.AllocateTp, T0.AllocatEnt, T0.AllocateLn, T0.ManagedBy, T1.MdAbsEntry, T1.ItemCode, T1.SysNumber, T0.LocCode, T0.DocEntry, T0.DocLine, T0.DocNum, T2.DistNumber " +
                            "HAVING(SUM(T1.AllocQty) > 0); ";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    dTO_OPKL.iNV1s = conn.Query<INV1_Ex>(query1, new { PickDoc = PickDoc }).ToArray();
                    List<BatchAllocateDocView> batchItem = new List<BatchAllocateDocView>();
                    var invs = dTO_OPKL.iNV1s.Select(x => x.DocEntry).Distinct();
                    foreach (var line in invs)
                    {
                        batchItem.AddRange(conn.Query<BatchAllocateDocView>(query2, new { AllocatEnt = line }).ToList());
                    }
                    foreach (var line in dTO_OPKL.iNV1s)
                    {
                        line.AllocatedBatches.AddRange(batchItem.Where(x => x.SnBAllocateViewDocLine == line.LineNum && x.SnBAllocateViewDocEntry == line.DocEntry).ToList());
                    }
                    return dTO_OPKL;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }
    }
}




///// <summary>
///// Update PickedQuantity In RI
///// </summary>
///// <param name="PickDoc"></param>
///// <returns></returns>
//public int UpdateRIPickedTOZero(int PickID)
//{
//    try
//    {
//        var queryUpdate = "UPDATE INV1 SET U_PickedQty = 0  " +
//                          "WHERE U_PickListNo = @PickID; ";

//        int result = -1;
//        using (var conn = new SqlConnection(databaseConnStr))
//        {
//            result = conn.Execute(queryUpdate, new { PickID });
//            return result;
//        }

//    }
//    catch (Exception excep)
//    {
//        LastErrorMessage = $"{excep}";
//        return -1;
//    }
//}
