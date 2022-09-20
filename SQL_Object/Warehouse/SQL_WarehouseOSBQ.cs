using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObj.Warehouse;

namespace WMSWebAPI.SQL_Object.Warehouse
{
    public class SQL_WarehouseOSBQ : IDisposable
    {
        public void Dispose() => GC.Collect();
        //public string DBConnectionString { get; set; } = "Server=DESKTOP-9TJOTI1;Database=SBODemoUS;User Id=sa;Password=1234;";

        public List<ItemObj> GetSQL_WarehouseOSBQ(string WhsCode, string ItemCode, string dbConn)
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
            var sql = @"Select A.[ItemCode], B.[ItemName], A.[WhsCode], C.[BinCode], D.[DistNumber], A.[OnHandQty] 
                            FROM OSBQ A
                            Left Join OITM B ON A.[ItemCode] = B.[ItemCode]
                            Inner Join OBIN C ON A.[BinAbs] = C.[AbsEntry]
                            Inner Join OSRN D ON A.[SnBMDAbs] = D.[AbsEntry] AND A.[ItemCode] = D.[ItemCode] 
                            Where A.[WhsCode] = @WhsCode
                            AND A.[ItemCode] = @ItemCode 
                            AND A.[OnHandQty] > 0 
                            Order By A.[WhsCode], C.[BinCode], D.[DistNumber]";
            
            var list = conn.Query<ItemObj>(sql, new { WhsCode, ItemCode }).ToList();

            list.ForEach(x => x.request = "QuerySerialItemWithBin");
            return list;

        }
    }
}