using System;
using System.Collections.Generic;
using System.Linq;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using System.IO;
using System.Text;
using VJLiabraries;
using AuditManagementCore.Service.Utilities;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowUpController : VJBaseGenericAPIController<FollowUp>
    {
        IMongoDbSettings _dbsetting;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        CommonServices _CommonServices;
        IEmailUtility _IEmailUtility;
        public FollowUpController(IMongoGenericRepository<FollowUp> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, CommonServices cs, IEmailUtility emailUtility) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _CommonServices = cs;
            _IEmailUtility = emailUtility;
        }

        public override ActionResult Post([FromBody] FollowUp e)
        {
            if (e == null) return ResponseBad("Follow up obj is null");

            if (e.AuditExist == true)
            {
                if (string.IsNullOrWhiteSpace(e.DraftReportId)) return ResponseBad("Draft report is null or blank");

                var DraftRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
                var DraftRecord = DraftRepo.GetFirst(x => x.Id == e.DraftReportId);

                if (DraftRecord == null)
                    return ResponseBad("Draft report not found");
                else
                {
                    try
                    {
                        populateFollowup(e, DraftRecord);
                    }
                    catch (Exception ex)
                    {
                        return ResponseError(ex.Message);
                    }
                }
            }
            else
                e.ImplementationEndDate = e.ImplementationEndDate?.ToLocalTime();

            var FollowUp = base.Post(e);

            //Activity Log
            var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
            var audit = _repoAudit.GetFirst(x => x.Id == e.AuditId);

            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "FollowUp(" + audit.AuditName + ")", "FollowUp", "FollowUp | Add", "Added FollowUp");

            return FollowUp;
        }

        private void populateFollowup(FollowUp e, DraftReport DraftRecord)
        {
            var dnRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var overallAssesmentRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);
            var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            var dnRecord = dnRepo.GetFirst(x => x.Id == DraftRecord.DiscussionNoteID);

            if (dnRecord != null)
            {
                e.ObservationGrading = dnRecord.ObservationGrading;
                e.ObservationHeading = dnRecord.ObservationHeading;
                e.DetailedObservation = dnRecord.DetailedObservation;
                e.RiskTypeId = dnRecord.RiskTypeId;
                e.Implications = dnRecord.Risks;
                e.Recommendation = DraftRecord.Recommendation;
                e.ActionPlan = DraftRecord.ActionPlan;
                e.ImplementationOwnerId = DraftRecord.ProcessOwnerID;
                e.ImplementationRevisedDate = DraftRecord.ImplementationStartDate;
                e.ImplementationEndDate = DraftRecord.ImplementationEndDate;
                //e.RootCauseId = DraftRecord.RootCause;
                e.RootCauses = DraftRecord.RootCauses;

                if (DraftRecord.AuditId != null)
                {
                    var audit = auditRepo.GetFirst(a => a.Id == DraftRecord.AuditId);

                    if (audit != null && audit.OverallAssesmentId != null)
                    {
                        var overallAssessment = overallAssesmentRepo.GetFirst(a => a.Id == audit.OverallAssesmentId);

                        if (overallAssessment != null)
                            e.ProcessLocationMappingId = overallAssessment.ProcessLocationMappingID;
                    }
                    if (audit != null && audit.ProcessLocationMapping != null)
                    {

                        var objPLM = plmRepo.GetFirst(p => p.Id == audit.ProcessLocationMapping.Id);
                        if (objPLM != null)
                            e.LocationID = objPLM.Locations.FirstOrDefault();
                    }
                }
            }
        }

        [HttpPut("UpdateFollowup/{actionplanFlag}")]
        public ActionResult UpdateFollowup(string actionplanFlag, FollowUp e)
        {
            var repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
            if (e == null)
                return ResponseBad("Action Planning object is null");

            var actionPlan = _api.GetFirst(x => x.Id == e.Id);
            if (e.ProcessLocationMappingId == null)
            {
                var objaudit = repoAudit.GetFirst(p => p.Id == e.AuditId);
                if (objaudit != null)
                {
                    e.ProcessLocationMappingId = objaudit.ProcessLocationMapping.Id;
                    e.LocationID = objaudit.Location.Id;
                }
            }
            if (actionPlan == null)
                return ResponseError("Action Planning does not exists");

            populateFollowup(actionPlan, e);

            _api.Update(actionPlan);
            SendEmail(e);
            //Activity Log
            if (actionplanFlag == "followup")
            {
                var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
                var audit = _repoAudit.GetFirst(x => x.Id == e.AuditId);
                _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "FollowUp(" + audit.AuditName + ")", "FollowUp", "Manage Audits | Follow Up | Edit", "Updated Follow Up");
                return ResponseOK(e);
            }
            else
            {
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var audit = _repoPLM.GetFirst(x => x.Id == (e.ProcessLocationMappingId));
                _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "ActionPlanning(" + (!String.IsNullOrEmpty(e.AuditName) ? e.AuditName : audit.AuditName) + ")", "FollowUp", "Auditee Action Plan Status | Edit", "Updated Action Plan");
                return ResponseOK(e);
            }
        }

        private void populateFollowup(FollowUp actionPlan, FollowUp e)
        {
            actionPlan.ObservationGrading = e.ObservationGrading;
            actionPlan.ObservationHeading = e.ObservationHeading;
            actionPlan.DetailedObservation = e.DetailedObservation;
            actionPlan.RiskTypeId = e.RiskTypeId;
            actionPlan.Implications = e.Implications;
            actionPlan.Recommendation = e.Recommendation;
            //actionPlan.ActionPlan = e.ActionPlan;
            //actionPlan.ImplementationOwnerId = e.ImplementationOwnerId;
            //actionPlan.ImplementationRevisedDate = e.ImplementationRevisedDate;
            actionPlan.ImplementationEndDate = e.ImplementationEndDate != null ? e.ImplementationEndDate?.ToLocalTime() : null;
            actionPlan.RootCauses = e.RootCauses;
            //actionPlan.Comments = e.Comments;
            actionPlan.Status = e.Status;
            actionPlan.AuditeeStatus = e.AuditeeStatus;
            actionPlan.ActionPlans = e.ActionPlans;
            actionPlan.UpdatedBy = e.UpdatedBy;
            actionPlan.ProcessLocationMappingId = e.ProcessLocationMappingId;
            actionPlan.LocationID = e.LocationID;
            actionPlan.ReviewQtr = e.ReviewQtr;
            actionPlan.ObsNumber = e.ObsNumber;
            actionPlan.ResponsibilityDepartment = e.ResponsibilityDepartment;
            actionPlan.ImplementationRemarks = e.ImplementationRemarks;
            actionPlan.AuditName = e.AuditName;
            actionPlan.AuditNumber = e.AuditNumber;
            actionPlan.ManagementResponse = e.ManagementResponse;
            actionPlan.AgreedActionPlan = e.AgreedActionPlan;
            actionPlan.ImplementationOwnerId = e.ImplementationOwnerId;
            actionPlan.RevisedDate = e.RevisedDate != null ? e.RevisedDate?.ToLocalTime() : null;
        }

        [HttpPut("UpdateStatus")]
        public ActionResult UpdateStatus(FollowUp f)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var Obj = _api.GetByID(f.Id);

            if (Obj != null)
            {
                if (!String.IsNullOrEmpty(f.Status) && !String.IsNullOrWhiteSpace(f.Status))
                {
                    switch (f.Status.ToUpper())
                    {
                        case "PENDING": Obj.Status = AuditConstants.Status.PENDING; break;
                        case "COMPLETED": Obj.Status = AuditConstants.Status.COMPLETED; break;
                        default: Obj.Status = AuditConstants.Status.INPROGRESS; break;
                    }
                }

                if (!String.IsNullOrEmpty(f.Comments) && !String.IsNullOrWhiteSpace(f.Comments))
                    Obj.Comments = f.Comments ?? "";

                if (f.RevisedDate != null)
                    Obj.RevisedDate = f.RevisedDate ?? null;

                _api.Update(Obj);
                //Activity Log
                var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
                var audit = _repoAudit.GetFirst(x => x.Id == f.AuditId);

                _CommonServices.ActivityLog(f.UpdatedBy, f.Id, "FollowUp(" + audit.AuditName + ")", "FollowUp", "FollowUp | Edit", "Updated FollowUp Status");
                return Ok(Obj);
            }
            else
            {
                return NotFound();
            }
        }

        public override ActionResult Delete(string id, string userid)
        {
            //Activity Log
            var followup = base.Delete(id, userid);
            _CommonServices.ActivityLog(userid, id, "", "FollowUp", "FollowUp | Delete", "Deleted FollowUp");
            return followup;
        }

        [HttpDelete("DeleteBydraftId/{draftId}/{userid}")]
        public ActionResult DeleteBydraftId(string draftId, string userid)
        {
            var isexist = _api.GetFirst(x => x.DraftReportId == draftId);
            var followup = base.Delete(isexist.Id, userid);
            _CommonServices.ActivityLog(userid, draftId, "", "FollowUp", "Final Report | Delete", "Deleted Final Report");
            return followup;
        }

        [HttpGet("getbyid/{id}")]
        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<Audit, ProcessLocationMapping, DraftReport>(x => x.Id == id).FirstOrDefault();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var repoAuditFiles = new MongoGenericRepository<AuditFiles>(_dbsetting);
            var scopeScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);

            if (!string.IsNullOrEmpty(tList.ImplementationOwnerId))
                tList.ImplementationOwner = userRepo.GetByID(tList.ImplementationOwnerId);
            tList.ActionPlansInfo = repoFollowupActionPlan.GetMany(a => a.FollowupId == tList.Id).ToList();
            tList.Audit = populateScopeAndSchedule(scopeScheduleRepo.GetFirst(x => x.AuditId == tList.AuditId));
            if (tList.ActionPlansInfo != null)
            {
                foreach (var item in tList.ActionPlansInfo)
                {
                    var filesInfo = new List<AuditFiles>();

                    if (item.Files != null)
                    {
                        foreach (var file in item.Files)
                        {
                            if (file != null)
                            {
                                var fileInfo = repoAuditFiles.GetByID(file);

                                if (fileInfo != null)
                                    filesInfo.Add(fileInfo);
                            }
                        }
                    }

                    item.FilesInfo = filesInfo;
                }
            }

            //tList.Responsibility = userRepo.GetByID(tList.ResponsibilityId);

            return ResponseOK(tList);
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<DraftReport>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            tList = FetchAllRequiredData(tList);

            return ResponseOK(tList);
        }

        private IQueryable<FollowUp> FetchAllRequiredData(IQueryable<FollowUp> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var dnRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var scopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var repoAuditFiles = new MongoGenericRepository<AuditFiles>(_dbsetting);
            var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);


            foreach (var item in tList)
            {
                if (item.ImplementationOwner != null)
                    item.ImplementationOwner = userRepo.GetFirst(p => p.Id == item.ImplementationOwnerId);
                item.Audit = populateScopeAndSchedule(scopeAndSchedule.GetFirst(x => x.AuditId == item.AuditId));

                if (item.DraftReportId != null)
                    item.DraftReport.DiscussionNote = dnRepo.GetWithInclude<RiskType>(a => a.Id == item.DraftReport.DiscussionNoteID).FirstOrDefault();

                if (item.RiskTypeId != null)
                    item.RiskType = riskTypeRepo.GetFirst(a => a.Id == item.RiskTypeId);

                item.ActionPlansInfo = repoFollowupActionPlan.GetMany(a => a.FollowupId == item.Id).ToList();

                if (item.ActionPlansInfo != null)
                {
                    foreach (var apItem in item.ActionPlansInfo)
                    {
                        var filesInfo = new List<AuditFiles>();

                        if (apItem.Files != null)
                        {
                            foreach (var file in apItem.Files)
                            {
                                if (file != null)
                                {
                                    var fileInfo = repoAuditFiles.GetByID(file);

                                    if (fileInfo != null)
                                        filesInfo.Add(fileInfo);
                                }
                            }
                        }

                        apItem.FilesInfo = filesInfo;
                    }
                }

                if (item.ProcessLocationMappingId != null)
                {
                    item.ProcessLocationMapping = plmRepo.GetByID(item.ProcessLocationMappingId);
                }
                if (item.LocationID != null)
                    item.Location = _CommonServices.GetLocationDetail(item.LocationID);
            }
            return tList;
        }

        public ScopeAndSchedule populateScopeAndSchedule(ScopeAndSchedule item)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            if (item != null)
            {
                if (item.LocationId != null)
                    item.Location = _CommonServices.GetLocationDetail(item.LocationId);

                if (item.AuditId != null)
                    item.Audit = _CommonServices.GetAuditDetail(item.AuditId);
            }
            return item;
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<DraftReport>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            tList = FetchAllRequiredData(tList);
            return ResponseOK(tList);
        }

        [HttpGet("GetFollowUp")]
        public ActionResult GetFollowUp()
        {
            var tList = GetFolllowupData();
            return ResponseOK(tList);
        }

        public IQueryable<FollowUp> GetFolllowupData()
        {
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var tList = _api.GetAllWithInclude<DraftReport, Audit>();
            if (tList != null)
            {
                var dashboardQuery = new DashboardQuery();

                dashboardQuery.Audit = Request.Query["AuditId"];
                dashboardQuery.CompletionStatus = Request.Query["Status"];
                dashboardQuery.Rating = Request.Query["Rating"];
                dashboardQuery.StartDate = Request.Query["StartDate"];
                dashboardQuery.EndDate = Request.Query["EndDate"];
                var ImplementationOwnerId = Request.Query["ImplementationOwnerId"];

                if (!string.IsNullOrWhiteSpace(dashboardQuery.EndDate) && !string.IsNullOrWhiteSpace(dashboardQuery.StartDate))
                {
                    tList = tList.Where(x => x.ImplementationEndDate != null && (x.ImplementationEndDate >= DateTime.Parse(dashboardQuery.StartDate).ToUniversalTime() && x.ImplementationEndDate <= DateTime.Parse(dashboardQuery.EndDate).ToUniversalTime()));
                }

                if (!string.IsNullOrWhiteSpace(dashboardQuery.CompletionStatus))
                {
                    tList = tList.Where(x => x.Status.ToLower() == dashboardQuery.CompletionStatus.ToString().ToLower());
                }

                if (!string.IsNullOrWhiteSpace(dashboardQuery.Audit))
                {
                    tList = tList.Where(x => x.ProcessLocationMappingId == dashboardQuery.Audit);
                }
                if (!string.IsNullOrWhiteSpace(dashboardQuery.Rating))
                {
                    string rating = dashboardQuery.Rating.ToString();
                    ObservationGradingEnum e = (ObservationGradingEnum)Enum.Parse(typeof(ObservationGradingEnum), rating);
                    tList = tList.Where(x => x.ObservationGrading == e);
                }
                tList = FetchAllRequiredData(tList);
                if (!string.IsNullOrWhiteSpace(ImplementationOwnerId))
                {
                    tList = tList.Where(p => p.ImplementationOwnerId == ImplementationOwnerId);
                }
            }
            return tList;
        }
        [HttpGet("GetUserbyFollowUp")]
        public ActionResult GetUserbyFollowUp()
        {
            var repoFollowup = new MongoGenericRepository<FollowUp>(_dbsetting);
            var repoUser = new MongoGenericRepository<User>(_dbsetting);
            var tList = (from x in repoFollowup.GetAll() join user in repoUser.GetAll() on x.ImplementationOwnerId equals user.Id select user).Distinct().ToList();
            return ResponseOK(tList);
        }
        public ActionResult GetAuditbyFollowUp()
        {
            var repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
            var repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
            var repoplm = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            var tList = (from x in repoFollowUp.GetAll() join audit in repoAudit.GetAll() on x.AuditId equals audit.Id select x).Distinct().ToList();
            foreach (var item in tList)
            {
                if (item.ProcessLocationMappingId != null)
                {
                    item.ProcessLocationMapping = repoplm.GetByID(item.ProcessLocationMappingId);
                }
            }
            return ResponseOK(tList);
        }
        [HttpGet("downloadpdf")]
        public IActionResult DownloadPDF()
        {
            var tList = GetFolllowupData();

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

            return File(memoryStream, UtilityMethods.GetContentType(".pdf"), "Followup.pdf");
        }

        [HttpGet("downloadexcelforReport")]
        public IActionResult DownloadExcelForReport()
        {
            var tList = GetFolllowupData();

            if (tList == null)
                return ResponseNotFound();

            var fileName = "FollowUp.xlsx";
            var memoryStream = new MemoryStream();
            var processLocationMappingRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var LocationRepo = new MongoGenericRepository<Location>(_dbsetting);
            var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "Audit";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Observation Heading";
                worksheet.Cells["D1"].Value = "Detailed Observation";
                worksheet.Cells["E1"].Value = "Root Cause";
                worksheet.Cells["F1"].Value = "Risk Type";
                worksheet.Cells["G1"].Value = "End Date";
                worksheet.Cells["H1"].Value = "Status";
                var rowIndex = 2;

                foreach (var followup in tList)
                {
                    if (followup.ProcessLocationMappingId != null)
                    {
                        var audit = processLocationMappingRepo.GetWithInclude<Location, Audit>(x => x.Id == followup.ProcessLocationMappingId).FirstOrDefault();
                        if (audit != null)
                        {
                            worksheet.Cells["A" + rowIndex.ToString()].Value = audit.AuditName;
                        }
                        var objLocation = LocationRepo.GetFirst(p => p.Id == followup.LocationID);
                        if (objLocation != null)
                            worksheet.Cells["B" + rowIndex.ToString()].Value = objLocation.LocationDescription;
                    }
                    worksheet.Cells["C" + rowIndex.ToString()].Value = followup.ObservationHeading != null ? VJLiabraries.UtilityMethods.HtmlToText(followup.ObservationHeading) : "";
                    worksheet.Cells["D" + rowIndex.ToString()].Value = followup.DetailedObservation != null ? VJLiabraries.UtilityMethods.HtmlToText(followup.DetailedObservation) : "";
                    worksheet.Cells["E" + rowIndex.ToString()].Value = followup.RootCauses != null ? _CommonServices.getCause(followup.RootCauses.ToList()) : ""; ;
                    if (followup.RiskTypeId != null)
                    {
                        var riskType = risktypeRepo.GetByID(followup.RiskTypeId);
                        if (riskType != null)
                        {
                            worksheet.Cells["F" + rowIndex.ToString()].Value = riskType.Name;
                        }
                    }
                    if (followup.ImplementationEndDate != null)
                    {
                        worksheet.Cells["G" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                        worksheet.Cells["G" + rowIndex.ToString()].Value = Convert.ToDateTime(followup.ImplementationEndDate).ToShortDateString();
                    }
                    if (!string.IsNullOrEmpty(followup.Status))
                    {
                        followup.Status = followup.Status.ToLower();
                        if (followup.Status == "inprogress" || followup.Status == "in progress")
                            worksheet.Cells["H" + rowIndex.ToString()].Value = "In Progress";
                        else if (followup.Status == "PENDING TO BE INITIATED" || followup.Status == "pending")
                            worksheet.Cells["H" + rowIndex.ToString()].Value = "Pending to be initiated";
                        else if (followup.Status == "completed")
                            worksheet.Cells["H" + rowIndex.ToString()].Value = "Completed";
                    }
                    rowIndex++;
                }
                double minimumSize = 20;
                double maximumSize = 50;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        public void SendEmail(FollowUp objFollowUp)
        {
            try
            {
                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "UpdateActionPlanning.html");
                var emailModel = new Service.Utilities.EmailModel();
                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }
                var repoScope = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var repoUser = new MongoGenericRepository<User>(_dbsetting);
                var objScope = repoScope.GetFirst(p => p.ProcessLocationMappingId == objFollowUp.ProcessLocationMappingId);
                if (objScope != null)
                {
                    List<string> lstEmail = new List<string>();
                    if (objScope.AuditResources != null)
                    {
                        foreach (var item in objScope.AuditResources)
                        {
                            var objUser = repoUser.GetFirst(p => p.Id == item.UserId);
                            if (objUser != null)
                            {
                                lstEmail.Add(objUser.EmailId);
                            }
                        }
                    }
                    if (objScope.AuditApprovalMapping != null)
                    {
                        foreach (var item in objScope.AuditApprovalMapping.UserData)
                        {
                            var objUser = repoUser.GetFirst(p => p.Id == item.UserId);
                            if (objUser != null)
                            {
                                lstEmail.Add(objUser.EmailId);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(objFollowUp.ImplementationOwnerId))
                    {
                        var objUser = repoUser.GetFirst(p => p.Id == objFollowUp.ImplementationOwnerId);
                        if (objUser != null)
                        {
                            lstEmail.Add(objUser.EmailId);
                        }
                    }
                    var objUpdatebyUser = repoUser.GetFirst(p => p.Id == objFollowUp.UpdatedBy);
                    if (objUpdatebyUser != null)
                    {
                        emailBody = emailBody
                             .Replace("#EmailId#", (objUpdatebyUser.EmailId))
                             .Replace("#ObservationHeading#", (objFollowUp.ObservationHeading));
                        emailModel.ToEmail = new List<string> { objUpdatebyUser.EmailId };
                    }
                    //emailModel.ToEmail = new List<string> { "baldev@silverwebbuzz.com" };
                    emailModel.CcEmail = lstEmail;
                    emailModel.Subject = "Update Action Planning";
                    emailModel.MailBody = emailBody.ToString();

                    _IEmailUtility.SendEmail(emailModel);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}