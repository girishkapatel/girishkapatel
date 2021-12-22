using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Pdf;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using VJLiabraries;
using static AuditManagementCore.Models.AuditConstants;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InitialDataRequestController : VJBaseGenericAPIController<InitialDataRequest>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        private readonly IWebHostEnvironment _IWebHostEnvironment;

        private readonly IDocumentUpload _IDocumentUpload;
        private readonly IEmailUtility _IEmailUtility;
        #endregion

        public InitialDataRequestController
            (IMongoGenericRepository<InitialDataRequest> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment,
            IDocumentUpload documentUpload, IEmailUtility emailUtility, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
            _IWebHostEnvironment = webHostEnvironment;
            _IDocumentUpload = documentUpload;
            _IEmailUtility = emailUtility;
        }

        public override ActionResult Post([FromBody] InitialDataRequest e)
        {
            if (e == null || e.AuditId == null) return ResponseBad("Initial Data Request object is null or AuditId is null.");
            //var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());
            //if (isExist)
            //{
            //    return ResponseError("Initial Data Request already exists for given Audit");
            //}

            e.DataRequestDate = e.DataRequestDate?.ToLocalTime();
            e.DataReceivedDate = e.DataReceivedDate?.ToLocalTime();

            var initialDataRequest = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Area, "InitialDataRequest", "Manage Audits | Planning | Data Tracker | Add", "Added Data Tracker");

            return ResponseOK(e);
        }
        public override ActionResult Put([FromBody] InitialDataRequest e)
        {
            if (e == null) return ResponseBad("InitialDataRequest object is null");

            var country = _api.GetFirst(x => x.Id == e.Id);

            if (country == null)
                return ResponseError("InitialDataRequest does not exists");

            country.UpdatedBy = e.UpdatedBy;

            e.DataRequestDate = e.DataRequestDate?.ToLocalTime();
            e.DataReceivedDate = e.DataReceivedDate?.ToLocalTime();

            _api.Update(e);

            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Area, "InitialDataRequest", "Manage Audits | Planning | Data Tracker | Edit", "Updated Data Tracker");

            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("Data Tracker object is null");
                var objInitialDataRequest = _api.GetFirst(x => x.Id == id);

                if (objInitialDataRequest == null)
                {
                    return ResponseError("Data Tracker does not exists");
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objInitialDataRequest.Area, "InitialDataRequest", "Manage Audits | Planning | Data Tracker | Delete", "Deleted Data Tracker");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<InitialDataRequest>();
            if (tList == null)
            {
                return ResponseNotFound();
            }

            tList = PopulateInitialDataRequest(tList);
            return ResponseOK(tList);
        }

        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<InitialDataRequest>(x => x.Id == id).FirstOrDefault();

            if (tList == null)
            {
                return ResponseNotFound();
            }
            tList = PopulateInitialDataRequest(tList);
            return ResponseOK(tList);
        }

        [HttpGet("getsummary/{id}")]
        public ActionResult GetSummary(string id)
        {
            var summary = new InitialDataRequestSummary();

            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            var notOverdue = 0;
            var onlyOverdue = 0;

            var partiallyReceived = 0;
            var pending = 0;
            var received = 0;

            foreach (var item in tList)
            {
                if (item.OverdueInDays != null)
                {
                    if (item.OverdueInDays == "0")
                        notOverdue++;

                    if (item.OverdueInDays != "0")
                        onlyOverdue++;
                }

                if (item.Status != null)
                {
                    if (item.Status.ToLower().Trim() == "partially received")
                        partiallyReceived++;

                    if (item.Status.ToLower().Trim() == "pending")
                        pending++;

                    if (item.Status.ToLower().Trim() == "received")
                        received++;
                }
            }

            summary.NotOverdue = notOverdue;
            summary.OnlyOverdue = onlyOverdue;
            summary.PartiallyReceived = partiallyReceived;
            summary.Pending = pending;
            summary.Received = received;

            return ResponseOK(summary);
        }

        [HttpGet("GetByAudit/{id}/{filter}/{status}")]
        public ActionResult GetByAudit(string id, int filter, string status)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            if (filter == 1)
                tList = tList.Where(a => a.OverdueInDays == "0");
            else if (filter == 2)
                tList = tList.Where(a => a.OverdueInDays != "0");

            if (status.Trim() != "" && status.ToLower().Trim() != "all")
                tList = tList.Where(a => a.Status == status);

            tList = PopulateInitialDataRequest(tList);

            return ResponseOK(tList);
        }

        [HttpGet("GetByAuditAndDue/{id}/{OverDue}")]
        public ActionResult GetByAuditAndDue(string id, string OverDue)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id && x.OverdueInDays == OverDue);

            if (tList == null)
            {
                return ResponseNotFound();
            }

            tList = PopulateInitialDataRequest(tList);

            return ResponseOK(tList);
        }

        private IQueryable<InitialDataRequest> PopulateInitialDataRequest(IQueryable<InitialDataRequest> tList)
        {
            foreach (var item in tList)
            {
                PopulateInitialDataRequest(item);
            }

            return tList;
        }

        private InitialDataRequest PopulateInitialDataRequest(InitialDataRequest item)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            item.ProcessOwner = userRepo.GetByID(item.ProcessOwnerId);
            item.Audit = _CommonServices.GetAuditDetail(item.AuditId);

            if (item.Audit.ProcessLocationMapping != null)
            {
                item.Audit.ProcessLocationMapping = plmRepo.GetByID(item.Audit.ProcessLocationMapping.Id);
            }

            var ssRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            item.ScopeAndSchedule = ssRepo.GetFirst(x => x.AuditId == item.AuditId);

            return item;
        }

        [HttpGet("GetInitialDataRequest")]
        public ActionResult GetActivity()
        {
            var tList = getInitialDataRequestData();
            return ResponseOK(tList);
        }
        public IQueryable<InitialDataRequest> getInitialDataRequestData()
        {
            var tList = _api.GetAllWithInclude<Audit>();
            if (tList != null)
            {
                var DataRequestDate = Request.Query["DataRequestDate"];
                var DataReceivedDate = Request.Query["DataReceivedDate"];
                var AuditId = Request.Query["AuditId"];
                var ProcessOwnerId = Request.Query["ProcessOwnerId"];
                var Status = Request.Query["Status"];

                if (!string.IsNullOrWhiteSpace(Status))
                {
                    tList = tList.Where(x => x.Status.ToLower() == Status.ToString().ToLower());
                }

                if (!string.IsNullOrWhiteSpace(DataRequestDate) && !string.IsNullOrWhiteSpace(DataReceivedDate))
                {
                    tList = tList.Where(x => x.DataRequestDate != null && x.DataReceivedDate != null && (x.DataRequestDate >= DateTime.Parse(DataRequestDate)) && (x.DataReceivedDate <= DateTime.Parse(DataReceivedDate)));
                }
                if (!string.IsNullOrWhiteSpace(ProcessOwnerId))
                {
                    tList = tList.Where(x => x.ProcessOwnerId == ProcessOwnerId);
                }
                tList = PopulateInitialDataRequest(tList);
                if (!string.IsNullOrWhiteSpace(AuditId))
                {
                    tList = tList.Where(x => x.Audit.ProcessLocationMapping.Id == AuditId);
                }
            }

            return tList;
        }
        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile(IFormFile[] files)
        {
            if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                return ResponseError("formfile is empty");

            var Id = Request.Form["Id"].ToString().Trim() == "" ? "0" : Request.Form["Id"].ToString().Trim();
            var AuditId = Request.Form["AuditId"].ToString().Trim();
            var module = Request.Form["module"].ToString().Trim();

            var auditModel = _api.GetWithInclude<Audit>(x => x.AuditId == AuditId).FirstOrDefault();

            if (auditModel == null)
                return ResponseNotFound();

            //var location = auditModel.Audit.Location.ProfitCenterCode.Replace("/", "-").Trim();
            //var year = DateTime.Now.Year.ToString();
            //var audit = auditModel.Audit.ProcessLocationMapping.AuditName.Replace("/", "-").Trim();

            try
            {
                List<AuditFiles> returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", AuditId, module.Trim(), Id);

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                foreach (var item in files)
                {
                    var res = await _IDocumentUpload.UploadToWebRoot(item, newPath);

                    AuditFiles auditFiles = new AuditFiles();
                    auditFiles.OriginalFileName = item.FileName;
                    auditFiles.UploadedDatetime = DateTime.Now;
                    auditFiles.UploadedFileName = res;
                    auditFiles.ModuleId = Id;
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
            var auditModel = _api.GetWithInclude<Audit>(x => x.AuditId == auditId).FirstOrDefault();

            if (auditModel == null)
                return ResponseNotFound();

            //var Id = id == null || id == "" ? "0" : id;
            //var location = auditModel.Audit.Location.ProfitCenterCode.Replace("/", "-").Trim();
            //var year = DateTime.Now.Year.ToString();
            //var audit = auditModel.Audit.ProcessLocationMapping.AuditName.Replace("/", "-").Trim();

            try
            {
                var returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", auditId, "datatracker", id);

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var auditFiles = repo.GetMany(a => a.AuditId == auditId && a.ModuleId == id && a.ModuleName == "datatracker");

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

        [HttpPost("sendemail")]
        public IActionResult SendEmail([FromBody] EmailModel emailModel)
        {
            try
            {
                var tList = _api.GetWithInclude<InitialDataRequest>(x => x.Id == emailModel.Id).FirstOrDefault();

                if (tList == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetByID(tList.ProcessOwnerId);

                if (userModel == null)
                    return ResponseNotFound();

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(tList.AuditId);

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

                //userModel.EmailId = "mayursasp.net@gmail.com";

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "InitialDataRequest.html");

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
                    .Replace("#ProcessOwnerName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#AuditName#", auditModel.AuditName)
                    .Replace("#AuditPeriod#", auditPeriod);

                //emailModel.ToEmail = new List<string>() { "baldev@silverwebbuzz.com" };

                emailModel.ToEmail = new List<string>() { userModel.EmailId };
                emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Initial Data Request";
                emailModel.MailBody = emailBody.ToString();
                var file = _CommonServices.DownloadExcelAttachmentForDataTracker(tList);
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

        [HttpGet("downloadexcel/{id}")]
        public IActionResult DownloadExcel(string id)
        {
            var tList = _api.GetWithInclude<Audit, User>(x => x.AuditId == id);
            var repoUser = new MongoGenericRepository<User>(_dbsetting);

            if (tList == null)
                return ResponseNotFound();
            foreach (var item in tList)
            {
                if (item.ProcessOwnerId != null)
                {
                    item.ProcessOwner = repoUser.GetFirst(p => p.Id == item.ProcessOwnerId);
                }
            }
            var fileName = "DataTracker.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                System.Drawing.Color yellow = ColorTranslator.FromHtml("#FFFF00");

                worksheet.Cells["A1"].Value = "Audit";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Area";
                worksheet.Cells["D1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                worksheet.Cells["D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["D1"].Value = "Data Requested*";
                worksheet.Cells["E1"].Value = "Process Owner";
                worksheet.Cells["F1:G1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                worksheet.Cells["F1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["F1:G1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["F1"].Value = "Data Request Date*";
                worksheet.Cells["G1"].Value = "Data Received Date*";
                worksheet.Cells["H1"].Value = "Status";
                worksheet.Cells["I1"].Value = "Overdue (In Days)";
                var rowIndex = 2;
                foreach (var audit in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.AuditName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.Location.ProfitCenterCode;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = audit.Area;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = audit.DataRequested != null ? VJLiabraries.UtilityMethods.HtmlToText(audit.DataRequested) : null;
                    worksheet.Cells["E" + rowIndex.ToString()].Value = audit.ProcessOwner == null ? "" : ((audit.ProcessOwner.FirstName) + " " + (audit.ProcessOwner.LastName));
                    worksheet.Cells["F" + rowIndex.ToString()].Value = audit.DataRequestDate == null ? "" : Convert.ToDateTime(audit.DataRequestDate).ToString("dd/MM/yyyy");
                    worksheet.Cells["G" + rowIndex.ToString()].Value = audit.DataReceivedDate == null ? "" : Convert.ToDateTime(audit.DataReceivedDate).ToString("dd/MM/yyyy");
                    if (!string.IsNullOrEmpty(audit.Status))
                    {
                        audit.Status = audit.Status.ToLower();
                        if (audit.Status == "partially received" || audit.Status == "partiallyreceived")
                            audit.Status = "Partially Received";
                        else if (audit.Status == "pending")
                            audit.Status = "Pending";
                        else if (audit.Status == "received")
                            audit.Status = "Received";
                        worksheet.Cells["H" + rowIndex.ToString()].Value = audit.Status;
                    }
                    var dt2 = audit.DataReceivedDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataReceivedDate);

                    if (DateTime.Now.Date >= dt2)
                    {
                        Int32 OverdueDay = Convert.ToInt32(audit.OverdueInDays) + 1;
                        worksheet.Cells["I" + rowIndex.ToString()].Value = OverdueDay;
                    }
                    else
                        worksheet.Cells["I" + rowIndex.ToString()].Value = 0;
                    rowIndex++;
                }
                double minimumSize = 20;
                double maximumSize = 50;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("sampledownloadexcel/{id}")]
        public IActionResult SampleDownloadExcel(string id)
        {
            var fileName = "DataTracker.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                System.Drawing.Color yellow = ColorTranslator.FromHtml("#FFFF00");

                worksheet.Cells["A1"].Value = "Audit";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Area";
                worksheet.Cells["D1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                worksheet.Cells["D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["D1"].Value = "Data Requested*";
                worksheet.Cells["E1"].Value = "Process Owner";
                worksheet.Cells["F1:G1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                worksheet.Cells["F1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["F1:G1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["F1"].Value = "Data Request Date*";
                worksheet.Cells["G1"].Value = "Data Received Date*";
                worksheet.Cells["H1"].Value = "Status";
                worksheet.Cells["I1"].Value = "Overdue (In Days)";

                #region Added Dropdown in particular column
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("Inputs");
                worksheet2.Cells["A1"].Value = "Status";
                worksheet2.Cells["B1"].Value = "Process Owner";
                int ProcessOwnerIndex = 2, StatusIndex = 2;

                #region Status
                string[] statusEnum = { "Partially Received", "Pending ", "Received" };
                var Status = worksheet.DataValidations.AddListValidation("H:H");
                foreach (var item in statusEnum)
                {
                    Status.Formula.Values.Add(item);
                    worksheet2.Cells["A" + StatusIndex.ToString()].Value = item;
                    StatusIndex++;
                }
                #endregion
                #region Control Owner 
                var _repoUser = new MongoGenericRepository<User>(_dbsetting);
                var lstControlOwner = _repoUser.GetWithInclude<Role, Company, User>(x => x.StakeHolder == true).OrderBy(p => p.FirstName);
                if (lstControlOwner.Count() > 0)
                {
                    foreach (var item in lstControlOwner)
                    {
                        worksheet2.Cells["B" + ProcessOwnerIndex.ToString()].Value = item.FirstName + " " + item.LastName;
                        ProcessOwnerIndex++;
                    }
                    var ProcessOwner = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 5, ExcelPackage.MaxRows, 5));
                    ProcessOwner.AllowBlank = false;
                    ProcessOwner.Formula.ExcelFormula = string.Format("'{0}'!$B$2:$B${1}", worksheet2, ProcessOwnerIndex);
                }
                #endregion
                #endregion
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("downloadpdf/{id}")]
        public IActionResult DownloadPDF(string id)
        {
            var tList = _api.GetWithInclude<Audit, User>(x => x.AuditId == id);
            var repoUser = new MongoGenericRepository<User>(_dbsetting);

            if (tList == null)
                return ResponseNotFound();
            foreach (var item in tList)
            {
                if (item.ProcessOwnerId != null)
                {
                    item.ProcessOwner = repoUser.GetFirst(p => p.Id == item.ProcessOwnerId);
                }
            }

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "OverallAssesmentPresentation.html");
            string bgcolor = "";
            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }

            var returnBuilder = new StringBuilder();
            returnBuilder.Append(emailBody);
            returnBuilder.Append("<p style='font-size: 24px; font-weight: bold;'>Data Tracker</p><table cellpadding='5' cellspacing='0'  border='1' style='width: 100%; font-size: 12px; font-family: Arial'>");
            returnBuilder.Append("<tr><td><b>Audit</b></td><td><b>Location</b></td><td><b>Area</b></td><td><b>Data Requested</b></td><td><b>Process Owner</b></td><td><b>Data Request Date</b></td><td><b>Data Received Date</b></td><td><b>Status</b></td><td><b>Overdue (In Days)</b></td></tr>");
            foreach (var audit in tList)
            {
                var dt1 = audit.DataRequestDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataRequestDate);
                var dt2 = audit.DataReceivedDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataReceivedDate);

                var numberofDays = Math.Ceiling((dt2 - dt1).TotalDays);

                returnBuilder.Append("<tr>");
                returnBuilder.Append("<td>" + (audit.Audit == null ? "" : audit.Audit.AuditName) + "</td>");
                returnBuilder.Append("<td>" + (audit.Audit == null ? "" : audit.Audit.Location.ProfitCenterCode) + "</td>");
                returnBuilder.Append("<td>" + audit.Area + "</td>");
                returnBuilder.Append("<td>" + (audit.DataRequested != null ? VJLiabraries.UtilityMethods.HtmlToText(audit.DataRequested) : null) + "</td>");
                returnBuilder.Append("<td>" + (audit.ProcessOwner == null ? "" : ((audit.ProcessOwner.FirstName) + " " + (audit.ProcessOwner.LastName))) + "</td>");
                returnBuilder.Append("<td>" + (audit.DataRequestDate == null ? "" : Convert.ToDateTime(audit.DataRequestDate).ToString("dd/MM/yyyy")) + "</td>");
                returnBuilder.Append("<td>" + (audit.DataReceivedDate == null ? "" : Convert.ToDateTime(audit.DataReceivedDate).ToString("dd/MM/yyyy")) + "</td>");
                if (audit.Status.ToLower() == "partially received")
                    bgcolor = BackgroundColor.partiallyreceived;
                else if (audit.Status.ToLower() == "received")
                    bgcolor = BackgroundColor.received;
                else
                    bgcolor = BackgroundColor.pending;

                returnBuilder.Append("<td><div style ='padding: 3px 10px; background:" + bgcolor + "; color:#ffffff'  class='text-capitalize'>" + audit.Status + " </div></td>");
                returnBuilder.Append("<td>" + numberofDays + "</td>");
                returnBuilder.Append("</tr>");
            }

            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DataTracker.pdf");
        }
        //public IActionResult DownloadPDF(string id)
        //{
        //    var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

        //    if (tList == null)
        //        return ResponseNotFound();

        //    tList = PopulateInitialDataRequest(tList);

        //    var webRootPath = _IWebHostEnvironment.WebRootPath;
        //    var htmlRowTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DataTrackerRowPDF.html");

        //    var mainRow = new StringBuilder();
        //    using (StreamReader streamReader = new StreamReader(htmlRowTemplatePath))
        //    {
        //        var htmlContent = streamReader.ReadToEnd();
        //        mainRow.Append(htmlContent);
        //    }

        //    var rowsBuilder = new StringBuilder();
        //    var counter = 1;
        //    foreach (var item in tList)
        //    {
        //        rowsBuilder.Append(mainRow);

        //        rowsBuilder = rowsBuilder
        //            .Replace("#Counter#", counter.ToString())
        //            .Replace("#Area#", item.Area)
        //            .Replace("#Status#", item.Status)
        //            .Replace("#DataRequested#", item.DataRequested)
        //            .Replace("#PendingData#", item.PendingData);

        //        if (item.DataRequestDate != null)
        //            rowsBuilder = rowsBuilder.Replace("#DataRequestDate#", Convert.ToDateTime(item.DataRequestDate).ToString("dd-MM-yyyy"));

        //        if (item.DataReceivedDate != null)
        //            rowsBuilder = rowsBuilder.Replace("#DataReceivedDate#", Convert.ToDateTime(item.DataReceivedDate).ToString("dd-MM-yyyy"));

        //        if (item.ProcessOwner != null)
        //            rowsBuilder = rowsBuilder.Replace("#ProcessOwner#", item.ProcessOwner.FirstName + " " + item.ProcessOwner.LastName);

        //        counter++;
        //    }

        //    var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DataTrackerPDF.html");

        //    var mainTable = new StringBuilder();
        //    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
        //    {
        //        var htmlContent = streamReader.ReadToEnd();
        //        mainTable.Append(htmlContent);
        //    }

        //    mainTable = mainTable.Replace("#DataTrackerRowPDF#", rowsBuilder.ToString());

        //    string wkhtmlexepath = Directory.GetCurrentDirectory() + "\\wkhtmltopdf";

        //    byte[] pdfbyte = UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", mainTable.ToString());

        //    var memoryStream = new MemoryStream(pdfbyte);
        //    memoryStream.Position = 0;

        //    return File(memoryStream, UtilityMethods.GetContentType(".pdf"), "DataTracker.pdf");
        //}


        [HttpGet("downloadexcelforReport")]
        public IActionResult DownloadExcelForReport()
        {
            var tList = getInitialDataRequestData();

            if (tList == null)
                return ResponseNotFound();
            var fileName = "DataTracker.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "Audit";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Area";
                worksheet.Cells["D1"].Value = "Data Requested";
                worksheet.Cells["E1"].Value = "Process Owner";
                worksheet.Cells["F1"].Value = "Data Request Date";
                worksheet.Cells["G1"].Value = "Data Received Date";
                worksheet.Cells["H1"].Value = "Status";
                worksheet.Cells["I1"].Value = "Overdue (In Days)";

                var rowIndex = 2;

                foreach (var audit in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.AuditName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.Location.ProfitCenterCode;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = audit.Area;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = audit.DataRequested != null ? VJLiabraries.UtilityMethods.HtmlToText(audit.DataRequested) : null;
                    worksheet.Cells["E" + rowIndex.ToString()].Value = audit.ProcessOwner == null ? "" : ((audit.ProcessOwner.FirstName) + " " + (audit.ProcessOwner.LastName));
                    worksheet.Cells["F" + rowIndex.ToString()].Value = audit.DataReceivedDate == null ? "" : Convert.ToDateTime(audit.DataReceivedDate).ToString("dd/MM/yyyy");
                    worksheet.Cells["G" + rowIndex.ToString()].Value = audit.DataReceivedDate == null ? "" : Convert.ToDateTime(audit.DataReceivedDate).ToString("dd/MM/yyyy");
                    if (!string.IsNullOrEmpty(audit.Status))
                    {
                        audit.Status = audit.Status.ToLower();
                        if (audit.Status == "received")
                            audit.Status = "Received";
                        else if (audit.Status == "partially received")
                            audit.Status = "Partically Received";
                        else if (audit.Status == "pending")
                        {
                            audit.Status = "Pending";
                        }
                    }
                    worksheet.Cells["H" + rowIndex.ToString()].Value = audit.Status;
                    var dt1 = audit.DataRequestDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataRequestDate);
                    var dt2 = audit.DataReceivedDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataReceivedDate);
                    var numberofDays = Math.Ceiling((dt2 - dt1).TotalDays);
                    worksheet.Cells["I" + rowIndex.ToString()].Value = numberofDays;
                    rowIndex++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;

                package.Save();
            }

            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        }

        [HttpGet("downloadpdfForReport")]
        public IActionResult DownloadPDFForReport()
        {
            var tList = getInitialDataRequestData();

            if (tList == null)
                return ResponseNotFound();

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "OverallAssesmentPresentation.html");

            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }
            string bgcolor = "";
            var returnBuilder = new StringBuilder();
            returnBuilder.Append(emailBody);
            returnBuilder.Append("<p style='font-size: 24px; font-weight: bold;'>Data Tracker</p><table cellpadding='5' cellspacing='0'  border='1' style='width: 100%; font-size: 12px; font-family: Arial'>");
            returnBuilder.Append("<tr><td>Audit</td><td>Location</td><td>Area</td><td>Data Requested</td><td>Process Owner</td><td>Data Request Date</td><td>Data Received Date</td><td>Status</td><td>Overdue (In Days)</td></tr>");
            foreach (var audit in tList)
            {
                var dt1 = audit.DataRequestDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataRequestDate);
                var dt2 = audit.DataReceivedDate == null ? DateTime.Now : Convert.ToDateTime(audit.DataReceivedDate);
                var numberofDays = Math.Ceiling((dt2 - dt1).TotalDays);

                returnBuilder.Append("<tr>");
                returnBuilder.Append("<td>" + (audit.Audit == null ? "" : audit.Audit.AuditName) + "</td>");
                returnBuilder.Append("<td>" + (audit.Audit == null ? "" : audit.Audit.Location.ProfitCenterCode) + "</td>");
                returnBuilder.Append("<td>" + audit.Area + "</td>");
                returnBuilder.Append("<td>" + (audit.DataRequested != null ? VJLiabraries.UtilityMethods.HtmlToText(audit.DataRequested) : null) + "</td>");
                returnBuilder.Append("<td>" + (audit.ProcessOwner == null ? "" : ((audit.ProcessOwner.FirstName) + " " + (audit.ProcessOwner.LastName))) + "</td>");
                returnBuilder.Append("<td>" + (audit.DataRequestDate == null ? "" : Convert.ToDateTime(audit.DataRequestDate).ToString("dd/MM/yyyy")) + "</td>");
                returnBuilder.Append("<td>" + (audit.DataReceivedDate == null ? "" : Convert.ToDateTime(audit.DataReceivedDate).ToString("dd/MM/yyyy")) + "</td>");
                if (audit.Status.ToLower() == "partially received")
                    bgcolor = BackgroundColor.partiallyreceived;
                else if (audit.Status.ToLower() == "received")
                    bgcolor = BackgroundColor.received;
                else
                    bgcolor = BackgroundColor.pending;

                returnBuilder.Append("<td><div style ='padding: 3px 10px; background:" + bgcolor + "; color:#ffffff'  class='text-capitalize'>" + audit.Status + " </div></td>");
                returnBuilder.Append("<td>" + numberofDays + "</td>");
                returnBuilder.Append("</tr>");
            }
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DataTracker.pdf");
        }

        [HttpGet("GetUserbyInitialDataRequest")]
        public ActionResult GetUserbyInitialDataRequest()
        {
            var repoInitialDataRequest = new MongoGenericRepository<InitialDataRequest>(_dbsetting);
            var repoUser = new MongoGenericRepository<User>(_dbsetting);

            var tList = (from x in repoInitialDataRequest.GetAll() join user in repoUser.GetAll() on x.ProcessOwnerId equals user.Id select user).Distinct().ToList();
            return ResponseOK(tList);
        }

        [HttpPost("importexcel/{id}/{userid}")]
        public ActionResult ImportExcel(string id, string userid)
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
                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);


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
                                //var audit = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : null;
                                //if (audit != null)
                                //{
                                //    var ResponsibleUser = auditRepo.GetFirst(a =>a.Id==audit);
                                //    if (ResponsibleUser != null)
                                //        exists.PersonResponsibleID = ResponsibleUser.Id;
                                //}
                                InitialDataRequest objInitialDataRequest = new InitialDataRequest();
                                var area = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : "";
                                if (area != "")
                                {
                                    objInitialDataRequest.Area = area;
                                }
                                var datarequest = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : "";
                                if (datarequest != "")
                                {
                                    objInitialDataRequest.DataRequested = datarequest;
                                }
                                var responsiblePerson = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : "";
                                if (responsiblePerson != "")
                                {
                                    var ResponsibleUser = userRepo.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsiblePerson.ToLower());
                                    if (ResponsibleUser != null)
                                        objInitialDataRequest.ProcessOwnerId = ResponsibleUser.Id;
                                }
                                var dateNumStart = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : null;
                                if (dateNumStart != null)
                                {
                                    object value = worksheet.Cells[row, 6].Value.ToString().Trim();

                                    if (value != null)
                                    {
                                        if (value is double)
                                        {
                                            DateTime endDate = DateTime.FromOADate((double)value);
                                            objInitialDataRequest.DataRequestDate = endDate.ToLocalTime();
                                        }
                                        else
                                        {
                                            DateTime dt;
                                            DateTime.TryParse((string)value, out dt);
                                            objInitialDataRequest.DataRequestDate = dt;
                                        }
                                    }
                                }
                                var dateNumEnd = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : null;
                                if (dateNumEnd != null)
                                {
                                    object value = worksheet.Cells[row, 7].Value.ToString().Trim();

                                    if (value != null)
                                    {
                                        if (value is double)
                                        {
                                            DateTime endDate = DateTime.FromOADate((double)value);
                                            objInitialDataRequest.DataReceivedDate = endDate.ToLocalTime();
                                        }
                                        else
                                        {
                                            DateTime dt;
                                            DateTime.TryParse((string)value, out dt);
                                            objInitialDataRequest.DataReceivedDate = dt;
                                        }
                                    }
                                }
                                var status = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : "";
                                if (status != "")
                                {
                                    objInitialDataRequest.Status = status;
                                }
                                var overdue = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : "";
                                if (overdue != "")
                                {
                                    objInitialDataRequest.OverdueInDays = overdue;
                                }
                                objInitialDataRequest.AuditId = id;
                                objInitialDataRequest.CreatedBy = userid;
                                _api.Insert(objInitialDataRequest);
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "InitialDataRequest/ImportExcel()");
                            }
                        }
                    }
                }
                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "InitialDataRequest/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
    }
}