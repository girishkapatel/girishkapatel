using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDocumentUpload _docUpload;
        IMongoDbSettings _dbsetting;
        IConfiguration _config;
        IEmailUtility _emailUtility;
        CommonServices _CommonServices;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDocumentUpload documentUpload, IMongoDbSettings mongoDbSettings, IConfiguration configuration, IEmailUtility e, CommonServices cs)
        {
            _logger = logger;
            _docUpload = documentUpload;
            _dbsetting = mongoDbSettings;
            _config = configuration;
            _emailUtility = e;
            _CommonServices = cs;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)] + "5"
            })
            .ToArray();
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            //sample upload controller
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var res = _docUpload.Upload(file);
            res.Wait();
            return Ok();

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

        [HttpPost("sendMail")]
        public async Task<IActionResult> sendMail()
        {
            string body = "<html><head><title>Demystifying Email Design</title></head><body><h1>EHLLLOOO</h1></body></html>";
            _emailUtility.SendEmail(
                new EmailModel()
                {
                    FromEmail = "gautvi@gmail.com",
                    ToEmail = new List<string>() { "mayursasp.net@gmail.com" },
                    //CcEmail = new List<string>() { "gautvi@gmail.com" },
                    Subject = "Test mail",
                    MailBody = body
                });
            return Ok();
        }
    }
}
