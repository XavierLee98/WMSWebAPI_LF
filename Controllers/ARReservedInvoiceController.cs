using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
    [ApiController]
    public class ARReservedInvoiceController : Controller
    {
        readonly string _dbName = "DatabaseWMSConn";
        readonly string _dbNameMidware = "DatabaseFTMiddleware";

        readonly IConfiguration _configuration;
        ILogger _logger;

        FileLogger _fileLogger = new FileLogger();
        string _dbConnectionStr = string.Empty;
        string _dbMidwareConnectionStr = string.Empty;
        string _lastErrorMessage = string.Empty;

        public ARReservedInvoiceController(IConfiguration configuration, ILogger<ARReservedInvoiceController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
            _dbMidwareConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
            _logger = logger;
        }

        [Authorize(Roles = "SuperAdmin, Admin, User")] /// tested with authenticated token based   
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public IActionResult ActionPost(Cio bag)
        {
            try
            {
                _lastErrorMessage = string.Empty;
                switch (bag.request)
                {
                    case "GetReservedInvoice":
                        {
                            return GetReservedInvoice(bag);
                        }
                    case "GetReservedInvoiceDetails":
                        {
                            return GetReservedInvoiceDetails(bag);
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


        public IActionResult GetReservedInvoiceDetails(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var list = new SQL_OINV(_dbConnectionStr);
                var dtoPKL = new DTO_OPKL();
                dtoPKL.iNV1s = list.GetINV1s(bag);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoPKL);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }
        public IActionResult GetReservedInvoice(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var list = new SQL_OINV(_dbConnectionStr);
                var dtoPKL = new DTO_OPKL();
                dtoPKL.OINVs = list.GetOINVLists(bag);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoPKL);
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
