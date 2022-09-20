using System;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WMSWebAPI.Class;
using WMSWebAPI.Models.ResponseMonitor;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ResponseMonitorController : ControllerBase
    {
        //readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030

        readonly string _dbName = "DatabaseFTMiddleware";
        readonly IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();
        string _dbConnectionStr = string.Empty;        

        public ResponseMonitorController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
            _logger = logger;
        }

        [Authorize(Roles = "SuperAdmin, Admin, User")] /// tested with authenticated token based   
        [HttpPost]
        public IActionResult ActionPost(ResponseMonitor result)
        {
            try
            {
                if (result != null)
                {
                    // insert into the database
                    using var conn = new SqlConnection(_dbConnectionStr);
                    var sql = @"INSERT INTO zmwResponseMonitor (
                                Request
                               ,Duration
                               ,SizeInByte
                               ,HttpStatusCode
                               ,AppName
                               ,[User]
                               ,TransDate 
                               ,EndPoint
                                ) VALUES (
                                @Request
                               ,@Duration
                               ,@SizeInByte
                               ,@HttpStatusCode
                               ,@AppName
                               ,@User
                               ,@TransDate
                               ,@EndPoint)";

                    var insertRes = conn.Execute(sql, result);
                    if (insertRes >= 1) return Ok();
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                Log($"{e}", result);
                return BadRequest($"{e}");
            }
        }

        void Log(string message, object bag)
        {
            _logger?.LogError(message, bag);
            _fileLogger.WriteLog(message);
        }
    }
}
