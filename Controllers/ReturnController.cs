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
    public class ReturnController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030        
        readonly IConfiguration _configuration;

        string _dbConnectionStr { get; set; } = string.Empty;
        ILogger _logger { get; set; } = null;
        FileLogger _fileLogger { get; set; } = new FileLogger();

        public ReturnController(IConfiguration configuration, ILogger<GrpoController> logger)
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
                case "GetDORreqList":
                    {
                        return GetDORreqList(bag);
                    }
                case "GetOpenReturnRequestLines":
                    {
                        return GetOpenReturnRequestLines(bag);
                    }
                case "GetOpenDoLines":
                    {
                        return GetOpenDeliveryOrderLines(bag);
                    }
                case "GetDocSeries":
                    {
                        return GetDocSeries(bag);
                    }
                case "CreateReturn":
                    {
                        return CreateReturn(bag);
                    }
                case "GetSerialInDo":
                    {
                        return GetSerialInDo(bag);
                    }
                case "GetBatchesInDo":
                    {
                        return GetBatchesInDo(bag);
                    }
                default:
                    {
                        return BadRequest($"Invalid request, please try again later. Thanks");
                    }
            }
        }

        /// <summary>
        /// Get Batches In Do
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBatchesInDo(Cio bag)
        {
            try
            {
                using var rrr = new SQL_ORDN(_dbConnectionStr);
                var result = rrr.GetBatchInDo(bag.QueryDocEntry, bag.QueryDocLineNum);

                if (rrr.LastErrorMessage.Length > 0) return BadRequest(rrr.LastErrorMessage);
                return Ok(result); // batch object in list
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Serial In Do
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetSerialInDo(Cio bag)
        {
            try
            {
                using var rrr = new SQL_ORDN(_dbConnectionStr);
                var result = rrr.GetSerialInDo(bag.QueryDocEntry, bag.QueryDocLineNum);

                if (rrr.LastErrorMessage.Length > 0) return BadRequest(rrr.LastErrorMessage);
                return Ok(result); // batch object in list
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Save to mid ware for create the return
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateReturn(Cio bag)
        {
            try
            {
                const string _dbNameMidware = "DatabaseFTMiddleware"; // 20200612T1030

                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                var _dbConnectionStrMw = _configuration.GetConnectionString(_dbNameMidware);

                using var requestHelp = new InsertRequestHelper(_dbConnectionStr, _dbConnectionStrMw);
                var result = requestHelp.CreateRequest(
                    bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);

                if (requestHelp.LastErrorMessage.Length > 0) return BadRequest(requestHelp.LastErrorMessage);

                return Ok(bag);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Doc series
        /// </summary>
        /// <returns></returns>
        IActionResult GetDocSeries(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORDN(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR { DocSeries = orrr.GetDocSeries() };

                if (orrr.LastErrorMessage.Length > 0) return BadRequest(orrr.LastErrorMessage);
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// return list of the DO line
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenDeliveryOrderLines(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORDN(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    DLN1_Exs = orrr.GetOpenDeliveryOrderLines(bag),
                    DocSeries = orrr.GetDocSeries()
                };
               
                if (orrr.LastErrorMessage.Length > 0) return BadRequest(orrr.LastErrorMessage);
                return Ok(dtoRrr);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Open return req lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenReturnRequestLines(Cio bag)
        {
            try
            {
                using var orrr = new SQL_ORDN(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    RRR1_Exs = orrr.GetOpenReturnRequestLines(bag),
                    DocSeries = orrr.GetDocSeries()
                };

                if (orrr.LastErrorMessage.Length > 0) return BadRequest(orrr.LastErrorMessage);
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
                using var orrr = new SQL_ORDN(_dbConnectionStr);
                var dtoRrr = new DTO_ORRR
                {
                    ODLN_Exs = orrr.GetOpenDOs(bag),
                    ORRR_Exs = orrr.GetOpenReturnRequest(bag)
                };

                if (orrr.LastErrorMessage.Length > 0) return BadRequest(orrr.LastErrorMessage);
                return Ok(dtoRrr);
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
