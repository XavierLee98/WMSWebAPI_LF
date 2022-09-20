using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using WMSWebAPI.Class;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_OPOR : IDisposable
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
        public SQL_OPOR(string dbConnStr) => databaseConnStr = dbConnStr;                    

        /// <summary>
        /// Return list of the business partner with open po item
        /// </summary>
        /// <returns></returns>
        public OCRD[] GetBpWithOpenPo()
        {
            try
            {
                string query =
                    $"SELECT * " +
                    $"FROM {nameof(OCRD)}" +
                    $"WHERE CardCode IN " +
                    $"(SELECT DISTINCT " +
                    $"CardCode " +
                    $"FROM {nameof(OPDN)} " +
                    $"WHERE DocStatus = 'O')";

                using var conn = new SqlConnection(databaseConnStr);
                return conn.Query<OCRD>(query).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Return array of the vendor object tgo app
        /// </summary>
        /// <returns></returns>
        public OCRD[] GetVendor()
        {
            try
            {
                return new SqlConnection(databaseConnStr)
                    .Query<OCRD>("SELECT * FROM FTS_vw_IMApp_OCRD_Supplier").ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// Return list of the purchase order line with open item
        /// </summary>
        /// <returns></returns>
        public OPOR[] GetOpenPo(Cio bag)
        {
            try
            {
                // 20200628t2358 QUERY FROM view from database
                string query = "SELECT * FROM FTS_vw_IMApp_OPOR ";

                if (bag.getPoType.Length > 0)
                {
                    query += $"WHERE DocStatus = @getPoType " +
                        $"AND DocDate >= @QueryStartDate " +
                        $"AND DocDate <= @QueryEndDate " +
                        $"AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OPOR>(query, new { bag.getPoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
                else
                {
                    query += $"WHERE " +
                        $" DocDate >= @QueryStartDate " +
                        $" AND DocDate <= @QueryEndDate " +
                        $" AND DocType = 'I' "; // only cater for the item query

                    using var conn = new SqlConnection(this.databaseConnStr);
                    return conn.Query<OPOR>(query, new { bag.getPoType, bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }
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
        public POR1[] GetOpenPoLines(int[] poEntries)
        {
            try
            {
                // Query from view and code filter for doc entry               
                using var conn = new SqlConnection(databaseConnStr);

                var POR1s = new List<POR1>();
                foreach (var docEntry in poEntries)
                {
                    var lines = conn.Query<POR1>(
                        "SELECT * FROM FTS_vw_IMApp_POR1 WHERE docEntry = @docEntry", new { docEntry }).ToArray();
                    if (lines == null) continue;
                    if (lines.Length == 0) continue;
                    POR1s.AddRange(lines);
                }

                return POR1s.ToArray();
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
            return conn.Query<NNM1>("SELECT * FROM NNM1 WHERE ObjectCode = '20'").ToArray();
        }

        /// <summary>
        /// Query the database and get list of the warehouse
        /// 20200617T0952
        /// </summary>
        /// <returns></returns>
        public OWHS[] GetWarehouses()
        {
            try
            {
                string query = "SELECT * FROM OWHS";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OWHS>(query).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }
    }
}