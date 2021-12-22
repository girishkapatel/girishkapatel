using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VJLiabraries.GenericRepository;
using OfficeOpenXml;
using System.IO;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Drawing;

namespace AuditManagementCore.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StakeHolderController : VJBaseGenericAPIController<User>
    {
        private readonly IEncryption encryption;
        IMongoDbSettings _dbsetting;
        private readonly IEmailUtility _IEmailUtility;
        IGlobalConfiguration _globalConfig;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        CommonServices _CommonServices;
        public StakeHolderController(IMongoGenericRepository<User> api, IEncryption Encryption, IMongoDbSettings mongoDbSettings, IEmailUtility emailUtility, IWebHostEnvironment webHostEnvironment, IGlobalConfiguration config, CommonServices cs) : base(api)
        {
            encryption = Encryption;
            _dbsetting = mongoDbSettings;
            _IEmailUtility = emailUtility;
            _IWebHostEnvironment = webHostEnvironment;
            _globalConfig = config;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] User e)
        {
            var isExist = _api.Exists(x => x.EmailId.ToLower() == e.EmailId.ToLower());
            if (isExist)
            {
                return ResponseError("Email Id is already Exists");
            }
            e.Password = encryption.Encrypt(e.Password);
            e.StakeHolder = true;
            e.UserType = "manual";
            var StakeHolder = base.Post(e);
            sendMail(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "User", "StakeHolder | Add", "Added StakeHolder");
            return StakeHolder;
        }
        public override ActionResult Put([FromBody] User tValue)
        {
            var tList = _api.GetWithInclude<Role, Company>(x => x.Id == tValue.Id && x.StakeHolder == true);
            if (tList == null)
            {
                return ResponseError("User does not exists.");
            }
            var user = _api.GetFirst(x => x.Id == tValue.Id);
            tValue.CreatedBy = user.CreatedBy;
            tValue.CreatedOn = user.CreatedOn;
            tValue.Password = encryption.Encrypt(tValue.Password);
            tValue.StakeHolder = true;
            sendMail(tValue);
            var stackHolder = base.Put(tValue);
            //Activity Log
            _CommonServices.ActivityLog(tValue.UpdatedBy, tValue.Id, "", "User", "StakeHolder | Edit", "Updated StakeHolder");
            return stackHolder;
        }

        public override ActionResult Delete(string id, string userid)
        {
            var tList = _api.GetWithInclude<Role, Company>(x => x.Id == id && x.StakeHolder == true);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var stackholder = base.Delete(id, userid);
            //Activity Log
            _CommonServices.ActivityLog(userid, id, "User", "", "StakeHolder | Delete", "Deleted StakeHolder");
            return stackholder;
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetWithInclude<Role, Company, User>(x => x.StakeHolder == true).OrderBy(p => p.FirstName);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                if (item.ReportToId != null)
                {
                    var user = userRepo.GetByID(item.ReportToId);
                    item.ReportTo = user;
                }
                item.Password = encryption.Decrypt(item.Password);
            }
            return ResponseOK(tList);
        }
        [HttpGet("getallstockuser")]
        public ActionResult GetAllStockUser()
        {
            var tList = _api.GetWithInclude<Role, Company, User>(x => x.StakeHolder == true).OrderBy(p => p.FirstName);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
        [HttpGet("getStackHolderServerSide")]
        public IActionResult getStackHolderServerSide()
        {
            var customerData = _api.GetWithInclude<Role, Company, User>(x => x.StakeHolder == true);
            if (customerData == null)
            {
                return ResponseNotFound();
            }
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            {
                //customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.FirstName.Contains(searchValue)
                                            || m.LastName.Contains(searchValue));
            }
            recordsTotal = customerData.Count();
            var data = customerData.Skip(skip).Take(pageSize).ToList();
            var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
            return ResponseOK(jsonData);
        }
        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<Role, Company>(x => x.Id == id && x.StakeHolder == true);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetWithInclude<User>(p => p.StakeHolder == true);
            if (tList == null)
                return ResponseNotFound();

            var roleRepo = new MongoGenericRepository<Role>(_dbsetting);
            var fileName = "StackHolder.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "First Name*";
                worksheet.Cells["B1"].Value = "Middle Name";
                worksheet.Cells["C1:D1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["C1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["C1:D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["C1"].Value = "Last Name*";
                worksheet.Cells["D1"].Value = "Email Id*";
                worksheet.Cells["E1"].Value = "Status";
                worksheet.Cells["F1:I1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["F1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["F1:I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["F1"].Value = "Designation*";
                worksheet.Cells["G1"].Value = "Work Experience*";
                worksheet.Cells["H1"].Value = "Qualification*";
                worksheet.Cells["I1"].Value = "Role*";
                worksheet.Cells["J1"].Value = "Report To";
                worksheet.Cells["K1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["K1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["K1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["K1"].Value = "Password*";
                worksheet.Cells["L1"].Value = "Employee Code";
                var rowIndex = 2;
                foreach (var user in tList)
                {
                    var rolid = roleRepo.GetFirst(x => x.Id == user.RoleId);
                    var reportid = _api.GetFirst(x => x.Id == user.ReportToId);

                    worksheet.Cells["A" + rowIndex.ToString()].Value = user.FirstName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = user.MiddleName;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = user.LastName;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = user.EmailId;
                    worksheet.Cells["E" + rowIndex.ToString()].Value = user.IsActive;
                    worksheet.Cells["F" + rowIndex.ToString()].Value = user.Designation;
                    worksheet.Cells["G" + rowIndex.ToString()].Value = user.Experiance;
                    worksheet.Cells["H" + rowIndex.ToString()].Value = user.Qualification;
                    worksheet.Cells["I" + rowIndex.ToString()].Value = (rolid == null ? "" : rolid.Name);
                    worksheet.Cells["J" + rowIndex.ToString()].Value = (reportid == null ? "" : reportid.EmailId);
                    worksheet.Cells["K" + rowIndex.ToString()].Value = encryption.Decrypt(user.Password);
                    worksheet.Cells["L" + rowIndex.ToString()].Value = user.EmployeeCode;
                    rowIndex++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("sampledownloadexcel")]
        public IActionResult SampleDownloadExcel()
        {
            var fileName = "StackHolder.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "First Name*";
                worksheet.Cells["B1"].Value = "Middle Name";
                worksheet.Cells["C1:D1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["C1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["C1:D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["C1"].Value = "Last Name*";
                worksheet.Cells["D1"].Value = "Email Id*";
                worksheet.Cells["E1"].Value = "Status";
                worksheet.Cells["F1:I1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["F1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["F1:I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["F1"].Value = "Designation*";
                worksheet.Cells["G1"].Value = "Work Experience*";
                worksheet.Cells["H1"].Value = "Qualification*";
                worksheet.Cells["I1"].Value = "Role*";
                worksheet.Cells["J1"].Value = "Report To";
                worksheet.Cells["K1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["K1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["K1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["K1"].Value = "Password*";
                worksheet.Cells["L1"].Value = "Employee Code";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("importexcel")]
        public ActionResult ImportUserExcel()
        {
            try
            {
                int ExceptionrowCount = 0;
                int TotalRow = 0;
                StringBuilder sb = new StringBuilder();

                if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                    return ResponseError("formfile is empty");

                var file = Request.Form.Files[0];

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return ResponseError("Not Support file extension");

                var roleRepo = new MongoGenericRepository<Role>(_dbsetting);
                var designationRepo = new MongoGenericRepository<Designation>(_dbsetting);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                        var rowCount = worksheet != null ? worksheet.Dimension.Rows : 0;
                        TotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var firstName = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : "";
                                var middleName = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().Trim() : "";
                                var lastName = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : "";
                                var emailid = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : "";
                                var isActive = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : "true";

                                var designation = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : null;
                                var experience = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : "0";
                                var qualification = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : "";
                                var role = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : null;
                                var reportToId = worksheet.Cells[row, 10].Value != null ? worksheet.Cells[row, 10].Value.ToString() : null;
                                var password = worksheet.Cells[row, 11].Value != null ? worksheet.Cells[row, 11].Value.ToString() : "";
                                var empCode = worksheet.Cells[row, 12].Value != null ? worksheet.Cells[row, 12].Value.ToString() : "";

                                var rolid = roleRepo.GetFirst(x => x.Name == role);
                                var reportid = _api.GetFirst(x => x.EmailId == reportToId);
                                var designationid = designationRepo.GetFirst(x => x.Name.Contains(designation));

                                var exists = _api.GetFirst(a => a.EmailId.Trim() == emailid);
                                if (exists != null)
                                {
                                    exists.FirstName = firstName;
                                    exists.MiddleName = middleName;
                                    exists.LastName = lastName;
                                    exists.EmailId = emailid;
                                    exists.IsActive = Convert.ToBoolean(isActive);
                                    exists.Designation = designationid != null ? designationid.Name : null;
                                    exists.Experiance = Convert.ToInt32(experience);
                                    exists.Qualification = qualification;
                                    exists.RoleId = rolid != null ? rolid.Id : null;
                                    exists.ReportToId = reportid != null ? reportid.Id : null;
                                    exists.Password = encryption.Encrypt(password);
                                    exists.StakeHolder = true;
                                    _api.Update(exists);
                                }
                                else
                                {
                                    exists = new User()
                                    {
                                        FirstName = firstName,
                                        MiddleName = middleName,
                                        LastName = lastName,
                                        EmailId = emailid,
                                        IsActive = Convert.ToBoolean(isActive),
                                        Designation = designationid != null ? designationid.Name : null,
                                        Experiance = Convert.ToInt32(experience),
                                        Qualification = qualification,
                                        RoleId = rolid != null ? rolid.Id : null,
                                        ReportToId = reportid != null ? reportid.Id : null,
                                        Password = encryption.Encrypt(password),
                                        StakeHolder = true,
                                        UserType = "manual"
                                    };
                                    _api.Insert(exists);
                                    sendMail(exists);
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "StackHolder/ImportExcel()");
                            }
                        }
                    }
                }
                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "StackHolder/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
        public void sendMail(User userModel)
        {
            /*userModel.EmailId= "mayursasp.net@gmail.com";*/
            var repoUser = new MongoGenericRepository<User>(_dbsetting);

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
                    .Replace("#UserName#", userModel.EmailId)
                    .Replace("#appendlink#", _globalConfig.WebURL + "/resetpassword?user=" + userModel.Id);

            emailModel.ToEmail = new List<string>() { userModel.EmailId };
            if (userModel.ReportToId != null)
            {
                var reportManager = repoUser.GetFirst(p => p.Id == userModel.ReportToId);
                if (reportManager != null)
                    emailModel.CcEmail = new List<string>() { reportManager.EmailId };
            }
            emailModel.Subject = "Welcom aboard, " + userModel.FirstName + " " + userModel.LastName;
            emailModel.MailBody = emailBody.ToString();
            _IEmailUtility.SendEmail(emailModel);
        }

    }
}