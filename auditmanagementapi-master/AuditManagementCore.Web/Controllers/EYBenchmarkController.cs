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
    public class EYBenchmarkController : VJBaseGenericAPIController<EYBenchmark>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public EYBenchmarkController(IMongoGenericRepository<EYBenchmark> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] EYBenchmark e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return ResponseError("EYBenchmark with name : " + e.Name + " already exists.");
            }
            var EYBenchmark = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id,e.Name, "EYBenchmark", "EYBenchmark | Add", "Added EYBenchmark");
            return EYBenchmark;
        }

        public override ActionResult Put([FromBody] EYBenchmark e)
        {
            if (e == null) return ResponseBad("EYBenchmark object is null");
            var eybenchmark = _api.GetFirst(x => x.Id == e.Id);

            if (eybenchmark == null)
            {
                return ResponseError("EYBenchmark does not exists");
            }
            
            populateEybenchmark(eybenchmark, e);
            _api.Update(eybenchmark);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "EYBenchmark", "EYBenchmark | Edit", "Updated EYBenchmark");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("EYBenchmark object is null");
                var objEYBenchmark = _api.GetFirst(x => x.Id == id);

                if (objEYBenchmark == null)
                {
                    return ResponseError("EYBenchmark does not exists");
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objEYBenchmark.Name, "EYBenchmark", "EYBenchmark | Delete", "Deleted EYBenchmark");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        private void populateEybenchmark(EYBenchmark objEYBenchmark, EYBenchmark e)
        {
            objEYBenchmark.Name = e.Name;
            objEYBenchmark.BottomPerformance = e.BottomPerformance;
            objEYBenchmark.Median = e.Median;
            objEYBenchmark.TopPerformance = e.TopPerformance;
            objEYBenchmark.BusinessCycleId = e.BusinessCycleId;
            objEYBenchmark.ProcessL1Id = e.ProcessL1Id;
            objEYBenchmark.ProcessL2Id = e.ProcessL2Id;
            objEYBenchmark.UpdatedBy = e.UpdatedBy;

        }

        [HttpGet("GetAll")]
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<ProcessL1, BusinessCycle, ProcessL2>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }
}
