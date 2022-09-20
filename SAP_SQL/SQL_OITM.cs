using Dapper;
using DbClass;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.Models.Demo;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_OITM : IDisposable
    {
        string databaseConnStr { get; set; } = "";

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbConnString"></param>
        public SQL_OITM(string dbConnStr) => databaseConnStr = dbConnStr;


        /// <summary>
        /// Return list of item code search
        /// </summary>
        /// <returns></returns>
        public OITM[] GetItems()
        {
            try
            {
                string query = "SELECT * FROM OITM WHERE InvntItem = 'Y'";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OITM>(query).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        ///  Return single item from the database
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public OITM GetItem(string itemCode)
        {
            try
            {
                string query = "SELECT * FROM OITM WHERE ItemCode = @itemcode";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OITM>(query, new { itemCode }).FirstOrDefault();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// 20200927T1030
        /// Get the content of the list view
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public BinContent[] GetItemBinContents(string itemCode)
        {
            try
            {                
                return new SqlConnection(databaseConnStr).Query<BinContent>(
                               "SELECT * FROM FTS_vw_IMApp_BinContentList " +
                               "WHERE ItemCode = @itemcode", new { itemCode }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }

        /// <summary>
        /// 20200927T1030
        /// Query database on item translog from date to date
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public StockTransactionLog[] GetItemTransLogs(string itemCode, DateTime startDate, DateTime endDate)
        {
            try
            {
                return new SqlConnection(databaseConnStr).Query<StockTransactionLog>(
                    "SELECT * " +
                    "FROM FTS_vw_IMApp_StockTransactionLogs " +
                    "WHERE itemCode = @itemCode " +
                    "AND docdate >= @startDate "+
                    "AND docdate <= @endDate", new { itemCode, startDate, endDate }).ToArray();
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }
    }
}
