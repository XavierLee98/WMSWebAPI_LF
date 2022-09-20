using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.Models.Lifewater.GRGI;
using WMSWebAPI.Models.PickList;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030
        readonly string _dbName_midware = "DatabaseFTMiddleware"; // 20200612T1030
        readonly IConfiguration _configuration;
        string _dbConnectionStr = string.Empty;
        string _dbConnectionStr_midware = string.Empty;
        string _lastErrorMessage = string.Empty;

        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public ItemsController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
            _dbConnectionStr_midware = _configuration.GetConnectionString(_dbName_midware);
            _logger = logger;
        }

        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() => GC.Collect();

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
                    case "GetItems":
                        {
                            return GetItems(bag);
                        }
                    case "GetItem":
                        {
                            return GetItem(bag);
                        }
                    case "CreateGoodsReceiveRequest":
                        {
                            return CreateGoodsReceiveRequest(bag);
                        }
                    case "CreateGoodsIssueRequest":
                        {
                            return CreateGoodsIssueRequest(bag);
                        }
                    case "GetGoodsReceiptPriceList": // 20200623T1048 
                        {
                            return GetGoodsReceiptPriceList(bag);
                        }
                    case "UpdateGoodsReceiptPriceListId": // 20200623T1054
                        {
                            return UpdateGoodsReceiptPriceListId(bag);
                        }
                    case "GetGoodsReceiptDocSeries":
                        {
                            return GetGoodsReceiptDocSeries(bag);
                        }
                    case "GetGoodsIssuesDocSeries":
                        {
                            return GetGoodsIssuesDocSeries(bag);
                        }
                    case "UpdateGoodsReceiptDocSeries":
                        {
                            return UpdateGoodsReceiptDocSeries(bag);
                        }
                    case "UpdateGoodsIssuesDocSeries":
                        {
                            return UpdateGoodsIssuesDocSeries(bag);
                        }
                    case "GetGoodsIssuesPriceList":
                        {
                            return GetGoodsIssuesPriceList(bag);
                        }
                    case "UpdateGoodsIssuesPriceListId":
                        {
                            return UpdateGoodsIssuesPriceListId(bag);
                        }
                    case "GetBinContentList":
                        {
                            return GetBinContentList(bag);
                        }
                    case "GIDocSeries":
                        {
                            return GIDocSeries(bag);
                        }
                    case "GRDocSeries":
                        {
                            return GRDocSeries(bag);
                        }
                    // life water 
                    case "LW_ReasonCodes":
                        {
                            return LW_ReasonCode(bag);
                        }
                    // life water GR / GI account code
                    case "LW_AcctCode":
                        {
                            return LW_AcctCode(bag);
                        }
                    case "GetSaleItem":
                        {
                            return GetSaleItem(bag);
                        }
                    case "GetGIReason":
                        {
                            return GetGIReason(bag);
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

        IActionResult GetGIReason(Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM [dbo].[@GIREASON];";
                using var conn = new SqlConnection(_dbConnectionStr);
                var list = conn.Query<GIReason>(sql).ToList();
                return Ok(list);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }


        IActionResult GetSaleItem(Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM OITM WHERE SellItem = 'Y' and validFor = 'Y';";
                using var conn = new SqlConnection(_dbConnectionStr);
                var list = conn.Query<OITM_Ex>(sql).ToList();
                return Ok(list);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }



        IActionResult LW_AcctCode (Cio bag)
        {
            try
            {
                var sql = "SELECT * FROM FTS_vw_IMApp_GRGIAcctCode";
                using var conn = new SqlConnection(_dbConnectionStr);
                var list = conn.Query<OACT>(sql).ToList();
                return Ok(list);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        // for life water 
        // 20210408
        IActionResult LW_ReasonCode(Cio bag)
        {
            try
            {
                var sql = @"SELECT * FROM FTS_vw_IMApp_Reason order by code";
                using var conn = new SqlConnection(_dbConnectionStr);
                var list = conn.Query<Reason>(sql).ToList();
                return Ok(list);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get the GI doc series in array
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GRDocSeries(Cio bag)
        {
            try
            {
                using (var gi = new SQL_OIGN(_dbConnectionStr))
                {
                    var dtoGR = new DTO_OIGN();
                    dtoGR.DocSeries = gi.GetDocSeries();
                    _lastErrorMessage = gi.LastErrorMessage;
                    if (string.IsNullOrWhiteSpace(_lastErrorMessage))
                        return Ok(dtoGR);

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
        /// Get the GI doc series in array
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GIDocSeries(Cio bag)
        {
            try
            {
                using (var gi = new SQL_OIGE(_dbConnectionStr))
                {
                    var dtoGI = new DTO_OIGE();

                    dtoGI.DocSeries = gi.GetDocSeries();
                    _lastErrorMessage = gi.LastErrorMessage;
                    if (string.IsNullOrWhiteSpace(_lastErrorMessage))
                        return Ok(dtoGI);

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
        ///  20200927 show item details on content list
        ///  Get Bin Content List
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetBinContentList(Cio bag)
        {
            try
            {
                using (var binContent = new SQL_OITM(_dbConnectionStr))
                {
                    bag.dtoBinContents = binContent.GetItemBinContents(bag.QueryItemCode);
                    bag.dtoStockTransLogs = binContent.GetItemTransLogs(bag.QueryItemCode, bag.QueryStartDate, bag.QueryEndDate);
                    LastErrorMessage = binContent.LastErrorMessage;

                    //QueryItemCode = itemCode,
                    //QueryStartDate = startDate,
                    //QueryEndDate = endDate
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Stock Trans Logs
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        //IActionResult GetStockTransLogs(Cio bag)
        //{
        //    try
        //    {
        //        using (var binContent = new SQL_OITM(_dbConnectionStr))
        //        {
        //            bag.dtoStockTransLogs = binContent.GetItemTransLogs(bag.QueryItemCode, bag.QueryStartDate, bag.QueryEndDate);
        //            LastErrorMessage = binContent.LastErrorMessage;
        //        }

        //        if (string.IsNullOrWhiteSpace(LastErrorMessage))
        //        {
        //            return Ok(bag);
        //        }
        //        return BadRequest(LastErrorMessage);
        //    }
        //    catch (Exception excep)
        //    {
        //        Log($"{excep}", bag);
        //        return BadRequest($"{excep}");
        //    }
        //}

        /// <summary>
        /// GetGoodsIssuesPriceList
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGoodsIssuesPriceList(Cio bag)
        {
            try
            {
                using (var pricelist = new SQL_OPLN(_dbConnectionStr))
                {
                    bag.PriceList = pricelist.GetPriceList();
                    bag.ExistingGiPriceListId = pricelist.GetExistingPriceList_GI();
                    LastErrorMessage = pricelist.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }


        /// <summary>
        /// UpdateGoodsIssuesPriceListId
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult UpdateGoodsIssuesPriceListId(Cio bag)
        {
            try
            {
                using (var pricelist = new SQL_OPLN(_dbConnectionStr))
                {
                    var result = pricelist.UpdateSelectedGiPriceList(bag.UpdateGiPriceListId);
                    LastErrorMessage = pricelist.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }


        /// <summary>
        /// return list if the item master
        /// </summary>
        /// <param name="Bag"></param>
        /// <returns></returns>
        IActionResult GetItems(Cio bag)
        {
            try
            {
                using (var items = new SQL_OITM(_dbConnectionStr))
                {
                    bag.Items = items.GetItems();
                    LastErrorMessage = items.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Single item by item code
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetItem(Cio bag)
        {
            try
            {
                using (var items = new SQL_OITM(_dbConnectionStr))
                {
                    bag.Item = items.GetItem(bag.QueryItemCode);
                    LastErrorMessage = items.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage)) return Ok(bag);
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Create Goods IssueRequest
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateGoodsIssueRequest(Cio bag)
        {
            try
            {
                using var requestHelp = new InsertRequestHelper(_dbConnectionStr, _dbConnectionStr_midware);
                var result = requestHelp.CreateRequest(
                    bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);

                _lastErrorMessage = requestHelp.LastErrorMessage;
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
        /// Create Goods Receive Request
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult CreateGoodsReceiveRequest(Cio bag)
        {
            try
            {
                using var requestHelp = new InsertRequestHelper(_dbConnectionStr, _dbConnectionStr_midware);
                var result = requestHelp.CreateRequest(
                    bag.dtoRequest, bag.dtoGRPO, bag.dtoItemBins, bag.dtozmwDocHeaderField);

                _lastErrorMessage = requestHelp.LastErrorMessage;
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
        /// Get Goods Receipt Price List
        /// </summary>
        /// <returns></returns>
        IActionResult GetGoodsReceiptPriceList(Cio bag)
        {
            try
            {
                using (var pricelist = new SQL_OPLN(_dbConnectionStr))
                {
                    bag.PriceList = pricelist.GetPriceList();
                    bag.ExistingGrPriceListId = pricelist.GetExistingPriceList();
                    LastErrorMessage = pricelist.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Update Gr Price List Id
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult UpdateGoodsReceiptPriceListId(Cio bag)
        {
            try
            {
                using (var pricelist = new SQL_OPLN(_dbConnectionStr))
                {
                    var result = pricelist.UpdateSelectedGrPriceList(bag.UpdateGrPriceListId);
                    LastErrorMessage = pricelist.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Update Goods Receipt Doc Series
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult UpdateGoodsReceiptDocSeries(Cio bag)
        {
            try
            {
                using (var goodsReceipt = new SQL_OIGN(_dbConnectionStr))
                {
                    var result = goodsReceipt.UpdateGoodsReceiptDocSeries(bag.UpdateGrDocSeries);
                    LastErrorMessage = goodsReceipt.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
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
        IActionResult UpdateGoodsIssuesDocSeries(Cio bag)
        {
            try
            {
                using (var goodIssues = new SQL_OIGE(_dbConnectionStr))
                {
                    var result = goodIssues.UpdateGoodsIssuesDocSeries(bag.UpdateIssueDocSeries);
                    LastErrorMessage = goodIssues.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }
                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Goods Receipt Doc Series
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGoodsReceiptDocSeries(Cio bag)
        {
            try
            {
                using (var goodsReceipt = new SQL_OIGN(_dbConnectionStr))
                {
                    bag.ExistingGrDocSeries = goodsReceipt.GetGoodsReceiptDocSeries();
                    LastErrorMessage = goodsReceipt.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(LastErrorMessage);
            }
            catch (Exception excep)
            {
                Log($"{excep}", bag);
                return BadRequest($"{excep}");
            }
        }

        /// <summary>
        /// Get Goods Receipt Doc Series
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        IActionResult GetGoodsIssuesDocSeries(Cio bag)
        {
            try
            {
                using (var goodsReceipt = new SQL_OIGE(_dbConnectionStr))
                {
                    bag.ExistingGIDocSeries = goodsReceipt.GetGoodsIssuesDocSeries();
                    LastErrorMessage = goodsReceipt.LastErrorMessage;
                }

                if (string.IsNullOrWhiteSpace(LastErrorMessage))
                {
                    return Ok(bag);
                }

                return BadRequest(LastErrorMessage);
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