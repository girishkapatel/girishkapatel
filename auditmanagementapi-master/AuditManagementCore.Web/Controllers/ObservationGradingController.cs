using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ObservationGradingController : VJBaseGenericAPIController<ObservationGrading>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ObservationGradingController(IMongoGenericRepository<ObservationGrading> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api) { _dbsetting = mongoDbSettings; _CommonServices = cs; }

        public override ActionResult Post([FromBody] ObservationGrading e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());

            if (isExist)
                return ResponseError("Observation grading name is already exists.");
            var observationGrading = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "ObservationGrading", "Master | ObservationGrading | Add", "Added ObservationGrading");
            return observationGrading;
        }
        public override ActionResult Put([FromBody] ObservationGrading e)
        {
            if (e == null) return ResponseBad("ObservationGrading object is null");
            var observationgrading = _api.GetFirst(x => x.Id == e.Id);

            if (observationgrading == null)
            {
                return ResponseError("ObservationGrading does not exists");
            }
            observationgrading.Name = e.Name;
            observationgrading.UpdatedBy = e.UpdatedBy;
            _api.Update(observationgrading);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "ObservationGrading", "Master | ObservationGrading | Edit", "Updated ObservationGrading");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("ObservationGrading object is null");
                var ObjObservationGrading = _api.GetFirst(x => x.Id == id);
                if (ObjObservationGrading == null)
                    return ResponseError("ObservationGrading does not exists");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObjObservationGrading.Name, "ObservationGrading", "Master | ObservationGrading | Delete", "Deleted ObservationGrading");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetManyQueryable(p => p.Name != null && p.Name != "");

            if (tList == null)
                return ResponseNotFound();

            return ResponseOK(tList);
        }
    }
}