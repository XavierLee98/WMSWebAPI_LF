using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.ClassObject;

namespace WMSWebAPI.SQL_Object
{
    public class SQL_OWHS
    {
        public void Dispose() => GC.Collect();
        //public string DBConnectionString { get; set; } = "Server=DESKTOP-9TJOTI1;Database=SBODemoUS;User Id=sa;Password=1234;";

        public List<OWHSObj> GetSQL_OWHSs(string dbConn)
        {
            using var conn = new SqlConnection(dbConn);
            return conn.Query<OWHSObj>("SELECT WhsCode, WhsName FROM OWHS").ToList();
        }
    }
}
