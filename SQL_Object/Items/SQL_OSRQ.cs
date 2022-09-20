using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObj.Warehouse;

namespace WMSWebAPI.SQL_Object
{
    public class SQL_OSRQ:IDisposable
    {
        public void Dispose() => GC.Collect();
        //public string DBConnectionString { get; set; } = "Server=DESKTOP-9TJOTI1;Database=SBODemoUS;User Id=sa;Password=1234;";

        public ItemObj[] GetSQL_OSRQs(string ItemCode, string dbConn)
        {
            //public string ItemCode { get; set; }
            //public string ItemName { get; set; }
            //public string WhsCode { get; set; }
            //public string DistNumber { get; set; }
            //public string BinCode { get; set; }
            //public decimal OnHandQty { get; set; }
            //public decimal OrderQty { get; set; }
            //public decimal CommitQty { get; set; }
            //public decimal CountQty { get; set; }
            //public string request { get; set; }

            using var conn = new SqlConnection(dbConn);
            var sql = @"Select A.[ItemCode], B.[ItemName], A.[WhsCode], C.[DistNumber], A.[Quantity][OnHandQty], A.[CommitQty], A.[CountQty] [OrderQty] 
                            FROM OSRQ A
                            Left Join OITM B ON A.[ItemCode] = B.[ItemCode]
                            Inner Join OSRN C ON A.[MdAbsEntry] = C.[AbsEntry]
                            where A.[ItemCode] = @ItemCode and (A.[Quantity] > 0 or A.[CommitQty] > 0 or A.[CountQty] > 0)
                            Order By A.[WhsCode], C.[DistNumber]";

            return conn.Query<ItemObj>(sql, new { ItemCode }).ToArray();
        }
    }
}
