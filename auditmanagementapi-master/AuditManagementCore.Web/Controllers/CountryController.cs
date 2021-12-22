using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using VJLiabraries;
using VJLiabraries.Wrappers;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : VJBaseGenericAPIController<Country>
    {
        IMongoDbSettings _dbsetting;
        IConfiguration _config;
        CommonServices _CommonServices;
        public CountryController(IMongoGenericRepository<Country> api, IMongoDbSettings mongoDbSettings, IConfiguration configuration, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _config = configuration;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Country e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return ResponseError("Country Name is already Exists");
            }
            var country = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "Country", "Master | Country | Add", "Added Country");
            return country;
        }


        public override ActionResult Put([FromBody] Country e)
        {
            if (e == null) return ResponseBad("Country object is null");
            var country = _api.GetFirst(x => x.Id == e.Id);

            if (country == null)
            {
                return ResponseError("Country does not exists");
            }
            country.Name = e.Name;
            country.UpdatedBy = e.UpdatedBy;
            _api.Update(country);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "Country", "Master | Country | Edit", "Updated Country");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoState = new MongoGenericRepository<State>(_dbsetting);
                var _repoCity = new MongoGenericRepository<CityOrTown>(_dbsetting);
                var _repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                var _repoCompany = new MongoGenericRepository<Company>(_dbsetting);
                if (id == null) return ResponseBad("Country object is null");
                var country = _api.GetFirst(x => x.Id == id);
                if (country == null)
                    return ResponseError("Country does not exists");

                var state = _repoState.GetFirst(x => x.CountryId == id);
                var city = _repoCity.GetFirst(x => x.CountryId == id);
                var loca = _repoLocation.GetFirst(x => x.CountryID == id);
                var company = _repoCompany.GetFirst(x => x.CountryId == id);

                if (state != null || city != null || loca != null || company != null)
                    return CustomResponseError("Country Reference already exists in state,city,location,company ! please try to remove it");

                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, country.Name, "Country", "Master | Country | Delete", "Deleted Country");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }


        public override ActionResult GetByID(string id)
        {
            var s = _CommonServices.GetCountryById(id.ToString());
            if (s != null)
                return ResponseOK(s);
            else
                return ResponseNotFound();
        }

        public override ActionResult GetAll()
        {
            var s = _CommonServices.GetAllCountries();
            if (s != null)
                return ResponseOK(s);
            else
                return ResponseNotFound();
        }

        [HttpPost("download")]
        public async Task<IActionResult> Download()
        {
            var s = _CommonServices.GetAllCountries().Select(x => new { x.Id, x.Name });
            var csvString = UtilityMethods.ToCsv(s);
            var location = _config.GetSection("FileLocation").Value;
            var fileName = "Country.csv";
            var memString = await _CommonServices.GetMemoryStream(csvString, location, fileName);
            return File(memString, "application/octet-stream", Path.GetFileName(fileName));
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _CommonServices.GetAllCountries().Select(x => new { x.Name });
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var fileName = "Country.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Value = "Name *";
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                var rowIndex = 2;

                foreach (var country in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = country.Name;
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
            var fileName = "Country.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells["A1"].Value = "Name *";
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
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
            int ExceptionrowCount = 0;
            int TotalRow = 0;
            StringBuilder sb = new StringBuilder();
            try
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

                var countries = new List<Country>();

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
                            countries.Add(new Country
                            {
                                Name = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            });
                        }
                    }
                }

                foreach (var c in countries)
                {
                    try
                    {

                        if (!_api.Exists(x => x.Name.Trim() == c.Name.Trim()))
                            _api.Insert(c);
                    }
                    catch (Exception e)
                    {
                        ExceptionrowCount++;
                        sb.Append(c.Name + ",");
                        _CommonServices.SendExcepToDB(e, "ImportExcel()");
                    }
                }
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "ImportExcel()");
            }
            return ResponseOK(new { ExcptionCount = ExceptionrowCount, ExcptionRowNumber = sb.ToString(), TotalRow = TotalRow - 1, status = "Ok" });
        }
    }
}