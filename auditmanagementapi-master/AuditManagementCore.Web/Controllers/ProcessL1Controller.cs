using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessL1Controller : VJBaseGenericAPIController<ProcessL1>
    {
        IMongoDbSettings _dbsetting; 
        CommonServices _CommonServices;
        public ProcessL1Controller(IMongoGenericRepository<ProcessL1> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] ProcessL1 e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower() && x.BusinessCycleId == e.BusinessCycleId);
            if (isExist)
            {
                return ResponseError("ProcessL1 with name : " + e.Name + " already exists.");
            }
            var ProcessL1 = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "ProcessL1", "Master | ProcessL1 | Add", "Added ProcessL1");
            return ProcessL1;
        }
        public override ActionResult Put([FromBody] ProcessL1 e)
        {
            if (e == null) return ResponseBad("ProcessL1 object is null");
            var processL1 = _api.GetFirst(x => x.Id == e.Id);

            if (processL1 == null)
            {
                return ResponseError("ProcessL1 does not exists");
            }
            processL1.Name = e.Name;
            processL1.BusinessCycleId = e.BusinessCycleId;
            processL1.UpdatedBy = e.UpdatedBy;
            _api.Update(processL1);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "ProcessL1", "Master | ProcessL1 | Edit", "Updated ProcessL1");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoERMRisks = new MongoGenericRepository<ERMRisks>(_dbsetting);
                var _repoEYBenchmark = new MongoGenericRepository<EYBenchmark>(_dbsetting);
                var _repoEYBenchmarkAuditwise = new MongoGenericRepository<EYBenchmarkAuditwise>(_dbsetting);
                var _repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                var _repoKeyBusinessInitiative = new MongoGenericRepository<KeyBusinessInitiative>(_dbsetting);
                var _repoOverallAssesment = new MongoGenericRepository<OverallAssesment>(_dbsetting);
                var _repoProcessL2 = new MongoGenericRepository<ProcessL2>(_dbsetting);
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var _repoRACM = new MongoGenericRepository<RACM>(_dbsetting);
                var _repoRACMAuditProcedure = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
                var _repoRisk = new MongoGenericRepository<Risk>(_dbsetting);

                if (id == null) return ResponseBad("ProcessL1 object is null");
                var ProcessL1 = _api.GetFirst(x => x.Id == id);
                if (ProcessL1 == null)
                    return ResponseError("ProcessL1 does not exists");

                var ermrisk = _repoERMRisks.GetFirst(x => x.ProcessL1ID == id);
                var EYBenchmark = _repoEYBenchmark.GetFirst(x => x.ProcessL1Id == id);
                var EYBenchmarkAuditwise = _repoEYBenchmarkAuditwise.GetFirst(x => x.ProcessL1Id == id);
                var followup = _repoFollowUp.GetFirst(x => x.ProcessL1ID == id);
                var KeyBusinessInitiative = _repoKeyBusinessInitiative.GetFirst(x => x.ProcessL1ID == id);
                var overallAssesment = _repoOverallAssesment.GetFirst(x => x.ProcessL1Id == id);
                var p2 = _repoProcessL2.GetFirst(x => x.ProcessL1Id == id);
                var racm = _repoRACM.GetFirst(x => x.ProcessL1Id == id);
                var RACMAuditProcedure = _repoRACMAuditProcedure.GetFirst(x => x.ProcessL1Id == id);
                var risk = _repoRisk.GetFirst(x => x.ProcessL1Id == id);
                if (ermrisk != null || EYBenchmark != null || EYBenchmarkAuditwise != null || followup != null || KeyBusinessInitiative != null || overallAssesment != null || p2 != null || racm != null || RACMAuditProcedure != null || risk != null)
                    return CustomResponseError("");

                var bc = _repoPLM.GetMany(p => p.ProcessL1s != null);
                foreach (var objBusinessCycles in bc)
                {
                    foreach (var ProcessL1ID in objBusinessCycles.ProcessL1s)
                    {
                        if (id == ProcessL1ID)
                            return CustomResponseError("");
                    }
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ProcessL1.Name, "ProcessL1", "Master | ProcessL1 | Delete", "Deleted ProcessL1");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }


        [HttpGet("GetByBusinessCycle/{id}")]
        public ActionResult GetByBusinessCycle(string id)
        {
            var states = _api.GetMany(x => x.BusinessCycleId == id);
            return ResponseSuccess(states);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }
}
