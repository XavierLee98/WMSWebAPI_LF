using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;

namespace WMSWebAPI.SAP_SQL
{
    public class SQL_VersionControl : IDisposable
    {

        string midwareConnStr;
        public string lastErrorMessage { get; private set; } = string.Empty; 
        public void Dispose()
        {
            GC.Collect();
        }

        public SQL_VersionControl(string _midwareCOnnStr)
        {
            midwareConnStr = _midwareCOnnStr;
        }

        public VersionControl GetAppInfo()
        {
            var conn = new SqlConnection(midwareConnStr);
            var query = "SELECT * FROM VersionControl";
            return conn.Query<VersionControl>(query).FirstOrDefault();
        }

        public bool CheckAppVersion(string AppName, string appVersion, out string url)
        {
            url = "";
            var conn = new SqlConnection(midwareConnStr);
            var query = "SELECT * FROM VersionControl WHERE AppName = @appName";
            var result = conn.Query<VersionControl>(query, new { appName = AppName }).FirstOrDefault();

            url = result.APKSourceUrl;

            if (result == null) return false;

            if (result.AppVersion != appVersion) return false;


            return true;

        }
    }
}
