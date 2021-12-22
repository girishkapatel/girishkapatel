using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using static AuditManagementCore.Models.RACMAuditProcedure;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RACMAuditProcedureController : VJBaseGenericAPIController<RACMAuditProcedure>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RACMAuditProcedureController(IMongoGenericRepository<RACMAuditProcedure> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] RACMAuditProcedure e)
        {
            //&& x.Risk.Title == e.Risk.Title,&& x.Control.Title.ToLower() == e.Control.Title.ToLower()
            var isExist = _api.Exists(x => x.AuditId == e.AuditId
                && x.Risk.RiskId == e.Risk.RiskId
                && x.Control.ControlId == e.Control.ControlId);

            if (isExist)
            {
                return AlreadyExistResponseError("RACM Audit Procedure with RACM number : " + e.Id + " already exists.");
            }
            base.Post(e);

            //Update Knowledge Library
            InsertRACM(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Risk.RiskId, "RACMAuditProcedure", "Manage Audits | Audit Execution | RACM | Add", "Added RACM");
            return ResponseOK(e);
        }
        public void InsertRACM(RACMAuditProcedure e)
        {
            try
            {
                var repoRACM = new MongoGenericRepository<RACM>(_dbsetting);
                var isExists = true;
                var exists = repoRACM.GetFirst(x => x.RACMnumber == e.RACMnumber && x.Risk.RiskId == e.Risk.RiskId && x.Control.ControlId == e.Control.ControlId);
                if (exists == null)
                {
                    isExists = false;
                    exists = new RACM();
                }
                exists.BusinessCycleId = e.BusinessCycleId;
                exists.ProcessL1Id = e.ProcessL1Id;
                exists.ProcessL2Id = e.ProcessL2Id;
                exists.RACMnumber = e.RACMnumber;
                exists.RiskId = e.RiskId;
                exists.Risk = e.Risk;
                exists.ControlId = e.ControlId;
                exists.Control = e.Control;
                exists.CreatedBy = e.CreatedBy;

                if (isExists)
                    repoRACM.Update(exists);
                else
                    repoRACM.Insert(exists);
            }
            catch (Exception)
            {


            }
        }
        public override ActionResult Put([FromBody] RACMAuditProcedure e)
        {
            var isExist = _api.Exists(x => x.Id != e.Id && x.AuditId == e.AuditId
               && x.Risk.RiskId == e.Risk.RiskId
               && x.Control.ControlId == e.Control.ControlId);

            if (isExist)
            {
                return AlreadyExistResponseError("RACM Audit Procedure with RACM number : " + e.Id + " already exists.");
            }

            var racmaduitprocedure = base.Put(e);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Risk.RiskId, "RACMAuditProcedure", "Manage Audits | Audit Execution | RACM | Edit", "Updated RACM");
            return racmaduitprocedure;
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("RACM object is null");
                var objRACM = _api.GetFirst(x => x.Id == id);

                if (objRACM == null)
                {
                    return ResponseError("RACM does not exists");
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objRACM.Risk.RiskId, "RACMAuditProcedure", "Manage Audits | Audit Execution | RACM | Delete", "Deleted RACM");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        [HttpGet("getsummary/{id}")]
        public ActionResult GetSummary(string id)
        {
            var racmSummary = new RACMSummary();

            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            tList = PopulateRacmAuditProcedureDetails(tList);

            var critical = 0;
            var high = 0;
            var medium = 0;
            var low = 0;

            var notStarted = 0;
            var inProgress = 0;
            var inReview = 0;
            var completed = 0;

            foreach (var item in tList)
            {
                if (item.Risk != null)
                {
                    if (item.Risk.Rating != null && item.Risk.Rating.ToLower() == "critical")
                        critical++;
                    if (item.Risk.Rating != null && item.Risk.Rating.ToLower() == "high")
                        high++;

                    if (item.Risk.Rating != null && item.Risk.Rating.ToLower() == "medium")
                        medium++;

                    if (item.Risk.Rating != null && item.Risk.Rating.ToLower() == "low")
                        low++;
                }

                if (item.RACMAuditProcedureDetails != null && item.RACMAuditProcedureDetails.Count > 0)
                {
                    foreach (var racmDetail in item.RACMAuditProcedureDetails)
                    {
                        if (racmDetail.Status != null && racmDetail.Status.ToLower() == "inprogress")
                            inProgress++;
                        else if (racmDetail.Status != null && racmDetail.Status.ToLower() == "inreview")
                            inReview++;
                        else if (racmDetail.Status != null && racmDetail.Status.ToLower() == "completed")
                            completed++;
                        else
                            notStarted++;
                    }
                }
            }

            racmSummary.Critical = critical;
            racmSummary.High = high;
            racmSummary.Medium = medium;
            racmSummary.Low = low;
            racmSummary.ProcNotStarted = notStarted;
            racmSummary.ProcInProgress = inProgress;
            racmSummary.ProcInReview = inReview;
            racmSummary.ProcCompleted = completed;
            racmSummary.Automated = tList.Count(a => a.Control.Nature == "Automated");
            racmSummary.Manual = tList.Count(a => a.Control.Nature == "Manual");
            racmSummary.ITDependent = tList.Count(a => a.Control.Nature == "IT Dependent");

            return ResponseOK(racmSummary);
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
            {
                return ResponseNotFound();
            }

            tList = PopulateRacmAuditProcedureDetails(tList);

            return ResponseOK(tList);
        }

        [HttpGet("GetByAuditAndRiskRating/{id}/{option}/{controlNature}")]
        public ActionResult GetByAuditAndRiskRating(string id, string option, string controlNature)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
                return ResponseNotFound();

            if (option.Trim() != "" && option.Trim().ToLower() != "all")
                tList = tList.Where(x => x.Risk.Rating.ToLower() == option.ToLower());

            if (controlNature.Trim() != "" && controlNature.Trim().ToLower() != "all")
                tList = tList.Where(x => x.Control.Nature.ToLower() == controlNature.ToLower());

            tList = PopulateRacmAuditProcedureDetails(tList);

            return ResponseOK(tList);
        }

        public IQueryable<RACMAuditProcedure> PopulateRacmAuditProcedureDetails(IQueryable<RACMAuditProcedure> tList)
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

        [HttpGet("GetIneffectiveRACMs/{id}")]
        public ActionResult GetIneffectiveRACMs(String id)                           //Added by Vandana to get Ineffective controls
        {
            var tList = _api.GetAllWithInclude<RACMAuditProcedure, Procedure>();
            var racmdetailRepo = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            List<RACMAuditProcedure> ineffectivePro = new List<RACMAuditProcedure>();
            foreach (var item in tList)
            {
                var inactiveCount = racmdetailRepo.GetCount(x => x.RACMAuditProcedureId == item.Id && x.AuditId == id);
                if (inactiveCount > 0)
                {
                    ineffectivePro.Add(item);
                }
            }

            return ResponseOK(ineffectivePro);
        }

        [HttpPost("addFromRACMLibrary")]
        public ActionResult addFromEybmLibrary([FromBody] RACMAuditwisePost e)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var repoRACM = new MongoGenericRepository<RACM>(_dbsetting);
            var repoRACMProcedure = new MongoGenericRepository<RACMProcedure>(_dbsetting);
            var repoRACMAuditProcedure = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);

            foreach (var item in e.RACMIDs)
            {
                var isExist = _api.Exists(x => x.RACMId == item && x.AuditId == e.AuditID);

                if (!isExist)
                {
                    var racm = repoRACM.GetFirst(y => y.Id == item);
                    var isExistRACM = _api.Exists(x => x.AuditId == e.AuditID
                  && x.Risk.RiskId == racm.Risk.RiskId
                  && x.Control.ControlId == racm.Control.ControlId);
                    if (isExistRACM)
                    {
                        return AlreadyExistResponseError("RACM Audit Procedure already exists.");
                    }
                    var racmAuditProcedure = new RACMAuditProcedure();
                    racmAuditProcedure.AuditId = e.AuditID;
                    racmAuditProcedure.RiskId = racm.RiskId;
                    racmAuditProcedure.ControlId = racm.ControlId;
                    racmAuditProcedure.ProcessL1Id = racm.ProcessL1Id;
                    racmAuditProcedure.ProcessL2Id = racm.ProcessL2Id;
                    racmAuditProcedure.BusinessCycleId = racm.BusinessCycleId;
                    racmAuditProcedure.Risk = racm.Risk;
                    racmAuditProcedure.Control = racm.Control;

                    base.Post(racmAuditProcedure);

                    var racmProcedure = repoRACMProcedure.GetMany(a => a.RACMId == racm.Id);

                    if (racmProcedure != null)
                    {
                        foreach (var subItem in racmProcedure)
                        {
                            var racmAuditProcedureDetails = new RACMAuditProcedureDetails();
                            racmAuditProcedureDetails.RACMAuditProcedureId = racmAuditProcedure.Id;
                            racmAuditProcedureDetails.ProcedureStartDate = subItem.ProcedureStartDate;
                            racmAuditProcedureDetails.ProcedureEndDate = subItem.ProcedureEndDate;
                            racmAuditProcedureDetails.ResponsibilityId = subItem.ResponsibilityId;
                            racmAuditProcedureDetails.ReviewerId = subItem.ReviewerId;
                            racmAuditProcedureDetails.Status = "NotStarted";
                            racmAuditProcedureDetails.AuditId = e.AuditID;
                            racmAuditProcedureDetails.Procedure = subItem.Procedure;
                            racmAuditProcedureDetails.DesignMarks = subItem.DesignMarks;
                            racmAuditProcedureDetails.DesignEffectiveness = subItem.DesignEffectiveness;
                            racmAuditProcedureDetails.OEMarks = subItem.OEMarks;
                            racmAuditProcedureDetails.OEEffectiveness = subItem.OEEffectiveness;

                            repoRACMAuditProcedure.Insert(racmAuditProcedureDetails);
                        }
                    }
                }
                else
                {

                }

            }

            return ResponseOK(200);
        }

        [HttpGet("downloadexcel/{auditId}")]
        public IActionResult DownloadExcel(string auditId)
        {
            try
            {
                var tList = _api.GetWithInclude<Audit, BusinessCycle, ProcessL1, ProcessL2>(x => x.AuditId == auditId);
                if (tList == null)
                    return ResponseNotFound();

                tList = PopulateRacmAuditProcedureDetails(tList);
                var repoUser = new MongoGenericRepository<User>(_dbsetting);
                var repoScopeSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var fileName = "AuditRACM.xlsx";
                var memoryStream = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet wSheetRACMs = package.Workbook.Worksheets.Add("AuditRACM");
                    Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    wSheetRACMs.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["A1"].Value = "RACM Number*";
                    wSheetRACMs.Cells["B1"].Value = "Business Cycle";
                    wSheetRACMs.Cells["C1"].Value = "Audit Area";
                    wSheetRACMs.Cells["D1"].Value = "Process L1";
                    wSheetRACMs.Cells["E1"].Value = "Process L2";
                    wSheetRACMs.Cells["F1"].Value = "Risk ID";
                    wSheetRACMs.Cells["G1"].Value = "Risk Rating";
                    wSheetRACMs.Cells["H1"].Value = "Risk Description";
                    wSheetRACMs.Cells["I1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["I1"].Value = "Control ID*";
                    wSheetRACMs.Cells["J1"].Value = "Control Type";
                    wSheetRACMs.Cells["K1"].Value = "Control Nature";
                    wSheetRACMs.Cells["L1"].Value = "Control Frequency";
                    wSheetRACMs.Cells["M1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["M1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["M1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["M1"].Value = "Control Owner*";
                    wSheetRACMs.Cells["N1"].Value = "Control Description";

                    wSheetRACMs.Cells["O1"].Value = "Procedure ID";
                    wSheetRACMs.Cells["P1"].Value = "Procedure Title";
                    wSheetRACMs.Cells["Q1"].Value = "Procedure Description";
                    wSheetRACMs.Cells["R1:S1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["R1:S1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["R1:S1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["R1"].Value = "Start Date*";
                    wSheetRACMs.Cells["S1"].Value = "End Date*";
                    wSheetRACMs.Cells["T1"].Value = "Responsibility";
                    wSheetRACMs.Cells["U1"].Value = "Reviewer";
                    var rowIndex = 2;

                    foreach (var item in tList)
                    {
                        wSheetRACMs.Cells["A" + rowIndex.ToString()].Value = item.RACMnumber;
                        wSheetRACMs.Cells["B" + rowIndex.ToString()].Value = item.BusinessCycle.Name;
                        wSheetRACMs.Cells["C" + rowIndex.ToString()].Value = item.AuditArea;
                        wSheetRACMs.Cells["D" + rowIndex.ToString()].Value = item.ProcessL1.Name;
                        wSheetRACMs.Cells["E" + rowIndex.ToString()].Value = item.ProcessL2.Name;
                        wSheetRACMs.Cells["F" + rowIndex.ToString()].Value = item.Risk.RiskId;
                        wSheetRACMs.Cells["G" + rowIndex.ToString()].Value = item.Risk.Rating;
                        wSheetRACMs.Cells["H" + rowIndex.ToString()].Value = item.Risk.Description;
                        wSheetRACMs.Cells["I" + rowIndex.ToString()].Value = item.Control.ControlId;
                        wSheetRACMs.Cells["J" + rowIndex.ToString()].Value = item.Control.Type;
                        wSheetRACMs.Cells["K" + rowIndex.ToString()].Value = item.Control.Nature;
                        wSheetRACMs.Cells["L" + rowIndex.ToString()].Value = item.Control.Frequency;

                        if (item.Control.UserId != null)
                        {
                            var owner = repoUser.GetByID(item.Control.UserId);

                            if (owner != null)
                                wSheetRACMs.Cells["M" + rowIndex.ToString()].Value = owner.FirstName + " " + owner.LastName;
                            else
                                wSheetRACMs.Cells["M" + rowIndex.ToString()].Value = "";
                        }
                        else
                            wSheetRACMs.Cells["N" + rowIndex.ToString()].Value = "";

                        wSheetRACMs.Cells["N" + rowIndex.ToString()].Value = item.Control.Description;

                        if (item.RACMAuditProcedureDetails != null)
                        {
                            foreach (var subItem in item.RACMAuditProcedureDetails)
                            {
                                wSheetRACMs.Cells["O" + rowIndex.ToString()].Value = subItem.Procedure.ProcedureId;
                                wSheetRACMs.Cells["P" + rowIndex.ToString()].Value = subItem.Procedure.ProcedureTitle;
                                wSheetRACMs.Cells["Q" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(subItem.Procedure.ProcedureDesc);

                                wSheetRACMs.Cells["R" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                                wSheetRACMs.Cells["R" + rowIndex.ToString()].Value = Convert.ToDateTime(subItem.ProcedureStartDate).ToShortDateString();

                                wSheetRACMs.Cells["S" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                                wSheetRACMs.Cells["S" + rowIndex.ToString()].Value = Convert.ToDateTime(subItem.ProcedureEndDate).ToShortDateString();

                                if (subItem.ResponsibilityId != null)
                                {
                                    var user = repoUser.GetByID(subItem.ResponsibilityId);

                                    if (user != null)
                                        wSheetRACMs.Cells["T" + rowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                    else
                                        wSheetRACMs.Cells["T" + rowIndex.ToString()].Value = "";
                                }

                                if (subItem.ReviewerId != null)
                                {
                                    var user = repoUser.GetByID(subItem.ReviewerId);

                                    if (user != null)
                                        wSheetRACMs.Cells["U" + rowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                    else
                                        wSheetRACMs.Cells["U" + rowIndex.ToString()].Value = "";
                                }
                                rowIndex++;
                            }
                        }
                    }
                    double minimumSize = 20;
                    double maximumSize = 50;
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns(minimumSize);
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                    wSheetRACMs.Cells["A1:XFD1"].Style.Font.Bold = true;
                    wSheetRACMs.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].Style.WrapText = true;
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
                var fileName = "AuditRACM.xlsx";
                var memoryStream = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet wSheetRACMs = package.Workbook.Worksheets.Add("AuditRACM");
                    Color yellow = ColorTranslator.FromHtml("#FFFF00");
                    wSheetRACMs.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["A1"].Value = "RACM Number*";
                    wSheetRACMs.Cells["B1"].Value = "Business Cycle";
                    wSheetRACMs.Cells["C1"].Value = "Audit Area";
                    wSheetRACMs.Cells["D1"].Value = "Process L1";
                    wSheetRACMs.Cells["E1"].Value = "Process L2";
                    wSheetRACMs.Cells["F1"].Value = "Risk ID";
                    wSheetRACMs.Cells["G1"].Value = "Risk Rating";
                    wSheetRACMs.Cells["H1"].Value = "Risk Description";
                    wSheetRACMs.Cells["I1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["I1"].Value = "Control ID*";
                    wSheetRACMs.Cells["J1"].Value = "Control Type";
                    wSheetRACMs.Cells["K1"].Value = "Control Nature";
                    wSheetRACMs.Cells["L1"].Value = "Control Frequency";
                    wSheetRACMs.Cells["M1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["M1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["M1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["M1"].Value = "Control Owner*";
                    wSheetRACMs.Cells["N1"].Value = "Control Description";

                    wSheetRACMs.Cells["O1"].Value = "Procedure ID";
                    wSheetRACMs.Cells["P1"].Value = "Procedure Title";
                    wSheetRACMs.Cells["Q1"].Value = "Procedure Description";
                    wSheetRACMs.Cells["R1:S1"].Style.Font.Color.SetColor(Color.Red);
                    wSheetRACMs.Cells["R1:S1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wSheetRACMs.Cells["R1:S1"].Style.Fill.BackgroundColor.SetColor(yellow);
                    wSheetRACMs.Cells["R1"].Value = "Start Date*";
                    wSheetRACMs.Cells["S1"].Value = "End Date*";
                    wSheetRACMs.Cells["T1"].Value = "Responsibility";
                    wSheetRACMs.Cells["U1"].Value = "Reviewer";
                    var rowIndex = 2;

                    #region Added Dropdown in particular column
                    ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("Inputs");
                    worksheet2.Cells["A1"].Value = "Business Cycle";
                    worksheet2.Cells["B1"].Value = "Audit Area";
                    worksheet2.Cells["C1"].Value = "Process L1";
                    worksheet2.Cells["D1"].Value = "Process L2";
                    worksheet2.Cells["E1"].Value = "Risk Rating";
                    worksheet2.Cells["F1"].Value = "Control Type";
                    worksheet2.Cells["G1"].Value = "Control Nature";
                    worksheet2.Cells["H1"].Value = "Control Frequency";
                    worksheet2.Cells["I1"].Value = "Control Owner";
                    worksheet2.Cells["J1"].Value = "Responsibility";
                    worksheet2.Cells["K1"].Value = "Reviewer";

                    //worksheet2.Cells["D1"].Value = "Location";
                    int ObservationGradingIndex = 2, ControlTypeIndex = 2, ControlNatureIndex = 2, ControlFrequencyIndex = 2, ControlOwnerIndex = 2,
                        BusinessCycleIndex = 2, ProcessL1Index = 2, ProcessL2Index = 2, AuditAreaIndex = 2, ResponsibilityIndex=2;

                    #region Business Cycle
                    var _repoBusinessCycle = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                    var lstBusinessCycle = _repoBusinessCycle.GetWithInclude<BusinessCycle>(x => x.Name != null).OrderBy(p => p.Name);
                    if (lstBusinessCycle.Count() > 0)
                    {
                        foreach (var item in lstBusinessCycle)
                        {
                            worksheet2.Cells["A" + BusinessCycleIndex.ToString()].Value = item.Name;
                            BusinessCycleIndex++;
                        }
                        var businessCycle = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 2, ExcelPackage.MaxRows, 2));
                        businessCycle.AllowBlank = false;
                        businessCycle.Formula.ExcelFormula = string.Format("'{0}'!$A$2:$A${1}", worksheet2, BusinessCycleIndex);
                    }
                    #endregion
                    #region Audit Area
                    var _repoTOR = new MongoGenericRepository<TOR>(_dbsetting);
                    var lstTOR = _repoTOR.GetMany(p => p.AuditId == auditId);
                    List<string> lstAuditScope = new List<string>();
                    foreach (var itemTor in lstTOR)
                    {
                        if (itemTor.AuditScopes != null)
                        {
                            foreach (var item in itemTor.AuditScopes)
                            {
                                if (item.Areas != null)
                                {
                                    var auditArea = VJLiabraries.UtilityMethods.HtmlToText(item.Areas);
                                    lstAuditScope.Add(auditArea);
                                }
                            }
                        }
                    }

                    if (lstAuditScope.Count() > 0)
                    {
                        foreach (var item in lstAuditScope)
                        {
                            worksheet2.Cells["B" + AuditAreaIndex.ToString()].Value = item;
                            AuditAreaIndex++;
                        }
                        var auditArea = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 3, ExcelPackage.MaxRows, 3));
                        auditArea.AllowBlank = false;
                        auditArea.Formula.ExcelFormula = string.Format("'{0}'!$B$2:$B${1}", worksheet2, AuditAreaIndex);
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
                        var processL1 = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 4, ExcelPackage.MaxRows, 4));
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
                        var processL2 = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 5, ExcelPackage.MaxRows, 5));
                        processL2.AllowBlank = false;
                        processL2.Formula.ExcelFormula = string.Format("'{0}'!$D$2:$D${1}", worksheet2, ProcessL2Index);
                    }
                    #endregion
                    #region Observation Grading
                    string[] lstObservationGrading = Enum.GetNames(typeof(ActionPlanObservationGradingEnum));
                    if (lstObservationGrading.Count() > 0)
                    {
                        foreach (var item in lstObservationGrading)
                        {
                            worksheet2.Cells["E" + ObservationGradingIndex.ToString()].Value = item;
                            ObservationGradingIndex++;
                        }
                        var grading = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 7, ExcelPackage.MaxRows, 7));
                        grading.AllowBlank = false;
                        grading.Formula.ExcelFormula = string.Format("'{0}'!$E$2:$E${1}", worksheet2, ObservationGradingIndex);
                    }
                    #endregion
                    #region Control Type
                    var lstControlType = _CommonServices.getControlType();
                    if (lstControlType.Count() > 0)
                    {
                        foreach (var item in lstControlType)
                        {
                            worksheet2.Cells["F" + ControlTypeIndex.ToString()].Value = item;
                            ControlTypeIndex++;
                        }
                        var controlType = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 10, ExcelPackage.MaxRows, 10));
                        controlType.AllowBlank = false;
                        controlType.Formula.ExcelFormula = string.Format("'{0}'!$F$2:$F${1}", worksheet2, ControlTypeIndex);
                    }
                    #endregion
                    #region Control Nature
                    var lstControlNature = _CommonServices.getControlNature();
                    if (lstControlNature.Count() > 0)
                    {
                        foreach (var item in lstControlNature)
                        {
                            worksheet2.Cells["G" + ControlNatureIndex.ToString()].Value = item;
                            ControlNatureIndex++;
                        }
                        var ControlNature = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 11, ExcelPackage.MaxRows, 11));
                        ControlNature.AllowBlank = false;
                        ControlNature.Formula.ExcelFormula = string.Format("'{0}'!$G$2:$G${1}", worksheet2, ControlNatureIndex);
                    }
                    #endregion
                    #region Control Frequency
                    var lstControlFrequency = _CommonServices.getControlFrequency();
                    if (lstControlFrequency.Count() > 0)
                    {
                        foreach (var item in lstControlFrequency)
                        {
                            worksheet2.Cells["H" + ControlFrequencyIndex.ToString()].Value = item;
                            ControlFrequencyIndex++;
                        }
                        var ControlFrequency = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 12, ExcelPackage.MaxRows, 12));
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
                        var ControlOwner = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 13, ExcelPackage.MaxRows, 13));
                        ControlOwner.AllowBlank = false;
                        ControlOwner.Formula.ExcelFormula = string.Format("'{0}'!$I$2:$I${1}", worksheet2, ControlOwnerIndex);
                    }
                    #endregion
                    #region Responsibility
                    //var respo = repoScopeSchedule.GetWithInclude<Location, Audit>(x => x.AuditId == auditId).ToList().;
                    //var lstRespo = respo.
                    var lstResponsibility = _repoUser.GetWithInclude<Role, Company, User>(x => x.StakeHolder != true).OrderBy(p => p.FirstName);
                    if (lstResponsibility.Count() > 0)
                    {
                        foreach (var item in lstResponsibility)
                        {
                            worksheet2.Cells["J" + ResponsibilityIndex.ToString()].Value = item.FirstName + " " + item.LastName;
                            worksheet2.Cells["K" + ResponsibilityIndex.ToString()].Value = item.FirstName + " " + item.LastName;
                            ResponsibilityIndex++;
                        }
                        var Responsibility = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 20, ExcelPackage.MaxRows, 20));
                        Responsibility.AllowBlank = false;
                        Responsibility.Formula.ExcelFormula = string.Format("'{0}'!$J$2:$J${1}", worksheet2, ResponsibilityIndex);

                        var approver = wSheetRACMs.DataValidations.AddListValidation(ExcelRange.GetAddress(2, 21, ExcelPackage.MaxRows, 21));
                        approver.AllowBlank = false;
                        approver.Formula.ExcelFormula = string.Format("'{0}'!$K$2:$K${1}", worksheet2, ResponsibilityIndex);
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

                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns(minimumSize);
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns(minimumSize, maximumSize);
                    wSheetRACMs.Cells["A1:XFD1"].Style.Font.Bold = true;
                    wSheetRACMs.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].Style.WrapText = true;
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
                int SubPlanExceptionrowCount = 0;
                int SubPlanTotalRow = 0;
                StringBuilder sbSubPlan = new StringBuilder();

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
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["AuditRACM"];
                        var rowCount = worksheet != null ? worksheet.Dimension.Rows : 0;
                        TotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var isExists = true;
                                var riskid = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : null;
                                var riskDesc = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : null;
                                var controlId = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : null;
                                var controlDesc = worksheet.Cells[row, 14].Value != null ? worksheet.Cells[row, 14].Value.ToString().Trim() : null;

                                //var exists = _api.GetFirst(a => a.RACMnumber.Trim() == racmNumber);
                                var exists = _api.GetFirst(x => x.AuditId == auditId
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

                                var auditarea = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : null;
                                if (auditarea != null)
                                {
                                    exists.AuditArea = auditarea;
                                }

                                var processL1Name = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().ToLower().Trim() : null;
                                if (processL1Name != null)
                                {
                                    var count = repoProcessL1.GetCount(a => a.Name.ToLower().Trim() == processL1Name);
                                    if (count > 1)
                                    {
                                        var processL1 = repoProcessL1.GetFirst(a => a.Name.ToLower().Trim() == processL1Name && a.BusinessCycleId == exists.BusinessCycleId);
                                        exists.ProcessL1Id = processL1 != null ? processL1.Id : null;
                                    }
                                    else
                                    {
                                        var processL1 = repoProcessL1.GetFirst(a => a.Name.ToLower().Trim() == processL1Name);
                                        exists.ProcessL1Id = processL1 != null ? processL1.Id : null;
                                    }
                                }
                                var processL2Name = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().ToLower().Trim() : null;
                                if (processL2Name != null)
                                {
                                    var count = repoProcessL2.GetCount(a => a.Name.ToLower().Trim() == processL2Name);
                                    if (count > 1)
                                    {
                                        var processL2 = repoProcessL2.GetFirst(a => a.Name.ToLower().Trim() == processL2Name && a.ProcessL1Id == exists.ProcessL1Id);
                                        exists.ProcessL2Id = processL2 != null ? processL2.Id : null;
                                    }
                                    else
                                    {
                                        var processL2 = repoProcessL2.GetFirst(a => a.Name.ToLower().Trim() == processL2Name);
                                        exists.ProcessL2Id = processL2 != null ? processL2.Id : null;
                                    }
                                }

                                exists.AuditId = auditId;
                                exists.Risk.RiskId = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : null;
                                exists.Risk.Rating = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : "Low";
                                exists.Risk.Description = worksheet.Cells[row, 8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : null;

                                exists.Control.ControlId = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : null;
                                exists.Control.Type = worksheet.Cells[row, 10].Value != null ? worksheet.Cells[row, 10].Value.ToString().Trim() : null;
                                exists.Control.Nature = worksheet.Cells[row, 11].Value != null ? worksheet.Cells[row, 11].Value.ToString().Trim() : null;
                                exists.Control.Frequency = worksheet.Cells[row, 12].Value != null ? worksheet.Cells[row, 12].Value.ToString().Trim() : null;

                                var ownerName = worksheet.Cells[row, 13].Value != null ? worksheet.Cells[row, 13].Value.ToString().ToLower().Trim() : null;
                                if (ownerName != null)
                                {
                                    var owner = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == ownerName);

                                    if (owner != null)
                                        exists.Control.UserId = owner.Id;
                                }

                                exists.Control.Description = worksheet.Cells[row, 14].Value != null ? worksheet.Cells[row, 14].Value.ToString().Trim() : null;

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

                                //Procedure 

                                var procedureId = worksheet.Cells[row, 15].Value != null ? worksheet.Cells[row, 15].Value.ToString().ToLower().Trim() : "";
                                var isExistsprocedure = true;
                                var existsprocedure = repoRACMProcedureDetails
                                    .GetFirst(a => a.RACMAuditProcedureId == exists.Id && a.Procedure.ProcedureId.Trim().ToLower() == procedureId);

                                if (existsprocedure == null)
                                {
                                    isExistsprocedure = false;
                                    existsprocedure = new RACMAuditProcedureDetails();
                                    existsprocedure.Procedure = new Procedure();
                                }
                                existsprocedure.AuditId = auditId;
                                existsprocedure.RACMAuditProcedureId = exists.Id;
                                existsprocedure.Procedure.ProcedureId = worksheet.Cells[row, 15].Value != null ? worksheet.Cells[row, 15].Value.ToString().Trim() : "";
                                existsprocedure.Procedure.ProcedureTitle = worksheet.Cells[row, 16].Value != null ? worksheet.Cells[row, 16].Value.ToString().Trim() : null;
                                existsprocedure.Procedure.ProcedureDesc = worksheet.Cells[row, 17].Value != null ? worksheet.Cells[row, 17].Value.ToString().Trim() : null;

                                var startDate = worksheet.Cells[row, 18].Value != null ? worksheet.Cells[row, 18].Value.ToString().Trim() : null;
                                if (startDate != null)
                                {
                                    object value = worksheet.Cells[row, 18].Value;
                                    if (value != null)
                                    {
                                        if (value is double)
                                        {
                                            DateTime dt = DateTime.FromOADate((double)value);
                                            existsprocedure.ProcedureStartDate = dt.ToLocalTime();
                                        }
                                        else
                                        {
                                            DateTime dt;
                                            DateTime.TryParse((string)value, out dt);
                                            existsprocedure.ProcedureStartDate = dt.ToLocalTime();
                                        }
                                    }
                                }

                                var endDate = worksheet.Cells[row, 19].Value != null ? worksheet.Cells[row, 19].Value.ToString().Trim() : null;
                                if (endDate != null)
                                {
                                    object value = worksheet.Cells[row, 19].Value;
                                    if (value != null)
                                    {
                                        if (value is double)
                                        {
                                            DateTime dt = DateTime.FromOADate((double)value);
                                            existsprocedure.ProcedureEndDate = dt.ToLocalTime();
                                        }
                                        else
                                        {
                                            DateTime dt;
                                            DateTime.TryParse((string)value, out dt);
                                            existsprocedure.ProcedureEndDate = dt.ToLocalTime();
                                        }
                                    }
                                }
                                var responsibility = worksheet.Cells[row, 20].Value != null ? worksheet.Cells[row, 20].Value.ToString().ToLower().Trim() : null;
                                if (responsibility != null)
                                {
                                    var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsibility);
                                    if (user != null)
                                        existsprocedure.ResponsibilityId = user.Id;
                                }

                                var reviewer = worksheet.Cells[row, 21].Value != null ? worksheet.Cells[row, 21].Value.ToString().ToLower().Trim() : null;
                                if (reviewer != null)
                                {
                                    var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == reviewer);
                                    if (user != null)
                                        existsprocedure.ReviewerId = user.Id;
                                }

                                if (isExistsprocedure)
                                    repoRACMProcedureDetails.Update(existsprocedure);
                                else
                                    repoRACMProcedureDetails.Insert(existsprocedure);
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "RACMAuditProcedure/ImportExcel()");
                            }
                        }

                        //ExcelWorksheet wSheetRAMProcedures = package.Workbook.Worksheets["AuditRACMProcedure"];
                        //rowCount = wSheetRAMProcedures.Dimension.Rows;
                        //SubPlanTotalRow = rowCount;
                        //for (int row = 2; row <= rowCount; row++)
                        //{
                        //    try
                        //    {
                        //        var riskId = wSheetRAMProcedures.Cells[row, 1].Value != null ? wSheetRAMProcedures.Cells[row, 1].Value.ToString().ToLower().Trim() : "";
                        //        var racm = _api.GetFirst(a => a.Risk.RiskId.ToLower().Trim() == riskId);
                        //        if (racm != null)
                        //        {
                        //            var procedureId = wSheetRAMProcedures.Cells[row, 1].Value != null ? wSheetRAMProcedures.Cells[row, 2].Value.ToString().ToLower().Trim() : "";
                        //            var isExists = true;
                        //            var exists = repoRACMProcedureDetails
                        //                .GetFirst(a => a.RACMAuditProcedureId == racm.Id && a.Procedure.ProcedureId.Trim().ToLower() == procedureId);

                        //            if (exists == null)
                        //            {
                        //                isExists = false;
                        //                exists = new RACMAuditProcedureDetails();
                        //                exists.Procedure = new Procedure();
                        //            }
                        //            exists.AuditId = auditId;
                        //            exists.RACMAuditProcedureId = racm.Id;
                        //            exists.Procedure.ProcedureId = wSheetRAMProcedures.Cells[row, 2].Value != null ? wSheetRAMProcedures.Cells[row, 2].Value.ToString().Trim() : null;
                        //            exists.Procedure.ProcedureTitle = wSheetRAMProcedures.Cells[row, 3].Value != null ? wSheetRAMProcedures.Cells[row, 3].Value.ToString().Trim() : null;
                        //            exists.Procedure.ProcedureDesc = wSheetRAMProcedures.Cells[row, 4].Value != null ? wSheetRAMProcedures.Cells[row, 4].Value.ToString().Trim() : null;

                        //            var startDate = wSheetRAMProcedures.Cells[row, 5].Value != null ? wSheetRAMProcedures.Cells[row, 5].Text.ToString().Trim() : null;
                        //            if (startDate != null)
                        //                exists.ProcedureStartDate = Convert.ToDateTime(startDate).ToLocalTime();

                        //            var endDate = wSheetRAMProcedures.Cells[row, 6].Value != null ? wSheetRAMProcedures.Cells[row, 6].Text.ToString().Trim() : null;
                        //            if (endDate != null)
                        //                exists.ProcedureEndDate = Convert.ToDateTime(endDate).ToLocalTime();

                        //            var responsibility = wSheetRAMProcedures.Cells[row, 7].Value != null ? wSheetRAMProcedures.Cells[row, 7].Value.ToString().ToLower().Trim() : null;
                        //            if (responsibility != null)
                        //            {
                        //                var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsibility);
                        //                if (user != null)
                        //                    exists.ResponsibilityId = user.Id;
                        //            }

                        //            var reviewer = wSheetRAMProcedures.Cells[row, 8].Value != null ? wSheetRAMProcedures.Cells[row, 8].Value.ToString().ToLower().Trim() : null;
                        //            if (reviewer != null)
                        //            {
                        //                var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == reviewer);
                        //                if (user != null)
                        //                    exists.ReviewerId = user.Id;
                        //            }

                        //            if (isExists)
                        //                repoRACMProcedureDetails.Update(exists);
                        //            else
                        //                repoRACMProcedureDetails.Insert(exists);
                        //        }
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        SubPlanExceptionrowCount++;
                        //        sbSubPlan.Append(row + ",");
                        //        _CommonServices.SendExcepToDB(e, "SubRACMAuditProcedure/ImportExcel()");
                        //    }
                        //}
                    }
                }
                var RACMAuditProcdeduremaster = new
                {
                    ExcptionCount = ExceptionrowCount,
                    ExcptionRowNumber = sb.ToString(),
                    TotalRow = TotalRow - 1,

                    //SubPlanExcptionCount = SubPlanExceptionrowCount,
                    //SubPlanExcptionRowNumber = sbSubPlan.ToString(),
                    //SubPlanTotalRow = SubPlanTotalRow - 1,
                };
                return ResponseOK(RACMAuditProcdeduremaster);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "RACMAuditProcedure/ImportExcel()");

            }
            return ResponseOK(new object[0]);

        }

        [HttpGet("getracmnumber/{auditId}")]
        public ActionResult GetRACMNumber(string auditId)
        {
            var racmSummary = new RACMAuditAutoNumber();
            racmSummary.RACMNumber = new List<string>();
            racmSummary.RiskId = new List<string>();
            racmSummary.ControlId = new List<string>();
            racmSummary.ProcedureId = new List<string>();
            int RacmNumber = 01, RiskNumber = 01, ControlNumber = 01, ProcedureNumber = 01;
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == auditId).OrderByDescending(p => p.CreatedOn);
            var repoPLM = new MongoGenericRepository<Audit>(_dbsetting);
            var repoProcdureDetails = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            if (tList.Count() == 0)
            {
                var objAudit = repoPLM.GetFirst(x => x.Id == auditId);
                if (objAudit != null)
                {
                    racmSummary.RACMNumber.Add(objAudit.AuditName + ".RACM.01");
                    racmSummary.RiskId.Add(objAudit.AuditName + ".R.01");
                    racmSummary.ControlId.Add(objAudit.AuditName + ".C.01");
                    racmSummary.ProcedureId.Add(objAudit.AuditName + ".P.01");
                }
            }
            else
            {
                var objAudit = repoPLM.GetFirst(x => x.Id == auditId);
                if (objAudit != null)
                {
                    foreach (var item in tList)
                    {
                        var objProcedureDetails = repoProcdureDetails.GetFirst(p => p.RACMAuditProcedureId == item.Id);
                        racmSummary.RACMNumber.Add(item.RACMnumber);
                        if (item.Risk != null)
                            racmSummary.RiskId.Add(item.Risk.RiskId);
                        if (item.Control != null)
                            racmSummary.ControlId.Add(item.Control.ControlId);
                        racmSummary.ProcedureId.Add(objProcedureDetails == null ? objAudit.AuditName + ".P.01" : objProcedureDetails.ProcedureId);
                        RacmNumber++;
                    }
                    if (objAudit != null)
                    {
                        racmSummary.RACMNumber.Add(objAudit.AuditName + ".RACM.0" + RacmNumber);
                        racmSummary.RiskId.Add(objAudit.AuditName + ".R.0" + RacmNumber);
                        racmSummary.ControlId.Add(objAudit.AuditName + ".C.0" + RacmNumber);
                        racmSummary.ProcedureId.Add(objAudit.AuditName + ".P.0" + RacmNumber);
                    }
                }
            }
            return ResponseOK(racmSummary);
        }
        [HttpGet("getprocedure/{auditId}/{totalProcedure}")]
        public ActionResult GetProcedure(string auditId, int totalProcedure)
        {
            var racmSummary = new RACMAuditAutoNumber();
            racmSummary.RACMNumber = new List<string>();
            racmSummary.RiskId = new List<string>();
            racmSummary.ControlId = new List<string>();
            racmSummary.ProcedureId = new List<string>();
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == auditId).OrderByDescending(p => p.CreatedOn);
            var repoPLM = new MongoGenericRepository<Audit>(_dbsetting);
            var repoProcdureDetails = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            int ProcedureNum = Convert.ToInt32(totalProcedure);
            if (tList.Count() == 0)
            {
                var objAudit = repoPLM.GetFirst(x => x.Id == auditId);
                if (objAudit != null)
                {
                    ProcedureNum++;
                    racmSummary.ProcedureId.Add(objAudit.AuditName + ".P.0" + ProcedureNum);
                }
            }
            else
            {
                var objAudit = repoPLM.GetFirst(x => x.Id == auditId);
                if (objAudit != null)
                {
                    foreach (var item in tList)
                    {
                        var objProcedureDetails = repoProcdureDetails.GetFirst(p => p.RACMAuditProcedureId == item.Id);
                        racmSummary.ProcedureId.Add(objProcedureDetails == null ? objAudit.AuditName + ".P.01" : objProcedureDetails.ProcedureId);
                        ProcedureNum++;
                    }
                    if (objAudit != null)
                    {
                        racmSummary.ProcedureId.Add(objAudit.AuditName + ".P.0" + ProcedureNum);
                    }
                }
            }
            return ResponseOK(racmSummary);
        }

        [HttpGet("getRiskData/{id}")]
        public ActionResult GetRiskData(String id)
        {
            var tList = _api.GetFirst(p => (p.Risk != null && p.Risk.RiskId.Contains(id)));
            return ResponseOK(tList);
        }
        [HttpGet("getControlData/{id}")]
        public ActionResult GetControlData(String id)
        {
            var tList = _api.GetFirst(p => (p.Control != null && p.Control.ControlId.Contains(id)));
            return ResponseOK(tList);
        }

    }
}