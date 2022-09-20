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
    public class SQL_ORDN : IDisposable
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

        public SQL_ORDN(string dbConnStr, string dbConnStr_midware = "")
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
        public void ConnectAndStartTrans_Midware()
        {
            try
            {
                conn = new SqlConnection(this.databaseConnStr_midware);
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
        /// Get ORDN doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '16'").ToArray();
        }

        /// <summary>
        /// Get list of delivery lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public RRR1_Ex[] GetOpenReturnRequestLines(Cio bag)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);

                var rrr1 = new List<RRR1_Ex>();

                foreach (var docEntry in bag.PoDocEntries)
                {
                    var lines = conn.Query<RRR1_Ex>(
                        "SELECT * FROM [FTS_vw_IMApp_RRR1] WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    rrr1.AddRange(lines);
                }

                return rrr1.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get list of delivery lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public DLN1_Ex[] GetOpenDeliveryOrderLines(Cio bag)
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

        /// <summary>
        /// Get list of open delivery order
        /// </summary>
        /// <param name="nag"></param>
        /// <returns></returns>
        public ODLN_Ex[] GetOpenDOs(Cio bag)
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
        /// Get Open Return Request
        /// </summary>
        /// <returns></returns>
        public ORRR_Ex[] GetOpenReturnRequest(Cio bag)
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
        /// Get Serial in DO 
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public Batch[] GetBatchInDo(int docEntry, int lineNum)
        {
            try
            {
                // [FTS_vw_BatchesInDO]
                string query = "Select * from [FTS_vw_BatchesInDO] where DocEntry = @docEntry AND LineNum =@lineNum";
                using var conn = new SqlConnection(this.databaseConnStr);
                List<Batch> returnList = new List<Batch>();

                var resultList = conn.Query<dynamic>(query, new { DocEntry = docEntry, LineNum = lineNum }).ToList();

                var batches = new List<Batch>();
                resultList.ForEach(x => returnList.Add(
                    new Batch
                    {
                        DistNumber = (string)x.BatchNum,
                        Qty = (Decimal)x.Quantity,
                        Attribute1 = (string)x.Attribute1,
                        Attribute2 = (string)x.Attribute2,
                        Admissiondate = (DateTime) x.Admissiondate,
                        Expireddate = (DateTime) x.Expireddate
                    }));

                return returnList?.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get Serial in DO 
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public string[] GetSerialInDo(int docEntry, int lineNum)
        {
            try
            {
                string query = "Select * from [FTS_vw_SerialsInDO] where DocEntry = @docEntry and LineNum = @linenum";
                using var conn = new SqlConnection(this.databaseConnStr);
                List<string> returnList = new List<string>();

                var resultList = conn.Query<dynamic>(query, new { DocEntry = docEntry, LineNum = lineNum }).ToList();
                if (resultList != null)
                {
                    resultList.ForEach(x => returnList.Add(x.Serial));
                }

                return returnList?.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Return the query OSRN to app
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OSRI CheckSerialStatus(Cio bag)
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<OSRI>("SELECT * FROM OSRI " +
                "WHERE IntrSerial = @QueryDistNum", 
                new { bag.QueryDistNum }).FirstOrDefault();
        }


        public OBTN CheckBatchStatus (Cio bag)
        {         
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<OBTN>("SELECT * FROM OBTN WHERE DistNumber = @QueryDistNum",
                new { bag.QueryDistNum }).FirstOrDefault();
        }
    }

    ///// <summary>
    ///// Create GRPO Request
    ///// </summary>
    //public int CreateReturn(zwaRequest dtoRequest,
    //    zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine, zmwDocHeaderField docHeaderfield)
    //{
    //    try
    //    {
    //        int result = -1;
    //        if (dtoRequest == null) return -1;
    //        if (grpoLines == null) return -1;
    //        if (grpoLines.Length == 0) return -1;

    //        ConnectAndStartTrans();

    //        using (conn)
    //        using (trans)
    //        {
    //            #region insert zwaRequest
    //            string insertSql = $"INSERT INTO {nameof(zwaRequest)} (" +
    //               $"request" +
    //               $",sapUser " +
    //               $",sapPassword" +
    //               $",requestTime" +
    //               $",phoneRegID" +
    //               $",status" +
    //               $",guid" +
    //               $",sapDocNumber" +
    //               $",completedTime" +
    //               $",attachFileCnt" +
    //               $",tried" +
    //               $",createSAPUserSysId " +
    //               $")VALUES(" +
    //               $"@request" +
    //               $",@sapUser" +
    //               $",@sapPassword" +
    //               $",GETDATE()" +
    //               $",@phoneRegID" +
    //               $",@status" +
    //               $",@guid" +
    //               $",@sapDocNumber" +
    //               $",GETDATE()" +
    //               $",@attachFileCnt" +
    //               $",@tried" +
    //               $",@createSAPUserSysId)";

    //            result = conn.Execute(insertSql, dtoRequest, trans);
    //            if (result < 0) { Rollback(); return -1; }
    //            #endregion

    //            #region insert zwaGRPO
    //            string insertGrpo = $"INSERT INTO {nameof(zwaGRPO)} " +
    //                 $"(Guid" +
    //                 $",ItemCode" +
    //                 $",Qty" +
    //                 $",SourceCardCode" +
    //                 $",SourceDocNum" +
    //                 $",SourceDocEntry" +
    //                 $",SourceDocBaseType" +
    //                 $",SourceBaseEntry" +
    //                 $",SourceBaseLine" +
    //                 $",Warehouse" +
    //                 $",SourceDocType" +
    //                 $") VALUES (" +
    //                 $"@Guid" +
    //                 $",@ItemCode" +
    //                 $",@Qty" +
    //                 $",@SourceCardCode" +
    //                 $",@SourceDocNum" +
    //                 $",@SourceDocEntry" +
    //                 $",@SourceDocBaseType" +
    //                 $",@SourceBaseEntry" +
    //                 $",@SourceBaseLine " +
    //                 $",@Warehouse" +
    //                 $",@SourceDocType" +
    //                 $")";

    //            result = conn.Execute(insertGrpo, grpoLines, trans);
    //            if (result < 0) { Rollback(); return -1; }
    //            #endregion

    //            #region insert zwaItemBin
    //            if (itemBinLine != null && itemBinLine.Length > 0)
    //            {
    //                // add in the bin lines transaction 
    //                string insertItemBinLine =
    //                    $"INSERT INTO {nameof(zwaItemBin)}(" +
    //                    $"Guid" +
    //                    $",ItemCode" +
    //                    $",Quantity" +
    //                    $",BinCode" +
    //                    $",BinAbsEntry" +
    //                    $",BatchNumber" +
    //                    $",SerialNumber" +
    //                    $",TransType" +
    //                    $",TransDateTime " +
    //                    $",BatchAttr1 " +
    //                    $",BatchAttr2 " +
    //                    $",BatchAdmissionDate " +
    //                    $",BatchExpiredDate " +
    //                    $")VALUES(" +
    //                    $"@Guid" +
    //                    $",@ItemCode" +
    //                    $",@Quantity" +
    //                    $",@BinCode" +
    //                    $",@BinAbsEntry" +
    //                    $",@BatchNumber" +
    //                    $",@SerialNumber" +
    //                    $",@TransType" +
    //                    $",@TransDateTime" +
    //                    $",@BatchAttr1 " +
    //                    $",@BatchAttr2 " +
    //                    $",@BatchAdmissionDate " +
    //                    $",@BatchExpiredDate " +
    //                    $")";

    //                result = conn.Execute(insertItemBinLine, itemBinLine, trans);
    //                if (result < 0) { Rollback(); return -1; }
    //            }
    //            #endregion

    //            #region insert zmwDocHeaderField
    //            if (docHeaderfield != null)
    //            {
    //                string insertdocHeaderfield =
    //                   $"INSERT INTO {nameof(zmwDocHeaderField)}(" +
    //                   $" Guid " +
    //                   $",DocSeries " +
    //                   $",Ref2 " +
    //                   $",Comments " +
    //                   $",JrnlMemo " +
    //                   $",NumAtCard" +
    //                   $",Series" +
    //                   $") VALUES (" +
    //                   $"@Guid " +
    //                   $",@DocSeries " +
    //                   $",@Ref2 " +
    //                   $",@Comments " +
    //                   $",@JrnlMemo " +
    //                   $",@NumAtCard" +
    //                   $",@Series" +
    //                   $")";

    //                result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
    //                if (result < 0) { Rollback(); return -1; }

    //            }
    //            #endregion

    //            CommitDatabase();

    //            CreateReturn_Mw(dtoRequest, grpoLines, itemBinLine, docHeaderfield); // add in for GKS on middleware
    //            return result;
    //        }
    //    }
    //    catch (Exception excep)
    //    {
    //        LastErrorMessage = $"{excep}";
    //        Rollback();
    //        return -1;
    //    }
    //}

    ///// <summary>
    ///// Create grpo line for middleware GKS site
    ///// </summary>
    ///// <param name="dtoRequest"></param>
    ///// <param name="grpoLines"></param>
    ///// <param name="itemBinLine"></param>
    ///// <returns></returns>
    //public int CreateReturn_Mw(zwaRequest dtoRequest,
    //    zwaGRPO[] grpoLines, zwaItemBin[] itemBinLine, zmwDocHeaderField docHeaderfield)
    //{
    //    try
    //    {
    //        if (dtoRequest == null) return -1;
    //        if (grpoLines == null) return -1;
    //        if (grpoLines.Length == 0) return -1;

    //        ConnectAndStartTrans_Midware();
    //        using (conn)
    //        using (trans)
    //        {
    //            #region insert zmwRequest
    //            string insertSql = $"INSERT INTO zmwRequest (" +
    //                   $"request" +
    //                   $",sapUser " +
    //                   $",sapPassword" +
    //                   $",requestTime" +
    //                   $",phoneRegID" +
    //                   $",status" +
    //                   $",guid" +
    //                   $",sapDocNumber" +
    //                   $",completedTime" +
    //                   $",attachFileCnt" +
    //                   $",tried" +
    //                   $",createSAPUserSysId " +
    //                   $")VALUES(" +
    //                   $"@request" +
    //                   $",@sapUser" +
    //                   $",@sapPassword" +
    //                   $",GETDATE()" +
    //                   $",@phoneRegID" +
    //                   $",@status" +
    //                   $",@guid" +
    //                   $",@sapDocNumber" +
    //                   $",GETDATE()" +
    //                   $",@attachFileCnt" +
    //                   $",@tried" +
    //                   $",@createSAPUserSysId)";
    //            var result = conn.Execute(insertSql, dtoRequest, trans);
    //            if (result < 0) { Rollback(); return -1; }
    //            #endregion

    //            #region insert zmwGRPO table
    //            string insertGrpo = $"INSERT INTO zmwGRPO " +
    //                  $"(Guid" +
    //                  $",ItemCode" +
    //                  $",Qty" +
    //                  $",SourceCardCode" +
    //                  $",SourceDocNum" +
    //                  $",SourceDocEntry" +
    //                  $",SourceDocBaseType" +
    //                  $",SourceBaseEntry" +
    //                  $",SourceBaseLine" +
    //                  $",Warehouse" +
    //                  $",SourceDocType" +
    //                  $") VALUES (" +
    //                  $"@Guid" +
    //                  $",@ItemCode" +
    //                  $",@Qty" +
    //                  $",@SourceCardCode" +
    //                  $",@SourceDocNum" +
    //                  $",@SourceDocEntry" +
    //                  $",@SourceDocBaseType" +
    //                  $",@SourceBaseEntry" +
    //                  $",@SourceBaseLine " +
    //                  $",@Warehouse" +
    //                  $",@SourceDocType" +
    //                  $")";

    //            result = conn.Execute(insertGrpo, grpoLines, trans);
    //            if (result < 0) { Rollback(); return -1; }
    //            #endregion

    //            #region insert zmwItemBin
    //            if (itemBinLine != null && itemBinLine.Length > 0)
    //            {
    //                // add in the bin lines transaction 
    //                string insertItemBinLine =
    //                    $"INSERT INTO zmwItemBin (" +
    //                    $"Guid" +
    //                    $",ItemCode" +
    //                    $",Quantity" +
    //                    $",BinCode" +
    //                    $",BinAbsEntry" +
    //                    $",BatchNumber" +
    //                    $",SerialNumber" +
    //                    $",TransType" +
    //                    $",TransDateTime " +
    //                    $",BatchAttr1 " +
    //                    $",BatchAttr2 " +
    //                    $",BatchAdmissionDate " +
    //                    $",BatchExpiredDate " +
    //                    $")VALUES(" +
    //                    $"@Guid" +
    //                    $",@ItemCode" +
    //                    $",@Quantity" +
    //                    $",@BinCode" +
    //                    $",@BinAbsEntry" +
    //                    $",@BatchNumber" +
    //                    $",@SerialNumber" +
    //                    $",@TransType" +
    //                    $",@TransDateTime" +
    //                    $",@BatchAttr1 " +
    //                    $",@BatchAttr2 " +
    //                    $",@BatchAdmissionDate " +
    //                    $",@BatchExpiredDate " +
    //                    $")";

    //                result = conn.Execute(insertItemBinLine, itemBinLine, trans);
    //                if (result < 0) { Rollback(); return -1; }
    //            }
    //            #endregion

    //            #region insert zmwDocHeaderField
    //            if (docHeaderfield != null)
    //            {
    //                string insertdocHeaderfield =
    //                   $"INSERT INTO {nameof(zmwDocHeaderField)}(" +
    //                   $" Guid " +
    //                   $",DocSeries " +
    //                   $",Ref2 " +
    //                   $",Comments " +
    //                   $",JrnlMemo " +
    //                   $",NumAtCard" +
    //                   $",Series" +
    //                   $") VALUES (" +
    //                   $"@Guid " +
    //                   $",@DocSeries " +
    //                   $",@Ref2 " +
    //                   $",@Comments " +
    //                   $",@JrnlMemo " +
    //                   $",@NumAtCard" +
    //                   $",@Series" +
    //                   $")";

    //                result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
    //                if (result < 0) { Rollback(); return -1; }
    //            }
    //            #endregion 
    //            CommitDatabase();
    //            return result;
    //        }
    //    }
    //    catch (Exception excep)
    //    {
    //        LastErrorMessage = $"{excep}";
    //        Rollback();
    //        return -1;
    //    }
    //}
}
