using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WMSWebAPI.Class;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DocStatusController : ControllerBase
    {
        readonly IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public DocStatusController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Controller entry point
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin, Admin, User")] /// tested with authenticated token based   
        [HttpPost]
        public IActionResult ActionPost(Cio bag)
        {
            switch (bag.request)
            {
                case "CheckRequestDocStatus":
                    {
                        return CheckRequestDocStatus(bag);
                    }
                case "GetRequestDetails":
                    {
                        return GetRequestDetails(bag);
                    }
                case "ResetRequestTried":
                    {
                        return ResetRequestTried(bag);
                    }
                default:
                    {
                        return BadRequest($"Invalid request, please try again later. Thanks");
                    }
            }
        }

        IActionResult ResetRequestTried (Cio bag)
        {
            try
            {
                const string _dbNameMidware = "DatabaseFTMiddleware";
                var _dbConnectionStr_midware = _configuration.GetConnectionString(_dbNameMidware);                

                using var sql_check = new SQL_DocStatus(_dbConnectionStr_midware);
                var result = sql_check.ResetRequestTried(bag.checkDocGuid);
                if (result == -1)
                {
                    Log(sql_check.LastErrorMessage, bag);
                    return BadRequest(sql_check.LastErrorMessage);
                }
                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        IActionResult GetRequestDetails(Cio bag)
        {
            try
            {
                const string _dbNameMidware = "DatabaseFTMiddleware";
                const string _dbName = "DatabaseWMSConn"; // 20200612T1030
                
                var _dbConnectionStr_midware = _configuration.GetConnectionString(_dbNameMidware);
                var _dbConnectionStr_sap = _configuration.GetConnectionString(_dbName);

                using var sql_check = new SQL_DocStatus(_dbConnectionStr_midware);

                var result = sql_check.GetRequestDetails(bag.checkDocGuid, _dbConnectionStr_sap);
                if (string.IsNullOrWhiteSpace(sql_check.LastErrorMessage)) return Ok(result);

                Log(sql_check.LastErrorMessage, bag);
                return BadRequest(sql_check.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// return te zmwRequest object based on the guid
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CheckRequestDocStatus(Cio bag)
        {
            try
            {
                const string _dbNameMidware = "DatabaseFTMiddleware";
                var _dbConnectionStr_midware = _configuration.GetConnectionString(_dbNameMidware);

                using var sql_check = new SQL_DocStatus(_dbConnectionStr_midware);
                var result = sql_check.GetRequestStatus(bag.checkDocGuid);

                if (string.IsNullOrWhiteSpace(sql_check.LastErrorMessage)) return Ok(result);

                Log(sql_check.LastErrorMessage, bag);
                return BadRequest(sql_check.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Logging error to log
        /// </summary>
        /// <param name="m        </param>
        /// <param name="obj"></param>
        void Log(string message, Cio bag)
        {
            _logger?.LogError(message, bag);
            _fileLogger.WriteLog(message);
        }
    }
}
