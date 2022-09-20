using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObj.Warehouse;

namespace WMSWebAPI.SQL_Object.Warehouse
{
    public class SQL_WarehouseOITW: IDisposable
    {
        public void Dispose() => GC.Collect();
        
        public List<ItemObj> GetSQL_WarehouseOITW(string WhsCode, string ItemCode, string dbConn)
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
            var sql = @"Select W.[ItemCode], I.[ItemName],W.[WhsCode],W.[OnHand] [OnHandQty], W.[IsCommited] [CommitQty], W.[OnOrder] [OrderQty] From OITW W
                            Left Join OITM I ON W.[ItemCode] = I.[ItemCode]
                            Where W.[WhsCode] = @WhsCode
                            And W.[ItemCode] = @ItemCode 
                            and (W.[OnHand] > 0 or W.[IsCommited] > 0 or W.[OnOrder] > 0)
                            Order by W.[WhsCode], W.[ItemCode]";
            var list = conn.Query<ItemObj>(sql, new { WhsCode, ItemCode }).ToList();

            list.ForEach(x => x.request = "QueryNonManageItem");
            return list;
        }
    }
}
