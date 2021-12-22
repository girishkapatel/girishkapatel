using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TORController : VJBaseGenericAPIController<TOR>
    {
        #region Class Porperties Declarations 
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        private readonly IWebHostEnvironment _IWebHostEnvironment;

        private readonly IDocumentUpload _IDocumentUpload;
        private readonly IEmailUtility _IEmailUtility;
        private readonly IGlobalConfiguration _globalConfig;

        #endregion

        public TORController(IMongoGenericRepository<TOR> api, IMongoDbSettings dbsetting, IWebHostEnvironment webHostEnvironment, IDocumentUpload documentUpload,
            IEmailUtility emailUtility, IGlobalConfiguration config, CommonServices cs) : base(api)
        {
            _dbsetting = dbsetting;
            _IWebHostEnvironment = webHostEnvironment;
            _globalConfig = config;
            _IDocumentUpload = documentUpload;
            _IEmailUtility = emailUtility;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] TOR e)
        {
            var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());

            if (isExist)
                return ResponseError("TOR already exists for Audit");

            e.AuditPeriodFromDate = e.AuditPeriodFromDate?.ToLocalTime();
            e.AuditPeriodToDate = e.AuditPeriodToDate?.ToLocalTime();
            e.TORIssuedDate = e.TORIssuedDate?.ToLocalTime();

            var country = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "TOR", "Manage Audits | Planning | TOR | Add", "Added TOR");
            return country;
        }

        public override ActionResult Put([FromBody] TOR e)
        {
            var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());

            if (!isExist)
                return ResponseError("TOR does not exists for Audit");

            e.AuditSpecificInformations = null;
            e.AuditPeriodFromDate = e.AuditPeriodFromDate?.ToLocalTime();
            e.AuditPeriodToDate = e.AuditPeriodToDate?.ToLocalTime();
            e.TORIssuedDate = e.TORIssuedDate?.ToLocalTime();

            var tor = base.Put(e);

            //Activity Log
            var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
            var audit = _repoAudit.GetFirst(x => x.Id == e.AuditId);
            var auditName = audit.AuditName != null ? audit.AuditName : "";

            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "TOR(" + auditName + ")", "TOR", "Manage Audits | Planning | TOR | Edit", "Updated TOR");
            return tor;
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAll();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            foreach (var item in tList)
            {
                item.Activities = activtiyRepo.GetMany(x => x.AuditID == item.AuditId).ToHashSet();
                item.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == item.AuditId);
            }
            return ResponseOK(tList);
        }

        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.Id == id).FirstOrDefault();

            if (tList == null)
            {
                return ResponseNotFound();
            }
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            tList.Activities = activtiyRepo.GetMany(x => x.AuditID == tList.AuditId).ToHashSet();
            return ResponseOK(tList);
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetFirst(x => x.AuditId == id);

            if (tList == null)
            {
                return ResponseNotFound();
            }
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var idrRepo = new MongoGenericRepository<InitialDataRequest>(_dbsetting);

            tList.Activities = activtiyRepo.GetMany(x => x.AuditID == tList.AuditId).ToHashSet();
            tList.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == tList.AuditId);
            tList.AuditSpecificInformations = idrRepo.GetMany(x => x.AuditId == tList.AuditId).ToList();
            if (tList.Audit != null)
            {
                tList.Audit = _CommonServices.populateScopeAndSchedule(tList.Audit);
            }
            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel/{id}")]
        public IActionResult DownloadExcel(string id)
        {
            var tList = _api.GetFirst(x => x.AuditId == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var idrRepo = new MongoGenericRepository<InitialDataRequest>(_dbsetting);
            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            tList.Activities = activtiyRepo.GetMany(x => x.AuditID == tList.AuditId).ToHashSet();
            tList.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == tList.AuditId);
            tList.AuditSpecificInformations = idrRepo.GetMany(x => x.AuditId == tList.AuditId).ToList();
            if (tList.Audit != null)
            {
                tList.Audit = _CommonServices.populateScopeAndSchedule(tList.Audit);
            }

            var fileName = "TOR.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                #region Sheet 1
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TOR");

                worksheet.Cells["A1"].Value = "Audit Name"; 
                worksheet.Cells["B1"].Value = tList.Audit.Audit.ProcessLocationMapping.AuditName;

                worksheet.Cells["A2"].Value = "Audit Unit";
                worksheet.Cells["B2"].Value = tList.Audit.Location.DivisionDescription;


                worksheet.Cells["A3"].Value = "Audit Approvers";
                worksheet.Cells["B3"].Value = _CommonServices.GetAuditApprovers(tList.Audit.AuditApprovalMapping);

                worksheet.Cells["A4"].Value = "Audit Team";
                worksheet.Cells["B4"].Value = _CommonServices.GetAuditTeam(tList.Audit.AuditResources);

                worksheet.Cells["A5"].Value = "Location";
                worksheet.Cells["B5"].Value = tList.Audit.Location.ProfitCenterCode;

                worksheet.Cells["A6"].Value = "Period Of Audit";
                worksheet.Cells["B6"].Value = Convert.ToDateTime(tList.AuditPeriodFromDate).ToString("dd-MM-yyyy") + " to " + Convert.ToDateTime(tList.AuditPeriodToDate).ToString("dd-MM-yyyy");

                worksheet.Cells["A7"].Value = "TOR Date Issued";
                worksheet.Cells["B7"].Style.Numberformat.Format = "dd-MM-yyyy";
                worksheet.Cells["B7"].Value = Convert.ToDateTime(tList.TORIssuedDate).ToString("dd-MM-yyyy");

                //worksheet.Cells["A8"].Value = "From Date";
                //worksheet.Cells["B8"].Style.Numberformat.Format = "dd-mm-yyyy";
                //worksheet.Cells["B8"].Value = tList.AuditPeriodFromDate;

                //worksheet.Cells["A9"].Value = "To Date";
                //worksheet.Cells["B9"].Style.Numberformat.Format = "dd-mm-yyyy";
                //worksheet.Cells["B9"].Value = tList.AuditPeriodToDate;
                var rowIndex = 8;

                foreach (var activity in tList.Activities)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = activity.ActivityName + " Start Date";
                    worksheet.Cells["B" + rowIndex.ToString()].Style.Numberformat.Format = "dd-MM-yyyy";
                    worksheet.Cells["B" + rowIndex.ToString()].Value = Convert.ToDateTime(activity.ActualStartDate).ToString("dd-MM-yyyy");
                    rowIndex++;

                    worksheet.Cells["A" + rowIndex.ToString()].Value = activity.ActivityName + " End Date";
                    worksheet.Cells["B" + rowIndex.ToString()].Style.Numberformat.Format = "dd-MM-yyyy";
                    worksheet.Cells["B" + rowIndex.ToString()].Value = Convert.ToDateTime(activity.ActualEndDate).ToString("dd-MM-yyyy");
                    rowIndex++;
                }
               
                worksheet.Cells["A" + rowIndex.ToString()].Value = "Audit Objectives";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.AuditObjective != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.AuditObjective) : null;
                worksheet.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Policies, Standards & Reference Document";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.Policies != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.Policies) : null;
                worksheet.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Deliverable";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.Deliverable != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.Deliverable) : null;
                worksheet.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Disclaimer";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.Disclaimer != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.Disclaimer) : null;
                worksheet.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Limitation";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.Limitation != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.Limitation) : null;
                worksheet.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                rowIndex++;
                //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                //Make all text fit the cells
              
                worksheet.Cells["A1:A" + rowIndex.ToString()].Style.Font.Bold = true;
                worksheet.Cells[worksheet.Dimension.Address].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;  
                worksheet.Cells[worksheet.Dimension.Address].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                #endregion

                #region Sheet 2 - Audit Scopes
                ExcelWorksheet worksheet1 = package.Workbook.Worksheets.Add("Audit Scope");

                worksheet1.Cells["A1"].Value = "Areas";
                worksheet1.Cells["B1"].Value = "Scope";

                rowIndex = 2;
                if (tList.AuditScopes != null)
                {
                    foreach (var scope in tList.AuditScopes)
                    {
                        worksheet1.Cells["A" + rowIndex.ToString()].Style.WrapText = true;
                        worksheet1.Cells["A" + rowIndex.ToString()].Value = scope.Areas != null ? VJLiabraries.UtilityMethods.HtmlToText(scope.Areas) : null;
                        worksheet1.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                        worksheet1.Cells["B" + rowIndex.ToString()].Value = scope.Scope != null ? VJLiabraries.UtilityMethods.HtmlToText(scope.Scope) : null;
                        rowIndex++;
                    }
                }
                //worksheet1.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet1.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet1.Cells[worksheet1.Dimension.Address].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet1.Cells[worksheet1.Dimension.Address].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                #endregion  

                #region Sheet 3 - Audit Specific Information
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("Audit Specific Information");
               
                worksheet2.Cells["A1"].Value = "Area";
                worksheet2.Cells["B1"].Value = "Data Requested";
                worksheet2.Cells["C1"].Value = "Process Owner";
                worksheet2.Cells["D1"].Value = "Data Request Date";
                worksheet2.Cells["E1"].Value = "Data Received Date";
                worksheet2.Cells["F1"].Value = "Pending Data";
                worksheet2.Cells["G1"].Value = "Status";

                rowIndex = 2;
                if (tList.AuditSpecificInformations != null)
                {
                    foreach (var info in tList.AuditSpecificInformations)
                    {
                        worksheet2.Cells["A" + rowIndex.ToString()].Value = info.Area;
                        worksheet2.Cells["B" + rowIndex.ToString()].Style.WrapText = true;
                        worksheet2.Cells["B" + rowIndex.ToString()].Value = info.DataRequested;
                        if (info.ProcessOwnerId != null && info.ProcessOwnerId != "")
                        {
                            var objUser = userRepo.GetFirst(x => x.Id == info.ProcessOwnerId);
                            worksheet2.Cells["C" + rowIndex.ToString()].Value = objUser != null ? objUser.FirstName + " " + objUser.LastName : "";
                        }
                        worksheet2.Cells["D" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet2.Cells["D" + rowIndex.ToString()].Value = info.DataRequestDate;
                        worksheet2.Cells["E" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet2.Cells["E" + rowIndex.ToString()].Value = info.DataReceivedDate;
                        worksheet2.Cells["F" + rowIndex.ToString()].Value = info.PendingData;
                        worksheet2.Cells["F" + rowIndex.ToString()].Style.WrapText = true;
                        worksheet2.Cells["G" + rowIndex.ToString()].Value = info.Status;
                        rowIndex++;
                    }
                }
                #region Root Cause 
                var lstRootCause = _CommonServices.getTorStatus();
                if (lstRootCause.Count() > 0)
                {
                    var rootCauseColumn = worksheet2.DataValidations.AddListValidation("G:G");
                    foreach (var item in lstRootCause)
                    {
                        rootCauseColumn.Formula.Values.Add(item); 
                    }
                }
                #endregion
                double minimumSize = 20;
                double maximumSize = 50;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(); 
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize); 
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);

                worksheet1.Cells[worksheet1.Dimension.Address].AutoFitColumns();
                worksheet1.Cells[worksheet1.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet1.Cells[worksheet1.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);


                worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns();
                worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);

                worksheet2.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet2.Cells[worksheet2.Dimension.Address].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet2.Cells[worksheet2.Dimension.Address].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                #endregion
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        public List<string> GetAuditApproverEmails(AuditApprovalMapping approvers)
        {
            var sList = new List<string>();

            foreach (var approver in approvers.UserData)
            {
                if (approver.User != null)
                    sList.Add(approver.User.EmailId);
            }

            return sList;
        }
        public List<string> GetAuditTeamEmails(List<AuditResource> resources)
        {
            var sList = new List<string>();

            foreach (var resource in resources)
            {
                if (resource.User != null)
                    sList.Add(resource.User.EmailId);
            }

            return sList;
        }
        public List<string> GetReporttoEmail(List<Auditees> audit)
        {
            var sList = new List<string>();

            foreach (var reportTo in audit)
            {
                if (reportTo.ReportToUser != null)
                    sList.Add(reportTo.ReportToUser.EmailId);
            }
            return sList;
        }

        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile(IFormFile[] files)
        {
            if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                return ResponseError("formfile is empty");

            var Id = Request.Form["Id"].ToString().Trim() == "" ? "0" : Request.Form["Id"].ToString().Trim();
            var AuditId = Request.Form["AuditId"].ToString().Trim();
            var module = Request.Form["module"].ToString().Trim();

            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var auditModel = auditRepo.GetByID(AuditId);

            if (auditModel == null)
                return ResponseNotFound();

            //var location = auditModel.Location.ProfitCenterCode.Replace("/", "-").Trim();
            //var year = DateTime.Now.Year.ToString();
            //var audit = auditModel.ProcessLocationMapping.AuditName.Replace("/", "-").Trim();

            try
            {
                List<AuditFiles> returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", AuditId, module.Trim());

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                foreach (var item in files)
                {
                    var res = await _IDocumentUpload.UploadToWebRoot(item, newPath);

                    AuditFiles auditFiles = new AuditFiles();
                    auditFiles.OriginalFileName = item.FileName;
                    auditFiles.UploadedDatetime = DateTime.Now;
                    auditFiles.UploadedFileName = res;
                    auditFiles.ModuleName = module;
                    auditFiles.AuditId = AuditId;
                    repo.Insert(auditFiles);

                    returnFiles.Add(auditFiles);
                }

                return ResponseOK(new { files = returnFiles, isUploaded = true });
            }
            catch (Exception ex)
            {
                return ResponseOK(new { isUploaded = false });
            }
        }

        [HttpDelete("removefile/{id}")]
        public ActionResult RemoveUploadedFile(string id)
        {
            try
            {
                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var deleteFile = repo.GetByID(id);

                if (!_IDocumentUpload.IsExists(deleteFile.UploadedFileName))
                    return ResponseOK("File does not exists.");

                _IDocumentUpload.DeleteFile(deleteFile.UploadedFileName);

                repo.Delete(deleteFile);

                return ResponseOK(new { isDeleted = true });
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpGet("getallfiles/{auditId}/{id?}")]
        public ActionResult GetAllFiles(string auditId, string id)
        {
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var auditModel = auditRepo.GetByID(auditId);

            if (auditModel == null)
                return ResponseNotFound();

            //var Id = id == null || id == "" ? "0" : id;
            //var location = auditModel.Location.ProfitCenterCode.Replace("/", "-").Trim();
            //var year = DateTime.Now.Year.ToString();
            //var audit = auditModel.ProcessLocationMapping.AuditName.Replace("/", "-").Trim();

            try
            {
                var returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", auditId, "tor");

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var auditFiles = repo.GetMany(a => a.AuditId == auditId && a.ModuleName == "tor");

                foreach (var file in auditFiles)
                {
                    if (System.IO.File.Exists(file.UploadedFileName))
                        returnFiles.Add(file);
                }

                return ResponseOK(returnFiles);
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpGet("downloadfile/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            try
            {
                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var downloadFile = repo.GetByID(id);

                if (!_IDocumentUpload.IsExists(downloadFile.UploadedFileName))
                    return ResponseOK("File does not exists.");

                var memory = new MemoryStream();
                using (var stream = new FileStream(downloadFile.UploadedFileName, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, UtilityMethods.GetContentType(downloadFile.UploadedFileName), downloadFile.OriginalFileName);
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail([FromBody] EmailModel emailModel)
        {
            try
            {
                var tList = _api.GetFirst(x => x.AuditId == emailModel.Id);
                if (tList == null)
                    return ResponseNotFound();

                var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                tList.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == tList.AuditId);

                if (tList.Audit != null)
                    tList.Audit = _CommonServices.populateScopeAndSchedule(tList.Audit);

                var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                var companyModel = companyRepo.GetByID(tList.Audit.Location.CompanyID);

                if (companyModel == null)
                    return ResponseNotFound();

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "TermsOfReference-TOR.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                var auditStartDate = Convert.ToDateTime(tList.Audit.AuditStartDate).ToString("dd-MMM-yyyy");
                var auditEndDate = Convert.ToDateTime(tList.Audit.AuditEndDate).ToString("dd-MMM-yyyy");
                var auditPeriod = auditStartDate + " to " + auditEndDate;

                var approverNames = _CommonServices.GetAuditApprovers(tList.Audit.AuditApprovalMapping);
                var resourcesNames = _CommonServices.GetAuditTeam(tList.Audit.AuditResources);

                emailBody = emailBody
                    .Replace("#Name#", "")
                    .Replace("#AuditName#", tList.Audit.Audit.AuditName)
                    .Replace("#ApproverName#", approverNames)
                    .Replace("#ResourcesName#", resourcesNames)
                    .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));
                emailModel.ToEmail = new List<string>() { };

                //emailModel.ToEmail = new List<string>() { "baldev@silverwebbuzz.com" };
                emailModel.ToEmail = GetAuditApproverEmails(tList.Audit.AuditApprovalMapping);
                emailModel.CcEmail = GetAuditTeamEmails(tList.Audit.AuditResources);

                //emailModel.BccEmail = GetAuditTeamEmails(tList.Audit.AuditResources);
                emailModel.BccEmail = GetReporttoEmail(tList.Audit.Auditees);
                emailModel.Subject = companyModel.Name + " | " + tList.Audit.Audit.AuditName + " | " + auditPeriod + " | Scope of audit";
                emailModel.MailBody = emailBody.ToString();
                var file = _CommonServices.DownloadExcelAttachmentForTOR(tList);
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

        [HttpGet("downloadpdf/{id}")]
        public IActionResult DownloadPDF(string id)
        {
            var tList = _api.GetFirst(x => x.AuditId == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var idrRepo = new MongoGenericRepository<InitialDataRequest>(_dbsetting);

            tList.Activities = activtiyRepo.GetMany(x => x.AuditID == tList.AuditId).ToHashSet();
            tList.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == tList.AuditId);
            tList.AuditSpecificInformations = idrRepo.GetMany(x => x.AuditId == tList.AuditId).ToList();
            if (tList.Audit != null)
            {
                tList.Audit = _CommonServices.populateScopeAndSchedule(tList.Audit);
            }
            var webRootPath = _IWebHostEnvironment.WebRootPath;

            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "TOR.html");
            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }

            var returnBuilder = new StringBuilder();
            returnBuilder.Append(emailBody);

            returnBuilder = returnBuilder
                .Replace("#AuditUnit#", tList.Audit.Location.DivisionDescription)
                .Replace("#AuditName#", tList.Audit.Audit.ProcessLocationMapping.AuditName)
                .Replace("#AuditApprover#", _CommonServices.GetAuditApprovers(tList.Audit.AuditApprovalMapping))
                .Replace("#AuditName#", _CommonServices.GetAuditTeam(tList.Audit.AuditResources))
                .Replace("#Location#", tList.Audit.Location.ProfitCenterCode)
                .Replace("#PeriodOfAudit#", Convert.ToDateTime(tList.AuditPeriodFromDate).ToString("dd/MM/yyyy") + " to " + Convert.ToDateTime(tList.AuditPeriodToDate).ToString("dd/MM/yyyy"))
                .Replace("#Auditee#", _CommonServices.GetAuditeeName(tList.Audit.Auditees))
                .Replace("#AuditObjective#", tList.AuditObjective)
                .Replace("#Policies#", tList.Policies)
                .Replace("#Deliverable#", tList.Deliverable)
                .Replace("#Disclaimer#", tList.Disclaimer)
                .Replace("#Limitation#", tList.Limitation);

            if (tList.TORIssuedDate != null)
                returnBuilder = returnBuilder.Replace("#TorDateissued#", Convert.ToDateTime(tList.TORIssuedDate).ToString("dd/MM/yyyy"));
            else
                returnBuilder = returnBuilder.Replace("#TorDateissued#", "");

            var auditModel = auditRepo.GetFirst(p => p.Id == id);
            if (auditModel != null)
            {
                var returnFiles = new List<AuditFiles>();
                string newPath = Path.Combine("manageaudits", id, "tor");
                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);
                var auditFiles = repo.GetMany(a => a.AuditId == id && a.ModuleName == "tor");
                if (auditFiles.Count() == 0)
                    returnBuilder = returnBuilder
                      .Replace("#UploadedFileName#", "");
                else
                {
                    foreach (var file in auditFiles)
                    {
                        if (System.IO.File.Exists(file.UploadedFileName))
                        {
                            returnFiles.Add(file);
                            returnBuilder = returnBuilder
                          .Replace("#UploadedFileName#", file.OriginalFileName);
                        }
                    }
                }
            }

            ////////////////Date

            var DateTable = Path.Combine(webRootPath, "ExportTemplates", "TOR_DateTable.html");
            var table = new StringBuilder();
            using (StreamReader streamReaderDate = new StreamReader(DateTable))
            {
                var htmlContentDate = streamReaderDate.ReadToEnd();
                table.Append(htmlContentDate);
            }
            var returnBuilderDate = new StringBuilder();
            foreach (var activity in tList.Activities)
            {
                returnBuilderDate.Append(table);

                returnBuilderDate = returnBuilderDate
                   .Replace("#AuditDateName1#", activity.ActivityName + " Start Date")
                   .Replace("#AuditStartDate#", activity.ActualStartDate.ToString("dd/MM/yyyy"))
                   .Replace("#AuditDateName2#", activity.ActivityName + " End Date")
                   .Replace("#AuditEndDate#", activity.ActualEndDate.ToString("dd/MM/yyyy"));
            }
            returnBuilder = returnBuilder.Replace("#DateTable#", returnBuilderDate.ToString());

            ////////////////Scope

            var returnBuilderScope = new StringBuilder();
            var tablescope = new StringBuilder();

            using (StreamReader streamReaderScope = new StreamReader(Path.Combine(webRootPath, "ExportTemplates", "TOR_AuditAndScope.html")))
            {
                var htmlContentScope = streamReaderScope.ReadToEnd();
                tablescope.Append(htmlContentScope);
            }

            if (tList.AuditScopes != null)
            {
                foreach (var scope in tList.AuditScopes)
                {
                    returnBuilderScope.Append(tablescope);

                    returnBuilderScope = returnBuilderScope
                       .Replace("#Areas#", scope.Areas)
                       .Replace("#Scope#", scope.Scope);

                }
            }
            returnBuilder = returnBuilder.Replace("#AuditScope#", returnBuilderScope.ToString());

            ////////////////ScopeInfo
            var returnBuilderScopeInfo = new StringBuilder();
            var tablescopeinfo = new StringBuilder();

            using (StreamReader streamReaderScopeinfo = new StreamReader(Path.Combine(webRootPath, "ExportTemplates", "TOR_AuditSpecificInfo.html")))
            {
                var htmlContentScopeinfo = streamReaderScopeinfo.ReadToEnd();
                tablescopeinfo.Append(htmlContentScopeinfo);
            }

            var userd = new MongoGenericRepository<User>(_dbsetting);
            if (tList.AuditSpecificInformations != null)
            {
                foreach (var info in tList.AuditSpecificInformations)
                {
                    returnBuilderScopeInfo.Append(tablescopeinfo);

                    if (info.ProcessOwnerId != null && info.ProcessOwnerId != "")
                    {
                        var objUser = userd.GetFirst(x => x.Id == info.ProcessOwnerId);

                        info.ProcessOwner = objUser == null ? new User() : objUser;
                    }

                    string dataRequestDate = info.DataRequestDate != null ? Convert.ToDateTime(info.DataRequestDate).ToString("dd/MM/yyyy") : "";
                    string dataReceivedDate = info.DataReceivedDate != null ? Convert.ToDateTime(info.DataReceivedDate).ToString("dd/MM/yyyy") : "";

                    returnBuilderScopeInfo = returnBuilderScopeInfo
                       .Replace("#Area#", info.Area)
                       .Replace("#DataRequested#", info.DataRequested)
                       .Replace("#ProcessOwner#", info.ProcessOwner.FirstName != null ? (info.ProcessOwner.FirstName + " " + info.ProcessOwner.LastName) : "")
                       .Replace("#DataRequestDate#", dataRequestDate)
                       .Replace("#DataReceivedDate#", dataReceivedDate)
                       .Replace("#endingData#", info.PendingData)
                       .Replace("#Status#", info.Status);
                }
            }

            returnBuilder = returnBuilder.Replace("#AuditScopeInfo#", returnBuilderScopeInfo.ToString());

            #region WKHTMLTOPDF
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";

            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", returnBuilder.ToString());
            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            #endregion

            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "TOR.pdf");
        }
        public string getAuditApprovers(AuditApprovalMapping approvers)
        {
            List<string> lstUser = new List<string>();
            foreach (var approver in approvers.UserData)
            {
                var resName =
                  approver.User.FirstName +
                  " " +
                  approver.User.LastName +
                  " (" +
                  approver.Responsibility +
                  ")";
                lstUser.Add(resName);
            }
            String[] users = lstUser.ToArray();
            var str = String.Join(",", users);
            return str;
        }
        public string getAuditTeam(List<AuditResource> resources)
        {
            List<string> lstUser = new List<string>();
            foreach (var resource in resources)
            {
                var resName =
                  resource.User.FirstName +
                  " " +
                  resource.User.LastName +
                  " (" +
                  resource.User.Designation +
                  ")";
                lstUser.Add(resName);
            }
            String[] users = lstUser.ToArray();
            var str = String.Join(",", users);
            return str;
        }
        public string getAuditees(List<Auditees> auditees)
        {
            List<string> lstUser = new List<string>();
            foreach (var resource in auditees)
            {
                var resName =
                  resource.User.FirstName +
                  " " +
                  resource.User.LastName +
                  " (" +
                  resource.User.Designation +
                  ")";
                lstUser.Add(resName);
            }
            String[] users = lstUser.ToArray();
            var str = String.Join(",", users);
            return str;
        }
        [HttpGet("getAuditArea/{id}")]
        public ActionResult getAuditArea(string id)
        {
            var tlist = _api.GetFirst(p => p.AuditId == id);
            if (tlist == null)
            {
                return ResponseNotFound();
            }
            List<AuditScope> lstAuditScope = new List<AuditScope>();
            if (tlist.AuditScopes != null) { 
            foreach (var scope in tlist.AuditScopes)
            {
                scope.Areas = scope.Areas != null ? VJLiabraries.UtilityMethods.HtmlToText(scope.Areas) : null;
                scope.Scope = scope.Scope != null ? VJLiabraries.UtilityMethods.HtmlToText(scope.Scope) : null;
                lstAuditScope.Add(scope);
            }
            }
            return ResponseOK(lstAuditScope);
        }

    }
}