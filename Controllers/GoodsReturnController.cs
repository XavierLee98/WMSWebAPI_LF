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
    public class GoodsReturnController : ControllerBase
    {
        /// <summary>
        /// Share controller between goods return request and good return
        /// </summary>
        /// 
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030
        readonly string _dbNameMidware = "DatabaseFTMiddleware"; // 20200612T1030
        readonly IConfiguration _configuration;        
        string _lastErrorMessage = string.Empty;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public GoodsReturnController(IConfiguration configuration, ILogger<GrpoController> logger)
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
            try
            {
                _lastErrorMessage = string.Empty;
                switch (bag.request)
                {
                    case "GetGrnGrrReqList":
                        {
                            return GetGrnGrrReqList(bag); // return GRR and GRPO
                        }
                    case "GetOpenGoodsRequest": // good return request doc
                        {
                            return GetOpenGoodsRequest(bag); // get open goods return line
                        }
                    case "GetOpenGrrLines":
                        {
                            return GetOpenGrrLines(bag);
                        }
                    case "GetGrpoLines":
                        {
                            return GetGrpoLines(bag);
                        }
                    case "CreateGoodsReturn":
                        {
                            return CreateGoodsReturn(bag);
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
        /// Get Grn Grr Req List
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGrnGrrReqList (Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var grr = new SQL_ORPD(_dbConnectionStr);

                var dtoGrr = new DTO_OPRR 
                { 
                    OPRR_Exs = grr.GetOpenGoodsReturnRequest(bag) ,
                    OPDNs = grr.GetOpenGrn(bag)
                };

                _lastErrorMessage = grr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                return Ok(dtoGrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get grpo lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGrpoLines (Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var grr = new SQL_ORPD(_dbConnectionStr);
                var DtoOPRR = new DTO_OPRR
                {
                    PDN1s = grr.GetOpenGrpoLines(bag.PoDocEntries),
                    DocSeries = grr.GR_GetDocSeries()
                };

                _lastErrorMessage = grr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(DtoOPRR);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Bag"></param>
        /// <returns></returns>
        IActionResult GetOpenGrrLines(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var grr = new SQL_ORPD(_dbConnectionStr);
                var DtoOPRR = new DTO_OPRR
                {
                    PRR1_Exs = grr.GetOpenPrrLines(bag.PoDocEntries),
                    DocSeries = grr.GR_GetDocSeries()
                };

                _lastErrorMessage = grr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(DtoOPRR);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Return list of goods return request
        /// </summary>
        /// <param name="Bag"></param>
        /// <returns></returns>
        IActionResult GetOpenGoodsRequest(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var grr = new SQL_ORPD(_dbConnectionStr);
                var dtoGrr = new DTO_OPRR { OPRR_Exs = grr.GetOpenGoodsReturnRequest(bag) };
                _lastErrorMessage = grr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoGrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        

        /// <summary>
        /// Create good return for midware
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateGoodsReturn(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                var _dbConnectionStr_midware = _configuration.GetConnectionString(_dbNameMidware);

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
