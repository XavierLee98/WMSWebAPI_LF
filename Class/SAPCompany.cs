using Dapper;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Models.SAP_DiApi;

namespace WMSWebAPI.Class
{
    public class SAPCompany 
    {
        public Company oCom { get; set; } = new Company();
        FileLogger _fileLogger = new FileLogger();
        public string errMsg { get; set; }
        public string UserID { get; set; }
        public SAPParam sapParam { get; set; }
        public string connStr { get; set; }

        public SAPCompany(string _connstr)
        {
            connStr = _connstr;
        }

        public SAPParam GetSAPSetting()
        {
            try
            {
                string query = "SELECT TOP 1 UserName, DBUser [DbUserName],DBPass [DbPassword], SAPCompany [CompanyDB], DBType [DbServerType], LicenseServer [LicenseServer], Server, SAPUser [UserName], SAPPass [Password]  FROM ft_SAPSettings";

                using (var conn = new SqlConnection(connStr))
                {
                    return conn.Query<SAPParam>(query).FirstOrDefault();
                };
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return null;
            }
        }
        public bool connectSAP()
        {

            if (oCom != null)
            {
                if (oCom.Connected) return true;
                else
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oCom);
                    oCom = null;
                }
            }

            sapParam = GetSAPSetting();
            if (sapParam == null)
            {
                errMsg = "Fail to get SAP Setting.";
                return false;
            }

            oCom = new SAPbobsCOM.Company();
            oCom.DbServerType = (SAPbobsCOM.BoDataServerTypes)int.Parse(sapParam.DbServerType);
            oCom.Server = sapParam.Server;
            oCom.DbUserName = sapParam.DbUserName;
            oCom.DbPassword = sapParam.DbPassword;
            oCom.CompanyDB = sapParam.CompanyDB;
            oCom.UserName = sapParam.UserName;
            oCom.Password = sapParam.Password;
            oCom.LicenseServer = sapParam.LicenseServer;

            if (oCom.Connect() != 0)
            {
                errMsg = oCom.GetLastErrorDescription();
                oCom = null;
                return false;
            }

            return true;
        }
        void Log(string message)
        {
            _fileLogger.WriteLog(message);
        }

    }
}
