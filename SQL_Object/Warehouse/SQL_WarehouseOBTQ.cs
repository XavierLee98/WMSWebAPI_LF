using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObj.Warehouse;

namespace WMSWebAPI.SQL_Object.Warehouse
{
    public class SQL_WarehouseOBTQ : IDisposable
    {
        public void Dispose() => GC.Collect();
        //public string DBConnectionString { get; set; } = "Server=DESKTOP-9TJOTI1;Database=SBODemoUS;User Id=sa;Password=1234;";

        public List<ItemObj> GetSQL_WarehouseOBTQ(string WhsCode, string ItemCode, string dbConn)
        {
            using var conn = new SqlConnection(dbConn);


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

            var sql = @"Select A.[ItemCode], B.[ItemName], A.[WhsCode], B.[DistNumber], A.[Quantity] [OnHandQty], A.[CommitQty] , A.[CountQty] [OrderQty]
                            FROM
                            OBTQ A 
                            LEFT JOIN OBTN B ON A.MdAbsEntry = B.AbsEntry
                            LEFT JOIN OITM C ON A.ItemCode = C.ItemCode
                            WHERE A.[ItemCode] = @ItemCode 
                            AND A.[WhsCode] = @WhsCode 
                            And (A.[Quantity] > 0 or A.[CommitQty] > 0 or A.[CountQty] > 0)
                            Order By A.[WhsCode]";



            //var sql = @"Select A.[ItemCode], B.[ItemName], A.[WhsCode], C.[BinCode], A.[Quantity], A.[CommitQty], A.[CountQty] From OBTQ A
            //                Left Join OITM B ON A.[ItemCode] = B.[ItemCode]
            //                Inner Join OBIN C ON A.[SysNumber] = C.[AbsEntry] AND A.[MdAbsEntry] = C.[AbsEntry]
            //                Where A.[WhsCode] = @WhsCode
            //                And A.[ItemCode] = @ItemCode 
            //                And (A.[Quantity] > 0 or A.[CommitQty] > 0 or A.[CountQty] > 0)
            //                Order By A.[WhsCode], C.[BinCode]";


            var list = conn.Query<ItemObj>(sql, new { WhsCode, ItemCode }).ToList();
            list.ForEach(x => x.request = "QueryBatchItemWithoutBin");
            return list;
        }
    }
}