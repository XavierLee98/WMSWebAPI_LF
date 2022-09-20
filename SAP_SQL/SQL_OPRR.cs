using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSWebAPI.Class;
using WMSWebAPI.Models;
using WMSWebAPI.Models.GoodsReturnRequest;

namespace WMSWebAPI.SAP_SQL
{
    /// <summary>
    /// Share controller between goods return request and good return
    /// </summary>
    public class SQL_OPRR : IDisposable
    {
        string databaseConnStr { get; set; } = string.Empty;
        public string LastErrorMessage { get; private set; } = string.Empty;
        public void Dispose() => GC.Collect();

        public SQL_OPRR(string dbConnStr) => databaseConnStr = dbConnStr;

        public NNM1[] GetDocSeries() // good return request
        {
            try
            {
                using var conn = new SqlConnection(databaseConnStr);
                return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '234000032'").ToArray();
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message + e.StackTrace;
                return null;
            }
        }

        /// <summary>
        /// Get the GRN line
        /// </summary>
        /// <param name="grnDocEntries"></param>
        /// <returns></returns>
        public PDN1_Ex[] GetGrnLines(int[] grnDocEntries)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);
                var pdn1 = new List<PDN1_Ex>();

                foreach (var docEntry in grnDocEntries)
                {
                    var lines = conn.Query<PDN1_Ex>(
                        "SELECT * FROM [FTS_vw_IMApp_PDN1] WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    pdn1.AddRange(lines);
                }

                return pdn1.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep.Message}\n{excep.StackTrace}";
                return null;
            }
        }

        /// <summary>
        /// return list of the ap invoice line
        /// </summary>
        /// <param name="apInvDocEntries"></param>
        /// <returns></returns>
        public PCH1_Ex[] GetOpenApInvoiceLines(int[] apInvDocEntries)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);
                var pdn1 = new List<PCH1_Ex>();

                foreach (var docEntry in apInvDocEntries)
                {
                    var lines = conn.Query<PCH1_Ex>(
                        "SELECT * FROM [FTS_vw_IMApp_PCH1] WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    pdn1.AddRange(lines);
                }

                return pdn1.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep.Message}\n{excep.StackTrace}";
                return null;
            }
        }

        /// <summary>
        /// Return Grpo based on date range
        /// </summary>
        /// <returns></returns>
        public OPDN_Ex[] GetOpenGrn(Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_OPDN ";

                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn1 = new SqlConnection(this.databaseConnStr);
                    return conn1.Query<OPDN_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }

                query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                using var conn = new SqlConnection(this.databaseConnStr);
                return conn.Query<OPDN_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep.Message}\n{excep.StackTrace}";
                return null;
            }
        }

        /// <summary>
        /// get list of the open AP invoice
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OPCH_Ex[] GetOpenApInvoice(Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM [FTS_vw_IMApp_OPCH] ";
                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn1 = new SqlConnection(this.databaseConnStr);
                    return conn1.Query<OPCH_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }

                query += $"WHERE " +
                    $" DocDate >= @QueryStartDate " +
                    $" AND DocDate <= @QueryEndDate " +
                    $" AND DocType = 'I' "; // only cater for the item query

                using var conn = new SqlConnection(this.databaseConnStr);
                return conn.Query<OPCH_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep.Message}\n{excep.StackTrace}";
                return null;
            }
        }
    }
}
