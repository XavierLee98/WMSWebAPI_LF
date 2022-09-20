using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObject.Warehouse;

namespace WMSWebAPI.SQL_Object.Warehouse
{
    public class SQL_AvailableItem : IDisposable
    {
        public void Dispose() => GC.Collect();
        //public string DBConnectionString { get; set; } = "Server=DESKTOP-9TJOTI1;Database=SBODemoUS;User Id=sa;Password=1234;";

        public AvailableItemObj[] GetAvailableItem(string WhsCode, string dbConn)
        {
            using var conn = new SqlConnection(dbConn);
            var sql = @"SELECT T0.WhsCode, T0.ItemCode, T1.ItemName, T2.ItmsGrpNam,T0.OnHand
                                FROM OITW T0  
                                INNER JOIN OITM T1 ON T0.ItemCode = T1.ItemCode 
                                INNER JOIN OITB T2 ON T1.ItmsGrpCod = T2.ItmsGrpCod 
                                WHERE T0.OnHand > 0 and T0.WhsCode = @WhsCode";
            return conn.Query<AvailableItemObj>(sql, new { WhsCode }).ToArray();
        }
    }
}
