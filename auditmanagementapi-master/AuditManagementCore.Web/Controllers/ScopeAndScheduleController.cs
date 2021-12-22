using Aspose.Pdf;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScopeAndScheduleController : VJBaseGenericAPIController<ScopeAndSchedule>
    {
        IMongoDbSettings _dbsetting;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        CommonServices _CommonServices;
        public ScopeAndScheduleController(IMongoGenericRepository<ScopeAndSchedule> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] ScopeAndSchedule e)
        {
            var isExist = _api.Exists(x => x.AuditId == e.AuditId);

            if (isExist)
                return ResponseError("Scope and Scheduler for Audit ID : " + e.AuditId + " already exists.");

            e.AuditStartDate = e.AuditStartDate.ToLocalTime();
            e.AuditEndDate = e.AuditEndDate.ToLocalTime();

            if (e.AuditResources != null && e.AuditResources.Count > 0)
            {
                foreach (var resource in e.AuditResources)
                {
                    resource.AuditStartDate = resource.AuditStartDate.ToLocalTime();
                    resource.AuditEndDate = resource.AuditEndDate.ToLocalTime();
                }
            }

            //AuditApprover mapping
            _CommonServices.InsertUpdateAuditApproverMapping(e.AuditApprovalMapping, e.CreatedBy);

            intAuditDefaults(e.AuditId, e.CreatedBy);

            var ScopeAndSchedule = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.AuditNumber, "ScopeAndSchedule", "ScopeAndSchedule | Add", "Added ScopeAndSchedule");
            return ScopeAndSchedule;
        }

        public override ActionResult Put([FromBody] ScopeAndSchedule tValue)
        {
            tValue.AuditStartDate = tValue.AuditStartDate.ToLocalTime();
            tValue.AuditEndDate = tValue.AuditEndDate.ToLocalTime();

            if (tValue.AuditResources != null && tValue.AuditResources.Count > 0)
            {
                foreach (var resource in tValue.AuditResources)
                {
                    resource.AuditStartDate = resource.AuditStartDate.ToLocalTime();
                    resource.AuditEndDate = resource.AuditEndDate.ToLocalTime();
                }
            }
            _CommonServices.InsertUpdateAuditApproverMapping(tValue.AuditApprovalMapping, tValue.UpdatedBy);
            intAuditDefaults(tValue.AuditId, tValue.UpdatedBy);
            var scopeAndSchedule = base.Put(tValue);
            //Activity Log
            _CommonServices.ActivityLog(tValue.UpdatedBy, tValue.Id, tValue.AuditNumber, "ScopeAndSchedule", "ScopeAndSchedule | Edit", "Updated ScopeAndSchedule");
            return scopeAndSchedule;
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<Location, Audit>(x => x.AuditId == id);

            if (tList == null)
            {
                return ResponseNotFound();
            }
            tList = populateScopeAndSchedule(tList);
            return ResponseOK(tList);

        }

        [HttpGet("GetAuditsByUser")]
        public ActionResult GetAuditsByUser()
        {
            var tList = _api.GetAllWithInclude<Location, Audit>();

            if (tList == null)
            {
                return ResponseNotFound();
            }
            var dashboardQuery = new DashboardQuery();
            dashboardQuery.StartDate = Request.Query["StartDate"];
            dashboardQuery.EndDate = Request.Query["EndDate"];
            dashboardQuery.UserId = Request.Query["UserId"];
            dashboardQuery.CompletionStatus = Request.Query["Status"];
            var auditList = new List<ScopeAndSchedule>();
            if (!string.IsNullOrWhiteSpace(dashboardQuery.EndDate) && !string.IsNullOrWhiteSpace(dashboardQuery.StartDate))
                tList = tList.Where(x => x.AuditStartDate >= DateTime.Parse(dashboardQuery.StartDate).ToUniversalTime() && x.AuditEndDate <= DateTime.Parse(dashboardQuery.EndDate).ToUniversalTime());

            foreach (var audit in tList)
            {
                foreach (var approver in audit.AuditApprovalMapping.UserData)
                {
                    if (approver.UserId == dashboardQuery.UserId)
                    {
                        if (!auditList.Contains(audit))
                        {
                            auditList.Add(audit);
                        }
                        break;
                    }
                }
                foreach (var resource in audit.AuditResources)
                {
                    if (resource.UserId == dashboardQuery.UserId)
                    {
                        if (!auditList.Contains(audit))
                        {
                            auditList.Add(audit);
                        }
                        break;
                    }
                }
                foreach (var approver in audit.Auditees)
                {
                    if (approver.UserId == dashboardQuery.UserId)
                    {
                        if (!auditList.Contains(audit))
                        {
                            auditList.Add(audit);
                        }
                        break;
                    }
                }
            }

            if (dashboardQuery.CompletionStatus.ToLower() != "all")
            {
                dashboardQuery.CompletionStatus = dashboardQuery.CompletionStatus.ToLower() == "planned" ? null : dashboardQuery.CompletionStatus;
                foreach (var audit in auditList.ToList())
                {
                    if (dashboardQuery.CompletionStatus == "overdue")
                    {
                        if (audit.AuditEndDate.Date < DateTime.Now.Date && audit.Status == "inprogress")
                            continue;
                        else
                            auditList.Remove(audit);
                    }
                    else if (dashboardQuery.CompletionStatus == "inprogress")
                    {
                        if (audit.AuditEndDate.Date >= DateTime.Now.Date && audit.Status == "inprogress")
                            continue;
                        else
                            auditList.Remove(audit);
                    }
                    else if (dashboardQuery.CompletionStatus == null)
                    {
                        if (audit.Status == null || audit.Status == "")
                            continue;
                        else
                            auditList.Remove(audit);
                    }
                    else if (audit.Status != dashboardQuery.CompletionStatus)
                        auditList.Remove(audit);
                }
            }
            //if (status.ToLower() == "overdue")
            //{
            //    status = status.ToLower() == "planned" ? null : status;

            //    foreach (var audit in auditList.ToList())
            //    {
            //        if (status == "overdue")
            //        {
            //            if (audit.AuditEndDate.Date < DateTime.Now.Date)
            //                break;
            //            else
            //                auditList.Remove(audit);
            //        }

            //        else if (audit.Status != status)
            //            auditList.Remove(audit);
            //    }
            //}

            if (auditList.Count > 0)
                auditList = populateScopeAndSchedule(auditList);

            return ResponseOK(auditList);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Location, Audit>();

            if (tList == null)
                return ResponseNotFound();

            tList = populateScopeAndSchedule(tList);

            return ResponseOK(tList);
        }

        public IQueryable<ScopeAndSchedule> populateScopeAndSchedule(IQueryable<ScopeAndSchedule> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                item.Location = _CommonServices.GetLocationDetail(item.Location.Id);
                item.Audit = _CommonServices.GetAuditDetail(item.AuditId);

                foreach (var res in item.AuditResources)
                {
                    res.User = userRepo.GetByID(res.UserId);
                }

                item.AuditApprovalMapping = _CommonServices.AttachAuditApproverMapping(item.AuditId);

                foreach (var auditee in item.Auditees)
                {
                    if (auditee.UserId != null && auditee.UserId != "")
                        auditee.User = userRepo.GetByID(auditee.UserId);

                    if (auditee.ReportToId != null && auditee.ReportToId != "")
                        auditee.ReportToUser = userRepo.GetByID(auditee.ReportToId);
                }
            }

            return tList;
        }

        public IQueryable<ScopeAndSchedule> populateScopeAndScheduleForDashboard(IQueryable<ScopeAndSchedule> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var auditClosureRepo = new MongoGenericRepository<AuditClosure>(_dbsetting);
            var followupRepo = new MongoGenericRepository<FollowUp>(_dbsetting);
            var racmRepo = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            var dnRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var idrRepo = new MongoGenericRepository<InitialDataRequest>(_dbsetting);
            //var locRepo = new MongoGenericRepository<Location>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            foreach (var item in tList)
            {
                item.Location = _CommonServices.GetLocationDetail(item.Location.Id);
                //item.Location = locRepo.GetWithInclude<Country>(x => x.Id == item.Location.Id).First();

                var auditValues = auditRepo.GetFirst(x => x.Id == item.AuditId);

                item.Audit = auditValues == null ? new Audit() : auditValues;

                if (item.ProcessLocationMappingId != null)
                {
                    item.ProcessLocationMapping = plmRepo.GetByID(item.ProcessLocationMappingId);
                    item.FollowUp = followupRepo.GetMany(x => x.ProcessLocationMappingId == item.ProcessLocationMappingId).ToList();
                }
                item.AuditClosure = auditClosureRepo.GetFirst(x => x.AuditId == item.AuditId);
                //item.FollowUp = followupRepo.GetMany(x => x.AuditId == item.AuditId).ToList();
                item.RACMAuditProcedureDetails = racmRepo.GetMany(x => x.AuditId == item.AuditId).ToList();
                item.DiscussionNotes = dnRepo.GetMany(x => x.AuditId == item.AuditId).ToList();
                item.InitialDataRequest = idrRepo.GetMany(x => x.AuditId == item.AuditId).ToList();

                foreach (var res in item.AuditResources)
                {
                    res.User = userRepo.GetByID(res.UserId);
                }
                item.AuditApprovalMapping = _CommonServices.AttachAuditApproverMapping(item.AuditId);
            }
            return tList;
        }

        public List<ScopeAndSchedule> populateScopeAndSchedule(List<ScopeAndSchedule> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            foreach (var item in tList)
            {
                item.Location = _CommonServices.GetLocationDetail(item.Location.Id);
                item.Audit = _CommonServices.GetAuditDetail(item.AuditId);
                foreach (var res in item.AuditResources)
                {
                    res.User = userRepo.GetByID(res.UserId);
                }
                item.AuditApprovalMapping = _CommonServices.AttachAuditApproverMapping(item.AuditId);
            }
            return tList;
        }

        public void intAuditDefaults(string AuditId, string userid)
        {
            var tor = new MongoGenericRepository<TOR>(_dbsetting);
            var torItem = tor.GetFirst(x => x.AuditId == AuditId);

            if (torItem == null)
            {
                var torObj = new TOR();
                torObj.AuditId = AuditId;
                torObj.CreatedBy = userid;
                tor.Insert(torObj);
            }

            var auditClosureRepo = new MongoGenericRepository<AuditClosure>(_dbsetting);
            var fetchedAuditClosure = auditClosureRepo.GetFirst(x => x.AuditId == AuditId);
            if (fetchedAuditClosure == null)
            {
                var auditClosure = new AuditClosure();
                auditClosure.AuditId = AuditId;
                auditClosure.CreatedBy = userid;
                auditClosureRepo.Insert(auditClosure);
            }
        }

        [HttpPut("UpdateInfo")]
        public ActionResult UpdateInfo([FromBody] ScopeAndSchedule tValue)
        {
            ScopeAndSchedule item = _api.GetFirst(x => x.Id == tValue.Id);

            if (!String.IsNullOrWhiteSpace(tValue.Status))
                item.Status = tValue.Status;

            return base.Put(item);
        }

        [HttpGet("GetDashboard")]
        public ActionResult GetDashboard()
        {
            var scopeAndSchedules = getScopeAndScheduleForDashboard();
            return ResponseOK(scopeAndSchedules);
        }
        public IQueryable<ScopeAndSchedule> getScopeAndScheduleForDashboard()
        {
            var dashboardQuery = new DashboardQuery();

            dashboardQuery.Sector = Request.Query["Sector"];
            dashboardQuery.Country = Request.Query["Country"];
            dashboardQuery.Company = Request.Query["Company"];
            dashboardQuery.Rating = Request.Query["Rating"];
            dashboardQuery.StartDate = Request.Query["StartDate"];
            dashboardQuery.EndDate = Request.Query["EndDate"];

            dashboardQuery.Audit = Request.Query["Audit"];
            dashboardQuery.StartYear = Request.Query["StartYear"];
            dashboardQuery.EndYear = Request.Query["EndYear"];
            dashboardQuery.Quarter = Request.Query["Quarter"];
            dashboardQuery.Location = Request.Query["Location"];
            dashboardQuery.Division = Request.Query["Division"];
            dashboardQuery.LocationId = Request.Query["LocationId"];
            dashboardQuery.CompletionStatus = Request.Query["CompletionStatus"];


            var scopeAndSchedules = _api.GetAllWithInclude<Location, Audit>();
            //Ignore unmapped status 
            foreach (var item in scopeAndSchedules)
            {
                scopeAndSchedules = populateScopeAndScheduleForDashboard(scopeAndSchedules);
            }

            if (!string.IsNullOrWhiteSpace(dashboardQuery.EndDate) && !string.IsNullOrWhiteSpace(dashboardQuery.StartDate))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.AuditStartDate >= DateTime.Parse(dashboardQuery.StartDate).ToUniversalTime() && x.AuditEndDate <= DateTime.Parse(dashboardQuery.EndDate).ToUniversalTime());

            if (!string.IsNullOrWhiteSpace(dashboardQuery.Country))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.Location.CountryID == dashboardQuery.Country || x.Location.Country.Name == dashboardQuery.Country);

            if (!string.IsNullOrWhiteSpace(dashboardQuery.Division))
            {
                string[] lstDivision = dashboardQuery.Division.Split(',');
                foreach (var item in lstDivision)
                {
                    scopeAndSchedules = scopeAndSchedules.Where(x => x.Location.Sector == item);
                }
            }
            if (!string.IsNullOrWhiteSpace(dashboardQuery.LocationId))
            {
                string[] lstLocationId = dashboardQuery.LocationId.Split(',');
                foreach (var item in lstLocationId)
                {
                    scopeAndSchedules = scopeAndSchedules.Where(x => x.Location.Id == item);
                }
            }
            if (!string.IsNullOrWhiteSpace(dashboardQuery.Sector))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.Location.Sector == dashboardQuery.Sector);

            if (!string.IsNullOrWhiteSpace(dashboardQuery.Company))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.Location.Company.Id == dashboardQuery.Company);

            if (!string.IsNullOrWhiteSpace(dashboardQuery.Rating))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.Audit.OverallAssesment.ProcessRiskMapping.FinalProcessrating.ToLower() == dashboardQuery.Rating.ToLower());

            if (!string.IsNullOrWhiteSpace(dashboardQuery.StartYear) && !string.IsNullOrWhiteSpace(dashboardQuery.EndYear))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.AuditStartDate.Year >= DateTime.Parse(dashboardQuery.StartYear).Year && x.AuditEndDate.Year <= DateTime.Parse(dashboardQuery.EndYear).Year);

            if (!string.IsNullOrWhiteSpace(dashboardQuery.Quarter))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.Quater == dashboardQuery.Quarter);

            if (!string.IsNullOrWhiteSpace(dashboardQuery.Location))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.Location.Id == dashboardQuery.Location);

            if (!string.IsNullOrWhiteSpace(dashboardQuery.CompletionStatus))
            {
                if (dashboardQuery.CompletionStatus == "null")
                {
                    scopeAndSchedules = scopeAndSchedules.Where(x => x.Status == null);
                }
                else
                {
                    scopeAndSchedules = scopeAndSchedules.Where(x => x.Status == dashboardQuery.CompletionStatus);
                }
            }
            if (!string.IsNullOrWhiteSpace(dashboardQuery.Audit))
                scopeAndSchedules = scopeAndSchedules.Where(x => x.ProcessLocationMappingId == dashboardQuery.Audit);

            return scopeAndSchedules;
        }

        [HttpGet("downloadpdf")]
        public IActionResult DownloadPDF()
        {
            var tList = getScopeAndScheduleForDashboard();

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
            var tList = getScopeAndScheduleForDashboard();

            if (tList == null)
                return ResponseNotFound();

            var fileName = "ScopeAndSchedule.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "Audit No";
                worksheet.Cells["B1"].Value = "Audit";
                worksheet.Cells["C1"].Value = "Location";
                worksheet.Cells["D1"].Value = "Sector";
                worksheet.Cells["E1"].Value = "Country";
                worksheet.Cells["F1"].Value = "Quarter";
                worksheet.Cells["G1"].Value = "Start Date";
                worksheet.Cells["H1"].Value = "End Date";
                worksheet.Cells["I1"].Value = "Status";
                worksheet.Cells["J1"].Value = "Criticality";

                var rowIndex = 2;

                foreach (var audit in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = audit.AuditNumber;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.AuditName;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = audit.Location == null ? "" : audit.Location.ProfitCenterCode;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = audit.Location == null ? "" : audit.Location.DivisionDescription;
                    worksheet.Cells["E" + rowIndex.ToString()].Value = audit.Location == null ? "" : audit.Location.Country.Name;
                    worksheet.Cells["F" + rowIndex.ToString()].Value = audit.Quater;
                    worksheet.Cells["G" + rowIndex.ToString()].Value = audit.AuditStartDate.Date.ToShortDateString();
                    worksheet.Cells["H" + rowIndex.ToString()].Value = audit.AuditEndDate.Date.ToShortDateString();
                    if (!string.IsNullOrEmpty(audit.Status))
                    {
                        audit.Status = audit.Status.ToLower();
                        if (audit.Status == "inprogress" || audit.Status == "in progress")
                            audit.Status = "In Progress";
                        else if (audit.Status == "completed")
                            audit.Status = "Completed";
                    }
                    else
                        audit.Status = "To be initiated";
                    worksheet.Cells["I" + rowIndex.ToString()].Value = audit.Status;
                    worksheet.Cells["J" + rowIndex.ToString()].Value = audit.Audit == null ? "" : audit.Audit.OverallAssesment.ProcessRiskMapping.FinalProcessrating;

                    rowIndex++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;

                package.Save();
            }

            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var tlst = auditRepo.GetAllWithInclude<OverallAssesment, ProcessLocationMapping>();

            if (tlst == null)
                return ResponseNotFound();

            var tList1 = tlst.Where(a => a.ProcessLocationMapping != null && a.OverallAssesment != null).ToList();
            if (tList1 == null)
                return ResponseNotFound();


            var locationobj = new MongoGenericRepository<Location>(_dbsetting);
            var userobj = new MongoGenericRepository<User>(_dbsetting);

            var fileName = "AuditPlan.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "Audit Name";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Audit Number";
                worksheet.Cells["D1"].Value = "Quarter";
                worksheet.Cells["E1"].Value = "Audit Start Date";
                worksheet.Cells["F1"].Value = "Audit End Date";

                worksheet.Cells["G1"].Value = "Approver Name";
                worksheet.Cells["H1"].Value = "Approver Responsibility";
                worksheet.Cells["I1"].Value = "Approver Designation";
                worksheet.Cells["J1"].Value = "Approver Experience";
                worksheet.Cells["K1"].Value = "Approver Qualification";

                worksheet.Cells["L1"].Value = "Resources Name";
                worksheet.Cells["M1"].Value = "Resources Designation";
                worksheet.Cells["N1"].Value = "Resources Experience";
                worksheet.Cells["O1"].Value = "Resources Qualification";
                worksheet.Cells["P1"].Value = "Resources Man Days";
                worksheet.Cells["Q1"].Value = "Resources Start Date";
                worksheet.Cells["R1"].Value = "Resources End Date";

                var rowIndex = 2;

                foreach (var auditPlan in tList1)
                {
                    var resourceIndex = rowIndex;
                    var resourceCounter = 0;
                    var approverIndex = rowIndex;
                    var approverCounter = 0;


                    var lstAuditdata = _api.GetWithInclude<Location, Audit>(x => x.AuditId == auditPlan.Id);

                    if (lstAuditdata != null)
                    {
                        lstAuditdata = populateScopeAndSchedule(lstAuditdata);
                        foreach (var objAudit in lstAuditdata)
                        {
                            foreach (var resource in objAudit.AuditResources)
                            {
                                var user = userobj.GetFirst(a => a.Id == resource.UserId);
                                if (user != null)
                                {
                                    worksheet.Cells["L" + resourceIndex.ToString()].Value = user.FirstName + ' ' + user.LastName;
                                    worksheet.Cells["M" + resourceIndex.ToString()].Value = user.Designation;
                                    worksheet.Cells["N" + resourceIndex.ToString()].Value = user.Experiance;
                                    worksheet.Cells["O" + resourceIndex.ToString()].Value = user.Qualification;
                                    worksheet.Cells["P" + resourceIndex.ToString()].Value = resource.ManDaysRequired;

                                    worksheet.Cells["Q" + resourceIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                                    worksheet.Cells["R" + resourceIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";

                                    worksheet.Cells["Q" + resourceIndex.ToString()].Formula = "=Date(" + resource.AuditStartDate.Year + "," + resource.AuditStartDate.Month + "," + resource.AuditStartDate.Day + ")";
                                    worksheet.Cells["R" + resourceIndex.ToString()].Formula = "=Date(" + resource.AuditEndDate.Year + "," + resource.AuditEndDate.Month + "," + resource.AuditEndDate.Day + ")";
                                    //worksheet.Cells["R" + resourceIndex.ToString()].Value = Convert.ToDateTime(resource.AuditEndDate);

                                    resourceIndex++;
                                    resourceCounter++;
                                }
                            }
                            if (objAudit.AuditApprovalMapping != null && objAudit.AuditApprovalMapping.UserData != null)
                            {
                                foreach (var approver in objAudit.AuditApprovalMapping.UserData)
                                {
                                    var user = userobj.GetFirst(a => a.Id == approver.UserId);
                                    if (user != null)
                                    {
                                        worksheet.Cells["G" + approverIndex.ToString()].Value = user.FirstName + ' ' + user.LastName;
                                        worksheet.Cells["H" + approverIndex.ToString()].Value = approver.Responsibility;
                                        worksheet.Cells["I" + approverIndex.ToString()].Value = user.Designation;
                                        worksheet.Cells["J" + approverIndex.ToString()].Value = user.Experiance;
                                        worksheet.Cells["K" + approverIndex.ToString()].Value = user.Qualification;

                                        approverIndex++;
                                        approverCounter++;
                                    }
                                }
                            }
                            worksheet.Cells["C" + rowIndex.ToString()].Value = objAudit.AuditNumber;
                            worksheet.Cells["D" + rowIndex.ToString()].Value = objAudit.Quater;
                            worksheet.Cells["E" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells["E" + rowIndex.ToString()].Formula = "=Date(" + objAudit.AuditStartDate.Year + "," + objAudit.AuditStartDate.Month + "," + objAudit.AuditStartDate.Day + ")";
                            //worksheet.Cells["E" + rowIndex.ToString()].Value = objAudit.AuditStartDate.ToShortDateString();

                            worksheet.Cells["F" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells["F" + rowIndex.ToString()].Formula = "=Date(" + objAudit.AuditEndDate.Year + "," + objAudit.AuditEndDate.Month + "," + objAudit.AuditEndDate.Day + ")";
                            //worksheet.Cells["F" + rowIndex.ToString()].Value = objAudit.AuditEndDate.ToShortDateString();

                        }
                        worksheet.Cells["A" + rowIndex.ToString()].Value = auditPlan.AuditName;
                        worksheet.Cells["B" + rowIndex.ToString()].Value = locationobj.GetWithInclude<Location>(x => x.Id == auditPlan.Location.Id).FirstOrDefault().ProfitCenterCode;

                    }

                    if (resourceCounter > approverCounter)
                    {
                        worksheet.Cells["A" + rowIndex.ToString() + ":A" + (rowIndex + resourceCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["B" + rowIndex.ToString() + ":B" + (rowIndex + resourceCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["C" + rowIndex.ToString() + ":C" + (rowIndex + resourceCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["D" + rowIndex.ToString() + ":D" + (rowIndex + resourceCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["E" + rowIndex.ToString() + ":E" + (rowIndex + resourceCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["F" + rowIndex.ToString() + ":F" + (rowIndex + resourceCounter - 1).ToString()].Merge = true;

                        worksheet.Cells["A" + rowIndex.ToString() + ":A" + (rowIndex + resourceCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["B" + rowIndex.ToString() + ":B" + (rowIndex + resourceCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["C" + rowIndex.ToString() + ":C" + (rowIndex + resourceCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["D" + rowIndex.ToString() + ":D" + (rowIndex + resourceCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["E" + rowIndex.ToString() + ":E" + (rowIndex + resourceCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["F" + rowIndex.ToString() + ":F" + (rowIndex + resourceCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        rowIndex += resourceCounter - 1;
                    }
                    else if (approverCounter > 1)
                    {
                        worksheet.Cells["A" + rowIndex.ToString() + ":A" + (rowIndex + approverCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["B" + rowIndex.ToString() + ":B" + (rowIndex + approverCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["C" + rowIndex.ToString() + ":C" + (rowIndex + approverCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["D" + rowIndex.ToString() + ":D" + (rowIndex + approverCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["E" + rowIndex.ToString() + ":E" + (rowIndex + approverCounter - 1).ToString()].Merge = true;
                        worksheet.Cells["F" + rowIndex.ToString() + ":F" + (rowIndex + approverCounter - 1).ToString()].Merge = true;


                        worksheet.Cells["A" + rowIndex.ToString() + ":A" + (rowIndex + approverCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["B" + rowIndex.ToString() + ":B" + (rowIndex + approverCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["C" + rowIndex.ToString() + ":C" + (rowIndex + approverCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["D" + rowIndex.ToString() + ":D" + (rowIndex + approverCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["E" + rowIndex.ToString() + ":E" + (rowIndex + approverCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells["F" + rowIndex.ToString() + ":F" + (rowIndex + approverCounter - 1).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        rowIndex += approverCounter;
                    }

                    rowIndex++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                package.Save();
            }

            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("getsummarycounts")]
        public IActionResult GetSummaryCounts()
        {
            var tList = _api.GetAllWithInclude<Location, Audit>();

            if (tList == null)
                return ResponseNotFound();

            var dashboardQuery = new DashboardQuery();
            dashboardQuery.StartDate = Request.Query["StartDate"];
            dashboardQuery.EndDate = Request.Query["EndDate"];
            dashboardQuery.UserId = Request.Query["UserId"];
            dashboardQuery.CompletionStatus = Request.Query["Status"];

            if (!string.IsNullOrWhiteSpace(dashboardQuery.EndDate) && !string.IsNullOrWhiteSpace(dashboardQuery.StartDate))
                tList = tList.Where(x => x.AuditStartDate >= DateTime.Parse(dashboardQuery.StartDate).ToUniversalTime() && x.AuditEndDate <= DateTime.Parse(dashboardQuery.EndDate).ToUniversalTime());

            var auditList = new List<ScopeAndSchedule>();

            foreach (var audit in tList)
            {
                foreach (var approver in audit.AuditApprovalMapping.UserData)
                {
                    if (approver.UserId == dashboardQuery.UserId)
                    {
                        if (!auditList.Contains(audit))
                            auditList.Add(audit);
                        break;
                    }
                }

                foreach (var resource in audit.AuditResources)
                {
                    if (resource.UserId == dashboardQuery.UserId)
                    {
                        if (!auditList.Contains(audit))
                            auditList.Add(audit);
                        break;
                    }
                }
                foreach (var approver in audit.Auditees)
                {
                    if (approver.UserId == dashboardQuery.UserId)
                    {
                        if (!auditList.Contains(audit))
                            auditList.Add(audit);
                        break;
                    }
                }
            }
            var summaryCount = new AuditsSummaryCount()
            {
                planned = auditList.Where(a => a.Status == null || a.Status == "").Count(),
                inprogress = auditList.Where(a => a.Status != null && a.Status.ToLower().Trim() == "inprogress" && a.AuditEndDate.Date >= DateTime.Now.Date).Count(),
                completed = auditList.Where(a => a.Status != null && a.Status.ToLower().Trim() == "completed").Count(),
                unplanned = auditList.Where(a => a.Status != null && a.Status.ToLower().Trim() == "unplanned").Count(),
                overdue = auditList.Where(a => a.AuditEndDate.Date < DateTime.Now.Date && a.Status == "inprogress").Count()
            };
            return ResponseOK(summaryCount);
        }

        [HttpGet("GetCompanybyScopeSchedule")]
        public ActionResult GetCompanybyScopeSchedule()
        {
            var repoScopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var repoLocation = new MongoGenericRepository<Location>(_dbsetting);

            var tList = (from x in repoScopeAndSchedule.GetAll() join loc in repoLocation.GetAll() on x.LocationId equals loc.Id select loc).Distinct().ToList();
            foreach (var item in tList)
            {
                item.Company = _CommonServices.GetCompanyDetail(item.CompanyID);
            }
            return ResponseOK(tList);
        }
        [HttpGet("GetAuditbyScopeSchedule")]
        public ActionResult GetAuditbyScopeSchedule()
        {
            var repoScopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var repoplm = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            var tList = (from x in this._api.GetAll() join plm in repoplm.GetAll() on x.ProcessLocationMappingId equals plm.Id select plm).Distinct().ToList();
            return ResponseOK(tList);
        }
        public StringBuilder commonPDFPPT(IEnumerable<Audit> tList1)
        {
            var _locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            var _sectorRepo = new MongoGenericRepository<Sector>(_dbsetting);
            var _countryRepo = new MongoGenericRepository<Country>(_dbsetting);
            var _RepoScopeSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var _RepoUser = new MongoGenericRepository<User>(_dbsetting);
            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "OverallAssesmentPresentation.html");

            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }
            var mainBuilder = new StringBuilder();
            mainBuilder.Append(emailBody);
            var returnBuilder = new StringBuilder();
            //returnBuilder.Append("<div><center><h2></h2></center></div>");
            returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1'  style='width: 100%; font-size: 12px; font-family: Arial'>");
            returnBuilder.Append("<caption><h1>CFPL - Audit Plan</h1></caption><tr><td><b>Audit Name</b></td><td><b>Location</b></td><td><b>Sector</b></td><td><b>Country</b></td><td><b>Quarter</b></td><td><b>Start Date</b></td><td><b>End Date</b></td><td><b>Approver name</b></td><td><b>Resource Name</b></td></tr>");
            foreach (var auditPlan in tList1)
            {
                //var locations = new List<string>();
                //var sectors = new List<string>();
                //var countryies = new List<string>();
                //if (auditPlan.ProcessLocationMapping != null)
                //{
                //    var locationArray = auditPlan.ProcessLocationMapping.Locations;

                //    foreach (var loc in locationArray)
                //    {
                //        var location = _locationRepo.GetFirst(a => a.Id == loc);
                //        if (location != null)
                //        {
                //            locations.Add(location.ProfitCenterCode);
                //            sectors.Add(location.LocationDescription);

                //        }

                //    }
                //}

                returnBuilder.Append("<tr>");
                returnBuilder.Append("<td>" + auditPlan.AuditName + "</td>");
                returnBuilder.Append("<td>" + auditPlan.Location.ProfitCenterCode + "</td>");
                returnBuilder.Append("<td>" + auditPlan.Location.LocationDescription + "</td>");
                if (auditPlan.Location.CountryID != null)
                {
                    var country = _countryRepo.GetFirst(a => a.Id == auditPlan.Location.CountryID);
                    if (country != null)
                        returnBuilder.Append("<td>" + country.Name + "</td>");
                    else
                        returnBuilder.Append("<td></td>");
                }
                var objScope = _RepoScopeSchedule.GetFirst(p => p.AuditId == auditPlan.Id);
                var lstApprover = new List<string>();
                var lstResource = new List<string>();

                if (objScope != null)
                {
                    returnBuilder.Append("<td>" + objScope.Quater + "</td>");
                    returnBuilder.Append("<td>" + objScope.AuditStartDate.ToString("dd-MM-yyyy") + "</td>");
                    returnBuilder.Append("<td>" + objScope.AuditEndDate.ToString("dd-MM-yyyy") + "</td>");
                    if (objScope.AuditApprovalMapping != null)
                    {
                        foreach (var itemUser in objScope.AuditApprovalMapping.UserData)
                        {
                            var objUser = _RepoUser.GetFirst(p => p.Id == itemUser.UserId);
                            if (objUser != null)
                            {
                                lstApprover.Add(objUser.FirstName + " " + objUser.LastName + " (" + objUser.Designation + ")");
                            }
                        }
                        foreach (var itemUser in objScope.AuditResources)
                        {
                            var objUser = _RepoUser.GetFirst(p => p.Id == itemUser.UserId);
                            if (objUser != null)
                            {
                                lstResource.Add(objUser.FirstName + " " + objUser.LastName + " (" + objUser.Designation + ")");
                            }
                        }
                        returnBuilder.Append("<td>" + string.Join(", ", lstApprover) + "</td>");
                        returnBuilder.Append("<td>" + string.Join(", ", lstResource) + "</td>");
                    }
                }
                else
                {
                    returnBuilder.Append("<td></td><td></td><td></td><td></td><td></td>");
                }
                returnBuilder.Append("</tr>");
            }
            mainBuilder.Replace("#AuditPlan#", returnBuilder.ToString());
            return mainBuilder;
        }
        [HttpGet("downloadpdfscopeandschedule")]
        public IActionResult DownloadPDFScopeAndSchedule()
        {
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var tlst = auditRepo.GetAllWithInclude<OverallAssesment, ProcessLocationMapping>();
            if (tlst == null)
                return ResponseNotFound();

            var tList1 = tlst.Where(a => a.ProcessLocationMapping != null && a.OverallAssesment != null).ToList();
            if (tList1 == null)
                return ResponseNotFound();

            StringBuilder returnBuilder = commonPDFPPT(tList1);
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "OverallAssesment.pdf");
        }
        [HttpGet("downloadppt")]
        public IActionResult DownloadPPT()
        {
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var tlst = auditRepo.GetAllWithInclude<OverallAssesment, ProcessLocationMapping>();

            if (tlst == null)
                return ResponseNotFound();

            var tList1 = tlst.Where(a => a.ProcessLocationMapping != null && a.OverallAssesment != null).ToList();
            if (tList1 == null)
                return ResponseNotFound();

            StringBuilder returnBuilder = commonPDFPPT(tList1);
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            var folderName = Path.Combine(wkhtmlexepath);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            var fullPath = Path.Combine(pathToSave, "ScopeAndSchedule" + myUniqueFileName + ".pdf");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(pdfbyte, 0, pdfbyte.Length);
            }
            Document pdfDocument = new Document(fullPath);
            PptxSaveOptions pptxOptions = new PptxSaveOptions();
            pdfDocument.Save(memoryStream, pptxOptions.SaveFormat);
            byte[] pptBytes = memoryStream.ToArray();
            return File(pptBytes, "application/octet-stream", "ScopeAndSchedule.pptx");
        }
        #region Export PDF and PPT for OverallAuditInformation,DivisionWise Audit Information, LocationWise Audit Information
        #region Export PDF and PPT for OverallAuditInformation
        [HttpGet("OverAllAuditsInformationPDF")]
        public IActionResult OverAllAuditsInformationPDF()
        {
            var returnBuilder = commonforPDFandPPT();
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "OverAllAuditsInformation.pdf");
        }
        [HttpGet("OverAllAuditsInformationPPT")]
        public IActionResult OverAllAuditsInformationPPT()
        {
            var returnBuilder = commonforPDFandPPT();
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            var folderName = Path.Combine(wkhtmlexepath);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            var fullPath = Path.Combine(pathToSave, "OverAllAuditsInformation" + myUniqueFileName + ".pptx");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(pdfbyte, 0, pdfbyte.Length);
            }
            Document pdfDocument = new Document(fullPath);
            PptxSaveOptions pptxOptions = new PptxSaveOptions();
            pdfDocument.Save(memoryStream, pptxOptions.SaveFormat);
            byte[] pptBytes = memoryStream.ToArray();
            return File(pptBytes, "application/octet-stream", "OverAllAuditsInformation.pptx");
        }
        public StringBuilder commonforPDFandPPT()
        {
            var returnBuilder = new StringBuilder();
            var tList = getScopeAndScheduleForDashboard();
            if (tList != null)
            {
                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "OverallAssesmentPresentation.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }
                returnBuilder.Append(emailBody);
                returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1'  style='width: 100%; font-size: 12px; font-family: Arial'>");
                returnBuilder.Append("<tr><td rowspan='2' style='text-align:center'>Audit</td><td rowspan='2' style='text-align:center'>Year</td><td rowspan='2' style='text-align:center'>Status</td><td colspan='4' style='text-align:center'>Audit Health</td><td colspan='2' style='text-align:center'>Action Taken</td></tr><tr><td style='text-align:center'>Audit rating</td><td style='text-align:center'>High rated</td><td style='text-align:center'>Red flags</td><td style='text-align:center'>Repeat</td><td style='text-align:center'>Total</td><td style='text-align:center'>Closed (%)</td></tr>");
                foreach (var audit in tList)
                {
                    returnBuilder.Append("<tr>");
                    var auditname = audit.ProcessLocationMapping != null ? audit.ProcessLocationMapping.AuditName : audit.Audit.AuditName;
                    returnBuilder.Append("<td>" + auditname + "</td>");
                    returnBuilder.Append("<td>" + _CommonServices.GetCurrentFinancialYearByDate(audit.AuditStartDate, audit.AuditEndDate) + "</td>");
                    returnBuilder.Append("<td>" + (audit.Status != null ? audit.Status : "to be Initiated") + "</td>");

                    var objDiscussionNote = audit.DiscussionNotes.Where(p => p.ObservationGrading == ObservationGradingEnum.High || p.ObservationGrading == ObservationGradingEnum.Critical).Count();
                    returnBuilder.Append("<td>" + objDiscussionNote + "</td>");
                    returnBuilder.Append("<td>" + objDiscussionNote + "</td>");
                    var redflag = audit.AuditClosure.ProcessImprovement.RedFlag != null ? audit.AuditClosure.ProcessImprovement.RedFlag : "0";

                    returnBuilder.Append("<td>" + redflag + "</td>");
                    returnBuilder.Append("<td>" + 0 + "</td>");


                    var total = audit.FollowUp.Count();
                    var completed = audit.FollowUp.Where(p => p.Status.ToLower() == "completed" && p.ImplementationEndDate >= DateTime.Now).Count();
                    returnBuilder.Append("<td>" + total + "</td>");
                    returnBuilder.Append("<td>" + completed + "</td>");
                    returnBuilder.Append("</tr>");
                }
            }
            return returnBuilder;
        }
        #endregion
        #region Export PDF and PPT for DivisionWise Audit Information
        [HttpGet("DivisionWiseAuditInformationPDF")]
        public IActionResult DivisionWiseAuditInformationPDF()
        {
            var returnBuilder = commonDivisionforPDFandPPT();
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "DivisionWiseAuditInformation.pdf");
        }
        [HttpGet("DivisionWiseAuditInformationPPT")]
        public IActionResult DivisionWiseAuditInformationPPT()
        {
            var returnBuilder = commonDivisionforPDFandPPT();

            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            var folderName = Path.Combine(wkhtmlexepath);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            var fullPath = Path.Combine(pathToSave, "OverAllAuditsInformation" + myUniqueFileName + ".pptx");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(pdfbyte, 0, pdfbyte.Length);
            }
            Document pdfDocument = new Document(fullPath);
            PptxSaveOptions pptxOptions = new PptxSaveOptions();
            pdfDocument.Save(memoryStream, pptxOptions.SaveFormat);
            byte[] pptBytes = memoryStream.ToArray();
            return File(pptBytes, "application/octet-stream", "DivisionWiseAuditInformation.pptx");
        }
        public StringBuilder commonDivisionforPDFandPPT()
        {
            var returnBuilder = new StringBuilder();
            var RepoSector = new MongoGenericRepository<Sector>(_dbsetting);
            int auditInitiated = 0, auditPending = 0, auditCompleted = 0, unplanned = 0, overdue = 0, auditRequired = 0, completed = 0, completedWithDelayedfollowup = 0, delayed = 0
                , controlsReport = 0, highRatedObservations = 0, redFlags = 0, repeatObservations = 0, statutoryDefault = 0, bestPractices = 0, enhancement = 0, potentialSaving = 0;

            var tList = getScopeAndScheduleForDashboard();
            var list = (from m in tList
                              .GroupBy(x => new { x.Location.Country.Name, x.Location.Sector })
                              .ToList()
                        select m);
            if (list != null)
            {
                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "LocationwiseAuditInfo.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }
                returnBuilder.Append(emailBody);
                returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1'  style='width: 100%; font-size: 12px; font-family: Arial'>");
                returnBuilder.Append("<tr><td rowspan='2' style='text-align:center'>Business Division</td><td colspan='5' style='text-align:center'>Audit (#)</td><td colspan='5' style='text-align:center'>Audit Effectiveness</td><td colspan='4' style='text-align:center'>Action Taken Report</td><td colspan='3' style='text-align:center'>Value Scorecard</td></tr><tr><td style='text-align:center'>Initiated</td><td style='text-align:center'>Completed</td><td style='text-align:center'>In Progress</td><td style='text-align:center'>Overdue</td><td style='text-align:center'>Unplanned</td><td style='text-align:center'>High risk observations</td><td style='text-align:center'>Repeat observations</td><td style='text-align:center'>Red flags</td><td style='text-align:center'>Statutory defaults</td><td style='text-align:center'>Control with no exception</td><td style='text-align:center'>In Progress</td><td style='text-align:center'>Completed</td><td style='text-align:center'>Delayed</td><td style='text-align:center'>Completed with delay</td><td style='text-align:center'>Saving Potential</td><td style='text-align:center'>Enhancement</td><td style='text-align:center'>Best Practices</td></tr>");

                foreach (var audit in list)
                {
                    var sector = RepoSector.GetFirst(p => p.Id == audit.Key.Sector);
                    returnBuilder.Append("<tr>");
                    returnBuilder.Append("<td>" + (sector != null ? sector.Name : "") + "</td>");

                    foreach (var item in audit)
                    {
                        var auditStats = this.getAuditStats(item.Status, item.AuditEndDate);
                        auditInitiated += auditStats.auditInitiated;
                        auditCompleted += auditStats.auditCompleted;
                        auditPending += auditStats.auditPending;
                        overdue += auditStats.overdue;
                        unplanned += auditStats.unplanned;

                        var followupStats = this.getFollowupStats(item.FollowUp);
                        auditRequired += followupStats.auditRequired;
                        completed += followupStats.completed;
                        completedWithDelayedfollowup += followupStats.completedWithDelayed;
                        delayed += followupStats.delayed;


                        var assuranceStats = this.getAssurance(item);
                        controlsReport += assuranceStats.controlsReport; //No of Effective controls
                        highRatedObservations += assuranceStats.highRatedObservations; //Observations Critical/High
                        redFlags += assuranceStats.redFlags;
                        repeatObservations += assuranceStats.repeatObservations; //Observations Repeated
                        statutoryDefault += assuranceStats.statutoryDefault;

                        var scorecard = this.getValueScorecard(item.AuditClosure);
                        bestPractices += scorecard.bestPractices;
                        enhancement += scorecard.enhancement;
                        potentialSaving += scorecard.potentialSaving;
                    }
                    returnBuilder.Append("<td>" + auditInitiated + "</td>");
                    returnBuilder.Append("<td>" + auditPending + "</td>");
                    returnBuilder.Append("<td>" + auditCompleted + "</td>");
                    returnBuilder.Append("<td>" + overdue + "</td>");
                    returnBuilder.Append("<td>" + unplanned + "</td>");

                    returnBuilder.Append("<td>" + highRatedObservations + "</td>");
                    returnBuilder.Append("<td>" + repeatObservations + "</td>");
                    returnBuilder.Append("<td>" + redFlags + "</td>");
                    returnBuilder.Append("<td>" + statutoryDefault + "</td>");
                    returnBuilder.Append("<td>" + controlsReport + "</td>");

                    returnBuilder.Append("<td>" + auditRequired + "</td>");
                    returnBuilder.Append("<td>" + completed + "</td>");
                    returnBuilder.Append("<td>" + completedWithDelayedfollowup + "</td>");
                    returnBuilder.Append("<td>" + delayed + "</td>");

                    returnBuilder.Append("<td>" + potentialSaving + "</td>");
                    returnBuilder.Append("<td>" + enhancement + "</td>");
                    returnBuilder.Append("<td>" + bestPractices + "</td>");
                    returnBuilder.Append("</tr>");
                    auditInitiated = 0; auditPending = 0; auditCompleted = 0; unplanned = 0; overdue = 0; auditRequired = 0; completed = 0; completedWithDelayedfollowup = 0; delayed = 0;
                    controlsReport = 0; highRatedObservations = 0; redFlags = 0; repeatObservations = 0; statutoryDefault = 0; bestPractices = 0; enhancement = 0; potentialSaving = 0;
                }
            }
            return returnBuilder;
        }
        #endregion
        #region Export PDF and PPT for LocationWise Audit Information
        [HttpGet("LocationWiseAuditInformationPDF")]
        public IActionResult LocationWiseAuditInformationPDF([FromQuery] List<Overview> overview)
        {
            var returnBuilder = commonLocationforPDFandPPT();
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "LocationWiseAuditInformation.pdf");
        }
        [HttpGet("LocationWiseAuditInformationPPT")]
        public IActionResult LocationWiseAuditInformationPPT()
        {
            var returnBuilder = commonLocationforPDFandPPT();

            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            var folderName = Path.Combine(wkhtmlexepath);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            var fullPath = Path.Combine(pathToSave, "LocationWiseAuditsInformation" + myUniqueFileName + ".pptx");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(pdfbyte, 0, pdfbyte.Length);
            }
            Document pdfDocument = new Document(fullPath);
            PptxSaveOptions pptxOptions = new PptxSaveOptions();
            pdfDocument.Save(memoryStream, pptxOptions.SaveFormat);
            byte[] pptBytes = memoryStream.ToArray();
            return File(pptBytes, "application/octet-stream", "LocationWiseAuditInformation.pptx");
        }
        public StringBuilder commonLocationforPDFandPPT()
        {
            var returnBuilder = new StringBuilder();
            var tList = getScopeAndScheduleForDashboard();
            var list = (from m in tList
                              .GroupBy(x => new { x.Location.Country.Name, x.Location.DivisionDescription })
                              .ToList()
                        select m);
            if (list != null)
            {
                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "LocationwiseAuditInfo.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }
                returnBuilder.Append(emailBody);
                returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1'  style='width: 100%; font-size: 12px; font-family: Arial'>");
                returnBuilder.Append("<tr><td rowspan='2' style='text-align:center'>Location</td><td colspan='5' style='text-align:center'>Audit (#)</td><td colspan='5' style='text-align:center'>Audit health</td><td colspan='4' style='text-align:center'>Action Taken Report</td><td colspan='3' style='text-align:center'>Value Scorecard</td></tr><tr><td style='text-align:center'>Initiated</td><td style='text-align:center'>Completed</td><td style='text-align:center'>In Progress</td><td style='text-align:center'>Overdue</td><td style='text-align:center'>Unplanned</td><td style='text-align:center'>High risk observations</td><td style='text-align:center'>Repeat observations</td><td style='text-align:center'>Red flags</td><td style='text-align:center'>Statutory defaults</td><td style='text-align:center'>Control with no exception</td><td style='text-align:center'>In Progress</td><td style='text-align:center'>Completed</td><td style='text-align:center'>Delayed</td><td style='text-align:center'>Completed with delay</td><td style='text-align:center'>Saving Potential</td><td style='text-align:center'>Process Improvement</td><td style='text-align:center'>Reporting Considerations</td></tr>");
                foreach (var audit in list)
                {
                    int auditInitiated = 0, auditPending = 0, auditCompleted = 0, unplanned = 0, overdue = 0, auditRequired = 0, completed = 0, completedWithDelayedfollowup = 0, delayed = 0
                        , controlsReport = 0, highRatedObservations = 0, redFlags = 0, repeatObservations = 0, statutoryDefault = 0, bestPractices = 0, enhancement = 0, potentialSaving = 0;

                    returnBuilder.Append("<tr>");
                    returnBuilder.Append("<td>" + (audit.Key.DivisionDescription) + "</td>");
                    foreach (var item in audit)
                    {
                        var auditStats = this.getAuditStats(item.Status, item.AuditEndDate);
                        auditInitiated += auditStats.auditInitiated;
                        auditCompleted += auditStats.auditCompleted;
                        auditPending += auditStats.auditPending;
                        unplanned += auditStats.unplanned;
                        overdue += auditStats.overdue;

                        var followupStats = this.getFollowupStats(item.FollowUp);
                        auditRequired += followupStats.auditRequired;
                        completed += followupStats.completed;
                        completedWithDelayedfollowup += followupStats.completedWithDelayed;
                        delayed += followupStats.delayed;


                        var assuranceStats = this.getAssurance(item);
                        controlsReport += assuranceStats.controlsReport; //No of Effective controls
                        highRatedObservations += assuranceStats.highRatedObservations; //Observations Critical/High
                        redFlags += assuranceStats.redFlags;
                        repeatObservations += assuranceStats.repeatObservations; //Observations Repeated
                        statutoryDefault += assuranceStats.statutoryDefault;

                        var scorecard = this.getValueScorecard(item.AuditClosure);
                        bestPractices += scorecard.bestPractices;
                        enhancement += scorecard.enhancement;
                        potentialSaving += scorecard.potentialSaving;

                    }
                    returnBuilder.Append("<td>" + auditInitiated + "</td>");
                    returnBuilder.Append("<td>" + auditCompleted + "</td>");
                    returnBuilder.Append("<td>" + auditPending + "</td>");
                    returnBuilder.Append("<td>" + overdue + "</td>");
                    returnBuilder.Append("<td>" + unplanned + "</td>");
                    returnBuilder.Append("<td>" + highRatedObservations + "</td>");
                    returnBuilder.Append("<td>" + repeatObservations + "</td>");
                    returnBuilder.Append("<td>" + redFlags + "</td>");
                    returnBuilder.Append("<td>" + statutoryDefault + "</td>");
                    returnBuilder.Append("<td>" + controlsReport + "</td>");
                    returnBuilder.Append("<td>" + auditRequired + "</td>");
                    returnBuilder.Append("<td>" + completed + "</td>");
                    returnBuilder.Append("<td>" + delayed + "</td>");
                    returnBuilder.Append("<td>" + completedWithDelayedfollowup + "</td>");
                    returnBuilder.Append("<td>" + potentialSaving + "</td>");
                    returnBuilder.Append("<td>" + enhancement + "</td>");
                    returnBuilder.Append("<td>" + bestPractices + "</td>");
                    returnBuilder.Append("</tr>");
                    auditInitiated = 0; auditPending = 0; auditCompleted = 0; unplanned = 0; overdue = 0; auditRequired = 0; completed = 0; completedWithDelayedfollowup = 0; delayed = 0;
                    controlsReport = 0; highRatedObservations = 0; redFlags = 0; repeatObservations = 0; statutoryDefault = 0; bestPractices = 0; enhancement = 0; potentialSaving = 0;
                }
            }
            return returnBuilder;
        }
        #endregion
        #endregion
        public AuditExecution getAuditStats(string auditStatus, DateTime auditEndDate)
        {
            var auditstatus = auditStatus;
            var _auditCompleted = 0;
            var _auditInitiated = 0;
            var _auditPending = 0;
            var _overdueAudit = 0;
            var _unplannedAudit = 0;
            var _overdue = (auditEndDate.Date < DateTime.Now.Date);

            if (auditstatus == "inprogress" && _overdue)
                _overdueAudit = _overdueAudit + 1;
            else if (auditstatus == null) _auditInitiated = _auditInitiated + 1;
            else if (auditstatus == "inprogress") _auditPending = _auditPending + 1;
            else if (auditstatus == "unplanned") _unplannedAudit = _unplannedAudit + 1;
            else if (auditstatus == "completed") _auditCompleted = _auditCompleted + 1;
            else _auditPending = _auditPending + 1;

            AuditExecution auditStat = new AuditExecution
            {
                auditInitiated = _auditInitiated,
                auditCompleted = _auditCompleted,
                auditPending = _auditPending,
                overdue = _overdueAudit,
                unplanned = _unplannedAudit
            };
            return auditStat;
        }
        public ActionTaken getFollowupStats(List<FollowUp> followupArray)
        {
            ActionTaken objActionTaken = new ActionTaken();
            var _auditRequired = 0;
            var _completed = 0;
            var _delayed = 0;
            var _completedWithDelayed = 0;
            foreach (var Itemfollowup in followupArray)
            {
                var status = (!string.IsNullOrEmpty(Itemfollowup.Status)) ? Itemfollowup.Status.ToLower() : "";

                var overdue = Itemfollowup != null ? (Itemfollowup.ImplementationEndDate < DateTime.Now.Date) : false;

                if (status == "inprogress" && overdue) _delayed = _delayed + 1;
                else if (status == "completed" && overdue) _completedWithDelayed = _completedWithDelayed + 1;
                else if (status == "inprogress") _auditRequired = _auditRequired + 1;
                else if (status == "completed") _completed = _completed + 1;
            }
            objActionTaken.auditRequired += _auditRequired;
            objActionTaken.completed += _completed;
            objActionTaken.delayed += _delayed;
            objActionTaken.completedWithDelayed += _completedWithDelayed;
            objActionTaken.total = followupArray.Count();
            return objActionTaken;
        }
        public Assurance getAssurance(ScopeAndSchedule audit)
        {
            Assurance objAssurance = new Assurance();

            var controlsReport = audit.RACMAuditProcedureDetails.Where(p => p.Status != null && p.Status.ToLower() == "effective").Count();
            var highRatedObservations = audit.DiscussionNotes.Where(p => p.ObservationGrading == ObservationGradingEnum.High || p.ObservationGrading == ObservationGradingEnum.Critical).Count();
            var auditClosure = audit.AuditClosure;
            if (auditClosure != null)
            {
                var redFlags = auditClosure.ProcessImprovement.RedFlag != ""
                   ? Convert.ToInt32(auditClosure.ProcessImprovement.RedFlag)
                   : 0;
                //var statutoryDefault = auditClosure.ProcessImprovement.statutoryNonCompliance
                //  ? parseInt(auditClosure.ProcessImprovement.statutoryNonCompliance)
                //  : 0; 
                objAssurance.redFlags = redFlags;
            }
            objAssurance.highRatedObservations = highRatedObservations;
            objAssurance.repeatObservations = 0;
            objAssurance.statutoryDefault = 0;
            objAssurance.controlsReport = controlsReport;
            return objAssurance;
        }
        public ValueScorecard getValueScorecard(AuditClosure auditClosure)
        {
            ValueScorecard objValueScorecard = new ValueScorecard();
            if (auditClosure != null)
            {
                objValueScorecard.potentialSaving = auditClosure.SavingPotential.PotentialsSavings != null
                  ? Convert.ToInt32(auditClosure.SavingPotential.PotentialsSavings)
                  : 0;
                objValueScorecard.enhancement = auditClosure.ProcessImprovement.SystemImprovement != null
                  ? Convert.ToInt32(auditClosure.ProcessImprovement.SystemImprovement)
                  : 0;
                objValueScorecard.bestPractices = auditClosure.ProcessImprovement.LeadingPractices != null
                  ? Convert.ToInt32(auditClosure.ProcessImprovement.LeadingPractices)
                  : 0;
            }
            return objValueScorecard;
        }
    }
}

public class DashboardQuery
{
    public string StartDate { get; set; }
    public string EndDate { get; set; }

    public string Country { get; set; }
    public string Company { get; set; }
    public string Sector { get; set; }
    public string Rating { get; set; }
    public string Audit { get; set; }
    public string StartYear { get; set; }

    public string EndYear { get; set; }
    public string Quarter { get; set; }
    public string Location { get; set; }
    public string Division { get; set; }
    public string LocationId { get; set; }
    public string CompletionStatus { get; set; }
    public string UserId { get; set; }
}