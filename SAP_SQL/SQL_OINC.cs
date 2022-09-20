using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSWebAPI.Class;
using WMSWebAPI.Models;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.InventoryCount;
using WMSWebAPI.Models.Request;
using zwaRequest = WMSWebAPI.Class.zwaRequest;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_OINC : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string databaseMidwareConnStr { get; set; } = "";
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
        public SQL_OINC(string dbConnStr, string dbMidwareConnStr = "")
        {
            databaseConnStr = dbConnStr;
            databaseMidwareConnStr = dbMidwareConnStr;
        }

        /// <summary>
        /// Get list of GRPO doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '1470000065'").ToArray();
        }

        /// <summary>
        /// Return list of the open statuc OINC - inventry counting doc
        /// </summary>
        /// <param name="docStatus"></param>
        /// <returns></returns>
        public OINC_Ex[] GetOpenOinc(string whsCode)
        {
            try
            {
                string query = "SELECT T0.*, T1.WhsCode FROM FTS_vw_IMApp_OINC T0 " +
                    "INNER JOIN INC1 T1 ON T0.DocEntry = T1.DocEntry " +
                    "WHERE T1.WhsCode = @whsCode; ";

                //if (docStatus.Length > 0) // open / closed / or all
                //{
                //    query += $"WHERE Status = @docStatus ";
                //}

                using var conn = new SqlConnection(this.databaseConnStr);
                return conn.Query<OINC_Ex>(query, new { whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Base on the oinc docentry ... return doc lines
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public INC1_Ex[] GetOincLines(int docEntry)
        {
            try
            {
                string query =
                    $"SELECT * " +
                    $"FROM FTS_vw_IMApp_INC1_Ex " +
                    $"WHERE DocEntry = @docEntry " +
                   // $"AND counted ='N' " +
                    $"AND LineStatus ='O'";

                using var conn = new SqlConnection(this.databaseConnStr);
                return conn.Query<INC1_Ex>(query, new { docEntry }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }


        /// <summary>
        /// Base on the oinc docentry ... return doc lines with Batch
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public INC1_DIST[] GetOincLinesWithBatch(int docEntry)
        {
            try
            {
                string query =
                    $"SELECT T0.[DocEntry],T0.[DocNum], T0.[CountDate], T1.[ItemCode], T2.[DistNumber], T1.[Quantity], T1.[WhsCode] " +
                    $"FROM OINC T0 " +
                    $"INNER JOIN INC3 T1 ON T0.[DocEntry] = T1.[DocEntry] " +
                    $"LEFT JOIN OBTN T2 on T2.[ItemCode] = T1.[ItemCode] and T1.[Objabs] = T2.[absentry] " +
                    $"WHERE T0.DocEntry = @DocEntry";

                using var conn = new SqlConnection(this.databaseConnStr);
                return conn.Query<INC1_DIST>(query, new { DocEntry = docEntry }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }


        /// <summary>
        /// Get list of the warehouse bin location for direction
        /// </summary>
        /// <returns></returns>
        public OBIN[] GetOBINs()
        {
            try
            {
                string query =
                    $"SELECT * " +
                    $"FROM {nameof(OBIN)} " +
                    $"WHERE Disabled = 'N'";

                using var conn = new SqlConnection(databaseConnStr);
                return conn.Query<OBIN>(query).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Create update inventory count Request
        /// </summary>
        public int CreateUpdateInventoryCountRequest(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine, zmwDocHeaderField docHeaderfield)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;


                ConnectAndStartTrans_Midware();
                //Comparing Data In Mobile and Actual Data in Sap
                //If match, return -1.
                //If Not match, delete record in middleware and insert new record

                using (conn)
                using (trans)
                {
                    #region Insert zwaRequest
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

                    #region Insert zwaGrpo
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
                    // add in the bin lines transaction 
                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
                        string insertItemBinLine =
                        $"INSERT INTO zmwItemBin(" +
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
                       $") VALUES (" +
                       $"@Guid " +
                       $",@DocSeries " +
                       $",@Ref2 " +
                       $",@Comments " +
                       $",@JrnlMemo " +
                       $",@NumAtCard" +
                       $")";

                        result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }
                    #endregion

                    CommitDatabase();


                    return CreateUpdateInventoryCountRequest_Midware(dtoRequest,
                                                        grpoLines, itemBinLine, docHeaderfield);
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                Rollback();
                return -1;
            }
        }

        //public int CheckDataDiffCount(zwaGRPO[] grpoLines)
        //{
        //    try
        //    {
        //        var docnum = grpoLines.Select(x => x.SourceDocNum).FirstOrDefault();
        //        using var conn = new SqlConnection(databaseConnStr);
        //        return conn.Query<int>("Select * From [dbo].[LoadCurrentAndSapDataDiff] (@DocNum)", new { DocNum = docnum }).Count();
        //    }
        //    catch (Exception excep)
        //    {
        //        LastErrorMessage = $"{excep}";
        //        return -1;
        //    }
        //}

        /// <summary>
        /// Create update inventory count Request
        /// </summary>
        public int CreateUpdateInventoryCountRequest_Midware(zwaRequest dtoRequest,
            zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine, zmwDocHeaderField docHeaderfield)
        {
            try
            {
                if (dtoRequest == null) return -1;
                if (grpoLines == null) return -1;
                if (grpoLines.Length == 0) return -1;
               
                ConnectAndStartTrans_Midware();                       

                using (conn)
                using (trans)
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

                    var result = conn.Execute(insertSql, dtoRequest, trans);
                    if (result < 0) { Rollback(); return -1; }
                    #endregion
                        
                    #region Insert zmwInventoryCountGRPO
                    /// perform insert of all the GRPO item 
                    ///     
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
                     
                    #region insert zwaItemBin

                    // add in the bin lines transaction 
                    if(itemBinLine!=null&& itemBinLine.Length>0)
                    {
                        string insertItemBinLine =
                       $"INSERT INTO zmwItemBin ( " +
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
                         $") VALUES (" +
                         $"@Guid " +
                         $",@DocSeries " +
                         $",@Ref2 " +
                         $",@Comments " +
                         $",@JrnlMemo " +
                         $",@NumAtCard" +
                         $")";

                        result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                        if (result < 0) { Rollback(); return -1; }
                    }

                    #endregion

                    //if (CheckDataDiffCount(grpoLines) <= 0)
                    //{
                    //    Rollback();
                    //    LastErrorMessage = "The data do not have any changes.";
                    //    return -1;
                    //}

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
        ///  use to init the database insert transation
        ///  mid ware
        /// </summary>
        public void ConnectAndStartTrans_Midware()
        {
            try
            {
                conn = new SqlConnection(databaseMidwareConnStr);
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
                trans?.Rollback();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
            }
        }
    }
}
