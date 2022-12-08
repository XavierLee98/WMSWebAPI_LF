using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.SAP_DiApi;
using WMSWebAPI.SAP_SQL.PickList;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SOPickListController : Controller
    {
        readonly string _dbName = "DatabaseWMSConn";
        readonly string _dbNameMidware = "DatabaseFTMiddleware";

        readonly IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();
        SAPCompany _company;
        string _sapConnectionStr = string.Empty;
        string _dbMidwareConnectionStr = string.Empty;
        string _lastErrorMessage = string.Empty;

        public SOPickListController(IConfiguration configuration, ILogger<PickListController> logger, SAPCompany company)
        {
            _configuration = configuration;
            _sapConnectionStr = _configuration.GetConnectionString(_dbName);
            _dbMidwareConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
            _logger = logger;
            _company = company;
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
                    case "GetAllPickListAndWarehouse":
                        {
                            return GetAllPickListAndWarehouse(bag);
                        }
                    case "GetAllPickList":
                        {
                            return GetPickList(bag);
                        }
                    case "GetPickDetails":
                        {
                            return GetPickDetails(bag);
                        }
                    case "GetBatchItem":
                        {
                            return GetBatchItem(bag);
                        }
                    case "AssignBatchToSO":
                        {
                            return AssignBatchToSO(bag);
                        }
                    case "UpdatePickList":
                        {
                            return UpdatePickList(bag);
                        }
                    case "HandleUpdatePickListHeader":
                        {
                            return HandleUpdatePickListHeader(bag);
                        }
                    case "RemoveSingleBatchAllocation":
                        {
                            return RemoveSingleBatchAllocation(bag);
                        }
                    case "UpdateValidateItemConfiguration":
                        {
                            return UpdateValidateItemConfiguration(bag);
                        }
                    case "GetConfigureItemValidation":
                        {
                            return GetConfigureItemValidation(bag);
                        }
                    case "ResetPicker":
                        {
                            return ResetPicker(bag);
                        }
                    case "InsertBatchVariance":
                        {
                            return InsertBatchVariance(bag);
                        }
                    case "UpdatePicker":
                        {
                            return UpdatePicker(bag);
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
        /// Cancel Allocation for single Batch Item (SO)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        //private IActionResult RemoveAllBatchesForSingleItem(Cio bag)
        //{
        //    try
        //    {
        //        var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
        //        using var list = new DiApiUpdatePickList(_configuration,_dbMidwareConnectionStr,_company);
        //        using var sqllist = new SQL_OPKL(_dbMidwareConnectionStr);

        //        int result = list.SORemoveAllBatchesForSingleItem(bag.PickItemLine);
        //        _lastErrorMessage = list.LastErrorMessage;
        //        if (result < 0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }
        //        result = sqllist.DeleteBatchVarianceForSingleItem(bag.PickItemLine, bag.oBTQs);
        //        _lastErrorMessage = sqllist.LastErrorMessage;
        //        if (result < 0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }
        //        result = sqllist.RemoveMultiBatch(bag.PickItemLine, bag.oBTQs);
        //        _lastErrorMessage = sqllist.LastErrorMessage;
        //        if (result < 0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }
        //        return Ok();
        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

        /// <summary>
        /// Update Pick List Header
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult HandleUpdatePickListHeader(Cio bag)
        {
            try
            {
                using var list = new DiApiUpdatePickList(_configuration, _dbMidwareConnectionStr,_company);

                int result = list.UpdatePickListHeader( bag.PickHead);
                _lastErrorMessage = list.LastErrorMessage;

                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// Remove Allocation Batch Item (SO)
        //private IActionResult HandleRemoveSOBatchAllocation(Cio bag)
        //{
        //    try
        //    {
        //        var _sapConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
        //        using var list = new DiApiUpdatePickList(_configuration, _dbMidwareConnectionStr,_company);
        //        using var midwarelist = new SQL_OPKL(_sapConnectionStr, _dbMidwareConnectionStr);

        //        int result = list.RemoveBatchAllocationSOPickList(bag.pKL1List);
        //        _lastErrorMessage = list.LastErrorMessage;
        //        if (result < 0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }
        //        result = midwarelist.DeleteBatchVarianceForAllItem(bag.pKL1List);
        //        _lastErrorMessage = midwarelist.LastErrorMessage;
        //        if (result < 0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }
        //        result = midwarelist.RemoveAllBatchesforPickList(bag.pKL1List);
        //        _lastErrorMessage = midwarelist.LastErrorMessage;
        //        if (result < 0)
        //        {
        //            return BadRequest(_lastErrorMessage);
        //        }

        //        return Ok();
        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

        /// <summary>
        /// Cancel Allocation for single Batch Item (SO)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult RemoveSingleBatchAllocation (Cio bag)
        {
            try
            {
                using var list = new DiApiUpdatePickList(_configuration, _dbMidwareConnectionStr,_company);
                using var sqllist = new SQL_OPKL(_dbMidwareConnectionStr);

                int result = list.SOCancelAssignSingleBatch(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = list.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                result = sqllist.DeleteBatchVariance(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = list.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                result = sqllist.RemoveHoldingSingleBatch(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = sqllist.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Allocate Batch Item (SO)
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult AssignBatchToSO(Cio bag)
        {
            try
            {
                using var diapilist = new DiApiUpdatePickList(_configuration, _dbMidwareConnectionStr, _company);
                using var sqllist = new SQL_OPKL(_dbMidwareConnectionStr);
                int result = -1;

                result = diapilist.AssignBatchToSo(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = diapilist.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                result = sqllist.InsertHoldingBatch(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = sqllist.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Available Batch Item
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetBatchItem(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);
                var batches = list.GetAvailableBatches(bag.ItemCodeInput,bag.QueryWhs);
                _lastErrorMessage = list.LastErrorMessage;

                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(batches);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Pick Lines
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetPickDetails(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);

                var dto = new DTO_OPKL();

                dto.pKL1s = list.GetPickDetails(bag.PickHead.AbsEntry);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                dto.IsEnableItemValidate = list.GetValidateItemConfiguration();
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                var tempdto = list.GetPickHeadProperties(bag);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                dto.picker = tempdto.picker;
                dto.driver = tempdto.driver;
                dto.truck = tempdto.truck;

                return Ok(dto);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep.Message}");
            }
        }

        /// <summary>
        /// Get All SO PickList
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetPickList(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);
                var OPKLs = list.GetOPKLLists(bag.QueryStartDate,bag.QueryEndDate, bag.QueryWhs);
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(OPKLs);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }
        private IActionResult GetAllPickListAndWarehouse(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr,_sapConnectionStr);
                var dtoPKL = new DTO_OPKL();

                dtoPKL.WarehouseList = list.GetPickWarehouse();
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                var firstWhs = dtoPKL.WarehouseList.FirstOrDefault();

                dtoPKL.OPKLs = list.GetOPKLLists(bag.QueryStartDate, bag.QueryEndDate, firstWhs.WhsCode);
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

        /// <summary>
        /// Update Picker
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult UpdatePicker(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);

                int result = list.UpdatePicker(bag.QueryPicker, bag.PickHead);
                _lastErrorMessage = list.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        //private IActionResult GetItemdetails(Cio bag)
        //{
        //    try
        //    {
        //        var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
        //        using var list = new SQL_OPKL(_sapConnectionStr);
        //        var dtoPKL = new DTO_OPKL();
        //        bag.ItemDetails = list.GetItemdetails(bag);
        //        _lastErrorMessage = list.LastErrorMessage;
        //        if (!String.IsNullOrWhiteSpace(_lastErrorMessage))
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
        /// Insert Batch Variance
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult InsertBatchVariance(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);

                int result = list.InsertBatchVariance(bag.SinglebatchVariance);
                _lastErrorMessage = list.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// ResetPicker
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult ResetPicker(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);

                int result = list.ResetPicker(bag.PickHead);
                _lastErrorMessage = list.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Update configuration Item Validation for Pick List
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult UpdateValidateItemConfiguration(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_sapConnectionStr);
                int result = list.UpdateValidateItemConfiguration(bag.IsEnableItemValidate);
                _lastErrorMessage = list.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok();
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get configuration Item Validation for Pick List
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IActionResult GetConfigureItemValidation(Cio bag)
        {
            try
            {
                Cio cio = new Cio();
                using var list = new SQL_OPKL(_sapConnectionStr);

                cio.IsEnableItemValidate = list.GetValidateItemConfiguration();
                _lastErrorMessage = list.LastErrorMessage;

                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(cio);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// Update picklist
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult UpdatePickList(Cio bag)
        {
            try
            {
                using (var updateCount = new SQL_OPKL(_sapConnectionStr, _dbMidwareConnectionStr))
                {
                    var result = updateCount.CreateUpdatePickList_Midware(
                        bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins);

                    _lastErrorMessage = updateCount.LastErrorMessage;
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

        void Log(string message, Cio bag)
        {
            _logger?.LogError(message, bag);
            _fileLogger.WriteLog(message);
        }
    }
}


#region OldCode
//                    case "GetPickDetailsFromSOWithOnhold":
//                        {
//    return GetPickDetailsFromSOWithOnhold(bag);//Get pick details (SO)
//}
//                    case "GetPickDetailsFromSOWithBatch":
//                        {
//    return GetPickDetailsFromSOWithBatch(bag);//Get pick details (SO)

//    /// <summary>
//    /// Get PickList Details (SO)
//    /// </summary>
//    /// <param name="bag"></param>
//    /// <returns></returns>
//    private IActionResult GetPickDetailsFromSOWithOnhold(Cio bag)
//    {
//        try
//        {
//            var _sapConnectionStr = _configuration.GetConnectionString(_dbName);
//            using var list = new SQL_OPKL(_sapConnectionStr, _dbMidwareConnectionStr);
//            var dtoPKL = new DTO_OPKL();
//            dtoPKL = list.GetPickDetailsFromSOWithOnholdBatch(bag.PickDoc);
//            _lastErrorMessage = list.LastErrorMessage;
//            if (_lastErrorMessage.Length > 0)
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
//}

///// <summary>
///// Get Pick Item Line With Batch
///// </summary>
///// <param name="bag"></param>
///// <returns></returns>
//private IActionResult GetPickDetailsFromSOWithBatch(Cio bag)
//{
//    try
//    {
//        using var list = new SQL_OPKL(_sapConnectionStr, _dbMidwareConnectionStr);
//        var dtoPKL = new DTO_OPKL();
//        dtoPKL = list.GetPickDetailsFromSOWithBatch(bag.PickDoc);
//        _lastErrorMessage = list.LastErrorMessage;
//        if (_lastErrorMessage.Length > 0)
//        {
//            return BadRequest(_lastErrorMessage);
//        }
//        return Ok(dtoPKL);
//    }
//    catch (Exception excep)
//    {
//        Log($"{excep}", bag);
//        return BadRequest($"{excep}");
//    }
//}

//case "HandleUpdatePickList":
//    {
//        return HandleUpdatePickList(bag);
//    }
///// <summary>
///// Update Picked in Pick List
///// </summary>
///// <param name="bag"></param>
///// <returns></returns>
//private IActionResult HandleUpdatePickList(Cio bag)
//{
//    try
//    {
//        var _sapConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//        using var diapilist = new DiApiUpdatePickList(_configuration, _dbMidwareConnectionStr,_company);
//        using var midwarelist = new SQL_OPKL(_sapConnectionStr, _dbMidwareConnectionStr);

//        var result = diapilist.PartialUpdatePickList(bag.PickDoc, bag.pKL1List,bag.PickHead);
//        _lastErrorMessage = diapilist.LastErrorMessage;

//        if (result < 0)
//        {
//            return BadRequest(_lastErrorMessage);
//        }
//        result = midwarelist.RemoveAllBatchesforPickList(bag.pKL1List);
//        _lastErrorMessage = midwarelist.LastErrorMessage;
//        if (result < 0)
//        {
//            return BadRequest(_lastErrorMessage);
//        }

//        return Ok();
//    }
//    catch (Exception excep)
//    {
//        Log($"{excep}", bag);
//        return BadRequest($"{excep}");
//    }
//}

#endregion