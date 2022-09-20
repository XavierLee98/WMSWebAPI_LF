using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSApp.Models.SAP;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.Request;
using WMSWebAPI.Models.ReturnRequest;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_ORRR : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string databaseConnStr_midware { get; set; } = "";
        SqlConnection conn;
        SqlTransaction trans;

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbConnString"></param>
        public SQL_ORRR(string dbConnStr, string dbConnStr_midware = "")
        {
            databaseConnStr = dbConnStr;
            databaseConnStr_midware = dbConnStr_midware;
        }
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
        /// for connect others database
        /// </summary>
        /// <param name="dbConnstr"></param>
        public void ConnectAndStartTrans(string dbConnstr)
        {
            try
            {
                conn = new SqlConnection(dbConnstr);
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
        /// Create GRPO Request
        /// </summary>
        public int CreateGoodsReturnRequest(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine, zmwDocHeaderField docHeaderfield)
        {
            try
            {
                int result = -1;
                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;

                ConnectAndStartTrans();

                using (conn)
                using (trans)
                {
                    #region insert zwaRequest
                    string insertSql = $"INSERT INTO {nameof(zwaRequest)} (" +
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

                    result = conn.Execute(insertSql, dtoRequest, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    #region insert zwaGRPO
                    string insertGrpo = $"INSERT INTO {nameof(zwaGRPO)} " +
                         $"(Guid" +
                         $",ItemCode" +
                         $",Qty" +
                         $",SourceCardCode" +
                         $",SourceDocNum" +
                         $",SourceDocEntry" +
                         $",SourceDocBaseType" +
                         $",SourceBaseEntry" +
                         $",SourceBaseLine" +
                         $",Warehouse" +
                         $",SourceDocType" +
                         $") VALUES (" +
                         $"@Guid" +
                         $",@ItemCode" +
                         $",@Qty" +
                         $",@SourceCardCode" +
                         $",@SourceDocNum" +
                         $",@SourceDocEntry" +
                         $",@SourceDocBaseType" +
                         $",@SourceBaseEntry" +
                         $",@SourceBaseLine " +
                         $",@Warehouse" +
                         $",@SourceDocType" +
                         $")";

                    result = conn.Execute(insertGrpo, grpoLines, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    #region insert zwaItemBin
                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
                        // add in the bin lines transaction 
                        string insertItemBinLine =
                            $"INSERT INTO {nameof(zwaItemBin)}(" +
                            $"Guid" +
                            $",ItemCode" +
                            $",Quantity" +
                            $",BinCode" +
                            $",BinAbsEntry" +
                            $",BatchNumber" +
                            $",SerialNumber" +
                            $",TransType" +
                            $",TransDateTime " +
                            $",BatchAttr1 " +
                            $",BatchAttr2 " +
                            $",BatchAdmissionDate " +
                            $",BatchExpiredDate " +
                            $")VALUES(" +
                            $"@Guid" +
                            $",@ItemCode" +
                            $",@Quantity" +
                            $",@BinCode" +
                            $",@BinAbsEntry" +
                            $",@BatchNumber" +
                            $",@SerialNumber" +
                            $",@TransType" +
                            $",@TransDateTime" +
                            $",@BatchAttr1 " +
                            $",@BatchAttr2 " +
                            $",@BatchAdmissionDate " +
                            $",@BatchExpiredDate " +
                            $")";

                        result = conn.Execute(insertItemBinLine, itemBinLine, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }
                    #endregion

                    #region insert zmwDocHeaderField
                    if (docHeaderfield != null)
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

                        result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                        if (result < 0) { Rollback(); return -1; }

                        //string insertdocHeaderfield =
                        //   $"INSERT INTO {nameof(zmwDocHeaderField)}(" +
                        //   $" Guid " +
                        //   $",DocSeries " +
                        //   $",Ref2 " +
                        //   $",Comments " +
                        //   $",JrnlMemo " +
                        //   $",NumAtCard" +
                        //   $") VALUES (" +
                        //   $"@Guid " +
                        //   $",@DocSeries " +
                        //   $",@Ref2 " +
                        //   $",@Comments " +
                        //   $",@JrnlMemo " +
                        //   $",@NumAtCard" +
                        //   $")";

                        //result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                        //if (result < 0) { Rollback(); return -1; }
                    }
                    #endregion

                    CommitDatabase();

                    CreateGoodsReturnRequest_Mw(dtoRequest, grpoLines, itemBinLine, docHeaderfield); // add in for GKS on middleware
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

        /// <summary>
        /// Create grpo line for middleware GKS site
        /// </summary>
        /// <param name="dtoRequest"></param>
        /// <param name="grpoLines"></param>
        /// <param name="itemBinLine"></param>
        /// <returns></returns>
        public int CreateGoodsReturnRequest_Mw(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine, zmwDocHeaderField docHeaderfield)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;

                ConnectAndStartTrans(this.databaseConnStr_midware);
                using (conn)
                using (trans)
                {
                    #region insert zmwRequest
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

                    #region insert zmwGRPO table
                    string insertGrpo = $"INSERT INTO zmwGRPO " +
                          $"(Guid" +
                          $",ItemCode" +
                          $",Qty" +
                          $",SourceCardCode" +
                          $",SourceDocNum" +
                          $",SourceDocEntry" +
                          $",SourceDocBaseType" +
                          $",SourceBaseEntry" +
                          $",SourceBaseLine" +
                          $",Warehouse" +
                          $",SourceDocType" +
                          $") VALUES (" +
                          $"@Guid" +
                          $",@ItemCode" +
                          $",@Qty" +
                          $",@SourceCardCode" +
                          $",@SourceDocNum" +
                          $",@SourceDocEntry" +
                          $",@SourceDocBaseType" +
                          $",@SourceBaseEntry" +
                          $",@SourceBaseLine " +
                          $",@Warehouse" +
                          $",@SourceDocType" +
                          $")";

                    result = conn.Execute(insertGrpo, grpoLines, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    #region insert zmwItemBin
                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
                        // add in the bin lines transaction 
                        string insertItemBinLine =
                            $"INSERT INTO zmwItemBin (" +
                            $"Guid" +
                            $",ItemCode" +
                            $",Quantity" +
                            $",BinCode" +
                            $",BinAbsEntry" +
                            $",BatchNumber" +
                            $",SerialNumber" +
                            $",TransType" +
                            $",TransDateTime " +
                            $",BatchAttr1 " +
                            $",BatchAttr2 " +
                            $",BatchAdmissionDate " +
                            $",BatchExpiredDate " +
                            $")VALUES(" +
                            $"@Guid" +
                            $",@ItemCode" +
                            $",@Quantity" +
                            $",@BinCode" +
                            $",@BinAbsEntry" +
                            $",@BatchNumber" +
                            $",@SerialNumber" +
                            $",@TransType" +
                            $",@TransDateTime" +
                            $",@BatchAttr1 " +
                            $",@BatchAttr2 " +
                            $",@BatchAdmissionDate " +
                            $",@BatchExpiredDate " +
                            $")";

                        result = conn.Execute(insertItemBinLine, itemBinLine, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }
                    #endregion

                    #region insert zmwDocHeaderField
                    if (docHeaderfield != null)
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

                        result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                        if (result < 0) { Rollback(); return -1; }
                        //string insertdocHeaderfield =
                        //    $"INSERT INTO zmwDocHeaderField (" +
                        //    $" Guid " +
                        //    $",DocSeries " +
                        //    $",Ref2 " +
                        //    $",Comments " +
                        //    $",JrnlMemo " +
                        //    $",NumAtCard" +
                        //    $") VALUES (" +
                        //    $"@Guid " +
                        //    $",@DocSeries " +
                        //    $",@Ref2 " +
                        //    $",@Comments " +
                        //    $",@JrnlMemo " +
                        //    $",@NumAtCard" +
                        //    $")";

                        //result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                        //if (result < 0) { Rollback(); return -1; }
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


        /// <summary>
        /// Get list of delivery lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public DLN1_Ex [] GetDoLines (Cio bag)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);
                var dln1 = new List<DLN1_Ex>();

                foreach (var docEntry in bag.PoDocEntries)
                {
                    var lines = conn.Query<DLN1_Ex>(
                        "SELECT * FROM [FTS_vw_IMApp_DLN1] WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    dln1.AddRange(lines);
                }

                return dln1.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        public INV1_Ex[] GetArInvLines(Cio bag)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);
                var inv1 = new List<INV1_Ex>();

                foreach (var docEntry in bag.PoDocEntries)
                {
                    var lines = conn.Query<INV1_Ex>(
                        "SELECT * FROM [FTS_vw_IMApp_INV1] WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    inv1.AddRange(lines);
                }

                return inv1.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }


        /// <summary>
        /// Get list of open delivery order
        /// </summary>
        /// <param name="nag"></param>
        /// <returns></returns>
        public ODLN_Ex [] GetOpenDOs (Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_ODLN ";

                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<ODLN_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<ODLN_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get all open invoice 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OINV_Ex [] GetOpenInvs (Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_OINV ";

                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OINV_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OINV_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get Open Return Request
        /// </summary>
        /// <returns></returns>
        public ORRR_Ex [] GetOpenReturnRequest(Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_ORRR ";

                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<ORRR_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<ORRR_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get list of GRPO doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '234000031'").ToArray();
        }

    }
}
