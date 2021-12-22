using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EYBenchmarkAuditwiseController : VJBaseGenericAPIController<EYBenchmarkAuditwise>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public EYBenchmarkAuditwiseController(IMongoGenericRepository<EYBenchmarkAuditwise> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        [HttpPost("addFromEybmLibrary")]
        public ActionResult addFromEybmLibrary([FromBody] EYBenchmarkAuditwisePost e)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            //List<EYBenchmark> list = new List<EYBenchmark>();
            //MasterMappingServices ss = new MasterMappingServices(_dbsetting);
            //string statusCode = "500";

            foreach (var item in e.EybenchmarkdIDs)
            {
                var isExist = _api.Exists(x => x.EYBenchmarkID == item && x.AuditID == e.AuditID);

                if (!isExist)
                {
                    var eyBMRepo = new MongoGenericRepository<EYBenchmark>(_dbsetting);
                    var eyBMAuditWise = eyBMRepo.GetFirst(y => y.Id == item);
                   
                    var eybmbyaudit = new EYBenchmarkAuditwise();
                    eybmbyaudit.EYBenchmarkID = item;
                    eybmbyaudit.AuditID = e.AuditID;
                    eybmbyaudit.Name = eyBMAuditWise.Name;
                    eybmbyaudit.BottomPerformance = eyBMAuditWise.BottomPerformance;
                    eybmbyaudit.Median = eyBMAuditWise.Median;
                    eybmbyaudit.TopPerformance = eyBMAuditWise.TopPerformance;
                    eybmbyaudit.CompanyPerformance = null;
                    eybmbyaudit.BusinessCycleId = eyBMAuditWise.BusinessCycleId;
                    eybmbyaudit.ProcessL1Id = eyBMAuditWise.ProcessL1Id;
                    eybmbyaudit.ProcessL2Id = eyBMAuditWise.ProcessL2Id;
                    base.Post(eybmbyaudit);
                    //Activity Log
                    _CommonServices.ActivityLog(eybmbyaudit.CreatedBy, eybmbyaudit.Id, eyBMAuditWise.Name, "EYBenchmarkAuditwisePost", "Manage Audits | EY Benchmarks | Add From Library", "Added EYBenchmarkAuditwisePost");
                }
            }
            return ResponseOK(200);
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<Audit, ProcessL1, ProcessL2, BusinessCycle>(x => x.AuditID == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
        public override ActionResult Put([FromBody] EYBenchmarkAuditwise e)
        {
            if (e == null) return ResponseBad("EYBenchmarkAuditwise object is null");
            _api.Update(e);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "EYBenchmarkAuditwise", "Manage Audits | EY Benchmarks | Edit", "Updated EYBenchmarkAuditwise");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("EYBenchmarkAuditwise object is null");
                var objEYBenchmarkAuditwise = _api.GetFirst(x => x.Id == id);

                if (objEYBenchmarkAuditwise == null)
                {
                    return ResponseError("EYBenchmarkAuditwise does not exists");
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objEYBenchmarkAuditwise.Name, "EYBenchmarkAuditwise", "Manage Audits | EY Benchmarks | Delete", "Deleted EYBenchmarkAuditwise");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }


    }
}
