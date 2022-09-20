using Dapper;
using DbClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.Models.Demo;
using WMSWebAPI.Models.Demo.Transfer1;
using WMSWebAPI.Models.GRPO;
using WMSWebAPI.Models.Request;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_DocStatus : IDisposable
    {
        string databaseConnStr { get; set; } = string.Empty;

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbConnString"></param>
        public SQL_DocStatus(string dbConnStr) => databaseConnStr = dbConnStr;

        /// <summary>
        /// Check the doc request status with guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public zmwRequest GetRequestStatus(string guid)
        {
            try
            {
                string query = $"SELECT * FROM {nameof(zmwRequest)} WHERE guid = @guid Order By Id Desc";
                var result = new SqlConnection(databaseConnStr).Query<zmwRequest>(query, new { guid }).FirstOrDefault();
                if (result != null && !result.status.ToLower().Equals("success"))
                {
                    result.tried++;
                    result.IsNotify = 1;
                    UpdateRequestNotification(guid);
                }
                return result;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message}\n{e.StackTrace}";
                return null;
            }
        }

        public DTO_Request GetRequestDetails(string guid, string sapDBConnStr)
        {
            try
            {
                var result = new DTO_Request();
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    result.Request = conn.Query<zwaRequest>("Select * from zmwRequest where guid = @guid", new { guid }).FirstOrDefault();
                    result.Grpo = conn.Query<zwaGRPO>("Select * from zmwGRPO where guid = @guid", new { guid }).ToArray();
                    result.ItemBin = conn.Query<zwaItemBin>("Select * from zmwItemBin where guid = @guid", new { guid }).ToArray();
                    result.DocHeaderField = conn.Query<zmwDocHeaderField>("Select * from zmwDocHeaderField where guid = @guid", new { guid }).FirstOrDefault();
                    result.TransferDocDetails = conn.Query<zwaTransferDocDetails>("Select * from zmwTransferDocDetails where guid = @guid", new { guid }).ToArray();
                    result.TransferDocDetailsBin = conn.Query<zwaTransferDocDetailsBin>("Select * from zmwTransferDocDetailsBin where guid = @guid", new { guid }).ToArray();
                    result.TransferDocHeader = conn.Query<zwaTransferDocHeader>("Select * from zmwTransferDocHeader where guid = @guid", new { guid }).FirstOrDefault();
                    result.HoldRequest = conn.Query<zwaHoldRequest>("Select * from zmwHoldRequest where HeadGuid = @guid", new { guid }).FirstOrDefault();
                    result.InventoryRequest = conn.Query<zwaInventoryRequest>("Select * from zmwInventoryRequest where guid = @guid", new { guid }).FirstOrDefault();
                    result.InventoryRequestHead = conn.Query<zwaInventoryRequestHead>("Select * from zmwInventoryRequestHead where guid = @guid", new { guid }).FirstOrDefault();
                }

                // get the bp object for this doc 
                // prepare for the needed used
                if (result.Grpo == null) return result;

                string[] bpCode = result.Grpo
                    .Where(x => x.SourceCardCode != null)
                    .Select(x=>x.SourceCardCode).Distinct().ToArray();

                if (bpCode == null) return result;
                if (bpCode.Length == 0) return result;

                using (var conn = new SqlConnection(sapDBConnStr))
                {
                    var bpList = new List<OCRD>();
                    for (int x = 0; x < bpCode.Length; x++)
                    {
                        var bp = conn.Query("SELECT * FROM OCRD WHERE CardCode=@CardCode", new { CardCode = bpCode[x] }).FirstOrDefault();
                        if (bp == null) continue;
                        bpList.Add(bp);
                    }
                    result.Bp = bpList.ToArray();
                }
                return result;
            }
            catch (Exception e)
            {
                LastErrorMessage = $"{e.Message}\n{e.StackTrace}";
                return null;
            }
        }

        public int ResetRequestTried(string guid)
        {
            try
            {
                return new SqlConnection(databaseConnStr)
                    .Execute($"UPDATE {nameof(zmwRequest)} SET tried = 0 WHERE guid=@guid",
                    new { guid });
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        ///  update the request is notified
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        int UpdateRequestNotification(string guid)
        {
            try
            {
                return new SqlConnection(databaseConnStr)
                    .Execute($"UPDATE {nameof(zmwRequest)} SET IsNotify = 1 WHERE guid=@guid",
                    new { guid });
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

    }
}
