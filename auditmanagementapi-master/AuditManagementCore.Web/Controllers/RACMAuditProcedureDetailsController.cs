using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RACMAuditProcedureDetailsController : VJBaseGenericAPIController<RACMAuditProcedureDetails>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;

        IWebHostEnvironment _IWebHostEnvironment;
        CommonServices _CommonServices;
        IEmailUtility _IEmailUtility;
        #endregion

        public RACMAuditProcedureDetailsController
            (IMongoGenericRepository<RACMAuditProcedureDetails> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment,
            IEmailUtility emailUtility, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;

            _IWebHostEnvironment = webHostEnvironment;
            _CommonServices = cs;
            _IEmailUtility = emailUtility;
        }

        public override ActionResult Post([FromBody] RACMAuditProcedureDetails e)
        {
            var isExist = _api.Exists(x => x.Id == e.Id);
            if (isExist)
                return ResponseError("RACM Audit Procedure with ID : " + e.Id + " already exists.");

            e.ProcedureStartDate = e.ProcedureStartDate?.ToLocalTime();
            e.ProcedureEndDate = e.ProcedureEndDate?.ToLocalTime();
            base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Procedure.ProcedureId, "RACMAuditProcedureDetails", "Manage Audits | Audit Execution | Testing of Control | Add", "Added RACMAuditProcedureDetails");
            return ResponseOK(e);
        }
        public override ActionResult Put([FromBody] RACMAuditProcedureDetails e)
        {
            #region Send email
            if (e.Status != null && e.Status.ToLower() == "inreview")
            {
                var tocModel = _api.GetByID(e.Id);

                if (tocModel == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetByID(tocModel.ReviewerId);
                //userModel.EmailId = "mayursasp.net@gmail.com";

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(tocModel.AuditId);

                var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "TestingOfControl.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                emailBody = emailBody
                    //.Replace("#ReviewerName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#AuditName#", auditModel.AuditName);
                //.Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));

                var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                var auditPeriod = auditStartDate + " to " + auditEndDate;

                var emailModel = new EmailModel()
                {
                    ToEmail = new List<string>() { userModel.EmailId },
                    Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Audit queries and working",
                    MailBody = emailBody.ToString()
                };

                _IEmailUtility.SendEmail(emailModel);
            }
            #endregion

            e.ProcedureStartDate = e.ProcedureStartDate?.ToLocalTime();
            e.ProcedureEndDate = e.ProcedureEndDate?.ToLocalTime();
            var racmaduitproceduredetails = base.Put(e);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Procedure.ProcedureId, "RACMAuditProcedureDetails", "Manage Audits | Audit Execution | Testing of Control | Edit", "Updated RACMAuditProcedureDetails");
            return racmaduitproceduredetails;
        }

        [HttpPost("Multiple")]
        public ActionResult AddMultiple([FromBody] List<RACMAuditProcedureDetails> e)
        {
            foreach (var item in e)
            {
                var isExist = _api.Exists(x => x.Id == item.Id);

                if (isExist)
                    return ResponseError("RACM Audit Procedure Details with ID : " + item.Id + " already exists.");

                item.Status = "NotStarted";

                item.ProcedureStartDate = item.ProcedureStartDate?.ToLocalTime();
                item.ProcedureStartDate = item.ProcedureEndDate?.ToLocalTime();

                base.Post(item);
                //Activity Log
                //Update Knowledge Library
                InsertRACM(item);
                _CommonServices.ActivityLog(item.CreatedBy, item.Id, item.Procedure.ProcedureId, "RACMAuditProcedureDetails", "Manage Audits | Audit Execution | Testing of Control | Add", "Added RACMAuditProcedureDetails");
            }
            return ResponseOK(e);
        }
        public void InsertRACM(RACMAuditProcedureDetails e)
        {
            try
            {
                var repoRACMAuditProcedure = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
                var repoRACMProcedure = new MongoGenericRepository<RACMProcedure>(_dbsetting);
                var repoRACM = new MongoGenericRepository<RACM>(_dbsetting);
                if (e.Procedure != null)
                {
                    var isExists = true;
                    if (e.Procedure.ProcedureId != null)
                    {
                        var exists = repoRACMProcedure.GetFirst(x => (x.Procedure != null && x.Procedure.ProcedureId == e.Procedure.ProcedureId));
                        if (exists == null)
                        {
                            isExists = false;
                            exists = new RACMProcedure();
                        }
                        Procedure objProcedure = new Procedure();
                        var RACMAuditProcedureId = repoRACMAuditProcedure.GetFirst(p => p.Id == e.RACMAuditProcedureId);
                        if (RACMAuditProcedureId != null)
                        {
                            var objRACM = repoRACM.GetFirst(p => p.RACMnumber == RACMAuditProcedureId.RACMnumber);
                            exists.RACMId = objRACM.Id;
                        }
                        objProcedure.ProcedureId = e.Procedure.ProcedureId;
                        objProcedure.ProcedureTitle = e.Procedure.ProcedureTitle;
                        objProcedure.ProcedureDesc = e.Procedure.ProcedureDesc;

                        exists.Procedure = objProcedure;
                        exists.Status = "NotStarted";
                        exists.ProcedureStartDate = e.ProcedureStartDate?.ToLocalTime();
                        exists.ProcedureEndDate = e.ProcedureEndDate?.ToLocalTime();
                        exists.ReviewerId = e.ReviewerId;
                        exists.ResponsibilityId = e.ResponsibilityId;
                        if (isExists)
                            repoRACMProcedure.Update(exists);
                        else
                            repoRACMProcedure.Insert(exists);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        [HttpPut("Multiple")]
        public ActionResult UpdateMultiple([FromBody] List<RACMAuditProcedureDetails> e)
        {
            foreach (var item in e)
            {
                var isExist = _api.Exists(x => x.Id == item.Id);

                if (!isExist)
                    return ResponseError("RACM Audit Procedure Details with ID : " + item.Id + " does not exist.");

                item.ProcedureStartDate = item.ProcedureStartDate?.ToLocalTime();
                item.ProcedureEndDate = item.ProcedureEndDate?.ToLocalTime();
                base.Put(item);
                //Update Knowledge Library
                InsertRACM(item);
                //Activity Log
                _CommonServices.ActivityLog(item.UpdatedBy, item.Id, item.Procedure.ProcedureId, "RACMAuditProcedureDetails", "Manage Audits | Audit Execution | Testing of Control | Edit", "Updated RACMAuditProcedureDetails");
            }
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, "", "RACMAuditProcedureDetails", "Manage Audits | Audit Execution | Testing of Control | Delete", "Deleted RACMAuditProcedureDetails");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        //GetByAuditId
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<RACMAuditProcedure>();
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
            }
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        [HttpGet("getsummary/{id}")]
        public ActionResult GetSummary(string id)
        {
            var racmSummary = new RACMSummary();

            var tList = _api.GetWithInclude<RACMAuditProcedure>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            racmSummary.ProcNotStarted = tList.Where(a => a.Status == null || a.Status.ToLower().Trim() == "notstarted").Count();
            racmSummary.ProcInProgress = tList.Where(a => a.Status != null && a.Status.ToLower().Trim() == "inprogress").Count();
            racmSummary.ProcInReview = tList.Where(a => a.Status != null && a.Status.ToLower().Trim() == "inreview").Count();
            racmSummary.ProcCompleted = tList.Where(a => a.Status != null && a.Status.ToLower().Trim() == "completed").Count();
            racmSummary.ProcEffective = tList.Where(a => a.Conclusion != null && a.Conclusion.ToLower().Trim() == "effective").Count();
            racmSummary.ProcIneffective = tList.Where(a => a.Conclusion != null && a.Conclusion.ToLower().Trim() == "ineffective").Count();
            racmSummary.ProcNotSelect = tList.Where(a => a.Conclusion == "" || a.Conclusion == null).Count();

            return ResponseOK(racmSummary);
        }

        [HttpGet("GetByAudit/{id}/{status}/{testingresult}")]
        public ActionResult GetByAudit(string id, string status, string testingresult)
        {
            var tList = _api.GetWithInclude<RACMAuditProcedure>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            if (status.Trim() != "" && status.ToLower().Trim() != "all")
            {
                if (status.Trim() == "notstarted")
                {
                    tList = tList.Where(a => a.Status == null || a.Status.ToLower().Trim().ToString() == status.ToLower().Trim());
                }
                else
                {
                    tList = tList.Where(a => a.Status != null && a.Status.ToLower().Trim().ToString() == status.ToLower().Trim());
                }
            }
            if (tList == null)
                return ResponseNotFound();

            if (testingresult.ToLower().Trim() == "notselected" && testingresult.ToLower().Trim() != "all")
                tList = tList.Where(a => a.Conclusion == "" || a.Conclusion == null);
            else if (testingresult.Trim() != "" && testingresult.ToLower().Trim() != "all")
                tList = tList.Where(a => a.Conclusion != null && a.Conclusion.ToLower().Trim().ToString() == testingresult.ToLower().Trim());

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
            var processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
            var processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);
            var tocUploadRepo = new MongoGenericRepository<TestingOfControlUpload>(_dbsetting);

            foreach (var item in tList)
            {
                item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
                item.Reviewer = userRepo.GetByID(item.ReviewerId);

                if (item.RACMAuditProcedure != null)
                {
                    item.RACMAuditProcedure.Control.User = userRepo.GetByID(item.RACMAuditProcedure.Control.UserId) == null ? new User() : userRepo.GetByID(item.RACMAuditProcedure.Control.UserId);
                    item.RACMAuditProcedure.BusinessCycle = businessCycleRepo.GetByID(item.RACMAuditProcedure.BusinessCycleId) == null ? new BusinessCycle() : businessCycleRepo.GetByID(item.RACMAuditProcedure.BusinessCycleId);
                    item.RACMAuditProcedure.ProcessL1 = processL1Repo.GetByID(item.RACMAuditProcedure.ProcessL1Id) == null ? new ProcessL1() : processL1Repo.GetByID(item.RACMAuditProcedure.ProcessL1Id);
                    item.RACMAuditProcedure.ProcessL2 = processL2Repo.GetByID(item.RACMAuditProcedure.ProcessL2Id) == null ? new ProcessL2() : processL2Repo.GetByID(item.RACMAuditProcedure.ProcessL2Id);
                }
                else
                {
                    item.RACMAuditProcedure = new RACMAuditProcedure();
                    item.RACMAuditProcedure.Control = new Control();
                    item.RACMAuditProcedure.Control.User = new User();
                    item.RACMAuditProcedure.BusinessCycle = new BusinessCycle();
                    item.RACMAuditProcedure.ProcessL1 = new ProcessL1();
                    item.RACMAuditProcedure.ProcessL2 = new ProcessL2();
                }


                item.TestingOfControlUploads = tocUploadRepo.GetMany(x => x.TestingOfCountrolId == item.Id).ToList();
            }

            return ResponseOK(tList);
        }

        [HttpGet("GetByID/{id}")]
        public ActionResult GetRacmDetailByID(string id)
        {
            var tList = _api.GetFirst(x => x.Id == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var tocUploadRepo = new MongoGenericRepository<TestingOfControlUpload>(_dbsetting);
            tList.TestingOfControlUploads = tocUploadRepo.GetMany(x => x.TestingOfCountrolId == tList.Id).ToList();
            return ResponseOK(tList);
        }

        [HttpPut("UpdateStatus")]
        public ActionResult UpdateStatus(StatusUpdate statusUpdate)
        {
            if (statusUpdate.Status == null)
            {
                return ResponseError("Status cannot be null.");
            }

            RACMAuditProcedureDetails testingOfControl = _api.GetFirst(x => x.Id == statusUpdate.Id);

            testingOfControl.Status = statusUpdate.Status;
            testingOfControl.Justification = statusUpdate.Justification;
            testingOfControl.UpdatedBy = statusUpdate.UpdatedBy;

            _CommonServices.SaveHistoryforTestingofControl(testingOfControl.UpdatedBy, testingOfControl.Status, testingOfControl.Id);
            var testingofContorol = base.Put(testingOfControl);
            _CommonServices.ActivityLog(testingOfControl.UpdatedBy, testingOfControl.Id, testingOfControl.Procedure.ProcedureId, "RACMAuditProcedureDetails", "Manage Audits | Audit Execution | Testing of Control | Edit", "Updated Status");
            return testingofContorol;
        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail([FromBody] EmailModel emailModel)
        {
            try
            {
                var tocModel = _api.GetWithInclude<RACMAuditProcedure, User>(p => p.Id == emailModel.Id).FirstOrDefault();

                if (tocModel == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetByID(tocModel.ReviewerId);
                //userModel.EmailId = "mayursasp.net@gmail.com";

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(tocModel.AuditId);

                var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "TestingOfControl.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                emailBody = emailBody
                    //.Replace("#ReviewerName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#AuditName#", auditModel.AuditName);
                //.Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));

                var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                var auditPeriod = auditStartDate + " to " + auditEndDate;
                //emailModel.ToEmail = new List<string>() { "baldev@silverwebbuzz.com" };

                emailModel.ToEmail = new List<string>() { userModel.EmailId };
                emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Audit queries and working";
                emailModel.MailBody = emailBody.ToString();
                var file = _CommonServices.DownloadExcelAttachmentForAuditProcedure(tocModel);
                var objAttachment = new AttachmentByte()
                {
                    FileContents = file.FileContents,
                    FileName = file.FileDownloadName
                };
                emailModel.Attachments = new List<AttachmentByte>() { objAttachment };
                _IEmailUtility.SendEmail(emailModel);

                return ResponseOK(new { sent = true });
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpPost("SendEmailToControlOwner")]
        public IActionResult SendEmailToControlOwner([FromBody] EmailModel emailModel)
        {
            try
            {
                var tocModel = _api.GetWithInclude<RACMAuditProcedure>(a => a.Id == emailModel.Id).FirstOrDefault();

                if (tocModel == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetByID(tocModel.RACMAuditProcedure.Control.UserId);
                //userModel.EmailId = "mayursasp.net@gmail.com";

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(tocModel.AuditId);

                var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "TestingOfControlToControlOwner.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                emailBody = emailBody
                    .Replace("#ControlOwnerName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#AuditName#", auditModel.AuditName)
                    .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));

                var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                var auditPeriod = auditStartDate + " to " + auditEndDate;
                //emailModel.ToEmail = new List<string>() { "baldev@silverwebbuzz.com" };

                emailModel.ToEmail = new List<string>() { userModel.EmailId };
                emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Testing of Control";
                emailModel.MailBody = emailBody.ToString();
                var file = _CommonServices.DownloadExcelAttachmentForAuditProcedure(tocModel);
                var objAttachment = new AttachmentByte()
                {
                    FileContents = file.FileContents,
                    FileName = file.FileDownloadName
                };

                emailModel.Attachments = new List<AttachmentByte>() { objAttachment };

                _IEmailUtility.SendEmail(emailModel);

                return ResponseOK(new { sent = true });
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpGet("downloadexcel/{auditId}")]
        public IActionResult DownloadExcel(string auditId)
        {
            try
            {
                var tList = _api.GetWithInclude<Audit, BusinessCycle, ProcessL1, ProcessL2, RACMAuditProcedure>(x => x.AuditId == auditId);
                if (tList == null)
                    return ResponseNotFound();
                var repoUser = new MongoGenericRepository<User>(_dbsetting);
                var fileName = "TestingOfControl.xlsx";
                var memoryStream = new MemoryStream();
                var racmAuditProcedureRepo = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);

                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet wSheetRACMs = package.Workbook.Worksheets.Add("TestingOfControl");
                    Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    wSheetRACMs.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["A1"].Value = "RACM Number*";
                    wSheetRACMs.Cells["B1"].Value = "Business Cycle";
                    wSheetRACMs.Cells["C1"].Value = "Process L1";
                    wSheetRACMs.Cells["D1"].Value = "Process L2";
                    wSheetRACMs.Cells["E1"].Value = "Risk ID";
                    wSheetRACMs.Cells["F1"].Value = "Risk Rating";
                    wSheetRACMs.Cells["G1"].Value = "Risk Description";
                    wSheetRACMs.Cells["H1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["H1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["H1"].Value = "Control ID*";
                    wSheetRACMs.Cells["I1"].Value = "Control Type";
                    wSheetRACMs.Cells["J1"].Value = "Control Nature";
                    wSheetRACMs.Cells["K1"].Value = "Control Frequency";
                    wSheetRACMs.Cells["L1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["L1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["L1"].Value = "Control Owner*";
                    wSheetRACMs.Cells["M1"].Value = "Control Description";
                    wSheetRACMs.Cells["N1"].Value = "Procedure ID";
                    wSheetRACMs.Cells["O1"].Value = "Procedure Title";
                    wSheetRACMs.Cells["P1"].Value = "Procedure Description";
                    wSheetRACMs.Cells["Q1:W1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["Q1:W1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["Q1:W1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["Q1"].Value = "Start Date*";
                    wSheetRACMs.Cells["R1"].Value = "End Date*";
                    wSheetRACMs.Cells["S1"].Value = "Responsibility*";
                    wSheetRACMs.Cells["T1"].Value = "Reviewer*";
                    wSheetRACMs.Cells["U1"].Value = "Testing Result*";
                    wSheetRACMs.Cells["V1"].Value = "Analytics*";
                    wSheetRACMs.Cells["W1"].Value = "Analytics Test Number*";
                    wSheetRACMs.Cells["X1"].Value = "Finding";
                    var rowIndex = 2;
                    foreach (var item in tList)
                    {
                        var objRACMAuditProcedure = racmAuditProcedureRepo.GetFirst(x => x.Id == item.RACMAuditProcedureId);
                        if (objRACMAuditProcedure != null)
                        {
                            wSheetRACMs.Cells["A" + rowIndex.ToString()].Value = objRACMAuditProcedure.RACMnumber;
                            wSheetRACMs.Cells["B" + rowIndex.ToString()].Value = objRACMAuditProcedure.BusinessCycle.Name;
                            wSheetRACMs.Cells["C" + rowIndex.ToString()].Value = objRACMAuditProcedure.ProcessL1.Name;
                            wSheetRACMs.Cells["D" + rowIndex.ToString()].Value = objRACMAuditProcedure.ProcessL2.Name;
                            wSheetRACMs.Cells["E" + rowIndex.ToString()].Value = objRACMAuditProcedure.Risk.RiskId;
                            wSheetRACMs.Cells["F" + rowIndex.ToString()].Value = objRACMAuditProcedure.Risk.Rating;
                            wSheetRACMs.Cells["G" + rowIndex.ToString()].Value = objRACMAuditProcedure.Risk.Description;
                            wSheetRACMs.Cells["H" + rowIndex.ToString()].Value = objRACMAuditProcedure.Control.ControlId;
                            wSheetRACMs.Cells["I" + rowIndex.ToString()].Value = objRACMAuditProcedure.Control.Type;
                            wSheetRACMs.Cells["J" + rowIndex.ToString()].Value = objRACMAuditProcedure.Control.Nature;
                            wSheetRACMs.Cells["K" + rowIndex.ToString()].Value = objRACMAuditProcedure.Control.Frequency;

                            if (objRACMAuditProcedure.Control.UserId != null)
                            {
                                var owner = repoUser.GetByID(objRACMAuditProcedure.Control.UserId);

                                if (owner != null)
                                    wSheetRACMs.Cells["L" + rowIndex.ToString()].Value = owner.FirstName + " " + owner.LastName;
                                else
                                    wSheetRACMs.Cells["L" + rowIndex.ToString()].Value = "";
                            }
                            else
                                wSheetRACMs.Cells["M" + rowIndex.ToString()].Value = "";

                            wSheetRACMs.Cells["M" + rowIndex.ToString()].Value = objRACMAuditProcedure.Control.Description;

                            if (item.Procedure != null)
                            {
                                wSheetRACMs.Cells["N" + rowIndex.ToString()].Value = item.Procedure.ProcedureId;
                                wSheetRACMs.Cells["O" + rowIndex.ToString()].Value = item.Procedure.ProcedureTitle;
                                wSheetRACMs.Cells["P" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(item.Procedure.ProcedureDesc);

                                wSheetRACMs.Cells["Q" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                                wSheetRACMs.Cells["Q" + rowIndex.ToString()].Value = Convert.ToDateTime(item.ProcedureStartDate);

                                wSheetRACMs.Cells["R" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                                wSheetRACMs.Cells["R" + rowIndex.ToString()].Value = Convert.ToDateTime(item.ProcedureEndDate);

                                if (item.ResponsibilityId != null)
                                {
                                    var user = repoUser.GetByID(item.ResponsibilityId);

                                    if (user != null)
                                        wSheetRACMs.Cells["S" + rowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                    else
                                        wSheetRACMs.Cells["S" + rowIndex.ToString()].Value = "";
                                }

                                if (item.ReviewerId != null)
                                {
                                    var user = repoUser.GetByID(item.ReviewerId);

                                    if (user != null)
                                        wSheetRACMs.Cells["T" + rowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                    else
                                        wSheetRACMs.Cells["T" + rowIndex.ToString()].Value = "";
                                }
                                wSheetRACMs.Cells["U" + rowIndex.ToString()].Value = item.Conclusion;
                                wSheetRACMs.Cells["V" + rowIndex.ToString()].Value = item.Analytics;
                                wSheetRACMs.Cells["W" + rowIndex.ToString()].Value = item.TestNumber;
                                wSheetRACMs.Cells["X" + rowIndex.ToString()].Value = item.Finding;
                                rowIndex++;
                            }
                        }
                    }
                    double minimumSize = 20;
                    double maximumSize = 50;
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns(minimumSize);
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].Style.WrapText = true;
                    wSheetRACMs.Cells["A1:XFD1"].Style.Font.Bold = true;
                    wSheetRACMs.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top; 
                    package.Save();
                }
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpGet("sampledownloadexcel/{auditId}")]
        public IActionResult SampleDownloadExcel(string auditId)
        {
            try
            {
                var fileName = "TestingOfControl.xlsx";
                var memoryStream = new MemoryStream();
                var racmAuditProcedureRepo = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet wSheetRACMs = package.Workbook.Worksheets.Add("TestingOfControl");
                    Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    wSheetRACMs.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["A1"].Value = "RACM Number*";
                    wSheetRACMs.Cells["B1"].Value = "Business Cycle";
                    wSheetRACMs.Cells["C1"].Value = "Process L1";
                    wSheetRACMs.Cells["D1"].Value = "Process L2";
                    wSheetRACMs.Cells["E1"].Value = "Risk ID";
                    wSheetRACMs.Cells["F1"].Value = "Risk Rating";
                    wSheetRACMs.Cells["G1"].Value = "Risk Description";
                    wSheetRACMs.Cells["H1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["H1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["H1"].Value = "Control ID*";
                    wSheetRACMs.Cells["I1"].Value = "Control Type";
                    wSheetRACMs.Cells["J1"].Value = "Control Nature";
                    wSheetRACMs.Cells["K1"].Value = "Control Frequency";
                    wSheetRACMs.Cells["L1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["L1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["L1"].Value = "Control Owner*";
                    wSheetRACMs.Cells["M1"].Value = "Control Description";
                    wSheetRACMs.Cells["N1"].Value = "Procedure ID";
                    wSheetRACMs.Cells["O1"].Value = "Procedure Title";
                    wSheetRACMs.Cells["P1"].Value = "Procedure Description";
                    wSheetRACMs.Cells["Q1:W1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["Q1:W1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["Q1:W1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["Q1"].Value = "Start Date*";
                    wSheetRACMs.Cells["R1"].Value = "End Date*";
                    wSheetRACMs.Cells["S1"].Value = "Responsibility*";
                    wSheetRACMs.Cells["T1"].Value = "Reviewer*";
                    wSheetRACMs.Cells["U1"].Value = "Testing Result*";
                    wSheetRACMs.Cells["V1"].Value = "Analytics*";
                    wSheetRACMs.Cells["W1"].Value = "Analytics Test Number*";
                    wSheetRACMs.Cells["X1"].Value = "Finding";
                    
                    #region Added Dropdown in particular column
                    ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("Inputs");
                    worksheet2.Cells["A1"].Value = "RACM Number";
                    worksheet2.Cells["B1"].Value = "Business Cycle";
                    worksheet2.Cells["C1"].Value = "Process L1";
                    worksheet2.Cells["D1"].Value = "Process L2";
                    worksheet2.Cells["E1"].Value = "Risk Rating";
                    worksheet2.Cells["F1"].Value = "Control Type";
                    worksheet2.Cells["G1"].Value = "Control Nature";
                    worksheet2.Cells["H1"].Value = "Control Frequency";
                    worksheet2.Cells["I1"].Value = "Control Owner";
                    worksheet2.Cells["J1"].Value = "Testing Result";
                    worksheet2.Cells["K1"].Value = "Analytics";

                    //worksheet2.Cells["D1"].Value = "Location";
                    int ObservationGradingIndex = 2, ControlTypeIndex = 2, ControlNatureIndex = 2, ControlFrequencyIndex = 2, ControlOwnerIndex = 2,
                        BusinessCycleIndex = 2, ProcessL1Index = 2, ProcessL2Index = 2, RACMIndex = 2,TestingResultIndex = 2, AnalyticIndex = 2;
                    #region RACM Number
                    var lstRACM = racmAuditProcedureRepo.GetMany(x => x.AuditId == auditId);
                    if (lstRACM.Count() > 0)
                    {
                        foreach (var item in lstRACM)
                        {
                            worksheet2.Cells["A" + RACMIndex.ToString()].Value = item.RACMnumber;
                            RACMIndex++;
                        }
                        var RACMNumber = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 1, ExcelPackage.MaxRows, 1));
                        RACMNumber.AllowBlank = false;
                        RACMNumber.Formula.ExcelFormula = string.Format("'{0}'!$A$2:$A${1}", worksheet2, RACMIndex);
                    }
                    #endregion
                    #region Business Cycle
                    var _repoBusinessCycle = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                    var lstBusinessCycle = _repoBusinessCycle.GetWithInclude<BusinessCycle>(x => x.Name != null).OrderBy(p => p.Name);
                    if (lstBusinessCycle.Count() > 0)
                    {
                        foreach (var item in lstBusinessCycle)
                        {
                            worksheet2.Cells["B" + BusinessCycleIndex.ToString()].Value = item.Name;
                            BusinessCycleIndex++;
                        }
                        var businessCycle = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 2, ExcelPackage.MaxRows, 2));
                        businessCycle.AllowBlank = false;
                        businessCycle.Formula.ExcelFormula = string.Format("'{0}'!$B$2:$B${1}", worksheet2, BusinessCycleIndex);
                    }
                    #endregion

                    #region Process L1
                    var _repoProcessL1 = new MongoGenericRepository<ProcessL1>(_dbsetting);
                    var lstProcessL1 = _repoProcessL1.GetWithInclude<ProcessL1>(x => x.Name != null).OrderBy(p => p.Name);
                    if (lstProcessL1.Count() > 0)
                    {
                        foreach (var item in lstProcessL1)
                        {
                            worksheet2.Cells["C" + ProcessL1Index.ToString()].Value = item.Name;
                            ProcessL1Index++;
                        }
                        var processL1 = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 3, ExcelPackage.MaxRows, 3));
                        processL1.AllowBlank = false;
                        processL1.Formula.ExcelFormula = string.Format("'{0}'!$C$2:$C${1}", worksheet2, ProcessL1Index);
                    }
                    #endregion
                    #region Process L2
                    var _repoProcessL2 = new MongoGenericRepository<ProcessL2>(_dbsetting);
                    var lstProcessL2 = _repoProcessL2.GetWithInclude<ProcessL2>(x => x.Name != null).OrderBy(p => p.Name);
                    if (lstProcessL2.Count() > 0)
                    {
                        foreach (var item in lstProcessL2)
                        {
                            worksheet2.Cells["D" + ProcessL2Index.ToString()].Value = item.Name;
                            ProcessL2Index++;
                        }
                        var processL2 = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 4, ExcelPackage.MaxRows, 4));
                        processL2.AllowBlank = false;
                        processL2.Formula.ExcelFormula = string.Format("'{0}'!$D$2:$D${1}", worksheet2, ProcessL2Index);
                    }
                    #endregion
                    #region Observation Grading
                    string[] lstObservationGrading = Enum.GetNames(typeof(ObservationGradingEnum));
                    if (lstObservationGrading.Count() > 0)
                    {
                        foreach (var item in lstObservationGrading)
                        {
                            worksheet2.Cells["E" + ObservationGradingIndex.ToString()].Value = item;
                            ObservationGradingIndex++;
                        }
                        var grading = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 6, ExcelPackage.MaxRows, 6));
                        grading.AllowBlank = false;
                        grading.Formula.ExcelFormula = string.Format("'{0}'!$E$2:$E${1}", worksheet2, ObservationGradingIndex);
                    }
                    #endregion
                    #region Control Type
                    string[] lstControlType = Enum.GetNames(typeof(ControlTypeEnum));
                    if (lstControlType.Count() > 0)
                    {
                        foreach (var item in lstControlType)
                        {
                            worksheet2.Cells["F" + ControlTypeIndex.ToString()].Value = item;
                            ControlTypeIndex++;
                        }
                        var controlType = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 9, ExcelPackage.MaxRows, 9));
                        controlType.AllowBlank = false;
                        controlType.Formula.ExcelFormula = string.Format("'{0}'!$F$2:$F${1}", worksheet2, ControlTypeIndex);
                    }
                    #endregion
                    #region Control Nature
                    string[] lstControlNature = Enum.GetNames(typeof(ControlNatureEnum));
                    if (lstControlNature.Count() > 0)
                    {
                        foreach (var item in lstControlNature)
                        {
                            worksheet2.Cells["G" + ControlNatureIndex.ToString()].Value = item;
                            ControlNatureIndex++;
                        }
                        var ControlNature = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 10, ExcelPackage.MaxRows, 10));
                        ControlNature.AllowBlank = false;
                        ControlNature.Formula.ExcelFormula = string.Format("'{0}'!$G$2:$G${1}", worksheet2, ControlNatureIndex);
                    }
                    #endregion
                    #region Control Frequency
                    string[] lstControlFrequency = Enum.GetNames(typeof(ControlFrequencyEnum));
                    if (lstControlFrequency.Count() > 0)
                    {
                        foreach (var item in lstControlFrequency)
                        {
                            worksheet2.Cells["H" + ControlFrequencyIndex.ToString()].Value = item;
                            ControlFrequencyIndex++;
                        }
                        var ControlFrequency = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 11, ExcelPackage.MaxRows, 11));
                        ControlFrequency.AllowBlank = false;
                        ControlFrequency.Formula.ExcelFormula = string.Format("'{0}'!$H$2:$H${1}", worksheet2, ControlFrequencyIndex);
                    }
                    #endregion
                    #region Control Owner 
                    var _repoUser = new MongoGenericRepository<User>(_dbsetting);
                    var lstControlOwner = _repoUser.GetWithInclude<Role, Company, User>(x => x.StakeHolder == true).OrderBy(p => p.FirstName);
                    if (lstControlOwner.Count() > 0)
                    {
                        foreach (var item in lstControlOwner)
                        {
                            worksheet2.Cells["I" + ControlOwnerIndex.ToString()].Value = item.FirstName + " " + item.LastName;
                            ControlOwnerIndex++;
                        }
                        var ControlOwner = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 12, ExcelPackage.MaxRows, 12));
                        ControlOwner.AllowBlank = false;
                        ControlOwner.Formula.ExcelFormula = string.Format("'{0}'!$I$2:$I${1}", worksheet2, ControlOwnerIndex);
                    }
                    #endregion
                    #region Testing Result 
                    string[] lstTestingResultEnum = Enum.GetNames(typeof(TestingResultEnum));
                    if (lstTestingResultEnum.Count() > 0)
                    {
                        foreach (var item in lstTestingResultEnum)
                        {
                            worksheet2.Cells["J" + TestingResultIndex.ToString()].Value = item;
                            TestingResultIndex++;
                        }
                        var testingResult = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 21, ExcelPackage.MaxRows, 21));
                        testingResult.AllowBlank = false;
                        testingResult.Formula.ExcelFormula = string.Format("'{0}'!$J$2:$J${1}", worksheet2, TestingResultIndex);
                    }
                    #endregion
                    #region Analytics
                    string[] lstAnalyticsEnume = Enum.GetNames(typeof(AnalyticsEnum));
                    if (lstAnalyticsEnume.Count() > 0)
                    {
                        foreach (var item in lstAnalyticsEnume)
                        {
                            worksheet2.Cells["K" + AnalyticIndex.ToString()].Value = item;
                            AnalyticIndex++;
                        }
                        var analytics = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 22, ExcelPackage.MaxRows, 22));
                        analytics.AllowBlank = false;
                        analytics.Formula.ExcelFormula = string.Format("'{0}'!$K$2:$K${1}", worksheet2, AnalyticIndex);
                    }
                    #endregion
                    worksheet2.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                    worksheet2.Cells["A1:XFD1"].Style.Font.Bold = true;
                    worksheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    #endregion
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                    wSheetRACMs.Cells["A1:XFD1"].Style.Font.Bold = true;
                    package.Save();
                }
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        [HttpPost("importexcel/{auditId}/{userid}")]
        public ActionResult ImportExcel(string auditId, string userid)
        {
            try
            {
                int ExceptionrowCount = 0;
                int TotalRow = 0;
                StringBuilder sb = new StringBuilder();
                var racmAuditProcedureRepo = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);

                if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                    return ResponseError("formfile is empty");

                var file = Request.Form.Files[0];

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return ResponseError("Not Support file extension");

                var repoBusinessCycle = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                var repoProcessL1 = new MongoGenericRepository<ProcessL1>(_dbsetting);
                var repoProcessL2 = new MongoGenericRepository<ProcessL2>(_dbsetting);
                var repoRACMProcedureDetails = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
                var repoUser = new MongoGenericRepository<User>(_dbsetting);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        int rowCount = 0;
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["TestingOfControl"];
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
                                var isExists = true;
                                var riskid = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : null;
                                var riskDesc = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : null;
                                var controlId = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : null;
                                var controlDesc = worksheet.Cells[row, 13].Value != null ? worksheet.Cells[row, 13].Value.ToString().Trim() : null;

                                //var exists = _api.GetFirst(a => a.RACMnumber.Trim() == racmNumber);
                                var exists = racmAuditProcedureRepo.GetFirst(x => x.AuditId == auditId
                                                           && x.Risk.RiskId == riskid && x.Risk.Description.ToLower() == riskDesc.ToLower()
                                                           && x.Control.ControlId == controlId && x.Control.Description.ToLower() == controlDesc.ToLower());
                                if (exists == null)
                                {
                                    isExists = false;
                                    exists = new RACMAuditProcedure();
                                }
                                var racmNumber = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : null;

                                if (racmNumber != null)
                                {
                                    exists.RACMnumber = racmNumber;
                                }

                                var businessCycleName = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().ToLower().Trim() : null;
                                if (businessCycleName != null)
                                {
                                    var businessCycle = repoBusinessCycle.GetFirst(a => a.Name.ToLower().Trim() == businessCycleName);
                                    exists.BusinessCycleId = businessCycle != null ? businessCycle.Id : null;
                                }

                                var processL1Name = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().ToLower().Trim() : null;
                                if (processL1Name != null)
                                {
                                    var processL1 = repoProcessL1.GetFirst(a => a.Name.ToLower().Trim() == processL1Name);
                                    exists.ProcessL1Id = processL1 != null ? processL1.Id : null;

                                }
                                var processL2Name = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().ToLower().Trim() : null;
                                if (processL2Name != null)
                                {
                                    var processL2 = repoProcessL2.GetFirst(a => a.Name.ToLower().Trim() == processL2Name);
                                    exists.ProcessL2Id = processL2 != null ? processL2.Id : null;
                                }

                                exists.AuditId = auditId;
                                exists.Risk.RiskId = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : null;
                                exists.Risk.Rating = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : "Low";
                                exists.Risk.Description = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : null;

                                exists.Control.ControlId = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : null;
                                exists.Control.Type = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : null;
                                exists.Control.Nature = worksheet.Cells[row, 10].Value != null ? worksheet.Cells[row, 10].Value.ToString().Trim() : null;
                                exists.Control.Frequency = worksheet.Cells[row, 11].Value != null ? worksheet.Cells[row, 11].Value.ToString().Trim() : null;

                                var ownerName = worksheet.Cells[row, 12].Value != null ? worksheet.Cells[row, 12].Value.ToString().ToLower().Trim() : null;
                                if (ownerName != null)
                                {
                                    var owner = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == ownerName);

                                    if (owner != null)
                                        exists.Control.UserId = owner.Id;
                                }

                                exists.Control.Description = worksheet.Cells[row, 13].Value != null ? worksheet.Cells[row, 13].Value.ToString().Trim() : null;

                                if (isExists)
                                {
                                    exists.UpdatedBy = userid;
                                    racmAuditProcedureRepo.Update(exists);
                                }
                                else
                                {
                                    exists.CreatedBy = userid;
                                    racmAuditProcedureRepo.Insert(exists);
                                }
                                #region procudure
                                // var racm = _api.GetFirst(a => a.RACMAuditProcedureId == exists.Id);
                                //if (racm != null)
                                //{
                                var procedureId = worksheet.Cells[row, 14].Value != null ? worksheet.Cells[row, 14].Value.ToString().ToLower().Trim() : "";
                                var isExistsProducte = true;
                                var existsProcedure = repoRACMProcedureDetails
                                    .GetFirst(a => a.RACMAuditProcedureId == exists.Id && a.Procedure.ProcedureId.Trim().ToLower() == procedureId);

                                if (existsProcedure == null)
                                {
                                    isExistsProducte = false;
                                    existsProcedure = new RACMAuditProcedureDetails();
                                    existsProcedure.Procedure = new Procedure();
                                }
                                existsProcedure.AuditId = auditId;
                                existsProcedure.RACMAuditProcedureId = exists.Id;
                                existsProcedure.Procedure.ProcedureId = worksheet.Cells[row, 14].Value != null ? worksheet.Cells[row, 14].Value.ToString().Trim() : null;
                                existsProcedure.Procedure.ProcedureTitle = worksheet.Cells[row, 15].Value != null ? worksheet.Cells[row, 15].Value.ToString().Trim() : null;
                                existsProcedure.Procedure.ProcedureDesc = worksheet.Cells[row, 16].Value != null ? worksheet.Cells[row, 16].Value.ToString().Trim() : null;

                                var startDate = worksheet.Cells[row, 17].Value != null ? worksheet.Cells[row, 17].Text.ToString().Trim() : null;
                                if (startDate != null)
                                    existsProcedure.ProcedureStartDate = Convert.ToDateTime(startDate).ToLocalTime();

                                var endDate = worksheet.Cells[row, 18].Value != null ? worksheet.Cells[row, 18].Text.ToString().Trim() : null;
                                if (endDate != null)
                                    existsProcedure.ProcedureEndDate = Convert.ToDateTime(endDate).ToLocalTime();

                                var responsibility = worksheet.Cells[row, 19].Value != null ? worksheet.Cells[row, 19].Value.ToString().ToLower().Trim() : null;
                                if (responsibility != null)
                                {
                                    var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsibility);
                                    if (user != null)
                                        existsProcedure.ResponsibilityId = user.Id;
                                }

                                var reviewer = worksheet.Cells[row, 20].Value != null ? worksheet.Cells[row, 20].Value.ToString().ToLower().Trim() : null;
                                if (reviewer != null)
                                {
                                    var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == reviewer);
                                    if (user != null)
                                        existsProcedure.ReviewerId = user.Id;
                                }

                                existsProcedure.Conclusion = worksheet.Cells[row, 21].Value != null ? worksheet.Cells[row, 21].Value.ToString().Trim() : null;
                                existsProcedure.Analytics = worksheet.Cells[row, 22].Value != null ? worksheet.Cells[row, 22].Value.ToString().Trim() : null;
                                existsProcedure.TestNumber = worksheet.Cells[row, 23].Value != null ? worksheet.Cells[row, 23].Value.ToString().Trim() : null;
                                existsProcedure.Finding = worksheet.Cells[row, 24].Value != null ? worksheet.Cells[row, 24].Value.ToString().Trim() : null;
                                if (isExistsProducte)
                                    repoRACMProcedureDetails.Update(existsProcedure);
                                else
                                    repoRACMProcedureDetails.Insert(existsProcedure);
                                //}

                                #endregion
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "RACMAuditProcedureDetails/ImportExcel()");
                            }
                        }
                    }
                }
                var RACMAuditProcdeduremaster = new
                {
                    ExcptionCount = ExceptionrowCount,
                    ExcptionRowNumber = sb.ToString(),
                    TotalRow = TotalRow - 1,
                };
                return ResponseOK(RACMAuditProcdeduremaster);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "RACMAuditProcedureDetails/ImportExcel()");

            }
            return ResponseOK(new object[0]);
        }
        public class StatusUpdate : BaseObjId
        {
            public string Status { get; set; }

            public string Justification { get; set; }
        }

    }
}