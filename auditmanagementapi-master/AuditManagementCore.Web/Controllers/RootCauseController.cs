using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RootCauseController : VJBaseGenericAPIController<RootCause>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RootCauseController(IMongoGenericRepository<RootCause> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] RootCause e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());

            if (isExist)
                return ResponseError("Root cause name is already exists.");

            var country = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "RootCause", "RootCause | Add", "Added RootCause");
            return country;
        }
        public override ActionResult Put([FromBody] RootCause e)
        {
            if (e == null) return ResponseBad("RootCause object is null");
            var rootCause = _api.GetFirst(x => x.Id == e.Id);

            if (rootCause == null)
            {
                return ResponseError("RootCause does not exists");
            }
            rootCause.Name = e.Name;
            rootCause.UpdatedBy = e.UpdatedBy;
            _api.Update(rootCause);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "RootCause", "RootCause | Edit", "Updated RootCause");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoActionPlanning = new MongoGenericRepository<ActionPlanning>(_dbsetting);
                var _repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);
                var _repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
                var _repoAuditClosure = new MongoGenericRepository<AuditClosure>(_dbsetting);
                var _repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                if (id == null) return ResponseBad("RootCause object is null");
                var ObjRootCause = _api.GetFirst(x => x.Id == id);
                if (ObjRootCause == null)
                    return ResponseError("RootCause does not exists");

                var actionPlanning = _repoActionPlanning.GetFirst(x => x.RootCauseId == id);
                var discussionNote = _repoDiscussionNote.GetFirst(x => x.RootCause == id);

                if (actionPlanning != null || discussionNote != null)
                    return CustomResponseError("");

                var rc = _repoAuditClosure.GetMany(p => p.RootCause != null);
                foreach (var objRootCause in rc)
                {
                    foreach (var RootCauseID in objRootCause.RootCause)
                    {
                        if (ObjRootCause.Name == RootCauseID.name)
                            return CustomResponseError("");

                    }
                }
                var rcDraftReport = _repoDraftReport.GetMany(p => p.RootCauses != null);
                foreach (var objRootCauses in rcDraftReport)
                {
                    foreach (var RootCausesID in objRootCauses.RootCauses)
                    {
                        if (id == RootCausesID)
                            return CustomResponseError("");
                    }
                }

                var followup = _repoFollowUp.GetMany(p => p.RootCauses != null);
                foreach (var objfollowup in followup)
                {
                    foreach (var RootCausesID in objfollowup.RootCauses)
                    {
                        if (id == RootCausesID)
                            return CustomResponseError("");
                    }
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObjRootCause.Name, "RootCause", "RootCause | Delete", "Deleted RootCause");
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