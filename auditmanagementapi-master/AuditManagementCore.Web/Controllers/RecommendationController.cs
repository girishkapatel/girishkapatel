using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : VJBaseGenericAPIController<Recommendation>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RecommendationController(IMongoGenericRepository<Recommendation> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Recommendation e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());

            if (isExist)
                return ResponseError("Recommendation name is already exists.");
            var country = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "Recommendation", "Recommendation | Add", "Added Recommendation");
            return country; 
        }
        public override ActionResult Put([FromBody] Recommendation e)
        {
            if (e == null) return ResponseBad("Recommendation object is null");
            var recommendation = _api.GetFirst(x => x.Id == e.Id);

            if (recommendation == null)
            {
                return ResponseError("Recommendation does not exists");
            }
            recommendation.Name = e.Name;
            recommendation.UpdatedBy = e.UpdatedBy;
            _api.Update(recommendation);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "Recommendation", "Recommendation | Edit", "Updated Recommendation");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoActionPlanning = new MongoGenericRepository<ActionPlanning>(_dbsetting);
                var _repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
                var _repoAuditClosure = new MongoGenericRepository<AuditClosure>(_dbsetting);
                var _repoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                if (id == null) return ResponseBad("Recommendation object is null");
                var ObjRecommendation = _api.GetFirst(x => x.Id == id);
                if (ObjRecommendation == null)
                    return ResponseError("Recommendation does not exists");


                var rc = _repoAuditClosure.GetMany(p => p.Recommendation != null);
                foreach (var objRecommendation in rc)
                {
                    foreach (var RecommendationID in objRecommendation.Recommendation)
                    {
                        if (ObjRecommendation.Name == RecommendationID.name)
                            return CustomResponseError("");
                    }
                }

                var rcDraftReport = _repoDraftReport.GetMany(p => p.Recommendations != null);
                foreach (var objRecommendations in rcDraftReport)
                {
                    foreach (var RecommendationsID in objRecommendations.Recommendations)
                    {
                        if (id == RecommendationsID)
                            return CustomResponseError("");
                    }
                }

                var followUp = _repoFollowUp.GetFirst(x => x.Recommendation == id);
                var actionPlanning = _repoActionPlanning.GetFirst(x => x.Recommendation == id);
                if (followUp != null || actionPlanning != null)
                    return CustomResponseError("");

                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObjRecommendation.Name, "Recommendation", "Recommendation | Delete", "Deleted Recommendation");
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