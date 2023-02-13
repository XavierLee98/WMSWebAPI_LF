using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WMSWebAPI.Class;
using WMSWebAPI.Dtos;
using WMSWebAPI.Interface;
using WMSWebAPI.Models;
using WMSWebAPI.Models.Authentcation;
using WMSWebAPI.Models.SAP_DiApi;
using WMSWebAPI.SAP_DiApi;
using WMSWebAPI.SAP_SQL;

namespace WMSWebAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        IUserService _userService;
        IConfiguration _configuration;
        string _dbConnString, _secrectKey;

        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IConfiguration configuration, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _configuration = configuration;
                        
            _dbConnString = configuration.GetConnectionString("DatabaseFTMiddleware");
            _secrectKey = appSettings.Value.Secret;
        }

        /// <summary>
        /// Query via DIAPI object to get the list of the sap company object
        /// </summary>
        /// <returns></returns>       
        ///api/SapQp/        
        [AllowAnonymous]
        [HttpPost("sapcompany")]
        public diSAPCompanyModel [] SAPCompany()
        {
            using (var company = new DiApiGetCompanyList(_configuration))
            {
                return (String.IsNullOrWhiteSpace(company.lastErrorMessage)) ? company.companyNameList : null;
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAppInfo")]
        public IActionResult GetAppInfo()
        {
            try
            {
                var versioncontrol = new SQL_VersionControl(_dbConnString);

                return Ok(versioncontrol.GetAppInfo());
            }
            catch (Exception excep)
            {
                return BadRequest(excep.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("token")]        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DtoAuthen>> Token(zwaUserModel zwaUser)
        {
            try
            {
                var user = new zwaUserModel(_dbConnString)
                {
                    userIdName = zwaUser.userIdName,
                    password = zwaUser.password,
                    companyId = zwaUser.companyId
                };

                var secKey = _configuration.GetSection("AppSettings").GetSection("Secret").Value;
                var decrytKey = _configuration.GetSection("AppSettings").GetSection("DecrytKey").Value;

                var versioncontrol = new SQL_VersionControl(_dbConnString);

                if (!versioncontrol.CheckAppVersion(zwaUser.AppName, zwaUser.AppVersion, out string AppUrl))
                {
                    return new ObjectResult(AppUrl) { StatusCode = 403};
                }

                if (!user.VerifiedLogin(secKey, decrytKey))
                {
                    return Unauthorized("Invalid login");
                }

                    // authentication successful so generate jwt token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_secrectKey);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim(ClaimTypes.Role, user.roleDesc),
                        new Claim(ClaimTypes.Name, user.userIdName),
                        new Claim(ClaimTypes.AuthenticationMethod, $"{Guid.NewGuid()}")
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    // prepare the access token
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var brearerToken = new BearerToken()
                    {
                        access_token = tokenHandler.WriteToken(token),
                        token_type = "grant_type",
                        expires_in = 1
                    };

                DtoAuthen replied = new DtoAuthen
                {
                    sap_logon_name = user.userIdName,
                    currentUser = user.displayName,
                    bearerToken = brearerToken,
                    currentGroup = user.currentGroup,
                    currentPermissions = user.currentPermissions,
                };  

                // prepare the cio as return to app
                //return Ok(new Cio
                //{
                //    sap_logon_name = user.userIdName,
                //    currentUser = user.displayName,
                //    bearerToken = brearerToken,
                //    currentGroup = user.currentGroup,
                //    currentPermissions = user.currentPermissions,
                //});

                // return Ok(new Cio());
                return Ok(replied);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                return BadRequest(excep.Message);
            }
        }
    }
}
