using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WMSWebAPI.Class;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BatchSerialController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030               
        readonly IConfiguration _configuration;
        string _dbConnectionStr { get; set; } = string.Empty;
        ILogger _logger { get; set; } = null;
        FileLogger _fileLogger { get; set; } = new FileLogger();

        public BatchSerialController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
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
                case "IsSerialNumExist":
                    {
                        return IsSerialNumExist(bag);
                    }
                case "IsBatchNumExist":
                    {
                        return IsBatchNumExist(bag);
                    }
                default:
                    {
                        return BadRequest($"Invalid request, please try again later. Thanks");
                    }
            }
        }

        /// <summary>
        /// Get the serial object and return to app for decision
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult IsSerialNumExist(Cio bag)
        {
            try
            {
                // CheckBatchStatus
                using var _return = new SQL_BatchSerial(_dbConnectionStr);
                var result = _return.IsSerialNumExist(bag);

                if (_return.LastErrorMessage.Length > 0) return BadRequest(_return.LastErrorMessage);
                return Ok(result);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get the serial object and return to app for decision
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult IsBatchNumExist(Cio bag)
        {
            try
            {
                // CheckBatchStatus
                using var _return = new SQL_BatchSerial(_dbConnectionStr);
                var result = _return.IsBatchNumExist(bag);

                if (_return.LastErrorMessage.Length > 0) return BadRequest(_return.LastErrorMessage);
                return Ok(result);
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
