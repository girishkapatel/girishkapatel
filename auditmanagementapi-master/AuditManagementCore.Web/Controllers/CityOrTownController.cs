using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityOrTownController : VJBaseGenericAPIController<CityOrTown>
    {
        IMongoDbSettings _dbsetting;
        IConfiguration _config;
        CommonServices _CommonServices;
        public CityOrTownController(IMongoGenericRepository<CityOrTown> api, IMongoDbSettings mongoDbSettings, IConfiguration configuration, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _config = configuration;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] CityOrTown e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower() && x.StateId == e.StateId);
            if (isExist)
            {
                return ResponseError("City or Town Name is already Exists within State");
            }
            var cityortown = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "CityOrTown", "Master | CityOrTown | Add", "Added CityOrTown");
            return cityortown;
        }
        public override ActionResult Put([FromBody] CityOrTown e)
        {
            if (e == null) return ResponseBad("CityOrTown object is null");
            var CityOrTown = _api.GetFirst(x => x.Id == e.Id);

            if (CityOrTown == null)
            {
                return ResponseError("CityOrTown does not exists");
            }
            CityOrTown.StateId = e.StateId;
            CityOrTown.Name = e.Name;
            CityOrTown.UpdatedBy = e.UpdatedBy;
            _api.Update(CityOrTown);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "CityOrTown", "Master | CityOrTown | Edit", "Updated CityOrTown");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                var _repoCompany = new MongoGenericRepository<Company>(_dbsetting);

                if (id == null) return ResponseBad("CityOrTown object is null");
                var city = _api.GetFirst(x => x.Id == id);

                if (city == null)
                {
                    return ResponseError("CityOrTown does not exists");
                }

                var loca = _repoLocation.GetFirst(x => x.CityId == id);
                var company = _repoCompany.GetFirst(x => x.CityId == id);
                if (loca != null || company != null)
                    return CustomResponseError("");

                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, city.Name, "CityOrTown", "Master | CityOrTown | Delete", "Deleted CityOrTown");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }

        [HttpGet("GetByState/{id}")]
        public ActionResult GetByState(string id)
        {
            var cities = _api.GetMany(x => x.StateId == id);
            return ResponseSuccess(cities);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<State, Country>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAllWithInclude<Country,State>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var fileName = "City.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Value = "Country *";
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["B1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["B1"].Value = "State *";
                worksheet.Cells["B1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["B1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["C1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["C1"].Value = "City *";
                worksheet.Cells["C1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["C1"].Style.Fill.BackgroundColor.SetColor(yellow);
                var rowIndex = 2;
                foreach (var city in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = city.Name;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = city.Country.Name;
                    worksheet.Cells["C" + rowIndex.ToString()].Value = city.State.Name;
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
        public IActionResult sampleDownloadExcel()
        {
            var fileName = "City.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Value = "Country *";
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["B1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["B1"].Value = "State *";
                worksheet.Cells["B1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["B1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["C1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["C1"].Value = "City *";
                worksheet.Cells["C1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["C1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpPost("importexcel")]
        public ActionResult ImportExcel()
        {
            if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
            {
                return ResponseError("formfile is empty");
            }

            var file = Request.Form.Files[0];

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return ResponseError("Not Support file extension");
            }

            var countries = _CommonServices.GetAllCountries();
            var states = _CommonServices.GetAllState();
            var cities = new List<CityOrTown>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                    var rowCount = worksheet != null ? worksheet.Dimension.Rows : 0;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        cities.Add(new CityOrTown
                        {
                            CountryId = countries.FirstOrDefault(x => x.Name == worksheet.Cells[row, 1].Value.ToString().Trim()).Id,
                            StateId = states.FirstOrDefault(x => x.Name == worksheet.Cells[row, 2].Value.ToString().Trim() &&
                             x.CountryId == countries.FirstOrDefault(x => x.Name == worksheet.Cells[row, 1].Value.ToString().Trim()).Id).Id,
                            Name = worksheet.Cells[row, 3].Value.ToString().Trim()
                        });
                    }
                }
            }

            foreach (var c in cities)
            {
                if (!_api.Exists(x => x.Name.Trim() == c.Name.Trim() && x.StateId == c.StateId && x.CountryId == c.CountryId))
                    _api.Insert(c);
            }

            return ResponseOK(new object[0]);
        }
    }
}