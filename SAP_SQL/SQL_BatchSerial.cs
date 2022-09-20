using Dapper;
using DbClass;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.Class;
using WMSWebAPI.Models.Demo;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_BatchSerial : IDisposable
    {
        string databaseConnStr { get; set; } = "";        
      
        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

        public SQL_BatchSerial(string dbConnStr) => databaseConnStr = dbConnStr;                    

        /// <summary>
        /// Return the query OSRN to app
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public OSRI IsSerialNumExist(Cio bag)
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<OSRI>("SELECT * FROM OSRI " +
                "WHERE IntrSerial = @QueryDistNum",
                new { bag.QueryDistNum }).FirstOrDefault();
        }

        public OBTN IsBatchNumExist(Cio bag)
        {
            using var conn = new SqlConnection(databaseConnStr);
            return conn.Query<OBTN>("SELECT * FROM OBTN WHERE DistNumber = @QueryDistNum",
                new { bag.QueryDistNum }).FirstOrDefault();
        }
    }
}
