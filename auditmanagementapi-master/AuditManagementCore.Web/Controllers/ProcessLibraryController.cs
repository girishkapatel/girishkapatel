using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessLibraryController : VJBaseGenericAPIController<ProcessLibrary>
    {
        IMongoDbSettings _dbsetting;
        public ProcessLibraryController(IMongoGenericRepository<ProcessLibrary> api, IMongoDbSettings mongoDbSettings) : base(api)
        {
            _dbsetting = mongoDbSettings;
        }

        [HttpGet("GetProcessLibrary/{id}")]
        public ActionResult GetProcessLibrary(string id)   // id:businessCycleId
        {
            var tList = _api.GetWithInclude<Sector>(x => x.BusinessCycleLibraryId == id);

            if (tList == null)
            {
                return ResponseNotFound();
            }

            var processL1LibRepo = new MongoGenericRepository<ProcessL1Library>(_dbsetting);
            var processL2LibRepo = new MongoGenericRepository<ProcessL2Library>(_dbsetting);

            foreach (var processLib in tList)
            {
                processLib.ProcessL1Libraries = processL1LibRepo.GetMany(x => x.BusinessCycleId == processLib.BusinessCycleLibraryId).ToList();

                foreach (var p1 in processLib.ProcessL1Libraries)
                {
                    p1.ProcessL2Libraries = processL2LibRepo.GetMany(x => x.BusinessCycleId == id
                                                                    && x.ProcessL1Id == p1.Id).ToList();
                }
            }
            return ResponseOK(tList);
        }

        [HttpPost("CopyToMasterBySector/id")]
        public ActionResult CopyToMasterBySector(string id)   //id:sectorId
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            try
            {
                var isExist = _api.Exists(x => x.SectorLibraryId == id);
                var processLib = new ProcessLibrary();

                if (!isExist)
                {
                    var businessCycleLibRepo = new MongoGenericRepository<BusinessCycleLibrary>(_dbsetting);
                    var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                    var processL1LibRepo = new MongoGenericRepository<ProcessL1Library>(_dbsetting);
                    var processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
                    var processL2LibRepo = new MongoGenericRepository<ProcessL2Library>(_dbsetting);
                    var processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);

                    processLib.BusinessCycleLibrary = businessCycleLibRepo.GetFirst(x => x.SectorId == id);

                    var _isExistBC = businessCycleRepo.Exists(x => x.Name.ToLower() == processLib.BusinessCycleLibrary.Name.ToLower());
                    if (!_isExistBC)
                    {
                        BusinessCycle b = new BusinessCycle
                        {
                            Name = processLib.BusinessCycleLibrary.Name
                        };

                        IEnumerable<ProcessL1Library> pL1Lib = processL1LibRepo.GetMany(x => x.BusinessCycleId == processLib.BusinessCycleLibrary.Id);

                        businessCycleRepo.Insert(b);

                        var checkBCMaster = businessCycleRepo.GetFirst(x => x.Name.ToLower() == processLib.BusinessCycleLibrary.Name.ToLower());

                        // var ListProcessL1 = new List<ProcessL1>();
                        var ProcessL1 = new ProcessL1();

                        foreach (var p in pL1Lib)
                        {
                            if (!(processL1Repo.Exists(x => x.Name.ToLower() == p.Name.ToLower() && x.BusinessCycleId == checkBCMaster.Id)))
                            {
                                ProcessL1.Name = p.Name;
                                ProcessL1.BusinessCycleId = checkBCMaster.Id;
                                ProcessL1.BusinessCycle = checkBCMaster;
                                //  ListProcessL1.Add(ProcessL1);

                                processL1Repo.Insert(ProcessL1);
                            }
                        }
                        // b.ProcessL1 = ListProcessL1;
                        var checkPL1Master = processL1Repo.GetMany(x => x.BusinessCycleId == checkBCMaster.Id);
                        var processL2 = new ProcessL2();

                        foreach (var pl1 in checkPL1Master)
                        {
                            IEnumerable<ProcessL2Library> pL2Lib = processL2LibRepo.GetMany(x => x.BusinessCycleId == processLib.BusinessCycleLibrary.Id
                            && x.ProcessL1.Name.ToLower() == pl1.Name.ToLower());

                            foreach (var pl2 in pL2Lib)
                            {
                                processL2.Name = pl2.Name;
                                processL2.ProcessL1Id = pl1.Id;
                                processL2.BusinessCycleId = pl1.BusinessCycleId;
                                processL2.ProcessL1 = pl1;
                                processL2.BusinessCycle = checkBCMaster;

                                processL2Repo.Insert(processL2);
                            }
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return ResponseError(ex.Message);
            }
        }

        [HttpPost("CopyToMasterByBusinessCycleId/id")]
        public ActionResult CopyToMasterByBusinessCycleId(string id)   //id:businessCycleId
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            try
            {
                var isExist = _api.Exists(x => x.SectorLibraryId == id);
                var processLib = new ProcessLibrary();

                if (!isExist)
                {
                    var businessCycleLibRepo = new MongoGenericRepository<BusinessCycleLibrary>(_dbsetting);
                    var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                    var processL1LibRepo = new MongoGenericRepository<ProcessL1Library>(_dbsetting);
                    var processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
                    var processL2LibRepo = new MongoGenericRepository<ProcessL2Library>(_dbsetting);
                    var processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);

                    processLib.BusinessCycleLibrary = businessCycleLibRepo.GetFirst(x => x.Id == id);

                    var _isExistBC = businessCycleRepo.Exists(x => x.Name.ToLower() == processLib.BusinessCycleLibrary.Name.ToLower());
                    if (!_isExistBC)
                    {
                        BusinessCycle b = new BusinessCycle
                        {
                            Name = processLib.BusinessCycleLibrary.Name
                        };

                        IEnumerable<ProcessL1Library> pL1Lib = processL1LibRepo.GetMany(x => x.BusinessCycleId == processLib.BusinessCycleLibrary.Id);

                        businessCycleRepo.Insert(b);

                        var checkBCMaster = businessCycleRepo.GetFirst(x => x.Name.ToLower() == processLib.BusinessCycleLibrary.Name.ToLower());

                        // var ListProcessL1 = new List<ProcessL1>();
                        var ProcessL1 = new ProcessL1();

                        foreach (var p in pL1Lib)
                        {
                            if (!(processL1Repo.Exists(x => x.Name.ToLower() == p.Name.ToLower() && x.BusinessCycleId == checkBCMaster.Id)))
                            {
                                ProcessL1.Name = p.Name;
                                ProcessL1.BusinessCycleId = checkBCMaster.Id;
                                ProcessL1.BusinessCycle = checkBCMaster;
                                //  ListProcessL1.Add(ProcessL1);

                                processL1Repo.Insert(ProcessL1);
                            }
                        }
                        // b.ProcessL1 = ListProcessL1;
                        var checkPL1Master = processL1Repo.GetMany(x => x.BusinessCycleId == checkBCMaster.Id);
                        var processL2 = new ProcessL2();

                        foreach (var pl1 in checkPL1Master)
                        {
                            IEnumerable<ProcessL2Library> pL2Lib = processL2LibRepo.GetMany(x => x.BusinessCycleId == processLib.BusinessCycleLibrary.Id
                            && x.ProcessL1.Name.ToLower() == pl1.Name.ToLower());

                            foreach (var pl2 in pL2Lib)
                            {
                                processL2.Name = pl2.Name;
                                processL2.ProcessL1Id = pl1.Id;
                                processL2.BusinessCycleId = pl1.BusinessCycleId;
                                processL2.ProcessL1 = pl1;
                                processL2.BusinessCycle = checkBCMaster;

                                processL2Repo.Insert(processL2);
                            }
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return ResponseError(ex.Message);
            }
        }
    }
}