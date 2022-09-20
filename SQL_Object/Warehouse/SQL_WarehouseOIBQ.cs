using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObj.Warehouse;

namespace WMSWebAPI.SQL_Object.Warehouse
{
    public class SQL_WarehouseOIBQ : IDisposable
    {
        public void Dispose() => GC.Collect();
        //public string DBConnectionString { get; set; } = "Server=DESKTOP-9TJOTI1;Database=SBODemoUS;User Id=sa;Password=1234;";

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

        public List<ItemObj> GetSQL_WarehouseOIBQ(string WhsCode, string ItemCode, string dbConn)
        {
            using var conn = new SqlConnection(dbConn);
            var sql = @"Select A.[ItemCode], B.[ItemName] , A.[WhsCode], C.[BinCode], A.[OnHandQty] 
                            From OIBQ A
                            Left Join OITM B ON A.[ItemCode] = B.[ItemCode]
                            Inner Join OBIN C ON A.[BinAbs] = C.[AbsEntry]
                            Where A.[WhsCode] = @WhsCode
                            and A.[ItemCode] = @ItemCode 
                            and A.[OnHandQty] > 0
                            Order By A.[WhsCode], C.[BinCode]";
            var list = conn.Query<ItemObj>(sql, new { WhsCode, ItemCode }).ToList();
            list.ForEach(x => x.request = "QueryNonManageItemWithBin");

            return list;
        }
    }
}
