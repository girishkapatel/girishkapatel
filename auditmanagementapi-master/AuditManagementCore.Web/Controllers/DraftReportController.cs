using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DraftReportController : VJBaseGenericAPIController<DraftReport>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;

        IWebHostEnvironment _IWebHostEnvironment;
        CommonServices _CommonServices;
        IDocumentUpload _IDocumentUpload;
        IEmailUtility _IEmailUtility;
        #endregion

        public DraftReportController
            (IMongoGenericRepository<DraftReport> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IDocumentUpload documentUpload,
            IEmailUtility emailUtility, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _CommonServices = cs;
            _IDocumentUpload = documentUpload;
            _IEmailUtility = emailUtility;
        }

        public override ActionResult Post([FromBody] DraftReport e)
        {
            var isExist = _api.Exists(x => x.DiscussionNoteID.ToLower() == e.DiscussionNoteID.ToLower());

            if (isExist)
                return ResponseError(" Discussion note already exists.");

            if (e.ActionPlans != null && e.ActionPlans.Count() > 0)
            {
                foreach (var item in e.ActionPlans)
                {
                    item.ImplementationStartDate = item.ImplementationStartDate?.ToLocalTime();
                    item.ImplementationEndDate = item.ImplementationEndDate?.ToLocalTime();
                }
            }
            var draftReport = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.ObservationNumber, "DraftReport", "Manage Audits | Reporting | DraftReport | Add", "Added DraftReport");
            return draftReport;
        }

        [HttpPost("SaveAsDraft")]
        public ActionResult SaveAsDraft(DraftReport dr)
        {
            var isExist = _api.Exists(x => x.DiscussionNoteID.ToLower() == dr.DiscussionNoteID.ToLower() && x.AuditId == dr.AuditId);
            if (isExist)
            {
                return ResponseError(" Draft already exists with same Discussion note and Audit ID.");
            }
            _CommonServices.SaveHistoryforDiscussionNote(dr.CreatedBy, AuditConstants.CommonStatus.SAVETODRAFT, dr.DiscussionNoteID);
            var draftReport = base.Post(dr);
            //Activity Log
            var repo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var objDiscussionNote = repo.GetFirst(x => x.Id == dr.DiscussionNoteID);
            var DiscussionNumber = objDiscussionNote.DiscussionNumber != null ? objDiscussionNote.DiscussionNumber : "";
            _CommonServices.ActivityLog(dr.CreatedBy, dr.Id, DiscussionNumber, "DraftReport", "Manage Audits | Reporting | DraftReport | Add", "SaveAsDraft");
            return draftReport;
        }

        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.Id == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        [HttpGet("GetByAudit/{id}/{grading}/{status}")]
        public ActionResult GetByAudit(string id, string grading, string status)
        {
            var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.AuditId == id);

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
                tList = tList.Where(a => a.Status != null && a.Status.ToLower().Trim() == status.ToLower().Trim());

            if (tList == null)
                return ResponseNotFound();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);

            foreach (var item in tList)
            {
                if (item.DiscussionNoteID != null)
                {
                    var objdiscussionNote = _repoDiscussionNote.GetFirst(p => p.Id == item.DiscussionNoteID);
                    item.DiscussionNote = objdiscussionNote != null ? objdiscussionNote : null;

                    if (item.DiscussionNote != null)
                    {
                        if (item.DiscussionNote.ReviewerId != null)
                            item.DiscussionNote.Reviewer = userRepo.GetByID(item.DiscussionNote.ReviewerId);

                        if (item.DiscussionNote.PersonResponsibleID != null)
                            item.DiscussionNote.ResponsiblePerson = userRepo.GetByID(item.DiscussionNote.PersonResponsibleID);

                        if (item.DiscussionNote.RiskTypeIds != null)
                            item.DiscussionNote.RiskTypes = GetRiskTypesList(item.DiscussionNote.RiskTypeIds);
                    }
                }
            }
            return ResponseOK(tList);
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
        [HttpGet("GetFinal/{id}/{grading}/{status}")]
        public ActionResult GetFinal(string id, string grading, string status)
        {
            var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.AuditId == id && x.Status.ToUpper() == "COMPLETED");

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
                tList = tList.Where(a => a.Status != null && a.Status.ToLower().Trim() == status.ToLower().Trim());

            if (tList == null)
                return ResponseNotFound();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            foreach (var item in tList)
            {
                if (item.DiscussionNoteID != null)
                {
                    var objdiscussionNote = _repoDiscussionNote.GetFirst(p => p.Id == item.DiscussionNoteID);
                    item.DiscussionNote = objdiscussionNote != null ? objdiscussionNote : null;
                    if (item.DiscussionNote != null)
                    {
                        if (item.DiscussionNote.ReviewerId != null)
                            item.DiscussionNote.Reviewer = userRepo.GetByID(item.DiscussionNote.ReviewerId);

                        if (item.DiscussionNote.PersonResponsibleID != null)
                            item.DiscussionNote.ResponsiblePerson = userRepo.GetByID(item.DiscussionNote.PersonResponsibleID);

                        if (item.DiscussionNote.RiskTypeId != null)
                            item.DiscussionNote.RiskType = riskTypeRepo.GetByID(item.DiscussionNote.RiskTypeId);
                    }
                }
            }
            return ResponseOK(tList);
        }
        [HttpGet("GetFinaldownloadpdf/{id}")]
        public IActionResult GetFinaldownloadpdf(string id)
        {
            try
            {
                var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.AuditId == id && x.Status.ToUpper() == "COMPLETED");

                if (tList == null)
                    return ResponseNotFound();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);

                foreach (var item in tList)
                {
                    if (item.DiscussionNoteID != null)
                    {
                        var objDiscussionNote = _repoDiscussionNote.GetFirst(p => p.Id == item.DiscussionNoteID);
                        if (objDiscussionNote != null)
                        {
                            item.DiscussionNote = objDiscussionNote;
                            item.DiscussionNote.Reviewer = userRepo.GetByID(objDiscussionNote.ReviewerId);
                            item.DiscussionNote.ResponsiblePerson = userRepo.GetByID(objDiscussionNote.PersonResponsibleID);
                        }
                    }
                }

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var auditModel = auditRepo.GetByID(id);

                var rootCauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
                var rootCauses = rootCauseRepo.GetAll();

                var impactMasterRepo = new MongoGenericRepository<ImpactMaster>(_dbsetting);
                var impactMaster = impactMasterRepo.GetAll();

                var recommRepo = new MongoGenericRepository<Recommendation>(_dbsetting);
                var recommendations = recommRepo.GetAll();

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DraftReportPDF.html");

                var emailBody = new StringBuilder();

                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                var returnBuilder = new StringBuilder();
                var rootCausesString = new StringBuilder();
                var impactMasterString = new StringBuilder();
                var recommendationsString = new StringBuilder();

                var counter = 1;
                foreach (var item in tList)
                {
                    rootCausesString.Clear();
                    impactMasterString.Clear();
                    recommendationsString.Clear();

                    returnBuilder.Append(emailBody);

                    var criticalColor = item.ObservationGrading == ObservationGradingEnum.Critical ? "red" : "white";
                    var highcolor = item.ObservationGrading == ObservationGradingEnum.High ? "red" : "white";
                    var lowcolor = item.ObservationGrading == ObservationGradingEnum.Low ? "red" : "white";
                    var mediumcolor = item.ObservationGrading == ObservationGradingEnum.Medium ? "red" : "white";

                    var isRepeat = item.DiscussionNote.IsRepeat ? "Yes" : "No";
                    if (rootCauses != null && item.RootCauses != null)
                    {
                        foreach (var rcItem in rootCauses)
                        {
                            if (item.RootCauses.Contains(rcItem.Id))
                                rootCausesString.Append(
                                    string.Format(
                                        "<tr><td style='padding:5px;'>{0}</td>" +
                                        "<td style='width:25px; text-align:center; padding:5px;'>{1}</td></tr>", rcItem.Name, "<img src='https://image.freepik.com/free-icon/black-tick-vector_318-8478.jpg' style='width:23px; height:23px;' />"));
                            else
                                rootCausesString.Append(string.Format("<tr><td style='padding:5px;'>{0}</td><td style='width:25px; padding:5px;'></td></tr>", rcItem.Name));
                        }
                    }
                    if (impactMaster != null && item.Impacts != null)
                    {
                        foreach (var impactItem in impactMaster)
                        {
                            if (item.Impacts.Contains(impactItem.Id))
                                impactMasterString.Append(
                                    string.Format(
                                        "<tr><td style='padding:5px;'>{0}</td>" +
                                        "<td style='width:25px; text-align:center; padding:5px;'>{1}</td></tr>", impactItem.Name, "<img src='https://image.freepik.com/free-icon/black-tick-vector_318-8478.jpg' style='width:23px; height:23px;' />"));
                            else
                                impactMasterString.Append(string.Format("<tr><td style='padding:5px;'>{0}</td><td style='width:25px; padding:5px;'></td></tr>", impactItem.Name));
                        }
                    }
                    if (recommendations != null && item.Recommendations != null)
                    {
                        foreach (var recommItem in recommendations)
                        {
                            if (item.Recommendations.Contains(recommItem.Id))
                                recommendationsString.Append(
                                    string.Format(
                                        "<tr><td style='padding:5px;'>{0}</td>" +
                                        "<td style='width:25px; text-align:center; padding:5px;'>{1}</td></tr>", recommItem.Name, "<img src='https://image.freepik.com/free-icon/black-tick-vector_318-8478.jpg' style='width:23px; height:23px;' />"));
                            else
                                recommendationsString.Append(string.Format("<tr><td style='padding:5px;'>{0}</td><td style='width:25px; padding:5px;'></td></tr>", recommItem.Name));
                        }
                    }

                    returnBuilder = returnBuilder
                        .Replace("#Indexing#", counter.ToString())
                        .Replace("#DiscussionNoteHeading#", item.DiscussionNote.ObservationHeading)
                        .Replace("#isRepeat#", isRepeat)
                        .Replace("#RiskValue#", "0").Replace("#CriticalRiskRating#", criticalColor)
                        .Replace("#HighRiskRating#", highcolor).Replace("#MediumRiskRating#", mediumcolor).Replace("#LowRiskRating#", lowcolor)
                        .Replace("#DiscussionNoteBackground#", item.DiscussionNote.FieldBackground)
                        .Replace("#ObservationDetail#", item.DiscussionNote.DetailedObservation)
                        .Replace("#RootCauses#", item.DiscussionNote.RootCause)
                        .Replace("#RootCausesString#", rootCausesString.ToString())
                        .Replace("#BusinessImpacts#", item.DiscussionNote.FinancialImpact)
                        .Replace("#ImpactsString#", impactMasterString.ToString())
                        .Replace("#RecommendationsString#", recommendationsString.ToString())
                        .Replace("#Recommendations#", item.Recommendation)
                        .Replace("#ManagementResponses#", item.DiscussionNote.ManagementComments);

                    var implementationOwner = new List<string>();
                    var implementationTimeline = new List<string>();

                    if (item.ActionPlans != null)
                    {
                        foreach (var apItem in item.ActionPlans)
                        {
                            var timeline = apItem.ImplementationEndDate != null ? Convert.ToDateTime(apItem.ImplementationEndDate).ToString("MMMM dd, yyyy") : "";

                            var user = userRepo.GetByID(apItem.ProcessOwnerID);

                            if (user != null)
                            {
                                implementationOwner.Add(user.FirstName + " " + user.LastName);
                                implementationTimeline.Add(timeline);
                            }
                        }
                    }

                    returnBuilder = returnBuilder
                            .Replace("#ImplemenatationOwners#", string.Join("<br/>", implementationOwner))
                            .Replace("#ImplemenatationOwnerTimeline#", string.Join("<br/>", implementationTimeline));

                    counter++;
                }

                #region WKHTMLTOPDF
                string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
                if (returnBuilder.Length == 0)
                {
                    returnBuilder.Append(emailBody);
                    returnBuilder = returnBuilder
                        .Replace("#Indexing#", "")
                        .Replace("#DiscussionNoteHeading#", "")
                        .Replace("#isRepeat#", "")
                        .Replace("#RiskValue#", "0").Replace("#CriticalRiskRating#", "")
                        .Replace("#HighRiskRating#", "").Replace("#MediumRiskRating#", "").Replace("#LowRiskRating#", "")
                        .Replace("#DiscussionNoteBackground#", "")
                        .Replace("#ObservationDetail#", "")
                        .Replace("#RootCauses#", "")
                        .Replace("#RootCausesString#", "")
                        .Replace("#BusinessImpacts#", "")
                        .Replace("#ImpactsString#", "")
                        .Replace("#RecommendationsString#", "")
                        .Replace("#Recommendations#", "")
                        .Replace("#ManagementResponses#", "")
                        .Replace("#ImplemenatationOwners#", "")
                        .Replace("#ImplemenatationOwnerTimeline#", "");
                }

                byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", returnBuilder.ToString());

                var memoryStream = new MemoryStream(pdfbyte);
                memoryStream.Position = 0;

                return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DraftReport.pdf");
            }
            catch (Exception e)
            {
                throw;
            }
            #endregion
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<DiscussionNote>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                item.ProcessOwner = userRepo.GetByID(item.ProcessOwnerID);
            }
            return ResponseOK(tList);
        }

        public override ActionResult Put([FromBody] DraftReport a)
        {

            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var DraftObj = _api.GetByID(a.Id);

            if (DraftObj != null)
            {
                if (a.ActionPlans != null && a.ActionPlans.Count > 0)
                {
                    foreach (var item in a.ActionPlans)
                    {
                        item.ImplementationStartDate = item.ImplementationStartDate?.ToLocalTime();
                        item.ImplementationEndDate = item.ImplementationEndDate?.ToLocalTime();
                    }
                }

                _api.Update(a);

                //Activity Log
                _CommonServices.ActivityLog(a.UpdatedBy, a.Id, a.ObservationNumber, "DraftReport", "Manage Audits | Reporting | DraftReport | Edit", "Updated DraftReport");

                return Ok(a);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("UpdateAuditReportStatus")]
        public ActionResult UpdateAuditReportStatus(DraftReportStatusUpdate draftReportStatusUpdate)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var DraftObj = _api.GetByID(draftReportStatusUpdate.Id);

            if (DraftObj != null)
            {
                if (!String.IsNullOrEmpty(draftReportStatusUpdate.Status) && !String.IsNullOrWhiteSpace(draftReportStatusUpdate.Status))
                {
                    switch (draftReportStatusUpdate.Status.ToUpper())
                    {
                        case "PENDING": DraftObj.Status = AuditConstants.Status.PENDING; break;
                        case "INPROGRESS": DraftObj.Status = AuditConstants.Status.INPROGRESS; break;
                        case "COMPLETED": DraftObj.Status = AuditConstants.Status.COMPLETED; break;
                        case "FINAL": DraftObj.Status = AuditConstants.Status.FINAL; break;
                        case "APPROVED": DraftObj.Status = "APPROVED"; break;
                        default: DraftObj.Status = AuditConstants.Status.DRAFT; break;
                    }
                }

                if (!String.IsNullOrEmpty(draftReportStatusUpdate.Justification) && !String.IsNullOrWhiteSpace(draftReportStatusUpdate.Justification))
                    DraftObj.Justification = draftReportStatusUpdate.Justification;

                if (!String.IsNullOrEmpty(draftReportStatusUpdate.ManagementComments) && !String.IsNullOrWhiteSpace(draftReportStatusUpdate.ManagementComments))
                    DraftObj.ManagementComments = draftReportStatusUpdate.ManagementComments;

                if (!String.IsNullOrEmpty(draftReportStatusUpdate.ImplementationDate) && !String.IsNullOrWhiteSpace(draftReportStatusUpdate.ImplementationDate))
                    DraftObj.ImplementationStartDate = Convert.ToDateTime(draftReportStatusUpdate.ImplementationDate);

                DraftObj.UpdatedBy = draftReportStatusUpdate.UpdatedBy;

                _api.Update(DraftObj);
                //Activity Log
                string draftstatus = "";
                switch (DraftObj.Status)
                {
                    case "INPROGRESS": draftstatus = "REJECT"; break;
                    case "COMPLETED": draftstatus = "SAVE FINAL REPORT"; break;
                    default: draftstatus = DraftObj.Status; break;
                }
                _CommonServices.ActivityLog(draftReportStatusUpdate.UpdatedBy, draftReportStatusUpdate.Id, DraftObj.ObservationNumber, "DraftReport", "Manage Audits | Reporting | DraftReport | Edit", "DraftReport " + draftstatus);
                _CommonServices.SaveHistoryForDraftReport(draftReportStatusUpdate.UpdatedBy, DraftObj.Status, DraftObj.Id);
                #region Send email
                if (DraftObj.Status.ToLower() == "pending")
                {
                    var toEmails = new List<string>();
                    var processOwners = new List<string>();

                    var userRepo = new MongoGenericRepository<User>(_dbsetting);
                    if (DraftObj.ActionPlans != null && DraftObj.ActionPlans.Count > 0)
                    {
                        foreach (var item in DraftObj.ActionPlans)
                        {
                            var user = userRepo.GetByID(item.ProcessOwnerID);

                            if (user != null)
                            {
                                processOwners.Add(user.FirstName + " " + user.LastName);
                                toEmails.Add(user.EmailId);
                            }
                        }
                    }

                    var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                    var auditModel = auditRepo.GetByID(DraftObj.AuditId);

                    var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                    var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                    var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                    var webRootPath = _IWebHostEnvironment.WebRootPath;
                    var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "DraftReportSendForApproval.html");

                    var emailBody = new StringBuilder();
                    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                    {
                        var htmlContent = streamReader.ReadToEnd();
                        emailBody.Append(htmlContent);
                    }

                    emailBody = emailBody
                        .Replace("#ProcessOwnerName#", string.Join(",", processOwners).Replace(",", ", "))
                        .Replace("#AuditName#", auditModel.AuditName)
                        .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));

                    var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                    var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                    var auditPeriod = auditStartDate + " to " + auditEndDate;

                    var emailModel = new Service.Utilities.EmailModel()
                    {
                        ToEmail = toEmails,
                        Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | DRAFT Report",
                        MailBody = emailBody.ToString()
                    };

                    _IEmailUtility.SendEmail(emailModel);
                }
                #endregion
                return Ok(DraftObj);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("downloadpdf/{id}")]
        public IActionResult DownloadPDF(string id)
        {
            var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);

            foreach (var item in tList)
            {
                if (item.DiscussionNoteID != null)
                {
                    var objDiscussionNote = _repoDiscussionNote.GetFirst(p => p.Id == item.DiscussionNoteID);
                    if (objDiscussionNote != null)
                    {
                        item.DiscussionNote = objDiscussionNote;
                        item.DiscussionNote.Reviewer = userRepo.GetByID(objDiscussionNote.ReviewerId);
                        item.DiscussionNote.ResponsiblePerson = userRepo.GetByID(objDiscussionNote.PersonResponsibleID);
                    }
                }
            }

            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var auditModel = auditRepo.GetByID(id);

            var rootCauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
            var rootCauses = rootCauseRepo.GetAll();

            var impactMasterRepo = new MongoGenericRepository<ImpactMaster>(_dbsetting);
            var impactMaster = impactMasterRepo.GetAll();

            var recommRepo = new MongoGenericRepository<Recommendation>(_dbsetting);
            var recommendations = recommRepo.GetAll();

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "DraftReportPDF.html");

            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }

            var returnBuilder = new StringBuilder();
            var rootCausesString = new StringBuilder();
            var impactMasterString = new StringBuilder();
            var recommendationsString = new StringBuilder();

            var counter = 1;
            foreach (var item in tList)
            {
                rootCausesString.Clear();
                impactMasterString.Clear();
                recommendationsString.Clear();

                returnBuilder.Append(emailBody);

                var criticalColor = item.ObservationGrading == ObservationGradingEnum.Critical ? "red" : "white";
                var highcolor = item.ObservationGrading == ObservationGradingEnum.High ? "red" : "white";
                var lowcolor = item.ObservationGrading == ObservationGradingEnum.Low ? "red" : "white";
                var mediumcolor = item.ObservationGrading == ObservationGradingEnum.Medium ? "red" : "white";

                var isRepeat = item.DiscussionNote.IsRepeat ? "Yes" : "No";
                if (rootCauses != null && item.RootCauses != null)
                {
                    foreach (var rcItem in rootCauses)
                    {
                        if (item.RootCauses.Contains(rcItem.Id))
                            rootCausesString.Append(
                                string.Format(
                                    "<tr><td style='padding:5px;'>{0}</td>" +
                                    "<td style='width:25px; text-align:center; padding:5px;'>{1}</td></tr>", rcItem.Name, "<img src='https://image.freepik.com/free-icon/black-tick-vector_318-8478.jpg' style='width:23px; height:23px;' />"));
                        else
                            rootCausesString.Append(string.Format("<tr><td style='padding:5px;'>{0}</td><td style='width:25px; padding:5px;'></td></tr>", rcItem.Name));
                    }
                }
                if (impactMaster != null && item.Impacts != null)
                {
                    foreach (var impactItem in impactMaster)
                    {
                        if (item.Impacts.Contains(impactItem.Id))
                            impactMasterString.Append(
                                string.Format(
                                    "<tr><td style='padding:5px;'>{0}</td>" +
                                    "<td style='width:25px; text-align:center; padding:5px;'>{1}</td></tr>", impactItem.Name, "<img src='https://image.freepik.com/free-icon/black-tick-vector_318-8478.jpg' style='width:23px; height:23px;' />"));
                        else
                            impactMasterString.Append(string.Format("<tr><td style='padding:5px;'>{0}</td><td style='width:25px; padding:5px;'></td></tr>", impactItem.Name));
                    }
                }
                if (recommendations != null && item.Recommendations != null)
                {
                    foreach (var recommItem in recommendations)
                    {
                        if (item.Recommendations.Contains(recommItem.Id))
                            recommendationsString.Append(
                                string.Format(
                                    "<tr><td style='padding:5px;'>{0}</td>" +
                                    "<td style='width:25px; text-align:center; padding:5px;'>{1}</td></tr>", recommItem.Name, "<img src='https://image.freepik.com/free-icon/black-tick-vector_318-8478.jpg' style='width:23px; height:23px;' />"));
                        else
                            recommendationsString.Append(string.Format("<tr><td style='padding:5px;'>{0}</td><td style='width:25px; padding:5px;'></td></tr>", recommItem.Name));
                    }
                }

                returnBuilder = returnBuilder
                    .Replace("#Indexing#", counter.ToString())
                    .Replace("#DiscussionNoteHeading#", item.DiscussionNote.ObservationHeading)
                    .Replace("#isRepeat#", isRepeat)
                    .Replace("#RiskValue#", "0").Replace("#CriticalRiskRating#", criticalColor)
                    .Replace("#HighRiskRating#", highcolor).Replace("#MediumRiskRating#", mediumcolor).Replace("#LowRiskRating#", lowcolor)
                    .Replace("#DiscussionNoteBackground#", item.DiscussionNote.FieldBackground)
                    .Replace("#ObservationDetail#", item.DiscussionNote.DetailedObservation)
                    .Replace("#RootCauses#", item.DiscussionNote.RootCause)
                    .Replace("#RootCausesString#", rootCausesString.ToString())
                    .Replace("#BusinessImpacts#", item.DiscussionNote.Risks)
                    .Replace("#ImpactsString#", impactMasterString.ToString())
                    .Replace("#RecommendationsString#", recommendationsString.ToString())
                    .Replace("#Recommendations#", item.Recommendation)
                    .Replace("#ManagementResponses#", item.ManagementResponse);

                var implementationOwner = new List<string>();
                var implementationTimeline = new List<string>();

                if (item.ActionPlans != null)
                {
                    foreach (var apItem in item.ActionPlans)
                    {
                        var timeline = apItem.ImplementationEndDate != null ? Convert.ToDateTime(apItem.ImplementationEndDate).ToString("MMMM dd, yyyy") : "";

                        var user = userRepo.GetByID(apItem.ProcessOwnerID);

                        if (user != null)
                        {
                            implementationOwner.Add(user.FirstName + " " + user.LastName);
                            implementationTimeline.Add(timeline);
                        }
                    }
                }

                returnBuilder = returnBuilder
                        .Replace("#ImplemenatationOwners#", string.Join("<br/>", implementationOwner))
                        .Replace("#ImplemenatationOwnerTimeline#", string.Join("<br/>", implementationTimeline));

                counter++;
            }

            #region WKHTMLTOPDF
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            if (returnBuilder.Length == 0)
            {
                returnBuilder.Append(emailBody);
                returnBuilder = returnBuilder
                    .Replace("#Indexing#", "")
                    .Replace("#DiscussionNoteHeading#", "")
                    .Replace("#isRepeat#", "")
                    .Replace("#RiskValue#", "0").Replace("#CriticalRiskRating#", "")
                    .Replace("#HighRiskRating#", "").Replace("#MediumRiskRating#", "").Replace("#LowRiskRating#", "")
                    .Replace("#DiscussionNoteBackground#", "")
                    .Replace("#ObservationDetail#", "")
                    .Replace("#RootCauses#", "")
                    .Replace("#RootCausesString#", "")
                    .Replace("#BusinessImpacts#", "")
                    .Replace("#ImpactsString#", "")
                    .Replace("#RecommendationsString#", "")
                    .Replace("#Recommendations#", "")
                    .Replace("#ManagementResponses#", "")
                    .Replace("#ImplemenatationOwners#", "")
                    .Replace("#ImplemenatationOwnerTimeline#", "");
            }

            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;

            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DraftReport.pdf");
            #endregion
        }

        [HttpGet("getsummary/{id}")]
        public ActionResult GetSummary(string id)
        {
            var discussionNoteSummary = new DiscussionNoteSummaryModel();

            var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            discussionNoteSummary.Critical = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Critical).Count();
            discussionNoteSummary.High = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.High).Count();
            discussionNoteSummary.Medium = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Medium).Count();
            discussionNoteSummary.Low = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Low).Count();
            discussionNoteSummary.NotStarted = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "notstarted").Count();
            discussionNoteSummary.InProgress = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "inprogress").Count();
            discussionNoteSummary.InReview = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "inreview").Count();
            discussionNoteSummary.Completed = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "completed").Count();

            return ResponseOK(discussionNoteSummary);
        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail([FromBody] Service.Utilities.EmailModel emailModel)
        {
            string discusionNumber = "";
            var repoSendMail = new MongoGenericRepository<SendMailHistory>(_dbsetting);
            try
            {
                var DraftObj = _api.GetByID(emailModel.Id);

                if (DraftObj == null)
                    return ResponseNotFound();

                var toEmails = new List<string>();
                var processOwners = new List<string>();

                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                if (DraftObj.ActionPlans != null && DraftObj.ActionPlans.Count > 0)
                {
                    foreach (var item in DraftObj.ActionPlans)
                    {
                        var user = userRepo.GetByID(item.ProcessOwnerID);

                        if (user != null)
                        {
                            processOwners.Add(user.FirstName + " " + user.LastName);
                            toEmails.Add(user.EmailId);
                        }
                    }
                }

                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var objAudit = auditRepo.GetFirst(p => p.Id == DraftObj.AuditId);
                var auditModel = objAudit != null ? objAudit : new Audit();

                var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == auditModel.Id).FirstOrDefault();

                var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                var repoDiscussion = new MongoGenericRepository<DiscussionNote>(_dbsetting);
                var objDiscussionNote = repoDiscussion.GetFirst(x => x.Id == DraftObj.DiscussionNoteID);
                if (objDiscussionNote != null)
                    discusionNumber = objDiscussionNote.DiscussionNumber;

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "DraftReport.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                emailBody = emailBody
                    //.Replace("#ProcessOwnerName#", string.Join(",", processOwners).Replace(",", ", "))
                    .Replace("#AuditName#", auditModel.AuditName)
                    .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"));

                var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                var auditPeriod = auditStartDate + " to " + auditEndDate;
                //emailModel.ToEmail = new List<string>() { "baldev@silverwebbuzz.com" };

                emailModel.ToEmail = toEmails;
                emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | DRAFT Report";
                emailModel.MailBody = emailBody.ToString();
                var file = _CommonServices.DownloadExcelAttachmentForDraftReport(DraftObj);
                var objAttachment = new AttachmentByte()
                {
                    FileContents = file.FileContents,
                    FileName = file.FileDownloadName
                };

                emailModel.Attachments = new List<AttachmentByte>() { objAttachment };

                // _IEmailUtility.SendEmail(emailModel);

                string toEmail = string.Join(",", emailModel.ToEmail);
                SendMailHistory objResponseSendMail = new SendMailHistory()
                {
                    DiscussionNo = discusionNumber,
                    EmailId = toEmail,
                    Message = "Draft email(s) sent successfully.",
                    IsSent = true,
                    CreatedBy = emailModel.CreatedBy
                };
                repoSendMail.Insert(objResponseSendMail);
                return ResponseOK(new { sent = true });
            }
            catch (Exception ex)
            {
                string toEmail = string.Join(",", emailModel.ToEmail);
                SendMailHistory objResponseSendMail = new SendMailHistory()
                {
                    DiscussionNo = discusionNumber,
                    EmailId = toEmail,
                    Message = ex.Message.ToString(),
                    IsSent = false,
                    CreatedBy = emailModel.CreatedBy
                };
                repoSendMail.Insert(objResponseSendMail);
                return ResponseOK(new { sent = false });
            }
        }

        [HttpGet("getfinalsummary/{id}")]
        public ActionResult GetFInalSummary(string id)
        {
            var discussionNoteSummary = new DiscussionNoteSummaryModel();

            var tList = _api.GetWithInclude<User, DiscussionNote>(x => x.AuditId == id && x.Status.ToUpper() == "COMPLETED");

            if (tList == null)
                return ResponseNotFound();

            discussionNoteSummary.Critical = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Critical).Count();
            discussionNoteSummary.High = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.High).Count();
            discussionNoteSummary.Medium = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Medium).Count();
            discussionNoteSummary.Low = tList.Where(a => a.ObservationGrading == ObservationGradingEnum.Low).Count();
            discussionNoteSummary.NotStarted = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "notstarted").Count();
            discussionNoteSummary.InProgress = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "inprogress").Count();
            discussionNoteSummary.InReview = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "inreview").Count();
            discussionNoteSummary.Completed = tList.Where(a => a.Status != null && a.Status.ToLower().ToString() == "completed").Count();

            return ResponseOK(discussionNoteSummary);
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
                string newPath = Path.Combine("manageaudits", auditId, "draftreport", id);

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var auditFiles = repo.GetMany(a => a.AuditId == auditId && a.ModuleId == id && a.ModuleName == "draftreport");

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
        [HttpGet("getDraftReportHistory/{id}")]
        public ActionResult GetDraftReportHistory(string id)
        {
            var repohistory = new MongoGenericRepository<DraftReportHistory>(_dbsetting);
            var tList = repohistory.GetWithInclude<DraftReport, User>(p => p.DraftReportID == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel/{id}")]
        public IActionResult DownloadExcel(string id)
        {
            var tList = _api.GetWithInclude<User>(x => x.AuditId == id);
            if (tList == null)
                return ResponseNotFound();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var discussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var repoAuditFile = new MongoGenericRepository<AuditFiles>(_dbsetting);
            var fileName = "DraftReport.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].Value = "Disussion No";
                worksheet.Cells["B1"].Value = "RACM Number";
                worksheet.Cells["C1"].Value = "Observation Title";
                worksheet.Cells["D1"].Value = "Background";
                worksheet.Cells["E1"].Value = "Detailed Observation";
                worksheet.Cells["F1"].Value = "Root Cause";
                worksheet.Cells["G1"].Value = "Risk(s)/Business Impact";
                worksheet.Cells["H1"].Value = "Recommendation";
                worksheet.Cells["I1"].Value = "Annexures";
                worksheet.Cells["J1"].Value = "Rating";
                worksheet.Cells["K1"].Value = "Implication";
                worksheet.Cells["L1"].Value = "Repeat";
                worksheet.Cells["M1"].Value = "Management Response";
                worksheet.Cells["N1"].Value = "Management Action Plan";
                worksheet.Cells["O1"].Value = "Implementation Owner";
                worksheet.Cells["P1"].Value = "Implementation Timeline";
                var rowIndex = 2;
                foreach (var draftReport in tList)
                {
                    var objDiscussion = discussionNoteRepo.GetFirst(a => a.Id == draftReport.DiscussionNoteID);
                    if (objDiscussion != null)
                    {
                        worksheet.Cells["A" + rowIndex.ToString()].Value = objDiscussion.DiscussionNumber;
                        worksheet.Cells["B" + rowIndex.ToString()].Value = objDiscussion.RACM_Ids != null ? objDiscussion.RACM_Ids[0] : "";
                        worksheet.Cells["C" + rowIndex.ToString()].Value = objDiscussion.ObservationHeading;
                        worksheet.Cells["D" + rowIndex.ToString()].Value = draftReport.FieldBackground != null ? VJLiabraries.UtilityMethods.HtmlToText(draftReport.FieldBackground) : null;
                        worksheet.Cells["E" + rowIndex.ToString()].Value = objDiscussion.DetailedObservation != null ? VJLiabraries.UtilityMethods.HtmlToText(objDiscussion.DetailedObservation) : null;
                        worksheet.Cells["F" + rowIndex.ToString()].Value = objDiscussion.RootCause != null ? VJLiabraries.UtilityMethods.HtmlToText(objDiscussion.RootCause) : null;
                        worksheet.Cells["G" + rowIndex.ToString()].Value = objDiscussion.Risks != null ? VJLiabraries.UtilityMethods.HtmlToText(objDiscussion.Risks) : null;
                        worksheet.Cells["H" + rowIndex.ToString()].Value = draftReport.Recommendation != null ? VJLiabraries.UtilityMethods.HtmlToText(draftReport.Recommendation) : null;
                        worksheet.Cells["J" + rowIndex.ToString()].Value = draftReport.ObservationGrading;
                        if (draftReport.Impacts != null)
                            worksheet.Cells["K" + rowIndex.ToString()].Value = _CommonServices.getImpacts(draftReport.Impacts.ToList());
                        worksheet.Cells["L" + rowIndex.ToString()].Value = objDiscussion.IsRepeat;
                        worksheet.Cells["M" + rowIndex.ToString()].Value = draftReport.ManagementResponse != null ? VJLiabraries.UtilityMethods.HtmlToText(draftReport.ManagementResponse) : null;
                        if (draftReport.ActionPlans != null)
                        {
                            if (draftReport.ActionPlans.Count > 0)
                                foreach (var actionPlan in draftReport.ActionPlans)
                                {

                                    #region Upload Files
                                    var listAuditFiles = repoAuditFile.GetMany(p => p.AuditId == id && p.ModuleName.ToLower() == "draftreport");
                                    int count = 0;
                                    List<string> lstimp = new List<string>();
                                    foreach (var item in listAuditFiles)
                                    {
                                        count++;
                                        lstimp.Add(count.ToString());
                                    }
                                    String[] Id = lstimp.ToArray();
                                    var files = String.Join(",", Id);
                                    #endregion
                                    worksheet.Cells["I" + rowIndex.ToString()].Value = "Annex " + files;
                                    worksheet.Cells["N" + rowIndex.ToString()].Value = actionPlan.ActionPlan != null ? VJLiabraries.UtilityMethods.HtmlToText(actionPlan.ActionPlan) : null;
                                    if (actionPlan.ProcessOwnerID != null)
                                    {
                                        var user = userRepo.GetFirst(p => p.Id == actionPlan.ProcessOwnerID);
                                        if (user != null)
                                            worksheet.Cells["O" + rowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                    }
                                    worksheet.Cells["P" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                                    worksheet.Cells["P" + rowIndex.ToString()].Value = Convert.ToDateTime(actionPlan.ImplementationEndDate);
                                    rowIndex++;
                                }
                            else
                            {
                                rowIndex++;
                            }
                        }
                        else
                            rowIndex++;
                    }
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("RemoveSendEmailHistory/{userid}")]
        public IActionResult RemoveSendEmailHistory(string userid)
        {
            var repoSendMailHistory = new MongoGenericRepository<SendMailHistory>(_dbsetting);
            repoSendMailHistory.Delete(x => x.CreatedBy == userid);
            return Ok(new { Deleted = true });
        }
        [HttpGet("GetSendEmailHistory/{userid}")]
        public ActionResult GetSendEmailHistory(string userid)
        {
            var repoSendMailHistory = new MongoGenericRepository<SendMailHistory>(_dbsetting);
            var tList = repoSendMailHistory.GetWithInclude<SendMailHistory>(x => x.CreatedBy == userid);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }

    public class DraftReportStatusUpdate : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Id { get; set; }
        [Required(ErrorMessage = "{0} is required.")]
        public string Status { get; set; }
        public string Justification { get; set; }
        public string ManagementComments { get; set; }
        public string ImplementationDate { get; set; }
    }
}