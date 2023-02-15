using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GrpoController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030
        readonly string _dbNameMidware = "DatabaseFTMiddleware"; // 20200612T1030
        readonly IConfiguration _configuration;

        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public GrpoController(IConfiguration configuration, ILogger<GrpoController> logger)
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
                switch (bag.request)
                {
                    case "GetBpWithOpenPo":
                        {
                            return GetBpWithOpenPo(bag);
                        }
                    case "GetWarehouseList":
                        {
                            return GetWarehouseList(bag);
                        }
                    case "GetOpenPo":
                        {
                            return GetOpenPo(bag);
                        }
                    case "GetOpenPoLines":
                        {
                            return GetOpenPoLines(bag);
                        }
                    case "CreateGRPORequest":
                        {
                            return CreateGRPORequest(bag);
                        }
                    case "QueryVendor":
                        {
                            return QueryVendor(bag);
                        }
                    case "GetGRPODocSeries":
                        {
                            return GetGRPODocSeries(bag);
                        }
                    case "GetMachineList":
                        {
                            return GetMachineList(bag);
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

        IActionResult GetMachineList(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);
                var list = po.GetOOCRs();
                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }
                return Ok(list);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get GRPO Doc serial selection
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGRPODocSeries(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);
                var dtoGrpo = new DtoGrpo();
                dtoGrpo.GrpoDocSeries = po.GetDocSeries();
                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }
                return Ok(dtoGrpo);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Query vendor 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult QueryVendor(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);
                var dtoGrpo = new DtoGrpo { Vendors = po.GetVendor() };
                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }
                return Ok(dtoGrpo);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Return the result of Get Biz Partner With Open PO
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        IActionResult GetBpWithOpenPo(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);
                bag.PoBp = po.GetBpWithOpenPo();
                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }
                return Ok(bag);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Warehouse List
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetWarehouseList(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);
                bag.dtoWhs = po.GetWarehouses(); // load the warehouse here

                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }
                return Ok(bag);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        ///  return list of PO by card code array
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenPo(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);
                var dtoGrpo = new DtoGrpo 
                {
                    OPORs = po.GetOpenPo(bag)
                };                

                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }
                return Ok(dtoGrpo);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Based on the PO doc entries (int array)
        /// Query and return it line
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetOpenPoLines(Cio bag)
        {
            try
            {
                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
                using var po = new SQL_OPOR(_dbConnectionStr);

                var dtoGrpo = new DtoGrpo 
                { 
                    POR1s = po.GetOpenPoLines(bag.PoDocEntries),
                    GrpoDocSeries = po.GetDocSeries() 
                };

                if (!string.IsNullOrWhiteSpace(po.LastErrorMessage))
                {
                    return BadRequest(po.LastErrorMessage);
                }

                return Ok(dtoGrpo);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Handler insert the request for the middle ware to create the GRPO
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateGRPORequest(Cio bag)
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