using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessCycleController : VJBaseGenericAPIController<BusinessCycle>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;

        public BusinessCycleController(IMongoGenericRepository<BusinessCycle> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] BusinessCycle e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return AlreadyExistResponseError("Business Cycle with name : " + e.Name + " already exists.");
            }
            var BusinessCycle = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "BusinessCycle", "Master | BusinessCycle | Add", "Added BusinessCycle");
            return BusinessCycle;
        }
        public override ActionResult Put([FromBody] BusinessCycle e)
        {
            if (e == null) return ResponseBad("Business Cycle object is null");
            var businessCycle = _api.GetFirst(x => x.Id == e.Id);

            if (businessCycle == null)
            {
                return ResponseError("BusinessCycle does not exists");
            }
            businessCycle.Name = e.Name;
            businessCycle.UpdatedBy = e.UpdatedBy;
            _api.Update(businessCycle);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "BusinessCycle", "Master | BusinessCycle | Edit", "Updated BusinessCycle");
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
                var _repoProcessL1 = new MongoGenericRepository<ProcessL1>(_dbsetting);
                var _repoProcessL2 = new MongoGenericRepository<ProcessL2>(_dbsetting);
                var _repoPLM = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                var _repoRACM = new MongoGenericRepository<RACM>(_dbsetting);
                var _repoRACMAuditProcedure = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);


                if (id == null) return ResponseBad("BusinessCycle object is null");
                var buisinssCycle = _api.GetFirst(x => x.Id == id);
                if (buisinssCycle == null)
                    return ResponseError("BusinessCycle does not exists");

                var ermrisk= _repoERMRisks.GetFirst(x => x.BusinessCycleID == id);
                var EYBenchmark = _repoEYBenchmark.GetFirst(x => x.BusinessCycleId == id);
                var EYBenchmarkAuditwise = _repoEYBenchmarkAuditwise.GetFirst(x => x.BusinessCycleId == id);
                var followup = _repoFollowUp.GetFirst(x => x.BusinessCycleID == id);
                var KeyBusinessInitiative = _repoKeyBusinessInitiative.GetFirst(x => x.BusinessCycleID == id);
                var overallAssesment = _repoOverallAssesment.GetFirst(x => x.BusinessCycleId == id);
                var p1 = _repoProcessL1.GetFirst(x => x.BusinessCycleId == id);
                var p2 = _repoProcessL2.GetFirst(x => x.BusinessCycleId == id);

                if (ermrisk != null || EYBenchmark != null || EYBenchmarkAuditwise != null || followup != null || KeyBusinessInitiative != null || overallAssesment != null || p1 != null || p2 != null)
                    return CustomResponseError("");

                var bc = _repoPLM.GetMany(p => p.BusinessCycles != null);
                foreach (var objBusinessCycles in bc)
                {
                    foreach (var BusinessCyclesID in objBusinessCycles.BusinessCycles)
                    {
                        if (id == BusinessCyclesID)
                            return ResponseError("BusinessCycle Reference already exists in Process Location Mapping, please try to remove it.");
                    }
                }

                var racm = _repoRACM.GetFirst(x => x.BusinessCycleId == id);
                if (racm != null)
                    return ResponseError("BusinessCycle Reference already exists in RACM , please try to remove it.");

                var RACMAuditProcedure = _repoRACMAuditProcedure.GetFirst(x => x.BusinessCycleId == id);
                if (RACMAuditProcedure != null)
                    return ResponseError("BusinessCycle Reference already exists in RACMAuditProcedure , please try to remove it.");

                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, buisinssCycle.Name, "BusinessCycle", "Master | BusinessCycle | Delete", "Deleted BusinessCycle");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }



    }
}
