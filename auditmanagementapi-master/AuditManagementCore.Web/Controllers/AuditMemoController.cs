using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditMemoController : VJBaseGenericAPIController<AuditMemo>
    {
        IMongoDbSettings _dbsetting;
        public AuditMemoController(IMongoGenericRepository<AuditMemo> api, IMongoDbSettings dbsetting) : base(api)
        {
            _dbsetting = dbsetting;
        }

        public override ActionResult Post([FromBody] AuditMemo e)
        {
            if (e == null) return ResponseBad("Audit Memo object is null");
            var isExist = _api.Exists(x => x.AuditId.ToLower() == e.AuditId.ToLower());
            if (isExist)
            {
                return ResponseError("Audit Memo already exists for Audit");
            }
            var AuditMemo = base.Post(e);
            //Activity Log
            CommonServices obj = new CommonServices(_dbsetting);
            obj.ActivityLog(e.CreatedBy, e.Id, "", "AuditMemo", "AuditMemo | Add", "Added AuditMemo");
            return AuditMemo;
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Audit>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            foreach (var item in tList)
            {
                item.Activities = activtiyRepo.GetMany(x => x.AuditID == item.AuditId).ToHashSet();
            }
            return ResponseOK(tList);
        }
        public override ActionResult GetByID(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.Id == id).FirstOrDefault();

            if (tList == null)
            {
                return ResponseNotFound();
            }
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            tList.Activities = activtiyRepo.GetMany(x => x.AuditID == tList.AuditId).ToHashSet();
            return ResponseOK(tList);
        }

        [HttpGet("GetByAudit/{id}")]
        public ActionResult GetByAudit(string id)
        {
            var tList = _api.GetWithInclude<Audit>(x => x.AuditId == id);

            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }
}