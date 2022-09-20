using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.Models.ReturnRequest;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_OINV : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        public string LastErrorMessage { get; private set; } = string.Empty;
        public void Dispose() => GC.Collect();

        public SQL_OINV(string dbConnStr) => databaseConnStr = dbConnStr;

        public OINV_Ex[] GetOINVLists(Cio bag)
        {
            try
            {
                var query = "SELECT T0.* FROM OINV T0 " +
                             "INNER JOIN INV1 T1 ON T0.DocEntry = T1.DocEntry " +
                             "LEFT JOIN PKL1 T2 ON T1.DocEntry = T2.OrderEntry AND T1.LineNum = T2.OrderLine AND BaseObject = 13 " +
                             "WHERE T0.DocDate >= @QueryStartDate " +
                             "AND T0.DocDate <= @QueryEndDate " +
                             "AND T0.isIns = 'Y' " +
                             "AND  T2.AbsEntry IS NULL"; 
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<OINV_Ex>(query, new { bag.QueryStartDate, bag.QueryEndDate }).ToArray();
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }


        //NotYetDone
        public INV1_Ex[] GetINV1s(Cio bag)
        {
            try
            {
                var query = "SELECT * FROM OINV" ;
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    return conn.Query<INV1_Ex>(query, new { bag.InvoiceDoc }).ToArray();
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = $"{excep}";
                return null;
            }
        }
    }

}
