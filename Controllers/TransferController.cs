﻿using DbClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WMSWebAPI.Class;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    // 20200920T1321 
    [Route("[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        string _dbConnectionStr = string.Empty;
        string _dbConnectionStr_Mid = string.Empty;
        string _MidwareDbName = string.Empty;
        string _lastErrorMessage = string.Empty;

        public TransferController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString("DatabaseWMSConn");
            _dbConnectionStr_Mid = _configuration.GetConnectionString("DatabaseFTMiddleware");
            _MidwareDbName = _configuration.GetSection("AppSettings").GetSection("MiddlewareDbName").Value;
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
                    case "GetTransferRequestList":
                        {
                            return GetTransferRequestList(bag);
                        }
                    case "GetTransferRequestLine":
                        {
                            return GetTransferRequestLine(bag);
                        }
                    case "CheckItemCodeExistance":
                        {
                            return CheckItemCodeExistance(bag);
                        }
                    case "CheckItemCodeAndWarehouseQty":
                        {
                            return CheckItemCodeAndWarehouseQty(bag);
                        }
                    case "GetItemWhsBin":
                        {
                            return GetItemWhsBin(bag); // 20201002
                        }
                    case "GetItemWhsBinBatch": 
                        {
                            return GetItemWhsBinBatch(bag); // 20201002
                        }
                    case "GetItemWhsBinSerial": 
                        {
                            return GetItemWhsBinSerial(bag); // 20201002
                        }
                    case "GetItemWhsBatch":
                        {
                            return GetItemWhsBatch(bag); // 20201002 - 2
                        }
                    case "GetItemWhsSerial":
                        {
                            return GetItemWhsSerial(bag); // 20201002 - 2
                        }
                    case "GetWarehouseBins":
                        {
                            return GetWarehouseBins(bag);
                        }
                    case "CreateTransfer":
                        {
                            return CreateTransfer(bag);
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
        /// Get list of the warehouse bin
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetWarehouseBins (Cio bag)
        {
            try
            {
                using (var transfer = new SQL_OWTR(_dbConnectionStr))
                {
                    bag.DtoBins = transfer.GetWarehouseBin(bag.QueryWhs);
                    _lastErrorMessage = transfer.LastErrorMessage;
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
        /// Request Middleware to create transfer document
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateTransfer(Cio bag)
        {
            try
            {
                using (var transfer = new SQL_OWTR(_dbConnectionStr))
                {
                    /// zwaTransferDetails[] dtoTransferDetails { get; set; }
                    //zwaTransferHead head { get; set; }

                    var result = transfer.CreateTransferRequest(bag.dtoRequest, bag.dtoDetailsBins ,
                        bag.dtoTransferDetails,bag.dtoTransferHead);
                    _lastErrorMessage = transfer.LastErrorMessage;
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
        /// Get the list of item in a warehouse with bin information 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemWhsBin(Cio bag)
        {
            try
            {
                using (var ItemWhsBin = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.CommonStockInfos = ItemWhsBin.GetItemWhsBin(bag.QueryItemCode, bag.QueryItemWhsCode);
                    _lastErrorMessage = ItemWhsBin.LastErrorMessage;
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
        /// Return all item and warehouse serial list for user selection
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemWhsSerial (Cio bag)
        {
            try
            {
                using (var ItemWhsBin = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.CommonStockInfos = ItemWhsBin.GetItemWhsSerial(bag.QueryItemCode, bag.QueryItemWhsCode);
                    _lastErrorMessage = ItemWhsBin.LastErrorMessage;
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
        /// Return all item and warehouse batch list for user selection
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemWhsBatch (Cio bag)
        {
            try
            {
                using (var ItemWhsBin = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.CommonStockInfos = ItemWhsBin.GetItemWhsBatch(bag.QueryItemCode, bag.QueryItemWhsCode);
                    _lastErrorMessage = ItemWhsBin.LastErrorMessage;
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
        /// GetItem Whs Bin Serial
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemWhsBinSerial(Cio bag)
        {
            try
            {
                using (var ItemWhsBin = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.CommonStockInfos = ItemWhsBin.GetItemWhsBinSerial(bag.QueryItemCode, bag.QueryItemWhsCode);
                    _lastErrorMessage = ItemWhsBin.LastErrorMessage;
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
        /// GetItem Whs Bin Serial
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemWhsBinBatch(Cio bag)
        {
            try
            {
                using (var ItemWhsBin = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.CommonStockInfos = ItemWhsBin.GetItemWhsBinBatch(bag.QueryItemCode, bag.QueryItemWhsCode);
                    _lastErrorMessage = ItemWhsBin.LastErrorMessage;
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
        /// Check item in dedicated warehouse with available qty
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CheckItemCodeAndWarehouseQty(Cio bag)
        {
            try
            {
                using (var OITM = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.oITW = OITM.CheckItemCodeQtyInWarehouse(bag.QueryItemCode, bag.QueryItemWhsCode);
                    _lastErrorMessage = OITM.LastErrorMessage;
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
        /// Check the item code exist in sap database
        /// </summary>
        /// <returns></returns>
        IActionResult CheckItemCodeExistance(Cio bag)
        {
            try
            {
                using (var OITM = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.Item = OITM.CheckItemCodeExist(bag.QueryItemCode);
                    _lastErrorMessage = OITM.LastErrorMessage;
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
        IActionResult GetTransferRequestList(Cio bag)
        {
            try
            {
                using (var grpo = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    if (bag.RequestTransferDocFilter.Equals("h"))
                    {
                        bag.dtozwaHoldRequests = grpo.GetHoldRequest(bag);
                    }
                    else
                    {
                        bag.TransferRequestList = grpo.GetTransferRequestList(bag);                        
                    }
                    _lastErrorMessage = grpo.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(_lastErrorMessage)) 
                    return Ok(bag);                
                
                return BadRequest(_lastErrorMessage);
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
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetTransferRequestLine(Cio bag)
        {
            try
            {
                using (var grpo = new SQL_OWTQ(_dbConnectionStr, _dbConnectionStr_Mid, _MidwareDbName))
                {
                    bag.TransferRequestLine = grpo.GetTransferRequestLines(bag.TransferRequestDocEntry);
                    _lastErrorMessage = grpo.LastErrorMessage;
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
