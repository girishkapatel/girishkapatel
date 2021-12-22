using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlController : VJBaseGenericAPIController<Control>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public ControlController(IMongoGenericRepository<Control> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api) 
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Control e)
        {
            var isExist = _api.Exists(x => x.Title.ToLower() == e.Title.ToLower() && x.ControlId == e.ControlId && x.Description.ToLower() == e.Description.ToLower());
            if (isExist)
            {
                return ResponseError("Control with name : " + e.Title + " already exists.");
            }
            var country = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, "", "Control", "Control | Add", "Added Control");
            return country;
        }
    }
}
