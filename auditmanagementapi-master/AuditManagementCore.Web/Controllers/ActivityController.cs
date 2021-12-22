using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : VJBaseGenericAPIController<Activity>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;

        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private readonly IEmailUtility _IEmailUtility;
        CommonServices _CommonServices;
        #endregion

        public ActivityController
            (IMongoGenericRepository<Activity> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _IEmailUtility = emailUtility;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Activity e)
        {
            var isExist = _api.Exists(x => x.ActivityName?.ToLower() == e.ActivityName.ToLower() && x.AuditID == e.AuditID);

            if (isExist)
                return ResponseError("Activity Name already Exists for this audit");

            e.PlannedStartDate = e.PlannedStartDate.ToLocalTime();
            e.PlannedEndDate = e.PlannedEndDate.ToLocalTime();
            e.ActualStartDate = e.ActualStartDate.ToLocalTime();
            e.ActualEndDate = e.ActualEndDate.ToLocalTime();

            var Activity = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.ActivityName, "Activity", "Manage Audits | Planning | Activity | Add", "Added Activity");
            return Activity;
        }

        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.Id == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            tList = FetchAllRequiredData(tList);
            return ResponseOK(tList);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Audit>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            tList = FetchAllRequiredData(tList);
            return ResponseOK(tList);
        }

        [HttpGet("GetTemplates")]
        public ActionResult GetTemplates()
        {
            var tList = new List<Activity>()
            {
                new Activity(){ ActivityName = "TOR circulated"},
                new Activity(){ ActivityName = "Kick off meeting announcement"},
                new Activity(){ ActivityName = "Planning of an audit"},
                new Activity(){ ActivityName = "Actual kick off meeting"},
                new Activity(){ ActivityName = "MoM circulation"},
                new Activity(){ ActivityName = "Weekly status update"},
                new Activity(){ ActivityName = "Walk through"},
                new Activity(){ ActivityName = "Release of initial issue"},
                new Activity(){ ActivityName = "Discussion and validation"},
                new Activity(){ ActivityName = "Release of draft report"}
            };
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        public override ActionResult Put([FromBody] Activity tValue)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            tValue.PlannedStartDate = tValue.PlannedStartDate.ToLocalTime();
            tValue.PlannedEndDate = tValue.PlannedEndDate.ToLocalTime();
            tValue.ActualStartDate = tValue.ActualStartDate.ToLocalTime();
            tValue.ActualEndDate = tValue.ActualEndDate.ToLocalTime();

            var Activity = base.Put(tValue);

            //Activity Log
            _CommonServices.ActivityLog(tValue.UpdatedBy, tValue.Id, tValue.ActivityName, "Activity", "Manage Audits | Planning | Activity | Edit", "Updated Activity");
            return Activity;
        }

        public override ActionResult Delete(string id, string userid)
        {
            //Activity Log
            if (id == null) return ResponseBad("Activity object is null");
            var objActivity = _api.GetFirst(x => x.Id == id);

            if (objActivity == null)
            {
                return ResponseError("Activity does not exists");
            }

            var activity = base.Delete(id, userid);
            _CommonServices.ActivityLog(userid, id, objActivity.ActivityName, "Activity", "Manage Audits | Planning | Activity | Delete", "Deleted Activity");
            return activity;
        }

        [HttpGet("GetByAudit/{id}/{option}")]
        public ActionResult GetByAudit(string id, string option)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditID == id);

            if (tList == null)
                return ResponseNotFound();

            if (option.Trim() != "" && option.ToLower().Trim() != "all")
            {
                if (option.ToLower().Trim() == "due")
                    tList = tList
                        .Where(a => a.ActualEndDate >= DateTime.Now && a.ActivityStatus.ToLower().Trim() != "completed" && a.ActivityStatus.ToLower().Trim() != "inprogress");
                else if (option.ToLower().Trim() == "completed")
                    tList = tList.Where(a => a.ActivityStatus.ToLower().Trim() == "completed");
                else if (option.ToLower().Trim() == "delayed")
                    tList = tList
                        .Where(a => a.ActualEndDate < DateTime.Now && a.ActivityStatus.ToLower().Trim() != "completed" && a.ActivityStatus.ToLower().Trim() != "inprogress");
                else if (option.ToLower().Trim() == "inprogress")
                    tList = tList.Where(a => a.ActivityStatus.ToLower().Trim() == "inprogress");
            }

            tList = FetchAllRequiredData(tList);

            return ResponseOK(tList);
        }

        [HttpGet("GetActivity")]
        public ActionResult GetActivity()
        {
            var tList = GetActivityData();
            return ResponseOK(tList);
        }
        public IQueryable<Activity> GetActivityData()
        {
            var tList = _api.GetAllWithInclude<Audit>();
            if (tList != null)
            {
                var PlannedStartDate = Request.Query["PlannedStartDate"];
                var PlannedEndDate = Request.Query["PlannedEndDate"];
                var ActualStartDate = Request.Query["ActualStartDate"];
                var ActualEndDate = Request.Query["ActualEndDate"];
                var Status = Request.Query["Status"];
                var Location = Request.Query["Location"];
                var Audit = Request.Query["Audit"];

                if (!string.IsNullOrWhiteSpace(PlannedStartDate) && !string.IsNullOrWhiteSpace(PlannedEndDate))
                {
                    tList = tList.Where(x => x.PlannedStartDate != null && x.PlannedEndDate != null && (DateTime.Parse(PlannedStartDate) <= x.PlannedStartDate) && (DateTime.Parse(PlannedEndDate) >= x.PlannedEndDate));
                }

                if (!string.IsNullOrWhiteSpace(ActualStartDate) && !string.IsNullOrWhiteSpace(ActualEndDate))
                {
                    tList = tList.Where(x => x.ActualStartDate != null && x.ActualEndDate != null && (DateTime.Parse(ActualStartDate) <= x.ActualStartDate) && (DateTime.Parse(ActualEndDate) >= x.ActualEndDate));
                }
                if (!string.IsNullOrWhiteSpace(Status))
                {
                    tList = tList.Where(x => x.ActivityStatus.ToLower() == Status.ToString().ToLower());
                }

                tList = FetchAllRequiredData(tList);
                if (!string.IsNullOrWhiteSpace(Location))
                    tList = tList.Where(x => x.Audit.Location.Id == Location);
                if (!string.IsNullOrWhiteSpace(Audit))
                    tList = tList.Where(x => x.Audit.ProcessLocationMapping.Id == Audit);
            }
            return tList;
        }

        private IQueryable<Activity> FetchAllRequiredData(IQueryable<Activity> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var bcRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
            var pl1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
            var pl2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);
            var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            foreach (var item in tList)
            {
                if (item.PersonResponsibleID != null && item.PersonResponsibleID != "")
                    item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);

                if (item.Audit.ProcessLocationMapping != null)
                {
                    item.Audit.ProcessLocationMapping = plmRepo.GetByID(item.Audit.ProcessLocationMapping.Id);
                }
                if (item.Audit.ProcessLocationMapping.BusinessCycles != null)
                {
                    item.Audit.ProcessLocationMapping.BusinessCycle = bcRepo.GetByID(item.Audit.ProcessLocationMapping.BusinessCycleID);
                }

                if (item.Audit.ProcessLocationMapping.ProcessL1ID != null)
                {
                    item.Audit.ProcessLocationMapping.ProcessL1 = pl1Repo.GetByID(item.Audit.ProcessLocationMapping.ProcessL1ID);
                }

                if (item.Audit.ProcessLocationMapping.ProcessL2ID != null)
                {
                    item.Audit.ProcessLocationMapping.ProcessL2 = pl2Repo.GetByID(item.Audit.ProcessLocationMapping.ProcessL2ID);
                }

            }
            return tList;
        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail([FromBody] EmailModel emailModel)
        {
            try
            {
                var tList = _api.GetWithInclude<Activity>(x => x.Id == emailModel.Id).FirstOrDefault();

                if (tList == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetByID(tList.PersonResponsibleID);

                if (userModel == null)
                    return ResponseNotFound();

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(tList.AuditID);

                if (auditModel == null)
                    return ResponseNotFound();

                var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                if (companyModel == null)
                    return ResponseNotFound();

                var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                if (scopeModel == null)
                    return ResponseNotFound();

                //userModel.EmailId = "baldev@silverwebbuzz.com";

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "ActivitySendToResponsible.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                var auditPeriod = auditStartDate + " to " + auditEndDate;

                emailBody = emailBody
                    .Replace("#ResponsibleName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#AuditName#", auditModel.AuditName)
                    .Replace("#ActivityName#", tList.ActivityName)
                    .Replace("#targetenddate#", tList.PlannedEndDate.ToShortDateString())
                    .Replace("#AuditPeriod#", auditPeriod);

                emailModel.ToEmail = new List<string>() { userModel.EmailId };
                emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Activity";
                emailModel.MailBody = emailBody.ToString();

                _IEmailUtility.SendEmail(emailModel);

                return ResponseOK(new { sent = true });
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpPost("importexcel/{id}")]
        public ActionResult ImportExcel(string id)
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

                var _processLocationMapRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var userRepo = new MongoGenericRepository<User>(_dbsetting);


                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        int rowCount = 0;
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                        if (worksheet != null)
                        {
                            while (_CommonServices.IsLastRowEmpty(worksheet))
                                worksheet.DeleteRow(worksheet.Dimension.End.Row);
                            rowCount = worksheet.Dimension.Rows;
                            TotalRow = rowCount;
                        }

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var activityName = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : null;

                                if (activityName != null)
                                {
                                    #region activity

                                    var responsiblePerson = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : "";
                                    var status = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : "";

                                    var isExists = true;

                                    var exists = _api.GetFirst(a => a.ActivityName.Trim() == activityName);

                                    if (exists == null)
                                    {
                                        isExists = false;
                                        exists = new Activity();
                                    }
                                    exists.ActivityName = activityName;
                                    var dateNumStart = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().Trim() : null;
                                    if (dateNumStart != null)
                                    {
                                        object value = worksheet.Cells[row, 2].Value;
                                        if (value != null)
                                        {
                                            if (value is double)
                                            {
                                                DateTime endDate = DateTime.FromOADate((double)value);
                                                exists.PlannedStartDate = endDate.ToLocalTime();
                                            }
                                            else
                                            {
                                                DateTime dt;
                                                DateTime.TryParse((string)dateNumStart, out dt);
                                                exists.PlannedStartDate = dt.ToLocalTime();
                                            }
                                        }
                                    }

                                    //double dateNumStart = worksheet.Cells[row, 2].Value != null ? double.Parse(worksheet.Cells[row, 2].Value.ToString().Trim()) : 0;
                                    //if (dateNumStart != 0)
                                    //{
                                    //    DateTime plannedStartDate = DateTime.FromOADate(dateNumStart);
                                    //    exists.PlannedStartDate = plannedStartDate.ToLocalTime();
                                    //}
                                    var dateNumEnd = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : null;
                                    if (dateNumEnd != null)
                                    {
                                        object value = worksheet.Cells[row, 3].Value;
                                        if (value != null)
                                        {
                                            if (value is double)
                                            {
                                                DateTime endDate = DateTime.FromOADate((double)value);
                                                exists.PlannedEndDate = endDate.ToLocalTime();
                                            }
                                            else
                                            {
                                                DateTime dt;
                                                DateTime.TryParse((string)dateNumEnd, out dt);
                                                exists.PlannedEndDate = dt.ToLocalTime();
                                            }
                                        }
                                    }
                                    //double dateNumEnd = worksheet.Cells[row, 3].Value != null ? double.Parse(worksheet.Cells[row, 3].Value.ToString().Trim()) : 0;
                                    //if (dateNumEnd != 0)
                                    //{
                                    //    DateTime plannedEndDate = DateTime.FromOADate(dateNumEnd);
                                    //    exists.PlannedEndDate = plannedEndDate.ToLocalTime();
                                    //}
                                    var actualSDate = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : null;
                                    if (actualSDate != null)
                                    {
                                        object value = worksheet.Cells[row, 4].Value;
                                        if (value != null)
                                        {
                                            if (value is double)
                                            {
                                                DateTime endDate = DateTime.FromOADate((double)value);
                                                exists.ActualStartDate = endDate.ToLocalTime();
                                            }
                                            else
                                            {
                                                DateTime dt;
                                                DateTime.TryParse((string)actualSDate, out dt);
                                                exists.ActualStartDate = dt.ToLocalTime();
                                            }
                                        }
                                    }
                                    //double actualSDate = worksheet.Cells[row, 4].Value != null ? double.Parse(worksheet.Cells[row, 4].Value.ToString().Trim()) : 0;
                                    //if (actualSDate != 0)
                                    //{
                                    //    DateTime actualStartDate = DateTime.FromOADate(actualSDate);
                                    //    exists.ActualStartDate = actualStartDate.ToLocalTime();
                                    //}
                                    var actualEDate = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : null;
                                    if (actualEDate != null)
                                    {
                                        object value = worksheet.Cells[row, 5].Value;
                                        if (value != null)
                                        {
                                            if (value is double)
                                            {
                                                DateTime endDate = DateTime.FromOADate((double)value);
                                                exists.ActualEndDate = endDate.ToLocalTime();
                                            }
                                            else
                                            {
                                                DateTime dt;
                                                DateTime.TryParse((string)actualEDate, out dt);
                                                exists.ActualEndDate = dt.ToLocalTime();
                                            }
                                        }
                                    }
                                    //double actualEDate = worksheet.Cells[row, 5].Value != null ? double.Parse(worksheet.Cells[row, 5].Value.ToString().Trim()) : 0;
                                    //if (actualEDate != 0)
                                    //{
                                    //    DateTime actualEndDate = DateTime.FromOADate(actualEDate);
                                    //    exists.ActualEndDate = actualEndDate.ToLocalTime();
                                    //}

                                    if (responsiblePerson != "")
                                    {
                                        var ResponsibleUser = userRepo.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsiblePerson.ToLower());
                                        if (ResponsibleUser != null)
                                            exists.PersonResponsibleID = ResponsibleUser.Id;
                                    }
                                    if (status != "")
                                    {
                                        exists.ActivityStatus = status;
                                    }
                                    exists.AuditID = id;

                                    if (isExists)
                                        _api.Update(exists);
                                    else
                                        _api.Insert(exists);
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "Activity/ImportExcel()");
                            }
                        }
                    }
                }
                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "Activity/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }


        [HttpGet("downloadexcel/{id}")]
        public IActionResult DownloadExcel(string id)
        {
            try
            {
                var tList = _api.GetWithInclude<Audit>(x => x.AuditID == id);
                if (tList == null)
                {
                    return ResponseNotFound();
                }
                tList = FetchAllRequiredData(tList);

                var repoUser = new MongoGenericRepository<User>(_dbsetting);
                var fileName = "Activity.xlsx";
                var memoryStream = new MemoryStream();

                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    worksheet.Cells["A1:E1"].Style.Font.Color.SetColor(Color.Red);
                    worksheet.Cells["A1:E1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A1:E1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["A1"].Value = "Activity Name*";
                    worksheet.Cells["B1"].Value = "Planned Timeline Start Date*";
                    worksheet.Cells["C1"].Value = "Planned Timeline End Date*";
                    worksheet.Cells["D1"].Value = "Actual Timeline Start Date*";
                    worksheet.Cells["E1"].Value = "Actual Timeline End Date*";
                    worksheet.Cells["F1"].Value = "Responsibility";
                    worksheet.Cells["G1"].Value = "Status";
                    var rowIndex = 2;

                    foreach (var activity in tList)
                    {
                        worksheet.Cells["A" + rowIndex.ToString()].Value = activity.ActivityName;
                        if (activity.PlannedStartDate != null)
                        {
                            worksheet.Cells["B" + rowIndex.ToString()].Value = Convert.ToDateTime(activity.PlannedStartDate).ToShortDateString();
                        }

                        if (activity.PlannedEndDate != null)
                        {
                            worksheet.Cells["C" + rowIndex.ToString()].Value = Convert.ToDateTime(activity.PlannedEndDate).ToShortDateString();
                        }

                        if (activity.ActualStartDate != null)
                        {
                            worksheet.Cells["D" + rowIndex.ToString()].Value = Convert.ToDateTime(activity.ActualStartDate).ToShortDateString();
                        }

                        if (activity.PlannedStartDate != null)
                        {
                            worksheet.Cells["E" + rowIndex.ToString()].Value = Convert.ToDateTime(activity.ActualEndDate).ToShortDateString();
                        }

                        if (activity.PersonResponsibleID != null && activity.PersonResponsibleID != "")
                        {
                            var objUser = repoUser.GetFirst(x => x.Id == activity.PersonResponsibleID);
                            worksheet.Cells["F" + rowIndex.ToString()].Value = objUser != null ? objUser.FirstName + " " + objUser.LastName : "";
                        }
                        worksheet.Cells["G" + rowIndex.ToString()].Value = activity.ActivityStatus;
                        rowIndex++;
                    }
                    worksheet.Column(2).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Column(3).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Column(4).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Column(5).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                    package.Save();
                }
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "Activity/DownloadExcel");
            }
            return Ok();
        }

        [HttpGet("sampledownloadexcel/{id}")]
        public IActionResult SampleDownloadExcel(string id)
        {
            try
            {
                var repoUser = new MongoGenericRepository<User>(_dbsetting);
                var fileName = "Activity.xlsx";
                var memoryStream = new MemoryStream();

                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    worksheet.Cells["A1:E1"].Style.Font.Color.SetColor(Color.Red);
                    worksheet.Cells["A1:E1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A1:E1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["A1"].Value = "Activity Name*";
                    worksheet.Cells["B1"].Value = "Planned Timeline Start Date*";
                    worksheet.Cells["C1"].Value = "Planned Timeline End Date*";
                    worksheet.Cells["D1"].Value = "Actual Timeline Start Date*";
                    worksheet.Cells["E1"].Value = "Actual Timeline End Date*";
                    worksheet.Cells["F1"].Value = "Responsibility";
                    worksheet.Cells["G1"].Value = "Status";

                    worksheet.Column(2).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Column(3).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Column(4).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Column(5).Style.Numberformat.Format = "dd-mm-yyyy";
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                    package.Save();
                }
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "Activity/SampleDownloadExcel");
            }
            return Ok();
        }

        [HttpGet("getsummary/{id}")]
        public ActionResult GetSummary(string id)
        {
            var summary = new ActivitySummary();

            var tList = _api.GetWithInclude<Audit>(x => x.AuditID == id);

            if (tList == null)
                return ResponseNotFound();

            summary.Due = tList
                .Count(a => a.ActualEndDate >= DateTime.Now && a.ActivityStatus.ToLower().Trim() != "completed" && a.ActivityStatus.ToLower().Trim() != "inprogress");

            summary.Completed = tList.Count(a => a.ActivityStatus.ToLower().Trim() == "completed");

            summary.Delayed = tList
                .Count(a => a.ActualEndDate < DateTime.Now && a.ActivityStatus.ToLower().Trim() != "completed" && a.ActivityStatus.ToLower().Trim() != "inprogress");

            summary.InProgress = tList.Count(a => a.ActivityStatus.ToLower().Trim() == "inprogress");

            return ResponseOK(summary);
        }


        [HttpGet("downloadpdf")]
        public IActionResult DownloadPDF()
        {
            var tList = GetActivityData();

            if (tList == null)
                return ResponseNotFound();

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlRowTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DataTrackerRowPDF.html");

            var mainRow = new StringBuilder();
            using (StreamReader streamReader = new StreamReader(htmlRowTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                mainRow.Append(htmlContent);
            }

            var rowsBuilder = new StringBuilder();
            var counter = 1;
            foreach (var item in tList)
            {
                rowsBuilder.Append(mainRow);

                //rowsBuilder = rowsBuilder
                //    .Replace("#Counter#", counter.ToString())
                //    .Replace("#Area#", item.Area)
                //    .Replace("#Status#", item.Status)
                //    .Replace("#DataRequested#", item.DataRequested)
                //    .Replace("#PendingData#", item.PendingData);

                //if (item.DataRequestDate != null)
                //    rowsBuilder = rowsBuilder.Replace("#DataRequestDate#", Convert.ToDateTime(item.DataRequestDate).ToString("dd-MM-yyyy"));

                //if (item.DataReceivedDate != null)
                //    rowsBuilder = rowsBuilder.Replace("#DataReceivedDate#", Convert.ToDateTime(item.DataReceivedDate).ToString("dd-MM-yyyy"));

                //if (item.ProcessOwner != null)
                //    rowsBuilder = rowsBuilder.Replace("#ProcessOwner#", item.ProcessOwner.FirstName + " " + item.ProcessOwner.LastName);

                counter++;
            }

            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DataTrackerPDF.html");

            var mainTable = new StringBuilder();
            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                mainTable.Append(htmlContent);
            }

            mainTable = mainTable.Replace("#DataTrackerRowPDF#", rowsBuilder.ToString());

            string wkhtmlexepath = Directory.GetCurrentDirectory() + "\\wkhtmltopdf";

            byte[] pdfbyte = UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", mainTable.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;

            return File(memoryStream, UtilityMethods.GetContentType(".pdf"), "ScopeAndSchedule.pdf");
        }

        [HttpGet("downloadexcelforReport")]
        public IActionResult DownloadExcelForReport()
        {
            var tList = GetActivityData();

            if (tList == null)
                return ResponseNotFound();

            var fileName = "ScopeAndSchedule.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].Value = "Audit";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Activity Name";
                worksheet.Cells["D1"].Value = "Planned Timeline";
                worksheet.Cells["E1"].Value = "Actual Timeline";
                worksheet.Cells["F1"].Value = "Status";

                var rowIndex = 2;

                foreach (var audit in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.AuditName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.Location.ProfitCenterCode;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = audit.ActivityName;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = audit.PlannedStartDate.ToShortDateString() + " - " + audit.PlannedEndDate.ToShortDateString();
                    worksheet.Cells["E" + rowIndex.ToString()].Value = audit.ActualStartDate.ToShortDateString() + " - " + audit.ActualEndDate.ToShortDateString();
                    if (!string.IsNullOrEmpty(audit.ActivityStatus))
                    {
                        audit.ActivityStatus = audit.ActivityStatus.ToLower();
                        if (audit.ActivityStatus == "inprogress")
                        {
                            audit.ActivityStatus = "In Progress";
                        }
                        else if (audit.ActivityStatus == "completed")
                        {
                            audit.ActivityStatus = "Completed";
                        }
                        worksheet.Cells["F" + rowIndex.ToString()].Value = audit.ActivityStatus;
                    }
                    rowIndex++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}