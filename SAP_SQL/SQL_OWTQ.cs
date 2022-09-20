using Dapper;
using DbClass;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Demo.Transfer1;
using WMSWebAPI.Models.GRPO;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_OWTQ : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string databaseConnStr_Midware { get; set; } = "";
        string midwareDbName { get; set; }
        SqlConnection conn;
        SqlTransaction trans;

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbConnString"></param>
        public SQL_OWTQ(string dbConnStr, string dbConnStrMidware, string midDbName)
        {
            databaseConnStr = dbConnStr;
            databaseConnStr_Midware = dbConnStrMidware;
            midwareDbName = midDbName;
        }

        /// <summary>
        /// return inv request transfer request doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '1250000001'").ToArray();
        }

        /// <summary>
        /// get list of the on hold request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public zwaHoldRequest[] GetHoldRequest(Cio bag)
        {
            try
            {

                return new SqlConnection(databaseConnStr_Midware) // <-- handle open and close
                     .Query<zwaHoldRequest>("SELECT * FROM zmwHoldRequest").ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get data from OWTQ - inventory transfer request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OWTQ[] GetTransferRequestList(Cio bag)
        {
            try
            {
                // get all line from the request
                string query = string.Empty;
                if (bag.RequestTransferDocFilter.Equals("a")) // <-- all list based on the date
                {
                    query = "SELECT * FROM [FTS_vw_IMApp_OWTQ] " +
                            "WHERE Docdate >= @StartDate " +
                            "AND docDate <= @EndDate ";
                    return new SqlConnection(databaseConnStr)
                           .Query<OWTQ>(query,
                           new
                           {
                               StartDate = bag.RequestTransferStartDt,
                               EndDate = bag.RequestTransferEndDt
                           }).ToArray();
                }

                // list of open request in the date range (as compare onhold status)
                if (bag.RequestTransferDocFilter.Equals("o"))
                {
                    query = "SELECT * FROM [FTS_vw_IMApp_OWTQ] " +
                          "WHERE Docdate >= @StartDate " +
                          "AND DocDate <= @EndDate " +
                          "AND DocStatus = @DocStatus " +
                          $"AND DocEntry NOT IN (SELECT DocEntry FROM [{midwareDbName}].[dbo].[zmwHoldRequest] WHERE Status ='O')";

                    return new SqlConnection(databaseConnStr) // <-- handle open and close
                           .Query<OWTQ>(query,
                           new
                           {
                               StartDate = bag.RequestTransferStartDt,
                               EndDate = bag.RequestTransferEndDt,
                               DocStatus = bag.RequestTransferDocFilter
                           }).ToArray();
                }

                // get all onhold request
                if (bag.RequestTransferDocFilter.Equals("h"))
                {
                    query = $"SELECT * FROM [{midwareDbName}].[dbo].[zmwHoldRequest] " +
                          "WHERE TransDate >= @StartDate " +
                          "AND TransDate <= @EndDate ";

                    return new SqlConnection(databaseConnStr) // <-- handle open and close
                           .Query<OWTQ>(query,
                           new
                           {
                               StartDate = bag.RequestTransferStartDt,
                               EndDate = bag.RequestTransferEndDt
                           }).ToArray();
                }

                // list of closed request in the date range
                if (bag.RequestTransferDocFilter.Equals("c"))
                {
                    query = "SELECT * FROM [FTS_vw_IMApp_OWTQ] " +
                               "WHERE Docdate >= @StartDate " +
                               "AND docDate <= @EndDate " +
                               "AND DocStatus = @DocStatus";
                    return new SqlConnection(databaseConnStr) // <-- handle open and close
                           .Query<OWTQ>(query,
                           new
                           {
                               StartDate = bag.RequestTransferStartDt,
                               EndDate = bag.RequestTransferEndDt,
                               DocStatus = bag.RequestTransferDocFilter
                           }).ToArray();
                }
                return null;
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// return transfer request open line
        /// </summary>
        /// <returns></returns>
        public WTQ1[] GetTransferRequestLines(int transferRequestDocEntry)
        {
            try
            {
                // get open line from the request
                return new SqlConnection(databaseConnStr).Query<WTQ1>(
                    $"select * from [FTS_vw_IMApp_WTQ1] " +
                    $"where DocEntry = @DocEntry", new { DocEntry = transferRequestDocEntry }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// check item code
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public OITM CheckItemCodeExist(string itemCode)
        {
            // get open line from the request
            return new SqlConnection(databaseConnStr).Query<OITM>(
                $"select * from [FTS_vw_IMApp_OITM] " +
                $"where ItemCode = @ItemCode", new { ItemCode = itemCode }).FirstOrDefault();
        }

        /// <summary>
        /// check item code
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public OITW CheckItemCodeQtyInWarehouse(string ItemCode, string WhsCode)
        {
            return new SqlConnection(databaseConnStr).Query<OITW>(
                $"SELECT * FROM [FTS_vw_IMApp_OITW] " +
                $"WHERE ItemCode = @ItemCode " +
                $"AND WhsCode = @WhsCode", new { ItemCode, WhsCode }).FirstOrDefault();
        }

        /// <summary>
        /// Get Item Whs Bin
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        public CommonStockInfo[] GetItemWhsBin(string ItemCode, string WhsCode)
        {
            return new SqlConnection(databaseConnStr).Query<CommonStockInfo>(
                $"SELECT * FROM [FTS_vw_IMApp_ItemWhsBin] " +
                $"WHERE ItemCode = @ItemCode " +
                $"AND WhsCode = @WhsCode", new { ItemCode, WhsCode }).ToArray();
        }

        /// <summary>
        /// Get List of the item, whs and serial
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        /// <returns></returns>
        public CommonStockInfo[] GetItemWhsBinSerial(string ItemCode, string WhsCode)
        {
            return new SqlConnection(databaseConnStr).Query<CommonStockInfo>(
                $"SELECT * FROM [{nameof(FTS_vw_IMApp_BinSerialContent)}] " +
                $"WHERE ItemCode = @ItemCode " +
                $"AND WhsCode = @WhsCode", new { ItemCode, WhsCode }).ToArray();
        }

        /// <summary>
        /// Get List of the item, whs and batch
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        /// <returns></returns>
        public CommonStockInfo[] GetItemWhsBinBatch(string ItemCode, string WhsCode)
        {
            return new SqlConnection(databaseConnStr).Query<CommonStockInfo>(
                $"SELECT * FROM [{nameof(FTS_vw_IMApp_BinBatchContent)}] " +
                $"WHERE ItemCode = @ItemCode " +
                $"AND WhsCode = @WhsCode", new { ItemCode, WhsCode }).ToArray();
        }

        /// <summary>
        /// List of item in a warehouse with available batch
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        /// <returns></returns>
        public CommonStockInfo[] GetItemWhsBatch(string ItemCode, string WhsCode)
        {
            return new SqlConnection(databaseConnStr).Query<CommonStockInfo>(
                $"SELECT * FROM FTS_vw_IMApp_ItemWhsBatch " +
                $"WHERE ItemCode = @ItemCode " +
                $"AND WhsCode = @WhsCode", new { ItemCode, WhsCode }).ToArray();
        }

        /// <summary>
        /// List of item in a warehouse with available serial
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        /// <returns></returns>
        public CommonStockInfo[] GetItemWhsSerial(string ItemCode, string WhsCode)
        {
            return new SqlConnection(databaseConnStr).Query<CommonStockInfo>(
                $"SELECT * FROM FTS_vw_IMApp_ItemWhsSerial " +
                $"WHERE ItemCode = @ItemCode " +
                $"AND WhsCode = @WhsCode", new { ItemCode, WhsCode }).ToArray();
        }

        /// <summary>
        ///  use to init the database insert transation
        /// </summary>
        public void ConnectAndStartTrans()
        {
            try
            {
                conn = new SqlConnection(databaseConnStr);
                conn.Open();
                trans = conn.BeginTransaction();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
            }
        }

        /// <summary>
        /// serve midware transaction
        /// </summary>
        public void ConnectAndStartTrans_midware()
        {
            try
            {
                conn = new SqlConnection(databaseConnStr_Midware);
                conn.Open();
                trans = conn.BeginTransaction();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
            }
        }

        /// <summary>
        /// Use to commit a database
        /// </summary>
        public void CommitDatabase()
        {
            try
            {
                trans?.Commit();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
            }
        }

        /// <summary>
        /// Use to roll back a transaction
        /// </summary>
        public void Rollback()
        {
            try
            {
                trans.Rollback();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
            }
        }

        /// <summary>
        /// Create Inventory Request
        /// </summary>
        /// <returns></returns>
        public int CreateInventoryRequest(
            zwaRequest dtoRequest,
            zwaInventoryRequest[] dtozwaInventoryRequest, 
            zwaInventoryRequestHead head,
            zmwDocHeaderField docHeaderField)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (dtozwaInventoryRequest == null) return -1;
                if (dtozwaInventoryRequest.Length == 0) return -1;
                if (head == null) return -1;

                return CreateInventoryRequest_Midware(dtoRequest, dtozwaInventoryRequest, head, docHeaderField);
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                Rollback();
                return -1;
            }
        }

        /// <summary>
        /// Create Inventory Request
        /// </summary>
        /// <returns></returns>
        public int CreateInventoryRequest_Midware(
            zwaRequest dtoRequest,
            zwaInventoryRequest[] dtozwaInventoryRequest, 
            zwaInventoryRequestHead head,
            zmwDocHeaderField DocHeaderField)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (dtozwaInventoryRequest == null) return -1;
                if (head == null) return -1;
                if (dtozwaInventoryRequest.Length == 0) return -1;

                ConnectAndStartTrans_midware();

                using (conn)
                using (trans)
                {
                    #region insert the request
                    string insertSql = $"INSERT INTO zmwRequest (" +
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

                    var result = conn.Execute(insertSql, dtoRequest, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    #region insert zmwInventoryRequest
                    string insertInventoryRequest = $"INSERT INTO zmwInventoryRequest " +
                    $"([Guid] " +
                            $",[ItemCode] " +
                            $",[Quantity] " +
                            $",[FromWarehouse] " +
                            $",[ToWarehouse] " +
                            $",[AppUser] " +
                            $",[TransTime]) " +
                            $"VALUES " +
                            $"(@Guid " +
                            $",@ItemCode " +
                            $",@Quantity " +
                            $",@FromWarehouse " +
                            $",@ToWarehouse " +
                            $",@AppUser " +
                            $",@TransTime) ";

                    result = conn.Execute(insertInventoryRequest, dtozwaInventoryRequest, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    #region insert the head
                    string insertHead =
                        $"INSERT INTO zmwInventoryRequestHead " +
                        $"([ToWarehouse]" +
                        $",[FromWarehouse]" +
                        $",[Guid]" +
                        $",[TransDate] " +
                        $",[DocNumber] " +
                        $",[Remarks]" +
                        $") VALUES " +
                        $"(@ToWarehouse" +
                        $",@FromWarehouse" +
                        $",@Guid" +
                        $",@TransDate " +
                        $",@DocNumber " +
                        $",@Remarks)";

                    result = conn.Execute(insertHead, head, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    #region insert the zmwDocHeaderField
                    if (DocHeaderField!=null)
                    {
                        string insertdocHeaderfield =
                               $"INSERT INTO {nameof(zmwDocHeaderField)}(" +
                               $" Guid " +
                               $",DocSeries " +
                               $",Ref2 " +
                               $",Comments " +
                               $",JrnlMemo " +
                               $",NumAtCard" +
                               $",Series" +
                               $") VALUES (" +
                               $"@Guid " +
                               $",@DocSeries " +
                               $",@Ref2 " +
                               $",@Comments " +
                               $",@JrnlMemo " +
                               $",@NumAtCard" +
                               $",@Series" +
                               $")";

                        result = conn.Execute(insertdocHeaderfield, DocHeaderField, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }
                    #endregion

                    CommitDatabase();
                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                Rollback();
                return -1;
            }
        }
    }
}
