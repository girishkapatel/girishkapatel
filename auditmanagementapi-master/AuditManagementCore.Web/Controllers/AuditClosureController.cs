using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Wkhtmltopdf.NetCore.Options;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditClosureController : VJBaseGenericAPIController<AuditClosure>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;

        public AuditClosureController(IMongoGenericRepository<AuditClosure> api, IMongoDbSettings dbsetting, CommonServices cs) : base(api)
        {
            _dbsetting = dbsetting;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] AuditClosure e)
        {
            if (e == null || e.AuditId == null) return ResponseBad("Audit Closure object is null or AuditId is null.");
            var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());
            if (isExist)
            {
                return ResponseError("Audit Closure already exists for Audit");
            }
            var auditClosure = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "AuditClosure", "Manage Audits | Audit Closure | Add", "Added AuditClosure");
            return auditClosure;
        }
        public override ActionResult Put([FromBody] AuditClosure e)
        {
            if (e == null || e.AuditId == null)
                return ResponseBad("Audit Closure object is null or AuditId is null.");

            var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());

            if (isExist) {
                var auditClosure = base.Put(e);
                //Activity Log
                var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
                var audit = _repoAudit.GetFirst(p => p.Id == e.AuditId);
                var auditname = audit != null ? audit.AuditName : "";
                _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "AuditClosure("+ auditname + ")", "AuditClosure", "Manage Audits | Audit Closure | Edit", "Updated AuditClosure");
                return auditClosure;
            }
            else
            {
                var auditClosure = base.Post(e);
                //Activity Log
                _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "AuditClosure", "Manage Audits | Audit Closure | Add", "Added AuditClosure");
                return auditClosure;
            }
              
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Audit>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            foreach (var item in tList)
            {
                populateAuditClosure(item);
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
            tList = populateAuditClosure(tList);
            return ResponseOK(tList);
        }

        [HttpGet("getauditclousrereportconsideration/{id}")]
        public IActionResult GetAuditClousreReportConsideration(string id)
        {
            //var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetMany(a => a.AuditId == id);

            if (draftReports == null)
                return ResponseNotFound();

            var reportConsiRepo = new MongoGenericRepository<ReportConsideration>(_dbsetting);
            var reportConsi = reportConsiRepo.GetAll();

            if (reportConsi == null)
                return ResponseNotFound();

            var list = new List<AuditClosureRecommendations>();

            foreach (var item in reportConsi)
            {
                var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

                if (isExists != null)
                    isExists.Value += draftReports.Where(a => a.ReportConsiderations != null && a.ReportConsiderations.Contains(item.Id)).Count();
                else
                {
                    list.Add(new AuditClosureRecommendations()
                    {
                        Name = item.Name,
                        Value = draftReports.Where(a => a.ReportConsiderations != null && a.ReportConsiderations.Contains(item.Id)).Count()
                    });
                }
            }

            return ResponseOK(list);
        }

        [HttpGet("getauditclousrerecommendation/{id}")]
        public IActionResult GetAuditClousreRecommendation(string id)
        {
            //var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetMany(a => a.AuditId == id);
            //var discussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            //var lstDiscussionNote = discussionNoteRepo.GetMany(a => a.AuditId == id);

            if (draftReports == null)
                return ResponseNotFound();

            var recommendationRepo = new MongoGenericRepository<Recommendation>(_dbsetting);
            var recommendations = recommendationRepo.GetAll();

            if (recommendations == null)
                return ResponseNotFound();

            var list = new List<AuditClosureRecommendations>();

            foreach (var item in recommendations)
            {
                var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

                if (isExists != null)
                    isExists.Value += draftReports.Where(a => a.Recommendations != null && a.Recommendations.Contains(item.Id)).Count();
                else
                {
                    list.Add(new AuditClosureRecommendations()
                    {
                        Name = item.Name,
                        Value = draftReports.Where(a => a.Recommendations != null && a.Recommendations.Contains(item.Id)).Count()
                    });
                }
            }

            //foreach (var item in recommendations)
            //{
            //    var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

            //    if (isExists != null)
            //        isExists.Value += lstDiscussionNote.Where(a => a.Recommendations != null && a.Recommendations.Contains(item.Id)).Count();
            //    else
            //    {
            //        list.Add(new AuditClosureRecommendations()
            //        {
            //            Name = item.Name,
            //            Value = lstDiscussionNote.Where(a => a.Recommendations != null && a.Recommendations.Contains(item.Id)).Count()
            //        });
            //    }
            //}
            return ResponseOK(list);
        }

        [HttpGet("getauditclousrerootcauses/{id}")]
        public IActionResult GetAuditClousreRootCauses(string id)
        {
            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetMany(a => a.AuditId == id);
            //var discussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            //var lstDiscussionNote = discussionNoteRepo.GetMany(a => a.AuditId == id);

            if (draftReports == null)
                return ResponseNotFound();

            var rootCauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
            var rootCauses = rootCauseRepo.GetAll();

            if (rootCauses == null)
                return ResponseNotFound();

            var list = new List<AuditClosureRecommendations>();
            foreach (var item in rootCauses)
            {
                var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

                if (isExists != null)
                    isExists.Value += draftReports.Where(a => a.RootCauses != null && a.RootCauses.Contains(item.Id)).Count();
                else
                {
                    list.Add(new AuditClosureRecommendations()
                    {
                        Name = item.Name,
                        Value = draftReports.Where(a => a.RootCauses != null && a.RootCauses.Contains(item.Id)).Count()
                    });
                }
            }
            //foreach (var item in rootCauses)
            //{
            //    var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

            //    if (isExists != null)
            //        isExists.Value += lstDiscussionNote.Where(a => a.RootCauses != null && a.RootCauses.Contains(item.Id)).Count();
            //    else
            //    {
            //        list.Add(new AuditClosureRecommendations()
            //        {
            //            Name = item.Name,
            //            Value = lstDiscussionNote.Where(a => a.RootCauses != null && a.RootCauses.Contains(item.Id)).Count()
            //        });
            //    }
            //}
            return ResponseOK(list);
        }

        [HttpGet("getauditclousreimpactmaster/{id}")]
        public IActionResult GetAuditClousreImpactMaster(string id)
        {
            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetMany(a => a.AuditId == id);
            //var discussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            //var lstDiscussionNote = discussionNoteRepo.GetMany(a => a.AuditId == id);
            if (draftReports == null)
                return ResponseNotFound();

            var impactMasterRepo = new MongoGenericRepository<ImpactMaster>(_dbsetting);
            var impactMasters = impactMasterRepo.GetAll();

            if (impactMasters == null)
                return ResponseNotFound();

            var list = new List<AuditClosureRecommendations>();

            foreach (var item in impactMasters)
            {
                var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

                if (isExists != null)
                    isExists.Value += draftReports.Where(a => a.Impacts != null && a.Impacts.Contains(item.Id)).Count();
                else
                {
                    list.Add(new AuditClosureRecommendations()
                    {
                        Name = item.Name,
                        Value = draftReports.Where(a => a.Impacts != null && a.Impacts.Contains(item.Id)).Count()
                    });
                }
            }

            //foreach (var item in impactMasters)
            //{
            //    var isExists = list.Where(a => a.Name == item.Name).FirstOrDefault();

            //    if (isExists != null)
            //        isExists.Value += lstDiscussionNote.Where(a => a.Impacts != null && a.Impacts.Contains(item.Id)).Count();
            //    else
            //    {
            //        list.Add(new AuditClosureRecommendations()
            //        {
            //            Name = item.Name,
            //            Value = lstDiscussionNote.Where(a => a.Impacts != null && a.Impacts.Contains(item.Id)).Count()
            //        });
            //    }
            //}
            return ResponseOK(list);
        }

        private AuditClosure populateAuditClosure(AuditClosure auditClosure)
        {
            var result = _api.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId).FirstOrDefault();
            if (result == null)
                return new AuditClosure();

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId);

            //var discussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            //var lstDiscussionNote = discussionNoteRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId);

            //Reinitializing all the dictionaries
            auditClosure.init();

            double potentialSavings = 0;
            double realisedSavings = 0;
            double leakage = 0;

            int systemImprovement = 0;
            int redFlag = 0;
            int leadingPractices = 0;

            populateReportConsideration(auditClosure);
            populateRecommendation(auditClosure);
            populateRootCause(auditClosure);
            populateImpact(auditClosure);

            //foreach (var itemDiscussionNote in lstDiscussionNote)
            //{
            //    if (itemDiscussionNote.PotentialSaving != null && !double.IsNaN(Convert.ToDouble(itemDiscussionNote.PotentialSaving)) && Convert.ToDouble(itemDiscussionNote.PotentialSaving)>0)
            //        potentialSavings += Convert.ToDouble(itemDiscussionNote.PotentialSaving);
            //    //potentialSavings += Convert.ToDouble(item.PotentialSaving);

            //    if (itemDiscussionNote.RealisedSaving != null && !double.IsNaN(Convert.ToDouble(itemDiscussionNote.RealisedSaving)) && Convert.ToDouble(itemDiscussionNote.RealisedSaving) > 0)
            //        realisedSavings += Convert.ToDouble(itemDiscussionNote.RealisedSaving);
            //    //realisedSavings += Convert.ToDouble(item.RealisedSaving);

            //    if (itemDiscussionNote.Leakage != null && !double.IsNaN(Convert.ToDouble(itemDiscussionNote.Leakage)) && Convert.ToDouble(itemDiscussionNote.Leakage) > 0)
            //        leakage += Convert.ToDouble(itemDiscussionNote.Leakage);
            //    //leakage += Convert.ToDouble(item.Leakage);

            //    if (itemDiscussionNote.isSystemImprovement)
            //        systemImprovement++;

            //    if (itemDiscussionNote.isRedFlag)
            //        redFlag++;

            //    if (itemDiscussionNote.isLeadingPractices)
            //        leadingPractices++;
            //}
            foreach (var item in draftReports)
            {
                auditClosure.NumberOfObservation = incrementKey(item.ObservationGrading.ToString(), auditClosure.NumberOfObservation);
                //auditClosure.Recommendation = incrementKey(item.Recommendation, auditClosure.Recommendation);
                //auditClosure.RootCause = incrementKey(item.RootCause, auditClosure.RootCause);
                //auditClosure.ReportConsideration = incrementKey(item.ReportConsideration, auditClosure.ReportConsideration);

                if (item.PotentialSaving != null && !double.IsNaN(Convert.ToDouble(item.PotentialSaving)) && Convert.ToDouble(item.PotentialSaving) > 0)
                    potentialSavings+= Convert.ToDouble(item.PotentialSaving);
                //potentialSavings += Convert.ToDouble(item.PotentialSaving);

                if (item.RealisedSaving != null && !double.IsNaN(Convert.ToDouble(item.RealisedSaving)) && Convert.ToDouble(item.RealisedSaving) > 0)
                    realisedSavings += Convert.ToDouble(item.RealisedSaving);
                //realisedSavings += Convert.ToDouble(item.RealisedSaving);

                if (item.Leakage != null && !double.IsNaN(Convert.ToDouble(item.Leakage)) && Convert.ToDouble(item.Leakage) > 0)
                    leakage += Convert.ToDouble(item.Leakage);
                //leakage += Convert.ToDouble(item.Leakage);

                if (item.isSystemImprovement)
                    systemImprovement++;

                if (item.isRedFlag)
                    redFlag++;

                if (item.isLeadingPractices)
                    leadingPractices++;
            }

            auditClosure.SavingPotential.PotentialsSavings = potentialSavings.ToString();
            auditClosure.SavingPotential.Leakage = leakage.ToString();
            auditClosure.SavingPotential.RealisedSavings = realisedSavings.ToString();

            auditClosure.ProcessImprovement.SystemImprovement = systemImprovement.ToString();
            auditClosure.ProcessImprovement.RedFlag = redFlag.ToString();
            auditClosure.ProcessImprovement.LeadingPractices = leadingPractices.ToString();

            populateControlHealthScorecard(auditClosure);

            return auditClosure;
        }

        private void populateControlHealthScorecard(AuditClosure auditClosure)
        {
            var detailRepo = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            var details = detailRepo.GetWithInclude<RACMAuditProcedure>(x => x.AuditId == auditClosure.AuditId);

            double designMarks = 0;
            double oeMarks = 0;
            double calculatedDesign = 0;
            double calculateOE = 0;

            foreach (var item in details)
            {
                if (item.DesignMarks > 0)
                {
                    designMarks++;
                    calculatedDesign += (item.RACMAuditProcedure.Control.DesignMarks * (item.RACMAuditProcedure.Control.DesignEffectiveness / 100));
                }

                if (item.OEMarks > 0)
                {
                    oeMarks++;
                    calculateOE += (item.RACMAuditProcedure.Control.OEMarks * (item.RACMAuditProcedure.Control.OEEffectiveness / 100));
                }
            }

            List<RACMAuditProcedure> racmAuditProcedure = GetRacmAuditProcedureByAudit(auditClosure.AuditId);

            //foreach (var racmAP in racmAuditProcedure)
            //{
            //    Control control = racmAP.Control;
            //    //designMarks = designMarks + control.DesignMarks;
            //    calculatedDesign = calculatedDesign + (control.DesignMarks * (control.DesignEffectiveness / 100));

            //    //oeMarks = oeMarks + control.OEMarks;
            //    calculateOE = calculateOE + (control.OEMarks * (control.OEEffectiveness / 100));
            //}

            var ControlsIdentifiedAndTested = racmAuditProcedure != null ? racmAuditProcedure.GroupBy(a => a.Control).Count() : 0;
            var DesignControlRating = ((calculatedDesign * 100) / designMarks);
            var OEControlRating = ((calculateOE * 100) / oeMarks);
            var ControlsWithObservation = this.racmWithObservationCount(auditClosure.AuditId);

            auditClosure.ControlHealthScoreCard.ControlsIdentifiedAndTested = ControlsIdentifiedAndTested.ToString();
            auditClosure.ControlHealthScoreCard.DesignControlRating = double.IsNaN(designMarks) ? "0" : designMarks.ToString();
            auditClosure.ControlHealthScoreCard.OEControlRating = double.IsNaN(oeMarks) ? "0" : oeMarks.ToString();
            auditClosure.ControlHealthScoreCard.ControlsWithObservation = ControlsWithObservation.ToString();
        }

        private int racmWithObservationCount(string AuditId)
        {
            var discussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var discussionNotes = discussionNoteRepo.GetMany(x => x.AuditId == AuditId);

            HashSet<string> racmIds = new HashSet<string>();

            foreach (var item in discussionNotes)
            {
                foreach (var racmId in item.RACM_Ids)
                {
                    racmIds.Add(racmId);
                }

            }
            return racmIds.Count;
        }

        private Dictionary<string, int> incrementKey(string key, Dictionary<string, int> keyValueMap)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                key = Regex.Replace(key, @"\s", "");
            }

            if (key == null)
            {
                return keyValueMap;
            }
            else if (keyValueMap.ContainsKey(key))
            {
                keyValueMap[key]++;
            }
            else
            {
                keyValueMap.Add(key, 1);
            }
            return keyValueMap;
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            foreach (var item in tList)
            {
                populateAuditClosure(item);
            }

            return ResponseOK(tList);
        }

        [HttpGet("GetOverallPerformance/{id}")]
        public IActionResult GetOverallPerformance(string id)
        {
            var repoRAP = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
            var repoRAPD = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);

            var lstAuditProcedure = repoRAP.GetWithInclude<Audit, ProcessL1, ProcessL2, RACM>(p => p.AuditId == id).ToList();
            var lstOverallperformance = new List<OverallPerformance>();

            foreach (var item in lstAuditProcedure)
            {
                int weightage = 0;
                double designControlRating = 0;
                double oeControlRating = 0;
                var lstProcedureDetails = repoRAPD.GetMany(p => p.RACMAuditProcedureId == item.Id);
                var procedureDetailsCount = lstProcedureDetails != null ? lstProcedureDetails.Count() : 0;
                int controlswithObservation = lstProcedureDetails != null ? lstProcedureDetails.Where(p => p.DesignMarks == 1).Count() : 0;
                //int controlwithnoException = lstProcedureDetails != null ? lstProcedureDetails.Where(p => p.OEMarks == 0).Count() : 0;
                if (lstProcedureDetails != null)
                {
                    foreach (var itemProcedureDetail in lstProcedureDetails)
                    {
                        int riskValue = 0;
                        switch (item.Risk.Rating)
                        {
                            case "Critical":
                                riskValue = (int)Weightage.Critical;
                                break;
                            case "High":
                                riskValue = (int)Weightage.High;
                                break;
                            case "Medium":
                                riskValue = (int)Weightage.Medium;
                                break;
                            case "Low":
                                riskValue = (int)Weightage.Low;
                                break;
                            default:
                                break;
                        }
                        designControlRating += riskValue * (1 - (itemProcedureDetail.DesignEffectiveness / 100));
                        oeControlRating += riskValue * (1 - itemProcedureDetail.OEEffectiveness / 100);
                        weightage += riskValue;
                    }
                }
                if (item.ProcessL1 != null  || item.BusinessCycle != null)
                {
                    OverallPerformance objOverallPerformance = new OverallPerformance()
                    {
                        Process = item.ProcessL1 != null?item.ProcessL1.Name:item.BusinessCycle.Name,
                        ControlsIdentifiedTested = procedureDetailsCount,
                        DesignControlRating = designControlRating * 100 / weightage,
                        ControlswithObservation = controlswithObservation,
                        ControlwithnoException = procedureDetailsCount - controlswithObservation,
                        OEControlRating = oeControlRating * 100 / weightage,
                    };
                    lstOverallperformance.Add(objOverallPerformance);
                }
            }

            return ResponseOK(lstOverallperformance);
        }

        public List<RACMAuditProcedure> GetRacmAuditProcedureByAudit(string auditId)
        {
            var racmAuditProcedureRepo = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
            var tList = racmAuditProcedureRepo.GetWithInclude<Audit>(x => x.AuditId == auditId);
            var racmAuditProcedureList = new List<RACMAuditProcedure>(tList);
            if (tList == null)
            {
                return racmAuditProcedureList;
            }

            return PopulateRacmAuditProcedureDetails(racmAuditProcedureList);

        }

        public List<RACMAuditProcedure> PopulateRacmAuditProcedureDetails(List<RACMAuditProcedure> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            foreach (var item in tList)
            {
                foreach (var temp in GetRACMAuditProcedureDetails(item.Id))
                {
                    item.RACMAuditProcedureDetails.Add(temp);
                }
            }
            return tList;
        }

        public IQueryable<RACMAuditProcedureDetails> GetRACMAuditProcedureDetails(string RACMAuditProcedureId)
        {
            var detailRepo = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            var details = detailRepo.GetWithInclude<RACMAuditProcedure>(x => x.RACMAuditProcedureId == RACMAuditProcedureId);

            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in details)
            {
                item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
            }
            return details;
        }

        public AuditClosure populateReportConsideration(AuditClosure auditClosure)
        {
            var repConsList = new List<CommonValues>();

            var reportConsiRepo = new MongoGenericRepository<ReportConsideration>(_dbsetting);
            var reportConsiderations = reportConsiRepo.GetAll();

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId && x.ReportConsiderations != null);

            foreach (var item in reportConsiderations)
            {
                var value = draftReports.Where(a => a.ReportConsiderations.Contains(item.Id)).Count();

                repConsList.Add(new CommonValues() { name = item.Name, value = value });
            }

            auditClosure.ReportConsideration = repConsList;

            return auditClosure;
        }

        public AuditClosure populateRecommendation(AuditClosure auditClosure)
        {
            var recommList = new List<CommonValues>();

            var recommendationRepo = new MongoGenericRepository<Recommendation>(_dbsetting);
            var recommendations = recommendationRepo.GetAll();

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId && x.Recommendations != null);

            foreach (var item in recommendations)
            {
                var value = draftReports.Where(a => a.Recommendations.Contains(item.Id)).Count();

                recommList.Add(new CommonValues() { name = item.Name, value = value });
            }

            auditClosure.Recommendation = recommList;

            return auditClosure;
        }

        public AuditClosure populateRootCause(AuditClosure auditClosure)
        {
            var rcList = new List<CommonValues>();

            var rootCauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
            var rootCauses = rootCauseRepo.GetAll();

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId && x.RootCauses != null);

            foreach (var item in rootCauses)
            {
                var value = draftReports.Where(a => a.RootCauses.Contains(item.Id)).Count();

                rcList.Add(new CommonValues() { name = item.Name, value = value });
            }

            auditClosure.RootCause = rcList;

            return auditClosure;
        }

        public AuditClosure populateImpact(AuditClosure auditClosure)
        {
            var impactList = new List<CommonValues>();

            var impactMasterRepo = new MongoGenericRepository<ImpactMaster>(_dbsetting);
            var impactMaster = impactMasterRepo.GetAll();

            var draftReportRepo = new MongoGenericRepository<DraftReport>(_dbsetting);
            var draftReports = draftReportRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId && x.Impacts != null);

            var DiscussionNoteRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var lstDiscussionNote = DiscussionNoteRepo.GetWithInclude<AuditClosure>(x => x.AuditId == auditClosure.AuditId && x.Impacts != null);

            foreach (var item in impactMaster)
            {
                var value = draftReports.Where(a => a.Impacts.Contains(item.Id)).Count();

                impactList.Add(new CommonValues() { name = item.Name, value = value });
            }
            foreach (var item in impactMaster)
            {
                var value = lstDiscussionNote.Where(a => a.Impacts.Contains(item.Id)).Count();

                impactList.Add(new CommonValues() { name = item.Name, value = value });
            }
            auditClosure.Impact = impactList;
            return auditClosure;
        }
    }
}