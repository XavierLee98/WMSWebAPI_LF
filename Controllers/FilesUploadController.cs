using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using WMSWebAPI.Class;

namespace WMSWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesUploadController : ControllerBase
    {
        readonly string _dbNameAppMidware = "DatabaseFTMiddleware"; // 20200612T1030     
        readonly string _SignPicPath = "SignPicSavePath"; // 20200612T1030     

        string _dbConnectionStr = string.Empty;
        string _signPicSavePath = string.Empty;

        readonly IConfiguration _configuration;
        ILogger _logger;
        FileLogger _fileLogger = new FileLogger();

        public FilesUploadController(IConfiguration configuration, ILogger<GrpoController> logger)
        {
            _configuration = configuration;
            _dbConnectionStr = _configuration.GetConnectionString(_dbNameAppMidware);
            _signPicSavePath = _configuration.GetSection(_SignPicPath).Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Content("file not selected");
                }   

                // read the header from request                         
                //HttpContext.Request.Headers.TryGetValue("token", out StringValues tokenValue);
                HttpContext.Request.Headers.TryGetValue("user", out StringValues userValue);
                HttpContext.Request.Headers.TryGetValue("guid", out StringValues guidValue);
                //HttpContext.Request.Headers.TryGetValue("salesFilePrefix", out StringValues salesFilePrefix);

                if (!Directory.Exists(_signPicSavePath))
                {
                    Directory.CreateDirectory(_signPicSavePath);
                }

                var path = Path.Combine(_signPicSavePath, file.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // save into the database
                string insertSql = $"INSERT INTO zmwFileUpload(" +
                                $"headerGuid" +
                                $",uploadDatetime" +
                                $",appUser" +
                                $",serverSavedPath" +
                                $")VALUES(" +
                                   $"@headerGuid" +
                                   $",GETDATE()" +
                                   $",@appUser" +
                                   $",@serverSavedPath)";

                var insertResult = -1;
                insertResult = new SqlConnection(_dbConnectionStr)
                    .Execute(insertSql,
                    new
                    {
                        headerGuid = guidValue,
                        appUser = userValue,
                        serverSavedPath = path
                    });

                if (insertResult == -1)
                {
                    Log("File details insert db fail", null);
                    return Content("File details insert db fail");
                }

                return Ok();
            }
            catch (Exception e)
            {
                Log($"{e.Message}\n{e.StackTrace}", null);
                return BadRequest(e.Message);
            }
        }

        //public async Task<IActionResult> Download(string filename)
        //{
        //    if (filename == null)
        //        return Content("filename not present");

        //    var path = Path.Combine(
        //                   Directory.GetCurrentDirectory(),
        //                   "wwwroot", filename);

        //    var memory = new MemoryStream();
        //    using (var stream = new FileStream(path, FileMode.Open))
        //    {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;
        //    return File(memory, GetContentType(path), Path.GetFileName(path));
        //}

        //private string GetContentType(string path)
        //{
        //    var types = GetMimeTypes();
        //    var ext = Path.GetExtension(path).ToLowerInvariant();
        //    return types[ext];
        //}


        //Dictionary<string, string> GetMimeTypes()
        //{
        //    return new Dictionary<string, string>
        //    {
        //        {".txt", "text/plain"},
        //        {".pdf", "application/pdf"},
        //        {".doc", "application/vnd.ms-word"},
        //        {".docx", "application/vnd.ms-word"},
        //        {".xls", "application/vnd.ms-excel"},
        //        {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
        //        {".png", "image/png"},
        //        {".jpg", "image/jpeg"},
        //        {".jpeg", "image/jpeg"},
        //        {".gif", "image/gif"},
        //        {".csv", "text/csv"}
        //    };
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
