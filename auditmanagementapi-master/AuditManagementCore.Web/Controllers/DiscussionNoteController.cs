using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service.Utilities;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Wkhtmltopdf.NetCore;
using AuditManagementCore.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VJLiabraries;
using AuditManagementCore.Service;
using Aspose.Pdf;
using System.Drawing;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscussionNoteController : VJBaseGenericAPIController<DiscussionNote>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;

        IWebHostEnvironment _IWebHostEnvironment;

        IEmailUtility _IEmailUtility;
        IDocumentUpload _IDocumentUpload;
        CommonServices _CommonServices;
        readonly IGeneratePdf _generatePdf;
        #endregion

        public DiscussionNoteController
            (IMongoGenericRepository<DiscussionNote> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility,
            IDocumentUpload documentUpload, IGeneratePdf generatePdf, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _CommonServices = cs;
            _IEmailUtility = emailUtility;
            _IDocumentUpload = documentUpload;
            _generatePdf = generatePdf;
        }

        public override ActionResult Post([FromBody] DiscussionNote e)
        {

            if (e == null) return ResponseBad("DiscussionNote is null");

            var isExist = _api.Exists(x => x.DiscussionNumber.ToLower() == e.DiscussionNumber.ToLower());

            if (isExist)
            {
                return AlreadyExistResponseError("Discussion number already Exists.");
            }

            var discussionNote = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.DiscussionNumber, "DiscussionNote", "Manage Audits | Audit Execution | DiscussionNote | Add", "Added DiscussionNote");

            return discussionNote;
        }

        [HttpGet("GetByAudit/{id}/{grading}/{status}")]
        public ActionResult GetByAudit(string id, string grading, string status)
        {
            var tList = _api.GetWithInclude<User>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            if (grading.Trim() != "" && grading.Trim().ToLower() != "all")
            {
                var grade = ObservationGradingEnum.Critical;
                if (grading.ToLower().Trim() == "high")
                    grade = ObservationGradingEnum.High;
                else if (grading.ToLower().Trim() == "medium")
                    grade = ObservationGradingEnum.Medium;
                else if (grading.ToLower().Trim() == "low")
                    grade = ObservationGradingEnum.Low;

                tList = tList.Where(a => a.ObservationGrading == grade);
            }

            if (tList == null)
                return ResponseNotFound();

            if (status.Trim() != "" && status.Trim().ToLower() != "all")
            {
                if (status.Trim().ToLower() == "notstarted")
                    tList = tList.Where(a => a.Status == null || a.Status.ToLower().Trim() == status.ToLower().Trim());
                else
                    tList = tList.Where(a => a.Status != null && a.Status.ToLower().Trim() == status.ToLower().Trim());
            }
            if (tList == null)
                return ResponseNotFound();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);

            foreach (var item in tList)
            {
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
                item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
                item.RiskType = riskTypeRepo.GetByID(item.RiskTypeId);
                item.RiskTypes = GetRiskTypesList(item.RiskTypeIds);
                item.FilesList = GetAllFilesList(id, item.Id);
            }

            return ResponseOK(tList);
        }

        [HttpGet("GetByAuditByGrading/{id}/{grading}")]
        public ActionResult GetByAuditByGrading(string id, string grading)
        {
            var grade = ObservationGradingEnum.High;
            if (grading.ToLower() == "medium")
            {
                grade = ObservationGradingEnum.Medium;
            }
            else if (grading.ToLower() == "low")
            {
                grade = ObservationGradingEnum.Low;
            }
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);

            if (grading.ToLower() == "all")
            {
                var tList = _api.GetWithInclude<User>(x => x.AuditId == id);


                if (tList == null)
                    return ResponseNotFound();

                foreach (var item in tList)
                {
                    item.Reviewer = userRepo.GetByID(item.ReviewerId);
                    item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
                    item.RiskType = riskTypeRepo.GetByID(item.RiskTypeId);
                }

                return ResponseOK(tList);
            }
            else
            {
                var tList = _api.GetWithInclude<User>(x => x.AuditId == id && x.ObservationGrading == grade);

                if (tList == null)
                    return ResponseNotFound();

                foreach (var item in tList)
                {
                    item.Reviewer = userRepo.GetByID(item.ReviewerId);
                    item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
                    item.RiskType = riskTypeRepo.GetByID(item.RiskTypeId);
                }

                return ResponseOK(tList);
            }


        }

        [HttpGet("getsummary/{id}")]
        public ActionResult GetSummary(string id)
        {
            var discussionNoteSummary = new DiscussionNoteSummaryModel();

            var tList = _api.GetWithInclude<User>(x => x.AuditId == id);
            if (tList == null)
                return ResponseNotFound();

            discussionNoteSummary.Critical = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Critical).Count();
            discussionNoteSummary.High = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.High).Count();
            discussionNoteSummary.Medium = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Medium).Count();
            discussionNoteSummary.Low = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Low).Count();
            discussionNoteSummary.NotStarted = tList.Where(a => a.Status == null || a.Status.ToLower().ToString() == "notstarted").Count();
            discussionNoteSummary.InProgress = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "inprogress").Count();
            discussionNoteSummary.InReview = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "inreview").Count();
            discussionNoteSummary.Completed = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "completed").Count();

            return ResponseOK(discussionNoteSummary);
        }

        public override ActionResult GetAll()
        {
            var discussionNoterepo = _api.GetAll();

            if (discussionNoterepo == null)
            {
                return ResponseNotFound();
            }

            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            foreach (var item in discussionNoterepo)
            {
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
                item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
            }
            return ResponseOK(discussionNoterepo);
        }

        public override ActionResult Put([FromBody] DiscussionNote tValue)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            //Activity Log
            _CommonServices.ActivityLog(tValue.UpdatedBy, tValue.Id, tValue.DiscussionNumber, "DiscussionNote", "Manage Audits | Audit Execution | DiscussionNote | Edit", "Updated DiscussionNote");
            _api.Update(tValue);
            if (tValue.Status != AuditConstants.Status.COMPLETED)
            {
                var repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
                var isExist = repoDraftReport.GetFirst(x => x.DiscussionNoteID.ToLower() == tValue.Id && x.AuditId == tValue.AuditId);
                if (isExist != null)
                {
                    repoDraftReport.Delete(isExist);
                }
            }
            return Ok(tValue);
        }

        [HttpPut("UpdateDiscussionNoteStatus")]
        public ActionResult UpdateDiscussionNoteStatus(DNStatusUpdate dNStatusUpdate)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");
            var discussionNote = _api.GetByID(dNStatusUpdate.Id);
            if (discussionNote != null)
            {
                switch (dNStatusUpdate.Status.ToUpper())
                {
                    case "NOTSTARTED":
                        discussionNote.Status = AuditConstants.Status.NOTSTRATED;
                        break;
                    case "INREVIEW":
                        discussionNote.Status = AuditConstants.Status.INREVIEW;
                        break;
                    case "COMPLETED":
                        discussionNote.Status = AuditConstants.Status.COMPLETED;
                        break;
                    default:
                        discussionNote.Status = AuditConstants.Status.INPROGRESS;
                        break;
                }
                if (dNStatusUpdate.Justification != "")
                    discussionNote.Justification = dNStatusUpdate.Justification;

                if (dNStatusUpdate.DiscussionComments != "")
                    discussionNote.DiscussionComments = dNStatusUpdate.DiscussionComments;
                discussionNote.UpdatedBy = dNStatusUpdate.UpdatedBy;

                _api.Update(discussionNote);

                if (discussionNote.Status != AuditConstants.Status.COMPLETED)
                {
                    var repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
                    var isExist = repoDraftReport.GetFirst(x => x.DiscussionNoteID.ToLower() == discussionNote.Id && x.AuditId == discussionNote.AuditId);
                    if (isExist != null)
                    {
                        repoDraftReport.Delete(isExist);
                    }
                }
                _CommonServices.SaveHistoryforDiscussionNote(discussionNote.UpdatedBy, discussionNote.Status, discussionNote.Id);
                #region
                _CommonServices.InsertNotification("", "DiscussionNote", false, discussionNote.AuditId, discussionNote.Status.ToUpper(), discussionNote.DiscussionNumber, discussionNote.UpdatedBy);

                #endregion
                #region Send email
                if (discussionNote.Status.ToLower() == "pending")
                {
                    var userRepo = new MongoGenericRepository<User>(_dbsetting);
                    var userModel = userRepo.GetByID(discussionNote.ReviewerId);
                    //userModel.EmailId = "mayursasp.net@gmail.com";

                    var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                    var auditModel = auditRepo.GetByID(discussionNote.AuditId);

                    var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                    var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                    var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                    var webRootPath = _IWebHostEnvironment.WebRootPath;
                    var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "DiscussionNoteSendForApproval.html");

                    var emailBody = new StringBuilder();
                    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                    {
                        var htmlContent = streamReader.ReadToEnd();
                        emailBody.Append(htmlContent);
                    }

                    emailBody = emailBody
                        .Replace("#ReviewerName#", userModel.FirstName + " " + userModel.LastName)
                        .Replace("#AuditName#", auditModel.AuditName)
                        .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));

                    var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                    var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                    var auditPeriod = auditStartDate + " to " + auditEndDate;

                    var emailModel = new Service.Utilities.EmailModel()
                    {
                        ToEmail = new List<string>() { userModel.EmailId },
                        Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Discussion Note",
                        MailBody = emailBody.ToString()
                    };
                    _IEmailUtility.SendEmail(emailModel);
                }
                #endregion
                _CommonServices.ActivityLog(discussionNote.UpdatedBy, discussionNote.Id, discussionNote.DiscussionNumber, "DiscussionNote", "Manage Audits | Audit Execution | DiscussionNote | Edit", "Updated DiscussionNote Status");
                return Ok(discussionNote);
            }
            else
                return NotFound();
        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail([FromBody] Service.Utilities.EmailModel emailModel)
        {
            try
            {
                var discussionNote = _api.GetByID(emailModel.Id);

                if (discussionNote == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetByID(discussionNote.ReviewerId);

                if (userModel == null)
                    return ResponseNotFound();

                //userModel.EmailId = "mayursasp.net@gmail.com";

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(discussionNote.AuditId);

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

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "DiscussionNote.html");

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
                    .Replace("#ReviewerName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#AuditName#", auditModel.AuditName)
                    .Replace("#AuditPeriod#", auditPeriod);

                emailModel.ToEmail = new List<string>() { userModel.EmailId };
                emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Draft discussion note";
                emailModel.MailBody = emailBody.ToString();

                var file = DownloadExcelAttachment(discussionNote);
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

        [HttpGet("downloadexcel/{id}")]
        public IActionResult DownloadExcel(string id)
        {
            try
            {
                var tList = _api.GetWithInclude<User>(x => x.AuditId == id);
                if (tList == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);

                foreach (var item in tList)
                {
                    item.Reviewer = item.ReviewerId != null ? userRepo.GetByID(item.ReviewerId) : null;
                    item.ResponsiblePerson = item.PersonResponsibleID != null ? userRepo.GetByID(item.PersonResponsibleID) : null;
                    item.RiskTypes = GetRiskTypesList(item.RiskTypeIds);
                }

                var fileName = "DiscussionNotes.xlsx";
                var memoryStream = new MemoryStream();

                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    System.Drawing.Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    worksheet.Cells["A1:C1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["A1:C1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells["A1"].Value = "RACM Number*";
                    worksheet.Cells["B1"].Value = "Discussion No.*";
                    worksheet.Cells["C1"].Value = "Discussion Heading*";
                    worksheet.Cells["D1"].Value = "Background";
                    worksheet.Cells["E1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells["E1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["E1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["E1"].Value = "Detailed Observation*";
                    worksheet.Cells["F1"].Value = "Recommendation";
                    worksheet.Cells["G1:L1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells["G1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["G1:L1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["G1"].Value = "Root Cause*";
                    worksheet.Cells["H1"].Value = "Risk(s) / Business Impact*";
                    worksheet.Cells["I1"].Value = "Risk Types*";
                    worksheet.Cells["J1"].Value = "Responsibility*";
                    worksheet.Cells["K1"].Value = "Reviewer*";
                    worksheet.Cells["L1"].Value = "Observation Grading*";
                    worksheet.Cells["M1"].Value = "Checkbox for flagging issue to be considered in report";
                    worksheet.Cells["N1"].Value = "Management Comments";
                    //worksheet.Cells["O1"].Value = "Repeat";
                    //worksheet.Cells["P1"].Value = "System Improvement";
                    //worksheet.Cells["Q1"].Value = "Red Flag";
                    //worksheet.Cells["R1"].Value = "Leading Practices";
                    //worksheet.Cells["S1:U1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    //worksheet.Cells["S1:U1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //worksheet.Cells["S1:U1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    //worksheet.Cells["S1"].Value = "Potential Saving*";
                    //worksheet.Cells["T1"].Value = "Realised Saving*";
                    //worksheet.Cells["U1"].Value = "Leakage*";
                    //worksheet.Cells["V1"].Value = "Impact";
                    //worksheet.Cells["W1"].Value = "Recommendation";
                    //worksheet.Cells["X1"].Value = "Root Cause"; 
                    var rowIndex = 2;

                    foreach (var discussionNote in tList)
                    {
                        var _responsiblityName =
                            discussionNote.ResponsiblePerson != null ?
                            (discussionNote.ResponsiblePerson.FirstName != "" ? discussionNote.ResponsiblePerson.FirstName + " " : "") +
                            (discussionNote.ResponsiblePerson.MiddleName != "" ? discussionNote.ResponsiblePerson.MiddleName + " " : "") +
                            (discussionNote.ResponsiblePerson.LastName != "" ? discussionNote.ResponsiblePerson.LastName : "") : "";

                        var _reviewerName =
                            discussionNote.Reviewer != null ?
                            (discussionNote.Reviewer.FirstName != "" ? discussionNote.Reviewer.FirstName + " " : "") +
                            (discussionNote.Reviewer.MiddleName != "" ? discussionNote.Reviewer.MiddleName + " " : "") +
                            (discussionNote.Reviewer.LastName != "" ? discussionNote.Reviewer.LastName : "") : "";

                        var _observationGrading = discussionNote.ObservationGrading == ObservationGradingEnum.High ? "High" :
                            discussionNote.ObservationGrading == ObservationGradingEnum.Medium ? "Medium" :
                            discussionNote.ObservationGrading == ObservationGradingEnum.Low ? "Low" :
                            discussionNote.ObservationGrading == ObservationGradingEnum.Critical ? "Critical" : "";

                        var _riskTypesNames = string.Empty;
                        foreach (var item in discussionNote.RiskTypes)
                        {
                            _riskTypesNames += item.Name + ", ";
                        }

                        _riskTypesNames = _riskTypesNames.Trim().TrimEnd(',');

                        worksheet.Cells["A" + rowIndex.ToString()].Value = discussionNote.RACM_Ids != null ? discussionNote.RACM_Ids[0] : "";
                        worksheet.Cells["B" + rowIndex.ToString()].Value = discussionNote.DiscussionNumber;
                        worksheet.Cells["C" + rowIndex.ToString()].Value = discussionNote.ObservationHeading;
                        worksheet.Cells["D" + rowIndex.ToString()].Value = discussionNote.FieldBackground != null ? UtilityMethods.HtmlToText(discussionNote.FieldBackground) : null;
                        worksheet.Cells["E" + rowIndex.ToString()].Value = discussionNote.DetailedObservation != null ? UtilityMethods.HtmlToText(discussionNote.DetailedObservation) : null;
                        worksheet.Cells["F" + rowIndex.ToString()].Value = discussionNote.Recommendation != null ? UtilityMethods.HtmlToText(discussionNote.Recommendation) : null;
                        worksheet.Cells["G" + rowIndex.ToString()].Value = discussionNote.RootCause != null ? UtilityMethods.HtmlToText(discussionNote.RootCause) : null;
                        worksheet.Cells["H" + rowIndex.ToString()].Value = discussionNote.Risks != null ? UtilityMethods.HtmlToText(discussionNote.Risks) : null;
                        worksheet.Cells["I" + rowIndex.ToString()].Value = _riskTypesNames;
                        worksheet.Cells["J" + rowIndex.ToString()].Value = _responsiblityName;
                        worksheet.Cells["K" + rowIndex.ToString()].Value = _reviewerName;
                        worksheet.Cells["L" + rowIndex.ToString()].Value = _observationGrading;
                        worksheet.Cells["M" + rowIndex.ToString()].Value = discussionNote.FlagIssueForReport ? "Yes" : "No";
                        worksheet.Cells["N" + rowIndex.ToString()].Value = discussionNote.ManagementComments != null ? UtilityMethods.HtmlToText(discussionNote.ManagementComments) : null;
                        //worksheet.Cells["O" + rowIndex.ToString()].Value = discussionNote.IsRepeat ? "Yes" : "No";
                        //worksheet.Cells["P" + rowIndex.ToString()].Value = discussionNote.isSystemImprovement ? "Yes" : "No";
                        //worksheet.Cells["Q" + rowIndex.ToString()].Value = discussionNote.isRedFlag ? "Yes" : "No";
                        //worksheet.Cells["R" + rowIndex.ToString()].Value = discussionNote.isLeadingPractices ? "Yes" : "No";
                        //worksheet.Cells["S" + rowIndex.ToString()].Value = discussionNote.PotentialSaving;
                        //worksheet.Cells["T" + rowIndex.ToString()].Value = discussionNote.RealisedSaving;
                        //worksheet.Cells["U" + rowIndex.ToString()].Value = discussionNote.Leakage;
                        //worksheet.Cells["V" + rowIndex.ToString()].Value = discussionNote.Impacts != null ?_CommonServices.getImpacts(discussionNote.Impacts.ToList()) : "";  
                        //worksheet.Cells["W" + rowIndex.ToString()].Value =  discussionNote.Recommendations != null ?_CommonServices.getRecommendations(discussionNote.Recommendations.ToList()) : "";  
                        //worksheet.Cells["X" + rowIndex.ToString()].Value = discussionNote.RootCauses != null ? _CommonServices.getCause(discussionNote.RootCauses.ToList()) : "";
                        rowIndex++;
                    }
                    double minimumSize = 20;
                    double maximumSize = 50;
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize);
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                    worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                    worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    package.Save();
                }
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "DiscussionNote/DownloadExcel");
            }
            return Ok();
        }

        [HttpGet("sampledownloadexcel/{id}")]
        public IActionResult SamleDownloadExcel(string id)
        {
            try
            {
                var fileName = "DiscussionNotes.xlsx";
                var memoryStream = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    System.Drawing.Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    worksheet.Cells["A1:C1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["A1:C1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells["A1"].Value = "RACM Number*";
                    worksheet.Cells["B1"].Value = "Discussion No.*";
                    worksheet.Cells["C1"].Value = "Discussion Heading*";
                    worksheet.Cells["D1"].Value = "Background";
                    worksheet.Cells["E1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells["E1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["E1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["E1"].Value = "Detailed Observation*";
                    worksheet.Cells["F1"].Value = "Recommendation";
                    worksheet.Cells["G1:L1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    worksheet.Cells["G1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["G1:L1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    worksheet.Cells["G1"].Value = "Root Cause*";
                    worksheet.Cells["H1"].Value = "Risk(s) / Business Impact*";
                    worksheet.Cells["I1"].Value = "Risk Types*";
                    worksheet.Cells["J1"].Value = "Responsibility*";
                    worksheet.Cells["K1"].Value = "Reviewer*";
                    worksheet.Cells["L1"].Value = "Observation Grading*";
                    worksheet.Cells["M1"].Value = "Checkbox for flagging issue to be considered in report";
                    worksheet.Cells["N1"].Value = "Management Comments";
                    #region Added Dropdown in particular column
                    string worksheet2Name = "Input_Params";
                    ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add(worksheet2Name);
                    worksheet2.Cells["A1"].Value = "RACM Number";
                    worksheet2.Cells["B1"].Value = "Risk Types";
                    worksheet2.Cells["C1"].Value = "Responsibility";
                    worksheet2.Cells["D1"].Value = "Reviewer";
                    worksheet2.Cells["E1"].Value = "Observation Grading";
                    worksheet2.Cells["F1"].Value = "Checkbox for flagging issue to be considered in report";
                    int ObservationGradingIndex = 2, RiskTypeIndex = 2, RacmNumberIndex = 2, _flagIssueIndex = 2, _usersIndex = 2, RootCauseIndex = 2, ImpactIndex = 2, RecommendationIndex = 2;
                    #region RACM Number
                    var _reporacmdetail = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);

                    var lstRacm = _reporacmdetail.GetAll();

                    if (lstRacm.Count() > 0)
                    {
                        foreach (var item in lstRacm)
                        {
                            if (item != null && item.RACMnumber != null && item.RACMnumber.Trim() != "")
                            {
                                worksheet2.Cells["A" + RacmNumberIndex.ToString()].Value = item.RACMnumber;

                                RacmNumberIndex++;
                            }
                        }

                        var locationEx = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 1, ExcelPackage.MaxRows, 1));
                        locationEx.AllowBlank = false;
                        locationEx.Formula.ExcelFormula = string.Format("'{0}'!$A$2:$A${1}", worksheet2Name, RacmNumberIndex);
                    }
                    #endregion
                    #region Risk Types
                    var _repoRiskType = new MongoGenericRepository<RiskType>(_dbsetting);
                    var lstRiskType = _repoRiskType.GetMany(p => p.Name != null);
                    if (lstRiskType.Count() > 0)
                    {
                        foreach (var item in lstRiskType)
                        {
                            if (item != null && item.Name != null && item.Name.Trim() != "")
                            {
                                worksheet2.Cells["B" + RiskTypeIndex.ToString()].Value = item.Name;
                                RiskTypeIndex++;
                            }
                        }
                        var riskType1 = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 9, ExcelPackage.MaxRows, 9));
                        riskType1.AllowBlank = false;
                        riskType1.Formula.ExcelFormula = string.Format("'{0}'!$B$2:$B${1}", worksheet2Name, RiskTypeIndex);


                        //var _sbMultiSelectScript = new StringBuilder();
                        //_sbMultiSelectScript.Append(
                        //    "Private Sub Worksheet_Change(ByVal Target As Range)\n" +
                        //    "   'Code by Sumit Bansal from https://trumpexcel.com\n" +
                        //    "   ' To make mutliple selections in a Drop Down List in Excel\n" +
                        //    "   Dim Oldvalue As String\n" +
                        //    "   Dim Newvalue As String\n" +
                        //    "   On Error GoTo Exitsub\n" +
                        //    "   If Target.Column  = \"9\" Then\n" +
                        //    "       If Target.SpecialCells(xlCellTypeAllValidation) Is Nothing Then\n" +
                        //    "           GoTo Exitsub\n" +
                        //    "       Else: If Target.Value = \"\" Then GoTo Exitsub Else\n" +
                        //    "           Application.EnableEvents = False\n" +
                        //    "           Newvalue = Target.Value\n" +
                        //    "           Application.Undo\n" +
                        //    "           Oldvalue = Target.Value\n" +
                        //    "           If Oldvalue = \"\" Then\n" +
                        //    "               Target.Value = Newvalue\n" +
                        //    "           Else\n" +
                        //    "               Target.Value = Oldvalue & \", \" & Newvalue\n" +
                        //    "           End If\n" +
                        //    "       End If\n" +
                        //    "   End If\n" +
                        //    "   Application.EnableEvents = True\n" +
                        //    "   Exitsub:\n" +
                        //    "   Application.EnableEvents = True\n" +
                        //    "   End Sub");

                        //package.Workbook.CreateVBAProject();
                        //worksheet.CodeModule.Code = _sbMultiSelectScript.ToString();
                        //package.Save();
                        //ScriptForAllowMultiSelectInExcel(package, worksheet, string.Format("$I$2:$I${0}", ExcelPackage.MaxRows.ToString()));
                    }
                    #endregion
                    #region Reviewer & Responsibility
                    var _userRepo = new MongoGenericRepository<User>(_dbsetting);
                    var usersList = _userRepo.GetAll();
                    if (usersList != null && usersList.Count() > 0)
                    {
                        foreach (var item in usersList)
                        {
                            var _userName =
                                (item.FirstName != "" ? item.FirstName + " " : "") + (item.MiddleName != "" ? item.MiddleName + " " : "") +
                                (item.LastName != "" ? item.LastName : "");

                            if (_userName != null && _userName.Trim() != "")
                            {
                                worksheet2.Cells["C" + _usersIndex.ToString()].Value = _userName;
                                worksheet2.Cells["D" + _usersIndex.ToString()].Value = _userName;
                                _usersIndex++;
                            }
                        }

                        var _responsibility = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 10, ExcelPackage.MaxRows, 10));
                        _responsibility.AllowBlank = false;
                        _responsibility.Formula.ExcelFormula = string.Format("'{0}'!$C$2:$C${1}", worksheet2Name, _usersIndex);

                        var _reviewers = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 11, ExcelPackage.MaxRows, 11));
                        _reviewers.AllowBlank = false;
                        _reviewers.Formula.ExcelFormula = string.Format("'{0}'!$D$2:$D${1}", worksheet2Name, _usersIndex);
                    }
                    #endregion
                    #region Observation Grading
                    string[] lstObservationGrading = Enum.GetNames(typeof(ObservationGradingEnum));
                    if (lstObservationGrading.Count() > 0)
                    {
                        foreach (var item in lstObservationGrading)
                        {
                            if (item != null && item.Trim() != "")
                            {
                                worksheet2.Cells["E" + ObservationGradingIndex.ToString()].Value = item;
                                ObservationGradingIndex++;
                            }
                        }
                        var observationColumn = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 12, ExcelPackage.MaxRows, 12));
                        observationColumn.AllowBlank = false;
                        observationColumn.Formula.ExcelFormula = string.Format("'{0}'!$E$2:$E${1}", worksheet2Name, ObservationGradingIndex);
                    }
                    #endregion
                    #region Checkbox for flagging issue to be considered in report
                    string[] _flagIssue = { "Yes", "No" };
                    if (_flagIssue.Count() > 0)
                    {
                        foreach (var item in _flagIssue)
                        {
                            if (item != null && item.Trim() != "")
                            {
                                worksheet2.Cells["F" + _flagIssueIndex.ToString()].Value = item;

                                _flagIssueIndex++;
                            }
                        }
                        var _flagIssueCol = worksheet.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 13, ExcelPackage.MaxRows, 13));
                        _flagIssueCol.AllowBlank = false;
                        _flagIssueCol.Formula.ExcelFormula = string.Format("'{0}'!$F$2:$F${1}", worksheet2Name, _flagIssueIndex);
                    }
                    #endregion
                    #endregion
                    double minimumSize = 20;
                    double maximumSize = 50;

                    worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns();
                    worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns(minimumSize);
                    worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                    worksheet2.Cells["A1:XFD1"].Style.Font.Bold = true;
                    worksheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize);
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                    worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                    worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    package.Save();
                }
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "DiscussionNote/SamleDownloadExcel");
            }
            return Ok();
        }

        [HttpGet("downloadpdf/{id}")]
        public IActionResult DownloadPDF(string id)
        {
            var tList = _api.GetWithInclude<User>(x => x.AuditId == id);
            if (tList == null)
                return ResponseNotFound();

            #region Create PDF 
            //PdfPTable tableLayout = new PdfPTable(10);

            //float[] headers = { 30, 50, 100, 30, 50, 50, 100, 25, 32, 17 }; //Header Widths  
            //tableLayout.SetWidths(headers); //Set the pdf headers  
            //tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            //tableLayout.HeaderRows = 1;
            ////Add Title to the PDF file at the top  

            ////tableLayout.AddCell(
            ////    new PdfPCell(new Phrase("Creating Pdf using ItextSharp", new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0))))
            ////    {
            ////        Colspan = 12,
            ////        Border = 0,
            ////        PaddingBottom = 5,
            ////        HorizontalAlignment = Element.ALIGN_CENTER
            ////    });

            //AddCellToHeader(tableLayout, "Discussion Note No.");
            //AddCellToHeader(tableLayout, "Observation Heading");
            //AddCellToHeader(tableLayout, "Detailed Observation");
            //AddCellToHeader(tableLayout, "Control Number");
            //AddCellToHeader(tableLayout, "Root Cause");
            //AddCellToHeader(tableLayout, "Risk Source");
            //AddCellToHeader(tableLayout, "Risk / Implication");
            //AddCellToHeader(tableLayout, "Financial Impact");
            //AddCellToHeader(tableLayout, "Observation Grading");
            //AddCellToHeader(tableLayout, "Flag Issue");

            //////Add body  
            //foreach (var discussionNote in tList)
            //{
            //    AddCellToBody(tableLayout, discussionNote.DiscussionNumber);
            //    AddCellToBody(tableLayout, discussionNote.ObservationHeading);
            //    AddCellToBody(tableLayout, discussionNote.DetailedObservation);
            //    AddCellToBody(tableLayout, "");
            //    AddCellToBody(tableLayout, discussionNote.RootCause);
            //    AddCellToBody(tableLayout, discussionNote.RiskType);
            //    AddCellToBody(tableLayout, discussionNote.Risks);
            //    AddCellToBody(tableLayout, discussionNote.FinancialImpact);
            //    AddCellToBody(tableLayout, discussionNote.ObservationGrading.ToString());
            //    AddCellToBody(tableLayout, discussionNote.FlagIssueForReport ? "Yes" : "No");
            //}

            //var fileName = "DiscussionNotes.pdf";
            //var memoryStream = new MemoryStream();

            //Document doc = new Document();
            //doc.SetPageSize(PageSize.A4.Rotate());
            //doc.SetMargins(5, 5, 5, 5);

            //PdfWriter.GetInstance(doc, memoryStream).CloseStream = false;
            //doc.Open();
            //doc.Add(tableLayout);
            //doc.Close();

            //memoryStream.Position = 0;
            //return File(memoryStream, "application/pdf", "Presentation.pdf");
            #endregion

            #region SyncfusiuonPPTX
            //string basePath = Path.Combine(_IWebHostEnvironment.WebRootPath, "Presentation", "DiscussionNoteExportTemplate.pptx");
            //FileStream fileStreamInput = new FileStream(basePath, FileMode.Open, FileAccess.Read);
            //IPresentation presentation = Presentation.Open(fileStreamInput);

            //IPresentation presentation = Presentation.Create();

            //var counter = 1;

            //foreach (var discussionNote in tList)
            //{
            //    ISlide slide = presentation.Slides.Add(SlideLayoutType.Blank);

            //    #region Header
            //    //Adds textbox to the slide
            //    IShape textboxShape = slide.AddTextBox(15, 15, 930, 100);

            //    //Adds paragraph to the textbody of textbox
            //    IParagraph paragraph = textboxShape.TextBody.AddParagraph();

            //    //Adds a TextPart to the paragraph
            //    ITextPart textPart = paragraph.AddTextPart();

            //    //Adds text to the TextPart
            //    textPart.Text = counter.ToString() + ". " + VJLiabraries.UtilityMethods.HtmlToText(discussionNote.ObservationHeading);
            //    textPart.Font.FontName = "Arial (Heading)";
            //    textPart.Font.Bold = true;
            //    textPart.Font.FontSize = 24;
            //    #endregion

            //    #region Body
            //    //Start - Seperator
            //    IShape seperatorShape = slide.AddTextBox(15, 115, 930, 2);
            //    seperatorShape.Fill.FillType = FillType.Solid;
            //    seperatorShape.Fill.SolidFill.Color = ColorObject.Gold;
            //    //End - Seperator

            //    //Start - Background Label
            //    IShape backgroundLabel = slide.AddTextBox(15, 120, 930, 25);
            //    backgroundLabel.Fill.FillType = FillType.Solid;
            //    backgroundLabel.Fill.SolidFill.Color = ColorObject.Gold;

            //    IParagraph paragraph1 = backgroundLabel.TextBody.AddParagraph();
            //    ITextPart textPart1 = paragraph1.AddTextPart();
            //    textPart1.Text = "Background";
            //    textPart1.Font.FontName = "Arial";
            //    textPart1.Font.Bold = true;
            //    textPart1.Font.FontSize = 12;
            //    //End - Background Label

            //    //Start - Background Value
            //    IShape backgroundShape = slide.AddTextBox(15, 145, 930, 100);

            //    IParagraph paragraph2 = backgroundShape.TextBody.AddParagraph();
            //    ITextPart textPart2 = paragraph2.AddTextPart();
            //    textPart2.Text = VJLiabraries.UtilityMethods.HtmlToText(discussionNote.FieldBackground).Trim();
            //    textPart2.Font.FontName = "Arial";
            //    textPart2.Font.FontSize = 12;
            //    //End - Background Value

            //    //Start - Observation Label
            //    IShape observationLabel = slide.AddTextBox(15, 265, 930, 25);
            //    observationLabel.Fill.FillType = FillType.Solid;
            //    observationLabel.Fill.SolidFill.Color = ColorObject.Gold;

            //    IParagraph paragraph3 = observationLabel.TextBody.AddParagraph();
            //    ITextPart textPart3 = paragraph3.AddTextPart();
            //    textPart3.Text = "Obversation(s)";
            //    textPart3.Font.FontName = "Arial";
            //    textPart3.Font.Bold = true;
            //    textPart3.Font.FontSize = 12;
            //    //End - Observation Label

            //    //Start - Observation Value
            //    IShape observationShape = slide.AddTextBox(15, 290, 930, 220);

            //    IParagraph paragraph4 = observationShape.TextBody.AddParagraph();
            //    ITextPart textPart4 = paragraph4.AddTextPart();
            //    textPart4.Text = VJLiabraries.UtilityMethods.HtmlToText(discussionNote.DetailedObservation).Trim();
            //    textPart4.Font.FontName = "Arial";
            //    textPart4.Font.FontSize = 12;
            //    //End - Observation Value
            //    #endregion

            //    counter++;
            //}

            //if (presentation.Slides.Count > 0)
            //{
            //    var slideCounter = 1;

            //    foreach (var slide in presentation.Slides)
            //    {
            //        slide.HeadersFooters.Footer.Visible = true;
            //        slide.HeadersFooters.Footer.Text = auditModel.AuditName + ": Internal Audit (FY 2020-21)";
            //        //slide.HeadersFooters.SlideNumber.Text = "Page " + slideCounter.ToString();
            //        slide.HeadersFooters.SlideNumber.Visible = true;
            //        //slide.HeadersFooters.Footer.add = HorizontalAlignmentType.Left;

            //        slideCounter++;
            //    }
            //}

            //PdfDocument pdfDocument = PresentationToPdfConverter.Convert(presentation);

            //MemoryStream ms = new MemoryStream();
            //presentation.Save(ms);
            //ms.Position = 0;

            //pdfDocument.Close();
            #endregion

            #region SYSNCFUSIONEXPORTTOPDF
            //string strdatetime = System.DateTime.Now.ToString().Replace(" ", "_").Replace(":", "");
            //string filename = webRootPath + "\\ExportTemplates\\" + "DiscussionNotePresentation_" + strdatetime + ".html";
            //using (StreamWriter writer = new StreamWriter(filename))
            //{
            //    writer.Write(returnBuilder);
            //}

            //HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
            //WebKitConverterSettings webKitConverterSettings = new WebKitConverterSettings();
            //webKitConverterSettings.WebKitPath = Path.Combine(webRootPath, "QtBinariesWindows");
            //webKitConverterSettings.Orientation = PdfPageOrientation.Landscape;
            //webKitConverterSettings.Margin.Top = 35;
            //webKitConverterSettings.Margin.Bottom = 35;
            ////webKitConverterSettings.PdfHeader = AddPdfHeader(webKitConverterSettings.PdfPageSize.Width, "Syncfusion Essential PDF", " ");
            ////webKitConverterSettings.PdfFooter = AddPdfFooter(webKitConverterSettings.PdfPageSize.Width, "@Copyright 2015");
            //htmlToPdfConverter.ConverterSettings = webKitConverterSettings;
            //PdfDocument pdfDocument = htmlToPdfConverter.Convert(filename);
            //var memoryStream = new MemoryStream();
            //pdfDocument.Save(memoryStream);
            //pdfDocument.Close();
            //memoryStream.Position = 0;
            //_IDocumentUpload.DeleteFullPath(filename);
            //return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DiscussionNotes.pdf");
            #endregion

            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            //WkhtmltopdfConfiguration.RotativaPath = "C:\\Program Files\\wkhtmltopdf\\bin";
            //byte[] pdfbyte= _generatePdf.GetPDF(returnBuilder.ToString());
            StringBuilder returnBuilder = commonPDFPPT(tList);

            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;

            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DiscussionNotes.pdf");
        }
        public StringBuilder commonPDFPPT(IEnumerable<DiscussionNote> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            foreach (var item in tList)
            {
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
                item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
            }

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DiscussionNotePresentation.html");

            var _mainBody = new StringBuilder();
            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                _mainBody.Append(htmlContent);
            }

            var _riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var table = new StringBuilder();
            var returnBuilder = new StringBuilder();
            var counter = 1;
            foreach (var discussionNote in tList)
            {
                var DateTable = Path.Combine(webRootPath, "ExportTemplates", "DiscussionNote.html");
                using (StreamReader streamReaderDate = new StreamReader(DateTable))
                {
                    var htmlContentDate = streamReaderDate.ReadToEnd();
                    returnBuilder.Append(htmlContentDate);
                }

                var criticalColor = discussionNote.ObservationGrading == ObservationGradingEnum.Critical ? "red" : "white";
                var highcolor = discussionNote.ObservationGrading == ObservationGradingEnum.High ? "red" : "white";
                var lowcolor = discussionNote.ObservationGrading == ObservationGradingEnum.Low ? "red" : "white";
                var mediumcolor = discussionNote.ObservationGrading == ObservationGradingEnum.Medium ? "red" : "white";

                var _riskTypes = string.Empty;

                if (discussionNote.RiskTypeIds != null && discussionNote.RiskTypeIds.Length > 0)
                {
                    foreach (var _riskItem in discussionNote.RiskTypeIds)
                    {
                        var _riskType = _riskTypeRepo.GetByID(_riskItem);

                        if (_riskType != null)
                        {
                            _riskTypes += _riskType.Name + ", ";
                        }
                    }
                    _riskTypes = _riskTypes.Trim().TrimEnd(',');
                }
                returnBuilder = returnBuilder
                    .Replace("#DiscussionNo#", counter.ToString())
                    .Replace("#DiscussionNoteHeading#", discussionNote.ObservationHeading)
                    .Replace("#DiscussionNoteBackground#", discussionNote.FieldBackground)
                    .Replace("#CriticalRiskRating#", criticalColor).Replace("#HighRiskRating#", highcolor).Replace("#MediumRiskRating#", mediumcolor).Replace("#LowRiskRating#", lowcolor)
                    .Replace("#ObservationDetail#", discussionNote.DetailedObservation)
                    .Replace("#Recommendation#", discussionNote.Recommendation)
                    .Replace("#RootCause#", discussionNote.RootCause)
                    .Replace("#RiskBusinessImpact#", discussionNote.Risks)
                    .Replace("#RiskTypes#", _riskTypes)
                    .Replace("#ManagementResponses#", discussionNote.ManagementComments);
                counter++;
            }
            _mainBody = _mainBody.Replace("#DateTable#", returnBuilder.ToString());

            return _mainBody;
        }
        [HttpGet("downloadppt/{id}")]
        public IActionResult DownloadPPT(string id)
        {
            var tList = _api.GetWithInclude<User>(x => x.AuditId == id);
            if (tList == null)
                return ResponseNotFound();

            StringBuilder returnBuilder = commonPDFPPT(tList);
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            var folderName = Path.Combine(wkhtmlexepath);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            var fullPath = Path.Combine(pathToSave, "DiscussionNotes" + myUniqueFileName + ".pdf");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(pdfbyte, 0, pdfbyte.Length);
            }

            Document pdfDocument = new Document(fullPath);

            PptxSaveOptions pptxOptions = new PptxSaveOptions();
            pdfDocument.Save(memoryStream, pptxOptions.SaveFormat);
            byte[] pptBytes = memoryStream.ToArray();
            return File(pptBytes, "application/octet-stream", "DiscussionNotes.pptx");
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

        [HttpDelete("removefile/{id}/{userId}")]
        public ActionResult RemoveUploadedFile(string id, string userId)
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
                string newPath = Path.Combine("manageaudits", auditId, "discussionnote", id);

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var auditFiles = repo.GetMany(a => a.AuditId == auditId && a.ModuleId == id && a.ModuleName == "discussionnote");

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

        //// Method to add single cell to the Header  
        //private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        //{
        //    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1)))
        //    {
        //        HorizontalAlignment = Element.ALIGN_LEFT,
        //        Padding = 3,
        //        //BackgroundColor = new BaseColor(128, 0, 0)
        //    });
        //}

        //// Method to add single cell to the body  
        //private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        //{
        //    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 0, BaseColor.BLACK)))
        //    {
        //        HorizontalAlignment = Element.ALIGN_LEFT,
        //        Padding = 3,
        //        //BackgroundColor = new BaseColor(255, 255, 255)
        //    });
        //}

        [HttpGet("getDiscussNoteHistory/{id}")]
        public ActionResult GetDiscussionNoteHistory(string id)
        {
            var repohistory = new MongoGenericRepository<DiscussionNoteHistory>(_dbsetting);
            var tList = repohistory.GetWithInclude<DiscussionNote, User>(p => p.DiscussionNoteID == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        public FileContentResult DownloadExcelAttachment(DiscussionNote tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);

            //foreach (var item in tList)
            //{
            //    item.Reviewer = userRepo.GetByID(item.ReviewerId);
            //    item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
            //}
            if (tList.ReviewerId != null)
            {
                var objReviewer = userRepo.GetFirst(p => p.Id == tList.ReviewerId);
                if (objReviewer != null)
                {
                    tList.Reviewer = objReviewer;
                }
            }
            if (tList.PersonResponsibleID != null)
            {
                var objResponsible = userRepo.GetFirst(p => p.Id == tList.PersonResponsibleID);
                if (objResponsible != null)
                {
                    tList.ResponsiblePerson = objResponsible;
                }
            }
            if (tList.RiskTypeId != null)
            {
                var objRiskType = risktypeRepo.GetFirst(p => p.Id == tList.RiskTypeId);
                if (objRiskType != null)
                {
                    tList.RiskType = objRiskType;
                }
            }
            var fileName = "DiscussionNotes.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "RACM Number";
                worksheet.Cells["B1"].Value = "Discussion No";
                worksheet.Cells["C1"].Value = "Discussion Heading";
                worksheet.Cells["D1"].Value = "Background";
                worksheet.Cells["E1"].Value = "Detailed Observation";
                worksheet.Cells["F1"].Value = "Root Cause";
                worksheet.Cells["G1"].Value = "Risks / Business Impact";
                worksheet.Cells["H1"].Value = "Risk Type";
                worksheet.Cells["I1"].Value = "Responsibility";
                worksheet.Cells["J1"].Value = "Reviewer";
                worksheet.Cells["K1"].Value = "Observation Grading";
                worksheet.Cells["L1"].Value = "Checkbox for flagging issue to be considered in report";
                worksheet.Cells["M1"].Value = "Management Comments";
                worksheet.Cells["N1"].Value = "Repeat";

                var rowIndex = 2;

                worksheet.Cells["A" + rowIndex.ToString()].Value = tList.RACM_Ids != null ? getRACM_Ids(tList.RACM_Ids) : "";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.DiscussionNumber;
                worksheet.Cells["C" + rowIndex.ToString()].Value = tList.ObservationHeading;
                worksheet.Cells["D" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.FieldBackground);
                worksheet.Cells["E" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.DetailedObservation);
                worksheet.Cells["F" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.RootCause);
                worksheet.Cells["G" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.Risks);
                worksheet.Cells["H" + rowIndex.ToString()].Value = tList.RiskType != null ? tList.RiskType.Name : "";
                worksheet.Cells["I" + rowIndex.ToString()].Value = tList.ResponsiblePerson != null ? tList.ResponsiblePerson.FirstName + " " + tList.ResponsiblePerson.LastName : "";
                worksheet.Cells["J" + rowIndex.ToString()].Value = tList.Reviewer != null ? tList.Reviewer.FirstName + " " + tList.Reviewer.LastName : "";
                worksheet.Cells["K" + rowIndex.ToString()].Value = tList.ObservationGrading;
                worksheet.Cells["L" + rowIndex.ToString()].Value = tList.FlagIssueForReport ? "Yes" : "No";
                worksheet.Cells["M" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.ManagementComments);
                worksheet.Cells["N" + rowIndex.ToString()].Value = tList.IsRepeat ? "Yes" : "No";
                rowIndex++;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;

                package.Save();
            }
            byte[] bytes = memoryStream.ToArray();

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public string getRACM_Ids(List<string> lstRacmID)
        {
            List<string> lstID = new List<string>();
            foreach (var racmID in lstRacmID)
            {
                lstID.Add(racmID);
            }
            String[] Id = lstID.ToArray();
            var str = String.Join(",", Id);
            return str;
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
                var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
                var repoRootCause = new MongoGenericRepository<RootCause>(_dbsetting);
                var repoImpactMaster = new MongoGenericRepository<ImpactMaster>(_dbsetting);
                var repoRecommendation = new MongoGenericRepository<Recommendation>(_dbsetting);

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
                                var DiscussionNumber = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().Trim() : null;

                                if (DiscussionNumber != null)
                                {
                                    var isExists = true;

                                    var exists = _api.GetFirst(a => a.DiscussionNumber.ToLower() == DiscussionNumber.ToLower());

                                    if (exists == null)
                                    {
                                        isExists = false;
                                        exists = new DiscussionNote();
                                    }

                                    var racmNumber = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : "";

                                    if (racmNumber != null)
                                    {
                                        List<string> racmnum = new List<string>();
                                        racmnum.Add(racmNumber);
                                        exists.RACM_Ids = racmnum;
                                    }

                                    exists.DiscussionNumber = DiscussionNumber;
                                    exists.ObservationHeading = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : "";
                                    exists.FieldBackground = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : "";
                                    exists.DetailedObservation = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : "";
                                    exists.Recommendation = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : "";
                                    exists.RootCause = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : "";
                                    exists.Risks = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : "";

                                    var riskTypesSource = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : "";
                                    var _riskTypeIds = new List<string>();

                                    if (riskTypesSource != "")
                                    {
                                        try
                                        {
                                            foreach (var riskItem in riskTypesSource.Split(','))
                                            {
                                                var risktype = riskTypeRepo.GetFirst(a => a.Name.Trim().ToLower() == riskItem.Trim().ToLower());

                                                if (risktype != null)
                                                    _riskTypeIds.Add(risktype.Id);
                                            }
                                        }
                                        catch (Exception ex) { }
                                    }

                                    exists.RiskTypeIds = _riskTypeIds.ToArray();

                                    var _responsobility = worksheet.Cells[row, 10].Value != null ? worksheet.Cells[row, 10].Value.ToString().Trim() : "";
                                    exists.PersonResponsibleID = GetUserIdByName(_responsobility);

                                    var _reviewer = worksheet.Cells[row, 11].Value != null ? worksheet.Cells[row, 11].Value.ToString().Trim() : "";
                                    exists.ReviewerId = GetUserIdByName(_reviewer);

                                    var observationGradint = worksheet.Cells[row, 12].Value != null ? worksheet.Cells[row, 12].Value.ToString().Trim() : "";

                                    if (observationGradint != "")
                                    {
                                        switch (observationGradint.ToLower())
                                        {
                                            case "high":
                                                exists.ObservationGrading = ObservationGradingEnum.High; break;
                                            case "medium":
                                                exists.ObservationGrading = ObservationGradingEnum.Medium; break;
                                            case "low":
                                                exists.ObservationGrading = ObservationGradingEnum.Low; break;
                                            case "critical":
                                                exists.ObservationGrading = ObservationGradingEnum.Critical; break;
                                            default:
                                                exists.ObservationGrading = ObservationGradingEnum.Repeat; break;
                                        }
                                    }

                                    var flagIssueForReport = worksheet.Cells[row, 13].Value != null ? worksheet.Cells[row, 13].Value.ToString().Trim().ToLower() : "";
                                    exists.FlagIssueForReport = flagIssueForReport == "yes" ? true : false;

                                    exists.ManagementComments = worksheet.Cells[row, 14].Value != null ? worksheet.Cells[row, 14].Value.ToString().Trim().ToLower() : "";

                                    //var _repeat = worksheet.Cells[row, 15].Value != null ? worksheet.Cells[row, 15].Value.ToString().Trim().ToLower() : "";
                                    //exists.IsRepeat = _repeat.ToLower() == "yes" ? true : false;

                                    //var _systemImprovement = worksheet.Cells[row, 16].Value != null ? worksheet.Cells[row, 16].Value.ToString().Trim().ToLower() : "";
                                    //exists.isSystemImprovement = _systemImprovement.ToLower() == "yes" ? true : false;

                                    //var _redFlag = worksheet.Cells[row, 17].Value != null ? worksheet.Cells[row, 17].Value.ToString().Trim().ToLower() : "";
                                    //exists.isRedFlag = _redFlag.ToLower() == "yes" ? true : false;

                                    //var _leadingPractices = worksheet.Cells[row, 18].Value != null ? worksheet.Cells[row, 18].Value.ToString().Trim().ToLower() : "";
                                    //exists.isLeadingPractices = _leadingPractices.ToLower() == "yes" ? true : false;

                                    //var _potentialSaving = worksheet.Cells[row, 19].Value != null ? worksheet.Cells[row, 19].Value.ToString().Trim().ToLower() : "0";
                                    //exists.PotentialSaving = _potentialSaving;

                                    //var _realisedSaving = worksheet.Cells[row, 20].Value != null ? worksheet.Cells[row, 20].Value.ToString().Trim().ToLower() : "0";
                                    //exists.RealisedSaving = _realisedSaving;

                                    //var _leakage = worksheet.Cells[row, 21].Value != null ? worksheet.Cells[row, 21].Value.ToString().Trim().ToLower() : "0";
                                    //exists.Leakage = _leakage;

                                    //var _impact = worksheet.Cells[row, 22].Value != null ? worksheet.Cells[row, 22].Value.ToString().Trim().ToLower() : "";
                                    //List<string> lstrepoImpactMaster = new List<string>();

                                    //if (_impact != null)
                                    //{
                                    //    string[] cause = _impact.Split(",");
                                    //    foreach (var item in cause)
                                    //    {
                                    //        var objImpactMaster = repoImpactMaster.GetFirst(p => p.Name.ToLower() == item);
                                    //        if (objImpactMaster != null)
                                    //        {
                                    //            lstrepoImpactMaster.Add(objImpactMaster.Id);
                                    //        }
                                    //    }
                                    //    exists.Impacts = lstrepoImpactMaster;
                                    //}
                                    //var _recommendation = worksheet.Cells[row, 23].Value != null ? worksheet.Cells[row, 23].Value.ToString().Trim().ToLower() : "";
                                    //List<string> lstrecommendation = new List<string>();

                                    //if (_recommendation != null)
                                    //{
                                    //    string[] cause = _impact.Split(",");
                                    //    foreach (var item in cause)
                                    //    {
                                    //        var objrecommendation = repoRecommendation.GetFirst(p => p.Name.ToLower() == item);
                                    //        if (objrecommendation != null)
                                    //        {
                                    //            lstrecommendation.Add(objrecommendation.Id);
                                    //        }
                                    //    }
                                    //    exists.Recommendations = lstrecommendation;
                                    //}
                                    //var _rootCause = worksheet.Cells[row, 24].Value != null ? worksheet.Cells[row, 24].Value.ToString().Trim().ToLower() : "";
                                    //List<string> lstrootCause = new List<string>();

                                    //if (_rootCause != null)
                                    //{
                                    //    string[] cause = _rootCause.Split(",");
                                    //    foreach (var item in cause)
                                    //    {
                                    //        var objImpactMaster = repoImpactMaster.GetFirst(p => p.Name.ToLower() == item);
                                    //        if (objImpactMaster != null)
                                    //        {
                                    //            lstrootCause.Add(objImpactMaster.Id);
                                    //        }
                                    //    }
                                    //    exists.RootCauses = lstrootCause;
                                    //}
                                    exists.AuditId = id;
                                    if (isExists)
                                    {
                                        exists.UpdatedBy = userid;
                                        _api.Update(exists);
                                    }
                                    else
                                    {
                                        exists.CreatedBy = userid;
                                        _api.Insert(exists);

                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;

                                sb.Append(row + ",");

                                _CommonServices.SendExcepToDB(e, "DiscussionNote/ImportExcel()");
                            }
                        }
                    }
                }

                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "DiscussionNote/ImportExcel()");
            }

            return ResponseOK(new object[0]);
        }

        private List<RiskType> GetRiskTypesList(string[] _riskTypeIds)
        {
            var list = new List<RiskType>();

            if (_riskTypeIds != null && _riskTypeIds.Length > 0)
            {
                var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);

                foreach (var _riskType in _riskTypeIds)
                {
                    var _rs = riskTypeRepo.GetByID(_riskType);

                    if (_rs != null)
                        list.Add(_rs);
                }
            }

            return list;
        }

        private List<AuditFiles> GetAllFilesList(string _auditId, string _discussionNoteId)
        {
            var returnFiles = new List<AuditFiles>();

            var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

            var auditFiles = repo.GetMany(a => a.AuditId == _auditId && a.ModuleId == _discussionNoteId && a.ModuleName == "discussionnote");

            foreach (var file in auditFiles)
            {
                if (System.IO.File.Exists(file.UploadedFileName))
                    returnFiles.Add(file);
            }

            return returnFiles;
        }

        private string GetUserIdByName(string _userName)
        {
            try
            {
                if (_userName != "")
                {
                    var userRepo = new MongoGenericRepository<User>(_dbsetting);
                    var _users = userRepo.GetAll();

                    var _user = _users.Where
                        (a =>
                        ((a.FirstName != null && a.FirstName != "" ? a.FirstName.Trim().ToLower() + " " : "") +
                        (a.MiddleName != null && a.MiddleName != "" ? a.MiddleName.Trim().ToLower() + " " : "") +
                        (a.LastName != null && a.LastName != "" ? a.LastName.Trim().ToLower() : "")) == _userName.Trim().ToLower()).FirstOrDefault();

                    if (_user != null)
                        return _user.Id;
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Address = $C$2
        //private void ScriptForAllowMultiSelectInExcel(ExcelPackage package, ExcelWorksheet workSheet, string cellAddress)
        //{
        //    var _sbMultiSelectScript = new StringBuilder();
        //    _sbMultiSelectScript.Append(
        //        "Private Sub Worksheet_Change(ByVal Target As Range)\n" +
        //        "   'Code by Sumit Bansal from https://trumpexcel.com\n" +
        //        "   ' To make mutliple selections in a Drop Down List in Excel\n" +
        //        "   Dim Oldvalue As String\n" +
        //        "   Dim Newvalue As String\n" +
        //        "   On Error GoTo Exitsub\n" +
        //        "   If Target.Address = \"$I$2\" Then\n" +
        //        "       If Target.SpecialCells(xlCellTypeAllValidation) Is Nothing Then\n" +
        //        "           GoTo Exitsub\n" +
        //        "       Else: If Target.Value = \"\" Then GoTo Exitsub Else\n" +
        //        "           Application.EnableEvents = False\n" +
        //        "           Newvalue = Target.Value\n" +
        //        "           Application.Undo\n" +
        //        "           Oldvalue = Target.Value\n" +
        //        "           If Oldvalue = \"\" Then\n" +
        //        "               Target.Value = Newvalue\n" +
        //        "           Else\n" +
        //        "               Target.Value = Oldvalue & \", \" & Newvalue\n" +
        //        "           End If\n" +
        //        "       End If\n" +
        //        "   End If\n" +
        //        "   Application.EnableEvents = True\n" +
        //        "   Exitsub:\n" +
        //        "   Application.EnableEvents = True\n" +
        //        "   End Sub");

        //    package.Workbook.CreateVBAProject();
        //    workSheet.CodeModule.Code = _sbMultiSelectScript.ToString();
        //    //package.Save();
        //}
        private void ScriptForAllowMultiSelectInExcel(ExcelPackage package, ExcelWorksheet workSheet, string cellAddress)
        {
            var _sbMultiSelectScript = new StringBuilder();
            _sbMultiSelectScript.Append(
                "Private Sub Worksheet_Change(ByVal Target As Range)\n" +
                "   'Code by Sumit Bansal from https://trumpexcel.com\n" +
                "   ' To make mutliple selections in a Drop Down List in Excel\n" +
                "   Dim Oldvalue As String\n" +
                "   Dim Newvalue As String\n" +
                "   On Error GoTo Exitsub\n" +
                "   If Target.Column  = \"9\" Then\n" +
                "       If Target.SpecialCells(xlCellTypeAllValidation) Is Nothing Then\n" +
                "           GoTo Exitsub\n" +
                "       Else: If Target.Value = \"\" Then GoTo Exitsub Else\n" +
                "           Application.EnableEvents = False\n" +
                "           Newvalue = Target.Value\n" +
                "           Application.Undo\n" +
                "           Oldvalue = Target.Value\n" +
                "           If Oldvalue = \"\" Then\n" +
                "               Target.Value = Newvalue\n" +
                "           Else\n" +
                "               Target.Value = Oldvalue & \", \" & Newvalue\n" +
                "           End If\n" +
                "       End If\n" +
                "   End If\n" +
                "   Application.EnableEvents = True\n" +
                "   Exitsub:\n" +
                "   Application.EnableEvents = True\n" +
                "   End Sub");

            package.Workbook.CreateVBAProject();
            workSheet.CodeModule.Code = _sbMultiSelectScript.ToString();
            //package.Save();
        }
    }

    public class DNStatusUpdate : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Status { get; set; }
        public string Justification { get; set; }
        public string DiscussionComments { get; set; }
    }
}