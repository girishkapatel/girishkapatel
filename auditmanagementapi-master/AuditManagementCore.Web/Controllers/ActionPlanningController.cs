using System;
using System.Collections.Generic;
using System.Linq;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Models;
using Microsoft.AspNetCore.Mvc;
using AuditManagementCore.Service;
using AuditManagementCore.ViewModels;
using VJLiabraries;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using System.Threading.Tasks;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using static AuditManagementCore.Service.CommonServices;
using OfficeOpenXml.Drawing.Chart;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionPlanningController : VJBaseGenericAPIController<ActionPlanning>
    {
        #region Class Properties Declarations

        IMongoDbSettings _dbsetting;
        IEmailUtility _IEmailUtility;
        IWebHostEnvironment _IWebHostEnvironment;
        IDocumentUpload _IDocumentUpload;
        CommonServices _CommonServices;
        #endregion

        public ActionPlanningController(IMongoGenericRepository<ActionPlanning> api, IMongoDbSettings mongoDbSettings, IDocumentUpload documentUpload, IEmailUtility emailUtility, IWebHostEnvironment webHostEnvironment, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IEmailUtility = emailUtility;
            _IDocumentUpload = documentUpload;
            _IWebHostEnvironment = webHostEnvironment;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] ActionPlanning e)
        {
            if (e == null) return ResponseBad("Action Planning object is null");
            var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());
            if (isExist)
            {
                return ResponseError("Action Planning already exists for Audit");
            }
            var country = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "ActionPlanning", "ActionPlanning | Add", "Added ActionPlanning");
            return country;
        }

        [HttpPost("AddFollowup")]
        public ActionResult AddFollowup(FollowUp e)
        {
            try
            {
                if (e == null)
                    return ResponseBad("Action Planning object is null");

                var actionPlanList = new MongoGenericRepository<FollowUp>(_dbsetting);

                //var isExist = actionPlanList.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());
                //if (isExist)
                //{
                //    return ResponseError("Action Planning already exists for Audit");
                //}

                e.ImplementationEndDate = e.ImplementationEndDate?.ToLocalTime();
                actionPlanList.Insert(e);
                //Activity Log
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var audit = _repoPLM.GetFirst(x => x.Id == e.ProcessLocationMappingId);

                _CommonServices.ActivityLog(e.CreatedBy, e.Id, "FollowUp(" + (!String.IsNullOrEmpty(e.AuditName) ? e.AuditName : audit.AuditName) + ")", "FollowUp", "FollowUp | Add", "Added FollowUp");
                return ResponseOK(e);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        [HttpPost("savefollowupactionplan")]
        public ActionResult SaveFollowupActionPlan(FollowupActionPlan e)
        {
            if (e == null) return ResponseBad("Action Planning object is null");

            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            repoFollowupActionPlan.Insert(e);
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "FollowupActionPlan", "FollowupActionPlan | Add", "Added FollowupActionPlan");
            return ResponseOK(e);
        }

        [HttpPost("Multiple")]
        public ActionResult AddMultiple(List<FollowupActionPlan> e)
        {
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var repoAuditFiles = new MongoGenericRepository<AuditFiles>(_dbsetting);

            foreach (var item in e)
            {
                var isExist = repoFollowupActionPlan.Exists(x => x.Id == item.Id);

                if (isExist)
                    return ResponseError("Action plan details with ID: " + item.Id + " already exists.");

                item.RevisedDate = item.RevisedDate?.ToLocalTime();

                repoFollowupActionPlan.Insert(item);

                var actionplan = item.ActionPlan != null ? VJLiabraries.UtilityMethods.HtmlToText(item.ActionPlan) : "";

                _CommonServices.ActivityLog(item.CreatedBy, item.Id, actionplan, "FollowupActionPlan", "FollowupActionPlan | Add", "Added FollowupActionPlan");
            }

            return ResponseOK(e);
        }

        [HttpPut("Multiple")]
        public ActionResult UpdateMultiple([FromBody] List<FollowupActionPlan> e)
        {
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);

            foreach (var item in e)
            {
                var isExist = repoFollowupActionPlan.Exists(x => x.Id == item.Id);

                item.RevisedDate = item.RevisedDate?.ToLocalTime();

                if (!isExist)
                    repoFollowupActionPlan.Insert(item);
                else
                    repoFollowupActionPlan.Update(item);
            }

            return ResponseOK(e);
        }

        [HttpDelete("deletefollowupactionplan/{id}/{userid}")]
        public ActionResult DeleteFollowupActionPlan(string id, string userId)
        {
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var repoAuditFiles = new MongoGenericRepository<AuditFiles>(_dbsetting);

            if (id != null)
            {
                var actionPlan = repoFollowupActionPlan.GetByID(id);

                repoFollowupActionPlan.Delete(id);

                if (actionPlan.Files != null)
                {
                    foreach (var file in actionPlan.Files)
                    {
                        repoAuditFiles.Delete(file);
                    }
                }
            }

            return ResponseSuccess(new JsonResult(id));
        }

        [HttpDelete("removemultiplefiles/{id}")]
        public ActionResult DeleteMultipleFiles(string id)
        {
            var repoAuditFiles = new MongoGenericRepository<AuditFiles>(_dbsetting);

            foreach (var item in id.Split(','))
            {
                repoAuditFiles.Delete(item);
            }

            return ResponseSuccess(new JsonResult(id));
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Audit>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                if (!string.IsNullOrEmpty(item.ImplementationOwnerId))
                    item.ImplementationOwner = userRepo.GetByID(item.ImplementationOwnerId);
                if (!string.IsNullOrEmpty(item.ResponsibilityId))
                    item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
            }
            return ResponseOK(tList);
        }

        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.Id == id).FirstOrDefault();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            tList.ImplementationOwner = userRepo.GetByID(tList.ImplementationOwnerId);
            tList.Responsibility = userRepo.GetByID(tList.ResponsibilityId);

            return ResponseOK(tList);
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
            {
                return ResponseNotFound();
            }

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                item.ImplementationOwner = userRepo.GetByID(item.ImplementationOwnerId);
                item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
            }

            return ResponseOK(tList);
        }

        [HttpGet("GetByStatus/{status}")]
        public ActionResult GetByStatus(string status)
        {
            try
            {
                var tList = fetchActionPlanning(status);
                if (tList == null)
                {
                    return ResponseNotFound();
                }
                var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
                var dashboardQuery = new DashboardQuery();
                dashboardQuery.Location = Request.Query["Divison"];
                dashboardQuery.Audit = Request.Query["Audit"];
                dashboardQuery.Quarter = Request.Query["Period"];
                dashboardQuery.CompletionStatus = Request.Query["ObservationGrading"];
                if (dashboardQuery.Location != "")
                {
                    var objLocation = locationRepo.GetFirst(p => p.Id == dashboardQuery.Location);
                    if (objLocation != null)
                    {
                        tList = tList.Where(p => (p.LocationID == dashboardQuery.Location));
                        //tList = tList.Where(p => (p.LocationID == dashboardQuery.Location) || (p.ProcessLocationMappingId != null && (p.ProcessLocationMapping.Locations.Any(t => t.ToString() == dashboardQuery.Location))));
                    }
                }
                if (dashboardQuery.Audit != "")
                {
                    //var auditRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                    //var objAudit = auditRepo.GetFirst(p => p.Id == dashboardQuery.Audit);
                    //if (objAudit != null)
                    tList = tList.Where(p => p.ProcessLocationMappingId != null && p.ProcessLocationMappingId == dashboardQuery.Audit);
                }
                if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
                {
                    string[] quater = dashboardQuery.Quarter.Split(',');
                    quater[0] = quater[0].Replace("FY ", "").Trim();
                    //var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]));
                    //if (audits != null)                                                       
                    //tList = tList.Where(a => a.ImplementationEndDate != null &&((a.ImplementationEndDate.Value.Month >= 4 ? (a.ImplementationEndDate.Value.Year + 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy") : (a.ImplementationEndDate.Value.Year - 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy")) == quater[1]));
                    //tList = tList.Where(a => a.ImplementationEndDate != null &&((a.ImplementationEndDate.Value.Month >= 4 ? a.ImplementationEndDate.Value.ToString("yy")+ "-" + (a.ImplementationEndDate.Value.Year + 1).ToString("yy") : (a.ImplementationEndDate.Value.Year - 1).ToString("yy") + "-" + a.ImplementationEndDate.Value.ToString("yy")) == quater[1]));
                    tList = tList.Where(a => a.ImplementationEndDate != null && (a.ImplementationEndDate.Value.Month >= 4 ? a.ImplementationEndDate.Value.ToString("yy") + "-" + (a.ImplementationEndDate.Value.Year + 1).ToString().Substring(2, 2) == quater[0] : (a.ImplementationEndDate.Value.Year - 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy") == quater[0]));
                }
                if (dashboardQuery.CompletionStatus != "")
                {
                    int ObsGrading = 0;
                    if (dashboardQuery.CompletionStatus.ToLower().Contains("critical")) { ObsGrading = (int)ObservationGradingEnum.Critical; }
                    else if (dashboardQuery.CompletionStatus.ToLower().Contains("high")) { ObsGrading = (int)ObservationGradingEnum.High; }
                    else if (dashboardQuery.CompletionStatus.ToLower().Contains("medium")) { ObsGrading = (int)ObservationGradingEnum.Medium; }
                    else if (dashboardQuery.CompletionStatus.ToLower().Contains("low")) { ObsGrading = (int)ObservationGradingEnum.Low; }

                    tList = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ((ObservationGradingEnum)ObsGrading) && a.ImplementationEndDate != null) || a.ObservationGrading == ((ObservationGradingEnum)ObsGrading));

                }
                var scopeScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var dnRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var rootcauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
                var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
                var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
                var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var actionPlanRepo = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);

                foreach (var item in tList)
                {
                    //item.Audit = populateScopeAndSchedule(scopeScheduleRepo.GetFirst(x => x.ProcessLocationMappingId == item.ProcessLocationMappingId));

                    if (item.DraftReportId != null)
                    {
                        item.DraftReport = draftReportRepo.GetWithInclude<DiscussionNote>(a => a.Id == item.DraftReportId).FirstOrDefault();

                        if (item.DraftReport != null)
                            item.DraftReport.DiscussionNote = dnRepo.GetWithInclude<RiskType>(a => a.Id == item.DraftReport.DiscussionNoteID).FirstOrDefault();
                    }

                    item.ProcessLocationMapping = plmRepo.GetByID(item.ProcessLocationMappingId);
                    if (item.ProcessLocationMapping != null)
                    {
                        if (item != null)
                        {
                            item.Location = _CommonServices.GetLocationDetail(item.ProcessLocationMapping.Locations.FirstOrDefault());
                            //item.Audit = mp.GetAuditDetail(item.AuditId);
                        }
                    }
                    if (item.LocationID != null)
                        item.Location = _CommonServices.GetLocationDetail(item.LocationID);
                    if (!string.IsNullOrEmpty(item.ImplementationOwnerId))
                        item.ImplementationOwner = userRepo.GetByID(item.ImplementationOwnerId);
                    if (!string.IsNullOrEmpty(item.RootCauseId))
                        item.RootCause = rootcauseRepo.GetByID(item.RootCauseId);
                    if (!string.IsNullOrEmpty(item.RiskTypeId))
                        item.RiskType = risktypeRepo.GetByID(item.RiskTypeId);

                    var objSubplan = actionPlanRepo.GetMany(p => p.FollowupId == item.Id);
                    if (objSubplan != null)
                        item.ActionPlansInfo = objSubplan.ToList();
                }
                return ResponseOK(tList);
            }
            catch (Exception e)
            {

                throw;
            }
            return ResponseOK(null);
        }
        private IEnumerable<FollowUp> fetchActionPlanning(string status)
        {
            status = status.Trim();

            var actionPlanList = new MongoGenericRepository<FollowUp>(_dbsetting);
            var tList = actionPlanList.GetAllWithInclude<Audit, DraftReport, Location, ProcessLocationMapping, ScopeAndSchedule>();

            if (getStatus().Contains(status.ToLower()))
            {
                if (status.ToLower() == "inprogress")
                {
                    //return tList.Where(x => x.Status.ToLower() == status && (x.ImplementationEndDate != null && (x.ImplementationEndDate > DateTime.UtcNow)));
                    return tList.Where(x => x.Status.ToLower() == status);
                }
                else
                {
                    return tList.Where(x => x.Status == status);
                }
            }
            else if (status.ToLower() == "pending")
            {
                return tList.Where(x =>  x.Status.ToLower() == "pending");
            }
            else if (status.ToLower() == "overdue")
            {
                return tList.Where(x => (x.Status.ToLower() == "pending" || x.Status.ToLower() == "inprogress") && (x.ImplementationEndDate != null && (x.ImplementationEndDate < DateTime.UtcNow)) || (x.RevisedDate != null && (x.RevisedDate < DateTime.UtcNow)));
            }
            else if (status.ToLower() == "duedate")
            {
                return tList.Where(x => (x.Status.ToLower() == "pending" || x.Status.ToLower() == "inprogress") && (x.ImplementationEndDate != null && x.ImplementationEndDate == DateTime.UtcNow));
            }
            else if (status.ToLower() == "notdue")
            {
                return tList.Where(a => (a.Status.ToLower() == "pending") && a.ImplementationEndDate != null && Convert.ToDateTime(a.ImplementationEndDate) > DateTime.Now);
            }
            else if (status.ToLower() == "revisedtimeline")
            {
                return tList.Where(x => x.RevisedDate != null);
            }
            else
            {
                return tList;
            }
            return null;
        }

        private List<string> getStatus()
        {
            List<string> statuses = new List<string>();
            statuses.Add("pending");
            statuses.Add("inprogress");
            statuses.Add("completed");
            return statuses;
        }

        public ScopeAndSchedule populateScopeAndSchedule(ScopeAndSchedule item)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            if (item != null)
            {
                item.Location = _CommonServices.GetLocationDetail(item.LocationId);
                item.Audit = _CommonServices.GetAuditDetail(item.AuditId);
            }
            return item;
        }

        [HttpGet("getchartdata")]
        public IActionResult GetChartData()
        {
            var tList = fetchActionPlanning("");
            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var dashboardQuery = new DashboardQuery();
            dashboardQuery.Location = Request.Query["Divison"];
            dashboardQuery.Audit = Request.Query["Audit"];
            dashboardQuery.Quarter = Request.Query["Period"];
            var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            //var auditId = Request.Query["Audit"];
            //var Period = Request.Query[""];
            if (dashboardQuery.Location != "")
            {
                var objLocation = locationRepo.GetFirst(p => p.Id == dashboardQuery.Location);
                if (objLocation != null)
                {
                    tList = tList.Where(p => (p.LocationID == dashboardQuery.Location));
                    //tList = tList.Where(p => (p.LocationID == dashboardQuery.Location) || (p.ProcessLocationMappingId != null && (p.ProcessLocationMapping.Locations.Any(t => t.ToString() == dashboardQuery.Location))));
                }
            }
            if (dashboardQuery.Audit != "")
            {
                //var auditRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                //var objAudit = auditRepo.GetFirst(p => p.Id == dashboardQuery.Audit);
                //if (objAudit != null)
                tList = tList.Where(p => p.ProcessLocationMappingId != null && p.ProcessLocationMappingId == dashboardQuery.Audit);
            }
            if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            {
                string[] quater = dashboardQuery.Quarter.Split(',');
                quater[0] = quater[0].Replace("FY ", "").Trim();
                //var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]));
                //if (audits != null)
                tList = tList.Where(a => a.ImplementationEndDate != null && (a.ImplementationEndDate.Value.Month >= 4 ? a.ImplementationEndDate.Value.ToString("yy") + "-" + (a.ImplementationEndDate.Value.Year + 1).ToString().Substring(2, 2) == quater[0] : (a.ImplementationEndDate.Value.Year - 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy") == quater[0]));

            }
            //if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            //{
            //    string[] quater = dashboardQuery.Quarter.Split(',');
            //    quater[1] = quater[1].Replace("FY ", "").Trim();
            //    var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]) && a.Quater == quater[0]);
            //    if (audits != null)
            //        tList = tList.Where(p => p.AuditId == audits.AuditId);
            //}
            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var returnList = new List<ActionPlanChartModel>();
            int highPlans = 0;
            int mediumPlans = 0;
            int lowPlans = 0;
            int criticalPlans = 0;

            int notDueHighPlans = 0;
            int notDueMediumPlans = 0;
            int notDueLowPlans = 0;
            int notDuecriticalPlans = 0;

            int implementedHighPlans = 0;
            int implementedMediumPlans = 0;
            int implementedLowPlans = 0;
            int implementedcriticalPlans = 0;

            int notImplementedHighPlans = 0;
            int notImplementedMediumPlans = 0;
            int notImplementedLowPlans = 0;
            int notImplementedcriticalPlans = 0;

            int partImplementedHighPlans = 0;
            int partImplementedMediumPlans = 0;
            int partImplementedLowPlans = 0;
            int partImplementedcriticalPlans = 0;
            //var allPlans = followupRepo.GetWithInclude<DraftReport>(p=>p.AuditExist==false);

            //foreach (var itemActionPlan in tList)
            //{
            //var allPlans = followupRepo.GetWithInclude<DraftReport>(a => a.AuditId == scopeAudit.AuditId);
            criticalPlans = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Critical && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Critical).Count();
            highPlans = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.High && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.High).Count();
            lowPlans = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Low && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Low).Count();
            mediumPlans = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Medium && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Medium).Count();

            var notDuePlans = tList
                     .Where(a => (a.Status.ToLower().ToString() == "pending") && a.ImplementationEndDate != null &&
                     Convert.ToDateTime(a.ImplementationEndDate) > DateTime.Now);

            if (notDuePlans != null)
            {
                notDuecriticalPlans += notDuePlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Critical && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Critical).Count();
                notDueHighPlans += notDuePlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.High && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.High).Count();
                notDueMediumPlans += notDuePlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Medium && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Medium).Count();
                notDueLowPlans += notDuePlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Low && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Low).Count();
            }

            var implementedPlans = tList.Where(a => a.Status.ToLower().Trim() == "completed");

            if (implementedPlans != null)
            {
                implementedcriticalPlans += implementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Critical && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Critical).Count();
                implementedHighPlans += implementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.High && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.High).Count();
                implementedMediumPlans += implementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Medium && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Medium).Count();
                implementedLowPlans += implementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Low && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Low).Count();
            }

            var notImplementedPlans = tList.Where(a => a.Status.ToLower().Trim() == "pending");

            if (notImplementedPlans != null)
            {
                notImplementedcriticalPlans += notImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Critical && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Critical).Count();
                notImplementedHighPlans += notImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.High && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.High).Count();
                notImplementedMediumPlans += notImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Medium && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Medium).Count();
                notImplementedLowPlans += notImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Low && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Low).Count();
            }

            var partImplementedPlans = tList.Where(a => a.Status.ToLower().Trim() == "inprogress");

            if (partImplementedPlans != null)
            {
                partImplementedcriticalPlans += partImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Critical && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Critical).Count();
                partImplementedHighPlans += partImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.High && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.High).Count();
                partImplementedMediumPlans += partImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Medium && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Medium).Count();
                partImplementedLowPlans += partImplementedPlans.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Low && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Low).Count();
            }
            //}

            if (criticalPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Total",
                    value = highPlans + mediumPlans + lowPlans + criticalPlans,
                    open = highPlans + mediumPlans + lowPlans,
                    stepValue = criticalPlans,
                    color = "#8B0000",
                    displayValue = "Critical : " + criticalPlans
                });
            if (highPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Total",
                    value = highPlans + mediumPlans + lowPlans,
                    open = mediumPlans + lowPlans,
                    stepValue = highPlans,
                    color = "#FF0000",
                    displayValue = "High : " + highPlans
                });

            if (mediumPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Total",
                    value = mediumPlans + lowPlans,
                    open = lowPlans,
                    stepValue = mediumPlans,
                    color = "#FFFF00",
                    displayValue = "Medium : " + mediumPlans
                });

            if (lowPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Total",
                    value = lowPlans,
                    open = 0,
                    stepValue = lowPlans,
                    color = "#00FF00",
                    displayValue = "Low : " + lowPlans
                });

            if (notDuecriticalPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Due",
                    value = notDueHighPlans + notDueMediumPlans + notDueLowPlans + notDuecriticalPlans,
                    open = notDueHighPlans + notDueMediumPlans + notDueLowPlans,
                    stepValue = notDuecriticalPlans,
                    color = "#8B0000",
                    displayValue = "Critical : " + notDuecriticalPlans
                });
            if (notDueHighPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Due",
                    value = notDueHighPlans + notDueMediumPlans + notDueLowPlans,
                    open = notDueMediumPlans + notDueLowPlans,
                    stepValue = notDueHighPlans,
                    color = "#FF0000",
                    displayValue = "High : " + notDueHighPlans
                });

            if (notDueMediumPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Due",
                    value = notDueMediumPlans + notDueLowPlans,
                    open = notDueLowPlans,
                    stepValue = notDueMediumPlans,
                    color = "#ffe866",
                    displayValue = "Medium : " + notDueMediumPlans
                });

            if (notDueLowPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Due",
                    value = notDueLowPlans,
                    open = 0,
                    stepValue = notDueLowPlans,
                    color = "#00FF00",
                    displayValue = "Low : " + notDueLowPlans
                });

            if (implementedcriticalPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Implemented",
                    value = implementedHighPlans + implementedMediumPlans + implementedLowPlans + implementedcriticalPlans,
                    open = implementedHighPlans + implementedMediumPlans + implementedLowPlans,
                    stepValue = implementedcriticalPlans,
                    color = "#8B0000",
                    displayValue = "Critical : " + implementedcriticalPlans
                });

            if (implementedHighPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Implemented",
                    value = implementedHighPlans + implementedMediumPlans + implementedLowPlans,
                    open = implementedMediumPlans + implementedLowPlans,
                    stepValue = implementedHighPlans,
                    color = "#FF0000",
                    displayValue = "High : " + implementedHighPlans
                });

            if (implementedMediumPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Implemented",
                    value = implementedMediumPlans + implementedLowPlans,
                    open = implementedLowPlans,
                    stepValue = implementedMediumPlans,
                    color = "#FFFF00",
                    displayValue = "Medium : " + implementedMediumPlans
                });

            if (implementedLowPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Implemented",
                    value = implementedLowPlans,
                    open = 0,
                    stepValue = implementedLowPlans,
                    color = "#00FF00",
                    displayValue = "Low : " + implementedLowPlans
                });

            if (notImplementedcriticalPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Implemented",
                    value = notImplementedHighPlans + notImplementedMediumPlans + notImplementedLowPlans + notImplementedcriticalPlans,
                    open = notImplementedHighPlans + notImplementedMediumPlans + notImplementedLowPlans,
                    stepValue = notImplementedcriticalPlans,
                    color = "#8B0000",
                    displayValue = "Critical : " + notImplementedcriticalPlans
                });
            if (notImplementedHighPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Implemented",
                    value = notImplementedHighPlans + notImplementedMediumPlans + notImplementedLowPlans,
                    open = notImplementedMediumPlans + notImplementedLowPlans,
                    stepValue = notImplementedHighPlans,
                    color = "#FF0000",
                    displayValue = "High : " + notImplementedHighPlans
                });

            if (notImplementedMediumPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Implemented",
                    value = notImplementedMediumPlans + notImplementedLowPlans,
                    open = notImplementedLowPlans,
                    stepValue = notImplementedMediumPlans,
                    color = "#FFFF00",
                    displayValue = "Medium : " + notImplementedMediumPlans
                });

            if (notImplementedLowPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Not Implemented",
                    value = notImplementedLowPlans,
                    open = 0,
                    stepValue = notImplementedLowPlans,
                    color = "#00FF00",
                    displayValue = "Low : " + notImplementedLowPlans
                });
            if (partImplementedcriticalPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Partially Implemented",
                    value = partImplementedHighPlans + partImplementedMediumPlans + partImplementedLowPlans + partImplementedcriticalPlans,
                    open = partImplementedHighPlans + partImplementedMediumPlans + partImplementedLowPlans,
                    stepValue = partImplementedcriticalPlans,
                    color = "#8B0000",
                    displayValue = "Critical : " + partImplementedcriticalPlans
                });

            if (partImplementedHighPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Partially Implemented",
                    value = partImplementedHighPlans + partImplementedMediumPlans + partImplementedLowPlans,
                    open = partImplementedMediumPlans + partImplementedLowPlans,
                    stepValue = partImplementedHighPlans,
                    color = "#FF0000",
                    displayValue = "High : " + partImplementedHighPlans
                });

            if (partImplementedMediumPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Partially Implemented",
                    value = partImplementedMediumPlans + partImplementedLowPlans,
                    open = partImplementedLowPlans,
                    stepValue = partImplementedMediumPlans,
                    color = "#FFFF00",
                    displayValue = "Medium : " + partImplementedMediumPlans
                });

            if (partImplementedLowPlans > 0)
                returnList.Add(new ActionPlanChartModel()
                {
                    category = "Partially Implemented",
                    value = partImplementedLowPlans,
                    open = 0,
                    stepValue = partImplementedLowPlans,
                    color = "#00FF00",
                    displayValue = "Low : " + partImplementedLowPlans
                });
            //return ResponseOK(sampleData());
            return ResponseOK(returnList);
        }

        [HttpGet("GetQuickView")]
        public ActionResult GetQuickView()
        {
            var actionPlanList = new MongoGenericRepository<FollowUp>(_dbsetting);
            var scopeScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);

            var tList = actionPlanList.GetAllWithInclude<Audit, DraftReport, Location, ProcessLocationMapping, ScopeAndSchedule>();

            if (tList == null)
                return ResponseNotFound();

            var dashboardQuery = new DashboardQuery();
            dashboardQuery.Location = Request.Query["Divison"];
            dashboardQuery.Audit = Request.Query["Audit"];
            dashboardQuery.Quarter = Request.Query["Period"];
            var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            if (dashboardQuery.Location != "")
            {
                var objLocation = locationRepo.GetFirst(p => p.Id == dashboardQuery.Location);
                if (objLocation != null)
                {
                    tList = tList.Where(p => (p.LocationID == dashboardQuery.Location));
                    //tList = tList.Where(p => (p.LocationID == dashboardQuery.Location) || (p.ProcessLocationMappingId != null && (p.ProcessLocationMapping.Locations.Any(t => t.ToString() == dashboardQuery.Location))));
                }
            }
            if (dashboardQuery.Audit != "")
            {
                //var auditRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                //var objAudit = auditRepo.GetFirst(p => p.Id == dashboardQuery.Audit);
                //if (objAudit != null)
                tList = tList.Where(p => p.ProcessLocationMappingId != null && p.ProcessLocationMappingId == dashboardQuery.Audit);
            }
            if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            {
                string[] quater = dashboardQuery.Quarter.Split(',');
                quater[0] = quater[0].Replace("FY ", "").Trim();
                //var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]));
                //if (audits != null)
                tList = tList.Where(a => a.ImplementationEndDate != null && (a.ImplementationEndDate.Value.Month >= 4 ? a.ImplementationEndDate.Value.ToString("yy") + "-" + (a.ImplementationEndDate.Value.Year + 1).ToString().Substring(2, 2) == quater[0] : (a.ImplementationEndDate.Value.Year - 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy") == quater[0]));

            }
            //if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            //{
            //    string[] quater = dashboardQuery.Quarter.Split(',');
            //    quater[1] = quater[1].Replace("FY ", "").Trim();
            //    var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]) && a.Quater == quater[0]);
            //    if (audits != null)
            //        tList = tList.Where(p => p.AuditId == audits.AuditId);
            //}

            var dnRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var rootcauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
            var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);

            var high = 0;

            foreach (var item in tList)
            {
                if (item.DraftReportId != null)
                {
                    item.DraftReport = draftReportRepo.GetWithInclude<DiscussionNote>(a => a.Id == item.DraftReportId).FirstOrDefault();

                    if (item.DraftReport != null)
                        item.DraftReport.DiscussionNote = dnRepo.GetWithInclude<RiskType>(a => a.Id == item.DraftReport.DiscussionNoteID).FirstOrDefault();
                }

                if (item.AuditId != null)
                    item.Audit = scopeScheduleRepo.GetFirst(a => a.AuditId == item.AuditId);
            }

            var audits = auditRepo.GetAll();
            var actionPlans = tList.Where(a => a.Audit != null);

            var returnObject = new QuickViewGroup()
            {
                High = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.High && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.High).Count(),
                Low = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Low && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Low).Count(),
                Medium = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Medium && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Medium).Count(),
                Critical = tList.Where(a => (a.DraftReport != null && a.DraftReport.ObservationGrading == ObservationGradingEnum.Critical && a.ImplementationEndDate != null) || a.ObservationGrading == ObservationGradingEnum.Critical).Count(),

                Total = tList.Count(),
                TodayDue = tList.Where(x => (x.Status.ToLower() == "pending" || x.Status.ToLower() == "inprogress") && (x.ImplementationEndDate != null && x.ImplementationEndDate == DateTime.UtcNow)).Count(),
                NotDue = tList
                .Where(a => (a.Status.ToLower().ToString() == "pending") && a.ImplementationEndDate != null &&
                Convert.ToDateTime(a.ImplementationEndDate) > DateTime.Now).Count(),

                Implemented = tList.Where(p => p.Status == Status.completed).Count(),
                //NotImplemented = tList.Where(p => p.Status == Status.pending && p.ImplementationEndDate != null && (p.ImplementationEndDate > DateTime.UtcNow)).Count(),
                NotImplemented = tList.Where(x => x.Status.ToLower() == Status.inprogress && (x.ImplementationEndDate == null || (x.ImplementationEndDate > DateTime.UtcNow))).Count(),
                PartialyImplemented = tList.Where(x => (x.Status.ToLower() == "pending" || x.Status.ToLower() == "inprogress") && (x.ImplementationEndDate != null && x.ImplementationEndDate < DateTime.UtcNow)).Count(),
                //PartialyImplemented = tList.Where(p => p.Status == Status.inprogress && (p.ImplementationEndDate == null || (p.ImplementationEndDate > DateTime.UtcNow))).Count(),
                //PartialyImplemented = tList.Where(p => p.Status == Status.inprogress && p.ImplementationEndDate != null && (p.ImplementationEndDate > DateTime.UtcNow)).Count(),

                AuditBreakUp = (from a in audits
                                join f in actionPlans on a.Id equals f.Audit.AuditId
                                where f.AuditExist && f.ImplementationEndDate != null
                                group a by a.AuditName into g
                                select new SummaryObject { Name = g.Key, Count = g.Count() }).ToList()
            };

            return ResponseOK(returnObject);
        }

        [HttpGet("GetAuditByDivison/{id}")]
        public ActionResult GetAuditByDivison(string id)
        {
            var AuditRepo = new MongoGenericRepository<Audit>(_dbsetting);

            var list = new List<Audit>();

            if (id != "all")
                list = AuditRepo.GetMany(a => a.Location != null && a.Location.Id == id).ToList();
            else
            {
                var audits = AuditRepo.GetAll().GroupBy(a => a.Id);

                foreach (var audit in audits)
                {
                    var item = AuditRepo.GetByID(audit.Key);

                    if (!list.Exists(a => a.Id == audit.Key))
                        list.Add(item);
                }
            }

            return ResponseOK(list);
        }

        [HttpGet("getperiodsbyaudit/{auditId}")]
        public IActionResult GetPeriodsByAudit(string auditId)
        {
            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);

            var list = new List<AuditsPeriodViewModel>();

            if (auditId != "all")
            {
                var audits = scopeRepo.GetMany(a => a.AuditId == auditId);

                if (audits == null)
                    return ResponseNotFound();

                foreach (var audit in audits)
                {
                    var finYear = (audit.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + audit.AuditEndDate.ToString("yy");

                    list.Add(new AuditsPeriodViewModel()
                    {
                        Quater = audit.Quater,
                        FinancialYear = finYear
                    });
                }
            }
            else
            {
                list.Add(new AuditsPeriodViewModel()
                {
                    Quater = "Q1",
                    FinancialYear = (DateTime.Now.Year - 1).ToString().Substring(2, 2) + "-" + (DateTime.Now.Year).ToString().Substring(2, 2)
                });

                list.Add(new AuditsPeriodViewModel()
                {
                    Quater = "Q2",
                    FinancialYear = (DateTime.Now.AddMonths(3).Year - 1).ToString().Substring(2, 2) + "-" + (DateTime.Now.AddMonths(3).Year).ToString().Substring(2, 2)
                });

                list.Add(new AuditsPeriodViewModel()
                {
                    Quater = "Q3",
                    FinancialYear = (DateTime.Now.AddMonths(6).Year - 1).ToString().Substring(2, 2) + "-" + (DateTime.Now.AddMonths(6).Year).ToString().Substring(2, 2)
                });

                list.Add(new AuditsPeriodViewModel()
                {
                    Quater = "Q4",
                    FinancialYear = (DateTime.Now.AddMonths(9).Year - 1).ToString().Substring(2, 2) + "-" + (DateTime.Now.AddMonths(9).Year).ToString().Substring(2, 2)
                });
            }

            return ResponseOK(list);
        }

        [HttpGet("getactionplansbyperiod")]
        public IActionResult GetActionPlansByPeriod(string scopeId)
        {
            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);

            var scopeAndSchedule = scopeRepo.GetByID(scopeId);

            if (scopeAndSchedule == null)
                return ResponseNotFound();

            var followUpRepo = new MongoGenericRepository<FollowUp>(_dbsetting);

            var actionPlans = followUpRepo.GetMany(a => a.AuditId == scopeAndSchedule.AuditId).ToList();

            foreach (var plan in actionPlans)
            {
                DateTime? planDate = plan.ImplementationEndDate != null ? Convert.ToDateTime(plan.ImplementationEndDate) : DateTime.MinValue;

                if (planDate == DateTime.MinValue ||
                    (planDate <= scopeAndSchedule.AuditStartDate && planDate >= scopeAndSchedule.AuditEndDate))
                    actionPlans.Remove(plan);
            }

            return ResponseOK(actionPlans);
        }

        [HttpGet("getsummarycounts")]
        public IActionResult GetSummaryCounts()
        {
            var actionPlanRepo = new MongoGenericRepository<FollowUp>(_dbsetting);
            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var tList = actionPlanRepo.GetAllWithInclude<Audit, DraftReport, Location, ProcessLocationMapping, ScopeAndSchedule>();

            if (tList == null)
            {
                return ResponseNotFound();
            }
            var dashboardQuery = new DashboardQuery();
            dashboardQuery.Location = Request.Query["Divison"];
            dashboardQuery.Audit = Request.Query["Audit"];
            dashboardQuery.Quarter = Request.Query["Period"];
            var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            //var auditId = Request.Query["Audit"];
            //var Period = Request.Query[""];
            if (dashboardQuery.Location != "")
            {
                var objLocation = locationRepo.GetFirst(p => p.Id == dashboardQuery.Location);
                if (objLocation != null)
                {
                    tList = tList.Where(p => (p.LocationID == dashboardQuery.Location));
                    //tList = tList.Where(p => (p.LocationID == dashboardQuery.Location) || (p.ProcessLocationMappingId != null && (p.ProcessLocationMapping.Locations.Any(t => t.ToString() == dashboardQuery.Location))));
                }
            }
            if (dashboardQuery.Audit != "")
            {
                //var auditRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                //var objAudit = auditRepo.GetFirst(p => p.Id == dashboardQuery.Audit);
                //if (objAudit != null)
                tList = tList.Where(p => p.ProcessLocationMappingId != null && p.ProcessLocationMappingId == dashboardQuery.Audit);
            }
            if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            {
                string[] quater = dashboardQuery.Quarter.Split(',');
                quater[0] = quater[0].Replace("FY ", "").Trim();
                //var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]));
                //if (audits != null)
                tList = tList.Where(a => a.ImplementationEndDate != null && (a.ImplementationEndDate.Value.Month >= 4 ? a.ImplementationEndDate.Value.ToString("yy") + "-" + (a.ImplementationEndDate.Value.Year + 1).ToString().Substring(2, 2) == quater[0] : (a.ImplementationEndDate.Value.Year - 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy") == quater[0]));

            }
            //if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            //{
            //    string[] quater = dashboardQuery.Quarter.Split(',');
            //    quater[1] = quater[1].Replace("FY ", "").Trim();
            //    var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]) && a.Quater == quater[0]);
            //    if (audits != null)
            //        tList = tList.Where(p => p.AuditId == audits.AuditId);
            //}
            var summaryCount = new SummaryCounts()
            {
                //inprogress = tList.Where(x => x.Status.ToLower() == "inprogress" && (x.ImplementationEndDate != null && (x.ImplementationEndDate > DateTime.UtcNow))).Count(),
                //inprogress = tList.Where(x => x.Status.ToLower() == "inprogress" && (x.ImplementationEndDate == null || (x.ImplementationEndDate > DateTime.UtcNow)) && x.RevisedDate == null).Count(),
                inprogress = tList.Where(x => x.Status.ToLower() == "inprogress").Count(),
                pending = tList.Where(x => (x.Status.ToLower() == "pending")).Count(),
               // duetoday = tList.Where(x => (x.Status.ToLower() == "pending" || x.Status.ToLower() == "inprogress") && (x.ImplementationEndDate != null && (x.ImplementationEndDate == DateTime.UtcNow))).Count(),
               // overdue = tList.Where(x => (x.Status.ToLower() == "pending" || x.Status.ToLower() == "inprogress") && (x.ImplementationEndDate != null && (x.ImplementationEndDate < DateTime.UtcNow)) || (x.RevisedDate != null && (x.RevisedDate < DateTime.UtcNow))).Count(),
              //  notDue = tList.Where(x => (x.Status.ToLower().Trim() == "pending") && x.ImplementationEndDate != null && x.ImplementationEndDate > DateTime.UtcNow).Count(),
                completed = tList.Where(x => x.Status.ToLower() == "completed").Count(),
              //  revisedTimeline = tList.Where(x => x.RevisedDate != null).Count(),
                all = tList.Count(),
            };
            return ResponseOK(summaryCount);
        }

        [HttpGet("downloadexcel/{status}")]
        public IActionResult DownloadExcel(string status)
        {
            var tList = fetchActionPlanning(status);

            if (tList == null)
                return ResponseNotFound();

            var dashboardQuery = new DashboardQuery();
            dashboardQuery.Location = Request.Query["Divison"];
            dashboardQuery.Audit = Request.Query["Audit"];
            dashboardQuery.Quarter = Request.Query["Period"];
            var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            //var auditId = Request.Query["Audit"];
            //var Period = Request.Query[""];
            if (dashboardQuery.Location != "")
            {
                var objLocation = locationRepo.GetFirst(p => p.Id == dashboardQuery.Location);
                if (objLocation != null)
                {
                    tList = tList.Where(p => (p.LocationID == dashboardQuery.Location));
                    //tList = tList.Where(p => (p.LocationID == dashboardQuery.Location) || (p.ProcessLocationMappingId != null && (p.ProcessLocationMapping.Locations.Any(t => t.ToString() == dashboardQuery.Location))));
                }
            }
            if (dashboardQuery.Audit != "")
            {
                //var auditRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                //var objAudit = auditRepo.GetFirst(p => p.Id == dashboardQuery.Audit);
                //if (objAudit != null)
                tList = tList.Where(p => p.ProcessLocationMappingId != null && p.ProcessLocationMappingId == dashboardQuery.Audit);
            }
            if (dashboardQuery.Quarter != "" && dashboardQuery.Quarter != "Select Period")
            {
                string[] quater = dashboardQuery.Quarter.Split(',');
                quater[0] = quater[0].Replace("FY ", "").Trim();
                //var audits = scopeRepo.GetFirst(a => a.AuditId == dashboardQuery.Audit && ((a.AuditStartDate.Year - 1).ToString().Substring(2, 2) + "-" + a.AuditEndDate.ToString("yy") == quater[1]));
                //if (audits != null)
                tList = tList.Where(a => a.ImplementationEndDate != null && (a.ImplementationEndDate.Value.Month >= 4 ? a.ImplementationEndDate.Value.ToString("yy") + "-" + (a.ImplementationEndDate.Value.Year + 1).ToString().Substring(2, 2) == quater[0] : (a.ImplementationEndDate.Value.Year - 1).ToString().Substring(2, 2) + "-" + a.ImplementationEndDate.Value.ToString("yy") == quater[0]));

            }

            var rootcauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
            var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var processLocationMappingRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var followupActionPlanRepo = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var LocationRepo = new MongoGenericRepository<Location>(_dbsetting);
            var lstStatus = new List<string>();

            var fileName = "ActionPlans.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ActionPlan");

                Color green = System.Drawing.ColorTranslator.FromHtml("#92D050");
                Color yellow = System.Drawing.ColorTranslator.FromHtml("#FFC001");
                Color blue = System.Drawing.ColorTranslator.FromHtml("#D9D9D9");

                worksheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["A1"].Value = "Audit Number";
                worksheet.Cells["B1"].Value = "Review Area";
                worksheet.Cells["C1"].Value = "Plant";
                worksheet.Cells["D1"].Value = "Root Cause";
                //worksheet.Cells["E1"].Value = "Audit Exists";
                worksheet.Cells["E1"].Value = "Review Qtr";
                worksheet.Cells["F1"].Value = "Obs Number";
                worksheet.Cells["G1"].Value = "Observation Heading";

                worksheet.Cells["H1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["H1:I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["H1"].Value = "Observation Description";
                worksheet.Cells["I1"].Value = "Management response";

                worksheet.Cells["J1:K1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["J1:K1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["J1"].Value = "Agreed action plan";
                worksheet.Cells["K1"].Value = "Risk Rating";

                worksheet.Cells["L1:M1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["L1:M1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["L1"].Value = "Risk Source";
                worksheet.Cells["M1"].Value = "Responsibility (Department)";

                worksheet.Cells["N1:R1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["N1:R1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["N1"].Value = "Responsibility (Person)";
                worksheet.Cells["O1"].Value = "Agreed Timeline";
                worksheet.Cells["P1"].Value = "Revised Date";
                worksheet.Cells["Q1"].Value = "Revision Count";
                worksheet.Cells["R1"].Value = "Updated Date";

                worksheet.Cells["S1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["S1"].Style.Fill.BackgroundColor.SetColor(blue);
                worksheet.Cells["S1"].Value = "Management Implementation Remarks";

                worksheet.Cells["T1:U1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["T1:U1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["T1"].Value = "Auditor Status";
                worksheet.Cells["U1"].Value = "Auditee Status";

                worksheet.Cells["V1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["V1"].Style.Fill.BackgroundColor.SetColor(blue);
                worksheet.Cells["V1"].Value = "Auditor Implementation Remarks";
                var rowIndex = 2;
                try
                {
                    foreach (var plan in tList)
                    {

                        //foreach (var subPlan in followActionPlans)
                        //{
                        if (plan.AuditExist && plan.AuditId != null)
                        {
                            var audit = scopeAndScheduleRepo.GetWithInclude<Location, Audit>(x => x.AuditId == plan.AuditId).FirstOrDefault();
                            if (audit != null)
                            {
                                worksheet.Cells["A" + rowIndex.ToString()].Value = audit.AuditNumber;
                                worksheet.Cells["B" + rowIndex.ToString()].Value = audit.Audit.AuditName;
                            }
                            if (audit.LocationId != null)
                                worksheet.Cells["C" + rowIndex.ToString()].Value = audit.Location.LocationDescription;
                        }
                        else if (plan.ProcessLocationMappingId != null)
                        {
                            var audit = processLocationMappingRepo.GetWithInclude<Location, Audit>(x => x.Id == plan.ProcessLocationMappingId).FirstOrDefault();
                            if (audit != null)
                            {
                                worksheet.Cells["B" + rowIndex.ToString()].Value = audit.AuditName;
                            }
                            worksheet.Cells["A" + rowIndex.ToString()].Value = plan.AuditNumber;
                            var objLocation = LocationRepo.GetFirst(p => p.Id == plan.LocationID);
                            if (objLocation != null)
                                worksheet.Cells["C" + rowIndex.ToString()].Value = objLocation.LocationDescription;
                        }
                        else
                        {
                            worksheet.Cells["A" + rowIndex.ToString()].Value = plan.AuditNumber;
                            //worksheet.Cells["B" + rowIndex.ToString()].Value = plan.AuditName;
                            var objLocation = LocationRepo.GetFirst(p => p.Id == plan.LocationID);
                            if (objLocation != null)
                                worksheet.Cells["C" + rowIndex.ToString()].Value = objLocation.LocationDescription;
                        }
                        //worksheet.Cells["E" + rowIndex.ToString()].Value = plan.AuditExist;
                        worksheet.Cells["D" + rowIndex.ToString()].Value = plan.RootCauses != null ? _CommonServices.getCause(plan.RootCauses.ToList()) : "";
                        if (!string.IsNullOrEmpty(plan.ReviewQtr))
                            worksheet.Cells["E" + rowIndex.ToString()].Value = plan.ReviewQtr.ToUpper();
                        worksheet.Cells["F" + rowIndex.ToString()].Value = plan.ObsNumber;


                        worksheet.Cells["G" + rowIndex.ToString()].Value = plan.ObservationHeading;
                        worksheet.Cells["H" + rowIndex.ToString()].Value = plan.DetailedObservation != null ? UtilityMethods.HtmlToText(plan.DetailedObservation) : null;
                        worksheet.Cells["I" + rowIndex.ToString()].Value = plan.ManagementResponse != null ? UtilityMethods.HtmlToText(plan.ManagementResponse) : null;
                        worksheet.Cells["J" + rowIndex.ToString()].Value = plan.AgreedActionPlan != null ? UtilityMethods.HtmlToText(plan.AgreedActionPlan) : null;
                        worksheet.Cells["K" + rowIndex.ToString()].Value = plan.ObservationGrading;

                        if (plan.RiskTypeId != null)
                        {
                            var riskType = risktypeRepo.GetByID(plan.RiskTypeId);

                            if (riskType != null)
                            {
                                worksheet.Cells["L" + rowIndex.ToString()].Value = riskType.Name;
                            }
                        }
                        worksheet.Cells["M" + rowIndex.ToString()].Value = plan.ResponsibilityDepartment;
                        if (plan.ImplementationOwnerId != null && !string.IsNullOrEmpty(plan.ImplementationOwnerId))
                        {
                            var owner = userRepo.GetByID(plan.ImplementationOwnerId);

                            if (owner != null)
                                worksheet.Cells["N" + rowIndex.ToString()].Value = owner.FirstName + " " + owner.LastName;
                        }
                        if (plan.ImplementationEndDate != null)
                        {
                            worksheet.Cells["O" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells["O" + rowIndex.ToString()].Formula = "=Date(" + Convert.ToDateTime(plan.ImplementationEndDate).Year + "," + Convert.ToDateTime(plan.ImplementationEndDate).Month + "," + Convert.ToDateTime(plan.ImplementationEndDate).Day + ")";
                            //worksheet.Cells["O" + rowIndex.ToString()].Value = Convert.ToDateTime(plan.ImplementationEndDate).ToShortDateString();
                        }
                        if (plan.RevisedDate != null)
                        {
                            worksheet.Cells["p" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells["p" + rowIndex.ToString()].Formula = "=Date(" + Convert.ToDateTime(plan.RevisedDate).Year + "," + Convert.ToDateTime(plan.RevisedDate).Month + "," + Convert.ToDateTime(plan.RevisedDate).Day + ")";
                            //worksheet.Cells["p" + rowIndex.ToString()].Value = Convert.ToDateTime(plan.RevisedDate).ToShortDateString();
                        }
                        worksheet.Cells["Q" + rowIndex.ToString()].Value = plan.RevisionCount;
                        if (plan.UpdatedOn != null)
                        {
                            worksheet.Cells["R" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells["R" + rowIndex.ToString()].Formula = "=Date(" + Convert.ToDateTime(plan.UpdatedOn).Year + "," + Convert.ToDateTime(plan.UpdatedOn).Month + "," + Convert.ToDateTime(plan.UpdatedOn).Day + ")";
                            //worksheet.Cells["R" + rowIndex.ToString()].Value = Convert.ToDateTime(plan.UpdatedOn).ToShortDateString();
                        }
                        var followActionPlans = followupActionPlanRepo.GetMany(a => a.FollowupId == plan.Id).ToList();
                        if (followActionPlans.Count > 0)
                        {
                            var comments = followActionPlans[followActionPlans.Count - 1].Comments;
                            worksheet.Cells["S" + rowIndex.ToString()].Value = comments != null ? UtilityMethods.HtmlToText(comments) : "";
                        }
                        if (!string.IsNullOrEmpty(plan.Status))
                        {
                            plan.Status = plan.Status.ToLower();
                            if (plan.Status == "inprogress" || plan.Status == "in progress")
                                plan.Status = "In Progress";
                            else if (plan.Status == "PENDING TO BE INITIATED" || plan.Status == "pending")
                                plan.Status = "Pending to be initiated";
                            else if (plan.Status == "completed")
                                plan.Status = "Completed";

                            worksheet.Cells["T" + rowIndex.ToString()].Value = plan.Status;
                        }
                        if (!string.IsNullOrEmpty(plan.AuditeeStatus))
                        {
                            plan.AuditeeStatus = plan.AuditeeStatus.ToLower();
                            if (plan.AuditeeStatus == "inprogress" || plan.AuditeeStatus == "in progress")
                                plan.AuditeeStatus = "In Progress";
                            else if (plan.AuditeeStatus == "PENDING TO BE INITIATED" || plan.AuditeeStatus == "pending")
                                plan.AuditeeStatus = "Pending to be initiated";
                            else if (plan.AuditeeStatus == "completed")
                                plan.AuditeeStatus = "Completed";

                            worksheet.Cells["U" + rowIndex.ToString()].Value = plan.AuditeeStatus;
                        }
                        worksheet.Cells["V" + rowIndex.ToString()].Value = plan.ImplementationRemarks;
                        rowIndex++;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                double minimumSize = 20;
                double maximumSize = 50;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpGet("sampledownloadexcel")]
        public IActionResult SampleDownloadExcel()
        {
            var fileName = "ActionPlans.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ActionPlan");

                Color green = System.Drawing.ColorTranslator.FromHtml("#92D050");
                Color yellow = System.Drawing.ColorTranslator.FromHtml("#FFC001");
                Color blue = System.Drawing.ColorTranslator.FromHtml("#D9D9D9");

                worksheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["A1"].Value = "Audit Number";
                worksheet.Cells["B1"].Value = "Review Area";
                worksheet.Cells["C1"].Value = "Plant";
                worksheet.Cells["D1"].Value = "Root Cause";
                //worksheet.Cells["E1"].Value = "Audit Exists";
                worksheet.Cells["E1"].Value = "Review Qtr";
                worksheet.Cells["F1"].Value = "Obs Number";
                worksheet.Cells["G1"].Value = "Observation Heading";

                worksheet.Cells["H1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["H1:I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["H1"].Value = "Observation Description";
                worksheet.Cells["I1"].Value = "Management response";

                worksheet.Cells["J1:K1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["J1:K1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["J1"].Value = "Agreed action plan";
                worksheet.Cells["K1"].Value = "Risk Rating";

                worksheet.Cells["L1:M1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["L1:M1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["L1"].Value = "Risk Source";
                worksheet.Cells["M1"].Value = "Responsibility (Department)";

                worksheet.Cells["N1:P1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["N1:P1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["N1"].Value = "Responsibility (Person)";
                worksheet.Cells["O1"].Value = "Agreed Timeline";
                worksheet.Cells["P1"].Value = "Revised Date";

                worksheet.Cells["Q1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["Q1"].Style.Fill.BackgroundColor.SetColor(blue);
                worksheet.Cells["Q1"].Value = "Management Implementation Remarks";

                worksheet.Cells["R1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["R1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["R1"].Value = "Status";

                #region Added Dropdown in particular column
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("Inputs");
                worksheet2.Cells["A1"].Value = "Location";
                worksheet2.Cells["B1"].Value = "Root Cause";
                worksheet2.Cells["C1"].Value = "Observation Grading";
                worksheet2.Cells["D1"].Value = "Risk Type";
                worksheet2.Cells["E1"].Value = "Status";
                int ObservationGradingIndex = 2, RiskTypeIndex = 2, StatusIndex = 2, LocationIndex = 2, RootCauseIndex = 2;

                #region Location
                var _repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                var lstlocation = _repoLocation.GetMany(p => !string.IsNullOrEmpty(p.LocationDescription));
                if (lstlocation.Count() > 0)
                {
                    var locationColumn = worksheet.DataValidations.AddListValidation("C:C");
                    foreach (var item in lstlocation)
                    {
                        locationColumn.Formula.Values.Add(item.LocationDescription);
                        worksheet2.Cells["A" + LocationIndex.ToString()].Value = item.LocationDescription;
                        LocationIndex++;
                    }
                }
                #endregion
                #region Root Cause
                var _repoRootCause = new MongoGenericRepository<RootCause>(_dbsetting);
                var lstRootCause = _repoRootCause.GetMany(p => !string.IsNullOrEmpty(p.Name));
                if (lstRootCause.Count() > 0)
                {
                    var rootCauseColumn = worksheet.DataValidations.AddListValidation("D:D");
                    foreach (var item in lstRootCause)
                    {
                        rootCauseColumn.Formula.Values.Add(item.Name);
                        worksheet2.Cells["B" + RootCauseIndex.ToString()].Value = item.Name;
                        RootCauseIndex++;
                    }
                }
                #endregion
                #region  ObservationGrading
                string[] lstObservationGrading = Enum.GetNames(typeof(ActionPlanObservationGradingEnum));
                if (lstObservationGrading.Count() > 0)
                {
                    var ObservationGradingColumn = worksheet.DataValidations.AddListValidation("K:K");
                    foreach (var item in lstObservationGrading)
                    {
                        ObservationGradingColumn.Formula.Values.Add(item);
                        worksheet2.Cells["C" + ObservationGradingIndex.ToString()].Value = item;
                        ObservationGradingIndex++;
                    }
                }
                #endregion
                #region RiskType
                var _repoRiskType = new MongoGenericRepository<RiskType>(_dbsetting);
                var lstRiskType = _repoRiskType.GetMany(p => p.Name != null && p.Name != "");
                if (lstRiskType.Count() > 0)
                {
                    var riskTypeColumn = worksheet.DataValidations.AddListValidation("L:L");
                    foreach (var item in lstRiskType)
                    {
                        riskTypeColumn.Formula.Values.Add(item.Name);
                        worksheet2.Cells["D" + RiskTypeIndex.ToString()].Value = item.Name;
                        RiskTypeIndex++;
                    }
                }
                #endregion
                #region Status
                string[] statusEnum = { "In Progress", "Pending to be initiated", "Completed" };
                var Actionstatus = worksheet.DataValidations.AddListValidation("R:R");

                foreach (var item in statusEnum)
                {
                    Actionstatus.Formula.Values.Add(item);
                    worksheet2.Cells["E" + StatusIndex.ToString()].Value = item;
                    StatusIndex++;
                }
                #endregion
                double minimumSize = 20;
                double maximumSize = 50;

                worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns();
                worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                worksheet2.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                #endregion

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpPost("importexcel/{Userid}")]
        public ActionResult ImportExcel(string Userid)
        {
            int ExceptionrowCount = 0;
            int TotalRow = 0;
            StringBuilder sb = new StringBuilder();
            try
            {
                if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                    return ResponseError("formfile is empty");

                var file = Request.Form.Files[0];

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return ResponseError("Not Support file extension");

                var repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                var repoRootCause = new MongoGenericRepository<RootCause>(_dbsetting);
                var repoRiskType = new MongoGenericRepository<RiskType>(_dbsetting);
                var repoUser = new MongoGenericRepository<User>(_dbsetting);
                var repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
                var repoProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
                var repoScopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var LocationRepo = new MongoGenericRepository<Location>(_dbsetting);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        int rowCount = 0;
                        ExcelWorksheet wSheetActionPlans = package.Workbook.Worksheets["ActionPlan"];
                        if (wSheetActionPlans != null)
                        {
                            while (_CommonServices.IsLastRowEmpty(wSheetActionPlans))
                                wSheetActionPlans.DeleteRow(wSheetActionPlans.Dimension.End.Row);
                            rowCount = wSheetActionPlans.Dimension.Rows;
                            TotalRow = rowCount;
                        }

                        for (int row = 2; row <= rowCount; row++)
                        {

                            var observationHeading = wSheetActionPlans.Cells[row, 7].Value != null ? wSheetActionPlans.Cells[row, 7].Value.ToString().Trim() : "";
                            try
                            {

                                //var isExist = true;

                                //var existFollowUp = repoFollowUp.GetFirst(a => a.ObservationHeading.Trim().ToLower() == observationHeading.ToLower());
                                var existFollowUp = new FollowUp();

                                //if (existFollowUp == null)
                                //{
                                //    existFollowUp = new FollowUp();
                                //    isExist = false;
                                //}


                                var auditNumber = wSheetActionPlans.Cells[row, 1].Value != null ? wSheetActionPlans.Cells[row, 1].Value.ToString().Trim().ToLower() : null;
                                var auditName = wSheetActionPlans.Cells[row, 2].Value != null ? wSheetActionPlans.Cells[row, 2].Value.ToString().Trim().ToLower() : null;
                                var plant = wSheetActionPlans.Cells[row, 3].Value != null ? wSheetActionPlans.Cells[row, 3].Value.ToString().Trim().ToLower() : null;
                                var rootCause = wSheetActionPlans.Cells[row, 4].Value != null ? wSheetActionPlans.Cells[row, 4].Value.ToString().Trim().ToLower() : null;
                                var reviewQtr = wSheetActionPlans.Cells[row, 5].Value != null ? wSheetActionPlans.Cells[row, 5].Value.ToString().Trim() : null;
                                var obsNumber = wSheetActionPlans.Cells[row, 6].Value != null ? wSheetActionPlans.Cells[row, 6].Value.ToString().Trim().ToLower() : null;
                                //var implemetnationEndDate = wSheetActionPlans.Cells[row, 9].Value != null ? wSheetActionPlans.Cells[row, 9].Value.ToString().Trim().ToLower() : null;
                                var riskSource = wSheetActionPlans.Cells[row, 12].Value != null ? wSheetActionPlans.Cells[row, 12].Value.ToString().Trim() : null;
                                var responsibilityDepartment = wSheetActionPlans.Cells[row, 13].Value != null ? wSheetActionPlans.Cells[row, 13].Value.ToString().Trim() : null;
                                var auditExists = false;


                                if (auditExists == true)
                                {
                                    var audit = repoScopeAndSchedule.GetFirst(a => a.AuditNumber.Trim().ToLower() == auditNumber);

                                    if (audit != null)
                                        existFollowUp.AuditId = audit.AuditId;
                                    //worksheet.Cells["C" + rowIndex.ToString()].Value = audit.Location.DivisionDescription;
                                    var lstfol = repoFollowUp.GetWithInclude<FollowUp>(p => p.AuditId == audit.AuditId);
                                    //foreach (var item in collection)
                                    //{

                                    //}
                                }
                                else
                                {
                                    existFollowUp.AuditNumber = auditNumber;
                                    if (auditName != null)
                                    {
                                        var objAudit = repoProcessLocationMapping.GetFirst(p => p.AuditName.ToLower() == auditName);
                                        if (objAudit != null)
                                            existFollowUp.ProcessLocationMappingId = objAudit.Id;
                                    }
                                    existFollowUp.AuditName = auditName;
                                    if (plant != null)
                                    {
                                        var objLocation = LocationRepo.GetFirst(p => p.LocationDescription.ToLower() == plant);
                                        if (objLocation != null)
                                            existFollowUp.LocationID = objLocation.Id;
                                    }
                                }
                                if (rootCause != null)
                                {
                                    var objRootCause = repoRootCause.GetFirst(p => p.Name.ToLower() == rootCause);
                                    if (objRootCause != null)
                                    {
                                        List<string> lstRootCause = new List<string>();
                                        lstRootCause.Add(objRootCause.Id);
                                        existFollowUp.RootCauses = lstRootCause;
                                    }
                                }
                                existFollowUp.AuditExist = auditExists;
                                existFollowUp.ReviewQtr = reviewQtr;
                                existFollowUp.ObsNumber = obsNumber;
                                existFollowUp.ObservationHeading = observationHeading;
                                existFollowUp.DetailedObservation = wSheetActionPlans.Cells[row, 8].Value != null ? wSheetActionPlans.Cells[row, 8].Value.ToString().Trim() : null;

                                //double impEndDate = wSheetActionPlans.Cells[row, 9].Value != null ? double.Parse(wSheetActionPlans.Cells[row, 9].Value.ToString().Trim()) : 0;
                                //if (impEndDate != 0)
                                //{
                                //    DateTime endDate = DateTime.FromOADate(impEndDate);
                                //    existFollowUp.ImplementationEndDate = endDate.ToLocalTime();
                                //}
                                var riskRating = wSheetActionPlans.Cells[row, 11].Value != null ? wSheetActionPlans.Cells[row, 11].Value.ToString().ToLower().Trim() : null;
                                switch (riskRating)
                                {
                                    case "high": existFollowUp.ObservationGrading = ObservationGradingEnum.High; break;
                                    case "medium": existFollowUp.ObservationGrading = ObservationGradingEnum.Medium; break;
                                    case "low": existFollowUp.ObservationGrading = ObservationGradingEnum.Low; break;
                                    case "critical": existFollowUp.ObservationGrading = ObservationGradingEnum.Critical; break;
                                    case "repeat": existFollowUp.ObservationGrading = ObservationGradingEnum.Repeat; break;
                                }

                                var riskType = wSheetActionPlans.Cells[row, 12].Value != null ? wSheetActionPlans.Cells[row, 12].Value.ToString().Trim() : null;
                                if (riskType != null)
                                {
                                    var riskTypeEntity = repoRiskType.GetFirst(a => a.Name.ToLower() == riskType.ToLower());
                                    if (riskTypeEntity != null)
                                        existFollowUp.RiskTypeId = riskTypeEntity.Id;
                                }
                                existFollowUp.ResponsibilityDepartment = responsibilityDepartment;

                                var status = wSheetActionPlans.Cells[row, 18].Value != null ? wSheetActionPlans.Cells[row, 18].Value.ToString().Trim().ToLower() : null;
                                if (status != null)
                                {
                                    status = status.Replace(" ", "");
                                    if (status.ToLower() == "inprogress")
                                        existFollowUp.Status = "inprogress";
                                    else if (status.Contains("pending"))
                                        existFollowUp.Status = "pending";
                                    else if (status.ToLower() == "completed")
                                        existFollowUp.Status = "completed";
                                }
                                var managementResponse = wSheetActionPlans.Cells[row, 9].Value != null ? wSheetActionPlans.Cells[row, 9].Value.ToString().Trim() : null;
                                existFollowUp.ManagementResponse = managementResponse;

                                var agreedActionPlan = wSheetActionPlans.Cells[row, 10].Value != null ? wSheetActionPlans.Cells[row, 10].Value.ToString().Trim() : null;
                                existFollowUp.AgreedActionPlan = agreedActionPlan;

                                var responsibilityPerson = wSheetActionPlans.Cells[row, 14].Value != null ? wSheetActionPlans.Cells[row, 14].Value.ToString().Trim() : null;
                                if (responsibilityPerson != null)
                                {
                                    var owner = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsibilityPerson.ToLower());
                                    if (owner != null)
                                    {
                                        if (!string.IsNullOrEmpty(owner.Id))
                                            existFollowUp.ImplementationOwnerId = owner.Id;
                                    }
                                }
                                var impEndDate = wSheetActionPlans.Cells[row, 15].Value != null ? wSheetActionPlans.Cells[row, 15].Value.ToString().Trim() : null;
                                if (impEndDate != null)
                                {
                                    object value = wSheetActionPlans.Cells[row, 15].Value;

                                    if (value != null)
                                    {
                                        if (value is double)
                                        {
                                            DateTime endDate = DateTime.FromOADate((double)value);
                                            existFollowUp.ImplementationEndDate = endDate.ToLocalTime();
                                        }
                                        else
                                        {
                                            DateTime dt;
                                            DateTime.TryParse((string)impEndDate, out dt);
                                            existFollowUp.ImplementationEndDate = dt.ToLocalTime();
                                        }
                                    }
                                }
                                var revisedDate = wSheetActionPlans.Cells[row, 16].Value != null ? wSheetActionPlans.Cells[row, 16].Value.ToString().Trim() : null;
                                if (revisedDate != null)
                                {
                                    object value = wSheetActionPlans.Cells[row, 16].Value;

                                    if (value != null)
                                    {
                                        if (value is double)
                                        {
                                            DateTime endDate = DateTime.FromOADate((double)value);
                                            existFollowUp.RevisedDate = endDate.ToLocalTime();
                                        }
                                        else
                                        {
                                            DateTime dt;
                                            DateTime.TryParse((string)revisedDate, out dt);
                                            existFollowUp.RevisedDate = dt.ToLocalTime();
                                        }
                                    }
                                }
                                #region
                                //var revisedDate = wSheetActionPlans.Cells[row, 15].Value != null ? wSheetActionPlans.Cells[row, 15].Value.ToString().Trim() : null;
                                //if (revisedDate != null)
                                //{
                                //    existFollowUp.RevisedDate = DateTime.ParseExact(revisedDate.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                //}

                                //if (isExist)
                                //{
                                //    existFollowUp.UpdatedBy = Userid;
                                //    repoFollowUp.Update(existFollowUp);
                                //}
                                //else
                                //{
                                #endregion
                                if (existFollowUp.ImplementationEndDate == null || existFollowUp.ImplementationOwnerId == null || existFollowUp.LocationID == null || existFollowUp.RiskTypeId == null || existFollowUp.Status == null || existFollowUp.Status == "" || existFollowUp.ImplementationOwnerId == "" || existFollowUp.ImplementationOwnerId == null)
                                {
                                    ExceptionrowCount++;
                                    sb.Append(row + ",");
                                    continue;
                                }
                                existFollowUp.CreatedBy = Userid;
                                existFollowUp.IsByImport = true;
                                repoFollowUp.Insert(existFollowUp);
                                //Sub Plan
                                var newSubPlan = new FollowupActionPlan();
                                var implementationRemarks = wSheetActionPlans.Cells[row, 17].Value != null ? wSheetActionPlans.Cells[row, 17].Value.ToString().Trim() : null;
                                if (implementationRemarks != null)
                                {
                                    newSubPlan.Comments = implementationRemarks;
                                }
                                newSubPlan.RevisedDate = existFollowUp.RevisedDate;
                                newSubPlan.FollowupId = existFollowUp.Id;
                                repoFollowupActionPlan.Insert(newSubPlan);
                                //}
                                #region Subplan

                                //var isExist = repoFollowupActionPlan.Exists(x => x.Id == existFollowUp.Id);



                                //if (agreedActionPlan != null)
                                //{
                                //    int index = 0;
                                //    string[] lstActionPlan = agreedActionPlan.Split("SubPlan:");
                                //    foreach (string strActinPlan in lstActionPlan)
                                //    {
                                //        if (strActinPlan != "" && !string.IsNullOrEmpty(strActinPlan))
                                //        {
                                //            var newSubPlan = new FollowupActionPlan();

                                //            newSubPlan.ActionPlan = strActinPlan;

                                //            if (revisedDate != null)
                                //            {
                                //                if (!revisedDate.Contains("SubPlan:"))
                                //                {
                                //                    double rdate = double.Parse(revisedDate);
                                //                    DateTime endDate = DateTime.FromOADate(rdate);
                                //                    existFollowUp.RevisedDate = endDate.ToLocalTime();
                                //                }
                                //                else
                                //                {
                                //                    string[] lstrevisedDate = revisedDate.Split("SubPlan:");
                                //                    var CountReviesedDate = lstrevisedDate.Count();
                                //                    if (CountReviesedDate > index)
                                //                    {
                                //                        if (lstrevisedDate[index] != null && lstrevisedDate[index] != "")
                                //                        {
                                //                            newSubPlan.RevisedDate = DateTime.ParseExact(lstrevisedDate[index].Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                //                        }
                                //                    }
                                //                }
                                //            }
                                //            if (managementResponse != null)
                                //            {
                                //                string[] lstComments = managementResponse.Split("SubPlan:");
                                //                var CountComment = lstComments.Count();
                                //                if (CountComment > index)
                                //                {
                                //                    if (lstComments[index] != null && lstComments[index] != "")
                                //                    {
                                //                        newSubPlan.Comments = VJLiabraries.UtilityMethods.HtmlToText(lstComments[index]);
                                //                    }
                                //                }
                                //            }
                                //            if (responsibilityPerson != null)
                                //            {
                                //                string[] lstOwner = responsibilityPerson.Split("SubPlan:");
                                //                var CountOwner = lstOwner.Count();
                                //                if (CountOwner > index)
                                //                {
                                //                    if (lstOwner[index] != null && lstOwner[index] != "")
                                //                    {
                                //                        var owner = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == lstOwner[index].ToLower());
                                //                        if (owner != null)
                                //                            newSubPlan.ImplementationOwnerId = owner.Id;
                                //                    }
                                //                }
                                //            }
                                //            newSubPlan.FollowupId = existFollowUp.Id;
                                //            newSubPlan.CreatedBy = Userid;
                                //            repoFollowupActionPlan.Insert(newSubPlan);
                                //        }
                                //        index++;
                                //    }
                                //}
                                //End Sub Plan
                                #endregion
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "ActionPlanning/ImportExcel()");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "ActionPlanning/ImportExcel()");
            }
            return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
        }

        [HttpPost("multiplefiles")]
        public async Task<IActionResult> UploadMultipleFiles(IFormFile[] files)
        {
            if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                return ResponseError("formfile is empty");

            try
            {
                List<AuditFiles> returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", "auditeeactionplanstatus");

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                foreach (var item in files)
                {
                    var res = await _IDocumentUpload.UploadToWebRoot(item, newPath);

                    AuditFiles auditFiles = new AuditFiles();
                    auditFiles.OriginalFileName = item.FileName;
                    auditFiles.UploadedDatetime = DateTime.Now;
                    auditFiles.UploadedFileName = res;
                    auditFiles.ModuleName = "auditeeactionplanstatus";
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
                string newPath = Path.Combine("manageaudits", auditId, "auditeeactionplan", id);

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var auditFiles = repo.GetMany(a => a.AuditId == auditId && a.ModuleId == id && a.ModuleName == "auditeeactionplan");

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
        public IActionResult SendEmail([FromBody] Service.Utilities.ActionPlanEmailModel objActionPlanEmailModel)
        {
            try
            {
                var actionPlanList = new MongoGenericRepository<FollowUp>(_dbsetting);
                var lstFollowup = actionPlanList.GetAllWithInclude<Audit, DraftReport, Location, ProcessLocationMapping, ScopeAndSchedule>();
                List<FollowUp> tList = new List<FollowUp>();
                //Ignore unmapped status 
                foreach (var itemID in objActionPlanEmailModel.IDs)
                {
                    var objFollowup = lstFollowup.FirstOrDefault(p => p.Id == itemID);
                    if (objFollowup != null)
                        tList.Add(objFollowup);
                }
                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "ActionPlanning.html");
                var emailModel = new Service.Utilities.EmailModel();
                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }
                emailBody = emailBody
                         .Replace("#MailBody#", (objActionPlanEmailModel.MailBody));
                //emailModel.ToEmail = new List<string> { "baldev@silverwebbuzz.com" };
                emailModel.ToEmail = objActionPlanEmailModel.ToEmail;
                emailModel.CcEmail = objActionPlanEmailModel.CcEmail;
                emailModel.Subject = "Action Planning";
                emailModel.MailBody = emailBody.ToString();
                var file = _CommonServices.DownloadExcelAttachment(tList);
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

    }
}