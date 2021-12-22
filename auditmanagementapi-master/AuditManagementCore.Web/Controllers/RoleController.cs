using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : VJBaseGenericAPIController<Role>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public RoleController(IMongoGenericRepository<Role> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Role e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return ResponseError("Role Name is already Exists");
            }
            try
            {
                //check if
                var scopeRepo = new MongoGenericRepository<UserScopeTemplate>(_dbsetting);
                var commonScopes = scopeRepo.GetFirst(x => x.ScopeName == AuditConstants.UserScopeTemplate.COMMON);
                if (commonScopes != null)
                {
                    e.Scopes.AddRange(commonScopes.Scopes);
                }
                //Making sure scopes are unique;
                e.Scopes = e.Scopes.Distinct().ToList();
            }
            catch (Exception ex)
            {

            }
            var role = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "Role", "Role | Add", "Added Role");
            return role;
        }

        public override ActionResult Put([FromBody] Role tValue)
        {
            var isExist = _api.Exists(x => x.Id != tValue.Id && x.Name == tValue.Name);
            if (isExist)
            {
                return ResponseError("Role already Exists");
            }

            try
            {
                //check if
                var scopeRepo = new MongoGenericRepository<UserScopeTemplate>(_dbsetting);
                var commonScopes = scopeRepo.GetFirst(x => x.ScopeName == AuditConstants.UserScopeTemplate.COMMON);
                if (commonScopes != null)
                {
                    tValue.Scopes.AddRange(commonScopes.Scopes);
                }
                //Making sure scopes are unique;
                tValue.Scopes = tValue.Scopes.Distinct().ToList();
            }
            catch (Exception ex)
            {

            }
            var role= base.Put(tValue);
            //Activity Log
            _CommonServices.ActivityLog(tValue.UpdatedBy, tValue.Id, tValue.Name, "Role", "Role | Edit", "Updated Role");
            return role;
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("Role object is null");
                var objRole = _api.GetFirst(x => x.Id == id);
                if (objRole == null)
                    return ResponseError("Role does not exists");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objRole.Name, "Role", "Role | Delete", "Deleted Role");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
    }
}