using AuditManagementCore.Models;
using AuditManagementCore.MongoDb.IUnitOfWork;
using AuditManagementCore.Service.Authentication;
using AuditManagementCore.Service.UserService;
using AuditManagementCore.ViewModels;
using AuditManagementCore.Web.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service.Security;
using AuditManagementCore.Service.Utilities;
using OfficeOpenXml;
using IAuthenticationService = AuditManagementCore.Service.Authentication.IAuthenticationService;
using EmailModel = AuditManagementCore.Service.Utilities.EmailModel;
using AuditManagementCore.Service;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using RestSharp;
using System.Runtime.Serialization.Json;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : VJBaseApiController, IAuthenticationService
    {
        public IUserService _userService;
        private readonly IEmailUtility _IEmailUtility;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        IMongoDbSettings _dbsetting;
        private readonly HttpClient client;
        private readonly IEncryption encryption;
        private readonly IGlobalConfiguration _globalConfig;


        public AuthenticationController(IUserService userService, IEmailUtility emailUtility, IWebHostEnvironment webHostEnvironment, IMongoDbSettings mongoDbSettings, IEncryption Encryption, IGlobalConfiguration config)
        {
            encryption = Encryption;
            _dbsetting = mongoDbSettings;
            _userService = userService;
            _IEmailUtility = emailUtility;
            _IWebHostEnvironment = webHostEnvironment;
            _globalConfig = config;
        }

        [HttpPost("Login")]
        public ActionResult Login(UserViewModel userRes)
        {

            if (userRes != null && (!string.IsNullOrWhiteSpace(userRes.EmailId) || !string.IsNullOrWhiteSpace(userRes.Password)))
            {
                var user = _userService.ValidateUser(userRes.EmailId, userRes.Password);

                if (user != null)
                {
                    var userdetails = UserDetails(user);
                    return new JsonResult(userdetails);
                }
                return UnauthorizedResponseError("Please enter valid credentials");
            }
            return UnauthorizedResponseError("EmailId or Password is blank");
        }

        [HttpGet("LogOut/{id}")]
        public ActionResult LogOff(string id)
        {
            //SignOut();
            //Activity Log
            CommonServices obj = new CommonServices(_dbsetting);
            obj.ActivityLog(id, "", "", "User", "Logout", "Logout User");
            return ResponseOK(null);
        }

        [NonAction]
        public string Token(User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the secret that needs to be at least 16 characeters long for HmacSha256"));
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                    new Claim(JwtRegisteredClaimNames.Email, user.EmailId),
                    new Claim(ClaimTypes.Name, user.EmailId),
                    new Claim(ClaimTypes.Role, user.Role.Name),
                    new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}"),
                    new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")
                };
            if (user.Role != null && user.Role.Scopes != null)
            {
                claims.Add(new Claim("scope", string.Join(";", user.Role.Scopes)));
            }
            var token = new JwtSecurityToken(new JwtHeader(new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }
        [NonAction]
        public async void SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        [HttpPost("forgotpassword")]
        public ActionResult ForgotPassword(ForgotPasswordModel userRes)
        {
            if (userRes != null && !string.IsNullOrWhiteSpace(userRes.EmailId))
            {
                var user = _userService.ValidateEmail(userRes.EmailId);

                if (user != null)
                {
                    sendMail(user, userRes.ResetURL);
                    return ResponseOK(new { sent = true });
                }
                return ResponseError("Please enter valid EmailId");
            }
            return ResponseError("EmailId is blank");
        }

        public void sendMail(User userModel, string resetURL)
        {
            //userModel.EmailId = "baldev@silverwebbuzz.com";

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "ForgotPassword.html");

            var emailBody = new StringBuilder();
            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }
            EmailModel emailModel = new EmailModel();
            emailBody = emailBody
                .Replace("#url#", resetURL);

            emailModel.ToEmail = new List<string>() { userModel.EmailId };
            emailModel.Subject = "Forgot Password";
            emailModel.MailBody = emailBody.ToString();

            _IEmailUtility.SendEmail(emailModel);
        }
        [HttpPost("resetpassword")]
        public ActionResult ResetPassword(UserViewModel userRes)
        {

            if (userRes != null && (!string.IsNullOrWhiteSpace(userRes.EmailId) || !string.IsNullOrWhiteSpace(userRes.Password)))
            {
                var user = _userService.UpdateValidateResetUser(userRes.EmailId, userRes.Password);

                if (user != null)
                {
                    return new JsonResult(user);
                }
                return ResponseError("Please enter valid credentials");
            }
            return ResponseError("EmailId or Password is blank");
        }
        [HttpPost("checkmail")]
        public ActionResult CheckEmail(ForgotPasswordModel userRes)
        {
            if (userRes != null && !string.IsNullOrWhiteSpace(userRes.EmailId))
            {
                var user = _userService.ValidateEmail(userRes.EmailId);

                if (user != null)
                {
                    return ResponseOK(user);
                }
                return ResponseError("EmailId does not exist , Please contact to administator");
            }
            return ResponseError("EmailId does not exist , Please contact to administator");
        }
        public void sendMail(User userModel)
        {
            //string EmailId = "baldev@silverwebbuzz.com";

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "User.html");

            var emailBody = new StringBuilder();
            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }
            EmailModel emailModel = new EmailModel();
            emailBody = emailBody
                .Replace("#uname#", userModel.FirstName + " " + userModel.LastName)
                .Replace("#username#", userModel.EmailId)
                .Replace("#password#", encryption.Decrypt(userModel.Password));

            emailModel.ToEmail = new List<string>() { userModel.EmailId };
            emailModel.Subject = "Welcom aboard, " + userModel.FirstName + " " + userModel.LastName;
            emailModel.MailBody = emailBody.ToString();

            _IEmailUtility.SendEmail(emailModel);
        }
        public ExpandoObject UserDetails(User user)
        {
            dynamic tokenObj = new ExpandoObject();
            tokenObj.Token = Token(user);
            tokenObj.UserId = user.Id;
            tokenObj.FirstName = user.FirstName;
            tokenObj.MiddleName = user.MiddleName;
            tokenObj.LastName = user.LastName;
            tokenObj.Mobile = user.Mobile;
            tokenObj.EmailId = user.EmailId;
            tokenObj.Role = user.Role.Name;
            tokenObj.StakeHolder = user.StakeHolder;
            tokenObj.UIScopes = user.Role.UIScopes;
            #region lstmenu  when user is stackholder
            List<UIScope> lstMenu = new List<UIScope>();

            //if (user.StakeHolder)
            //{
            //    foreach (var item in user.Role.UIScopes)
            //    {
            //        UIScope objsubmodule = new UIScope();
            //        objsubmodule.isAdd = false;
            //        objsubmodule.isEdit = false;
            //        objsubmodule.isDelete = false;
            //        objsubmodule.Access = true;
            //        objsubmodule.Name = item.Name;
            //        List<SubModules> lstsubmodule = new List<SubModules>();
            //        foreach (var submodule in item.Submodules)
            //        {
            //            SubModules objsub = new SubModules();
            //            objsub.isAdd = false;
            //            objsub.isEdit = false;
            //            objsub.isDelete = false;
            //            objsub.Access = true;
            //            objsub.Name = submodule.Name;
            //            List<SubModule> lstsubmod = new List<SubModule>();
            //            if (submodule.Submodules != null)
            //            {
            //                foreach (var itemsub in submodule.Submodules)
            //                {
            //                    SubModule objsubm = new SubModule();
            //                    objsubm.isAdd = false;
            //                    objsubm.isEdit = false;
            //                    objsubm.isDelete = false;
            //                    objsubm.Access = true;
            //                    objsubm.Name = itemsub.Name;
            //                    lstsubmod.Add(objsubm);
            //                }
            //            }
            //            objsub.Submodules = lstsubmod;
            //            lstsubmodule.Add(objsub);
            //        }
            //        objsubmodule.Submodules = lstsubmodule;
            //        lstMenu.Add(objsubmodule);
            //    }
            //    tokenObj.UIScopes = lstMenu;
            //}
            //else
            //{
            //    tokenObj.UIScopes = user.Role.UIScopes;
            //}
            #endregion
            //Activity Log
            CommonServices obj = new CommonServices(_dbsetting);
            obj.ActivityLog(user.Id, "", "", "User", "Login", "Login User");
            return tokenObj;
        }

    }
}
