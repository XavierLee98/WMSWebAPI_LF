using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.Request;
namespace WMSWebAPI.SAP_SQL
{
    public class SQL_OIGN : IDisposable
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
        public SQL_OIGN(string dbConnStr, string dbConnStr_midware = "")
        {
            databaseConnStr = dbConnStr;
            databaseConnStr_midware = dbConnStr_midware;
        }

        /// <summary>
        /// return array of the GI doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '59'").ToArray();
        }

        /// <summary>
        /// Use to update the UDT for goods receipt doc series
        /// </summary>
        /// <param name="docSeries"></param>
        /// <returns></returns>
        public int UpdateGoodsReceiptDocSeries(string docSeries)
        {
            try
            {
                string updateSql = "UPDATE [@APPSETUP] SET U_DocumentSeries = @docSeries " +
                    "WHERE U_Operation='Goods Receipt'";

                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Execute(updateSql, new { docSeries });
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        /// Get Goods Receipt DocSeries
        /// </summary>
        /// <returns></returns>
        public string GetGoodsReceiptDocSeries()
        {
            try
            {
                string query = "SELECT U_DocumentSeries FROM [@APPSETUP] WHERE U_Operation='Goods Receipt'";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var result = conn.ExecuteScalar(query);
                    if (result == null) return string.Empty;

                    return (string)result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// Create GRPO Request
        /// </summary>
        public int CreateGoodsReceiveRequest(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines,
            zwaItemBin[] itemBinLine,
            zmwDocHeaderField docHeaderfield)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;
                if (docHeaderfield == null) return -1;

                ConnectAndStartTrans();
                using (conn)
                using (trans)
                {
                    #region Insert request
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
                    #endregion

                    #region Insert GRPO
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

                    result = conn.Execute(insertGrpo, grpoLines, trans);
                    if (result < 0) { Rollback(); return -1; };
                    #endregion

                    #region insert zwaItemBin

                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
                        // add in the bin lines transaction 
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
                    }
                    #endregion

                    CommitDatabase();
                    return CreateGoodsReceiveRequest_midware(dtoRequest,
                                                             grpoLines,
                                                             itemBinLine,
                                                            docHeaderfield);
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
        public int CreateGoodsReceiveRequest_midware(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines,
            zwaItemBin[] itemBinLine,
            zmwDocHeaderField docHeaderfield)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;
                if (docHeaderfield == null) return -1;

                ConnectAndStartTrans_midware();
                using (conn)
                using (trans)
                {
                    #region Insert request
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
                    if (result < 0) { Rollback(); return -1; };
                    #endregion

                    #region Insert GRPO
                    /// perform insert of all the GRPO item 
                    string insertGrpo = $"INSERT INTO zmwGRPO  " +
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

                    result = conn.Execute(insertGrpo, grpoLines, trans);
                    if (result < 0) { Rollback(); return -1; };
                    #endregion

                    #region insert zwaItemBin
                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
                        // add in the bin lines transaction 
                        // add in the bin lines transaction 
                        string insertItemBinLine =
                            $"INSERT INTO  zmwItemBin (" +
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
                    if (docHeaderfield != null)
                    {
                        // insert the doc header field
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
        /// For middle ware
        /// </summary>
        public void ConnectAndStartTrans_midware()
        {
            try
            {
                conn = new SqlConnection(databaseConnStr_midware);
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
