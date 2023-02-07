using DbClass;
using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Class;
using System.Transactions;
using WMSWebAPI.Models.Demo.Transfer1;
using WMSWebAPI.Models.GRPO;
using System.Data;
using WMSWebAPI.Models.Lifewater;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_QueryCode : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        string databaseConnStr_MidWare { get; set; } = "";
        SqlConnection conn;
        SqlTransaction trans;

        string MidwareDbName { get; set; } = "";

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbConnStr"></param>
        public SQL_QueryCode(string dbConnStr, string midWareDbConnStr, string midwareDbName)
        {
            databaseConnStr = dbConnStr;
            databaseConnStr_MidWare = midWareDbConnStr;
            MidwareDbName = midwareDbName;
        }

        /// <summary>
        /// Get list of GRPO doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '67'").ToArray();
        }

        public OPLN_Ex[] GetTransferPriceList()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<OPLN_Ex>("SELECT T0.ListNum, T0.ListName, T1.U_Module, T1.U_Value FROM OPLN T0 INNER JOIN [@APPPRICELIST] T1 ON T0.ListName = T1.U_Value WHERE U_Module = 'OWTR'; ").ToArray();
        }

        /// <summary>
        /// Return a batch object if found by the code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public OBTN IsBatchNum(string code)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OBTN>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOBTN] " +
                    $"WHERE DistNumber = @code ",
                    new { code }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Return a serial object if found by the code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public OSRN IsSerialNum(string code)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OSRN>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOSRN] " +
                    $"WHERE DistNumber = @code ", new { code }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public OBIN IsBinCode(string code)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OBIN>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOBIN] " +
                    $"WHERE BinCode = @code ", new { code }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Get or check the code is ItemCode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public OITM IsItemCode(string code)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OITM>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOITM] " +
                    $"WHERE ItemCode = @code ", new { code }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// get the item code, serial, whs and bin link
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="serialNum"></param>
        /// <param name="whs"></param>
        /// <returns></returns>
        public OSBQ GetBinSerialAccumulator(string itemCode, int binAbs, string whsCode, string serialNum)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OSBQ>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOSBQ] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND BinAbs= @binAbs " +
                    $"AND WhsCode = @whsCode " +
                    $"AND DistNumber= @serialNum", new { itemCode, binAbs, whsCode, serialNum }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// get list of bin with item Qty
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="whsCode"></param>
        /// <returns></returns>
        public OIBQ_Ex[] GetItemWhsBins(string itemCode, string whsCode)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OIBQ_Ex>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOIBQs] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND WhsCode = @whsCode ", new { itemCode, whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Get list of wahs content batch (no bin)
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="whsCode"></param>
        /// <returns></returns>
        public OBTQ_Ex[] GetBatchInWhs(string itemCode, string whsCode)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OBTQ_Ex>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOBTQs] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND WhsCode = @whsCode ", new { itemCode, whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Query get the list of available serial # from the warehouse
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="whsCode"></param>
        /// <returns></returns>
        public OSRQ_Ex[] GetSerialInWhs(string itemCode, string whsCode)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OSRQ_Ex>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOSRQs] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND WhsCode = @whsCode ", new { itemCode, whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// get the line of the hold request, share code with SAT
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public zwaTransferDocDetails[] GetPickedRequestLines(Guid headGuid)
        {
            try
            {   
                return new SqlConnection(databaseConnStr_MidWare).Query<zwaTransferDocDetails>(
                   $"select * from [zmwTransferDocDetails] " +
                   $"where Guid = @headGuid", new { headGuid }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        ///  share code with SAT
        /// </summary>
        /// <param name="headGuid"></param>
        /// <returns></returns>
        public zwaTransferDocDetailsBin[] GetPickedRequestLinesBins(Guid headGuid)
        {
            try
            {
                // get open line from the request
                return new SqlConnection(databaseConnStr_MidWare).Query<zwaTransferDocDetailsBin>(
                    $"select * from zmwTransferDocDetailsBin " +
                    $"where Guid = @headGuid", new { headGuid }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get List of bin and content serial #
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="binAbs"></param>
        /// <param name="whsCode"></param>
        /// <param name="serialNum"></param>
        /// <returns></returns>
        public OSBQ_Ex[] GetBinSerialAccumulators(string itemCode, string whsCode)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OSBQ_Ex>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOSBQs] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND WhsCode = @whsCode ", new { itemCode, whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// get the item code, batch, whs and bin link
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="batchNum"></param>
        /// <param name="whs"></param>
        /// <returns></returns>
        public OBBQ GetBinBatchAccumulator(string itemCode, int binAbs, string whsCode, string batchNum)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OBBQ>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOBBQ] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND BinAbs= @binAbs " +
                    $"AND WhsCode = @whsCode " +
                    $"AND DistNumber= @serialNum", new { itemCode, binAbs, whsCode, batchNum }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Get list of bin + batch lcoation with item and warehouse
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="binAbs"></param>
        /// <param name="whsCode"></param>
        /// <param name="batchNum"></param>
        /// <returns></returns>
        public OBBQ_Ex[] GetBinBatchAccumulators(string itemCode, string whsCode)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OBBQ_Ex>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOBBQs] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND WhsCode = @whsCode ", new { itemCode, whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// get the item code, batch, whs and bin link
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="batchNum"></param>
        /// <param name="whs"></param>
        /// <returns></returns>
        public OIBQ GetItemBinAccumulator(string itemCode, int binAbs, string whsCode)
        {
            try
            {
                // select * from FTS_vw_IMApp_TransferOIBQ where ItemCode = '' and BinAbs = '' and WhsCode = ''

                return new SqlConnection(databaseConnStr).Query<OIBQ>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOIBQ] " +
                    $"WHERE ItemCode = @itemCode " +
                    $"AND BinAbs= @binAbs " +
                    $"AND WhsCode = @whsCode ", new { itemCode, binAbs, whsCode }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// To verify the serial exit in the warehouse 
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        /// <param name="IntrSerial"></param>
        /// <returns></returns>
        public OSRI CheckSerialWhs(string ItemCode, string WhsCode, string IntrSerial)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OSRI>(
                    $"SELECT * " +
                    $"FROM [FTS_vw_IMApp_TransferOSRI] " +
                    $"WHERE ItemCode = @ItemCode " +
                    $"AND WhsCode = @WhsCode " +
                    $"AND IntrSerial = @IntrSerial " +
                    $"AND Status = 0", new { ItemCode, WhsCode, IntrSerial }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Check batch whs qty, to prevent user to scan in wrong batch
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        /// <param name="AbsEntry"></param>
        /// <returns></returns>
        public OBTQ CheckBatchWhs(string ItemCode, string WhsCode, int AbsEntry)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<OBTQ>(
                    "SELECT * FROM [FTS_vw_IMApp_TransferOBTQ] " +
                    "WHERE ItemCode  = @ItemCode " +
                    "AND WhsCode = WhsCode " +
                    "AND AbsEntry = @AbsEntry", new { ItemCode, WhsCode, AbsEntry }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }
        
        /// <summary>
        /// Create transfer for mid ware 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int CreateTransfer_Midware(Cio bag)
        {
            try
            {
                int result = -1;
                using (var transactionScope = new TransactionScope())
                using (var conn = new SqlConnection(databaseConnStr_MidWare))
                {
                    #region Clear the middleware bins table                    
                    result = conn.Execute($"DELETE FROM zmwTransferDocDetailsBin WHERE Guid = @Guid", new { bag.dtoRequest.guid });
                    if (result <= 0)
                    {
                        LastErrorMessage = "Server replied: update the line / bin details table, please try again later. Thanks [MW]";
                        return -1;
                    }
                    #endregion

                    #region Clear the middleware details table                    
                    result = conn.Execute($"DELETE FROM zmwTransferDocDetails WHERE Guid = @Guid", new { bag.dtoRequest.guid });
                    if (result <= 0)
                    {
                        LastErrorMessage = "Server replied: update the line / bin details table, please try again later. Thanks [MW]";
                        return -1;
                    }
                    #endregion

                    // insert the request
                    // into the to warehouse line
                    //bag.dtoRequest // for middleware
                    //bag.TransferDocDetailsBins // for middleware
                    #region
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
                        $",@createSAPUserSysId )";

                    result = conn.Execute(insertSql, bag.dtoRequest);
                    if (result <= 0) // if no row or negative row return
                    {
                        LastErrorMessage = "Server replied: Error insert the trasfer request, please try again later. Thanks";
                        return -1;
                    }
                    #endregion

                    #region insert the details table again
                    var insert_zwaTransferDocDetails = $"INSERT INTO zmwTransferDocDetails (" +
                       $"[Guid] " +
                       $",[LineGuid] " +
                       $",[ItemCode] " +
                       $",[ItemName] " +
                       $",[Qty] " +
                       $",[FromWhsCode] " +
                       $",[ToWhsCode] " +
                       $",[Serial] " +
                       $",[Batch] " +
                       $",[SourceDocBaseType] " +
                       $",[SourceBaseEntry] " +
                       $",[SourceBaseLine] " +
                       $",[ActualReceiptQty] " +
                       $") VALUES ( " +
                       $" @Guid " +
                       $",@LineGuid " +
                       $",@ItemCode " +
                       $",@ItemName " +
                       $",@Qty " +
                       $",@FromWhsCode " +
                       $",@ToWhsCode " +
                       $",@Serial " +
                       $",@Batch " +
                       $",@SourceDocBaseType " +
                       $",@SourceBaseEntry " +
                       $",@SourceBaseLine " +
                       $",@ActualReceiptQty " +
                       $")";

                    result = conn.Execute(insert_zwaTransferDocDetails, bag.TransferDocDetails);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Details";
                        return -1;
                    }
                    #endregion

                    #region insert the bin line 
                    var insert = $"INSERT INTO zmwTransferDocDetailsBin (" +
                        $"[Guid] " +
                        $",[LineGuid] " +
                        $",[ItemCode] " +
                        $",[ItemName] " +
                        $",[Qty] " +
                        $",[Serial] " +
                        $",[Batch] " +
                        $",[InternalSerialNumber] " +
                        $",[ManufacturerSerialNumber] " +
                        $",[BinAbs] " +
                        $",[BinCode] " +
                        $",[SnBMDAbs] " +
                        $",[WhsCode] " +
                        $",[Direction] " +
                        $") VALUES (" +
                         $"@Guid" +
                        $",@LineGuid " +
                        $",@ItemCode " +
                        $",@ItemName " +
                        $",@Qty " +
                        $",@Serial " +
                        $",@Batch " +
                        $",@InternalSerialNumber " +
                        $",@ManufacturerSerialNumber " +
                        $",@BinAbs " +
                        $",@BinCode " +
                        $",@SnBMDAbs " +
                        $",@WhsCode " +
                        $",@Direction " +
                        $")";

                    result = conn.Execute(insert, bag.TransferDocDetailsBins);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Details bin info";
                        return -1;
                    }
                    #endregion

                    #region Update the Header
                    result = conn.Execute("sp_Transfer1_UpdatezmwTransferDocHeader",
                        new
                        {
                            Guid = bag.dtoRequest.guid,
                            Series = bag.dtozmwDocHeaderField.Series,
                            JrnlMemo = bag.dtozmwDocHeaderField.JrnlMemo,
                            PriceList = bag.dtozmwDocHeaderField.PriceList,
                            Comments = bag.dtozmwDocHeaderField.Comments
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail Update TransferDocHeader";
                        return -1;
                    }
                    #endregion

                    // insert the doc heade details 
                    #region insert zmwDocHeaderField
                    //if (bag.dtozmwDocHeaderField != null)
                    //{
                    //    string insertdocHeaderfield =
                    //            $"INSERT INTO {nameof(zmwDocHeaderField)}(" +
                    //            $" Guid " +
                    //            $",DocSeries " +
                    //            $",Ref2 " +
                    //            $",Comments " +
                    //            $",JrnlMemo " +
                    //            $",NumAtCard" +
                    //            $",Series" +
                    //            $") VALUES (" +
                    //            $"@Guid " +
                    //            $",@DocSeries " +
                    //            $",@Ref2 " +
                    //            $",@Comments " +
                    //            $",@JrnlMemo " +
                    //            $",@NumAtCard" +
                    //            $",@Series" +
                    //            $")";

                    //    result = conn.Execute(insertdocHeaderfield, bag.dtozmwDocHeaderField);
                    //    if (result <= 0)
                    //    {
                    //        LastErrorMessage = "Fail insert the transfer doc header fields info";
                    //        return -1;
                    //    }
                    //}

                    #endregion

                    transactionScope.Complete();
                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }


        /// <summary>
        /// Save the 
        /// Stand Alone Transfer On Hold
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int SaveStandAloneTransferOnHold(Cio bag)
        {
            try
            {
                // save and create a onhold entry
                // save the doc header
                // save the line 
                // save line from table
                int result = -1;
                if (bag.TransferHoldRequest == null)
                {
                    LastErrorMessage = "Empty on hold request, please try again. Thanks";
                    return -1;
                }

                // all all transfer first
                result = ResetOnHoldDoc_Midware(bag);
                if (result == -1)
                {
                    LastErrorMessage += "Error remove the last on hold request, " +
                         "please contact support for help. Thanks [ref: 1]"; // 1" + "\n" + databaseConnStr_MidWare;
                    return result;
                }

                SaveStandAloneTransferOnHold_Midware(bag);
                return 1;
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }
        /// <summary>
        /// SaveStandAloneTransferOnHold_Midware
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>

        public int SaveStandAloneTransferOnHold_Midware(Cio bag)
        {
            try
            {
                // save and create a onhold entry
                // save the doc header
                // save the line 
                // save line from table
                int result = -1;
                if (bag.TransferHoldRequest == null)
                {
                    LastErrorMessage = "Empty on hold request, please try again. Thanks";
                    return -1;
                }

                using (var transactionScope = new TransactionScope())
                using (var conn = new SqlConnection(databaseConnStr_MidWare))
                {
                    #region insert the on hold request doc
                    string insert = $"INSERT INTO zmwHoldRequest (" +
                                            $" [DocEntry] " +
                                            $",[DocNum]" +
                                            $",[Picker]" +
                                            $",[TransDate]" +
                                            $",[HeadGuid]" +
                                            $",[Status] " +
                                            $") VALUES ( " +
                                            $" @DocEntry " +
                                            $",@DocNum " +
                                            $",@Picker" +
                                            $",@TransDate" +
                                            $",@HeadGuid" +
                                            $",@Status" +
                                            $"); SELECT CAST(SCOPE_IDENTITY() as int);";

                    result = (int)conn.ExecuteScalar(insert, bag.TransferHoldRequest);

                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the OnHold Request";
                        return -1;
                    }

                    // update the insert ID 
                    bag.STAHoldRequestId = result;

                    #endregion

                    #region insert the header

                    insert = $"INSERT INTO zmwTransferDocHeader (" +
                        $"[DocDate] " +
                        $",[TaxDate] " +
                        $",[FromWhsCode] " +
                        $",[ToWhsCode] " +
                        $",[JrnlMemo] " +
                        $",[Comments] " +
                        $",[Guid] " +
                        $",[DocNumber] " +
                        $",[DocStatus] " +
                        $",[LastErrorMessage]" +
                        $") VALUES ( " +
                        $"@DocDate " +
                        $",@TaxDate " +
                        $",@FromWhsCode" +
                        $",@ToWhsCode " +
                        $",@JrnlMemo " +
                        $",@Comments " +
                        $",@Guid " +
                        $",@DocNumber " +
                        $",@DocStatus " +
                        $",@LastErrorMessage)";

                    result = conn.Execute(insert, bag.TransferDocHeader);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Header";
                        return -1;
                    }
                    #endregion

                    #region insert the transfer line 
                    insert = $"INSERT INTO zmwTransferDocDetails (" +
                        $"[Guid] " +
                        $",[LineGuid] " +
                        $",[ItemCode] " +
                        $",[ItemName] " +
                        $",[Qty] " +
                        $",[FromWhsCode] " +
                        $",[ToWhsCode] " +
                        $",[Serial] " +
                        $",[Batch] " +
                        $",[SourceDocBaseType] " +
                        $",[SourceBaseEntry] " +
                        $",[SourceBaseLine] " +
                        $") VALUES ( " +
                        $" @Guid " +
                        $",@LineGuid " +
                        $",@ItemCode " +
                        $",@ItemName " +
                        $",@Qty " +
                        $",@FromWhsCode " +
                        $",@ToWhsCode " +
                        $",@Serial " +
                        $",@Batch " +
                        $",@SourceDocBaseType " +
                        $",@SourceBaseEntry " +
                        $",@SourceBaseLine " +
                        $")";

                    result = conn.Execute(insert, bag.TransferDocDetails);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Details";
                        return -1;
                    }
                    #endregion

                    #region insert the bin line 
                    insert = $"INSERT INTO zmwTransferDocDetailsBin (" +
                        $"[Guid] " +
                        $",[LineGuid] " +
                        $",[ItemCode] " +
                        $",[ItemName] " +
                        $",[Qty] " +
                        $",[Serial] " +
                        $",[Batch] " +
                        $",[InternalSerialNumber] " +
                        $",[ManufacturerSerialNumber] " +
                        $",[BinAbs] " +
                        $",[BinCode] " +
                        $",[SnBMDAbs] " +
                        $",[WhsCode] " +
                        $",[Direction] " +
                        $") VALUES (" +
                        $" @Guid " +
                        $",@LineGuid " +
                        $",@ItemCode " +
                        $",@ItemName " +
                        $",@Qty " +
                        $",@Serial " +
                        $",@Batch " +
                        $",@InternalSerialNumber " +
                        $",@ManufacturerSerialNumber " +
                        $",@BinAbs " +
                        $",@BinCode " +
                        $",@SnBMDAbs " +
                        $",@WhsCode " +
                        $",@Direction " +
                        $")";

                    result = conn.Execute(insert, bag.TransferDocDetailsBins);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Details bin info";
                        return -1;
                    }
                    #endregion

                    // complete the transaction
                    transactionScope.Complete();
                }
                return 1;
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        /// Save the 
        /// </summary>
        /// <returns></returns>
        public int SaveRequestOnHold(Cio bag)
        {
            try
            {
                // save and create a onhold entry
                // save the doc header
                // save the line 
                // save line from table
                int result = -1;
                if (bag.TransferHoldRequest == null)
                {
                    LastErrorMessage = "Empty on hold request, please try again. Thanks";
                    return -1;
                }

                // remove all transfer first
                result = ResetOnHoldDoc_Midware(bag);
                if (result == -1)
                {
                    LastErrorMessage += "Error remove the last on hold request, " +
                         "please contact support for help. Thanks [ref: 2]";// + "\n" + databaseConnStr_MidWare;
                    return result;
                }
             
                SaveRequestOnHold_Midware(bag); // save a copy into the mid ware db

                return 1;
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        /// Save same copy to midelware
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public int SaveRequestOnHold_Midware(Cio bag)
        {
            try
            {
                // save and create a onhold entry
                // save the doc header
                // save the line 
                // save line from table
                int result = -1;
                if (bag.TransferHoldRequest == null)
                {
                    LastErrorMessage = "Empty on hold request, please try again. Thanks";
                    return -1;
                }
                
                using (var transactionScope = new TransactionScope())
                using (var conn = new SqlConnection(databaseConnStr_MidWare))
                {
                    #region insert the on hold request doc
                    string insert = $"INSERT INTO zmwHoldRequest (" +
                                            $" [DocEntry] " +
                                            $",[DocNum]" +
                                            $",[Picker]" +
                                            $",[TransDate]" +
                                            $",[HeadGuid]" +
                                            $",[Status] " +
                                            $") VALUES ( " +
                                            $" @DocEntry " +
                                            $",@DocNum " +
                                            $",@Picker" +
                                            $",@TransDate" +
                                            $",@HeadGuid" +
                                            $",@Status" +
                                            $")";
                    result = conn.Execute(insert, bag.TransferHoldRequest);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the OnHold Request";
                        return -1;
                    }
                    #endregion

                    #region insert the header

                    insert = $"INSERT INTO zmwTransferDocHeader (" +
                        $"[DocDate] " +
                        $",[TaxDate] " +
                        $",[FromWhsCode] " +
                        $",[ToWhsCode] " +
                        $",[JrnlMemo] " +
                        $",[Comments] " +
                        $",[Guid] " +
                        $",[DocNumber] " +
                        $",[DocStatus] " +
                        $",[LastErrorMessage]" +
                        $") VALUES ( " +
                        $"@DocDate " +
                        $",@TaxDate " +
                        $",@FromWhsCode" +
                        $",@ToWhsCode " +
                        $",@JrnlMemo " +
                        $",@Comments " +
                        $",@Guid " +
                        $",@DocNumber " +
                        $",@DocStatus " +
                        $",@LastErrorMessage)";

                    result = conn.Execute(insert, bag.TransferDocHeader);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Header";
                        return -1;
                    }
                    #endregion

                    #region insert the transfer line 
                    insert = $"INSERT INTO zmwTransferDocDetails (" +
                        $"[Guid] " +
                        $",[LineGuid] " +
                        $",[ItemCode] " +
                        $",[ItemName] " +
                        $",[Qty] " +
                        $",[FromWhsCode] " +
                        $",[ToWhsCode] " +
                        $",[Serial] " +
                        $",[Batch] " +
                        $",[SourceDocBaseType] " +
                        $",[SourceBaseEntry] " +
                        $",[SourceBaseLine] " +
                        $",[ActualReceiptQty] " +
                        $") VALUES ( " +
                        $" @Guid " +
                        $",@LineGuid " +
                        $",@ItemCode " +
                        $",@ItemName " +
                        $",@Qty " +
                        $",@FromWhsCode " +
                        $",@ToWhsCode " +
                        $",@Serial " +
                        $",@Batch " +
                        $",@SourceDocBaseType " +
                        $",@SourceBaseEntry " +
                        $",@SourceBaseLine " +
                        $",@ActualReceiptQty " +
                        $")";

                    result = conn.Execute(insert, bag.TransferDocDetails);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Details";
                        return -1;
                    }
                    #endregion

                    #region insert the bin line 
                    insert = $"INSERT INTO zmwTransferDocDetailsBin (" +
                        $"[Guid] " +
                        $",[LineGuid] " +
                        $",[ItemCode] " +
                        $",[ItemName] " +
                        $",[Qty] " +
                        $",[Serial] " +
                        $",[Batch] " +
                        $",[InternalSerialNumber] " +
                        $",[ManufacturerSerialNumber] " +
                        $",[BinAbs] " +
                        $",[BinCode] " +
                        $",[SnBMDAbs] " +
                        $",[WhsCode] " +
                        $",[Direction] " +
                        $") VALUES (" +
                        $" @Guid " +
                        $",@LineGuid " +
                        $",@ItemCode " +
                        $",@ItemName " +
                        $",@Qty " +
                        $",@Serial " +
                        $",@Batch " +
                        $",@InternalSerialNumber " +
                        $",@ManufacturerSerialNumber " +
                        $",@BinAbs " +
                        $",@BinCode " +
                        $",@SnBMDAbs " +
                        $",@WhsCode " +
                        $",@Direction " +
                        $")";

                    result = conn.Execute(insert, bag.TransferDocDetailsBins);
                    if (result <= 0)
                    {
                        LastErrorMessage = "Fail insert the Transfer Doc Details bin info";
                        return -1;
                    }
                    #endregion

                    // complete the transaction
                    transactionScope.Complete();
                }

                return 1;
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }
   
        /// <summary>
        /// Middle ware
        /// </summary>
        /// <param name="Bag"></param>
        /// <returns></returns>
        int ResetOnHoldDoc_Midware(Cio Bag)
        {
            try
            {
                // need to delete the mid ware database record
                var sp_query = "EXEC sp_ResetOnHoldTransferDoc @guid";
                return new SqlConnection(databaseConnStr_MidWare).Execute(sp_query, new { guid = Bag.TransferDocHeader.Guid });

                //using (var conn = new SqlConnection(databaseConnStr_MidWare))
                //{



                //    string deleteQuery = "DELETE zmwHoldRequest WHERE DocEntry = @DocEntry";
                //    result = conn.Execute(deleteQuery, new { Bag.TransferHoldRequest.DocEntry });

                //    deleteQuery = "DELETE zmwTransferDocHeader WHERE Guid = @Guid";
                //    result = conn.Execute(deleteQuery, new { Bag.TransferDocHeader.Guid });

                //    var GuidVal = Bag.TransferDocHeader.Guid;
                //    deleteQuery = "DELETE zmwTransferDocDetails WHERE Guid = @GuidVal";
                //    result = conn.Execute(deleteQuery, new { GuidVal });

                //    deleteQuery = "DELETE zmwTransferDocDetailsBin WHERE Guid = @GuidVal";
                //    result = conn.Execute(deleteQuery, new { GuidVal });                    
                //    return result;
                //}
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        /// Based on doc entry
        /// </summary>
        /// <returns></returns>
        public zwaTransferDocDetails GetTransferRequesFromGuids(Cio bag)
        {
            try
            {
                string query = "SELECT * " +
                    "FROM zmwTransferDocDetails " +
                    "WHERE SourceBaseEntry = @TransferRequestDocEntry " +
                    "AND SourceBaseLine = @TransferDocRequestBaseLine " +
                    "ORDER BY Id desc";

                return new SqlConnection(databaseConnStr_MidWare)
                        .Query<zwaTransferDocDetails>(query,
                        new
                        {
                            bag.TransferRequestDocEntry,
                            bag.TransferDocRequestBaseLine
                        }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        ///  Get list of serial based on docEntry
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public zwaTransferDocDetailsBin[] GetSerialsList(Cio bag)
        {
            try
            {
                string query =
                        $"SELECT * " +
                        $"FROM [zmwTransferDocDetailsBin] " +
                        $"WHERE Guid = @TransferDocRequestGuid " +
                        $"AND LineGuid = @TransferDocRequestGuidLine ";

                return new SqlConnection(databaseConnStr_MidWare)
                        .Query<zwaTransferDocDetailsBin>(query,
                        new
                        {
                            bag.TransferDocRequestGuid,
                            bag.TransferDocRequestGuidLine
                        }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        ///  Get list of item based on docEntry
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public zwaTransferDocDetailsBin[] GetItemList(Cio bag)
        {
            try
            {
                string query =
                        $"SELECT * " +
                        $"FROM zmwTransferDocDetailsBin " +
                        $"WHERE Guid = @TransferDocRequestGuid " +
                        $"AND LineGuid = @TransferDocRequestGuidLine ";

                return new SqlConnection(databaseConnStr_MidWare)
                        .Query<zwaTransferDocDetailsBin>(query,
                        new
                        {
                            bag.TransferDocRequestGuid,
                            bag.TransferDocRequestGuidLine
                        }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Get list of batch based on docEntry
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public zwaTransferDocDetailsBin[] GetBatchesList(Cio bag)
        {
            try
            {
                string query1 =
                   $"SELECT * " +
                   $"FROM zmwTransferDocDetailsBin " +
                   $"WHERE Guid = @TransferDocRequestGuid " +
                   $"AND LineGuid = @TransferDocRequestGuidLine ";

                return new SqlConnection(databaseConnStr_MidWare)
                    .Query<zwaTransferDocDetailsBin>(query1, new
                    {
                        bag.TransferDocRequestGuid,
                        bag.TransferDocRequestGuidLine
                    }).ToArray();

            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// GetBatchsList
        /// Get list of batch based on docEntry
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public zwaTransferDocDetailsBin[] GetBatchsList(Cio bag)
        {
            try
            {
                string query1 =
                   $"SELECT * " +
                   $"FROM zmwTransferDocDetailsBin " +
                   $"WHERE Guid = @TransferDocRequestGuid " +
                   $"AND LineGUid = @TransferDocRequestGuidLine ";

                return new SqlConnection(databaseConnStr_MidWare)
                    .Query<zwaTransferDocDetailsBin>(query1,
                    new
                    {
                        bag.TransferDocRequestGuid,
                        bag.TransferDocRequestGuidLine
                    }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Get list of bin code from whs 
        /// </summary>
        /// <param name="whsCode"></param>
        /// <returns></returns>
        public OBIN[] GetWhsBins(string whsCode)
        {
            try
            {
                string query =
                    $"SELECT * " +
                    $"FROM  FTS_vw_IMApp_TransferOBIN " +
                    $"WHERE WhsCode = @whsCode " +
                    $"AND Disabled ='N'";

                return new SqlConnection(databaseConnStr)
                    .Query<OBIN>(query, new { whsCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Check the item in the spcific warehouse having qty from Transfer
        /// </summary>
        /// <returns></returns>
        public OITW GetWarehouseItemQty(string whsCode, string itemCode)
        {
            try
            {
                string query =
                    $"SELECT * " +
                    $"FROM  [FTS_vw_IMApp_OITW] " +
                    $"WHERE WhsCode = @whsCode " +
                    $"AND ItemCode = @itemCode ";

                return new SqlConnection(databaseConnStr)
                    .Query<OITW>(query, new { whsCode, itemCode }).FirstOrDefault();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public zwaHoldRequest[] LoadSTADocList()
        {
            try
            {
                string query =
                   $"SELECT * " +
                   $"FROM zmwHoldRequest " +
                   $"WHERE DocEntry = -1 AND Status ='O'";

                return new SqlConnection(databaseConnStr_MidWare)
                    .Query<zwaHoldRequest>(query).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        /// <summary>
        /// Load the stand alone transfer line 
        /// the from wareouse 
        /// </summary>
        /// <returns></returns>
        public zwaTransferDocDetails[] LoadSTADocLines(Cio bag)
        {
            try
            {
                string query =
                   $"SELECT * " +
                   $"FROM zmwTransferDocDetails " +
                   $"WHERE Guid = @TransferDocRequestGuid";

                return new SqlConnection(databaseConnStr_MidWare)
                    .Query<zwaTransferDocDetails>(query, new { bag.TransferDocRequestGuid }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return null;
            }
        }

        public int RemoveOnHold(string guid)
        {
            try
            {               
                return new SqlConnection(databaseConnStr_MidWare)
                    .Execute("EXEC sp_ResetOnHoldTransferDoc @guid", new { guid });  
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

    }
}
