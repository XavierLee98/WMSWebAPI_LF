using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WMSApp.Dtos;
using WMSWebAPI.Class;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GoodsReturnRequestController : ControllerBase
    {
        /// <summary>
        /// Share controller between goods return request and good return
        /// </summary>
        /// 
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030
        readonly string _dbNameMidware = "DatabaseFTMiddleware"; // 20200612T1030
        readonly IConfiguration _configuration;
        string _dbConnectionStr = string.Empty;
        string _dbConnectionStr_midware = string.Empty;

        string _lastErrorMessage = string.Empty;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public GoodsReturnRequestController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
            _dbConnectionStr_midware = _configuration.GetConnectionString(_dbNameMidware);
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
            try
            {
                _lastErrorMessage = string.Empty;
                switch (bag.request)
                {
                    case "GetGrnApInvList":
                        {
                            return GetGrnApInvList(bag);
                        }
                    case "GetOpenGRn":
                        {
                            return GetOpenGrn(bag);
                        }
                    case "GetOpenGrnLines":
                        {
                            return GetOpenGrnLines(bag);
                        }
                    case "GetOpenApInvoice":
                        {
                            return GetOpenApInvoice(bag);
                        }
                    case "GetOpenApInvoiceLines":
                        {
                            return GetOpenApInvoiceLines(bag);
                        }
                    case "CreateGoodsReturnRequest":
                        {
                            return CreateGoodsReturnRequest(bag);
                        }
                    case "GetDocSeries":
                        {
                            return GetDocSeries(bag);
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
        /// return the open ap invoice 
        /// and open grn 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGrnApInvList(Cio bag)
        {
            try
            {
                using var grn = new SQL_OPRR(_dbConnectionStr);
                var dtogrn = new DTO_OPRR
                {
                    OPDNs = grn.GetOpenGrn(bag),
                    OPCH_Exs = grn.GetOpenApInvoice(bag)
                };

                _lastErrorMessage = grn.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                return Ok(dtogrn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }


        /// <summary>
        /// Create Goods Return Request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateGoodsReturnRequest(Cio bag)
        {
            try
            {
                using var requestHelp = new InsertRequestHelper(_dbConnectionStr, _dbConnectionStr_midware);
                var result = requestHelp.CreateRequest(
                    bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);

                _lastErrorMessage = requestHelp.LastErrorMessage;
                if (string.IsNullOrWhiteSpace(_lastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(_lastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get the doc series
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetDocSeries(Cio bag)
        {
            try
            {
                using var prr = new SQL_OPRR(_dbConnectionStr);
                var dtoPPr = new DTO_OPRR { DocSeries = prr.GetDocSeries() };
                _lastErrorMessage = prr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoPPr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }


        /// <summary>
        /// get list if of the invoice line
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenApInvoiceLines(Cio bag)
        {
            try
            {
                using var grn = new SQL_OPRR(_dbConnectionStr);
                var dtogrn = new DTO_OPRR
                {
                    PCH1_Exs = grn.GetOpenApInvoiceLines(bag.PoDocEntries),
                    DocSeries = grn.GetDocSeries()
                };

                _lastErrorMessage = grn.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtogrn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Query the grn lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenGrnLines(Cio bag)
        {
            try
            {
                using var grn = new SQL_OPRR(_dbConnectionStr);
                var dtogrn = new DTO_OPRR
                {
                    PDN1s = grn.GetGrnLines(bag.PoDocEntries),
                    DocSeries = grn.GetDocSeries()
                };

                _lastErrorMessage = grn.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtogrn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get list of the open GRN 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenGrn(Cio bag)
        {
            try
            {
                using var grn = new SQL_OPRR(_dbConnectionStr);
                var dtoGrn = new DTO_OPRR { OPDNs = grn.GetOpenGrn(bag) };
                _lastErrorMessage = grn.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoGrn);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Return list of ap invoice 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenApInvoice(Cio bag)
        {
            try
            {
                using var grn = new SQL_OPRR(_dbConnectionStr);
                var dtoGrn = new DTO_OPRR
                {
                    OPCH_Exs = grn.GetOpenApInvoice(bag),
                    DocSeries = grn.GetDocSeries()
                };

                _lastErrorMessage = grn.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoGrn);
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
        /// <param name="message"></param>
        /// <param name="obj"></param>
        void Log(string message, Cio bag)
        {
            _logger?.LogError(message, bag);
            _fileLogger.WriteLog(message);
        }
    }
}
