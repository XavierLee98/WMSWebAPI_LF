using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WMSApp.Dtos;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.SAP_DiApi;
using WMSWebAPI.SAP_SQL;
using WMSWebAPI.SAP_SQL.PickList;
using Dapper;
using System.Data.SqlClient;
using WMSWebAPI.Models.PickList;
using System.Linq;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PickListController : Controller
    {
        readonly string _dbName = "DatabaseWMSConn"; 
        readonly string _dbNameMidware = "DatabaseFTMiddleware";

        readonly IConfiguration _configuration;
        ILogger _logger;

        FileLogger _fileLogger = new FileLogger();
        string _sapConnectionStr = string.Empty;
        string _dbMidwareConnectionStr = string.Empty;
        string _lastErrorMessage = string.Empty;

        public PickListController(IConfiguration configuration, ILogger<PickListController> logger)
        {
            _configuration = configuration;
            _sapConnectionStr = _configuration.GetConnectionString(_dbName);
            _dbMidwareConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
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
                    case "QueryDriver":
                        {
                            return GetDriver(bag);
                        }
                    case "QueryTruck":
                        {
                            return GetTrucks(bag);
                        }
                    case "QueryPicker":
                        {
                            return GetPicker(bag);
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
        /// Get PickWarehouse
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        //private IActionResult GetPickWarehouse(Cio bag)
        //{
        //    try
        //    {
        //        var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
        //        using var list = new SQL_OPKL(_dbMidwareConnectionStr);
        //        bag.dtoWhs = list.GetPickWarehouse();
        //        _lastErrorMessage = list.LastErrorMessage;
        //        if (_lastErrorMessage.Length>0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }
        //        return Ok(bag);
        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

        /// <summary>
        /// Get Picker
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetPicker(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_sapConnectionStr);
                var dtoPKL = new DTO_OPKL();
                dtoPKL.pickers = list.GetPickers();
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length>0)
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

        /// <summary>
        /// Get Truck
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetTrucks(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_sapConnectionStr);
                var dtoPKL = new DTO_OPKL();
                dtoPKL.trucks = list.GetTrucks(bag);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length>0)
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

        /// <summary>
        /// Get Driver
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetDriver(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_sapConnectionStr);
                var dtoPKL = new DTO_OPKL();
                dtoPKL.drivers = list.GetDrivers(bag);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length>0)
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


//    private IActionResult GetSO(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//            dtoPKL.oRDRs2 = list.GetSOs(bag);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (_lastErrorMessage == null)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok(dtoPKL);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }


//    private IActionResult CreateInvoiceRequest(Cio bag)
//    {
//        try
//        {
//            using var requestHelp = new InsertRequestHelper(_sapConnectionStr, _dbNameMidware);
//            var result = requestHelp.CreateRequest(
//                bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);

//            _lastErrorMessage = requestHelp.LastErrorMessage;
//            if (string.IsNullOrWhiteSpace(_lastErrorMessage))
//            {
//                return Ok(bag);
//            }
//            return BadRequest(_lastErrorMessage);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }




//    /// <summary>
//    /// Get PickList Details (Reserve Invoice)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult GetPickDetailsFromInvoice(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//            dtoPKL.pKL1_Exs = list.GetPKL1FromInvoice(bag.PickDoc);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (_lastErrorMessage==null)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok(dtoPKL);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }

//    /// <summary>
//    /// Create Pick List (Reserved Invoice)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    public IActionResult CreatePickListFromReservedInvoice(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//            using var list = new DiApiUpdatePickList();
//            int result = list.ReserveInvoiceToPickList(bag.InvoiceDoc);
//            _lastErrorMessage = list.LastErrorMessage;

//            if (result<0)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok();
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }


//    /// <summary>
//    /// Update Picked in Pick List
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult HandleUpdatePickList(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//            using var list = new DiApiUpdatePickList();

//            int result = list.PartialUpdatePickList(bag.PickDoc,bag.pKL1List, bag.PickHead);
//            _lastErrorMessage = list.LastErrorMessage;

//            if (result<0)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok();
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }

//    /// <summary>
//    /// Allocate Batch Item Quantity (SO)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult AssignBatchToSO(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//            using var list = new DiApiUpdatePickList();

//            int result = list.AssignBatchToSo(bag.PickItemLine, bag.oIBT, bag.Picked);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (result<0)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok();    
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }

///// <summary>
///// Get Available Batch Item
///// </summary>
///// <param name="bag"></param>
///// <returns></returns>
//private IActionResult GetAvailableBatchItem(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//            dtoPKL.oIBTs = list.getOIBTs(bag.ItemCodeInput);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (_lastErrorMessage==null)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok(dtoPKL);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }

//    /// <summary>
//    /// Get Single PickList
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult GetSingleOPKL(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//             dtoPKL.OPKL = list.GetSingleOPKL(bag.PickDoc);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (_lastErrorMessage==null)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok(dtoPKL);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }

//    /// <summary>
//    /// Get PickList Details (SO)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult GetPickDetailsFromSo(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//            dtoPKL.pKL1_Exs = list.GetPKL1FromSo(bag.PickDoc);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (_lastErrorMessage==null)
//            {
//                return BadRequest(_lastErrorMessage);
//            }
//            return Ok(dtoPKL);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }

//    /// <summary>
//    /// Get PickList (SO)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult GetPickList(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//            dtoPKL.OPKLs = list.GetOPKLLists(bag);
//            _lastErrorMessage = list.LastErrorMessage;
//             if (_lastErrorMessage==null)
//             {
//                 return BadRequest(_lastErrorMessage);
//             }
//            return Ok(dtoPKL);
//        }
//        catch (Exception excep)
//        {
//            Log($"{excep}", bag);
//            return BadRequest($"{excep}");
//        }
//    }