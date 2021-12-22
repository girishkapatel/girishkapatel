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
    public class StateController : VJBaseGenericAPIController<State>
    {
        IMongoDbSettings _dbsetting;
        IConfiguration _config;
        CommonServices _CommonServices;
        public StateController(IMongoGenericRepository<State> api, IMongoDbSettings mongoDbSettings, IConfiguration configuration, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _config = configuration;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] State e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower() && x.CountryId == e.CountryId);
            if (isExist)
            {
                return ResponseError("State Name is already Exists within Country");
            }
            var state = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id,e.Name, "State", "Master | State | Add", "Added State");
            return state; 
        }

        public override ActionResult Put([FromBody] State e)
        {
            if (e == null) return ResponseBad("State object is null");
            var state = _api.GetFirst(x => x.Id == e.Id);

            if (state == null)
            {
                return ResponseError("State does not exists");
            }
            state.CountryId = e.CountryId;
            state.Name = e.Name;
            state.UpdatedBy = e.UpdatedBy;
            _api.Update(state);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id,e.Name, "State", "Master | State | Edit", "Updated State");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoCity = new MongoGenericRepository<CityOrTown>(_dbsetting);
                var _repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                var _repoCompany = new MongoGenericRepository<Company>(_dbsetting);

                if (id == null) return ResponseBad("State object is null");
                var state = _api.GetFirst(x => x.Id == id);

                if (state == null)
                {
                    return ResponseError("State does not exists");
                }

                var city = _repoCity.GetFirst(x => x.StateId == id);
                var loca = _repoLocation.GetFirst(x => x.StateID == id);
                var company = _repoCompany.GetFirst(x => x.StateId == id);
                if (city != null || loca != null || company != null)
                    return CustomResponseError("");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, state.Name, "State", "Master | State | Delete", "Deleted State");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }

        [HttpGet("GetByCountry/{id}")]
        public ActionResult GetByCountry(string id)
        {
            var states = _api.GetWithInclude<Country>(x => x.CountryId == id);
            return ResponseSuccess(states);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Country>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAllWithInclude<Country>(); 
            if (tList == null)
            {
                return ResponseNotFound();
            }

            var fileName = "State.xlsx";
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
                var rowIndex = 2;
                foreach (var state in tList)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = state.Name;
                    worksheet.Cells["B" + rowIndex.ToString()].Value =state.Country.Name;
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
            var fileName = "State.xlsx";
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
            var states = new List<State>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                    var rowCount = worksheet != null ? worksheet.Dimension.Rows : 0;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        states.Add(new State
                        {
                            //Country = countries.FirstOrDefault(x => x.Name == worksheet.Cells[row, 1].Value.ToString().Trim()),
                            CountryId = countries.FirstOrDefault(x => x.Name == worksheet.Cells[row, 1].Value.ToString().Trim()).Id,
                            Name = worksheet.Cells[row, 2].Value.ToString().Trim()
                        });
                    }
                }
            }

            foreach (var s in states)
            {
                if (!_api.Exists(x => x.Name == s.Name && x.CountryId == s.CountryId))
                    _api.Insert(s);
            }

            return ResponseOK(new object[0]);
        }
    }
}