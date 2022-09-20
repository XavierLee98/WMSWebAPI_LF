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
    public class RIPickListController : Controller
    {
        readonly string _dbName = "DatabaseWMSConn";
        readonly string _dbNameMidware = "DatabaseFTMiddleware";

        readonly IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();
        SAPCompany _company;

        public RIPickListController(IConfiguration configuration, ILogger<RIPickListController> logger, SAPCompany company)
        {
            _configuration = configuration;
            _logger = logger;
            _company = company;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                if (!_company.connectSAP())
                {
                    throw new Exception(_company.errMsg);
                }
                return Ok("OK");
            }
            catch (Exception e)
            {
                _fileLogger.WriteLog(e.ToString());
                return BadRequest("Fail");
            }
        }



    }
}

//         try
//            {
//                _lastErrorMessage = string.Empty;
//                switch (bag.request)
//                {
//                    case "GetRIPickHead":
//                        {
//                            return GetRIPickHead(bag);//Get All SO Pick List
//    }
//                    case "GetReservedInvoiceList":
//                        {
//                            return GetReservedInvoiceList(bag);//Get All OINV
//}
//                    case "CreateRIPickList":
//                        {
//    return CreateRIPickList(bag);
//}
//                    case "GetItemdetails":
//                        {
//    return GetItemdetails(bag);
//}
//                    case "GetPickDetailsFromRIWithOnHoldBatch":
//                        {
//    return GetPickDetailsFromRIWithOnHoldBatch(bag);
//}
//                    case "GetPickDetailsFromRIWithBatch":
//                        {
//    return GetPickDetailsFromRIWithBatch(bag);
//}
//                    case "AssignBatchToRI":
//                        {
//    return AssignBatchToRI(bag);
//}
//                    case "RICancelAssignBatchForAllItems":
//                        {
//    return RICancelAssignBatchForAllItems(bag);
//}
//                    case "RICancelAssignSingleBatch":
//                        {
//    return RICancelAssignSingleBatch(bag);
//}
//                    case "RICancelAssignBatchForSingleItem":
//                        {
//    return RICancelAssignBatchForSingleItem(bag);
//}
//                    case "RIUpdatePickList":
//                        {
//    return RIUpdatePickList(bag);
//}
//                    case "RIUpdatePickListHeader":
//                        {
//    return RIUpdatePickListHeader(bag);
//}
//                    case "RICreateSAPPickList":
//                        {
//    return RICreateSAPPickList(bag);
//}
//                    case "RIRejectPickList":
//                        {
//    return RIReject(bag);
//}
//                    case "ResetPicker":
//                        {
//    return ResetPicker(bag);
//}
//                    case "UpdatePicker":
//                        {
//    return UpdatePicker(bag);
//}
//                    case "CancelReleasedPickList":
//                        {
//    return CancelReleasedPickList(bag);
//}
//                }
//                return BadRequest($"Invalid request, please try again later. Thanks");
//            }
//            catch (Exception excep)
//{
//    Log($"{excep}", bag);
//    return BadRequest($"{excep}");
//}
///// <summary>
///// Cancel RI PickList
///// </summary>
///// <param name="bag"></param>
///// <returns></returns>
//private IActionResult CancelReleasedPickList(Cio bag)
//        {
//            try
//            {
//                using var list = new SQL_OPKL_RI(_dbMidwareConnectionStr);
//                using var dblist = new SQL_OPKL_RI(_dbConnectionStr);
//                var result = dblist.CancelPickListToRI(bag.INV1s);
//                _lastErrorMessage = list.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = list.CancelReleasedPickList(bag.PickHead.AbsEntry);
//                _lastErrorMessage = list.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Update Picker
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult UpdatePicker(Cio bag)
//        {
//            try
//            {
//                using var list = new SQL_OPKL_RI(_dbMidwareConnectionStr);
//                var result = list.UpdatePicker(bag.QueryPicker,bag.PickHead.AbsEntry);
//                _lastErrorMessage = list.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                return Ok(bag);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Reset Picker
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult ResetPicker(Cio bag)
//        {
//            try
//            {
//                using var list = new SQL_OPKL_RI(_dbMidwareConnectionStr);
//                var result = list.ResetPicker(bag.PickHead);
//                _lastErrorMessage = list.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                return Ok(bag);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Reject Pick List
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RIReject(Cio bag)
//        {
//            try
//            {
//                //Get RIItemLine
//                //GetAllocatedBatch
//                //RemoveBatch

//                using var Diapi = new DIApiRIPickList(_configuration,_dbMidwareConnectionStr);
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                var _MidwaredbConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                using var Midware = new SQL_OPKL_RI(_MidwaredbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                //var INV1s = list.GetPickDetailsFromRI(bag.PickDoc);

//                //_lastErrorMessage = list.LastErrorMessage;
//                //if (_lastErrorMessage.Length > 0)
//                //{
//                //    return BadRequest(_lastErrorMessage);
//                //}

//                //var batch = list.GetAllocatedBatchFromRI(INV1s);

//                //_lastErrorMessage = list.LastErrorMessage;
//                //if (_lastErrorMessage.Length > 0)
//                //{
//                //    return BadRequest(_lastErrorMessage);
//                //}

//                var result = Diapi.RICancelAssignAllBatchesForAllItem(bag);
//                _lastErrorMessage = Diapi.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = Midware.RemoveHoldingMultiBatchForAllItem(bag);
//                _lastErrorMessage = Midware.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = list.RemovePickNoFromRI(bag.PickHead.AbsEntry);
//                _lastErrorMessage = list.LastErrorMessage;


//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }


//                result = Midware.UpdateRIPickItemStatusToReject(bag.PickHead);
//                _lastErrorMessage = Midware.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                return Ok();

//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Get Item Details
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult GetItemdetails(Cio bag)
//        {
//            try
//            {
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                bag.ItemDetails = list.GetItemdetails(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (!String.IsNullOrWhiteSpace(_lastErrorMessage))
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(bag);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Post to SAP for creating actual Picklist and Update DocStatus to Approved
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RICreateSAPPickList(Cio bag)
//        {
//            try
//            {
//                var _MidwaredbConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                using var Midware = new SQL_OPKL_RI(_MidwaredbConnectionStr);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                using var Diapi = new DIApiRIPickList(_configuration,_MidwaredbConnectionStr);
//                var PicDoc = Diapi.RICreatePickList(bag);
//                _lastErrorMessage = Diapi.LastErrorMessage;

//                if (PicDoc == null)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                var result = Diapi.RIPickBatchAfterCreated(PicDoc, bag);
//                _lastErrorMessage = Diapi.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = Midware.RemoveHoldingMultiBatchForAllItem(bag);
//                _lastErrorMessage = Midware.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = Midware.UpdateRIPickItemStatusToApproved(bag.PickHead.AbsEntry, Int32.Parse(PicDoc));
//                _lastErrorMessage = Midware.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(PicDoc);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Update PickList Header
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RIUpdatePickListHeader(Cio bag)
//        {
//            try
//            {
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                var result = list.UpdateRIPickItemHeader(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Update U_PickedQty in INV1 And Change Status to Completed
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RIUpdatePickList(Cio bag)
//        {
//            try
//            {
//                using var Diapi = new DIApiRIPickList(_configuration,_dbMidwareConnectionStr);
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                var _MidwaredbConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                using var Midware = new SQL_OPKL_RI(_MidwaredbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                var result = list.RIUpdatePickList(bag.PickDoc, bag.INV1s);
//                _lastErrorMessage = list.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = Midware.UpdateRIPickItemStatus(bag.PickDoc,bag.Weight);
//                _lastErrorMessage = Midware.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }


//        /// <summary>
//        /// Cancel All batches Allocation For Single Item
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RICancelAssignBatchForSingleItem(Cio bag)
//        {
//            try
//            {
//                using var Diapi = new DIApiRIPickList(_configuration,_dbMidwareConnectionStr);
//                using var list = new SQL_OPKL_RI(_dbMidwareConnectionStr);
//                var dtoPKL = new DTO_OPKL();

//                var result = Diapi.RICancelAssignBatchForSingleItem(bag);
//                _lastErrorMessage = Diapi.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = list.DeleteBatchVarianceForSingleItem(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                result = list.RemoveHoldingMultiBatchForSingleItem(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Cancel Single batch Allocation
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RICancelAssignSingleBatch(Cio bag)
//        {
//            try
//            {
//                using var Diapi = new DIApiRIPickList(_configuration,_dbMidwareConnectionStr);
//                using var list = new SQL_OPKL_RI(_dbMidwareConnectionStr);

//                var result = Diapi.RICancelAssignSingleBatch(bag);
//                _lastErrorMessage = Diapi.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                result = list.DeleteBatchVariance(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = list.RemoveHoldingSingleBatch(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Cancel all Item Batch Allocation
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult RICancelAssignBatchForAllItems(Cio bag)
//        {
//            try
//            {
//                using var Diapi = new DIApiRIPickList(_configuration,_dbMidwareConnectionStr);
//                using var list = new SQL_OPKL_RI(_dbMidwareConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                var result = Diapi.RICancelAssignAllBatchesForAllItem(bag);
//                _lastErrorMessage = Diapi.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                result = list.RemoveHoldingMultiBatchForAllItem(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                result = list.DeleteBatchVarianceForAllItem(bag);
//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }

//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Assign Batch To Reserve Invoice
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult AssignBatchToRI(Cio bag)
//        {
//            try
//            {
//                using var list = new DIApiRIPickList(_configuration, _dbMidwareConnectionStr);
//                using var Midlist = new SQL_OPKL_RI(_dbMidwareConnectionStr);
//                int result = list.AssignBatchToRI(bag.RIItemLine,bag.oBTQs);

//                _lastErrorMessage = list.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                result = Midlist.InsertHoldingBatch(bag.RIItemLine, bag.oBTQ);
//                _lastErrorMessage = Midlist.LastErrorMessage;

//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok();
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Get Pick ItemLine With batch allocated in RI (INV1)
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult GetPickDetailsFromRIWithBatch(Cio bag)
//        {
//            try
//            {
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                dtoPKL = list.GetPickDetailsFromRIWithBatch(bag.PickDoc);

//                _lastErrorMessage = list.LastErrorMessage;
//                if (_lastErrorMessage.Length > 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(dtoPKL);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Generate PKL1 Line from RI itemLine(INV1)
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult GetPickDetailsFromRIWithOnHoldBatch(Cio bag)
//        {
//            try
//            {
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr, _dbMidwareConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                dtoPKL.iNV1s = list.GetPickDetailsFromRI(bag.PickDoc);

//                _lastErrorMessage = list.LastErrorMessage;
//                if (_lastErrorMessage.Length > 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(dtoPKL);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }


//        /// <summary>
//        /// Create Pick List inside MidwareDB
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult CreateRIPickList(Cio bag)
//        {
//            try
//            {
//                var _middbConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//                var _sapdbConnectionStr = _configuration.GetConnectionString(_dbName);

//                using var midware = new SQL_OPKL_RI(_middbConnectionStr);
//                using var sap = new SQL_OPKL_RI(_sapdbConnectionStr);
//                var dtoPKL = new DTO_OPKL();

//                var id = midware.CreatePickList(bag);
//                _lastErrorMessage = midware.LastErrorMessage;
//                if (id < 0 )
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                var result = sap.UpdateRIPickItem(bag,id);
//                _lastErrorMessage = sap.LastErrorMessage;
//                if (result < 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(id);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Get RI List
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult GetReservedInvoiceList(Cio bag)
//        {
//            try
//            {
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbName);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                dtoPKL = list.GetReservedInvoiceList(bag);

//                _lastErrorMessage = list.LastErrorMessage;
//                if (_lastErrorMessage.Length > 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(dtoPKL);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        /// <summary>
//        /// Get PickList Header
//        /// </summary>
//        /// <param name="bag"></param>
//        /// <returns></returns>
//        private IActionResult GetRIPickHead(Cio bag)
//        {
//            try
//            {
//                var _dbConnectionStr = _configuration.GetConnectionString(_dbNameMidware);
//                using var list = new SQL_OPKL_RI(_dbConnectionStr);
//                var dtoPKL = new DTO_OPKL();
//                dtoPKL.OPKLs = list.GetRIPickHead(bag);

//                _lastErrorMessage = list.LastErrorMessage;
//                if (_lastErrorMessage.Length > 0)
//                {
//                    return BadRequest(_lastErrorMessage);
//                }
//                return Ok(dtoPKL);
//            }
//            catch (Exception excep)
//            {
//                Log($"{excep}", bag);
//                return BadRequest($"{excep}");
//            }
//        }

//        void Log(string message, Cio bag)
//        {
//            _logger?.LogError(message, bag);
//            _fileLogger.WriteLog(message);
//        }