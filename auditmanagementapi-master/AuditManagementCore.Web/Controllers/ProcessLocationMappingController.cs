using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using System;
using AuditManagementCore.Service;

namespace AuditManagementCore.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class ProcessLocationMappingController : VJBaseGenericAPIController<ProcessLocationMapping>
    {
        IMongoDbSettings _mongoDbSettings;

        public ProcessLocationMappingController(IMongoGenericRepository<ProcessLocationMapping> api, IMongoDbSettings mongoDbSettings) : base(api)
        {
            _mongoDbSettings = mongoDbSettings;
        }

        public override ActionResult Post([FromBody] ProcessLocationMapping e)
        {
            // var isExist = _api.Exists(x => x.BusinessCycleID.ToLower() == e.BusinessCycleID.ToLower()
            // && x.ProcessL1ID == e.ProcessL1ID && x.ProcessL2ID == e.ProcessL2ID
            //);

            if (e.AuditName == "")
            {
                return ResponseError("Audit name cannot be blank. Please enter valid audit name.");
            }

            var isExist = _api.Exists(x => x.AuditName.ToLower() == e.AuditName.ToLower());
            if (isExist)
            {
                return ResponseError("Audit name is already exists. Please enter another unique audit name.");
            }
            var country = base.Post(e);
            //Activity Log
            CommonServices obj = new CommonServices(_mongoDbSettings);
            obj.ActivityLog(e.CreatedBy, e.Id, e.AuditName, "ProcessLocationMapping", "Master | ProcessLocationMapping | Add", "Added ProcessLocationMapping");
            return country;
        }

        public override ActionResult Put([FromBody] ProcessLocationMapping e)
        {
            if (e == null) return ResponseBad("ProcessLocationMapping object is null");
            var plm = _api.GetFirst(x => x.Id == e.Id);

            if (plm == null)
            {
                return ResponseError("ProcessLocationMapping does not exists");
            }
            populatePLM(plm, e);
            _api.Update(plm);
            //Activity Log
            CommonServices obj = new CommonServices(_mongoDbSettings);
            obj.ActivityLog(e.UpdatedBy, e.Id, e.AuditName, "ProcessLocationMapping", "Master | ProcessLocationMapping | Edit", "Updated ProcessLocationMapping");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoAudit = new MongoGenericRepository<Audit>(_mongoDbSettings);
                var _repoERMRisks = new MongoGenericRepository<ERMRisks>(_mongoDbSettings);
                var _repoFollowUp = new MongoGenericRepository<FollowUp>(_mongoDbSettings);
                var _repoKeyBusinessInitiative = new MongoGenericRepository<KeyBusinessInitiative>(_mongoDbSettings);
                var _repoOverallAssesment = new MongoGenericRepository<OverallAssesment>(_mongoDbSettings);
                var _repoProcessRiskMapping = new MongoGenericRepository<ProcessRiskMapping>(_mongoDbSettings);
                var _repoScopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_mongoDbSettings);
                var _repoTrialBalance = new MongoGenericRepository<TrialBalance>(_mongoDbSettings);

                if (id == null) return ResponseBad("ProcessLocationMapping object is null");
                var ProcessLocationMapping = _api.GetFirst(x => x.Id == id);
                if (ProcessLocationMapping == null)
                    return ResponseError("ProcessLocationMapping does not exists");

                var plm = _repoAudit.GetMany(p => p.ProcessLocationMapping != null);
                foreach (var objplm in plm)
                {
                    if (id == objplm.ProcessLocationMapping.Id)
                        return CustomResponseError("");
                }

                var objrisk = _repoERMRisks.GetFirst(x => x.ProcessLocationMappingID == id);
                var followup = _repoFollowUp.GetFirst(x => x.ProcessLocationMappingId == id);
                var KeyBusinessInitiative = _repoKeyBusinessInitiative.GetFirst(x => x.ProcessLocationMappingID == id);
                var overallAssesment = _repoOverallAssesment.GetFirst(x => x.ProcessLocationMappingID == id);
                var processRiskMapping = _repoProcessRiskMapping.GetFirst(x => x.ProcessLocationMappingID == id);
                var ScopeAndSchedule = _repoScopeAndSchedule.GetFirst(x => x.ProcessLocationMappingId == id);
                var trialbalance = _repoTrialBalance.GetFirst(x => x.ProcessLocationMappingId == id);

                if (objrisk != null || followup != null || KeyBusinessInitiative != null || overallAssesment != null || processRiskMapping != null || ScopeAndSchedule != null || trialbalance != null)
                    return CustomResponseError("");
                _api.Delete(id);
                //Activity Log
                CommonServices obj = new CommonServices(_mongoDbSettings);
                obj.ActivityLog(userid, id, ProcessLocationMapping.AuditName, "ProcessLocationMapping", "Master | ProcessLocationMapping | Delete", "Deleted ProcessLocationMapping");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }


        private void populatePLM(ProcessLocationMapping objPlm, ProcessLocationMapping e)
        {
            objPlm.AuditName = e.AuditName;
            objPlm.isAll = e.isAll;
            objPlm.isBusinessCycle = e.isBusinessCycle;
            objPlm.isProcessL1 = e.isProcessL1;
            objPlm.isProcessL2 = e.isProcessL2;
            objPlm.Locations = e.Locations;
            objPlm.BusinessCycles = e.BusinessCycles;
            objPlm.ProcessL1s = e.ProcessL1s;
            objPlm.ProcessL2s = e.ProcessL2s;
            objPlm.UpdatedBy = e.UpdatedBy;
        }


        [HttpGet("GetByProcessL1/{id}")]
        public ActionResult GetByProcessL1(string id)
        {
            var process = _api.GetWithInclude<Location>(x => x.ProcessL1ID == id);
            var locRepo = new MongoGenericRepository<Location>(_mongoDbSettings);
            foreach (var p in process)
            {
                //p.LocationDetails = locRepo.GetWithInclude<Company, State, Country, CityOrTown>(x => p.Locations.Contains(x.Id)).ToList();
                var locations = locRepo.GetWithInclude<Company, State, Country, CityOrTown>(x => p.Locations.Contains(x.Id)).ToList();

                foreach (var locatoin in locations)
                {
                    p.LocationDetails.Add(new LocationTB()
                    {
                        Location = locatoin,
                        TrialBalance = 0

                    });
                }
            }
            return ResponseSuccess(process);

        }

        [HttpGet("GetByAuditName/{id}")]

        public ActionResult GetByAuditName(string id)
        {
            var process = _api.GetWithInclude<Location>(x => x.Id == id);

            var locRepo = new MongoGenericRepository<Location>(_mongoDbSettings);
            var trialRepo = new MongoGenericRepository<TrialBalance>(_mongoDbSettings);
            var processRiskMappingRepo = new MongoGenericRepository<ProcessRiskMapping>(_mongoDbSettings);

            foreach (var p in process)
            {
                //p.LocationDetails = locRepo.GetWithInclude<Company, State, Country, CityOrTown>(x => p.Locations.Contains(x.Id)).ToList();
                var locationList = new List<LocationTB>();

                var locations = locRepo.GetWithInclude<Company, State, Country, CityOrTown>(x => p.Locations.Contains(x.Id)).ToList();
                var processRiskMapping = processRiskMappingRepo.GetFirst(a => a.ProcessLocationMappingID == id);

                foreach (var location in locations)
                {
                    var locationTB = new LocationTB()
                    {
                        Location = location,
                        TrialBalance = 0
                    };

                    if (processRiskMapping != null)
                    {
                        var isExists = processRiskMapping.LocationTrialBalance.FirstOrDefault(a => a.LocationId == location.Id);

                        if (isExists != null)
                            locationTB.TrialBalance =
                                isExists == null ? trialRepo.GetFirst(a => a.LocationId == location.Id).TrialBalances : isExists.TrialBalance;
                    }
                    else
                    {
                        var isExists = trialRepo.GetFirst(a => a.LocationId == location.Id);

                        locationTB.TrialBalance = isExists == null ? 0 : isExists.TrialBalances;
                    }

                    locationList.Add(locationTB);
                }

                p.LocationDetails = locationList;
            }
            return ResponseSuccess(process);

        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle, ProcessL1, ProcessL2>();
            if (tList == null)
                return ResponseNotFound();

            return ResponseOK(tList);
        }

        [HttpGet("getlocationsbyplmapid/{id}")]
        public IActionResult GetLocationsByPLMapId(string id)
        {
            var locations = new List<Location>();

            var mapping = _api.GetByID(id);

            if (mapping == null)
                return ResponseNotFound();

            var locationRepo = new MongoGenericRepository<Location>(_mongoDbSettings);

            if (mapping.Locations != null)
            {
                foreach (var location in mapping.Locations)
                {
                    var loc = locationRepo.GetByID(location);

                    if (loc != null)
                        locations.Add(loc);
                }
            }

            return ResponseOK(locations);
        }
    }
}