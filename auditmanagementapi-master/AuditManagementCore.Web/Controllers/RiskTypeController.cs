using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiskTypeController : VJBaseGenericAPIController<RiskType>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RiskTypeController(IMongoGenericRepository<RiskType> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] RiskType e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
                return ResponseError("Risk type name is already exists.");

            var RiskType = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id,e.Name, "RiskType", "Master | RiskType | Add", "Added RiskType");
            return RiskType;
        }
        public override ActionResult Put([FromBody] RiskType e)
        {
            if (e == null) return ResponseBad("RiskType object is null");
            var riskType = _api.GetFirst(x => x.Id == e.Id);

            if (riskType == null)
            {
                return ResponseError("RiskType does not exists");
            }
            riskType.Name = e.Name;
            riskType.UpdatedBy = e.UpdatedBy;
            _api.Update(riskType);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "RiskType", "Master | RiskType | Edit", "Updated RiskType");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoActionPlanning = new MongoGenericRepository<ActionPlanning>(_dbsetting);
                var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);
                var _repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                if (id == null) return ResponseBad("RiskType object is null");
                var riskType = _api.GetFirst(x => x.Id == id);
                if (riskType == null)
                    return ResponseError("RiskType  does not exists");

                var actionPlanning = _repoActionPlanning.GetFirst(x => x.RiskTypeId == id);
                var discussionNote = _repoDiscussionNote.GetFirst(x => x.RiskTypeId == id);
                var followUp = _repoFollowUp.GetFirst(x => x.RiskTypeId == id);
                if (actionPlanning != null || discussionNote != null || followUp != null)
                    return CustomResponseError("");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, riskType.Name, "RiskType", "Master | RiskType | Delete", "Deleted RiskType");
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