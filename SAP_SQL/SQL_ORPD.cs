using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSWebAPI.Class;
using WMSWebAPI.Models.GoodsReturnRequest;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_ORPD : IDisposable
    {
        string databaseConnStr { get; set; } = "";
       
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
        public SQL_ORPD(string dbConnStr) =>databaseConnStr = dbConnStr;           
        

        /// <summary>
        /// Return grpo lines
        /// </summary>
        /// <param name="grpoEntris"></param>
        /// <returns></returns>
        public PDN1_Ex [] GetOpenGrpoLines (int [] grpoEntris)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);

                var PDN1_Exs = new List<PDN1_Ex>();
                foreach (var docEntry in grpoEntris)
                {
                    var lines = conn.Query<PDN1_Ex>(
                        "SELECT * FROM FTS_vw_IMApp_PDN1 WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    PDN1_Exs.AddRange(lines);
                }

                return PDN1_Exs.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// GetOpenPoLines by multiple pos entries
        /// </summary>
        /// <param name="poDocEntries"></param>
        /// <returns></returns>
        public PRR1_Ex[] GetOpenPrrLines(int[] grrEntries)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);

                var PRR1_Exs = new List<PRR1_Ex>();
                foreach (var docEntry in grrEntries)
                {
                    var lines = conn.Query<PRR1_Ex>(
                        "SELECT * FROM FTS_vw_IMApp_PRR1 WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    PRR1_Exs.AddRange(lines);
                }

                return PRR1_Exs.ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
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

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OPDN_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OPDN_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get list of good return request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OPRR_Ex[] GetOpenGoodsReturnRequest(Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_OPRR ";

                if (bag.getSoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getSoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OPRR_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OPRR_Ex>(query, new { bag.getSoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Get good return doc series
        /// </summary>
        /// <returns></returns>
        public NNM1[] GR_GetDocSeries()
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '21'").ToArray();
        }

    }
}
