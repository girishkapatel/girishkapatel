using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessRiskMappingController : VJBaseGenericAPIController<ProcessRiskMapping>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ProcessRiskMappingController(IMongoGenericRepository<ProcessRiskMapping> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] ProcessRiskMapping e)
        {
            var isExist = _api.Exists(x => x.ProcessLocationMappingID == e.ProcessLocationMappingID);
            if (isExist)
                return ResponseError("Mapping exists already.");
            var processRiskMapping = base.Post(e);
            if (e.ProcessLocationMappingID != null)
            {
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var objPLM = _repoPLM.GetFirst(p => p.Id == e.ProcessLocationMappingID);
                var auditName = objPLM.AuditName != null ? objPLM.AuditName : "";
                //Activity Log
                _CommonServices.ActivityLog(e.CreatedBy, e.Id, "Process Risk Mapping(" + auditName + ")", "ProcessRiskMapping", "Audit Planning Engine | ProcessRiskMapping | Add", "Added ProcessRiskMapping");
            }
            return processRiskMapping;
        }

        public override ActionResult Put([FromBody] ProcessRiskMapping e)
        {
            if (e == null) return ResponseBad("ProcessRiskMapping object is null");
            var prm = _api.GetFirst(x => x.Id == e.Id);

            if (prm == null)
            {
                return ResponseError("ProcessRiskMapping does not exists");
            }

            populateProcessRiskMapping(prm, e);
            _api.Update(prm);
            if (e.ProcessLocationMappingID != null)
            {
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var objPLM = _repoPLM.GetFirst(p => p.Id == e.ProcessLocationMappingID);

                //Activity Log
                var auditName = objPLM.AuditName != null ? objPLM.AuditName : "";
                _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "Process Risk Mapping(" + auditName + ")", "ProcessRiskMapping", "Audit Planning Engine | ProcessRiskMapping | Edit", "Updated ProcessRiskMapping");
            }
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("ProcessRiskMapping object is null");
                var objProcessRiskMapping = _api.GetFirst(x => x.Id == id);

                if (objProcessRiskMapping == null)
                {
                    return ResponseError("ProcessRiskMapping does not exists");
                }
                _api.Delete(id);
                if (objProcessRiskMapping.ProcessLocationMappingID != null)
                {
                    var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                    var objPLM = _repoPLM.GetFirst(p => p.Id == objProcessRiskMapping.ProcessLocationMappingID);
                    var auditName = objPLM.AuditName != null ? objPLM.AuditName : "";

                    //Activity Log
                    _CommonServices.ActivityLog(userid, id, "Process Risk Mapping(" + auditName + ")", "ProcessRiskMapping", "Audit Planning Engine | ProcessRiskMapping | Delete", "Deleted ProcessRiskMapping");
                }
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        private void populateProcessRiskMapping(ProcessRiskMapping objProcessRiskMapping, ProcessRiskMapping e)
        {
            objProcessRiskMapping.ProcessLocationMappingID = e.ProcessLocationMappingID;
            objProcessRiskMapping.QuantativeAssessment = e.QuantativeAssessment;
            objProcessRiskMapping.QualitativeAssessment = e.QualitativeAssessment;
            objProcessRiskMapping.FinalProcessrating = e.FinalProcessrating;
            objProcessRiskMapping.LocationTrialBalance = e.LocationTrialBalance;
            objProcessRiskMapping.UpdatedBy = e.UpdatedBy;
        }


        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle, ProcessL1, ProcessL2, ProcessLocationMapping>();

            if (tList == null)
                return ResponseNotFound();

            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle, ProcessL1, ProcessL2, ProcessLocationMapping>();

            if (tList == null)
                return ResponseNotFound();

            var fileName = "ProcessRiskMapping.xlsx";
            var memoryStream = new MemoryStream();

            var repoLocation = new MongoGenericRepository<Location>(_dbsetting);
            var repoCompany = new MongoGenericRepository<Company>(_dbsetting);
            var repoCityOrTown = new MongoGenericRepository<CityOrTown>(_dbsetting);

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet wSheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                wSheet.Cells["A1:D1"].Style.Font.Color.SetColor(Color.Red);
                wSheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wSheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                wSheet.Cells["A1"].Value = "Audit Name*";
                wSheet.Cells["B1"].Value = "Quantitative Assessment*";
                wSheet.Cells["C1"].Value = "Qualitative Assessment*";
                wSheet.Cells["D1"].Value = "Final Process Rating*";
                wSheet.Cells["E1"].Value = "Company Name";
                wSheet.Cells["F1"].Value = "Division";
                wSheet.Cells["G1"].Value = "Location";
                wSheet.Cells["H1"].Value = "Profit Center Code";
                wSheet.Cells["I1"].Value = "Trial Balance";

                var rowIndex = 2;

                foreach (var item in tList)
                {
                    if (item.ProcessLocationMappingID != null)
                    {
                        if (item.LocationTrialBalance != null)
                        {
                            foreach (var subItem in item.LocationTrialBalance)
                            {
                                var location = repoLocation.GetByID(subItem.LocationId);

                                if (location != null)
                                {
                                    wSheet.Cells["A" + rowIndex.ToString()].Value = item.ProcessLocationMapping.AuditName;
                                    wSheet.Cells["B" + rowIndex.ToString()].Value = item.QuantativeAssessment;
                                    wSheet.Cells["C" + rowIndex.ToString()].Value = item.QualitativeAssessment;
                                    wSheet.Cells["D" + rowIndex.ToString()].Value = item.FinalProcessrating;

                                    if (location.CompanyID != null)
                                    {
                                        var company = repoCompany.GetByID(location.CompanyID);

                                        if (company != null)
                                            wSheet.Cells["E" + rowIndex.ToString()].Value = company.Name;
                                        else
                                            wSheet.Cells["E" + rowIndex.ToString()].Value = "";
                                    }
                                    else
                                        wSheet.Cells["E" + rowIndex.ToString()].Value = "";

                                    wSheet.Cells["F" + rowIndex.ToString()].Value = location.DivisionDescription;

                                    if (location.CityId != null)
                                    {
                                        var cityOrTown = repoCityOrTown.GetByID(location.CityId);

                                        if (cityOrTown != null)
                                            wSheet.Cells["G" + rowIndex.ToString()].Value = cityOrTown.Name;
                                        else
                                            wSheet.Cells["G" + rowIndex.ToString()].Value = "";
                                    }
                                    else
                                        wSheet.Cells["G" + rowIndex.ToString()].Value = "";

                                    wSheet.Cells["H" + rowIndex.ToString()].Value = location.ProfitCenterCode;
                                    wSheet.Cells["I" + rowIndex.ToString()].Value = subItem.TrialBalance;
                                }
                                else
                                {
                                    wSheet.Cells["A" + rowIndex.ToString()].Value = item.ProcessLocationMapping.AuditName;
                                    wSheet.Cells["B" + rowIndex.ToString()].Value = item.QuantativeAssessment;
                                    wSheet.Cells["C" + rowIndex.ToString()].Value = item.QualitativeAssessment;
                                    wSheet.Cells["D" + rowIndex.ToString()].Value = item.FinalProcessrating;
                                    wSheet.Cells["E" + rowIndex.ToString()].Value = "";
                                    wSheet.Cells["F" + rowIndex.ToString()].Value = "";
                                    wSheet.Cells["G" + rowIndex.ToString()].Value = "";
                                    wSheet.Cells["H" + rowIndex.ToString()].Value = "";
                                    wSheet.Cells["I" + rowIndex.ToString()].Value = "";
                                }
                                rowIndex++;
                            }
                        }
                        else
                        {
                            wSheet.Cells["A" + rowIndex.ToString()].Value = item.ProcessLocationMapping.AuditName;
                            wSheet.Cells["B" + rowIndex.ToString()].Value = item.QuantativeAssessment;
                            wSheet.Cells["C" + rowIndex.ToString()].Value = item.QualitativeAssessment;
                            wSheet.Cells["D" + rowIndex.ToString()].Value = item.FinalProcessrating;
                            wSheet.Cells["E" + rowIndex.ToString()].Value = "";
                            wSheet.Cells["F" + rowIndex.ToString()].Value = "";
                            wSheet.Cells["G" + rowIndex.ToString()].Value = "";
                            wSheet.Cells["H" + rowIndex.ToString()].Value = "";
                            wSheet.Cells["I" + rowIndex.ToString()].Value = "";
                        }
                    }
                    else
                    {
                        wSheet.Cells["A" + rowIndex.ToString()].Value = "";
                        wSheet.Cells["B" + rowIndex.ToString()].Value = item.QuantativeAssessment;
                        wSheet.Cells["C" + rowIndex.ToString()].Value = item.QualitativeAssessment;
                        wSheet.Cells["D" + rowIndex.ToString()].Value = item.FinalProcessrating;
                        wSheet.Cells["E" + rowIndex.ToString()].Value = "";
                        wSheet.Cells["F" + rowIndex.ToString()].Value = "";
                        wSheet.Cells["G" + rowIndex.ToString()].Value = "";
                        wSheet.Cells["H" + rowIndex.ToString()].Value = "";
                        wSheet.Cells["I" + rowIndex.ToString()].Value = "";
                    }
                    rowIndex++;
                }
                wSheet.Cells[wSheet.Dimension.Address].AutoFitColumns();
                wSheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("sampledownloadexcel")]
        public IActionResult SampleDownloadExcel()
        {
            var fileName = "ProcessRiskMapping.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet wSheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                wSheet.Cells["A1:D1"].Style.Font.Color.SetColor(Color.Red);
                wSheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wSheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(yellow);
                wSheet.Cells["A1"].Value = "Audit Name*";
                wSheet.Cells["B1"].Value = "Quantitative Assessment*";
                wSheet.Cells["C1"].Value = "Qualitative Assessment*";
                wSheet.Cells["D1"].Value = "Final Process Rating*";
                wSheet.Cells["E1"].Value = "Company Name";
                wSheet.Cells["F1"].Value = "Division";
                wSheet.Cells["G1"].Value = "Location";
                wSheet.Cells["H1"].Value = "Profit Center Code";
                wSheet.Cells["I1"].Value = "Trial Balance";
                wSheet.Cells[wSheet.Dimension.Address].AutoFitColumns();
                wSheet.Cells["A1:XFD1"].Style.Font.Bold = true;
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

                var repoProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var repoCompany = new MongoGenericRepository<Company>(_dbsetting);
                var repoCityOrTown = new MongoGenericRepository<CityOrTown>(_dbsetting);
                var repoLocation = new MongoGenericRepository<Location>(_dbsetting);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var dtData = VJLiabraries.UtilityMethods.GetDataTableFromExcel(package, true);

                        if (dtData != null)
                        {
                            var dtMain = dtData.Copy();
                            dtMain.Columns.RemoveAt(8);
                            dtMain.Columns.RemoveAt(7);
                            dtMain.Columns.RemoveAt(6);
                            dtMain.Columns.RemoveAt(5);
                            dtMain.Columns.RemoveAt(4);

                            var dtTB = dtData.Copy();
                            dtTB.Columns.RemoveAt(3);
                            dtTB.Columns.RemoveAt(2);
                            dtTB.Columns.RemoveAt(1);

                            foreach (DataRow row in dtMain.Rows)
                            {
                                try
                                {


                                    var auditName = Convert.ToString(row["Audit Name"]).ToLower().Trim();
                                    var processLocationMapping = repoProcessLocationMapping.GetFirst(a => a.AuditName.Trim().ToLower() == auditName);

                                    var isExists = true;

                                    var exists = _api.GetFirst(a => a.ProcessLocationMappingID == processLocationMapping.Id);
                                    if (exists == null)
                                    {
                                        isExists = false;
                                        exists = new ProcessRiskMapping();
                                    }

                                    var locationTBs = new List<LocationTrialBalance>();
                                    var locationTBRows = dtTB.Select("[Audit Name] = '" + Convert.ToString(row["Audit Name"]) + "'");

                                    if (locationTBRows.Count() > 0)
                                    {
                                        foreach (DataRow subRow in locationTBRows)
                                        {
                                            var locationTB = new LocationTrialBalance();

                                            var location = repoLocation.GetFirst(a => a.ProfitCenterCode == Convert.ToString(subRow["Profit Center Code"]));

                                            if (location != null)
                                            {
                                                locationTB.LocationId = location.Id;
                                                locationTB.TrialBalance = 0;

                                                var tb = Convert.ToString(subRow["Trial Balance"]);
                                                var tbDouble = 0.0;

                                                if (double.TryParse(tb, out tbDouble))
                                                    locationTB.TrialBalance = tbDouble;
                                            }
                                            locationTBs.Add(locationTB);
                                        }
                                    }

                                    exists.ProcessLocationMappingID = processLocationMapping.Id;
                                    exists.QuantativeAssessment = Convert.ToString(row["Quantitative Assessment"]);
                                    exists.QualitativeAssessment = Convert.ToString(row["Qualitative Assessment"]);
                                    exists.FinalProcessrating = Convert.ToString(row["Final Process Rating"]);
                                    exists.LocationTrialBalance = locationTBs;

                                    if (isExists)
                                    {
                                        exists.UpdatedBy = Userid;
                                        _api.Update(exists);
                                    }
                                    else
                                    {
                                        exists.CreatedBy = Userid;
                                        _api.Insert(exists);
                                    }
                                }
                                catch (Exception e)
                                {
                                    ExceptionrowCount++;
                                    sb.Append(row + ",");
                                    _CommonServices.SendExcepToDB(e, "ProcessRiskMapping/ImportExcel()");
                                }
                            }
                            return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "ProcessRiskMapping/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
    }
}