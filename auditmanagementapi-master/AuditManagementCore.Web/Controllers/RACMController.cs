using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RACMController : VJBaseGenericAPIController<RACM>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RACMController(IMongoGenericRepository<RACM> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] RACM e)
        {
            if (e.RACMModified != null)
                return InsertRACMModified(e);
            else
                return InsertRACM(e);
        }
        public override ActionResult Put([FromBody] RACM e)
        {
            if (e == null) return ResponseBad("RACM object is null");
            var racm = _api.GetFirst(x => x.Id == e.Id);

            if (racm == null)
            {
                return ResponseError("RACM does not exists");
            }
            populateRACM(racm, e);
            _api.Update(racm);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Risk.RiskId, "RACM", "RACM | Edit", "Updated RACM");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            var racmRepo = new MongoGenericRepository<RACM>(_dbsetting);
            var racm = racmRepo.GetFirst(x => x.Id == id);

            ActionResult actionResult = base.Delete(id, userid);

            DeleteRacmRisk(racm, racmRepo, userid);
            DeleteRacmControl(racm, racmRepo, userid);
            //Activity Log
            _CommonServices.ActivityLog(userid, id, racm.Risk.RiskId, "RACM", "RACM | Delete", "Deleted RACM");
            return actionResult;

        }
        private void populateRACM(RACM objRACM, RACM e)
        {
            objRACM.RACMnumber = e.RACMnumber;
            objRACM.RiskId = e.RiskId;
            objRACM.ControlId = e.ControlId;
            objRACM.BusinessCycleId = e.BusinessCycleId;
            objRACM.ProcessL1Id = e.ProcessL1Id;
            objRACM.ProcessL2Id = e.ProcessL2Id;
            objRACM.Risk = e.Risk;
            objRACM.Control = e.Control;
            objRACM.UpdatedBy = e.UpdatedBy;
        }

        [HttpGet("GetByRisk/{id}")]
        public ActionResult GetByRisk(string id)
        {
            var RACM = _api.GetWithInclude<Risk, Control>(x => x.Risk.RiskId == id);
            return ResponseSuccess(RACM);
        }

        [HttpGet("GetByControl/{id}")]
        public ActionResult GetByControl(string id)
        {
            var RACM = _api.GetWithInclude<Risk, Control>(x => x.Control.ControlId == id);
            return ResponseSuccess(RACM);
        }

        [HttpGet("GetById/{id}")]
        public ActionResult GetById(string id)
        {
            var racm = _api.GetWithInclude<Risk, Control, BusinessCycle, ProcessL1, ProcessL2>(x => x.Id == id);
            return ResponseSuccess(racm);
        }
        [HttpGet("getRACMbyAudit/{id}")]
        public ActionResult getRACMbyAudit(string id)
        {
            var tList = _api.GetAllWithInclude<Risk, Control, BusinessCycle, ProcessL1, ProcessL2>();

            if (tList == null)
                return ResponseNotFound();

            tList = PopulateRacmProcedureLibary(tList, id);

            return ResponseOK(tList);
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Risk, Control, BusinessCycle, ProcessL1, ProcessL2>();

            if (tList == null)
                return ResponseNotFound();

            tList = PopulateRacmProcedure(tList);

            return ResponseOK(tList);
        }
        public IQueryable<RACM> PopulateRacmProcedure(IQueryable<RACM> tList)
        {
            foreach (var item in tList)
            {
                foreach (var temp in GetRACMProcedure(item.Id))
                {
                    item.RACMProcedure.Add(temp);
                }

            }
            return tList;
        }
        public IQueryable<RACM> PopulateRacmProcedureLibary(IQueryable<RACM> tList, string auditId)
        {
            var _repoplm = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
            foreach (var item in tList)
            {
                foreach (var temp in GetRACMProcedure(item.Id))
                {
                    item.RACMProcedure.Add(temp);
                }
                List<string> lstbusiness = new List<string>();
                List<string> lstprocessl1 = new List<string>();
                List<string> lstprocessl2 = new List<string>();
                var objAudit = _repoAudit.GetFirst(p => p.Id == auditId);
                if (objAudit != null)
                {
                    var objplm = _repoplm.GetFirst(p => p.Id == objAudit.ProcessLocationMapping.Id);
                    if (item.BusinessCycleId != null)
                    {
                        if (objplm.BusinessCycles != null)
                            lstbusiness.AddRange(objplm.BusinessCycles);
                        var isBusiness = lstbusiness.Contains(item.BusinessCycleId);
                        if (isBusiness)
                            tList = tList.Where(p => p.BusinessCycleId == item.BusinessCycleId);
                        else
                            tList = tList.Where(p => p.BusinessCycleId != item.BusinessCycleId);

                    }
                    else if (item.ProcessL1Id != null)
                    {
                        if (objplm.ProcessL1s != null)
                            lstprocessl1.AddRange(objplm.ProcessL1s);
                        var isProcessL1 = lstprocessl1.Contains(item.ProcessL1Id);
                        if (isProcessL1)
                            tList = tList.Where(p => p.ProcessL1Id == item.ProcessL1Id);
                        else
                            tList = tList.Where(p => p.ProcessL1Id != item.ProcessL1Id);
                    }
                    else if (item.ProcessL2Id != null)
                    {
                        if (objplm.ProcessL2s != null)
                            lstprocessl1.AddRange(objplm.ProcessL2s);
                        var isProcessL1 = lstprocessl1.Contains(item.ProcessL2Id);
                        if (isProcessL1)
                            tList = tList.Where(p => p.ProcessL2Id == item.ProcessL2Id);
                        else
                            tList = tList.Where(p => p.ProcessL2Id != item.ProcessL2Id);
                    }
                }
            }
            return tList;
        }
        public IQueryable<RACMProcedure> GetRACMProcedure(string RACMId)
        {
            var detailRepo = new MongoGenericRepository<RACMProcedure>(_dbsetting);
            var details = detailRepo.GetWithInclude<RACM>(x => x.RACMId == RACMId);

            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            foreach (var item in details)
            {
                item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
            }
            return details;
        }
        private void DeleteRacmControl(RACM racm, MongoGenericRepository<RACM> racmRepo, string userid)
        {
            if (racm.RACMModified != null)
            {
                if (racm.ControlId != racm.RACMModified.ControlId)
                {
                    var anotherRacmModified = racmRepo.GetFirst(x => x.ControlId == racm.RACMModified.ControlId || x.RACMModified.ControlId == racm.RACMModified.ControlId);
                    if (anotherRacmModified == null)
                        DeleteControl(racm.RACMModified.ControlId, userid);
                }
            }
            var anotherRacm = racmRepo.GetFirst(x => x.ControlId == racm.ControlId || x.RACMModified.ControlId == racm.ControlId);
            if (anotherRacm == null)
                DeleteControl(racm.ControlId, userid);
        }
        private void DeleteRacmRisk(RACM racm, MongoGenericRepository<RACM> racmRepo, string userid)
        {
            if (racm.RACMModified != null)
            {
                if (racm.RiskId != racm.RACMModified.RiskId)
                {
                    var anotherRacmModified = racmRepo.GetFirst(x => x.RiskId == racm.RACMModified.RiskId || x.RACMModified.RiskId == racm.RACMModified.RiskId);
                    if (anotherRacmModified == null)
                        DeleteRisk(racm.RACMModified.RiskId, userid);
                }
            }
            var anotherRacm = racmRepo.GetFirst(x => x.RiskId == racm.RiskId || x.RACMModified.RiskId == racm.RiskId);
            if (anotherRacm == null)
                DeleteRisk(racm.RiskId, userid);
        }
        public void DeleteRisk(string id, string userid)
        {
            var riskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var objeRisk = riskRepo.GetFirst(x => x.Id == id);
            riskRepo.Delete(id);
            //Activity Log
            _CommonServices.ActivityLog(userid, id, objeRisk.Description, "Risk", "Risk | Delete", "Deleted Risk");
        }
        public void DeleteControl(string id, string userid)
        {
            var controlRepo = new MongoGenericRepository<Control>(_dbsetting);
            controlRepo.Delete(id);
            var objeControl = controlRepo.GetFirst(x => x.Id == id);
            //Activity Log
            _CommonServices.ActivityLog(userid, id, objeControl.Description, "Control", "Control | Delete", "Deleted Control");
        }

        [HttpPost("InsertRACMModified")]
        public ActionResult InsertRACMModified(RACM e)
        {
            var RiskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var ControlRepo = new MongoGenericRepository<Control>(_dbsetting);
            var risk = RiskRepo.GetFirst(x => x.RiskId == e.RACMModified.Risk.RiskId && x.Title == e.RACMModified.Risk.Title && x.Description.ToLower() == e.RACMModified.Risk.Description.ToLower());
            var control = ControlRepo.GetFirst(x => x.ControlId == e.RACMModified.Control.ControlId && x.Description.ToLower() == e.RACMModified.Control.Description.ToLower() && x.Title.ToLower() == e.RACMModified.Control.Title.ToLower()); ;

            if (risk != null && control != null)
            {
                var isExist = _api.Exists(x => x.RiskId == risk.Id && x.ControlId == control.Id);

                if (isExist)
                    return ResponseError("RACM containing Risk with ID : " + e.RACMModified.Risk.RiskId + " and Control with ID : " + e.RACMModified.Control.ControlId + " already exists.");
            }

            e.RACMModified.Risk = InsertRisk(e.RACMModified.Risk, e.CreatedBy);
            e.RACMModified.RiskId = e.RACMModified.Risk.Id;

            e.RACMModified.Control = InsertControl(e.RACMModified.Control, e.CreatedBy);
            e.RACMModified.ControlId = e.RACMModified.Control.Id;

            base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "RACM", "RACM | Add", "Added RACM");
            return ResponseOK(e);
        }

        [HttpPost("InsertRACM")]
        public ActionResult InsertRACM(RACM e)
        {
            var RiskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var ControlRepo = new MongoGenericRepository<Control>(_dbsetting);
            if (e.Risk.Description != null) { }
            var risk = RiskRepo.GetFirst(x => x.RiskId == e.Risk.RiskId && x.Title == e.Risk.Title && x.Description.ToLower() == e.Risk.Description.ToLower());
            var control = ControlRepo.GetFirst(x => x.ControlId == e.Control.ControlId && x.Description.ToLower() == e.Control.Description.ToLower() && x.Title == e.Control.Title);

            if (risk != null && control != null)
            {
                var isExist = _api.Exists(x => x.RiskId == risk.Id && x.ControlId == control.Id);

                if (isExist)
                    return ResponseError("RACM containing Risk with ID : " + e.Risk.RiskId + " and Control with ID : " + e.Control.ControlId + " already exists.");
            }

            e.Risk = InsertRisk(e.Risk, e.CreatedBy);
            e.RiskId = e.Risk.Id;

            e.Control = InsertControl(e.Control, e.CreatedBy);
            e.ControlId = e.Control.Id;

            base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Risk.RiskId, "RACM", "RACM | Add", "Added RACM");
            return ResponseOK(e);
        }

        public Risk InsertRisk(Risk risk, string createdby)
        {
            var RiskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var fetchedRisk = RiskRepo.GetFirst(x => x.RiskId == risk.RiskId && x.Title == risk.Title && x.Description.ToLower() == risk.Description.ToLower());
            if (fetchedRisk != null)
            {
                risk = fetchedRisk;
            }
            else
            {
                risk.CreatedBy = createdby;
                RiskRepo.Insert(risk);
                _CommonServices.ActivityLog(createdby, risk.Id, risk.Description, "Risk", "Risk | Add", "Added Risk");
            }
            return risk;
        }

        public Control InsertControl(Control control, string createdby)
        {
            var controlRepo = new MongoGenericRepository<Control>(_dbsetting);
            var fetchedControl = controlRepo.GetFirst(x => x.ControlId == control.ControlId && x.Description.ToLower() == control.Description.ToLower() && x.Title == control.Title);
            if (fetchedControl != null)
            {
                control = fetchedControl;
            }
            else
            {
                control.CreatedBy = createdby;
                controlRepo.Insert(control);
                //Activity Log
                _CommonServices.ActivityLog(createdby, control.Id, control.Description, "Control", "Control | Add", "Added Control");
            }
            return control;
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAllWithInclude<Risk, Control, BusinessCycle, ProcessL1, ProcessL2>();

            if (tList == null)
                return ResponseNotFound();

            tList = PopulateRacmProcedure(tList);

            var repoUser = new MongoGenericRepository<User>(_dbsetting);
            var fileName = "RACM.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet wSheetRACMs = package.Workbook.Worksheets.Add("RACM");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                wSheetRACMs.Cells["A1:L1"].Style.Font.Color.SetColor(Color.Red);
                wSheetRACMs.Cells["A1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wSheetRACMs.Cells["A1:L1"].Style.Fill.BackgroundColor.SetColor(yellow);
                wSheetRACMs.Cells["A1"].Value = "RACM Number*";
                wSheetRACMs.Cells["B1"].Value = "Business Cycle*";
                wSheetRACMs.Cells["C1"].Value = "Process L1*";
                wSheetRACMs.Cells["D1"].Value = "Process L2*";
               
                wSheetRACMs.Cells["E1"].Value = "Risk ID*";
                wSheetRACMs.Cells["F1"].Value = "Risk Rating*";
                wSheetRACMs.Cells["G1"].Value = "Risk Description*";
                wSheetRACMs.Cells["H1"].Value = "Control ID*";
                wSheetRACMs.Cells["I1"].Value = "Control Type*";
                wSheetRACMs.Cells["J1"].Value = "Control Nature*";
                wSheetRACMs.Cells["K1"].Value = "Control Frequency*";
                wSheetRACMs.Cells["L1"].Value = "Control Description*";

                ExcelWorksheet wSheetRACMProcedures = package.Workbook.Worksheets.Add("RACMProcedure");
                wSheetRACMProcedures.Cells["A1:H1"].Style.Font.Color.SetColor(Color.Red);
                wSheetRACMProcedures.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wSheetRACMProcedures.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(yellow);

                wSheetRACMProcedures.Cells["A1"].Value = "Risk ID*";
                wSheetRACMProcedures.Cells["B1"].Value = "Procedure ID*";
                wSheetRACMProcedures.Cells["C1"].Value = "Procedure Title*";
                wSheetRACMProcedures.Cells["D1"].Value = "Procedure Description*";
                wSheetRACMProcedures.Cells["E1"].Value = "Start Date*";
                wSheetRACMProcedures.Cells["F1"].Value = "End Date*";
                wSheetRACMProcedures.Cells["G1"].Value = "Responsibility*";
                wSheetRACMProcedures.Cells["H1"].Value = "Reviewer*";
                var rowIndex = 2;
                var cRowIndex = 2;

                foreach (var item in tList)
                {
                    wSheetRACMs.Cells["A" + rowIndex.ToString()].Value = item.RACMnumber;
                    wSheetRACMs.Cells["B" + rowIndex.ToString()].Value = item.BusinessCycle == null ? "" : item.BusinessCycle.Name;
                    wSheetRACMs.Cells["C" + rowIndex.ToString()].Value = item.ProcessL1 == null ? "" : item.ProcessL1.Name;
                    wSheetRACMs.Cells["D" + rowIndex.ToString()].Value = item.ProcessL2 == null ? "" : item.ProcessL2.Name;
                    wSheetRACMs.Cells["E" + rowIndex.ToString()].Value = item.Risk.RiskId;
                    wSheetRACMs.Cells["F" + rowIndex.ToString()].Value = item.Risk.Rating;
                    wSheetRACMs.Cells["G" + rowIndex.ToString()].Value = item.Risk.Description;
                    wSheetRACMs.Cells["H" + rowIndex.ToString()].Value = item.Control.ControlId;
                    wSheetRACMs.Cells["I" + rowIndex.ToString()].Value = item.Control.Type;
                    wSheetRACMs.Cells["J" + rowIndex.ToString()].Value = item.Control.Nature;
                    wSheetRACMs.Cells["K" + rowIndex.ToString()].Value = item.Control.Frequency;
                    wSheetRACMs.Cells["L" + rowIndex.ToString()].Value = item.Control.Description;

                    if (item.RACMProcedure != null)
                    {
                        foreach (var subItem in item.RACMProcedure)
                        {
                            wSheetRACMProcedures.Cells["A" + cRowIndex.ToString()].Value = item.Risk.RiskId;
                            wSheetRACMProcedures.Cells["B" + cRowIndex.ToString()].Value = subItem.Procedure.ProcedureId;
                            wSheetRACMProcedures.Cells["C" + cRowIndex.ToString()].Value = subItem.Procedure.ProcedureTitle;
                            wSheetRACMProcedures.Cells["D" + cRowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(subItem.Procedure.ProcedureDesc);

                            wSheetRACMProcedures.Cells["E" + cRowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            wSheetRACMProcedures.Cells["E" + cRowIndex.ToString()].Value = Convert.ToDateTime(subItem.ProcedureStartDate);

                            wSheetRACMProcedures.Cells["F" + cRowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            wSheetRACMProcedures.Cells["F" + cRowIndex.ToString()].Value = Convert.ToDateTime(subItem.ProcedureEndDate);

                            if (subItem.ResponsibilityId != null)
                            {
                                var user = repoUser.GetByID(subItem.ResponsibilityId);

                                if (user != null)
                                    wSheetRACMProcedures.Cells["G" + cRowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                else
                                    wSheetRACMProcedures.Cells["G" + cRowIndex.ToString()].Value = "";
                            }

                            if (subItem.ReviewerId != null)
                            {
                                var user = repoUser.GetByID(subItem.ReviewerId);

                                if (user != null)
                                    wSheetRACMProcedures.Cells["H" + cRowIndex.ToString()].Value = user.FirstName + " " + user.LastName;
                                else
                                    wSheetRACMProcedures.Cells["H" + cRowIndex.ToString()].Value = "";
                            }
                            cRowIndex++;
                        }
                    }
                    rowIndex++;
                }

                wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                wSheetRACMs.Cells["A1:XFD1"].Style.Font.Bold = true;

                wSheetRACMProcedures.Cells[wSheetRACMProcedures.Dimension.Address].AutoFitColumns();
                wSheetRACMProcedures.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpGet("sampledownloadexcel")]
        public IActionResult SampleDownloadExcel()
        {
            var fileName = "RACM.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet wSheetRACMs = package.Workbook.Worksheets.Add("RACM");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                wSheetRACMs.Cells["A1:L1"].Style.Font.Color.SetColor(Color.Red);
                wSheetRACMs.Cells["A1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wSheetRACMs.Cells["A1:L1"].Style.Fill.BackgroundColor.SetColor(yellow);
                wSheetRACMs.Cells["A1"].Value = "RACM Number*";
                wSheetRACMs.Cells["B1"].Value = "Business Cycle*";
                wSheetRACMs.Cells["C1"].Value = "Process L1*";
                wSheetRACMs.Cells["D1"].Value = "Process L2*";

                wSheetRACMs.Cells["E1"].Value = "Risk ID*";
                wSheetRACMs.Cells["F1"].Value = "Risk Rating*";
                wSheetRACMs.Cells["G1"].Value = "Risk Description*";
                wSheetRACMs.Cells["H1"].Value = "Control ID*";
                wSheetRACMs.Cells["I1"].Value = "Control Type*";
                wSheetRACMs.Cells["J1"].Value = "Control Nature*";
                wSheetRACMs.Cells["K1"].Value = "Control Frequency*";
                wSheetRACMs.Cells["L1"].Value = "Control Description*";

                ExcelWorksheet wSheetRACMProcedures = package.Workbook.Worksheets.Add("RACMProcedure");
                wSheetRACMProcedures.Cells["A1:H1"].Style.Font.Color.SetColor(Color.Red);
                wSheetRACMProcedures.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wSheetRACMProcedures.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(yellow);

                wSheetRACMProcedures.Cells["A1"].Value = "Risk ID*";
                wSheetRACMProcedures.Cells["B1"].Value = "Procedure ID*";
                wSheetRACMProcedures.Cells["C1"].Value = "Procedure Title*";
                wSheetRACMProcedures.Cells["D1"].Value = "Procedure Description*";
                wSheetRACMProcedures.Cells["E1"].Value = "Start Date*";
                wSheetRACMProcedures.Cells["F1"].Value = "End Date*";
                wSheetRACMProcedures.Cells["G1"].Value = "Responsibility*";
                wSheetRACMProcedures.Cells["H1"].Value = "Reviewer*";
                wSheetRACMs.Cells[wSheetRACMs.Dimension.Address].AutoFitColumns();
                wSheetRACMs.Cells["A1:XFD1"].Style.Font.Bold = true;

                wSheetRACMProcedures.Cells[wSheetRACMProcedures.Dimension.Address].AutoFitColumns();
                wSheetRACMProcedures.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("importexcel")]
        public ActionResult ImportExcel()
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
                var repoRACMProcedure = new MongoGenericRepository<RACMProcedure>(_dbsetting);
                var repoUser = new MongoGenericRepository<User>(_dbsetting);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["RACM"];
                        var rowCount = worksheet != null ? worksheet.Dimension.Rows : 0;
                        TotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var businessCycleName = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().ToLower().Trim() : null;
                                var businessCycle = repoBusinessCycle.GetFirst(a => a.Name.ToLower().Trim() == businessCycleName);

                                var processL1Name = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().ToLower().Trim() : null;
                                var processL1 = repoProcessL1.GetFirst(a => a.Name.ToLower().Trim() == processL1Name);

                                var processL2Name = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().ToLower().Trim() : null;
                                var processL2 = repoProcessL2.GetFirst(a => a.Name.ToLower().Trim() == processL2Name);

                                var riskId = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : null;
                                var isExists = true;
                                var exists = _api.GetFirst(a => a.Risk.RiskId.Trim() == riskId);
                                if (exists == null)
                                {
                                    isExists = false;
                                    exists = new RACM();
                                }

                                exists.BusinessCycleId = businessCycle != null ? businessCycle.Id : null;
                                exists.ProcessL1Id = processL1 != null ? processL1.Id : null;
                                exists.ProcessL2Id = processL2 != null ? processL2.Id : null;

                                exists.Risk.RiskId = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row,5].Value.ToString().Trim() : "";
                                exists.Risk.Rating = worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString().Trim() : "";
                                exists.Risk.Description = worksheet.Cells[row, 7].Value != null ? worksheet.Cells[row, 7].Value.ToString().Trim() : "";

                                exists.Control.ControlId = worksheet.Cells[row,8].Value != null ? worksheet.Cells[row, 8].Value.ToString().Trim() : "";
                                exists.Control.Type = worksheet.Cells[row, 9].Value != null ? worksheet.Cells[row, 9].Value.ToString().Trim() : "";
                                exists.Control.Nature = worksheet.Cells[row, 10].Value != null ? worksheet.Cells[row, 10].Value.ToString().Trim() : "";
                                exists.Control.Frequency = worksheet.Cells[row, 11].Value != null ? worksheet.Cells[row, 11].Value.ToString().Trim() : "";
                                exists.Control.Description = worksheet.Cells[row, 12].Value != null ? worksheet.Cells[row, 12].Value.ToString().Trim() : "";

                                if (isExists)
                                    _api.Update(exists);
                                else
                                    _api.Insert(exists);
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "RACM/ImportExcel()");
                            }
                        }

                        ExcelWorksheet wSheetRAMProcedures = package.Workbook.Worksheets["RACMProcedure"];
                        rowCount = wSheetRAMProcedures != null ? wSheetRAMProcedures.Dimension.Rows : 0;
                        SubPlanTotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var riskId = wSheetRAMProcedures.Cells[row, 1].Value != null ? wSheetRAMProcedures.Cells[row, 1].Value.ToString().ToLower().Trim() : "";
                                var racm = _api.GetFirst(a => a.Risk.RiskId.ToLower().Trim() == riskId);
                                if (racm != null)
                                {
                                    var procedureId = wSheetRAMProcedures.Cells[row, 2].Value != null ? wSheetRAMProcedures.Cells[row, 2].Value.ToString().ToLower().Trim() : "";
                                    var isExists = true;
                                    var exists = repoRACMProcedure.GetFirst(a => a.RACMId == racm.Id && a.Procedure.ProcedureId.Trim().ToLower() == procedureId);
                                    if (exists == null)
                                    {
                                        isExists = false;
                                        exists = new RACMProcedure();
                                        exists.Procedure = new Procedure();
                                    }

                                    exists.RACMId = racm.Id;
                                    exists.Procedure.ProcedureId = wSheetRAMProcedures.Cells[row, 2].Value != null ? wSheetRAMProcedures.Cells[row, 2].Value.ToString().Trim() : null;
                                    exists.Procedure.ProcedureTitle = wSheetRAMProcedures.Cells[row, 3].Value != null ? wSheetRAMProcedures.Cells[row, 3].Value.ToString().Trim() : null;
                                    exists.Procedure.ProcedureDesc = wSheetRAMProcedures.Cells[row, 4].Value != null ? wSheetRAMProcedures.Cells[row, 4].Value.ToString().Trim() : null;

                                    var startDate = wSheetRAMProcedures.Cells[row, 5].Value != null ? wSheetRAMProcedures.Cells[row, 5].Text.ToString().Trim() : null;
                                    if (startDate != null)
                                        exists.ProcedureStartDate = Convert.ToDateTime(startDate).ToLocalTime();

                                    var endDate = wSheetRAMProcedures.Cells[row, 6].Value != null ? wSheetRAMProcedures.Cells[row, 6].Text.ToString().Trim() : null;
                                    if (endDate != null)
                                        exists.ProcedureEndDate = Convert.ToDateTime(endDate).ToLocalTime();

                                    var responsibility = wSheetRAMProcedures.Cells[row, 7].Value != null ? wSheetRAMProcedures.Cells[row, 7].Value.ToString().ToLower().Trim() : null;
                                    if (responsibility != null)
                                    {
                                        var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == responsibility);

                                        if (user != null)
                                            exists.ResponsibilityId = user.Id;
                                    }

                                    var reviewer = wSheetRAMProcedures.Cells[row, 8].Value != null ? wSheetRAMProcedures.Cells[row, 8].Value.ToString().ToLower().Trim() : null;
                                    if (reviewer != null)
                                    {
                                        var user = repoUser.GetFirst(a => (a.FirstName + " " + a.LastName).ToLower().Trim() == reviewer);

                                        if (user != null)
                                            exists.ReviewerId = user.Id;
                                    }

                                    if (isExists)
                                        repoRACMProcedure.Update(exists);
                                    else
                                        repoRACMProcedure.Insert(exists);
                                }
                            }
                            catch (Exception e)
                            {
                                SubPlanExceptionrowCount++;
                                sbSubPlan.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "SubActionPlanning/ImportExcel()");
                            }
                        }
                    }
                }
                var RACMmaster = new
                {
                    ExcptionCount =ExceptionrowCount,
                    ExcptionRowNumber = sb.ToString(),
                    TotalRow = TotalRow - 1,

                    SubPlanExcptionCount = SubPlanExceptionrowCount,
                    SubPlanExcptionRowNumber = sbSubPlan.ToString(),
                    SubPlanTotalRow = SubPlanTotalRow - 1,
                };
                return ResponseOK(RACMmaster); 
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "RACM/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
    }
}