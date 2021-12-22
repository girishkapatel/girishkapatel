using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportRACMController : VJBaseGenericAPIController<RACM>
    {
        IMongoDbSettings _dbsetting;
        private readonly IDocumentUpload _docUpload;
        public ImportRACMController(IMongoGenericRepository<RACM> api, IMongoDbSettings mongoDbSettings, IDocumentUpload documentUpload) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _docUpload = documentUpload;
        }

        [HttpPost("PostRacm")]
        public ActionResult PostRacm(IFormFile file)  //File template : Country
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<ImportRACM> ImportRACM = new List<ImportRACM>();

            var RacmRepo = new MongoGenericRepository<RACM>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString()))
                {
                    ImportRACM.Add(new ImportRACM() {
                        RiskId = csvTable.Rows[i][0].ToString(),
                        //RiskTitle = csvTable.Rows[i][1].ToString(),
                        RiskRating = csvTable.Rows[i][2].ToString(),
                        RiskDescription = csvTable.Rows[i][3].ToString(),
                        ControlId = csvTable.Rows[i][4].ToString(),                  
                        //ControlTitle = csvTable.Rows[i][5].ToString(),
                        ControlType = csvTable.Rows[i][6].ToString(),
                        ControlNature = csvTable.Rows[i][7].ToString(),
                        ControlFrequency = csvTable.Rows[i][8].ToString(),
                        ControlDescription = csvTable.Rows[i][9].ToString()
                    });
                }
            }
            foreach (var c in ImportRACM)
            {
                var Risk = new Risk()
                {
                    RiskId = c.RiskId,
                    Rating = c.RiskRating,
                    Title = c.RiskTitle,
                    Description = c.RiskDescription
                };

                var Control = new Control()
                {
                    ControlId = c.ControlId,
                    Title = c.ControlTitle,
                    Type = c.ControlType,
                    Nature = c.ControlNature,
                    Frequency = c.ControlFrequency,
                    Description = c.ControlDescription
                };

                var RACM = new RACM();
                RACM.Risk = Risk;
                RACM.Control = Control;

                InsertRACM(RACM);
            }
            _docUpload.ReleaseObject(csvTable);
            return ResponseOK(ImportRACM);
        }
        [HttpPost("InsertRACM")]
        public ActionResult InsertRACM(RACM e)
        {
            var RiskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var ControlRepo = new MongoGenericRepository<Control>(_dbsetting);
            var risk = RiskRepo.GetFirst(x => x.RiskId == e.Risk.RiskId && x.Title == e.Risk.Title);
            var control = ControlRepo.GetFirst(x => x.ControlId == e.Control.ControlId && x.Title.ToLower() == e.Control.Title.ToLower());

            if (risk != null && control != null)
            {
                var isExist = _api.Exists(x => x.RiskId == risk.Id && x.ControlId == control.Id);
                if (isExist)
                {
                    return null;
                    //Donot insert RACM
                }
            }

            e.Risk = InsertRisk(e.Risk);
            e.RiskId = e.Risk.Id;

            e.Control = InsertControl(e.Control);
            e.ControlId = e.Control.Id;

            return base.Post(e);
        }
        [HttpPost("InsertRisk")]
        public Risk InsertRisk(Risk risk)
        {
            var RiskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var fetchedRisk = RiskRepo.GetFirst(x => x.RiskId == risk.RiskId && x.Title == risk.Title && x.Description.ToLower() == risk.Description.ToLower());
            if (fetchedRisk != null)
            {
                risk = fetchedRisk;
            }
            else
            {
                RiskRepo.Insert(risk);
            }
            return risk;
        }
        [HttpPost("InsertControl")]
        public Control InsertControl(Control control)
        {
            var controlRepo = new MongoGenericRepository<Control>(_dbsetting);
            var fetchedControl = controlRepo.GetFirst(x => x.ControlId == control.ControlId && x.Description.ToLower() == control.Description.ToLower() && x.Title.ToLower() == control.Title.ToLower());
            if (fetchedControl != null)
            {
                control = fetchedControl;
            }
            else
            {
                controlRepo.Insert(control);
            }
            return control;
        }

    }
}
