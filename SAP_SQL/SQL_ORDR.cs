using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Do;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.Request;
namespace WMSWebAPI.SAP_SQL
{
    public class SQL_ORDR : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string databaseConnStr_Midware { get; set; } = "";

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
        public SQL_ORDR(string dbConnStr, string dbConnStr_midware = "")
        {
            databaseConnStr = dbConnStr;
            databaseConnStr_Midware = dbConnStr_midware;
        }

        /// <summary>
        /// Return list of the purchase order line with open item
        /// </summary>
        /// <returns></returns>
        public ORDR_Ex[] GetOpenSo(Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_ORDR ";

                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<ORDR_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<ORDR_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }           
        }

        /// <summary>
        /// Return list of the PO line, based on Doc entry array 
        /// Support 1 or more purchase order doc entry
        /// </summary>
        /// <param name="DocEntry"></param>
        /// <returns></returns>
        public RDR1_Ex[] GetOpenSoLines(int[] poEntries)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);

                var RDR1s = new List<RDR1_Ex>();
                foreach (var docEntry in poEntries)
                {
                    var lines = conn.Query<RDR1_Ex>(
                        "SELECT * FROM FTS_vw_IMApp_RDR1 WHERE docEntry = @docEntry", new { docEntry }).ToArray();

                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    RDR1s.AddRange(lines);
                }

                return RDR1s.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '17'").ToArray();
        }

        /// <summary>
        /// Get list of the business partner from the database
        /// </summary>
        /// <returns></returns>
        public OCRD [] QueryBp()
        {
            try
            {
                return new SqlConnection(databaseConnStr)
                    .Query<OCRD>("SELECT * FROM FTS_vw_IMApp_OCRD_Customer").ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Base on the item code
        /// Query the item qty for each warehouse
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public OITW[] GetItemQtyWarehouse(string itemCode)
        {
            try
            {
                string query = $"SELECT * FROM {nameof(OITW)} WHERE ItemCode = @itemCode AND OnHand > 0";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OITW>(query, itemCode).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Create GRPO Request
        /// </summary>
        public int CreateDoRequest(
            zwaRequest dtoRequest,
            zwaGRPO[] doLines,
            zwaItemBin[] itemBinLine,
            zmwDocHeaderField docHeadField) // resue the zwaGRPO object table
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (doLines == null) return -1;
                if (doLines.Length == 0) return -1;
                //if (itemBinLine == null) return -1;
                //if (itemBinLine.Length == 0) return -1;
                if (docHeadField == null) return -1;

                ConnectAndStartTrans();
                using (conn)
                using (trans)
                {
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
                    var result = conn.Execute(insertSql, dtoRequest, trans);
                    if (result < 0) { Rollback(); return -1; };

                    /// perform insert of all the GRPO item                                        
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
                         $",LineGuid" +
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
                         $",@LineGuid" +
                         $")";

                    result = conn.Execute(insertGrpo, doLines, trans);
                    if (result < 0) { Rollback(); return -1; };
                    
                    #region insert zwaItemBin
                    // add in the bin lines transaction 
                    if (itemBinLine != null && itemBinLine.Length> 0)
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
                            $",LineGuid " +
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
                            $",@LineGuid " +
                            $")";
                        result = conn.Execute(insertItemBinLine, itemBinLine, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }

                    #endregion

                    #region insert zmwDocHeaderField

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

                    result = conn.Execute(insertdocHeaderfield, docHeadField, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion

                    CommitDatabase();
                    return CreateDoRequest_Midware( dtoRequest,
                                                    doLines,
                                                    itemBinLine,
                                                    docHeadField);
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
        /// Create GRPO Request
        /// </summary>
        public int CreateDoRequest_Midware(
            zwaRequest dtoRequest,
            zwaGRPO[] doLines,
            zwaItemBin[] itemBinLine,
            zmwDocHeaderField docHeadField) // resue the zwaGRPO object table
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (doLines == null) return -1;
                if (doLines.Length == 0) return -1;                                

                ConnectAndStartTrans_Midware();
                using (conn)
                using (trans)
                {
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
                    var result = conn.Execute(insertSql, dtoRequest, trans);
                    if (result < 0) { Rollback(); return -1; };

                    /// perform insert of all the GRPO item                                        
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
                         $",LineGuid" +
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
                         $",@LineGuid" +
                         $")";

                    result = conn.Execute(insertGrpo, doLines, trans);
                    if (result < 0) { Rollback(); return -1; };

                    #region insert zwaItemBin
                    // add in the bin lines transaction 
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
                            $",LineGuid " +
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
                            $",@LineGuid " +
                            $")";
                        result = conn.Execute(insertItemBinLine, itemBinLine, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }
                    #endregion

                    #region insert zmwDocHeaderField
                    if (docHeadField != null)
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

                        result = conn.Execute(insertdocHeaderfield, docHeadField, trans);
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
        /// Midware connection
        /// </summary>
        public void ConnectAndStartTrans_Midware()
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
    }
}
