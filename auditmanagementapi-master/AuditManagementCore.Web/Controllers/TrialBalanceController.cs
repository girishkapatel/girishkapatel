using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrialBalanceController : VJBaseGenericAPIController<TrialBalance>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        #endregion

        public TrialBalanceController(IMongoGenericRepository<TrialBalance> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] TrialBalance e)
        {
            var isExist = _api.Exists(x => x.GLCode == e.GLCode);

            if (isExist)
                return ResponseError("GLCode exists already.");
            var trialBalance = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "TrialBalance", "Master | TrialBalance | Add", "Added TrialBalance");
            return trialBalance;
        }

        [HttpPost("savetrialbalance")]
        public ActionResult SaveTrialBalance([FromBody] List<TrialBalance> e)
        {
            foreach (var tb in e)
            {
                base.Post(tb);
                //Activity Log
                _CommonServices.ActivityLog(tb.CreatedBy, tb.Id, tb.GLCode, "TrialBalance", "Master | TrialBalance | Add", "Added TrialBalance");
            }
            return ResponseOK(e);
        }

        public override ActionResult Put([FromBody] TrialBalance e)
        {
            if (e == null) return ResponseBad("TrialBalance object is null");
            var tbalance = _api.GetFirst(x => x.Id == e.Id);

            if (tbalance == null)
            {
                return ResponseError("TrialBalance does not exists");
            }

            populateTrialBalance(tbalance, e);
            _api.Update(tbalance);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.GLCode, "TrialBalance", "Master | TrialBalance | Edit", "Updated TrialBalance");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("TrialBalance object is null");
                var ObjTrialBalance = _api.GetFirst(x => x.Id == id);
                if (ObjTrialBalance == null)
                    return ResponseError("TrialBalance does not exists");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObjTrialBalance.GLCode, "TrialBalance", "Master | TrialBalance | Delete", "Deleted TrialBalance");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        private void populateTrialBalance(TrialBalance trialBalance, TrialBalance e)
        {
            trialBalance.GLCode = e.GLCode;
            trialBalance.GLClass = e.GLClass;
            trialBalance.GLDescription = e.GLDescription;
            trialBalance.ProcessLocationMappingId = e.ProcessLocationMappingId;
            trialBalance.LocationId = e.LocationId;
            trialBalance.SectorId = e.SectorId;
            trialBalance.UpdatedBy = e.UpdatedBy;
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<ProcessLocationMapping, Location, Sector>();
            if (tList == null)
            {
                return ResponseNotFound();
            }

            foreach (var item in tList)
            {
                foreach (var ItemlocactionId in item.ProcessLocationMapping.Locations)
                {
                    item.Location = _CommonServices.GetLocationDetail(ItemlocactionId);
                }
                //item.Location = mp.GetLocationDetail(item.ProcessLocationMapping.);
            }
            return ResponseOK(tList);
        }

        [HttpPost("importexcel")]
        public ActionResult ImportExcel()
        {
            if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                return ResponseError("formfile is empty");

            var file = Request.Form.Files[0];

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return ResponseError("Not Support file extension");

            var _processLocationMapRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var _locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            var _sectorRepo = new MongoGenericRepository<Sector>(_dbsetting);

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        var rowCount = worksheet.Dimension.Rows;
                        var columnCount = worksheet.Dimension.Columns;

                        //var dtData = generateDataTable();

                        for (int row = 5; row <= rowCount; row++)
                        {
                            for (int col = 7; col <= columnCount; col++)
                            {
                                if (col % 2 != 0)
                                {
                                    if (worksheet.Cells[1, col].Value == null || worksheet.Cells[1, col].Value.ToString().Trim() == "")
                                        break;

                                    //var newRow = dtData.NewRow();

                                    //newRow["GLCode"] = worksheet.Cells[row, 1].Value.ToString().Trim();
                                    //newRow["GLClass"] = worksheet.Cells[row, 2].Value.ToString().Trim();
                                    //newRow["GLDescription"] = worksheet.Cells[row, 3].Value.ToString().Trim();
                                    //newRow["AuditName"] = worksheet.Cells[row, 4].Value.ToString().Trim();
                                    //newRow["Balance"] = worksheet.Cells[row, 5].Value.ToString().Trim();
                                    //newRow["MaterialAccount"] = worksheet.Cells[row, 6].Value.ToString().Trim();

                                    //newRow["ProfitCenter"] = worksheet.Cells[1, col].Value.ToString().Trim();
                                    //newRow["ProfitCenterName"] = worksheet.Cells[2, col].Value.ToString().Trim();
                                    //newRow["Location"] = worksheet.Cells[3, col].Value.ToString().Trim();

                                    //newRow["LocationBalance"] = worksheet.Cells[row, col].Value.ToString().Trim();

                                    //dtData.Rows.Add(newRow);

                                    var auditname = worksheet.Cells[row, 4].Value.ToString().Trim();
                                    var balance = worksheet.Cells[row, 5].Value.ToString().Trim();
                                    var meterialaccount = worksheet.Cells[row, 6].Value.ToString().Trim() == "Yes" ? true : false;

                                    var ProfitCenter = worksheet.Cells[1, col].Value.ToString().Trim();
                                    var ProfitCenterName = worksheet.Cells[2, col].Value.ToString().Trim();
                                    var Location = worksheet.Cells[3, col].Value.ToString().Trim();

                                    var locationBalance = worksheet.Cells[row, col].Value.ToString().Trim();

                                    var locationexists = _locationRepo.GetFirst(p => p.ProfitCenterCode == ProfitCenter && p.LocationDescription == Location);
                                    var processLocationMapping = _processLocationMapRepo.GetFirst(a => a.AuditName.Trim() == auditname);
                                    var sector = _sectorRepo.GetFirst(s => s.Name == ProfitCenterName);

                                    var exists = new TrialBalance()
                                    {
                                        GLCode = worksheet.Cells[row, 1].Value.ToString().Trim(),
                                        GLClass = worksheet.Cells[row, 2].Value.ToString().Trim(),
                                        GLDescription = worksheet.Cells[row, 3].Value.ToString().Trim(),
                                        MainBalance = Convert.ToDouble(worksheet.Cells[row, 5].Value.ToString().Trim()),
                                        MaterialAccount = Convert.ToBoolean(meterialaccount),
                                        TrialBalances = Convert.ToDouble(locationBalance)
                                    };

                                    if (locationexists != null)
                                        exists.LocationId = locationexists.Id;

                                    if (processLocationMapping != null)
                                        exists.ProcessLocationMappingId = processLocationMapping.Id;

                                    if (sector != null)
                                        exists.SectorId = sector.Id;

                                    _api.Insert(exists);

                                    //var exists = _api.GetFirst(a => a.Audit.AuditName == auditname);
                                    //if (exists != null)
                                    //{
                                    //    exists.TrialBalances += Convert.ToDecimal(balance);
                                    //    _api.Update(exists);
                                    //}
                                    //else
                                    //{
                                    //    var locationexists = _locationRepo.GetFirst(p => p.ProfitCenterCode == ProfitCenter && p.LocationDescription == Location && p.DivisionDescription == ProfitCenterName);
                                    //    var auditexists = _auditRepo.GetFirst(a => a.AuditName.Trim() == auditname);
                                    //    exists = new TrialBalance()
                                    //    {
                                    //        GLCode = worksheet.Cells[row, 1].Value.ToString().Trim(),
                                    //        GLClass = worksheet.Cells[row, 2].Value.ToString().Trim(),
                                    //        GLDescription = worksheet.Cells[row, 3].Value.ToString().Trim(),

                                    //        TrialBalances = Convert.ToDecimal(balance),
                                    //        MaterialAccount = Convert.ToBoolean(meterialaccount)
                                    //    };
                                    //    if (locationexists != null)
                                    //        exists.LocationId = locationexists.Id;
                                    //    if (auditexists != null)
                                    //        exists.AuditId = auditexists.Id;

                                    //    _api.Insert(exists);
                                    //}
                                }
                            }
                        }

                    }
                }
            }

            return ResponseOK(new object[0]);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var locationobj = new MongoGenericRepository<Location>(_dbsetting);

            var tList = _api.GetAllWithInclude<ProcessLocationMapping, Location, Sector>().Take(100);
            if (tList == null)
                return ResponseNotFound();

            var fileName = "TrialBalance.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                #region worksheet header
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "GL Codes*";
                worksheet.Cells["A1:A4"].Merge = true;
                //worksheet.Cells["A1:A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                worksheet.Cells["B1"].Value = "GL Class";
                worksheet.Cells["B1:B4"].Merge = true;
                //worksheet.Cells["B1:B4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["C1"].Value = "Profit Center";
                worksheet.Cells["C2"].Value = "Profit Center Name";
                worksheet.Cells["C3"].Value = "Location";
                worksheet.Cells["C4"].Value = "GL Description";

                worksheet.Cells["D1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["D1"].Value = "Audit Name*";
                worksheet.Cells["D1:D4"].Merge = true;
                //worksheet.Cells["D1:D3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["E1"].Value = "Balance";
                worksheet.Cells["E1:E4"].Merge = true;
                //worksheet.Cells["E1:E3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["F1"].Value = "Material Account";
                worksheet.Cells["F1:F3"].Merge = true;
                worksheet.Cells["F1:F3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["F4"].Value = "Yes or No (Based  on Limit)";
                worksheet.Cells["F4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                #endregion

                var dataRowIndex = 5;
                var prevGLCode = string.Empty;
                foreach (var item in tList)
                {
                    if (prevGLCode != item.GLCode)
                    {
                        worksheet.Cells["A" + dataRowIndex.ToString()].Value = item.GLCode;
                        worksheet.Cells["B" + dataRowIndex.ToString()].Value = item.GLClass;
                        worksheet.Cells["C" + dataRowIndex.ToString()].Value = item.GLDescription;
                        worksheet.Cells["D" + dataRowIndex.ToString()].Value = item.ProcessLocationMapping != null ? item.ProcessLocationMapping.AuditName : "";
                        worksheet.Cells["E" + dataRowIndex.ToString()].Value = item.MainBalance;
                        worksheet.Cells["F" + dataRowIndex.ToString()].Value = item.MaterialAccount;
                        var colIndex = 7;
                        var locations = tList.Where(a => a.GLCode == item.GLCode && a.LocationId != null);
                        if (locations != null & locations.Count() > 0)
                        {
                            foreach (var location in locations)
                            {
                                worksheet.Cells[1, colIndex].Value = location.Location.ProfitCenterCode;
                                worksheet.Cells[1, colIndex, 1, (colIndex + 1)].Merge = true;

                                worksheet.Cells[2, colIndex].Value = location.Sector.Name;
                                worksheet.Cells[2, colIndex, 2, (colIndex + 1)].Merge = true;

                                worksheet.Cells[3, colIndex].Value = location.Location.LocationDescription;
                                worksheet.Cells[3, colIndex, 3, (colIndex + 1)].Merge = true;

                                worksheet.Cells[4, colIndex].Value = "Balance";
                                worksheet.Cells[4, (colIndex + 1)].Value = "%";

                                worksheet.Cells[(dataRowIndex), colIndex].Value = location.TrialBalances.ToString("N2") + " INR";
                                worksheet.Cells[(dataRowIndex), (colIndex + 1)].Value = 0;
                                colIndex += 2;
                            }
                        }
                        dataRowIndex++;
                    }
                    prevGLCode = item.GLCode;
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
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var locationobj = new MongoGenericRepository<Location>(_dbsetting);
            var fileName = "TrialBalance.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                #region worksheet header
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["A1"].Value = "GL Codes*";
                worksheet.Cells["A1:A4"].Merge = true;
                //worksheet.Cells["A1:A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                worksheet.Cells["B1"].Value = "GL Class";
                worksheet.Cells["B1:B4"].Merge = true;
                //worksheet.Cells["B1:B4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["C1"].Value = "Profit Center";
                worksheet.Cells["C2"].Value = "Profit Center Name";
                worksheet.Cells["C3"].Value = "Location";
                worksheet.Cells["C4"].Value = "GL Description";

                worksheet.Cells["D1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["D1"].Value = "Audit Name*";
                worksheet.Cells["D1:D4"].Merge = true;
                //worksheet.Cells["D1:D3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["E1"].Value = "Balance";
                worksheet.Cells["E1:E4"].Merge = true;
                //worksheet.Cells["E1:E3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["F1"].Value = "Material Account";
                worksheet.Cells["F1:F3"].Merge = true;
                worksheet.Cells["F1:F3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["F4"].Value = "Yes or No (Based  on Limit)";
                worksheet.Cells["F4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                #endregion
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public DataTable generateDataTable()
        {
            var returnDt = new DataTable();

            returnDt.Columns.Add("GLCode", typeof(String));
            returnDt.Columns.Add("GLClass", typeof(String));
            returnDt.Columns.Add("GLDescription", typeof(String));
            returnDt.Columns.Add("AuditName", typeof(String));
            returnDt.Columns.Add("Balance", typeof(String));
            returnDt.Columns.Add("MaterialAccount", typeof(String));
            returnDt.Columns.Add("ProfitCenter", typeof(String));
            returnDt.Columns.Add("ProfitCenterName", typeof(String));
            returnDt.Columns.Add("Location", typeof(String));
            returnDt.Columns.Add("LocationBalance", typeof(String));

            return returnDt;
        }
    }
}