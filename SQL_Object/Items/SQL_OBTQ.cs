using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObj.Warehouse;

namespace WMSWebAPI.SQL_Object
{
    public class SQL_OBTQ: IDisposable
    {
        public void Dispose() => GC.Collect();        

        public ItemObj[] GetSQL_OBTQs(string ItemCode, string dbConnStr)
        {
            using var conn = new SqlConnection(dbConnStr);

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

            //var sql = @"Select A.[ItemCode], B.[ItemName], A.[WhsCode], C.[BinCode], A.[Quantity], A.[CommitQty], A.[CountQty] 
            //            From OBTQ A
            //            Left Join OITM B ON A.[ItemCode] = B.[ItemCode]
            //            Inner Join OBIN C ON A.[SysNumber] = C.[AbsEntry] AND A.[MdAbsEntry] = C.[AbsEntry]
            //            Where A.[ItemCode] = @ItemCode And (A.[Quantity] > 0 or A.[CommitQty] > 0 or A.[CountQty] > 0)

            var sql = @"Select A.[ItemCode], B.[ItemName], A.[WhsCode],  B.[DistNumber], A.[Quantity] [OnHandQty], A.[CommitQty] [CommitQty], A.[CountQty] [OrderQty]
                            FROM
                            OBTQ A 
                                LEFT JOIN OBTN B ON A.MdAbsEntry = B.AbsEntry
                                LEFT JOIN OITM C ON A.ItemCode = C.ItemCode
                            WHERE A.[ItemCode] = @ItemCode And (A.[Quantity] > 0 or A.[CommitQty] > 0 or A.[CountQty] > 0)
                            Order By A.[WhsCode]";


            return conn.Query<ItemObj>(sql, new { ItemCode }).ToArray();
        }
    }
}
