using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlPerformanceIndicatorController : VJBaseGenericAPIController<ControlPerformanceIndicator>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;

        public ControlPerformanceIndicatorController(IMongoGenericRepository<ControlPerformanceIndicator> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] ControlPerformanceIndicator e)
        {
            
            var controlPerformanceIndicator = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.ControlRating, "ControlPerformanceIndicator", "Master | Control Performance Indicator | Add", "Added ControlPerformanceIndicator");
            return controlPerformanceIndicator;
        }
        public override ActionResult Put([FromBody] ControlPerformanceIndicator e)
        {
            if (e == null) return ResponseBad("ControlPerformanceIndicator object is null");
            var location = _api.GetFirst(x => x.Id == e.Id);

            if (location == null)
            {
                return ResponseError("ControlPerformanceIndicator does not exists");
            }
            location.ControlRating = e.ControlRating;
            location.Weightage = e.Weightage;
            location.UpdatedBy = e.UpdatedBy;
            _api.Update(location);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.ControlRating, "ControlPerformanceIndicator", "Master | ControlPerformanceIndicator | Edit", "Updated ControlPerformanceIndicator");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("ControlPerformanceIndicator object is null");
                var objCPI = _api.GetFirst(x => x.Id == id);
                if (objCPI == null)
                    return ResponseError("ControlPerformanceIndicator does not exists");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objCPI.ControlRating, "ControlPerformanceIndicator", "Master | ControlPerformanceIndicator | Delete", "Deleted ControlPerformanceIndicator");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
    }
}