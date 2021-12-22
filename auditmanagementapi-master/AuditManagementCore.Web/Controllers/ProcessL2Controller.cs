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
    public class ProcessL2Controller : VJBaseGenericAPIController<ProcessL2>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ProcessL2Controller(IMongoGenericRepository<ProcessL2> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] ProcessL2 e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower() && x.ProcessL1Id == e.ProcessL1Id);
            if (isExist)
            {
                return ResponseError("ProcessL2 with name : " + e.Name + " already exists.");
            }
            var country = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "ProcessL2", "Master | ProcessL2 | Add", "Added ProcessL2");
            return country; 
        }
        public override ActionResult Put([FromBody] ProcessL2 e)
        {
            if (e == null) return ResponseBad("ProcessL2 object is null");
            var processL2 = _api.GetFirst(x => x.Id == e.Id);

            if (processL2 == null)
            {
                return ResponseError("ProcessL2 does not exists");
            }
            processL2.Name = e.Name;
            processL2.BusinessCycleId = e.BusinessCycleId;
            processL2.ProcessL1Id = e.ProcessL1Id;
            processL2.UpdatedBy = e.UpdatedBy;
            _api.Update(processL2);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "ProcessL2", "Master | ProcessL2 | Edit", "Updated ProcessL2");
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
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var _repoRACM = new MongoGenericRepository<RACM>(_dbsetting);
                var _repoRACMAuditProcedure = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
                var _repoRisk = new MongoGenericRepository<Risk>(_dbsetting);

                if (id == null) return ResponseBad("ProcessL2 object is null");
                var ProcessL2 = _api.GetFirst(x => x.Id == id);
                if (ProcessL2 == null)
                    return ResponseError("ProcessL2 does not exists");

                var ermrisk = _repoERMRisks.GetFirst(x => x.ProcessL2Id == id);
                var EYBenchmark = _repoEYBenchmark.GetFirst(x => x.ProcessL2Id == id);
                var EYBenchmarkAuditwise = _repoEYBenchmarkAuditwise.GetFirst(x => x.ProcessL2Id == id);
                var followup = _repoFollowUp.GetFirst(x => x.ProcessL2ID == id);
                var KeyBusinessInitiative = _repoKeyBusinessInitiative.GetFirst(x => x.ProcessL2Id == id);
                var overallAssesment = _repoOverallAssesment.GetFirst(x => x.ProcessL2Id == id);
                var racm = _repoRACM.GetFirst(x => x.ProcessL2Id == id);
                var RACMAuditProcedure = _repoRACMAuditProcedure.GetFirst(x => x.ProcessL2Id == id);
                var risk = _repoRisk.GetFirst(x => x.ProcessL2Id == id);

                if (ermrisk != null || EYBenchmark != null || EYBenchmarkAuditwise != null || followup != null || KeyBusinessInitiative != null || overallAssesment != null || racm != null || RACMAuditProcedure != null || risk != null)
                    return CustomResponseError("");
                var p2 = _repoPLM.GetMany(p => p.ProcessL2s != null);
                foreach (var objp2 in p2)
                {
                    foreach (var ProcessL2ID in objp2.ProcessL2s)
                    {
                        if (id == ProcessL2ID)
                            return CustomResponseError("");
                    }
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ProcessL2.Name, "ProcessL2", "Master | ProcessL2 | Delete", "Deleted ProcessL2");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }

        [HttpGet("GetByProcessL1/{id}")]
        public ActionResult GetByProcessL1(string id)
        {
            var states = _api.GetMany(x => x.ProcessL1Id == id);
            return ResponseSuccess(states);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle, ProcessL1>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }
}
