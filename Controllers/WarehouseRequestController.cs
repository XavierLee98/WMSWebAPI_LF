using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WMSWebAPI.ClassObject;
using WMSWebAPI.SQL_Object.Warehouse;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WarehouseRequestController : ControllerBase
    {
        readonly string _dbName = "DatabaseWMSConn"; // 20200612T1030        
        readonly IConfiguration _configuration;
        readonly ILogger<WarehouseRequestController> logger;
        string _dbConnectionStr = string.Empty;
      
        public WarehouseRequestController(IConfiguration Configuration , ILogger<WarehouseRequestController> Logger)
        {
            _configuration = Configuration;
            logger = Logger;
            _dbConnectionStr = _configuration.GetConnectionString(_dbName);
        }

       // [Authorize(Roles = "SuperAdmin, Admin, User")] /// tested with authenticated token based   
        [HttpPost]
        public IActionResult Post(WarehouseDTO bag)
        {
            switch (bag.Request)
            {
                case "QueryAllWarehouse":
                    {
                        return QueryAllWarehouse(bag);
                    }
                case "QueryGetAvailableItem":
                    {
                        return QueryGetAvailableItem(bag);
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

        IActionResult QueryBatchItemWithoutBin(WarehouseDTO bag)
        {
            bag.ItemWhsResult = 
                new SQL_WarehouseOBTQ()
                .GetSQL_WarehouseOBTQ(bag.SelectedWarehouse, bag.SelectedItem, _dbConnectionStr);
            return Ok(bag);
        }

        IActionResult QueryBatchItemWithBin(WarehouseDTO bag)
        {
            bag.ItemWhsResult = new SQL_WarehouseOBBQ()
                .GetSQL_WarehouseOBBQ(bag.SelectedWarehouse, bag.SelectedItem, _dbConnectionStr);
            return Ok(bag);
        }

        IActionResult QuerySerialItemWithoutBin(WarehouseDTO bag)
        {
            bag.ItemWhsResult = new SQL_WarehouseOSRQ()
                .GetSQL_WarehouseOSRQ(bag.SelectedWarehouse, bag.SelectedItem, _dbConnectionStr);
            return Ok(bag);
        }

        IActionResult QuerySerialItemWithBin(WarehouseDTO bag)
        {
            bag.ItemWhsResult = new SQL_WarehouseOSBQ()
                .GetSQL_WarehouseOSBQ(bag.SelectedWarehouse, bag.SelectedItem, _dbConnectionStr);
            return Ok(bag);
        }

        IActionResult QueryNonManageItemWithBin(WarehouseDTO bag)
        {
            bag.ItemWhsResult = new SQL_WarehouseOIBQ()
                .GetSQL_WarehouseOIBQ(bag.SelectedWarehouse, bag.SelectedItem, _dbConnectionStr);
            return Ok(bag);
        }

        IActionResult QueryNonManageItem(WarehouseDTO bag)
        {
            bag.ItemWhsResult = new SQL_WarehouseOITW()
                .GetSQL_WarehouseOITW(bag.SelectedWarehouse, bag.SelectedItem, _dbConnectionStr);
            return Ok(bag);
        }
        IActionResult QueryGetAvailableItem(WarehouseDTO bag)
        {
            bag.AvailableItemObjs = 
                new SQL_AvailableItem().GetAvailableItem(bag.SelectedWarehouse, _dbConnectionStr);
            return Ok(bag);
        }

        IActionResult QueryAllWarehouse(WarehouseDTO bag)
        {
            bag.oWHSs = new SQL_Object.SQL_OWHS().GetSQL_OWHSs(_dbConnectionStr);
            return Ok(bag);
        }
    }
}
