using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Models.ReturnToCN;

namespace WMSWebAPI.SAP_SQL.ReturnToCN
{
    public class SQL_ORIN : IDisposable
    {
        string databaseConnStr { get; set; } = "";
        public string LastErrorMessage { get; private set; } = string.Empty;
        public void Dispose() => GC.Collect();

        public SQL_ORIN(string dbConnStr) => databaseConnStr = dbConnStr;


        /// <summary>
        /// Update SRNLineToChecked
        /// </summary>
        /// <returns></returns>
        public int QueryUpdateHeaderStatus(ReturnHeader returnHeader)
        {
            try
            {
                string updatequery = "UPDATE zmwReturnHeader SET DocStatus = @DocStatus, " +
                    " UpdatedDate = @UpdatedDate " +
                    " WHERE Guid = @Guid; ";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var result = conn.Execute(updatequery, new { DocStatus = returnHeader.DocStatus, UpdatedDate = DateTime.Now, Guid = returnHeader.Guid });
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        /// Update SRNLineToChecked
        /// </summary>
        /// <returns></returns>
        public int QueryUpdateHeaderStatusToPosted(ReturnHeader returnHeader, string DocNo, List<ReturnDetails> returnDetails)
        {
            try
            {
                string updatequery = "UPDATE zmwReturnHeader SET DocStatus = @DocStatus, " +
                    " UpdatedDate = @UpdatedDate, " +
                    " DocNum = @DocNum " +
                    " WHERE Guid = @Guid; ";

                string updatelinequery = "UPDATE zmwReturnDetails SET UnitPrice = @UnitPrice " +
                                         "WHERE Guid = @Guid AND LineNum = @LineNum; ";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var result = conn.Execute(updatequery, new { DocStatus = returnHeader.DocStatus, UpdatedDate = DateTime.Now, DocNum = DocNo , Guid = returnHeader.Guid });
                    if (result < 0) return result;
                    foreach(var line in returnDetails)
                    {
                        result = conn.Execute(updatelinequery, new { UnitPrice = line.UnitPrice, Guid = line.Guid, LineNum = line.LineNum });
                    }
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }


        /// <summary>
        /// Update SRNLineToChecked
        /// </summary>
        /// <returns></returns>
        public int QueryUpdateGoodQty(List<ReturnDetails> returnDetails )
        {
            try
            {
                string updatequery = "UPDATE zmwReturnDetails SET GoodQty = @GoodQty," +
                    " IsChecked = @IsChecked " +
                    "WHERE Guid = @Guid AND LineNum = @LineNum; ";
                using (var conn = new SqlConnection(databaseConnStr))
                {
                    int result = -1;
                    foreach(var line in returnDetails)
                    {
                        result = conn.Execute(updatequery, new { GoodQty = line.GoodQty, IsChecked = line.IsChecked, Guid = line.Guid, LineNum = line.LineNum });
                    }
                    return result;
                }

            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

        /// <summary>
        /// Insert(Save) A Stock Return Note Header
        /// </summary>
        /// <returns></returns>
        public int CreateSRNHeader(ReturnHeader returnHeader, List<ReturnDetails> returnDetails)
        {
            try
            {
                string headquery = "INSERT INTO zmwReturnHeader "
                                +"(Guid " +
                                ", DocID " 
                                +",DocDate "
                                +",CreatedDate "
                                +",UpdatedDate "
                                +",CustName "
                                +",CustCode "
                                +",Driver "
                                +",DriverName "
                                +",Remark "
                                +",TotalPrice "
                                +",NumAtCard " +
                                ",DocStatus" +
                                ",CreatedUser ) " +
                                "VALUES " +
                                " ( @Guid, " +
                                "(Select COUNT(ID)+1 from zmwReturnHeader WITH (UPDLOCK, HOLDLOCK)), " +
                                "@DocDate, " +
                                "@CreatedDate, " +
                                "@UpdatedDate, " +
                                "@CustName, " +
                                "@CustCode, " +
                                "@Driver, " +
                                "@DriverName, " +
                                "@Remark, " +
                                "@TotalPrice, " +
                                "@NumAtCard, " +
                                "@DocStatus, " +
                                "@CreatedUser); ";


                using (var conn = new SqlConnection(databaseConnStr))
                {
                    var result = 
                        conn.Execute(headquery, 
                        new { Guid = returnHeader.Guid, 
                              DocDate = returnHeader.DocDate, 
                              CreatedDate = returnHeader.CreatedDate, 
                              UpdatedDate = returnHeader.UpdatedDate,
                              CustName = returnHeader.CustName,
                              CustCode = returnHeader.CustCode,
                              Driver = returnHeader.Driver,
                              DriverName = returnHeader.DriverName,
                              Remark = returnHeader.Remark,
                              TotalPrice = returnHeader.TotalPrice,
                              NumAtCard = returnHeader.NumAtCard,
                              DocStatus = "Open",
                              CreatedUser = returnHeader.CreatedUser
                        });
                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }


        /// <summary>
        /// Insert(Save) Stock Return Note Details
        /// </summary>
        /// <returns></returns>
        public int CreateSRNDetails(ReturnHeader returnHeader, List<ReturnDetails> returnDetails)
        {
            try
            {
                string selectquery = "SELECT * FROM zmwReturnHeader WHERE guid = @guid; ";
                string linequery = "INSERT INTO zmwReturnDetails " +
                                   "(DocEntry, " +
                                   "Guid, " +
                                   "LineNum, " +
                                   "ItemCode, " +
                                   "ItemDesc," +
                                   "Quantity, " +
                                   "LineTotal, " +
                                   "UnitPrice, " +
                                   "ToWhsCode, " +
                                   "Batch, " +
                                   "CNReason, " +
                                   "IsChecked, " +
                                   "GoodQty," +
                                   "ManufactureDate ) " +
                                   " VALUES " +
                                   "( @DocEntry, " +
                                   " @Guid, " +
                                   " @LineNum, " +
                                   " @ItemCode, " +
                                   " @ItemDesc, " +
                                   " @Quantity, " +
                                   " @LineTotal, " +
                                   " @UnitPrice, " +
                                   " @ToWhsCode, " +
                                   " @Batch, " +
                                   " @CNReason, " +
                                   " @IsChecked, " +
                                   " @GoodQty," +
                                   " @ManufactureDate ); ";


                using (var conn = new SqlConnection(databaseConnStr))
                {

                    var header = conn.Query<ReturnHeader>(selectquery, new { Guid = returnHeader.Guid }).FirstOrDefault();
                    int result = -1;
                    foreach (var line in returnDetails)
                    {
                        line.DocEntry = header.DocEntry;
                        result = 
                            conn.Execute(linequery, 
                            new { DocEntry = line.DocEntry,
                                  Guid = line.Guid,
                                  LineNum = line.LineNum,
                                  ItemCode = line.ItemCode,
                                  ItemDesc = line.ItemDesc,
                                  Quantity = line.Quantity,
                                  LineTotal = line.LineTotal,
                                  UnitPrice = line.UnitPrice,
                                  ToWhsCode = line.ToWhsCode,
                                  Batch = line.Batch,
                                  CNReason = line.CNReason,
                                  IsChecked = line.IsChecked,
                                  GoodQty = line.GoodQty,
                                ManufactureDate = line.ManufactureDate
                            });
                    }
                    return result;
                }
            }
            catch (Exception excep)
            {
                LastErrorMessage = excep.ToString();
                return -1;
            }
        }

    }
}
