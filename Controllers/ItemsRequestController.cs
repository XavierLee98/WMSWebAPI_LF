using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.SQL_Object;
using WMSWebAPI.ClassObject;
using WMSWebAPI.SQL_Object;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsRequestController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030        
        readonly IConfiguration _configuration;
        readonly ILogger<ItemsRequestController> logger;
        string _dbConnectionStr = string.Empty;

        public ItemsRequestController(IConfiguration Configuration,  ILogger<ItemsRequestController> Logger)
        {
            _configuration = Configuration;
            logger = Logger;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
        }

       // [Authorize(Roles = "SuperAdmin, Admin, User")] /// tested with authenticated token based   
        [HttpPost]
        public IActionResult Post (ItemDTO bag)
        {
            switch (bag.Request)
            {
                case "QueryGetAllItem":
                    {
                        return QueryGetAllItem(bag);
                    }
                case "QueryNonManageItem":
                    {
                        return QueryNonManageItem(bag);
                    }
                case "QueryNonManageItemWithBin":
                    {
                        return QueryNonManageItemWithBin(bag);
                    }
                case "QuerySerialItemWithBin":
                    {
                        return QuerySerialItemWithBin(bag);
                    }
                case "QuerySerialItemWithoutBin":
                    {
                        return QuerySerialItemWithoutBin(bag);
                    }
                case "QueryBatchItemWithBin":
                    {
                        return QueryBatchItemWithBin(bag);
                    }
                case "QueryBatchItemWithoutBin":
                    {
                        return QueryBatchItemWithoutBin(bag);
                    }
            }
            return BadRequest("Request no recognised");
        }
        IActionResult QueryBatchItemWithoutBin(ItemDTO bag)
        {            
            bag.ItemWhsResult = new SQL_OBTQ().GetSQL_OBTQs(bag.SelectedItem, _dbConnectionStr);
            foreach (var x in bag.ItemWhsResult)
            {
                x.request = bag.Request;
            }
            return Ok(bag);
        }

        IActionResult QueryBatchItemWithBin(ItemDTO bag)
        {            
            bag.ItemWhsResult = new SQL_OBBQ().GetSQL_OBBQs(bag.SelectedItem, _dbConnectionStr);
            foreach (var x in bag.ItemWhsResult)
            {
                x.request = bag.Request;
            }
            return Ok(bag);
        }

        IActionResult QuerySerialItemWithoutBin(ItemDTO bag)
        {
            bag.ItemWhsResult = new SQL_OSRQ().GetSQL_OSRQs(bag.SelectedItem, _dbConnectionStr);
            foreach (var x in bag.ItemWhsResult)
            {
                x.request = bag.Request;
            }
            return Ok(bag);
        }
        IActionResult QuerySerialItemWithBin(ItemDTO bag)
        {
            bag.ItemWhsResult = new SQL_OSBQ().GetSQL_OSBQs(bag.SelectedItem, _dbConnectionStr);
            foreach (var x in bag.ItemWhsResult)
            {
                x.request = bag.Request;
            }
            return Ok(bag);
        }
        IActionResult QueryNonManageItemWithBin(ItemDTO bag)
        {
            bag.ItemWhsResult = new SQL_OIBQ().GetSQL_OIBQs(bag.SelectedItem, _dbConnectionStr);
            foreach (var x in bag.ItemWhsResult)
            {
                x.request = bag.Request;
            }
            return Ok(bag);
        }
        IActionResult QueryNonManageItem(ItemDTO bag)
        {
            bag.ItemWhsResult = new SQL_OITW().GetSQL_OITWs(bag.SelectedItem, _dbConnectionStr);
            foreach (var x in bag.ItemWhsResult)
            {
                x.request = bag.Request;
            }
            return Ok(bag);
        }
        IActionResult QueryGetAllItem(ItemDTO bag)
        {
            bag.oITMs = new SQL_OITM().GetSQL_OITMs(_dbConnectionStr);
            return Ok(bag);
        }
    }
}
