using Dapper;
using DbClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.Models.ReturnToCN;
using WMSWebAPI.SAP_DiApi;
using WMSWebAPI.SAP_SQL.ReturnToCN;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReturnToCNController : Controller
    {
        readonly string _dbName = "DatabaseWMSConn";
        readonly string _dbNameMidware = "DatabaseFTMiddleware";

        readonly IConfiguration _configuration;
        ILogger _logger;

        FileLogger _fileLogger = new FileLogger();
        SAPCompany _company;
        string _dbConnectionStr = string.Empty;
        string _dbMidwareConnectionStr = string.Empty;
        string _lastErrorMessage = string.Empty;

        public ReturnToCNController(IConfiguration configuration, ILogger<PickListController> logger, SAPCompany company)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
            _dbMidwareConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
            _logger = logger;
            _company = company;
        }

        /// <summary>
        /// Controller entry point
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin, Admin, User")] /// tested with authenticated token based   
        [System.Web.Http.HttpPost]
        public IActionResult ActionPost(Cio bag)
        {
            try
            {
                _lastErrorMessage = string.Empty;
                switch (bag.request)
                {
                    case "QueryGetCNReason":
                        {
                            return QueryGetCNReason(bag);
                        }
                    case "CreateSRN":
                        {
                            return CreateSRN(bag);
                        }
                    case "QueryGetReturnHeader":
                        {
                            return QueryGetReturnHeader(bag);
                        }
                    case "QueryGetReturnDetails":
                        {
                            return QueryGetReturnDetails(bag);
                        }
                    case "QueryUpdateStatusToQCChecked":
                        {
                            return QueryUpdateStatusToQCChecked(bag);
                        }
                    case "QueryUpdateStatus":
                        {
                            return QueryUpdateStatus(bag);
                        }
                    case "QueryUpdateStatusToPosted":
                        {
                            return QueryUpdateStatusToPosted(bag);
                        }
                    case "QueryGetReturnDetailsWithPrice":
                        {
                            return QueryGetReturnDetailsWithPrice(bag);
                        }
                }
                return BadRequest($"Invalid request, please try again later. Thanks");
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Return Details by header
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryGetReturnDetailsWithPrice(Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM [dbo].[zmwReturnDetails] " +
                    " WHERE DocEntry = @DocEntry AND Guid = @Guid; ";

                var sql2 = "select Top 1 * from RDR1 T0 " +
                          "Where T0.ItemCode = @ItemCode AND T0.Price > 0 " +
                          "order by T0.DocDate desc ";
                using var conn = new SqlConnection(_dbMidwareConnectionStr);              
                using var dbconn = new SqlConnection(_dbConnectionStr);

                var dtoReturn = new DTO_ORIN();
                var returnDetails = conn.Query<ReturnDetails>(sql, new { DocEntry = bag.QueryStringDocEntry, Guid = bag.QueryGuid }).ToArray();

                foreach (var line in returnDetails)
                {
                    var soline = dbconn.Query<RDR1>(sql2, new { ItemCode = line.ItemCode }).FirstOrDefault();
                    if (soline == null)
                    {
                        line.UnitPrice = 0;
                    }
                    else
                    {
                        line.UnitPrice = soline.Price;
                    }
                }
                dtoReturn.returnLines = returnDetails.ToArray();

                return Ok(dtoReturn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// UpdatePrice And Post to SAP
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryUpdateStatusToPosted(Cio bag)
        {
            try
            {
                int result = -1;
                using var diapi = new DiApiCN(_configuration, _dbConnectionStr,_dbMidwareConnectionStr, _company);
                string diapiresult = diapi.RICreateCN(bag.ReturnHeader,bag.ReturnDetails);

                if (diapiresult == null)
                {
                    _lastErrorMessage = diapi.LastErrorMessage;
                    return BadRequest(_lastErrorMessage);
                }

                //Get Doc Num
                string queryselect = "SELECT DocNum FROM ORIN WHERE DocEntry = @DocEntry; ";
                using var dbconn = new SqlConnection(_dbConnectionStr);
                var docNo = dbconn.Query<string>(queryselect, new { DocEntry = diapiresult }).FirstOrDefault();

                using (var orin = new SQL_ORIN(_dbMidwareConnectionStr))
                {

                    result = orin.QueryUpdateHeaderStatusToPosted(bag.ReturnHeader, docNo,bag.ReturnDetails);

                    if (result < 0)
                    {
                        _lastErrorMessage = orin.LastErrorMessage;
                        return BadRequest(_lastErrorMessage);
                    }
                    return Ok(docNo);
                }
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Update Document Status to Approved
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryUpdateStatus(Cio bag)
        {
            try
            {
                using (var orin = new SQL_ORIN(_dbMidwareConnectionStr))
                {

                    var result = orin.QueryUpdateHeaderStatus(bag.ReturnHeader);

                    if (result < 0)
                    {
                        _lastErrorMessage = orin.LastErrorMessage;
                        return BadRequest(_lastErrorMessage);
                    }
                    return Ok();
                }
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Return Details by header
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryUpdateStatusToQCChecked(Cio bag)
        {
            try
            {
                using (var orin = new SQL_ORIN(_dbMidwareConnectionStr))
                {

                    var result = orin.QueryUpdateGoodQty(bag.ReturnDetails);

                    if (result < 0)
                    {
                        _lastErrorMessage = orin.LastErrorMessage;
                        return BadRequest(_lastErrorMessage);
                    }

                    result = orin.QueryUpdateHeaderStatus(bag.ReturnHeader);

                    if (result < 0)
                    {
                        _lastErrorMessage = orin.LastErrorMessage;
                        return BadRequest(_lastErrorMessage);
                    }
                    return Ok();
                }
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Return Details by header
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryGetReturnDetails(Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM [dbo].[zmwReturnDetails] " +
                    " WHERE DocEntry = @DocEntry AND Guid = @Guid; ";
                using var conn = new SqlConnection(_dbMidwareConnectionStr);
                var dtoReturn = new DTO_ORIN();
                dtoReturn.returnLines = conn.Query<ReturnDetails>(sql, new { DocEntry = bag.QueryStringDocEntry, Guid = bag.QueryGuid }).ToArray();
                return Ok(dtoReturn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Return Header
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryGetReturnHeader(Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM [dbo].[zmwReturnHeader] " +
                    " WHERE DocDate >= @QueryStartDate AND DocDate <= @QueryEndDate; ";
                using var conn = new SqlConnection(_dbMidwareConnectionStr);
                var dtoReturn = new DTO_ORIN();
                dtoReturn.returnHeaders = conn.Query<ReturnHeader>(sql, new { QueryStartDate = bag.QueryStartDate, QueryEndDate = bag.QueryEndDate }).ToArray();
                return Ok(dtoReturn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get CN Reason
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult QueryGetCNReason(Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM [dbo].[@CNREASON]";
                using var conn = new SqlConnection(_dbConnectionStr);
                var list = conn.Query<CNReason>(sql).ToList();
                return Ok(list);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        
        /// <summary>
        /// Create A SRM
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult CreateSRN(Cio bag)
        {
            try
            {
                using (var orin = new SQL_ORIN(_dbMidwareConnectionStr))
                {
                    var result = orin.CreateSRNHeader(bag.ReturnHeader,bag.ReturnDetails);

                    if (result < 0)
                    {
                        _lastErrorMessage = orin.LastErrorMessage;
                        return BadRequest(_lastErrorMessage);
                    }

                    result = orin.CreateSRNDetails( bag.ReturnHeader, bag.ReturnDetails);

                    if (result < 0)
                    {
                        _lastErrorMessage = orin.LastErrorMessage;
                        return BadRequest(_lastErrorMessage);
                    }

                    return Ok();
                };
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        void Log(string message, Cio bag)
        {
            _logger?.LogError(message, bag);
            _fileLogger.WriteLog(message);
        }
    }
}
