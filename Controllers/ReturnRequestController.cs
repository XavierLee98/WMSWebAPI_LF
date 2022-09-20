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
    public class ReturnRequestController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030
        readonly string _dbNameMidware = "DatabaseFTMiddleware"; // 20200612T1030
        readonly IConfiguration _configuration;
        string _dbConnectionStr = string.Empty;
        string _dbConnectionStr_midware = string.Empty;

        string _lastErrorMessage = string.Empty;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public ReturnRequestController(IConfiguration configuration, ILogger<GrpoController> logger)
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
                    case "GetDOArInvList":
                        {
                            return GetDOArInvList(bag);
                        }
                    case "GetArInvLines":
                        {
                            return GetArInvLines(bag);
                        }
                    case "GetOpenDOs":
                        {
                            return GetOpenDOs(bag);
                        }
                    case "GetOpenDoLines":
                        {
                            return GetOpenDoLines(bag);
                        }
                    case "GetOpenReturnRequest":
                        {
                            return GetOpenReturnRequest(bag);
                        }
                    case "CreateReturnRequest":
                        {
                            return CreateReturnRequest(bag);
                        }
                    case "GetDocSeries":
                        {
                            return GetDocSeries(bag);
                        }
                    case "GetDORreqList":
                        {
                            return GetDORreqList(bag);
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
        /// GetDOArInvList
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetDOArInvList(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORRR(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    ODLN_Exs = orrr.GetOpenDOs(bag),
                    OINV_Exs = orrr.GetOpenInvs(bag)
                };

                _lastErrorMessage = orrr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }

        }

        /// <summary>
        /// GetInvLines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetArInvLines(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORRR(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    INV1_Exs = orrr.GetArInvLines(bag),
                    DocSeries = orrr.GetDocSeries()
                };

                _lastErrorMessage = orrr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Base on give start and end 
        /// return open Do and open Return req
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetDORreqList(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORRR(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    ODLN_Exs = orrr.GetOpenDOs(bag),
                    ORRR_Exs = orrr.GetOpenReturnRequest(bag)
                };

                _lastErrorMessage = orrr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Return the doc series for return request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetDocSeries(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORRR(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    DocSeries = orrr.GetDocSeries()
                };

                _lastErrorMessage = orrr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }

        }

        /// <summary>
        /// Create Return Request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateReturnRequest(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                var _dbConnectionStrMw = _configuration.GetConnectionString(_dbNameMidware);

                using var requestHelp = new InsertRequestHelper(_dbConnectionStr, _dbConnectionStrMw);
                var result = requestHelp.CreateRequest(
                    bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);

                if (!string.IsNullOrWhiteSpace(requestHelp.LastErrorMessage))
                {
                    return BadRequest(requestHelp.LastErrorMessage);
                }
                return Ok(bag);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }

            //using var rrr = new SQL_ORRR(_dbConnectionStr, _dbConnectionStr_midware);
            //var result = rrr.CreateGoodsReturnRequest(bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);
            //_lastErrorMessage = rrr.LastErrorMessage;
            //if (_lastErrorMessage.Length > 0)
            //{
            //    return BadRequest(_lastErrorMessage);
            //}
            //return Ok(result);
        }


        /// <summary>
        /// Return list of DO line
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenDoLines(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORRR(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    DLN1_Exs = orrr.GetDoLines(bag),
                    DocSeries = orrr.GetDocSeries()
                };

                _lastErrorMessage = orrr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Return list of do 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenDOs(Cio bag)
        {
            try
            {
                using var rr = new SQL_ORRR(_dbConnectionStr);
                var dtoOrrr = new DTO_ORRR { ODLN_Exs = rr.GetOpenDOs(bag) };
                _lastErrorMessage = rr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoOrrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get list of the Return request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenReturnRequest(Cio bag)
        {
            try
            {
                using var rr = new SQL_ORRR(_dbConnectionStr);
                var dtoOrrr = new DTO_ORRR { ORRR_Exs = rr.GetOpenReturnRequest(bag) };
                _lastErrorMessage = rr.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(dtoOrrr);
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
