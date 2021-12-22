using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RACMProcedureController : VJBaseGenericAPIController<RACMProcedure>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RACMProcedureController(IMongoGenericRepository<RACMProcedure> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] RACMProcedure e)
        {
            var isExist = _api.Exists(x => x.Id == e.Id);

            if (isExist)
                return ResponseError("RACM Procedure with ID: " + e.Id + " already exists.");

            e.ProcedureStartDate = e.ProcedureStartDate?.ToLocalTime();
            e.ProcedureEndDate = e.ProcedureEndDate?.ToLocalTime();

            var RACMProcedure = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "RACMProcedure", "RACMProcedure | Add", "Added RACMProcedure");
            return RACMProcedure;
        }

        public override ActionResult Put([FromBody] RACMProcedure e)
        {
            e.ProcedureStartDate = e.ProcedureStartDate?.ToLocalTime();
            e.ProcedureEndDate = e.ProcedureEndDate?.ToLocalTime();

            var racmProcedure = base.Put(e);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, "", "RACMProcedure", "RACMProcedure | Edit", "Updated RACMProcedure");
            return racmProcedure;
        }

        [HttpPost("Multiple")]
        public ActionResult AddMultiple([FromBody] List<RACMProcedure> e)
        {
            foreach (var item in e)
            {
                var isExist = _api.Exists(x => x.Id == item.Id);

                if (isExist)
                    return ResponseError("RACM Procedure Details with ID: " + item.Id + " already exists.");

                item.Status = "NotStarted";
                item.ProcedureStartDate = item.ProcedureStartDate?.ToLocalTime();
                item.ProcedureEndDate = item.ProcedureEndDate?.ToLocalTime();

                base.Post(item);

                //Activity Log
                _CommonServices.ActivityLog(item.CreatedBy, item.Id, "", "RACMProcedure", "RACMProcedure | Add", "Added RACMProcedure");
            }
            return ResponseOK(e);
        }

        [HttpPut("Multiple")]
        public ActionResult UpdateMultiple([FromBody] List<RACMProcedure> e)
        {
            foreach (var item in e)
            {
                var isExist = _api.Exists(x => x.Id == item.Id);

                if (!isExist)
                    return ResponseError("RACM Procedure Details with ID: " + item.Id + " does not exist.");

                item.ProcedureStartDate = item.ProcedureStartDate?.ToLocalTime();
                item.ProcedureEndDate = item.ProcedureEndDate?.ToLocalTime();
                base.Put(item);
                //Activity Log
                _CommonServices.ActivityLog(item.UpdatedBy, item.Id, "", "RACMProcedure", "RACMProcedure | Edit", "Updated RACMProcedure");
            }
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, "", "RACMProcedure", "RACMProcedure | Delete", "Deleted RACMProcedure");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<RACM>();

            if (tList == null)
                return ResponseNotFound();

            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            foreach (var item in tList)
            {
                item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
                item.Reviewer = userRepo.GetByID(item.ReviewerId);
            }

            return ResponseOK(tList);
        }
    }
}