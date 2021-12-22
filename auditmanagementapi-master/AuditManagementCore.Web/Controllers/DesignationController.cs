using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationController : VJBaseGenericAPIController<Designation>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;

        public DesignationController(IMongoGenericRepository<Designation> api, IMongoDbSettings mongoDbSettings, CommonServices cs ) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] Designation e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return ResponseError("Designation Name is already Exists");
            }
            var country = base.Post(e);

            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "Designation", "Master | Designation | Add", "Added Designation");
            return country;
        }


        public override ActionResult Put([FromBody] Designation e)
        {
            if (e == null) return ResponseBad("Designation object is null");
            var country = _api.GetFirst(x => x.Id == e.Id);

            if (country == null)
            {
                return ResponseError("Designation does not exists");
            }
            country.Name = e.Name;
            country.UpdatedBy = e.UpdatedBy;
            _api.Update(country);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "Designation", "Master | Designation | Edit", "Updated Designation");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoUser = new MongoGenericRepository<User>(_dbsetting);

                if (id == null) return ResponseBad("Designation object is null");
                var objDesignation = _api.GetFirst(x => x.Id == id);

                if (objDesignation == null)
                {
                    return ResponseError("Designation does not exists");
                }
                var designation = _repoUser.GetFirst(x => x.Designation == objDesignation.Name);
                if (designation != null)
                    return CustomResponseError("");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objDesignation.Name, "Designation", "Master | Designation | Delete", "Deleted Designation");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
          
    }
}