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
                    //case "GetAllPickListAndWarehouse":
                    //    {
                    //        return GetAllPickListAndWarehouse(bag);
                    //    }
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
                            return GetAvailableBatchItem(bag);
                        }
                    case "AddBatch":
                        {
                            return AddBatch(bag);
                        }
                    case "RemoveBatch":
                        {
                            return RemoveBatch(bag);
                        }
                    //case "UpdatePickList":
                    //    {
                    //        //return UpdatePickList(bag);
                    //    }
                    case "GetAllWarehouseFilter":
                        {
                            return GetAllWarehouseFilter(bag);
                        }
                    case "PostPickList":
                        {
                            return PostPickList(bag);
                        }
                    case "HandleUpdatePickListHeader":
                        {
                            return HandleUpdatePickListHeader(bag);
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
                    case "ResetPickList":
                        {
                            return ResetPickList(bag);
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

        private IActionResult AddBatch(Cio bag)
        {
            try
            {
                using var sqllist = new SQL_OPKL(_dbMidwareConnectionStr);
                using var diapi = new DiApiUpdatePickList(_dbMidwareConnectionStr, _company);

                int result = -1;
                result = sqllist.AddBatch(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = diapi.LastErrorMessage;
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

        private IActionResult RemoveBatch(Cio bag)
        {
            try
            {
                using var sqllist = new SQL_OPKL(_dbMidwareConnectionStr);
                using var diapi = new DiApiUpdatePickList(_dbMidwareConnectionStr, _company);
                int result = -1;

                result = sqllist.RemoveBatch(bag.PickItemLine, bag.oBTQ);
                _lastErrorMessage = diapi.LastErrorMessage;
                if (result < 0)
                {
                    return BadRequest(_lastErrorMessage);
                }

                result = sqllist.DeleteBatchVariance(bag.PickItemLine, bag.oBTQ);
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

        private IActionResult HandleUpdatePickListHeader(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr,_sapConnectionStr);

                int result = list.UpdatePickHeader(bag.PickHead);
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

        private IActionResult GetAvailableBatchItem(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);
                var line = bag.PickItemLine;
                var batches = list.GetAvailableBatches(line.ItemCode, line.WhsCode, line.AbsEntry, line.PickEntry);
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

                //dto.draftList = list.GetLineDrafts(bag.PickHead.AbsEntry);
                //_lastErrorMessage = list.LastErrorMessage;
                //if (_lastErrorMessage.Length > 0)
                //{
                //    return BadRequest(_lastErrorMessage);
                //}

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

        private IActionResult GetAllWarehouseFilter(Cio bag)
        {
            try
            {
                using var list = new SQL_OPKL(_dbMidwareConnectionStr, _sapConnectionStr);

                var warehouseList = list.GetPickWarehouse();
                _lastErrorMessage = list.LastErrorMessage;
                if (_lastErrorMessage.Length > 0)
                {
                    return BadRequest(_lastErrorMessage);
                }
                return Ok(warehouseList);

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

        IActionResult ResetPickList(Cio bag)
        {
            try
            {
                using (var updateCount = new SQL_OPKL(_sapConnectionStr, _dbMidwareConnectionStr))
                {
                    var result = updateCount.ResetPickList_Midware(bag.dtoRequest);

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

        //IActionResult UpdatePickList(Cio bag)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

        IActionResult PostPickList(Cio bag)
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


