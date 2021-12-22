using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpactMasterController : VJBaseGenericAPIController<ImpactMaster>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ImpactMasterController(IMongoGenericRepository<ImpactMaster> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] ImpactMaster e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());

            if (isExist)
                return ResponseError("Impact name is already exists.");

            var ImpactMaster = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "ImpactMaster", "Master | ImpactMaster | Add", "Added ImpactMaster");
            return ImpactMaster;
        }

        public override ActionResult Put([FromBody] ImpactMaster e)
        {
            if (e == null) return ResponseBad("ImpactMaster object is null");
            var impactmaster = _api.GetFirst(x => x.Id == e.Id);

            if (impactmaster == null)
            {
                return ResponseError("ImpactMaster does not exists");
            }
            impactmaster.Name = e.Name;
            impactmaster.UpdatedBy = e.UpdatedBy;
            _api.Update(impactmaster);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "ImpactMaster", "Master | ImpactMaster | Edit", "Updated ImpactMaster");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);
                var _repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
                var _repoAuditClosure = new MongoGenericRepository<AuditClosure>(_dbsetting);
                var _repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                if (id == null) return ResponseBad("Impact object is null");
                var ObjImpact = _api.GetFirst(x => x.Id == id);
                if (ObjImpact == null)
                    return ResponseError("Impact does not exists");

                var rc = _repoAuditClosure.GetMany(p => p.Impact != null);
                foreach (var objImpact in rc)
                {
                    foreach (var ImpactID in objImpact.Impact)
                    {
                        if (ObjImpact.Name == ImpactID.name)
                            return CustomResponseError("");
                    }
                }

                var rcDraftReport = _repoDraftReport.GetMany(p => p.Impacts != null);
                foreach (var objImpacts in rcDraftReport)
                {
                    foreach (var ImpactsID in objImpacts.Impacts)
                    {
                        if (id == ImpactsID)
                            return CustomResponseError("");
                    }
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObjImpact.Name, "ImpactMaster", "Master | ImpactMaster | Delete", "Deleted ImpactMaster");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAll();

            if (tList == null)
                return ResponseNotFound();

            return ResponseOK(tList);
        }
    }
}