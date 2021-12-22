using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AuditManagementCore.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class LocationController : VJBaseGenericAPIController<Location>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public LocationController(IMongoGenericRepository<Location> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api) { _dbsetting = mongoDbSettings; _CommonServices = cs; }

        public override ActionResult Post([FromBody] Location e)
        {
            var isExist = _api.Exists(x => x.LocationID.ToLower() == e.LocationID.ToLower()
            && x.Sector == e.Sector && x.Division == e.Division);
            if (isExist)
            {
                return ResponseError("Location already exists with same sector and division.");
            }
            var country = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.LocationDescription, "Location", "Master | Location | Add", "Added Location");
            return country;
        }

        public override ActionResult Put([FromBody] Location e)
        {
            if (e == null) return ResponseBad("Location object is null");
            var location = _api.GetFirst(x => x.Id == e.Id);

            if (location == null)
            {
                return ResponseError("Location does not exists");
            }
            populateLocation(location, e);
            _api.Update(location);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.LocationDescription, "Location", "Master | Location | Edit", "Updated Location");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var _repoScopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var _repoAudit = new MongoGenericRepository<Audit>(_dbsetting);
                if (id == null) return ResponseBad("Location object is null");
                var objloc = _api.GetFirst(x => x.Id == id);
                if (objloc == null)
                    return ResponseError("Location does not exists");

                var loc = _repoPLM.GetMany(p => p.Locations != null);
                foreach (var objLocation in loc)
                {
                    foreach (var LoactionID in objLocation.Locations)
                    {
                        if (id == LoactionID)
                            return CustomResponseError("");
                    }
                }
                var scopeandschedule = _repoScopeAndSchedule.GetFirst(x => x.LocationId == id);
                if (scopeandschedule != null)
                    return CustomResponseError("");

                var lstaudit = _repoAudit.GetMany(p => p.Location != null);
                foreach (var objLocation in lstaudit)
                {
                    if (objLocation.Location != null)
                    {
                        if (id == objLocation.Location.Id)
                            return CustomResponseError("");
                    }
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objloc.LocationDescription, "Location", "Master | Location | Delete", "Deleted Location");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        private void populateLocation(Location objLocation, Location e)
        {
            objLocation.Sector = e.Sector;
            objLocation.Division = e.Division;
            objLocation.DivisionDescription = e.DivisionDescription;
            objLocation.CountryID = e.CountryID;
            objLocation.StateID = e.StateID;
            objLocation.CompanyID = e.CompanyID;
            objLocation.ProfitCenterCode = e.ProfitCenterCode;
            objLocation.LocationID = e.LocationID;
            objLocation.LocationDescription = e.LocationDescription;
            objLocation.RiskIndex = e.RiskIndex;
            objLocation.CityId = e.CityId;
            objLocation.Latitude = e.Latitude;
            objLocation.Longitude = e.Longitude;
            objLocation.Countrycode = e.Countrycode;
            objLocation.UpdatedBy = e.UpdatedBy;
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<State, Country, Company, CityOrTown>().OrderBy(a => a.ProfitCenterCode);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
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
                var repoSector = new MongoGenericRepository<Sector>(_dbsetting);
                var repoCountry = new MongoGenericRepository<Country>(_dbsetting);
                var repoState = new MongoGenericRepository<State>(_dbsetting);
                var repoCity = new MongoGenericRepository<CityOrTown>(_dbsetting);
                var repoCompany = new MongoGenericRepository<Company>(_dbsetting);

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
                                var sector = worksheet.Cells[row, 1].Value.ToString().Trim();
                                var divisionId = worksheet.Cells[row, 2].Value.ToString().Trim();
                                var divisionDescription = worksheet.Cells[row, 3].Value.ToString().Trim();
                                var country = worksheet.Cells[row, 4].Value.ToString().Trim();
                                var state = worksheet.Cells[row, 5].Value.ToString().Trim();
                                var city = worksheet.Cells[row, 6].Value.ToString().Trim();
                                var company = worksheet.Cells[row, 7].Value.ToString().Trim();
                                var profitCenterCode = worksheet.Cells[row, 8].Value.ToString().Trim();
                                var riskIndex = worksheet.Cells[row, 9].Value.ToString().Trim();
                                var locationId = worksheet.Cells[row, 10].Value.ToString().Trim();
                                var location = worksheet.Cells[row, 11].Value.ToString().Trim();

                                var exists = _api.GetFirst(a => a.ProfitCenterCode.Trim() == profitCenterCode);
                                if (exists != null)
                                {

                                    exists.Division = divisionId;
                                    exists.DivisionDescription = divisionDescription;
                                    if (sector != null)
                                    {
                                        var sectorId = repoSector.GetFirst(p => p.Name.ToLower().Contains(sector.ToLower()));
                                        exists.Sector = sectorId != null ? (sectorId.Id) : null;
                                    }
                                    if (country != null)
                                    {
                                        var countryId = repoCountry.GetFirst(p => p.Name.ToLower().Contains(country.ToLower()));
                                        exists.CountryID = countryId != null ? (countryId.Id) : null;
                                    }
                                    if (state != null)
                                    {
                                        var stateId = repoState.GetFirst(p => p.Name.ToLower().Contains(state.ToLower()));
                                        exists.StateID = stateId != null ? stateId.Id : null;
                                    }
                                    if (city != null)
                                    {
                                        var cityId = repoCity.GetFirst(p => p.Name.ToLower().Contains(city.ToLower()));
                                        exists.CityId = cityId != null ? cityId.Id : null;
                                    }
                                    if (company != null)
                                    {
                                        var companyId = repoCompany.GetFirst(p => p.Name.ToLower().Contains(company.ToLower()));
                                        exists.CompanyID = companyId != null ? companyId.Id : null;
                                    }
                                    exists.ProfitCenterCode = profitCenterCode;
                                    exists.RiskIndex = riskIndex;
                                    exists.LocationID = locationId;
                                    exists.LocationDescription = location;
                                    exists.UpdatedBy = Userid;
                                    _api.Update(exists);
                                }
                                else
                                {
                                    var objLocation = new Location();
                                    objLocation.Division = divisionId;
                                    objLocation.DivisionDescription = divisionDescription;
                                    if (sector != null)
                                    {
                                        var sectorId = repoSector.GetFirst(p => p.Name.ToLower().Contains(sector.ToLower()));
                                        objLocation.Sector = sectorId != null ? (sectorId.Id) : null;
                                    }
                                    if (country != null)
                                    {
                                        var countryId = repoCountry.GetFirst(p => p.Name.ToLower().Contains(country.ToLower()));
                                        objLocation.CountryID = countryId != null ? (countryId.Id) : null;
                                    }
                                    if (state != null)
                                    {
                                        var stateId = repoState.GetFirst(p => p.Name.ToLower().Contains(state.ToLower()));
                                        objLocation.StateID = stateId != null ? stateId.Id : null;
                                    }
                                    if (city != null)
                                    {
                                        var cityId = repoCity.GetFirst(p => p.Name.ToLower().Contains(city.ToLower()));
                                        objLocation.CityId = cityId != null ? cityId.Id : null;
                                    }
                                    if (company != null)
                                    {
                                        var companyId = repoCompany.GetFirst(p => p.Name.ToLower().Contains(company.ToLower()));
                                        objLocation.CompanyID = companyId != null ? companyId.Id : null;
                                    }
                                    objLocation.ProfitCenterCode = profitCenterCode;
                                    objLocation.RiskIndex = riskIndex;
                                    objLocation.LocationID = locationId;
                                    objLocation.LocationDescription = location;
                                    objLocation.CreatedBy = Userid;
                                    _api.Insert(objLocation);
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionrowCount++;
                                sb.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "Location/ImportExcel()");
                            }
                        }
                    }
                }
                return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1 });
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "Location/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAllWithInclude<State, Country, Company, CityOrTown>().OrderBy(a => a.ProfitCenterCode);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var repoSector = new MongoGenericRepository<Sector>(_dbsetting);
            var fileName = "Location.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Value = "Sector";
                worksheet.Cells["B1"].Value = "Division Id";
                worksheet.Cells["C1"].Value = "Division Description";
                worksheet.Cells["D1:H1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["D1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["D1:H1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["D1"].Value = "Country*";
                worksheet.Cells["E1"].Value = "State*";
                worksheet.Cells["F1"].Value = "City*";
                worksheet.Cells["G1"].Value = "Company*";
                worksheet.Cells["H1"].Value = "Profit Center Code*";
                worksheet.Cells["I1"].Value = "Risk Index";
                worksheet.Cells["J1"].Value = "Location Id";
                worksheet.Cells["K1"].Value = "Location";
                var rowIndex = 2;

                foreach (var location in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = location.Sector != null ? repoSector.GetByID(location.Sector).Name : "";
                    worksheet.Cells["B" + rowIndex.ToString()].Value = location.Division;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = location.DivisionDescription;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = location.CountryID != null ? location.Country.Name : "";
                    worksheet.Cells["E" + rowIndex.ToString()].Value = location.StateID != null ? location.State.Name : "";
                    worksheet.Cells["F" + rowIndex.ToString()].Value = location.CityId != null ? location.CityOrTown.Name : "";
                    worksheet.Cells["G" + rowIndex.ToString()].Value = location.CompanyID != null ? location.Company.Name : "";
                    worksheet.Cells["H" + rowIndex.ToString()].Value = location.ProfitCenterCode;
                    worksheet.Cells["I" + rowIndex.ToString()].Value = location.RiskIndex;
                    worksheet.Cells["J" + rowIndex.ToString()].Value = location.LocationID;
                    worksheet.Cells["K" + rowIndex.ToString()].Value = location.LocationDescription;
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
            var fileName = "Location.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Value = "Sector";
                worksheet.Cells["B1"].Value = "Division Id";
                worksheet.Cells["C1"].Value = "Division Description";
                worksheet.Cells["D1:H1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["D1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["D1:H1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["D1"].Value = "Country*";
                worksheet.Cells["E1"].Value = "State*";
                worksheet.Cells["F1"].Value = "City*";
                worksheet.Cells["G1"].Value = "Company*";
                worksheet.Cells["H1"].Value = "Profit Center Code*";
                worksheet.Cells["I1"].Value = "Risk Index";
                worksheet.Cells["J1"].Value = "Location Id";
                worksheet.Cells["K1"].Value = "Location";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}