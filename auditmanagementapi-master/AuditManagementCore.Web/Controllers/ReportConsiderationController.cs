using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportConsiderationController : VJBaseGenericAPIController<ReportConsideration>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ReportConsiderationController(IMongoGenericRepository<ReportConsideration> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] ReportConsideration e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());

            if (isExist)
                return ResponseError("Report consideration name is already exists.");
            var ReportConsideration = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id,e.Name, "ReportConsideration", "ReportConsideration | Add", "Added ReportConsideration");
            return ReportConsideration;
        }

        public override ActionResult Put([FromBody] ReportConsideration e)
        {
            if (e == null) return ResponseBad("ReportConsideration object is null");
            var riskType = _api.GetFirst(x => x.Id == e.Id);

            if (riskType == null)
            {
                return ResponseError("ReportConsideration does not exists");
            }
            riskType.Name = e.Name;
            riskType.UpdatedBy = e.UpdatedBy;
            _api.Update(riskType);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "ReportConsideration", "ReportConsideration | Edit", "Updated ReportConsideration");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoAuditClosure = new MongoGenericRepository<AuditClosure>(_dbsetting);
                var _repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);

                if (id == null) return ResponseBad("ReportConsideration object is null");
                var ObjactionPlanning = _api.GetFirst(x => x.Id == id);
                if (ObjactionPlanning == null)
                    return ResponseError("ReportConsideration does not exists");

                var rc = _repoAuditClosure.GetMany(p => p.ReportConsideration != null);
                foreach (var objReportConsideration in rc)
                {
                    foreach (var ReportConsiderationID in objReportConsideration.ReportConsideration)
                    {
                        if (ObjactionPlanning.Name == ReportConsiderationID.name)
                            return CustomResponseError("");
                    }
                }

                var rcDraftReport = _repoDraftReport.GetMany(p => p.ReportConsiderations != null);
                foreach (var objReportConsideration in rcDraftReport)
                {
                    foreach (var ReportConsiderationID in objReportConsideration.ReportConsiderations)
                    {
                        if (id == ReportConsiderationID)
                            return CustomResponseError("");
                    }
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObjactionPlanning.Name, "ReportConsideration", "ReportConsideration | Delete", "Deleted ReportConsideration");
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