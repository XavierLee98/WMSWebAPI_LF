using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObject;

namespace WebApi.SQL_Object
{
    public class SQL_OITM: IDisposable
    {
        public void Dispose() => GC.Collect();        

        public List<OITMObj> GetSQL_OITMs(string dbConn) => 
            new SqlConnection(dbConn).Query<OITMObj>("SELECT ISNULL(ItemName, '') [ItemName], ItemCode " +
                "FROM OITM " +
                "WHERE InvntItem ='Y' AND SellItem ='Y' AND PrchseItem='Y' ").ToList();

        // ""SELECT * FROM SQLView_OITM
    }
}
