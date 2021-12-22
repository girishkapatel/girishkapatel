using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiskController : VJBaseGenericAPIController<Risk>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RiskController( IMongoDbSettings mongoDbSettings,IMongoGenericRepository<Risk> api,CommonServices cs) : base(api) { _dbsetting = mongoDbSettings; _CommonServices = cs; }

        public override ActionResult Post([FromBody] Risk e)
        {
            var isExist = _api.Exists(x => x.Title.ToLower() == e.Title.ToLower() && x.ProcessL1Id == e.ProcessL1Id);
            if (isExist)
            {
                return ResponseError("Risk with name : " + e.Title + " already exists.");
            }
            var Risk = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "Risk", "Risk | Add", "Added Risk");
            return Risk; ;
        }
        public override ActionResult Put([FromBody] Risk e)
        {
            if (e == null) return ResponseBad("Risk object is null");
            var risk = _api.GetFirst(x => x.Id == e.Id);

            if (risk == null)
            {
                return ResponseError("Risk does not exists");
            }
            risk.Title = e.Title;
            risk.UpdatedBy = e.UpdatedBy;
            _api.Update(risk);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "", "Risk", "Risk | Edit", "Updated Risk");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, "Risk", "", "Risk | Delete", "Deleted Risk");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<ProcessL1, ProcessL2>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }
}
