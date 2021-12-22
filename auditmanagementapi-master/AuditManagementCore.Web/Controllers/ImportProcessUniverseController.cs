using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.ViewModels;
using LumenWorks.Framework.IO.Csv;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportProcessUniverseController : VJBaseGenericAPIController<ProcessL1Library>
    {

        IMongoDbSettings _dbsetting;
        private readonly IDocumentUpload _docUpload;
        public ImportProcessUniverseController(IMongoGenericRepository<ProcessL1Library> api, IMongoDbSettings mongoDbSettings, IDocumentUpload documentUpload) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _docUpload = documentUpload;
        }


        [HttpPost("PostSectorLibrary")]
        public ActionResult PostSectorLibrary(IFormFile file)   //File template : Sector
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<SectorLibrary> import = new List<SectorLibrary>();

            var SLRepo = new MongoGenericRepository<SectorLibrary>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString()))
                {
                    import.Add(new SectorLibrary() { Name = csvTable.Rows[i][0].ToString() });
                }
            }
            foreach (var c in import)
            {
                if (!SLRepo.Exists(x => x.Name == c.Name))
                {
                    SLRepo.Insert(c);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }


        [HttpPost("PostBusinessCycleLibrary")]
        public ActionResult PostBusinessCycleLibrary(IFormFile file)  //File template : SectorName,BusincessCyleName
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<BusinessCycleLibrary> import = new List<BusinessCycleLibrary>();
            var SLRepo = new MongoGenericRepository<SectorLibrary>(_dbsetting);
            var BCLRepo = new MongoGenericRepository<BusinessCycleLibrary>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString())
                    && !String.IsNullOrWhiteSpace( csvTable.Rows[i][1].ToString()))
                {
                    import.Add(new BusinessCycleLibrary() { Name = csvTable.Rows[i][1].ToString(),
                     SectorLibrary = new SectorLibrary() { Name= csvTable.Rows[i][0].ToString() }  });
                }
            }
            foreach (var i in import)
            {
                var sect = new SectorLibrary();
                
                if (!BCLRepo.Exists(x => x.Name == i.Name && x.SectorLibrary.Name == i.SectorLibrary.Name ))
                { 
                    if(!SLRepo.Exists(x=>x.Name == i.SectorLibrary.Name))
                    { sect.Name = i.SectorLibrary.Name;
                        SLRepo.Insert(sect);
                    }
                    i.SectorLibrary = SLRepo.GetFirst(x=>x.Name== i.SectorLibrary.Name);
                    i.SectorId = i.SectorLibrary.Id;
                    BCLRepo.Insert(i);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }


        [HttpPost("PostProcessL1Library")]
        public ActionResult PostProcessL1Library(IFormFile file)  //File template : BusincessCyleName,ProcessL1
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<ProcessL1Library> import = new List<ProcessL1Library>();
           
            var BCLRepo = new MongoGenericRepository<BusinessCycleLibrary>(_dbsetting);
            var PL1Repo = new MongoGenericRepository<ProcessL1Library>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString())
                    && !String.IsNullOrWhiteSpace(csvTable.Rows[i][1].ToString()))
                {
                    import.Add(new ProcessL1Library()
                    {
                        Name = csvTable.Rows[i][1].ToString(),
                        BusinessCycle = new BusinessCycleLibrary() { Name = csvTable.Rows[i][0].ToString() }
                    });
                }
            }
            foreach (var i in import)
            {
                var BC = new BusinessCycleLibrary();

                if (!PL1Repo.Exists(x => x.Name == i.Name && x.BusinessCycle.Name == i.BusinessCycle.Name))
                {
                    if (!BCLRepo.Exists(x => x.Name == i.BusinessCycle.Name))
                    {
                        BC.Name = i.BusinessCycle.Name;
                        BCLRepo.Insert(BC);
                    }
                    i.BusinessCycle = BCLRepo.GetFirst(x => x.Name == i.BusinessCycle.Name);
                    i.BusinessCycleId = i.BusinessCycle.Id;
                    i.Name = i.Name;
                    PL1Repo.Insert(i);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }

        [HttpPost("PostProcessL2Library")]
        public ActionResult PostProcessL2Library(IFormFile file)  //File template : BusincessCyleName,ProcessL1,ProcessL2
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<ProcessL2Library> import = new List<ProcessL2Library>();
            var SLRepo = new MongoGenericRepository<SectorLibrary>(_dbsetting);
            var BCLRepo = new MongoGenericRepository<BusinessCycleLibrary>(_dbsetting);
            var PL1Repo = new MongoGenericRepository<ProcessL1Library>(_dbsetting);
            var PL2Repo = new MongoGenericRepository<ProcessL2Library>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString())
                    && !String.IsNullOrWhiteSpace(csvTable.Rows[i][1].ToString()))
                {
                    import.Add(new ProcessL2Library()
                    {
                        Name = csvTable.Rows[i][2].ToString(),
                        ProcessL1 = new ProcessL1Library() { Name= csvTable.Rows[i][1].ToString() },
                        BusinessCycle = new BusinessCycleLibrary() { Name = csvTable.Rows[i][0].ToString() }
                    });
                }
            }
            foreach (var i in import)
            {
                var BC = new BusinessCycleLibrary();
                var p1 = new ProcessL1Library();

                if (!PL2Repo.Exists(x => x.Name == i.Name && x.BusinessCycle.Name == i.BusinessCycle.Name && x.ProcessL1.Name==i.ProcessL1.Name))
                {
                    if (!PL1Repo.Exists(x => x.Name == i.ProcessL1.Name && x.BusinessCycle.Name==i.BusinessCycle.Name))
                    {
                        if(!BCLRepo.Exists(x=>x.Name == i.BusinessCycle.Name))
                        {
                            BC.Name = i.BusinessCycle.Name;
                            BCLRepo.Insert(BC);
                        }
                        p1.BusinessCycle = BCLRepo.GetFirst(x => x.Name == i.BusinessCycle.Name);
                        p1.BusinessCycleId = p1.BusinessCycle.Id;
                        p1.Name = i.ProcessL1.Name;
                        PL1Repo.Insert(p1);
                    }
                    
                    i.BusinessCycle = BCLRepo.GetFirst(x => x.Name == i.BusinessCycle.Name);
                    i.BusinessCycleId = i.BusinessCycle.Id;
                    i.ProcessL1 = PL1Repo.GetFirst(x => x.Name == i.ProcessL1.Name && x.BusinessCycleId ==i.BusinessCycleId);
                    
                    i.ProcessL1Id = i.ProcessL1.Id;
                    
                    PL2Repo.Insert(i);

                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }

    }
}