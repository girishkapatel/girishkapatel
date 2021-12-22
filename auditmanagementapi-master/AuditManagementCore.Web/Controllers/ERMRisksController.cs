using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using System.IO;
using OfficeOpenXml;
using AuditManagementCore.Service;
using System.Drawing;
using System.Text;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ERMRisksController : VJBaseGenericAPIController<ERMRisks>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ERMRisksController(IMongoGenericRepository<ERMRisks> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] ERMRisks e)
        {
            //var isExist = _api.Exists(x => x.BusinessCycleID == e.BusinessCycleID
            //&& x.ProcessL1ID == e.ProcessL1ID && x.RiskRating == e.RiskRating);
            //if (isExist)
            //{
            //    return ResponseError("ERM ID already exists with same business cycle, process L1 and risk rating.");
            //}
            var ERMRisks = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.ERMID, "ERMRisks", "Audit Planning Engine | ERMRisks | Add", "Added ERMRisks");
            return ERMRisks;
        }

        public override ActionResult Put([FromBody] ERMRisks e)
        {
            if (e == null) return ResponseBad("ERMRisks object is null");
            var ermrisk = _api.GetFirst(x => x.Id == e.Id);

            if (ermrisk == null)
            {
                return ResponseError("ERMRisks does not exists");
            }
            populateErmrisk(ermrisk, e);
            _api.Update(ermrisk);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.ERMID, "ERMRisks", "Audit Planning Engine | ERMRisks | Edit", "Updated ERMRisks");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            var _overallAssesRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);

            var exists = _overallAssesRepo.GetFirst(a => a.ERMRisks != null && a.ERMRisks.Id == id);
            if (exists != null)
            {
                exists.ERMRisks = null;
                _overallAssesRepo.Update(exists);
            }
            if (id == null) return ResponseBad("ERMRisks object is null");
            var objDesignation = _api.GetFirst(x => x.Id == id);

            if (objDesignation == null)
            {
                return ResponseError("ERMRisks does not exists");
            }
            var ermrisk = base.Delete(id, userid);
            //Activity Log
            _CommonServices.ActivityLog(userid, id, objDesignation.ERMID, "ERMRisks", "Audit Planning Engine | ERMRisks | Delete", "Deleted ERMRisks");
            return ermrisk;
        }
        private void populateErmrisk(ERMRisks objERMRisks, ERMRisks e)
        {
            objERMRisks.ProcessLocationMappingID = e.ProcessLocationMappingID;
            objERMRisks.ERMID = e.ERMID;
            objERMRisks.RiskRating = e.RiskRating;
            objERMRisks.RiskTitle = e.RiskTitle;
            objERMRisks.RiskDescription = e.RiskDescription;
            objERMRisks.UpdatedBy = e.UpdatedBy;
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle, ProcessL1, ProcessLocationMapping>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAll();
            if (tList == null)
                return ResponseNotFound();

            var ProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

            var fileName = "KeyERMRisks.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1:E1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1:E1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:E1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "Audit Name*";
                worksheet.Cells["B1"].Value = "ERM ID*";
                worksheet.Cells["C1"].Value = "Risk Rating*";
                worksheet.Cells["D1"].Value = "Risk Title*";
                worksheet.Cells["E1"].Value = "Risk Description*";
                var rowIndex = 2;

                foreach (var ErmRisks in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = ProcessLocationMapping.GetWithInclude<ProcessLocationMapping>(x => x.Id == ErmRisks.ProcessLocationMappingID).FirstOrDefault().AuditName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = ErmRisks.ERMID;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = ErmRisks.RiskRating;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = ErmRisks.RiskTitle;
                    worksheet.Cells["E" + rowIndex.ToString()].Value = ErmRisks.RiskDescription;
                    rowIndex++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("sampledownloadexcel")]
        public IActionResult SampleDownloadExcel()
        {
            var fileName = "KeyERMRisks.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1:E1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1:E1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:E1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "Audit Name*";
                worksheet.Cells["B1"].Value = "ERM ID*";
                worksheet.Cells["C1"].Value = "Risk Rating*";
                worksheet.Cells["D1"].Value = "Risk Title*";
                worksheet.Cells["E1"].Value = "Risk Description*";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("importexcel/{Userid}")]
        public ActionResult ImportExcel(string Userid)
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

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                        var rowCount = worksheet != null ? worksheet.Dimension.Rows : 0;
                        TotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var auditName = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : null;
                                var ermId = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().Trim() : "";
                                var riskRating = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : "";
                                var riskTitle = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : "";
                                var riskDescription = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : "";

                                var exists = _api.GetFirst(a => a.ERMID.Trim() == ermId);
                                if (exists != null)
                                {
                                    var existsMap = _processLocationMapRepo.GetFirst(a => a.Id == exists.ProcessLocationMappingID);
                                    if (existsMap != null)
                                    {
                                        existsMap.AuditName = auditName;
                                        existsMap.UpdatedBy = Userid;
                                        _processLocationMapRepo.Update(existsMap);
                                    }
                                    else
                                    {
                                        existsMap = _processLocationMapRepo.GetFirst(a => a.AuditName.Trim() == auditName);
                                        if (existsMap == null)
                                        {
                                            existsMap = new ProcessLocationMapping()
                                            {
                                                AuditName = auditName,
                                                isAll = true,
                                                isBusinessCycle = false,
                                                isProcessL1 = false,
                                                isProcessL2 = false
                                            };
                                            existsMap.CreatedBy = Userid;
                                            _processLocationMapRepo.Insert(existsMap);
                                        }
                                    }

                                    exists.ProcessLocationMappingID = existsMap.Id;
                                    exists.RiskRating = riskRating;
                                    exists.RiskTitle = riskTitle;
                                    exists.RiskDescription = riskDescription;
                                    exists.UpdatedBy = Userid;
                                    _api.Update(exists);
                                }
                                else
                                {
                                    var existsMap = _processLocationMapRepo.GetFirst(a => a.AuditName == auditName);
                                    if (existsMap == null)
                                    {
                                        existsMap = new ProcessLocationMapping()
                                        {
                                            AuditName = auditName,
                                            isAll = true,
                                            isBusinessCycle = false,
                                            isProcessL1 = false,
                                            isProcessL2 = false
                                        };
                                        existsMap.CreatedBy = Userid;
                                        _processLocationMapRepo.Insert(existsMap);
                                    }

                                    exists = new ERMRisks()
                                    {
                                        ProcessLocationMappingID = existsMap.Id,
                                        ERMID = ermId,
                                        RiskRating = riskRating,
                                        RiskTitle = riskTitle,
                                        RiskDescription = riskDescription
                                    };
                                    exists.CreatedBy = Userid;
                                    _api.Insert(exists);
                                }
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
                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "ERMRisks/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
    }
}