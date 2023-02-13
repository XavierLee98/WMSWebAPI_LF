using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Transfer1Controller : ControllerBase
    {
        readonly IConfiguration _configuration;
        readonly IWebHostEnvironment _hostingEnv;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();
        string _dbConnectionStr = string.Empty;
        string _dbMidwareConnectionStr = string.Empty;
        string _midwareDbName = string.Empty;

        [Obsolete]
        public Transfer1Controller(IConfiguration configuration, ILogger<GrpoController> logger, IWebHostEnvironment enviroment)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString("DatabaseWMSConn");
            _dbMidwareConnectionStr = _configuration.GetConnectionString("DatabaseFTMiddleware");
            _midwareDbName = _configuration.GetSection("AppSettings").GetSection("MiddlewareDbName").Value;

            _logger = logger;
            _hostingEnv = enviroment;
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
                    case "QueryCode":
                        {
                            return QueryCode(bag);
                        }
                    case "QuerySerialNum":
                        {
                            return QuerySerialNum(bag);
                        }
                    case "QueryBatchNum":
                        {
                            return QueryBatchNum(bag);
                        }
                    case "QueryItemBinAccumulator":
                        {
                            return QueryItemBinAccumulator(bag);
                        }
                    case "CheckItemSerialExistInWhs":
                        {
                            return CheckItemSerialExistInWhs(bag);
                        }
                    case "CheckItemBatchExistInWhs":
                        {
                            return CheckItemBatchExistInWhs(bag);
                        }
                    case "GetBinSerialAccumulators": // get list of bin and serial#
                        {
                            return GetBinSerialAccumulators(bag);
                        }
                    case "GetSerialInWhs": // get list of serial# in whs
                        {
                            return GetSerialInWhs(bag);
                        }
                    case "GetBinBatchAccumulators": // get list of bin batch cotent
                        {
                            return GetBinBatchAccumulators(bag);
                        }
                    case "GetBatchInWhs": // get list of batch# in whs
                        {
                            return GetBatchInWhs(bag);
                        }
                    case "GetItemWhsBins":
                        {
                            return GetItemWhsBins(bag);
                        }
                    case "SaveRequestOnHold": // save the transfer 1 from warehouse
                        {
                            return SaveRequestOnHold(bag);
                        }
                    case "CreateTransfer": // based on put in warehouse qty to create transfer
                        {
                            return CreateTransfer(bag);
                        }                    
                    case "GetSerialsList": // handle the to warehouse operation
                        {
                            return GetSerialsList(bag);
                        }

                    case "GetWhsBins": // get list of bin by warehouse
                        {
                            return GetWhsBins(bag);
                        }
                    // batch
                    case "GetBatchsList":
                        {
                            return GetBatchsList(bag);
                        }
                    case "GetBatchesList":
                        {
                            return GetBatchesList(bag);
                        }
                    // Query the request line guid 
                    case "GetTransferRequestFromGuids":
                        {
                            return GetTransferRequestFromGuids(bag);
                        }
                    case "GetWarehouseItemQty":
                        {
                            return GetWarehouseItemQty(bag);
                        }
                        // save the 
                    case "SaveStandAloneTransferOnHold":
                        {
                            return SaveStandAloneTransferOnHold(bag);
                        }
                    case "LoadSTADocList": 
                        {
                            return LoadSTADocList(bag);
                        }
                    case "LoadSTADocLines": // 202011011112
                        {
                            return LoadSTADocLines(bag);
                        }
                    case "GetPickedRequestLines":
                        {
                            return GetPickedRequestLines(bag);
                        }
                    case "GetDocSeries":
                        {
                            return GetDocSeries(bag); //67
                        }
                    case "GetItemList":
                        {
                            return GetItemList(bag);
                        }
                    case "RemoveOnHold":
                        {
                            return RemoveOnHold(bag);
                        }
                }

                // sample code for modern switch
                //return bag.request switch
                //{
                //    "LoadSTADocList" => LoadSTADocList(bag),
                //    _ => BadRequest($"Invalid request, please try again later. Thanks");
                //};

                return BadRequest($"Invalid request, please try again later. Thanks");
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        IActionResult RemoveOnHold (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                int result  = transfer1.RemoveOnHold(bag.checkDocGuid);
                
                if (result == -1)
                {
                    return BadRequest(transfer1.LastErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get the requested line for item 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemList (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferDocDetailsBins = transfer1.GetItemList(bag);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get the inventory transfer Doc series
        /// </summary>
        /// <returns></returns>
        IActionResult GetDocSeries (Cio bag)
        {
            try
            {
                using (var transfer = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName))
                {
                    var dtoOWstr = new DtoOwtq { DocSeries = transfer.GetDocSeries() };
                    if (string.IsNullOrWhiteSpace(transfer.LastErrorMessage))
                    {
                        return Ok(dtoOWstr);
                    }                        
                    return BadRequest(transfer.LastErrorMessage);
                }
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// get the detail line and bin information from the based on selected guid
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetPickedRequestLines (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr,_dbMidwareConnectionStr, _midwareDbName);
                bag.dtozmwTransferDocDetails = transfer1.GetPickedRequestLines(bag.TransferDocRequestGuid); 
                bag.dtozwaTransferDocDetailsBin = transfer1.GetPickedRequestLinesBins(bag.TransferDocRequestGuid);
                bag.dtoPriceList = transfer1.GetTransferPriceList();

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Load list STA doc list (stand alone transfer)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult LoadSTADocList (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.dtozwaHoldRequests = transfer1.LoadSTADocList(); // save the standalone transfer

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Load the from whr line from the table
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult LoadSTADocLines(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.dtozmwTransferDocDetails = transfer1.LoadSTADocLines(bag); // save the standalone transfer
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Save the stand alone transfer
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult SaveStandAloneTransferOnHold (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                int result = transfer1.SaveStandAloneTransferOnHold(bag); // save the standalone transfer
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Return the OITW object for the item qty
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetWarehouseItemQty (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.oITW = transfer1.GetWarehouseItemQty(bag.QueryItemWhsCode, bag.QueryItemCode);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Transfer Request FromGuids
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetTransferRequestFromGuids (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferDocDetail = transfer1.GetTransferRequesFromGuids(bag);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Batches List for non bin warehouse receipt acknoledgement
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBatchesList (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferDocDetailsBins = transfer1.GetBatchesList(bag);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get list of the picked batch
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBatchsList(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);                
                bag.TransferDocDetailsBins = transfer1.GetBatchsList(bag);

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// get list of the bin from warehouse
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetWhsBins (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.WarehouseBin = transfer1.GetWhsBins(bag.QueryWhs);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// return list of the serial number
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetSerialsList (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferDocDetailsBins = transfer1.GetSerialsList(bag);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Save request to create transfer document
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateTransfer(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                int result = transfer1.CreateTransfer_Midware(bag); // save the request 

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Save Request On Hold
        /// Await driver to delivery door step, and people scan received.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult SaveRequestOnHold(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                int result = transfer1.SaveRequestOnHold(bag); // save the request 

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// get list of item in whs bin
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItemWhsBins (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferBinItems = transfer1
                    .GetItemWhsBins(bag.TransferItemCode, bag.TransferWhsCode);

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get list of the batch in warehouse
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBatchInWhs (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferContentBatch = transfer1
                    .GetBatchInWhs(bag.TransferItemCode, bag.TransferWhsCode);

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Bin Batch Accumulators
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBinBatchAccumulators (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferBinContentBatch = transfer1
                    .GetBinBatchAccumulators(bag.TransferItemCode, bag.TransferWhsCode);

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get list of the serial number from the warehouse
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetSerialInWhs (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferContentSerial = transfer1
                    .GetSerialInWhs(bag.TransferItemCode, bag.TransferWhsCode);

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// 20201018
        /// Return list of the bin + serial for user to select
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBinSerialAccumulators (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferBinContentSerial = transfer1
                    .GetBinSerialAccumulators(bag.TransferItemCode, bag.TransferWhsCode);

                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }
 
        /// <summary>
        /// Check Item Batch Exist In Whs
        /// </summary>
        /// <returns></returns>
        IActionResult CheckItemBatchExistInWhs (Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferBatch = transfer1.CheckBatchWhs(bag.TransferQueryCode, bag.TransferWhsCode, bag.TransBatchAbs);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Check Item Serial Exist In Whs
        /// </summary>
        /// <returns></returns>
        IActionResult CheckItemSerialExistInWhs(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferOSRI = transfer1.CheckSerialWhs(bag.TransferQueryCode, bag.TransferWhsCode, bag.TransSerialCode);
                if (string.IsNullOrWhiteSpace(transfer1.LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(transfer1.LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Query to get item acculautor
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult QueryItemBinAccumulator(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                bag.TransferBinAccumulator = transfer1.GetItemBinAccumulator
                            (bag.TransferItemCode,
                            bag.TransferFoundBin.AbsEntry,
                            bag.TransferFoundBin.WhsCode);

                return Ok(bag);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Query the batch number object
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult QueryBatchNum(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);

                // check code is serial
                var isBatch = transfer1.IsBatchNum(bag.TransferQueryCode);
                if (isBatch != null) // found code is batch code
                {
                    bag.TransferFoundItem = transfer1.IsItemCode(isBatch.ItemCode);
                    bag.TransferFoundBatch = isBatch;
                    bag.TransferBinBatchAccumulator = transfer1.GetBinBatchAccumulator
                                (bag.TransferItemCode,
                                bag.TransferFoundBin.AbsEntry,
                                bag.TransferFoundBin.WhsCode,
                                bag.TransferQueryCode);

                    return Ok(bag);
                }

                return NotFound("The input serial# no found/ not recognized.");
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Query the serial number object
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult QuerySerialNum(Cio bag)
        {
            try
            {
                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);

                // check code is serial
                var isSerial = transfer1.IsSerialNum(bag.TransferQueryCode);
                if (isSerial != null) // found code is batch code
                {
                    bag.TransferFoundItem = transfer1.IsItemCode(isSerial.ItemCode);
                    bag.TransferFoundSerial = isSerial;
                    bag.TransferBinSerialAccumulator = transfer1.GetBinSerialAccumulator
                                (bag.TransferItemCode,
                                bag.TransferFoundBin.AbsEntry,
                                bag.TransferFoundBin.WhsCode,
                                bag.TransferQueryCode);
                    return Ok(bag);
                }

                return NotFound("The input serial# no found/ not recognized.");
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// A capture code from user 
        /// Indentified the given code is item code, bin code or batch or serial
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult QueryCode(Cio bag)
        {
            try
            {
                // testing 
                //using (var v = new LabelGenerator(_hostingEnv))
                //{
                //    var isOkay = v.GenerateQrCode(bag.TransferQueryCode, @"test.png");
                //    Console.WriteLine(isOkay);
                //}

                using var transfer1 = new SQL_QueryCode(_dbConnectionStr, _dbMidwareConnectionStr, _midwareDbName);
                // check is bin code
                var isBinCode = transfer1.IsBinCode(bag.TransferQueryCode);
                if (isBinCode != null)
                {
                    bag.TransferFoundBin = isBinCode;
                    return Ok(bag);
                }

                // check code is Batch
                var isBatch = transfer1.IsBatchNum(bag.TransferQueryCode);
                if (isBatch != null) // found code is batch code
                {
                    bag.TransferFoundItem = transfer1.IsItemCode(isBatch.ItemCode);
                    bag.TransferFoundBatch = isBatch;
                    return Ok(bag);
                }

                // check code is serial
                var isSerial = transfer1.IsSerialNum(bag.TransferQueryCode);
                if (isSerial != null) // found code is batch code
                {
                    bag.TransferFoundItem = transfer1.IsItemCode(isSerial.ItemCode);
                    bag.TransferFoundSerial = isSerial;
                    return Ok(bag);
                }

                var isItem = transfer1.IsItemCode(bag.TransferQueryCode);
                if (isItem != null) // found code is batch code
                {
                    bag.TransferFoundItem = isItem;
                    return Ok(bag);
                }
                return NotFound($"The input code -> {bag.TransferQueryCode} no found/ not recognized.");
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
