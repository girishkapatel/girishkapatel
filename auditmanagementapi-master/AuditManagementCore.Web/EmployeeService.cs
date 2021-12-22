using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Security;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.ViewModels;
using AuditManagementCore.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AuditManagementCore.Models.AuditConstants;
using EmailModel = AuditManagementCore.Service.Utilities.EmailModel;

namespace AuditManagementCore.Web
{
    public class EmployeeService : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundEmailSending> logger;
        public int number = 0;
        private Timer timer;
        IMongoDbSettings _dbsetting;
        IEmailUtility _IEmailUtility;
        IWebHostEnvironment _IWebHostEnvironment;
        IGlobalConfiguration _globalConfig;
        private readonly IEncryption encryption;
        public string serviceName = "Employee Service";
        CommonServices _CommonServices;

        public EmployeeService(ILogger<BackgroundEmailSending> logger, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility, IGlobalConfiguration config, IEncryption Encryption,CommonServices cs)
        {
            encryption = Encryption;
            this.logger = logger;
            _dbsetting = mongoDbSettings;
            _IEmailUtility = emailUtility;
            _IWebHostEnvironment = webHostEnvironment;
            _globalConfig = config;
            _CommonServices = cs;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(TaskRoutine, cancellationToken);
            return Task.CompletedTask;
        }
        public Task TaskRoutine()
        {
            while (true)
            {
                var _repoServiceConfiguration = new MongoGenericRepository<ServiceConfiguration>(_dbsetting);
                var lstService = _repoServiceConfiguration.GetAll();
                if (lstService != null)
                {
                    if (lstService.Count() > 0)
                    {
                        foreach (var item in lstService)
                        {
                            //var setHour = _globalConfig.EmployeeAPISetHours;
                            //Wait  8 am  daily service till next execution
                            TimeSpan first = TimeSpan.Parse(item.EmployeeAPISetHours);
                            TimeSpan second = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));

                            if (first.CompareTo(second) == 0)
                            {
                                MigrateEmployee(item);
                                //DateTime nextStop = DateTime.Now.AddHours(24);
                                //var timeToWait = nextStop - DateTime.Now;
                                //var millisToWait = timeToWait.TotalMilliseconds;
                                //Thread.Sleep((int)millisToWait);
                            }
                        }
                    }
                }
            }
        }
        public void MigrateEmployee(ServiceConfiguration objServiceConfiguration)
        {
          
            try
            {
                _CommonServices.ServiceLog(serviceName, "Start", "", "", "MigrateEmployee()");
                var _repoUser = new MongoGenericRepository<User>(_dbsetting);
                var repoRole = new MongoGenericRepository<Role>(_dbsetting);

                RestClient client = new RestClient("https://portal.zinghr.com/2015/route/EmployeeDetails/GetEmployeebasicDetails");

                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Cookie", "BNI_persistence=t3S_ZqsyV_T2UPQov9UE8BF89d6cr8QkLuH2CsooWXIui-Bl-eWzJpj5Is34lPh6CSeQQuBLh7LjePv3-vCysQ==; BNIS_vid=sm3rYqeGWz11OG3S+9BJWkUC1vzHBE71NQOSHip1WzlXBcuPm670GoLYwpnHX6qOwMoFM/T0BatGC5nvJ6YU2qXag/leJIV/885aFYKGm/k=; ASP.NET_SessionId=2konsoake3up3pj2gqigpwbr; BNIS___utm_is1=A3wbHI2CtANc7Xy0Zwj85RQJkqTVmLYK4GeVQ23AdSSKda7uKKgSuDvc1BEFXDJbkFKsRBTd1PUqn492G/vCzW3KqA0dO2uPKJhZjelT5L7JNjrw4h494Q==; BNIS___utm_is2=9CFIGtnOQYof5TJAvQo8goGT3x4NmM3V2K8Swd49gpwmULoTp6OcDMsGQop9jHMhpVu5K53UiOY=; BNIS___utm_is3=MYEWsP699CcPqXJ/vZIGkzhYGr/Y6sASQCAktVNMb/nzbenmtsKyptg5Ct6qQ/1atD5X3kCzRGdQ+PVm5Lx7s46A7Is0txg9INuyjzjNbjjiibcrKPKOeQ==");
                request.AddParameter("application/json", "{\r\n    \"SubscriptionName\": \"CFPL\",\r\n    \"Token\": \"a87356a5c3ad4865ad8cf7521ca663f2\",\r\n    \"AttributeTypeCode\": \"EmployeeSubType\",\r\n    \"AttributeTypeUnitCode\": \"Onroll\"\r\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Employee lstEmp = JsonConvert.DeserializeObject<Employee>(response.Content);
                if (lstEmp != null && lstEmp.Employees != null)
                {
                    var lstEmployee = lstEmp.Employees.Where(p => p.Attributes.Any(p => p.AttributeTypeUnitCode.ToLower() == "onroll") && (p.EmployeeStatus.ToLower() == "existing" || p.EmployeeStatus.ToLower() == "new")).ToList();

                    foreach (var objEmployee in lstEmployee)
                    {
                        #region Migrate db
                        var isExists = true;

                        var exists = _repoUser.GetFirst(p => p.EmailId == objEmployee.Email);
                        if (exists == null)
                        {
                            isExists = false;
                            exists = new User();
                        }

                        exists.EmailId = objEmployee.Email;
                        exists.FirstName = objEmployee.FirstName;
                        exists.MiddleName = objEmployee.MiddleName;
                        exists.LastName = objEmployee.LastName;
                        exists.Mobile = objEmployee.Mobile;
                        exists.Designation = "N/A";
                        exists.Qualification = "";

                        exists.StakeHolder = true;
                        exists.IsActive = true;
                        exists.Experiance = 0;
                        var role = repoRole.GetFirst(p => p.Name.ToLower() == "auditee");
                        exists.RoleId = role != null ? role.Id : null;
                        exists.Role = role != null ? role : null;
                        exists.ReportToId = null;
                        exists.UserType = "zinghr_api";
                        exists.EmployeeCode = objEmployee.EmployeeCode;
                        if (isExists)
                        {
                            _repoUser.Update(exists);
                        }
                        else
                        {
                            //Random Password
                            var randompass = new CommonServices(_dbsetting);
                            exists.Password = encryption.Encrypt(randompass.RandomPassword());
                            _repoUser.Insert(exists);
                            if (objServiceConfiguration.EmployeeSendMail)
                                SendMail(exists);
                        }
                        #endregion
                    }
                    var lstUser = _repoUser.GetWithInclude<User>(p => p.UserType == "zinghr_api");
                    foreach (var user in lstUser)
                    {
                        var objUser = lstEmployee.Where(p => p.Email == user.EmailId).FirstOrDefault();
                        if (objUser != null)
                        {
                            var objReporter = _repoUser.GetFirst(p => p.EmailId == objUser.ReportingManagerEmail);
                            user.ReportToId = objReporter != null ? objReporter.Id : null;
                            _repoUser.Update(user);
                        }
                    }
                }
                else
                {
                    _CommonServices.ServiceLog(serviceName, "Employee API: " + lstEmp.Message, "", null, "MigrateEmployee()");

                    // when error is occured then call next 1 mint 
                    DateTime nextStop = DateTime.Now.AddMinutes(1);
                    var timeToWait = nextStop - DateTime.Now;
                    var millisToWait = timeToWait.TotalMilliseconds;
                    Thread.Sleep((int)millisToWait);
                }
            }
            catch (Exception e)
            {
                _CommonServices.ServiceLog(serviceName, "Error", e.Message, e.StackTrace, "MigrateEmployee()");
                // when error is occured then call next 1 mint 
                DateTime nextStop = DateTime.Now.AddMinutes(1);
                var timeToWait = nextStop - DateTime.Now;
                var millisToWait = timeToWait.TotalMilliseconds;
                Thread.Sleep((int)millisToWait);
            }
            _CommonServices.ServiceLog(serviceName, "End", "", "", "MigrateEmployee()");
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Sending mail");
            return Task.CompletedTask;
        }
        public void SendMail(User userModel)
        {
             try
            {
                //userModel.EmailId= "shailesh.jain@in.ey.com";
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
                    .Replace("#appendlink#", _globalConfig.WebURL + "/resetpassword?user=" + userModel.Id);

                emailModel.ToEmail = new List<string>() { userModel.EmailId };
                emailModel.Subject = "Welcom aboard, " + userModel.FirstName + " " + userModel.LastName;
                emailModel.MailBody = emailBody.ToString();

                _IEmailUtility.SendEmail(emailModel);
                _CommonServices.ServiceLog(serviceName, userModel.EmailId + " mail send successfully", "", "", "SendMail()");

            }
            catch (Exception e)
            {
                _CommonServices.ServiceLog(serviceName, "Error", e.Message, e.StackTrace, "SendMail()");
            }
        }
    }
}