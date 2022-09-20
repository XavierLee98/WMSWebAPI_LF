using Dapper;
using DbClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Linq;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransferRequestController : ControllerBase
    { 
        IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        string _dbConnectionStr = string.Empty;
        string _dbConnectionStr_Mid = string.Empty;        
        string _MidDbName = string.Empty;

        string _lastErrorMessage = string.Empty;

        public TransferRequestController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString("DatabaseWMSConn");
            _dbConnectionStr_Mid = _configuration.GetConnectionString("DatabaseFTMiddleware");
            _MidDbName = _configuration.GetSection("AppSettings").GetSection("MiddlewareDbName").Value;
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
                    //case "GetTransferRequestList":
                    //    {
                    //        return GetTransferRequestList(bag);
                    //    }
                    //case "GetTransferRequestLine":
                    //    {
                    //        return GetTransferRequestLine(bag);
                    //    }
                    case "CheckItemCodeExistance":
                        {
                            return CheckItemCodeExistance(bag);
                        }
                    case "CreateInventoryRequest":
                        {
                            return CreateInventoryRequest(bag);
                        }
                    case "GetDocSeries":
                        {
                            return GetDocSeries(bag);
                        }
                        //case "QueryWhsQty":
                        //    {
                        //        return QueryWhsQty(bag);
                        //    }
                }
                return BadRequest($"Invalid request, please try again later. Thanks");
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        Cio QueryWhsQty(Cio bag)
        {
            try
            {
                var sql = @"Select * FROM FTS_vw_IMApp_OITW 
                           WHERE ItemCode=@ItemCode AND WhsCode=@WhsCode";

                using (var conn = new SqlConnection(_dbConnectionStr))
                {
                    OITW from_whs = conn.Query<OITW>(sql, new { ItemCode = bag.QueryItemCode, WhsCode = bag.QueryFromWhs }).FirstOrDefault();
                    bag.FromWhsItemAval = PrepareWhsAvailStr(bag.QueryFromWhs, from_whs, "From");

                    OITW to_whs = conn.Query<OITW>(sql, new { ItemCode = bag.QueryItemCode, WhsCode = bag.QueryToWhs }).FirstOrDefault();
                    bag.ToWhsItemAval = PrepareWhsAvailStr(bag.QueryToWhs, to_whs, "To");
                }
                return bag;
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return bag;
            }
        }

        string PrepareWhsAvailStr(string whsCode, OITW warehouse, string direction)
        {
            if (warehouse != null)
            {
                var avail = (warehouse.OnHand + warehouse.OnOrder) - warehouse.IsCommited;
                return $"{direction} {warehouse.WhsCode}, Avail {avail:N2}";
            }
            else
            {
                return $"{direction} {whsCode}, Avail 0.00";
            }
        }

        /// <summary>
        /// Get the inventory transfer request doc series 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetDocSeries(Cio bag)
        {
            try
            {
                using (var request = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidDbName))
                {
                    var dtoOwtq = new DtoOwtq();
                    dtoOwtq.DocSeries = request.GetDocSeries();

                    _lastErrorMessage = request.LastErrorMessage;
                    if (string.IsNullOrWhiteSpace(_lastErrorMessage))
                        return Ok(dtoOwtq);
                    return BadRequest(_lastErrorMessage);
                }
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }

        }

        /// <summary>
        /// Handler create inventpry request
        /// </summary>
        /// <returns></returns>
        IActionResult CreateInventoryRequest(Cio bag)
        {
            using (var inventoryRequest = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidDbName))
            {
                var result = inventoryRequest.CreateInventoryRequest_Midware(
                    bag.dtoRequest, bag.dtoInventoryRequest,
                    bag.dtoInventoryRequestHead, bag.dtozmwDocHeaderField);

                _lastErrorMessage = inventoryRequest.LastErrorMessage;
            }

            if (string.IsNullOrWhiteSpace(_lastErrorMessage))
            {
                return Ok(bag);
            }

            return BadRequest(_lastErrorMessage);
        }

        /// <summary>
        /// Check the item code exist in sap database
        /// </summary>
        /// <returns></returns>
        IActionResult CheckItemCodeExistance(Cio bag)
        {
            try
            {
                using (var oitm = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidDbName))
                {
                    bag.Item = oitm.CheckItemCodeExist(bag.QueryItemCode);
                    QueryWhsQty(bag);
                    _lastErrorMessage = oitm.LastErrorMessage;
                }

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
        /// Get Transfer RequestList
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        //IActionResult GetTransferRequestList(Cio bag)
        //{
        //    try
        //    {
        //        using (var grpo = new SQL_OWTQ(_dbConnectionStr))
        //        {
        //            bag.TransferRequestList = grpo.GetTransferRequestList();
        //            _lastErrorMessage = grpo.LastErrorMessage;
        //        }

        //        if (string.IsNullOrWhiteSpace(_lastErrorMessage))
        //        {
        //            return Ok(bag);
        //        }

        //        return BadRequest(_lastErrorMessage);
        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        //IActionResult GetTransferRequestLine(Cio bag)
        //{
        //    try
        //    {
        //        using (var grpo = new SQL_OWTQ(_dbConnectionStr))
        //        {
        //            bag.TransferRequestLine = grpo.GetTransferRequestLines(bag.TransferRequestDocEntry);
        //            _lastErrorMessage = grpo.LastErrorMessage;
        //        }

        //        if (string.IsNullOrWhiteSpace(_lastErrorMessage))
        //        {
        //            return Ok(bag);
        //        }

        //        return BadRequest(_lastErrorMessage);
        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

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
