using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class UserScopeTemplateController : VJBaseGenericAPIController<UserScopeTemplate>
    {

        public UserScopeTemplateController(IMongoGenericRepository<UserScopeTemplate> api) : base(api)
        {
        }

        public override ActionResult Post([FromBody] UserScopeTemplate e)
        {
            var isExist = _api.Exists(x => x.ScopeName.ToLower() == e.ScopeName.ToLower());
            if (isExist)
            {
                return ResponseError("ScopeName already exists for module");
            }

            return base.Post(e);
        }
        [AllowAnonymous]
        public override ActionResult GetAll()
        {
            return base.GetAll();
        }
    }
}