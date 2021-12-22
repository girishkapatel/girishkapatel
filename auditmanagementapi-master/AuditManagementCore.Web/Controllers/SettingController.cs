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
    public class SettingController : VJBaseGenericAPIController<Settings>
    {
        public SettingController(IMongoGenericRepository<Settings> api) : base(api)
        {

        }

        [Authorize("create:settings")]
        public override ActionResult Post([FromBody] Settings e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower() && x.Module.ToLower() == e.Module.ToLower() && x.Value.ToLower() == e.Value.ToLower());
            if (isExist)
            {
                return ResponseError("Setting with given name and value already exists for module");
            }
            return base.Post(e);
        }
        [Authorize("read:settings")]
        public override ActionResult GetAll()
        {
            return base.GetAll();
        }
    }
}