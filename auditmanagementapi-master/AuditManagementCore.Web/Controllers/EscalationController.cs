using System;
using System.Collections.Generic;
using System.Linq;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscalationController : VJBaseGenericAPIController<Escalation>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public EscalationController(IMongoGenericRepository<Escalation> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Escalation e)
        {
            //var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());
            //if (isExist)
            //{
            //    return ResponseError("Audit escalation is already exists.");
            //}
            var escalation = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id,e.Module, "Escalation", "Master | Escalation | Add", "Added Escalation");
            return escalation;
        }
        public override ActionResult Put([FromBody] Escalation e)
        {
            if (e == null) return ResponseBad("Escalation object is null");
            var sector = _api.GetFirst(x => x.Id == e.Id);

            if (sector == null)
            {
                return ResponseError("Escalation does not exists");
            }
            sector.Module = e.Module;
            sector.EscalationRules = e.EscalationRules;
            sector.UpdatedBy = e.UpdatedBy;
            _api.Update(sector);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Module, "Escalation", "Master | Escalation | Edit", "Updated Escalation");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("Escalation object is null");
                var ObEscalation = _api.GetFirst(x => x.Id == id);
                if (ObEscalation == null)
                    return ResponseError("Escalation does not exists");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, ObEscalation.Module, "Escalation", "Master | Escalation | Delete", "Deleted Escalation");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Audit>();

            if (tList == null)
                return ResponseNotFound();

            return ResponseOK(tList);
        }

        [HttpGet("GetByAuditaID/{processlocationid}")]
        public ActionResult GetByAuditaID(string processlocationid)
        {
            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var overallAssesmentRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);


            var lstScope = scopeRepo.GetAll().Where(p => p.AuditId == processlocationid);

            var audits = new List<Audit>();
            foreach (var item in lstScope)
            {
                audits.Add(auditRepo.GetByID(item.AuditId));
            }
            var lstoverallassOverallAssesment = overallAssesmentRepo.GetAll().Where(p => p.ProcessLocationMappingID == processlocationid);

            var lstReturn = from a in audits join loa in lstoverallassOverallAssesment on a.OverallAssesmentId equals loa.Id select loa;

            return ResponseOK(lstReturn);
        }

        [HttpGet("getlocationsbyplmap/{plmapId}")]
        public IActionResult GetLocationsByPlmapId(string plmapId)
        {
            var overallAssesmentRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);

            var overallAssesments = overallAssesmentRepo.GetMany(a => a.ProcessLocationMappingID == plmapId);

            if (overallAssesments == null)
                return ResponseNotFound();

            var auditsList = new List<Audit>();

            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);

            foreach (var item in overallAssesments)
            {
                var audit = auditRepo.GetFirst(a => a.OverallAssesmentId == item.Id);

                if (audit != null)
                    auditsList.Add(audit);
            }

            return ResponseOK(auditsList);
        }
        [HttpGet("getescaltedto/{plmapId}")]
        public IActionResult GetEscaltedTo(string plmapId)
        {
            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var userRepo = new MongoGenericRepository<User>(_dbsetting);


            var lstAudit = scopeRepo.GetMany(a => a.ProcessLocationMappingId == plmapId);

            if (lstAudit == null)
                return ResponseNotFound();
            var lstuser = userRepo.GetAll();
            if (lstuser == null)
                return ResponseNotFound();
            var auditsList = new List<User>();
            foreach (var auditItem in lstAudit)
            {
                if (auditItem.AuditApprovalMapping != null)
                {
                    foreach (var userItem in auditItem.AuditApprovalMapping.UserData)
                    {
                        var lstApproverUser = lstuser.Where(p => p.Id == userItem.UserId);
                        auditsList.AddRange(lstApproverUser);

                    }
                }
                if (auditItem.AuditResources != null)
                {
                    foreach (var userItem in auditItem.AuditResources)
                    {
                        var lstResourceUser = lstuser.Where(p => p.Id == userItem.UserId);
                        auditsList.AddRange(lstResourceUser);

                    }
                }
                if (auditItem.Auditees != null)
                {
                    foreach (var userItem in auditItem.Auditees)
                    {
                        var lstAuditeesUser = lstuser.Where(p => p.Id == userItem.UserId);
                        auditsList.AddRange(lstAuditeesUser);

                    }
                }

            }
            return ResponseOK(auditsList.Distinct());
        }
    }
}