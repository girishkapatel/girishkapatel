using System;
using System.Collections.Generic;
using System.Linq;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using OfficeOpenXml;
using AuditManagementCore.Service;
using System.Drawing;
using System.Text;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyBusinessInitiativeController : VJBaseGenericAPIController<KeyBusinessInitiative>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public KeyBusinessInitiativeController(IMongoGenericRepository<KeyBusinessInitiative> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] KeyBusinessInitiative e)
        {
            var isExist = _api.Exists(x => x.BusinessInitiativeID == e.BusinessInitiativeID
            && x.BusinessCycleID == e.BusinessCycleID
            && x.ProcessL1ID == e.ProcessL1ID && x.RiskRating == e.RiskRating);
            if (isExist)
            {
                return ResponseError("Business Initiative ID already exists with same business cycle, process L1 and risk rating.");
            }
            var keyBusinessInitiative = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.BusinessInitiativeID, "KeyBusinessInitiative", "Audit Planning Engine | KeyBusinessInitiative | Add", "Added KeyBusinessInitiative");
            return keyBusinessInitiative;
        }
        public override ActionResult Put([FromBody] KeyBusinessInitiative e)
        {
            if (e == null) return ResponseBad("KeyBusinessInitiative object is null");
            var keyBusinessInitiative = _api.GetFirst(x => x.Id == e.Id);

            if (keyBusinessInitiative == null)
            {
                return ResponseError("KeyBusinessInitiative does not exists");
            }
            KeyBusinessInitiative obj = new KeyBusinessInitiative();
            populateKeyBusinessInitiative(keyBusinessInitiative, e);
            _api.Update(keyBusinessInitiative);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.BusinessInitiativeID, "KeyBusinessInitiative", "Audit Planning Engine | KeyBusinessInitiative | Edit", "Updated KeyBusinessInitiative");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("KeyBusinessInitiative object is null");
                var objKeyBusinessInitiative = _api.GetFirst(x => x.Id == id);

                if (objKeyBusinessInitiative == null)
                {
                    return ResponseError("KeyBusinessInitiative does not exists");
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objKeyBusinessInitiative.BusinessInitiativeID, "KeyBusinessInitiative", "Audit Planning Engine | KeyBusinessInitiative | Delete", "Deleted KeyBusinessInitiative");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        private void populateKeyBusinessInitiative(KeyBusinessInitiative objkeyBusinessInitiative, KeyBusinessInitiative e)
        {
            objkeyBusinessInitiative.BusinessInitiativeID = e.BusinessInitiativeID;
            objkeyBusinessInitiative.ProcessLocationMappingID = e.ProcessLocationMappingID;
            objkeyBusinessInitiative.RiskRating = e.RiskRating;
            objkeyBusinessInitiative.BusinessIntiativeDescription = e.BusinessIntiativeDescription;
            objkeyBusinessInitiative.UpdatedBy = e.UpdatedBy;
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
            {
                return ResponseNotFound();
            }
            var ProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var fileName = "KeyBusinessInitiatives.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1:D1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "Audit Name*";
                worksheet.Cells["B1"].Value = "Business Initiative ID*";
                worksheet.Cells["C1"].Value = "Risk Rating*";
                worksheet.Cells["D1"].Value = "Business Initiative Description*";
                var rowIndex = 2;
                foreach (var KeyBusinessInitiative in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = ProcessLocationMapping.GetWithInclude<ProcessLocationMapping>(x => x.Id == KeyBusinessInitiative.ProcessLocationMappingID).FirstOrDefault().AuditName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = KeyBusinessInitiative.BusinessInitiativeID;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = KeyBusinessInitiative.RiskRating;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = KeyBusinessInitiative.BusinessIntiativeDescription;
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
            var fileName = "KeyBusinessInitiatives.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1:D1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "Audit Name*";
                worksheet.Cells["B1"].Value = "Business Initiative ID*";
                worksheet.Cells["C1"].Value = "Risk Rating*";
                worksheet.Cells["D1"].Value = "Business Initiative Description*";
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
                                var auditName = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : "";
                                var businessId = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().Trim() : "";
                                var riskRating = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : "";
                                var description = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : "";

                                var exists = _api.GetFirst(a => a.BusinessInitiativeID.Trim() == businessId);
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
                                    exists.BusinessIntiativeDescription = description;
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
                                        existsMap.UpdatedBy = Userid;
                                        _processLocationMapRepo.Insert(existsMap);
                                    }

                                    exists = new KeyBusinessInitiative()
                                    {
                                        ProcessLocationMappingID = existsMap.Id,
                                        BusinessInitiativeID = businessId,
                                        RiskRating = riskRating,
                                        BusinessIntiativeDescription = description
                                    };
                                    exists.CreatedBy = Userid;
                                    _api.Insert(exists);
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "KeyBusinessInitiative/ImportExcel()");
                            }
                        }
                    }
                }
                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
            }
            catch (Exception e)
            { 
                _CommonServices.SendExcepToDB(e, "KeyBusinessInitiative/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
    }
}