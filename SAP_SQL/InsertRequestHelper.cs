using Dapper;
using System;
using System.Data.SqlClient;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.Request;

namespace WMSWebAPI.SAP_SQL
{
    public class InsertRequestHelper : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string databaseConnStr_midware { get; set; } = "";
        SqlConnection conn { get; set; }
        SqlTransaction trans { get; set; }

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

        public InsertRequestHelper (string dbConnStr, string dbConnStr_midware = "")
        {
            databaseConnStr = dbConnStr;
            databaseConnStr_midware = dbConnStr_midware;
        }

        void ConnectAndStartTrans()
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
        /// </summary>
        void ConnectAndStartTrans_Midware()
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
        void CommitDatabase()
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
        void Rollback()
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

        // insert the request with GUID
        // insert the OPOR_Ex table
        // insert the POR1_Ex table
        // ask refres the GRPO list for line disapper
        public int CreateRequest(
            zwaRequest dtoRequest,
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

                //ConnectAndStartTrans();

                using (conn)
                using (trans)
                {
                    #region insert request
                    //string insertSql = $"INSERT INTO {nameof(zwaRequest)} (" +
                    //    $"request" +
                    //    $",sapUser " +
                    //    $",sapPassword" +
                    //    $",requestTime" +
                    //    $",phoneRegID" +
                    //    $",status" +
                    //    $",guid" +
                    //    $",sapDocNumber" +
                    //    $",completedTime" +
                    //    $",attachFileCnt" +
                    //    $",tried" +
                    //    $",createSAPUserSysId " +
                    //    $")VALUES(" +
                    //    $"@request" +
                    //    $",@sapUser" +
                    //    $",@sapPassword" +
                    //    $",GETDATE()" +
                    //    $",@phoneRegID" +
                    //    $",@status" +
                    //    $",@guid" +
                    //    $",@sapDocNumber" +
                    //    $",GETDATE()" +
                    //    $",@attachFileCnt" +
                    //    $",@tried" +
                    //    $",@createSAPUserSysId)";

                    //var result = conn.Execute(insertSql, dtoRequest, trans);
                    //if (result < 0) { Rollback(); return -1; };
                    //#endregion

                    //#region insert grpo
                    ///// perform insert of all the GRPO item 

                    //string insertGrpo = $"INSERT INTO {nameof(zwaGRPO)} " +
                    //                         $"(Guid" +
                    //                         $",ItemCode" +
                    //                         $",Qty" +
                    //                         $",SourceCardCode" +
                    //                         $",SourceDocNum" +
                    //                         $",SourceDocEntry" +
                    //                         $",SourceDocBaseType" +
                    //                         $",SourceBaseEntry" +
                    //                         $",SourceBaseLine" +
                    //                         $",Warehouse" +
                    //                         $",SourceDocType" +
                    //                         $",LineGuid" +
                    //                         $") VALUES (" +
                    //                         $"@Guid" +
                    //                         $",@ItemCode" +
                    //                         $",@Qty" +
                    //                         $",@SourceCardCode" +
                    //                         $",@SourceDocNum" +
                    //                         $",@SourceDocEntry" +
                    //                         $",@SourceDocBaseType" +
                    //                         $",@SourceBaseEntry" +
                    //                         $",@SourceBaseLine " +
                    //                         $",@Warehouse" +
                    //                         $",@SourceDocType" +
                    //                         $",@LineGuid" +
                    //                         $")";

                    //result = conn.Execute(insertGrpo, grpoLines, trans);
                    //if (result < 0) { Rollback(); return -1; };
                    //#endregion

                    //#region insert zwaItemBin
                    //// add in the bin lines transaction 
                    //if (itemBinLine != null && itemBinLine.Length > 0)
                    //{
                    //    // add in the bin lines transaction 
                    //    string insertItemBinLine =
                    //        $"INSERT INTO {nameof(zwaItemBin)}(" +
                    //        $"Guid" +
                    //        $",ItemCode" +
                    //        $",Quantity" +
                    //        $",BinCode" +
                    //        $",BinAbsEntry" +
                    //        $",BatchNumber" +
                    //        $",SerialNumber" +
                    //        $",TransType" +
                    //        $",TransDateTime " +
                    //        $",BatchAttr1 " +
                    //        $",BatchAttr2 " +
                    //        $",BatchAdmissionDate " +
                    //        $",BatchExpiredDate " +
                    //        $",LineGuid " +
                    //        $")VALUES(" +
                    //        $"@Guid" +
                    //        $",@ItemCode" +
                    //        $",@Quantity" +
                    //        $",@BinCode" +
                    //        $",@BinAbsEntry" +
                    //        $",@BatchNumber" +
                    //        $",@SerialNumber" +
                    //        $",@TransType" +
                    //        $",@TransDateTime" +
                    //        $",@BatchAttr1 " +
                    //        $",@BatchAttr2 " +
                    //        $",@BatchAdmissionDate " +
                    //        $",@BatchExpiredDate " +
                    //        $",@LineGuid " +
                    //        $")";

                    //    result = conn.Execute(insertItemBinLine, itemBinLine, trans);
                    //    if (result < 0) { Rollback(); return -1; }
                    //}
                    //#endregion

                    //// insert the doc header field
                    //#region insert zmwDocHeaderField
                    //if (docHeaderfield != null)
                    //{
                    //    string insertdocHeaderfield =
                    //      $"INSERT INTO {nameof(zmwDocHeaderField)}(" +
                    //      $" Guid " +
                    //      $",DocSeries " +
                    //      $",Ref2 " +
                    //      $",Comments " +
                    //      $",JrnlMemo " +
                    //      $",NumAtCard" +
                    //      $",Series" +
                    //      $") VALUES (" +
                    //      $"@Guid " +
                    //      $",@DocSeries " +
                    //      $",@Ref2 " +
                    //      $",@Comments " +
                    //      $",@JrnlMemo " +
                    //      $",@NumAtCard" +
                    //      $",@Series" +
                    //      $")";

                    //    result = conn.Execute(insertdocHeaderfield, docHeaderfield, trans);
                    //    if (result < 0) { Rollback(); return -1; }
                    //}

                    #endregion

                    //CommitDatabase();
                    return CreateRequest_Midware(dtoRequest, grpoLines, itemBinLine, docHeaderfield);
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                Rollback();
                return -1;
            }           
        }
        // <summary>
        /// Create GRPO Request
        /// </summary>
        int CreateRequest_Midware(
            zwaRequest dtoRequest,
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

                ConnectAndStartTrans_Midware();

                using (conn)
                using (trans)
                {
                    #region insert request
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

                    #region insert grpo
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
                                             $",Remarks" + // <--- for addin line remark
                                             $",ReasonCode" +     // <--- for reason code
                                             $",ReasonName" + // <--- for reason name
                                             $",AcctCode" +     // <--- for reason code
                                             $",AcctName" + // <--- for reason name
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
                                             $",@Remarks" + // <--- for addin line remark
                                             $",@ReasonCode" +     // <--- for reason code
                                             $",@ReasonName" + // <--- for reason name
                                             $",@AcctCode" +     // <--- for reason code
                                             $",@AcctName" + // <--- for reason name
                                             $")";

                    result = conn.Execute(insertGrpo, grpoLines, trans);
                    if (result < 0) { Rollback(); return -1; };
                    #endregion

                    #region insert zwaItemBin
                    // add in the bin lines transaction 
                    if (itemBinLine != null && itemBinLine.Length > 0)
                    {
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

                    // insert the doc header field
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
                         $",NumAtCard " +
                         $",Series " +
                         $",GIReasonCode " +
                         $",GIReasonName " +
                         $") VALUES (" +
                         $"@Guid " +
                         $",@DocSeries " +
                         $",@Ref2 " +
                         $",@Comments " +
                         $",@JrnlMemo " +
                         $",@NumAtCard" +
                         $",@Series " +
                         $",@GIReasonCode " +
                         $",@GIReasonName" +
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
    }
}
